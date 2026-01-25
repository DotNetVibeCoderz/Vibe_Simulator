using System;
using System.Drawing;

namespace RagdollSim
{
    // Represents a single particle in the physics world
    public class Point
    {
        public float X, Y;
        public float OldX, OldY;
        public bool Pinned = false; // If true, the point won't move (useful for hanging the ragdoll)

        public Point(float x, float y)
        {
            X = x;
            Y = y;
            OldX = x;
            OldY = y;
        }

        // Verlet integration step
        public void Update(float friction, float gravity, float width, float height)
        {
            if (Pinned) return;

            float vx = (X - OldX) * friction;
            float vy = (Y - OldY) * friction;

            OldX = X;
            OldY = Y;

            X += vx;
            Y += vy + gravity;

            // Simple boundary collision
            if (X < 0) { X = 0; OldX = X + vx; }
            else if (X > width) { X = width; OldX = X + vx; }

            if (Y < 0) { Y = 0; OldY = Y + vy; }
            else if (Y > height) { Y = height; OldY = Y + vy; }
        }
        
        // Force move (for dragging)
        public void SetPosition(float x, float y)
        {
            X = x;
            Y = y;
            // Reset velocity to avoid flinging
            OldX = x;
            OldY = y;
        }
    }

    // Represents a solid connection between two points
    public class Stick
    {
        public Point P0, P1;
        public float Length;

        public Stick(Point p0, Point p1)
        {
            P0 = p0;
            P1 = p1;
            // Calculate initial distance
            float dx = P1.X - P0.X;
            float dy = P1.Y - P0.Y;
            Length = (float)Math.Sqrt(dx * dx + dy * dy);
        }

        public void Update()
        {
            float dx = P1.X - P0.X;
            float dy = P1.Y - P0.Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            
            // Avoid division by zero
            if (dist == 0) dist = 0.001f;

            float diff = Length - dist;
            float percent = diff / dist / 2;
            
            float offsetX = dx * percent;
            float offsetY = dy * percent;

            if (!P0.Pinned)
            {
                P0.X -= offsetX;
                P0.Y -= offsetY;
            }

            if (!P1.Pinned)
            {
                P1.X += offsetX;
                P1.Y += offsetY;
            }
        }

        public void Draw(Graphics g, Pen pen)
        {
            g.DrawLine(pen, P0.X, P0.Y, P1.X, P1.Y);
        }
    }
}
