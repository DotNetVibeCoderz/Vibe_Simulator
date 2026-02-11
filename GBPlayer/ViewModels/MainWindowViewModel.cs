using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ReactiveUI;
using System.Reactive;
using System.IO;
using System.Threading.Tasks;
using GBPlayer.Core;
using Avalonia;
using Avalonia.Threading;
using System.Runtime.InteropServices;
using GBPlayer.Models;
using System.Collections.Generic;

namespace GBPlayer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private GameBoy _gameBoy;
        private WriteableBitmap _screenBuffer;
        private string _statusText = "Ready";
        private bool _isEmulationRunning;
        private AppSettings _appSettings;

        public MainWindowViewModel()
        {
            _appSettings = new AppSettings();

            _gameBoy = new GameBoy
            {
                TargetFps = _appSettings.TargetFps
            };

            _gameBoy.OnFrameReady += OnFrameReady;
            
            // Initialize screen buffer (160x144 for GameBoy)
            // PixelFormat.Bgra8888 corresponds to the int[] layout 0xAARRGGBB on Little Endian systems
            _screenBuffer = new WriteableBitmap(new PixelSize(160, 144), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);

            LoadRomCommand = ReactiveCommand.CreateFromTask(LoadRomAsync);
            ResetCommand = ReactiveCommand.Create(ResetEmulation);
            TogglePauseCommand = ReactiveCommand.Create(TogglePause);
            OpenSettingsCommand = ReactiveCommand.Create(OpenSettings);
            ShowAboutCommand = ReactiveCommand.Create(ShowAbout);
            ExitCommand = ReactiveCommand.Create(() => Environment.Exit(0));
        }

        public WriteableBitmap ScreenBuffer
        {
            get => _screenBuffer;
            set => this.RaiseAndSetIfChanged(ref _screenBuffer, value);
        }

        public string StatusText
        {
            get => _statusText;
            set => this.RaiseAndSetIfChanged(ref _statusText, value);
        }

        public ReactiveCommand<Unit, Unit> LoadRomCommand { get; }
        public ReactiveCommand<Unit, Unit> ResetCommand { get; }
        public ReactiveCommand<Unit, Unit> TogglePauseCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenSettingsCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowAboutCommand { get; }
        public ReactiveCommand<Unit, Unit> ExitCommand { get; }

        public void HandleKeyDown(GameBoyKey key)
        {
            if (_isEmulationRunning)
            {
                _gameBoy.KeyDown(key);
            }
        }

        public void HandleKeyUp(GameBoyKey key)
        {
             if (_isEmulationRunning)
            {
                _gameBoy.KeyUp(key);
            }
        }

        private async Task LoadRomAsync()
        {
            if (Application.Current.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                 var dialog = new OpenFileDialog();
                 dialog.Filters.Add(new FileDialogFilter { Name = "GameBoy ROMs", Extensions = { "gb", "gbc", "bin", "gba" } });
                 dialog.AllowMultiple = false;

                 var result = await dialog.ShowAsync(desktop.MainWindow);
                 if (result != null && result.Length > 0)
                 {
                     string romPath = result[0];
                     StatusText = $"Loading {Path.GetFileName(romPath)}...";
                     // Run loading on background thread to not freeze UI if it takes time (usually fast though)
                     await Task.Run(() => {
                        _gameBoy.LoadRom(romPath);
                        _gameBoy.Start();
                     });
                     
                     _isEmulationRunning = true;
                     StatusText = "Running";
                 }
            }
        }

        private void ResetEmulation()
        {
            if (_isEmulationRunning)
            {
                _gameBoy.Reset();
                StatusText = "Reset";
            }
        }

        private void TogglePause()
        {
            if (_isEmulationRunning)
            {
                _gameBoy.TogglePause();
                StatusText = _gameBoy.IsPaused ? "Paused" : "Running";
            }
        }

        private void OpenSettings()
        {
            if (Application.Current.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                var settingsVm = new SettingsViewModel(_appSettings);
                var settingsWin = new Views.SettingsWindow
                {
                    DataContext = settingsVm
                };

                settingsVm.OnSaveRequested += settings =>
                {
                    _appSettings = new AppSettings
                    {
                        TargetFps = settings.TargetFps,
                        KeyMapping = new Dictionary<GameBoyKey, Avalonia.Input.Key>(settings.KeyMapping)
                    };

                    _gameBoy.TargetFps = _appSettings.TargetFps;
                    settingsWin.Close();
                };

                settingsVm.OnCancelRequested += () => settingsWin.Close();

                settingsWin.ShowDialog(desktop.MainWindow);
            }
        }

        private void ShowAbout()
        {
             if (Application.Current.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                var aboutWin = new Views.AboutWindow();
                aboutWin.ShowDialog(desktop.MainWindow);
            }
        }
        
        private void OnFrameReady(int[] frameBuffer)
        {
			
			Dispatcher.UIThread.InvokeAsync(() => {
                var wb = new WriteableBitmap(new PixelSize(160, 144), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque); 
				using (var fb = wb.Lock()) { 
					// write pixel data here 
					Marshal.Copy(frameBuffer, 0, fb.Address, frameBuffer.Length);
				}
				ScreenBuffer = wb;
				/*
				using (var fb = _screenBuffer.Lock())
                 {
                     // Copy int[] directly to IntPtr
                     Marshal.Copy(frameBuffer, 0, fb.Address, frameBuffer.Length);
                 }
                 this.RaisePropertyChanged(nameof(ScreenBuffer));
				*/
             });
        }

        public GameBoyKey? MapKey(Avalonia.Input.Key key)
        {
            if (_appSettings?.KeyMapping == null)
            {
                return null;
            }

            foreach (var mapping in _appSettings.KeyMapping)
            {
                if (mapping.Value == key)
                {
                    return mapping.Key;
                }
            }

            return null;
        }
    }
}
