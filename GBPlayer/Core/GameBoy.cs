using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GBPlayer.Core.Utils;

namespace GBPlayer.Core {

    public class GameBoy
    {
        private bool _isRunning;
        private bool _isPaused;
        private CancellationTokenSource _cts;
        
        private MMU _mmu;
        private CPU _cpu;
        private PPU _ppu;
        private TIMER _timer;
        private JOYPAD _joypad;
        
        public event Action<Int32[]> OnFrameReady;

        public bool IsPaused => _isPaused;

        // Speed Control
        public int TargetFps { get; set; } = 60;

        public GameBoy()
        {
            InitializeHardware();
        }

        private void InitializeHardware()
        {
            _mmu = new MMU();
            _cpu = new CPU(_mmu);
            _ppu = new PPU();
            _timer = new TIMER();
            _joypad = new JOYPAD();
            _ppu.OnFrameReady += Ppu_OnFrameReady;
        }

        private void Ppu_OnFrameReady()
        {
            OnFrameReady?.Invoke(_ppu.bmp.Bits);
        }

        public void LoadRom(string path)
        {
            try
            {
                // Stop existing loop if running
                if (_isRunning)
                {
                    _cts?.Cancel();
                    _isRunning = false;
                }

                InitializeHardware();
                _mmu.loadGamePak(path);
                Console.WriteLine($"Loaded ROM: {path}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading ROM: {ex.Message}");
            }
        }

        public void Start()
        {
            if (_isRunning) return;
            _isRunning = true;
            _cts = new CancellationTokenSource();
            Task.Run(() => EmulationLoop(_cts.Token));
        }

        public void Reset()
        {
            _isPaused = false;
            // Soft reset CPU by re-instantiating it with the existing MMU
            _cpu = new CPU(_mmu);
        }

        public void TogglePause()
        {
            _isPaused = !_isPaused;
        }

        public void KeyDown(GameBoyKey key)
        {
            _joypad.KeyDown(key);
        }

        public void KeyUp(GameBoyKey key)
        {
            _joypad.KeyUp(key);
        }

        private void EmulationLoop(CancellationToken token)
        {
            double targetFrameTimeMs = 1000.0 / (TargetFps > 0 ? TargetFps : 60);
            Stopwatch frameTimer = new Stopwatch();

            int cyclesThisUpdate = 0;

            while (!token.IsCancellationRequested)
            {
                if (_isPaused)
                {
                    Thread.Sleep(100);
                    continue;
                }

                frameTimer.Restart();

                // Update target FPS in case it changed at runtime
                targetFrameTimeMs = 1000.0 / (TargetFps > 0 ? TargetFps : 60);

                while (cyclesThisUpdate < Constants.CYCLES_PER_UPDATE)
                {
                    int cpuCycles = _cpu.Exe();
                    cyclesThisUpdate += cpuCycles;

                    _timer.update(cpuCycles, _mmu);
                    _ppu.update(cpuCycles, _mmu);
                    _joypad.update(_mmu);
                    HandleInterrupts();
                }

                cyclesThisUpdate -= Constants.CYCLES_PER_UPDATE;

                frameTimer.Stop();
                
				double elapsedMs = frameTimer.Elapsed.TotalMilliseconds;
				
                if (elapsedMs < targetFrameTimeMs)
                {
                    int sleepTime = (int)(targetFrameTimeMs - elapsedMs);
                    if (sleepTime > 0)
                    {
                        Thread.Sleep(sleepTime);
                    }
                }
            }
            _isRunning = false;
        }

        private void HandleInterrupts()
        {
            byte interruptEnable = _mmu.IE;
            byte interruptFlags = _mmu.IF;

            for (int i = 0; i < 5; i++)
            {
                if ((((interruptEnable & interruptFlags) >> i) & 0x1) == 1)
                {
                    _cpu.ExecuteInterrupt(i);
                }
            }

            _cpu.UpdateIME();
        }
    }
}
