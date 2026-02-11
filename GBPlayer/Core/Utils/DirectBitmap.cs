using System;
using System.Runtime.CompilerServices;

namespace GBPlayer.Core.Utils {
    public class DirectBitmap {
        public Int32[] Bits { get; private set; }
        public static int Height = 144;
        public static int Width = 160;

        public DirectBitmap() {
            Bits = new Int32[Width * Height];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPixel(int x, int y, int colour) {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return;
            int index = x + (y * Width);
            Bits[index] = colour;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetPixel(int x, int y) {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return 0;
            int index = x + (y * Width);
            return Bits[index];
        }
    }
}