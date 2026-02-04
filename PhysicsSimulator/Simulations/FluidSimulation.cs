using Avalonia;
using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace PhysicsSimulator.Simulations
{
    public class FluidSimulation : SimulationBase
    {
        public override string Name => "Fluid Dynamics (Simple)";
        public override string Description => "Particle based fluid simulation. Click to spawn water.";

        private class Particle
        {
            public Point Position;
            public Vector Velocity;
            public double Life;
        }

        private List<Particle> _particles = new();
        private Random _random = new();
        private double _gravity = 500.0;

        public FluidSimulation()
        {
            Reset();
        }

        public override void Reset()
        {
            _particles.Clear();
        }

        public override void OnPointerPressed(Avalonia.Input.PointerPoint point, Size bounds)
        {
            // Spawn splash
            for(int i=0; i<50; i++)
            {
                SpawnParticle(point.Position);
            }
        }
        
        public override void OnPointerMoved(Avalonia.Input.PointerPoint point, Size bounds)
        {
            // Continuous stream if mouse down? 
            // Input handling in Avalonia is event based. 
            // We'll just spawn a few if moving
            if(point.Properties.IsLeftButtonPressed)
            {
                for(int i=0; i<5; i++)
                    SpawnParticle(point.Position);
            }
        }

        private void SpawnParticle(Point pos)
        {
             _particles.Add(new Particle
             {
                 Position = pos,
                 Velocity = new Vector(_random.Next(-100, 100), _random.Next(-50, 100)),
                 Life = 1.0
             });
        }

        public override void Update(double deltaTime, Size bounds)
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var p = _particles[i];
                p.Velocity = p.Velocity.WithY(p.Velocity.Y + _gravity * deltaTime);
                p.Position += p.Velocity * deltaTime;

                // Floor collision
                if (p.Position.Y > bounds.Height)
                {
                    p.Position = p.Position.WithY(bounds.Height);
                    p.Velocity = p.Velocity.WithY(-p.Velocity.Y * 0.5); // Dampen
                    p.Velocity = p.Velocity.WithX(p.Velocity.X * 0.9); // Friction
                }
                
                // Walls
                if(p.Position.X < 0 || p.Position.X > bounds.Width)
                {
                    p.Velocity = p.Velocity.WithX(-p.Velocity.X * 0.5);
                     p.Position = p.Position.WithX(Math.Clamp(p.Position.X, 0, bounds.Width));
                }

                // Interaction with other particles (Very simple repulsion to fake volume)
                // O(N^2) is too slow for many particles. We skip detailed collision for "Simple" demo.
                // Just pure quantity.
                
                // Fade out logic if we wanted smoke, but for water we keep them
                if (Math.Abs(p.Velocity.Y) < 10 && Math.Abs(p.Velocity.X) < 10 && p.Position.Y >= bounds.Height - 1)
                {
                    // Remove stagnant particles to keep performance up
                   // _particles.RemoveAt(i);
                }
                
                // Cap count
                if (_particles.Count > 1500)
                {
                     _particles.RemoveAt(0);
                }
            }
        }

        public override void Draw(DrawingContext context, Size bounds)
        {
            var brush = Brushes.DeepSkyBlue;
            foreach (var p in _particles)
            {
                context.DrawEllipse(brush, null, p.Position, 3, 3);
            }
        }
    }
}