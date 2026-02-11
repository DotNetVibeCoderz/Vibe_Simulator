namespace GBPlayer.Core.GamePak {
    interface IGamePak {
        void Init(byte[] rom);
        byte ReadLoROM(ushort addr);
        byte ReadHiROM(ushort addr);
        void WriteROM(ushort addr, byte val);
        byte ReadERAM(ushort addr);
        void WriteERAM(ushort addr, byte val);
    }
}