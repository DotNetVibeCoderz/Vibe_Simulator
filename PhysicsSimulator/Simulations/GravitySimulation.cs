using Avalonia;
using Avalonia.Media;
using Avalonia.Input;
using System;
using System.Collections.Generic;

namespace PhysicsSimulator.Simulations
{
    public class GravitySimulation : SimulationBase
    {
        public override string Name => "Gravity & Orbits";
        public override string Description => "N-Body Simulation. Drag and release to shoot planets.";

        private class Body
        {
            public Point Position;
            public Vector Velocity;
            public double Mass;
            public double Radius;
            public IBrush Brush { get; set; } = Brushes.White;
            public bool IsStatic;
        }

        private List<Body> _bodies = new();
        private Point? _dragStart;
        private Point _currentMousePos;
        private Random _rand = new();

        public GravitySimulation()
        {
            Reset();
        }

        public override void Reset()
        {
            _bodies.Clear();
            // Sun
            _bodies.Add(new Body
            {
                Position = new Point(400, 300),
                Velocity = new Vector(0, 0),
                Mass = 50000,
                Radius = 30,
                Brush = Brushes.Yellow,
                IsStatic = true
            });
        }

        public override void OnPointerPressed(PointerPoint point, Size bounds)
        {
            if (point.Properties.IsLeftButtonPressed)
            {
                _dragStart = point.Position;
                _currentMousePos = point.Position;
            }
        }

        public override void OnPointerMoved(PointerPoint point, Size bounds)
        {
            if (_dragStart.HasValue)
            {
                _currentMousePos = point.Position;
            }
        }

        public override void OnPointerReleased(PointerPoint point, Size bounds)
        {
            if (_dragStart.HasValue)
            {
                var start = _dragStart.Value;
                var end = point.Position;
                var velocity = (start - end) * 1.5; // Drag direction opposite to shoot

                _bodies.Add(new Body
                {
                    Position = start,
                    Velocity = velocity,
                    Mass = 100,
                    Radius = 10,
                    Brush = new SolidColorBrush(Color.FromRgb((byte)_rand.Next(100,255), (byte)_rand.Next(100,255), (byte)_rand.Next(100,255))),
                    IsStatic = false
                });

                _dragStart = null;
            }
        }

        public override void Update(double deltaTime, Size bounds)
        {
            // Calculate Forces
            // F = G * m1 * m2 / r^2
            double G = 5.0; // Adjusted G constant

            for (int i = 0; i < _bodies.Count; i++)
            {
                var b1 = _bodies[i];
                if (b1.IsStatic) continue;

                Vector acceleration = new Vector(0, 0);

                for (int j = 0; j < _bodies.Count; j++)
                {
                    if (i == j) continue;
                    var b2 = _bodies[j];

                    var dir = b2.Position - b1.Position;
                    double distSq = dir.X * dir.X + dir.Y * dir.Y;
                    double dist = Math.Sqrt(distSq);

                    if (dist < b1.Radius + b2.Radius) // Collision/Merge logic could go here
                    {
                        // Minimal distance to prevent infinite force
                        dist = b1.Radius + b2.Radius;
                        distSq = dist * dist;
                    }

                    double f = G * b2.Mass / distSq; // a = F/m1 = (G*m1*m2/r^2)/m1 = G*m2/r^2
                    acceleration += (dir / dist) * f;
                }

                b1.Velocity += acceleration * deltaTime;
            }

            // Apply Velocity
            foreach (var b in _bodies)
            {
                if (!b.IsStatic)
                    b.Position += b.Velocity * deltaTime;
            }
            
            // Cleanup far away bodies
             _bodies.RemoveAll(b => b.Position.X < -5000 || b.Position.X > 5000 || b.Position.Y < -5000 || b.Position.Y > 5000);
        }

        public override void Draw(DrawingContext context, Size bounds)
        {
            foreach (var b in _bodies)
            {
                context.DrawEllipse(b.Brush, null, b.Position, b.Radius, b.Radius);
            }

            if (_dragStart.HasValue)
            {
                context.DrawLine(new Pen(Brushes.White, 2, new DashStyle(new double[]{4,2}, 0)), _dragStart.Value, _currentMousePos);
            }
        }
    }
}