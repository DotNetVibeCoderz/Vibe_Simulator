using System.Runtime.CompilerServices;
using GBPlayer.Core.Utils;
using static GBPlayer.Core.Utils.BitOps;

namespace GBPlayer.Core {
    public class PPU {

        private const int SCREEN_WIDTH = 160;
        private const int SCREEN_HEIGHT = 144;
        private const int SCREEN_VBLANK_HEIGHT = 153;
        private const int OAM_CYCLES = 80;
        private const int VRAM_CYCLES = 172;
        private const int HBLANK_CYCLES = 204;
        private const int SCANLINE_CYCLES = 456;

        private const int VBLANK_INTERRUPT = 0;
        private const int LCD_INTERRUPT = 1;

        // ARGB colors: White, Light Gray, Dark Gray, Black
        private int[] color = new int[] { unchecked((int)0xFFFFFFFF), unchecked((int)0xFFAAAAAA), unchecked((int)0xFF555555), unchecked((int)0xFF000000) };

        public DirectBitmap bmp;
        private int scanlineCounter;
        
        public event System.Action OnFrameReady;

        public PPU() {
            bmp = new DirectBitmap();
        }

        public void update(int cycles, MMU mmu) {
            scanlineCounter += cycles;
            byte currentMode = (byte)(mmu.STAT & 0x3);

            if (isLCDEnabled(mmu.LCDC)) {
                switch (currentMode) {
                    case 2: // OAM
                        if (scanlineCounter >= OAM_CYCLES) {
                            changeSTATMode(3, mmu);
                            scanlineCounter -= OAM_CYCLES;
                        }
                        break;
                    case 3: // VRAM
                        if (scanlineCounter >= VRAM_CYCLES) {
                            changeSTATMode(0, mmu);
                            drawScanLine(mmu);
                            scanlineCounter -= VRAM_CYCLES;
                        }
                        break;
                    case 0: // HBLANK
                        if (scanlineCounter >= HBLANK_CYCLES) {
                            mmu.LY++;
                            scanlineCounter -= HBLANK_CYCLES;

                            if (mmu.LY == SCREEN_HEIGHT) {
                                changeSTATMode(1, mmu);
                                mmu.requestInterrupt(VBLANK_INTERRUPT);
                                OnFrameReady?.Invoke();
                            } else {
                                changeSTATMode(2, mmu);
                            }
                        }
                        break;
                    case 1: // VBLANK
                        if (scanlineCounter >= SCANLINE_CYCLES) {
                            mmu.LY++;
                            scanlineCounter -= SCANLINE_CYCLES;

                            if (mmu.LY > SCREEN_VBLANK_HEIGHT) {
                                changeSTATMode(2, mmu);
                                mmu.LY = 0;
                            }
                        }
                        break;
                }

                if (mmu.LY == mmu.LYC) {
                    mmu.STAT = bitSet(2, mmu.STAT);
                    if (isBit(6, mmu.STAT)) {
                        mmu.requestInterrupt(LCD_INTERRUPT);
                    }
                } else {
                    mmu.STAT = bitClear(2, mmu.STAT);
                }

            } else {
                scanlineCounter = 0;
                mmu.LY = 0;
                mmu.STAT = (byte)(mmu.STAT & ~0x3);
            }
        }

        private void changeSTATMode(int mode, MMU mmu) {
            byte STAT = (byte)(mmu.STAT & ~0x3);
            mmu.STAT = (byte)(STAT | mode);

            if (mode == 2 && isBit(5, STAT)) mmu.requestInterrupt(LCD_INTERRUPT);
            else if (mode == 0 && isBit(3, STAT)) mmu.requestInterrupt(LCD_INTERRUPT);
            else if (mode == 1 && isBit(4, STAT)) mmu.requestInterrupt(LCD_INTERRUPT);
        }

        private void drawScanLine(MMU mmu) {
            byte LCDC = mmu.LCDC;
            if (isBit(0, LCDC)) renderBG(mmu);
            if (isBit(1, LCDC)) renderSprites(mmu);
        }

        private void renderBG(MMU mmu) {
            byte WX = (byte)(mmu.WX - 7);
            byte WY = mmu.WY;
            byte LY = mmu.LY;
            byte LCDC = mmu.LCDC;
            byte SCY = mmu.SCY;
            byte SCX = mmu.SCX;
            byte BGP = mmu.BGP;
            bool isWin = isWindow(LCDC, WY, LY);

            byte y = isWin ? (byte)(LY - WY) : (byte)(LY + SCY);
            byte tileLine = (byte)((y & 7) * 2);

            ushort tileRow = (ushort)(y / 8 * 32);
            ushort tileMap = isWin ? getWindowTileMapAdress(LCDC) : getBGTileMapAdress(LCDC);

            byte hi = 0;
            byte lo = 0;

            for (int p = 0; p < SCREEN_WIDTH; p++) {
                byte x = isWin && p >= WX ? (byte)(p - WX) : (byte)(p + SCX);
                if ((p & 0x7) == 0 || ((p + SCX) & 0x7) == 0) {
                    ushort tileCol = (ushort)(x / 8);
                    ushort tileAdress = (ushort)(tileMap + tileRow + tileCol);

                    ushort tileLoc;
                    if (isSignedAdress(LCDC)) {
                        tileLoc = (ushort)(getTileDataAdress(LCDC) + mmu.readVRAM(tileAdress) * 16);
                    } else {
                        tileLoc = (ushort)(getTileDataAdress(LCDC) + ((sbyte)mmu.readVRAM(tileAdress) + 128) * 16);
                    }

                    lo = mmu.readVRAM((ushort)(tileLoc + tileLine));
                    hi = mmu.readVRAM((ushort)(tileLoc + tileLine + 1));
                }

                int colorBit = 7 - (x & 7);
                int colorId = GetColorIdBits(colorBit, lo, hi);
                int colorIdThroughtPalette = GetColorIdThroughtPalette(BGP, colorId);

                bmp.SetPixel(p, LY, color[colorIdThroughtPalette]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetColorIdBits(int colorBit, byte l, byte h) {
            int hi = (h >> colorBit) & 0x1;
            int lo = (l >> colorBit) & 0x1;
            return (hi << 1 | lo);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetColorIdThroughtPalette(int palette, int colorId) {
            return (palette >> colorId * 2) & 0x3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool isSignedAdress(byte LCDC) { return isBit(4, LCDC); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ushort getBGTileMapAdress(byte LCDC) { return isBit(3, LCDC) ? (ushort)0x9C00 : (ushort)0x9800; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ushort getWindowTileMapAdress(byte LCDC) { return isBit(6, LCDC) ? (ushort)0x9C00 : (ushort)0x9800; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ushort getTileDataAdress(byte LCDC) { return isBit(4, LCDC) ? (ushort)0x8000 : (ushort)0x8800; }

        private void renderSprites(MMU mmu) {
            byte LY = mmu.LY;
            byte LCDC = mmu.LCDC;
            for (int i = 0x9C; i >= 0; i -= 4) {
                int y = mmu.readOAM(i) - 16;
                int x = mmu.readOAM(i + 1) - 8;
                byte tile = mmu.readOAM(i + 2);
                byte attr = mmu.readOAM(i + 3);

                if ((LY >= y) && (LY < (y + spriteSize(LCDC)))) {
                    byte palette = isBit(4, attr) ? mmu.OBP1 : mmu.OBP0;

                    int tileRow = isYFlipped(attr) ? spriteSize(LCDC) - 1 - (LY - y) : (LY - y);

                    ushort tileddress = (ushort)(0x8000 + (tile * 16) + (tileRow * 2));
                    byte lo = mmu.readVRAM(tileddress);
                    byte hi = mmu.readVRAM((ushort)(tileddress + 1));

                    for (int p = 0; p < 8; p++) {
                        int IdPos = isXFlipped(attr) ? p : 7 - p;
                        int colorId = GetColorIdBits(IdPos, lo, hi);
                        int colorIdThroughtPalette = GetColorIdThroughtPalette(palette, colorId);

                        if ((x + p) >= 0 && (x + p) < SCREEN_WIDTH) {
                            if (!isTransparent(colorId) && (isAboveBG(attr) || isBGWhite(mmu.BGP, x + p, LY))) {
                                bmp.SetPixel(x + p, LY, color[colorIdThroughtPalette]);
                            }
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool isBGWhite(byte BGP, int x, int y) {
            int id = BGP & 0x3;
            return bmp.GetPixel(x, y) == color[id];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool isAboveBG(byte attr) { return attr >> 7 == 0; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool isLCDEnabled(byte LCDC) { return isBit(7, LCDC); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int spriteSize(byte LCDC) { return isBit(2, LCDC) ? 16 : 8; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool isXFlipped(int attr) { return isBit(5, attr); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool isYFlipped(byte attr) { return isBit(6, attr); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool isTransparent(int b) { return b == 0; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool isWindow(byte LCDC, byte WY, byte LY) { return isBit(5, LCDC) && WY <= LY; }
    }
}