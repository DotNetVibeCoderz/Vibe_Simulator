using System;

namespace GBPlayer.Core.GamePak {
    class MBC1 : IGamePak {
        private byte[] ROM;
        private byte[] RAM;
        private byte ROMBank = 1;
        private byte RAMBank = 0;
        private bool RAMEnabled = false;
        private int BankingMode = 0; // 0=ROM, 1=RAM

        public void Init(byte[] rom) {
            ROM = rom;
            RAM = new byte[0x8000]; // 32KB Max
        }

        public byte ReadLoROM(ushort addr) {
            return ROM[addr];
        }

        public byte ReadHiROM(ushort addr) {
            int bank = ROMBank == 0 ? 1 : ROMBank;
            int offset = (bank * 0x4000) + (addr - 0x4000);
            return (offset < ROM.Length) ? ROM[offset] : (byte)0xFF;
        }

        public void WriteROM(ushort addr, byte val) {
            if (addr < 0x2000) {
                RAMEnabled = (val & 0xF) == 0xA;
            } else if (addr < 0x4000) {
                ROMBank = (byte)(val & 0x1F);
                if (ROMBank == 0) ROMBank = 1;
            } else if (addr < 0x6000) {
                if (BankingMode == 0) {
                    ROMBank = (byte)((ROMBank & 0x1F) | ((val & 3) << 5));
                } else {
                    RAMBank = (byte)(val & 3);
                }
            } else if (addr < 0x8000) {
                BankingMode = val & 1;
            }
        }

        public byte ReadERAM(ushort addr) {
            if (!RAMEnabled) return 0xFF;
            int offset = (RAMBank * 0x2000) + (addr - 0xA000);
            return (offset < RAM.Length) ? RAM[offset] : (byte)0xFF;
        }

        public void WriteERAM(ushort addr, byte val) {
            if (!RAMEnabled) return;
            int offset = (RAMBank * 0x2000) + (addr - 0xA000);
            if (offset < RAM.Length) RAM[offset] = val;
        }
    }
}