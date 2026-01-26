using System;
using System.Collections.Generic;
using System.Drawing;

namespace SonarSim
{
    public enum EntityType
    {
        Rock,       // Statis
        Diver,      // Lambat
        Fish,       // Medium, erratic
        Ship,       // Lambat/Medium, lurus
        Torpedo     // Cepat, lurus
    }

    public enum DetectionStatus
    {
        Unknown,
        Static,
        Approaching,
        Receding
    }

    public class SonarEntity
    {
        public Guid Id { get; private set; }
        public EntityType Type { get; set; }
        public PointF Position { get; set; }
        public float Speed { get; set; }
        public float Heading { get; set; } // 0-360 degrees
        public List<PointF> Trail { get; set; }
        public DateTime LastDetected { get; set; }
        public bool IsDetected { get; set; }
        public float DistanceFromCenter { get; private set; }
        
        // Helper untuk status pergerakan
        private float _lastDistance;

        public SonarEntity(EntityType type, PointF startPos, float speed, float heading)
        {
            Id = Guid.NewGuid();
            Type = type;
            Position = startPos;
            Speed = speed;
            Heading = heading;
            Trail = new List<PointF>();
            LastDetected = DateTime.MinValue;
            IsDetected = false;
            _lastDistance = 0;
            UpdateDistance(new PointF(0,0)); // Initialize distance
        }

        public void Move(Rectangle boundary, float simulationSpeedMultiplier)
        {
            if (Type == EntityType.Rock) return; // Objek statis tidak bergerak

            // Konversi heading ke vector movement
            float rad = (float)(Heading * Math.PI / 180.0);
            float dx = (float)(Math.Cos(rad) * Speed * simulationSpeedMultiplier);
            float dy = (float)(Math.Sin(rad) * Speed * simulationSpeedMultiplier);

            float newX = Position.X + dx;
            float newY = Position.Y + dy;

            // Simple bounce logic or wrap around? Let's do Bounce for simplicity to keep them in screen
            bool bounced = false;
            if (newX < boundary.Left || newX > boundary.Right)
            {
                Heading = 180 - Heading;
                bounced = true;
            }
            if (newY < boundary.Top || newY > boundary.Bottom)
            {
                Heading = -Heading;
                bounced = true;
            }

            if (Heading < 0) Heading += 360;
            if (Heading > 360) Heading -= 360;

            if (!bounced)
            {
                Position = new PointF(newX, newY);
            }
            else
            {
                // Recalculate move after bounce
                rad = (float)(Heading * Math.PI / 180.0);
                dx = (float)(Math.Cos(rad) * Speed * simulationSpeedMultiplier);
                dy = (float)(Math.Sin(rad) * Speed * simulationSpeedMultiplier);
                Position = new PointF(Position.X + dx, Position.Y + dy);
            }

            // Batasi panjang trail
            if (Trail.Count > 20) Trail.RemoveAt(0);
        }

        public DetectionStatus GetStatus()
        {
            if (Type == EntityType.Rock) return DetectionStatus.Static;
            
            float diff = _lastDistance - DistanceFromCenter;
            // Jika diff positif, berarti jarak mengecil (Approaching)
            // Jika diff negatif, berarti jarak membesar (Receding)
            if (Math.Abs(diff) < 0.1f) return DetectionStatus.Static; 
            return diff > 0 ? DetectionStatus.Approaching : DetectionStatus.Receding;
        }

        public void UpdateDistance(PointF center)
        {
            _lastDistance = DistanceFromCenter;
            DistanceFromCenter = (float)Math.Sqrt(Math.Pow(Position.X - center.X, 2) + Math.Pow(Position.Y - center.Y, 2));
        }

        public void AddTrailPoint()
        {
            Trail.Add(Position);
        }
    }
}