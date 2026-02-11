using System;

namespace GBPlayer.Core.GamePak {
    class MBC3 : IGamePak {
        private byte[] ROM;
        private byte[] RAM;
        private byte[] RTC = new byte[5];
        private byte ROMBank = 1;
        private byte RAMBank = 0;
        private bool RAMEnabled = false;
        private bool RTCEnabled = false;

        public void Init(byte[] rom) {
            ROM = rom;
            RAM = new byte[0x8000]; // 32KB
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
                RTCEnabled = (val & 0xF) == 0xA;
            } else if (addr < 0x4000) {
                ROMBank = (byte)(val & 0x7F);
                if (ROMBank == 0) ROMBank = 1;
            } else if (addr < 0x6000) {
                if (val <= 0x3) RAMBank = val;
                else if (val >= 0x8 && val <= 0xC) RAMBank = val;
            } else if (addr < 0x8000) {
                // Latch Clock Data (Not implemented fully)
            }
        }

        public byte ReadERAM(ushort addr) {
            if (!RAMEnabled) return 0xFF;
            if (RAMBank <= 0x3) {
                int offset = (RAMBank * 0x2000) + (addr - 0xA000);
                return (offset < RAM.Length) ? RAM[offset] : (byte)0xFF;
            } else if (RAMBank >= 0x8 && RAMBank <= 0xC) {
                return RTC[RAMBank - 0x8];
            }
            return 0xFF;
        }

        public void WriteERAM(ushort addr, byte val) {
            if (!RAMEnabled) return;
            if (RAMBank <= 0x3) {
                int offset = (RAMBank * 0x2000) + (addr - 0xA000);
                if (offset < RAM.Length) RAM[offset] = val;
            } else if (RAMBank >= 0x8 && RAMBank <= 0xC) {
                 RTC[RAMBank - 0x8] = val;
            }
        }
    }
}