using static GBPlayer.Core.Utils.BitOps;

namespace GBPlayer.Core {
    public class JOYPAD {

        private const int JOYPAD_INTERRUPT = 4;
        private const byte PAD_MASK = 0x10;
        private const byte BUTTON_MASK = 0x20;
        private byte pad = 0xF;
        private byte buttons = 0xF;

        public void KeyDown(GameBoyKey key) {
            byte b = GetKeyBit(key);
            if ((b & PAD_MASK) == PAD_MASK) {
                pad = (byte)(pad & ~(b & 0xF));
            } else if((b & BUTTON_MASK) == BUTTON_MASK) {
                buttons = (byte)(buttons & ~(b & 0xF));
            }
        }

        public void KeyUp(GameBoyKey key) {
            byte b = GetKeyBit(key);
            if ((b & PAD_MASK) == PAD_MASK) {
                pad = (byte)(pad | (b & 0xF));
            } else if ((b & BUTTON_MASK) == BUTTON_MASK) {
                buttons = (byte)(buttons | (b & 0xF));
            }
        }

        public void update(MMU mmu) {
            byte JOYP = mmu.JOYP;
            if(!isBit(4, JOYP)) {
                mmu.JOYP = (byte)((JOYP & 0xF0) | pad);
                if(pad != 0xF) mmu.requestInterrupt(JOYPAD_INTERRUPT);
            }
            if (!isBit(5, JOYP)) {
                mmu.JOYP = (byte)((JOYP & 0xF0) | buttons);
                if (buttons != 0xF) mmu.requestInterrupt(JOYPAD_INTERRUPT);
            }
            if ((JOYP & 0b00110000) == 0b00110000) mmu.JOYP = 0xFF;
        }

        private byte GetKeyBit(GameBoyKey key) {
            switch (key) {
                case GameBoyKey.Right: return 0x11;
                case GameBoyKey.Left:  return 0x12;
                case GameBoyKey.Up:    return 0x14;
                case GameBoyKey.Down:  return 0x18;
                case GameBoyKey.A:     return 0x21;
                case GameBoyKey.B:     return 0x22;
                case GameBoyKey.Select: return 0x24;
                case GameBoyKey.Start:  return 0x28;
            }
            return 0;
        }
    }
}