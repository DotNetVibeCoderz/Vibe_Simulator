using System.Runtime.CompilerServices;
using static GBPlayer.Core.Utils.BitOps;

namespace GBPlayer.Core {
    public class TIMER {

        private int divCounter;
        private int timerCounter;
        private const int TIMER_INTERRUPT = 2;

        public void update(int cycles, MMU mmu) {
            divCounter += cycles;
            if (divCounter >= 256) {
                divCounter -= 256;
                mmu.DIV++;
            }

            if (mmu.TAC_ENABLED) {
                timerCounter += cycles;
                int freq = GetFrequency(mmu.TAC_FREQ);
                if (timerCounter >= freq) {
                    timerCounter -= freq;
                    if (mmu.TIMA == 0xFF) {
                        mmu.TIMA = mmu.TMA;
                        mmu.requestInterrupt(TIMER_INTERRUPT);
                    } else {
                        mmu.TIMA++;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetFrequency(byte freq) {
            switch (freq) {
                case 0: return 1024;
                case 1: return 16;
                case 2: return 64;
                case 3: return 256;
                default: return 1024;
            }
        }
    }
}