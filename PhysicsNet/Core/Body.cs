using System.Numerics;
using Avalonia.Media; // Untuk warna

namespace PhysicsNet.Core
{
    public enum ShapeType
    {
        Circle,
        Box
    }

    public abstract class Shape
    {
        public ShapeType Type { get; }
        
        protected Shape(ShapeType type)
        {
            Type = type;
        }
    }

    public class CircleShape : Shape
    {
        public float Radius { get; }

        public CircleShape(float radius) : base(ShapeType.Circle)
        {
            Radius = radius;
        }
    }

    public class BoxShape : Shape
    {
        public float Width { get; }
        public float Height { get; }

        public BoxShape(float width, float height) : base(ShapeType.Box)
        {
            Width = width;
            Height = height;
        }
    }

    public class Body
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Vector2 Force;
        
        public float Rotation; // Dalam radian
        public float AngularVelocity;
        public float Torque;

        public Shape Shape { get; }
        public float Mass { get; private set; }
        public float InverseMass { get; private set; }
        public float Inertia { get; private set; }
        public float InverseInertia { get; private set; }
        
        public float Restitution { get; set; } = 0.5f; // Kelentingan (0 = tidak memantul, 1 = memantul sempurna)
        public float StaticFriction { get; set; } = 0.5f;
        public float DynamicFriction { get; set; } = 0.3f;
        
        public bool IsStatic => InverseMass == 0f;

        public IBrush Color { get; set; } = Brushes.White;

        public Body(Vector2 position, float mass, Shape shape)
        {
            Position = position;
            Shape = shape;
            SetMass(mass);
        }

        private void SetMass(float mass)
        {
            Mass = mass;
            if (mass != 0f)
            {
                InverseMass = 1f / mass;
                Inertia = CalculateInertia(Shape, mass);
                InverseInertia = (Inertia != 0f) ? 1f / Inertia : 0f;
            }
            else
            {
                InverseMass = 0f;
                Inertia = 0f;
                InverseInertia = 0f;
            }
        }

        private float CalculateInertia(Shape shape, float mass)
        {
            if (shape is CircleShape c)
            {
                return 0.5f * mass * c.Radius * c.Radius;
            }
            else if (shape is BoxShape b)
            {
                return (1f / 12f) * mass * (b.Width * b.Width + b.Height * b.Height);
            }
            return 0f;
        }

        public void ApplyForce(Vector2 force)
        {
            Force += force;
        }

        public void Integrate(float dt)
        {
            if (IsStatic) return;

            // Linear
            Vector2 acceleration = Force * InverseMass;
            Velocity += acceleration * dt;
            Position += Velocity * dt;

            // Angular (Simple Euler)
            float angularAcc = Torque * InverseInertia;
            AngularVelocity += angularAcc * dt;
            Rotation += AngularVelocity * dt;

            // Clear forces
            Force = Vector2.Zero;
            Torque = 0f;
        }
    }
}