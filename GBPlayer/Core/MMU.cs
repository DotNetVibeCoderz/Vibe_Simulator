using System;
using System.IO;
using System.Runtime.CompilerServices;
using GBPlayer.Core.GamePak;
using GBPlayer.Core.Utils;
using static GBPlayer.Core.Utils.BitOps;

namespace GBPlayer.Core {
    public class MMU {

        //GamePak
        private IGamePak gamePak;

        //DMG Memory Map
        private byte[] VRAM = new byte[0x2000];
        private byte[] WRAM0 = new byte[0x1000];
        private byte[] WRAM1 = new byte[0x1000];
        private byte[] OAM = new byte[0xA0];
        private byte[] IO = new byte[0x80];
        private byte[] HRAM = new byte[0x80];

        //Timer IO Regs
        public byte DIV { get { return IO[0x04]; } set { IO[0x04] = value; } } //FF04 - DIV - Divider Register (R/W)
        public byte TIMA { get { return IO[0x05]; } set { IO[0x05] = value; } } //FF05 - TIMA - Timer counter (R/W)
        public byte TMA { get { return IO[0x06]; } set { IO[0x06] = value; } } //FF06 - TMA - Timer Modulo (R/W)
        public byte TAC { get { return IO[0x07]; } set { IO[0x07] = value; } } //FF07 - TAC - Timer Control (R/W)
        public bool TAC_ENABLED { get { return (IO[0x07] & 0x4) != 0; } } // Check if byte 2 is 1
        public byte TAC_FREQ { get { return (byte)(IO[0x07] & 0x3); } } // returns byte 0 and 1

        //Interrupt IO Flags
        public byte IE { get { return HRAM[0x7F]; } set { HRAM[0x7F] = value; } }//FFFF - IE - Interrupt Enable (R/W)
        public byte IF { get { return IO[0x0F]; } set { IO[0x0F] = value; } }//FF0F - IF - Interrupt Flag (R/W)

        //PPU IO Regs
        public byte LCDC { get { return IO[0x40]; } }//FF40 - LCDC - LCD Control (R/W)
        public byte STAT { get { return IO[0x41]; } set { IO[0x41] = value; } }//FF41 - STAT - LCDC Status (R/W)

        public byte SCY { get { return IO[0x42]; } }//FF42 - SCY - Scroll Y (R/W)
        public byte SCX { get { return IO[0x43]; } }//FF43 - SCX - Scroll X (R/W)
        public byte LY { get { return IO[0x44]; } set { IO[0x44] = value; } }//FF44 - LY - LCDC Y-Coordinate (R) bypasses on write always 0
        public byte LYC { get { return IO[0x45]; } }//FF45 - LYC - LY Compare(R/W)
        public byte WY { get { return IO[0x4A]; } }//FF4A - WY - Window Y Position (R/W)
        public byte WX { get { return IO[0x4B]; } }//FF4B - WX - Window X Position minus 7 (R/W)

        public byte BGP { get { return IO[0x47]; } }//FF47 - BGP - BG Palette Data(R/W) - Non CGB Mode Only
        public byte OBP0 { get { return IO[0x48]; } }//FF48 - OBP0 - Object Palette 0 Data (R/W) - Non CGB Mode Only
        public byte OBP1 { get { return IO[0x49]; } }//FF49 - OBP1 - Object Palette 1 Data (R/W) - Non CGB Mode Only

        public byte JOYP { get { return IO[0x00]; } set { IO[0x00] = value; } }//FF00 - JOYP

        public MMU() {
            IO[0x4D] = 0xFF;
            IO[0x10] = 0x80;
            IO[0x11] = 0xBF;
            IO[0x12] = 0xF3;
            IO[0x14] = 0xBF;
            IO[0x16] = 0x3F;
            IO[0x19] = 0xBF;
            IO[0x1A] = 0x7F;
            IO[0x1B] = 0xFF;
            IO[0x1C] = 0x9F;
            IO[0x1E] = 0xBF;
            IO[0x20] = 0xFF;
            IO[0x23] = 0xBF;
            IO[0x24] = 0x77;
            IO[0x25] = 0xF3;
            IO[0x26] = 0xF1;
            IO[0x40] = 0x91;
            IO[0x47] = 0xFC;
            IO[0x48] = 0xFF;
            IO[0x49] = 0xFF;
        }

        public byte readByte(ushort addr) {
            if (addr <= 0x3FFF) return gamePak.ReadLoROM(addr);
            if (addr <= 0x7FFF) return gamePak.ReadHiROM(addr);
            if (addr <= 0x9FFF) return VRAM[addr & 0x1FFF];
            if (addr <= 0xBFFF) return gamePak.ReadERAM(addr);
            if (addr <= 0xCFFF) return WRAM0[addr & 0xFFF];
            if (addr <= 0xDFFF) return WRAM1[addr & 0xFFF];
            if (addr <= 0xEFFF) return WRAM0[addr & 0xFFF];
            if (addr <= 0xFDFF) return WRAM1[addr & 0xFFF];
            if (addr <= 0xFE9F) return OAM[addr - 0xFE00];
            if (addr <= 0xFEFF) return 0x00;
            if (addr <= 0xFF7F) return IO[addr & 0x7F];
            if (addr <= 0xFFFF) return HRAM[addr & 0x7F];
            return 0xFF;
        }

        public void writeByte(ushort addr, byte b) {
            if (addr <= 0x7FFF) gamePak.WriteROM(addr, b);
            else if (addr <= 0x9FFF) VRAM[addr & 0x1FFF] = b;
            else if (addr <= 0xBFFF) gamePak.WriteERAM(addr, b);
            else if (addr <= 0xCFFF) WRAM0[addr & 0xFFF] = b;
            else if (addr <= 0xDFFF) WRAM1[addr & 0xFFF] = b;
            else if (addr <= 0xEFFF) WRAM0[addr & 0xFFF] = b;
            else if (addr <= 0xFDFF) WRAM1[addr & 0xFFF] = b;
            else if (addr <= 0xFE9F) OAM[addr & 0x9F] = b;
            else if (addr <= 0xFEFF) { /* Not Usable */ }
            else if (addr <= 0xFF7F) {
                switch (addr) {
                    case 0xFF0F: b |= 0xE0; break;
                    case 0xFF04:
                    case 0xFF44: b = 0; break;
                    case 0xFF46: DMA(b); break;
                }
                IO[addr & 0x7F] = b;
            }
            else if (addr <= 0xFFFF) HRAM[addr & 0x7F] = b;
        }

        public ushort readWord(ushort addr) {
            return (ushort)(readByte((ushort)(addr + 1)) << 8 | readByte(addr));
        }

        public void writeWord(ushort addr, ushort w) {
            writeByte((ushort)(addr + 1), (byte)(w >> 8));
            writeByte(addr, (byte)w);
        }

        public byte readOAM(int addr) {
            return OAM[addr];
        }

        public byte readVRAM(int addr) {
            return VRAM[addr & 0x1FFF];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void requestInterrupt(byte b) {
            IF = bitSet(b, IF);
        }

        private void DMA(byte b) {
            ushort addr = (ushort)(b << 8);
            for (byte i = 0; i < OAM.Length; i++) {
                OAM[i] = readByte((ushort)(addr + i));
            }
        }

        public void loadGamePak(String cartName) {
            byte[] rom = File.ReadAllBytes(cartName);
            byte mbcType = rom[0x147];
            
            // Simplified MBC selection
            if (mbcType == 0x00) gamePak = new MBC0();
            else if (mbcType <= 0x03) gamePak = new MBC1();
            else if (mbcType <= 0x06) gamePak = new MBC2();
            else if (mbcType <= 0x13) gamePak = new MBC3();
            else if (mbcType <= 0x1E) gamePak = new MBC5(); // Broad catch for MBC5
            else gamePak = new MBC0(); // Default fallback
            
            gamePak.Init(rom);
        }

        // Overload to support loading from existing byte array if needed, but path is fine
        public void InitGamePak(byte[] rom) {
             byte mbcType = rom[0x147];
             if (mbcType == 0x00) gamePak = new MBC0();
            else if (mbcType <= 0x03) gamePak = new MBC1();
            else if (mbcType <= 0x06) gamePak = new MBC2();
            else if (mbcType <= 0x13) gamePak = new MBC3();
            else if (mbcType <= 0x1E) gamePak = new MBC5();
            else gamePak = new MBC0();
            gamePak.Init(rom);
        }
    }
}