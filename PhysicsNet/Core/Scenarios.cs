using System;
using System.Numerics;
using Avalonia.Media;
using System.Collections.Generic;

namespace PhysicsNet.Core
{
    // --- Sample 1: Pyramid Stack (Stability Test) ---
    public class ScenarioPyramid : IScenario
    {
        public string Name => "Pyramid Stack";
        public string Description => "Tests friction and stacking stability.";

        public void Setup(PhysicsWorld world)
        {
            world.Gravity = new Vector2(0, 500f);

            // Floor
            var ground = new Body(new Vector2(400, 580), 0f, new BoxShape(800, 40));
            ground.Color = Brushes.Gray;
            world.AddBody(ground);

            // Pyramid
            float boxSize = 30;
            int rows = 10;
            Vector2 startPos = new Vector2(400, 550);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    float x = startPos.X - (i * boxSize / 2) + (j * boxSize);
                    float y = startPos.Y - ((rows - i) * boxSize) - boxSize; 
                    
                    var box = new Body(new Vector2(x, y), 5f, new BoxShape(boxSize - 2, boxSize - 2));
                    box.Color = Brushes.CornflowerBlue;
                    box.StaticFriction = 0.6f;
                    world.AddBody(box);
                }
            }
        }
        public void Update(PhysicsWorld world, float dt) { }
        public void OnInput(string key, bool isPressed) { }
    }

    // --- Sample 2: Wrecking Ball (Joints & Momentum) ---
    public class ScenarioWreckingBall : IScenario
    {
        public string Name => "Wrecking Ball";
        public string Description => "Demonstrates chain joints and heavy impact.";

        public void Setup(PhysicsWorld world)
        {
            world.Gravity = new Vector2(0, 500f);

            // Wall of boxes
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    var b = new Body(new Vector2(200 + x * 32, 550 - y * 32), 2f, new BoxShape(30, 30));
                    b.Color = Brushes.IndianRed;
                    world.AddBody(b);
                }
            }
            
            // Floor
            var ground = new Body(new Vector2(400, 600), 0f, new BoxShape(800, 10));
            ground.Color = Brushes.Gray;
            world.AddBody(ground);

            // Chain Anchor
            var anchor = new Body(new Vector2(300, 50), 0f, new CircleShape(5));
            world.AddBody(anchor);

            // Chain Links
            Body prev = anchor;
            // FIX: Extended chain length from 8 to 16
            int chainLength = 8; 
            for (int i = 0; i < chainLength; i++)
            {
                var link = new Body(new Vector2(300, 130 + i * 30), 1f, new BoxShape(5, 20));
                link.Color = Brushes.White;
                world.AddBody(link);
                world.AddJoint(new SpringJoint(prev, link, 25f, 500f, 5f));
                prev = link;
            }

            // Heavy Ball
            var ball = new Body(new Vector2(300, 10 + chainLength * 30 + 20), 50f, new CircleShape(40));
            // FIX: Changed ball color to Blue
            ball.Color = Brushes.Blue; 
            world.AddBody(ball);
            world.AddJoint(new SpringJoint(prev, ball, 40f, 1000f, 5f));

            // Initial Push
            ball.ApplyForce(new Vector2(-50000, 0));
        }
        public void Update(PhysicsWorld world, float dt) { }
        public void OnInput(string key, bool isPressed) { }
    }

    // --- Sample 3: Platformer Controller (Interactive Game) ---
    public class ScenarioPlatformer : IScenario
    {
        public string Name => "Platformer Game";
        public string Description => "Use Arrow Keys/WASD to move the green box. SPACE to jump.";

        private Body _player;
        private bool _left, _right, _jump;

        public void Setup(PhysicsWorld world)
        {
            world.Gravity = new Vector2(0, 900f);

            // Ground & Platforms
            world.AddBody(new Body(new Vector2(400, 580), 0f, new BoxShape(800, 40)) { Color = Brushes.Gray });
            world.AddBody(new Body(new Vector2(200, 450), 0f, new BoxShape(200, 20)) { Color = Brushes.Gray });
            world.AddBody(new Body(new Vector2(600, 350), 0f, new BoxShape(200, 20)) { Color = Brushes.Gray });
            world.AddBody(new Body(new Vector2(100, 250), 0f, new BoxShape(100, 20)) { Color = Brushes.Gray });

            // Player
            _player = new Body(new Vector2(400, 500), 1f, new BoxShape(30, 30));
            _player.Color = Brushes.LimeGreen;
            _player.Restitution = 0f; // No bounce for better control
            _player.Rotation = 0f;
            // High friction to stop quickly
            _player.StaticFriction = 2.0f; 
            _player.DynamicFriction = 1.0f;
            world.AddBody(_player);

            // Some dynamic obstacles
            // The fix in Collisions.cs (CircleVsPolygon) prevents this circle from getting stuck in walls
            world.AddBody(new Body(new Vector2(50, 400), 1f, new CircleShape(50)) { Color = Brushes.MediumPurple });
            world.AddBody(new Body(new Vector2(650, 300), 1f, new BoxShape(20, 20)) { Color = Brushes.Orange });
        }

        public void Update(PhysicsWorld world, float dt)
        {
            float speed = 2000f;
            if (_left) _player.ApplyForce(new Vector2(-speed, 0));
            if (_right) _player.ApplyForce(new Vector2(speed, 0));
            
            // Very simple jump (allow jump if velocity Y is near zero)
            if (_jump && Math.Abs(_player.Velocity.Y) < 5f)
            {
                _player.Velocity = new Vector2(_player.Velocity.X, -550);
                _jump = false; // Reset jump trigger
            }

            // Lock rotation for platformer feel
            _player.Rotation = 0f;
            _player.AngularVelocity = 0f;
        }

        public void OnInput(string key, bool isPressed)
        {
            key = key.ToUpper();
            if (key == "A" || key == "LEFT") _left = isPressed;
            if (key == "D" || key == "RIGHT") _right = isPressed;
            if ((key == "W" || key == "SPACE") && isPressed) _jump = true;
        }
    }

    // --- Sample 4: Soft Body / Blob (Complex Joints) ---
    public class ScenarioSoftBody : IScenario
    {
        public string Name => "Soft Body Blob";
        public string Description => "A circle of bodies connected by springs to simulate a jelly-like object.";

        public void Setup(PhysicsWorld world)
        {
            world.Gravity = new Vector2(0, 300f);

            // Walls
            world.AddBody(new Body(new Vector2(400, 580), 0f, new BoxShape(800, 40)) { Color = Brushes.Gray });
            world.AddBody(new Body(new Vector2(10, 300), 0f, new BoxShape(20, 600)) { Color = Brushes.Gray });
            world.AddBody(new Body(new Vector2(790, 300), 0f, new BoxShape(20, 600)) { Color = Brushes.Gray });

            // Create Blob
            Vector2 center = new Vector2(400, 200);
            int segments = 12;
            float radius = 60f;
            List<Body> nodes = new List<Body>();

            // Outer ring
            for (int i = 0; i < segments; i++)
            {
                float angle = (float)i / segments * MathF.PI * 2;
                Vector2 pos = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
                var node = new Body(pos, 1f, new CircleShape(8));
                node.Color = Brushes.Pink;
                world.AddBody(node);
                nodes.Add(node);
            }

            // Center core
            var core = new Body(center, 1f, new CircleShape(10));
            core.Color = Brushes.HotPink;
            world.AddBody(core);

            // Connect nodes
            for (int i = 0; i < segments; i++)
            {
                var current = nodes[i];
                var next = nodes[(i + 1) % segments];

                // Connect to neighbor
                float dist = Vector2.Distance(current.Position, next.Position);
                world.AddJoint(new SpringJoint(current, next, dist, 200f, 5f));

                // Connect to core (pressure)
                world.AddJoint(new SpringJoint(current, core, radius, 200f, 5f));
                
                // Connect to opposite (structural stability)
                var opposite = nodes[(i + segments/2) % segments];
                world.AddJoint(new SpringJoint(current, opposite, radius * 2, 100f, 5f));
            }
        }
        public void Update(PhysicsWorld world, float dt) { }
        public void OnInput(string key, bool isPressed) { }
    }

    // --- Sample 5: Newton's Cradle (Restitution) ---
    public class ScenarioNewtonsCradle : IScenario
    {
        public string Name => "Newton's Cradle";
        public string Description => "Conservation of momentum and elasticity.";

        public void Setup(PhysicsWorld world)
        {
            world.Gravity = new Vector2(0, 500f);

            Vector2 startPos = new Vector2(250, 100);
            float radius = 25f;
            int count = 5;

            for (int i = 0; i < count; i++)
            {
                Vector2 anchorPos = startPos + new Vector2(i * (radius * 2), 0);
                var anchor = new Body(anchorPos, 0f, new CircleShape(5)); // Static
                world.AddBody(anchor);

                Vector2 ballPos = anchorPos + new Vector2(0, 200);
                
                // Pull the first one up
                if (i == 0) ballPos = anchorPos + new Vector2(-150, 50);

                var ball = new Body(ballPos, 10f, new CircleShape(radius));
                ball.Color = Brushes.Silver;
                ball.Restitution = 1.0f; // Perfect bounce
                ball.DynamicFriction = 0f;
                ball.StaticFriction = 0f;
                world.AddBody(ball);

                float len = Vector2.Distance(anchorPos, ballPos);
                world.AddJoint(new SpringJoint(anchor, ball, len, 2000f, 0.5f)); 
                // Note: Spring joint is slightly elastic, rigid rod is better for real cradle, but this works for demo
            }
        }
        public void Update(PhysicsWorld world, float dt) { }
        public void OnInput(string key, bool isPressed) { }
    }

    // --- Sample 6: Zero Gravity (Space) ---
    public class ScenarioZeroGravity : IScenario
    {
        public string Name => "Zero Gravity";
        public string Description => "No gravity. Objects float and bounce off walls.";

        public void Setup(PhysicsWorld world)
        {
            world.Gravity = Vector2.Zero;

            // Enclosure
            float w = 800, h = 600;
            float thick = 20;
            world.AddBody(new Body(new Vector2(w/2, 0), 0f, new BoxShape(w, thick)) { Color = Brushes.Gray }); // Top
            world.AddBody(new Body(new Vector2(w/2, h), 0f, new BoxShape(w, thick)) { Color = Brushes.Gray }); // Bottom
            world.AddBody(new Body(new Vector2(0, h/2), 0f, new BoxShape(thick, h)) { Color = Brushes.Gray }); // Left
            world.AddBody(new Body(new Vector2(w, h/2), 0f, new BoxShape(thick, h)) { Color = Brushes.Gray }); // Right

            Random rnd = new Random();
            for (int i = 0; i < 30; i++)
            {
                var pos = new Vector2(rnd.Next(50, 750), rnd.Next(50, 550));
                
                Body b;
                if (rnd.NextDouble() > 0.5)
                    b = new Body(pos, 1f, new CircleShape(rnd.Next(10, 25)));
                else
                    b = new Body(pos, 1f, new BoxShape(rnd.Next(20, 50), rnd.Next(20, 50)));

                b.Color = new SolidColorBrush(Color.FromRgb((byte)rnd.Next(100, 255), (byte)rnd.Next(100, 255), (byte)rnd.Next(100, 255)));
                b.Restitution = 0.9f;
                b.StaticFriction = 0f;
                b.DynamicFriction = 0f;
                
                // Random push
                b.Velocity = new Vector2(rnd.Next(-200, 200), rnd.Next(-200, 200));
                b.AngularVelocity = (float)(rnd.NextDouble() * 4 - 2);

                world.AddBody(b);
            }
        }
        public void Update(PhysicsWorld world, float dt) { }
        public void OnInput(string key, bool isPressed) { }
    }
}
