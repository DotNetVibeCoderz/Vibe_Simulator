using System;

namespace GBPlayer.Core.GamePak {
    class MBC2 : IGamePak {
        private byte[] ROM;
        private byte[] RAM; // 512x4 bits
        private byte ROMBank = 1;
        private bool RAMEnabled = false;

        public void Init(byte[] rom) {
            ROM = rom;
            RAM = new byte[512]; 
        }

        public byte ReadLoROM(ushort addr) {
            return ROM[addr];
        }

        public byte ReadHiROM(ushort addr) {
             int offset = (ROMBank * 0x4000) + (addr - 0x4000);
            return (offset < ROM.Length) ? ROM[offset] : (byte)0xFF;
        }

        public void WriteROM(ushort addr, byte val) {
            if (addr < 0x2000) {
                if ((addr & 0x100) == 0)
                    RAMEnabled = (val & 0xF) == 0xA;
            } else if (addr < 0x4000) {
                if ((addr & 0x100) != 0) {
                    ROMBank = (byte)(val & 0xF);
                    if (ROMBank == 0) ROMBank = 1;
                }
            }
        }

        public byte ReadERAM(ushort addr) {
             if (!RAMEnabled) return 0xFF;
             return (byte)(RAM[addr & 0x1FF] | 0xF0);
        }

        public void WriteERAM(ushort addr, byte val) {
             if (!RAMEnabled) return;
             RAM[addr & 0x1FF] = (byte)(val & 0xF);
        }
    }
}