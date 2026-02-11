using System;

namespace GBPlayer.Core.GamePak {
    class MBC5 : IGamePak {
        private byte[] ROM;
        private byte[] RAM;
        private int ROMBank = 1;
        private byte RAMBank = 0;
        private bool RAMEnabled = false;

        public void Init(byte[] rom) {
            ROM = rom;
            RAM = new byte[0x20000]; // 128KB Max
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
                RAMEnabled = (val & 0xF) == 0xA;
            } else if (addr < 0x3000) {
                ROMBank = (ROMBank & 0x100) | val;
            } else if (addr < 0x4000) {
                ROMBank = (ROMBank & 0xFF) | ((val & 1) << 8);
            } else if (addr < 0x6000) {
                RAMBank = (byte)(val & 0xF);
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