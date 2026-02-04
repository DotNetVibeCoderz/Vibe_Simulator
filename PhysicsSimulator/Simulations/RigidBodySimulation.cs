using Avalonia;
using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace PhysicsSimulator.Simulations
{
    public class RigidBodySimulation : SimulationBase
    {
        public override string Name => "Rigid Body Collisions";
        public override string Description => "Bouncing balls with elastic collisions. Click to add balls.";

        private class Ball
        {
            public Point Position;
            public Vector Velocity;
            public double Radius;
            public IBrush Brush { get; set; } = Brushes.Blue;
        }

        private List<Ball> _balls = new();
        private Random _random = new();

        public RigidBodySimulation()
        {
            Reset();
        }

        public override void Reset()
        {
            _balls.Clear();
            for (int i = 0; i < 10; i++)
            {
                AddRandomBall(new Point(400, 300));
            }
        }

        private void AddRandomBall(Point center)
        {
            _balls.Add(new Ball
            {
                Position = center + new Point(_random.Next(-100, 100), _random.Next(-100, 100)),
                Velocity = new Vector(_random.Next(-200, 200), _random.Next(-200, 200)),
                Radius = _random.Next(10, 30),
                Brush = new SolidColorBrush(Color.FromRgb((byte)_random.Next(50, 255), (byte)_random.Next(50, 255), (byte)_random.Next(50, 255)))
            });
        }

        public override void OnPointerPressed(Avalonia.Input.PointerPoint point, Size bounds)
        {
            AddRandomBall(point.Position);
        }

        public override void Update(double deltaTime, Size bounds)
        {
            foreach (var ball in _balls)
            {
                ball.Position += ball.Velocity * deltaTime;

                // Wall collisions
                if (ball.Position.X - ball.Radius < 0)
                {
                    ball.Position = ball.Position.WithX(ball.Radius);
                    ball.Velocity = ball.Velocity.WithX(-ball.Velocity.X);
                }
                else if (ball.Position.X + ball.Radius > bounds.Width)
                {
                    ball.Position = ball.Position.WithX(bounds.Width - ball.Radius);
                    ball.Velocity = ball.Velocity.WithX(-ball.Velocity.X);
                }

                if (ball.Position.Y - ball.Radius < 0)
                {
                    ball.Position = ball.Position.WithY(ball.Radius);
                    ball.Velocity = ball.Velocity.WithY(-ball.Velocity.Y);
                }
                else if (ball.Position.Y + ball.Radius > bounds.Height)
                {
                    ball.Position = ball.Position.WithY(bounds.Height - ball.Radius);
                    ball.Velocity = ball.Velocity.WithY(-ball.Velocity.Y);
                }
            }

            // Ball collisions (Simple O(N^2))
            for (int i = 0; i < _balls.Count; i++)
            {
                for (int j = i + 1; j < _balls.Count; j++)
                {
                    var b1 = _balls[i];
                    var b2 = _balls[j];

                    var delta = b2.Position - b1.Position;
                    double distSq = delta.X * delta.X + delta.Y * delta.Y;
                    double minDist = b1.Radius + b2.Radius;

                    if (distSq < minDist * minDist)
                    {
                        double dist = Math.Sqrt(distSq);
                        if (dist == 0) continue; // prevent div by zero

                        // Normal
                        var n = delta / dist;

                        // Resolve overlap
                        double overlap = minDist - dist;
                        var correction = n * (overlap / 2.0);
                        b1.Position -= correction;
                        b2.Position += correction;

                        // Resolve Velocity (Elastic)
                        // v1' = v1 - 2*m2/(m1+m2) * dot(v1-v2, n) * n
                        // Mass proportional to area (radius^2)
                        double m1 = b1.Radius * b1.Radius;
                        double m2 = b2.Radius * b2.Radius;

                        // Relative velocity along normal
                        double dotRel = (b1.Velocity.X - b2.Velocity.X) * n.X + (b1.Velocity.Y - b2.Velocity.Y) * n.Y;

                        if (dotRel > 0) continue; // Moving apart

                        // Simple perfectly elastic
                        // Impulse scalar j = -(1 + e) * vel_along_normal / (1/m1 + 1/m2)
                        // e = 1 for elastic
                        double impulseScalar = -(1 + 1) * dotRel / (1/m1 + 1/m2);

                        var impulse = n * impulseScalar;
                        b1.Velocity += impulse / m1;
                        b2.Velocity -= impulse / m2;
                    }
                }
            }
        }

        public override void Draw(DrawingContext context, Size bounds)
        {
            foreach (var ball in _balls)
            {
                context.DrawEllipse(ball.Brush, null, ball.Position, ball.Radius, ball.Radius);
            }
        }
    }
}