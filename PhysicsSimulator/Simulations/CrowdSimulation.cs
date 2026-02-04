using Avalonia;
using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace PhysicsSimulator.Simulations
{
    public class CrowdSimulation : SimulationBase
    {
        public override string Name => "Crowd Simulation (Boids)";
        public override string Description => "Flocking behavior. They follow the mouse if pressed.";

        private class Boid
        {
            public Point Position;
            public Vector Velocity;
            public Vector Acceleration;
        }

        private List<Boid> _boids = new();
        private Random _rand = new();
        private double _maxSpeed = 200;
        private Point _target;
        private bool _hasTarget = false;

        public CrowdSimulation()
        {
            Reset();
        }

        public override void Reset()
        {
            _boids.Clear();
            for (int i = 0; i < 100; i++)
            {
                _boids.Add(new Boid
                {
                    Position = new Point(_rand.Next(0, 800), _rand.Next(0, 600)),
                    Velocity = new Vector(_rand.NextDouble() * 2 - 1, _rand.NextDouble() * 2 - 1) * 50
                });
            }
        }

        public override void OnPointerPressed(Avalonia.Input.PointerPoint point, Size bounds)
        {
            _target = point.Position;
            _hasTarget = true;
        }

        public override void OnPointerMoved(Avalonia.Input.PointerPoint point, Size bounds)
        {
            if (point.Properties.IsLeftButtonPressed)
            {
                _target = point.Position;
                _hasTarget = true;
            }
        }
        
        public override void OnPointerReleased(Avalonia.Input.PointerPoint point, Size bounds)
        {
            _hasTarget = false;
        }

        public override void Update(double deltaTime, Size bounds)
        {
            foreach (var boid in _boids)
            {
                Vector align = Align(boid);
                Vector cohere = Cohesion(boid);
                Vector separate = Separation(boid);
                Vector seek = _hasTarget ? Seek(boid, _target) : new Vector(0,0);

                boid.Acceleration += align * 1.0;
                boid.Acceleration += cohere * 1.0;
                boid.Acceleration += separate * 1.5;
                if(_hasTarget) boid.Acceleration += seek * 2.0;

                boid.Velocity += boid.Acceleration * deltaTime;
                
                // Limit speed
                double speed = Math.Sqrt(boid.Velocity.X * boid.Velocity.X + boid.Velocity.Y * boid.Velocity.Y);
                if (speed > _maxSpeed)
                {
                    boid.Velocity = (boid.Velocity / speed) * _maxSpeed;
                }

                boid.Position += boid.Velocity * deltaTime;
                boid.Acceleration = new Vector(0, 0);

                // Wrap around
                if (boid.Position.X < 0) boid.Position = boid.Position.WithX(bounds.Width);
                if (boid.Position.X > bounds.Width) boid.Position = boid.Position.WithX(0);
                if (boid.Position.Y < 0) boid.Position = boid.Position.WithY(bounds.Height);
                if (boid.Position.Y > bounds.Height) boid.Position = boid.Position.WithY(0);
            }
        }

        private Vector Seek(Boid boid, Point target)
        {
            Vector desired = target - boid.Position;
            double d = Math.Sqrt(desired.X * desired.X + desired.Y * desired.Y);
            if (d == 0) return new Vector(0,0);
            
            desired = (desired / d) * _maxSpeed;
            Vector steer = desired - boid.Velocity;
            
             // Limit force
             // Not implemented for simplicity, assumed logic handles it naturally via mass
            return steer;
        }

        private Vector Align(Boid boid)
        {
            double perceptionRadius = 50;
            Vector steering = new Vector();
            int total = 0;
            foreach (var other in _boids)
            {
                double d = Distance(boid.Position, other.Position);
                if (other != boid && d < perceptionRadius)
                {
                    steering += other.Velocity;
                    total++;
                }
            }
            if (total > 0)
            {
                steering /= total;
                steering = steering.Normalize() * _maxSpeed; // This might crash if zero
                if(double.IsNaN(steering.X)) steering = new Vector(0,0);
                steering -= boid.Velocity;
            }
            return steering;
        }

        private Vector Cohesion(Boid boid)
        {
            double perceptionRadius = 50;
            Vector steering = new Vector();
            int total = 0;
            foreach (var other in _boids)
            {
                double d = Distance(boid.Position, other.Position);
                if (other != boid && d < perceptionRadius)
                {
                    steering += (Vector)other.Position; // Cast Point to Vector for addition? No Point + Point invalid.
                    // We need to accumulate points then divide
                    // Actually steering is a Vector accumulation here logic-wise
                     steering = steering + new Vector(other.Position.X, other.Position.Y);
                    total++;
                }
            }
            if (total > 0)
            {
                steering /= total;
                // Convert back to point concept to Seek
                Point center = new Point(steering.X, steering.Y);
                return Seek(boid, center);
            }
            return new Vector(0, 0);
        }

        private Vector Separation(Boid boid)
        {
            double perceptionRadius = 25;
            Vector steering = new Vector();
            int total = 0;
            foreach (var other in _boids)
            {
                double d = Distance(boid.Position, other.Position);
                if (other != boid && d < perceptionRadius)
                {
                    Vector diff = boid.Position - other.Position;
                    diff /= (d * d); // Weight by distance
                    steering += diff;
                    total++;
                }
            }
            if (total > 0)
            {
                steering /= total;
                steering = steering.Normalize() * _maxSpeed;
                 if(double.IsNaN(steering.X)) steering = new Vector(0,0);
                steering -= boid.Velocity;
            }
            return steering;
        }

        private double Distance(Point p1, Point p2)
        {
            double dx = p1.X - p2.X;
            double dy = p1.Y - p2.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public override void Draw(DrawingContext context, Size bounds)
        {
            var brush = Brushes.LimeGreen;
            foreach (var boid in _boids)
            {
                // Draw triangle oriented to velocity
                double angle = Math.Atan2(boid.Velocity.Y, boid.Velocity.X);
                
                var transform = new MatrixTransform(Matrix.CreateRotation(angle) * Matrix.CreateTranslation(boid.Position));
                
                using (context.PushTransform(transform.Value))
                {
                     // Triangle pointing right (since 0 angle is right)
                     var geometry = new StreamGeometry();
                     using(var ctx = geometry.Open())
                     {
                         ctx.BeginFigure(new Point(10, 0), true);
                         ctx.LineTo(new Point(-5, 5));
                         ctx.LineTo(new Point(-5, -5));
                         ctx.EndFigure(true);
                     }
                     context.DrawGeometry(brush, null, geometry);
                }
            }
        }
    }
}