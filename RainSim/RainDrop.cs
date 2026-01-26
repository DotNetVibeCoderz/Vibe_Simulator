using System;

namespace RainSim
{
    // Kelas untuk merepresentasikan satu tetes hujan
    public class RainDrop
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; } // Z-depth untuk efek paralaks (kedalaman)
        public float Length { get; set; }
        public float Speed { get; set; }
        
        private static Random rand = new Random();
        private int screenWidth;
        private int screenHeight;

        public RainDrop(int width, int height)
        {
            screenWidth = width;
            screenHeight = height;
            Reset();
            // Acak posisi awal Y agar tidak muncul serentak di atas baris
            Y = rand.Next(-screenHeight, height);
        }

        // Reset properti tetesan hujan
        public void Reset()
        {
            X = rand.Next(screenWidth);
            Y = rand.Next(-100, -10); // Mulai sedikit di atas layar
            Z = (float)(rand.NextDouble() * 20 + 0.5); // Kedalaman antara 0.5 sampai 20
            Length = map(Z, 0, 20, 10, 20); // Panjang berdasarkan kedalaman
            Speed = map(Z, 0, 20, 4, 15);   // Kecepatan berdasarkan kedalaman
        }

        // Update posisi
        public void Update(float gravity, float windSpeed)
        {
            Y += Speed + gravity;
            X += windSpeed; // Efek angin

            // Jaga agar tetap di dalam layar secara horizontal (wrapping)
            if (X > screenWidth) X = 0;
            if (X < 0) X = screenWidth;

            // Jika jatuh ke bawah layar, reset ke atas
            if (Y > screenHeight)
            {
                Reset();
            }
        }

        // Fungsi helper untuk mapping nilai range
        private float map(float val, float min1, float max1, float min2, float max2)
        {
            return (val - min1) / (max1 - min1) * (max2 - min2) + min2;
        }
    }
}
