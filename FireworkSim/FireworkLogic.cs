using System;
using System.Collections.Generic;
using System.Drawing;

namespace FireworkSim
{
    // Helper to convert HSL to Color
    public static class ColorHelper
    {
        public static Color FromHsl(double h, double s, double l)
        {
            double r, g, b;

            if (s == 0)
            {
                r = g = b = l; // achromatic
            }
            else
            {
                var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                var p = 2 * l - q;
                r = HueToRgb(p, q, h + 1.0 / 3);
                g = HueToRgb(p, q, h);
                b = HueToRgb(p, q, h - 1.0 / 3);
            }

            return Color.FromArgb(255, (int)(r * 255), (int)(g * 255), (int)(b * 255));
        }

        private static double HueToRgb(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1.0 / 6) return p + (q - p) * 6 * t;
            if (t < 1.0 / 2) return q;
            if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3 - t) * 6;
            return p;
        }
    }

    public class Particle
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float VelX { get; set; }
        public float VelY { get; set; }
        public float Alpha { get; set; }
        public float Decay { get; set; }
        public Color BaseColor { get; set; }
        public float Size { get; set; }
        public bool IsDead => Alpha <= 0;

        public Particle(float x, float y, float vx, float vy, Color color, float decay, float size)
        {
            X = x;
            Y = y;
            VelX = vx;
            VelY = vy;
            BaseColor = color;
            Alpha = 1.0f;
            Decay = decay;
            Size = size;
        }

        public void Update(float gravity)
        {
            VelY += gravity;
            VelX *= 0.98f; // Air resistance
            VelY *= 0.98f;
            X += VelX;
            Y += VelY;
            Alpha -= Decay;
            if (Alpha < 0) Alpha = 0;
        }

        public void Draw(Graphics g)
        {
            int a = (int)(Alpha * 255);
            using (Brush b = new SolidBrush(Color.FromArgb(a, BaseColor)))
            {
                g.FillEllipse(b, X - Size / 2, Y - Size / 2, Size, Size);
            }
        }
    }

    public class Firework
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float VelX { get; set; }
        public float VelY { get; set; }
        public Color MainColor { get; set; }
        public bool Exploded { get; private set; }
        public List<Particle> Particles { get; private set; }
        
        private float _fuseTimer; 

        public Firework(float x, float y, float targetY, Color color)
        {
            X = x;
            Y = y;
            // Initial velocity calculation to reach roughly targetY
            // v^2 = u^2 + 2as -> u = sqrt(-2as) approx, but let's just use constant speed
            VelX = (float)(new Random().NextDouble() * 2 - 1); 
            VelY = (float)(-10 - (new Random().NextDouble() * 5)); // Initial launch speed
            
            MainColor = color;
            Exploded = false;
            Particles = new List<Particle>();
            _fuseTimer = 0;
        }

        public void Update(float gravity, float explosionForce, int particleCount, float particleSize)
        {
            if (!Exploded)
            {
                VelY += gravity;
                X += VelX;
                Y += VelY;

                // Simple physics: when it starts falling down or gets too slow, explode
                if (VelY >= -1.0f) 
                {
                    Explode(explosionForce, particleCount, particleSize);
                }
            }
            else
            {
                for (int i = Particles.Count - 1; i >= 0; i--)
                {
                    Particles[i].Update(gravity * 0.5f); // Particles float a bit more
                    if (Particles[i].IsDead)
                    {
                        Particles.RemoveAt(i);
                    }
                }
            }
        }

        private void Explode(float force, int count, float size)
        {
            Exploded = true;
            Random rnd = new Random();
            for (int i = 0; i < count; i++)
            {
                double angle = rnd.NextDouble() * Math.PI * 2;
                double speed = rnd.NextDouble() * force;
                float vx = (float)(Math.Cos(angle) * speed);
                float vy = (float)(Math.Sin(angle) * speed);
                float decay = (float)(0.01 + rnd.NextDouble() * 0.02);
                
                Particles.Add(new Particle(X, Y, vx, vy, MainColor, decay, size));
            }
        }

        public void Draw(Graphics g)
        {
            if (!Exploded)
            {
                using (Brush b = new SolidBrush(MainColor))
                {
                    g.FillEllipse(b, X - 2, Y - 2, 4, 4);
                }
            }
            else
            {
                foreach (var p in Particles)
                {
                    p.Draw(g);
                }
            }
        }

        public bool IsDead()
        {
            return Exploded && Particles.Count == 0;
        }
    }
}