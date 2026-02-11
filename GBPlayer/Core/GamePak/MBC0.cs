using System;

namespace GBPlayer.Core.GamePak {
    class MBC0 : IGamePak {
        private byte[] ROM;

        public void Init(byte[] rom) {
            ROM = rom;
        }

        public byte ReadLoROM(ushort addr) {
            return ROM[addr];
        }

        public byte ReadHiROM(ushort addr) {
            return ROM[addr];
        }

        public void WriteROM(ushort addr, byte val) {
            // Read Only
        }

        public byte ReadERAM(ushort addr) {
            return 0xFF; // No RAM
        }

        public void WriteERAM(ushort addr, byte val) {
            // No RAM
        }
    }
}