using Avalonia;
using System;

namespace PhysicsSimulator
{
    public static class Extensions
    {
        public static Vector Normalize(this Vector v)
        {
            double l = Math.Sqrt(v.X * v.X + v.Y * v.Y);
            if (l == 0) return new Vector(0, 0);
            return v / l;
        }

        // Avalonia Point has WithX/Y usually, but let's ensure Vector does too or if Point misses it
        public static Point WithX(this Point p, double x) => new Point(x, p.Y);
        public static Point WithY(this Point p, double y) => new Point(p.X, y);

        public static Vector WithX(this Vector v, double x) => new Vector(x, v.Y);
        public static Vector WithY(this Vector v, double y) => new Vector(v.X, y);
    }
}