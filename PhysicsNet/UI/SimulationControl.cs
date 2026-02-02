using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using PhysicsNet.Core;
using System;
using System.Diagnostics;
using System.Numerics;

namespace PhysicsNet.UI
{
    public class SimulationControl : Control
    {
        private PhysicsWorld _world;
        private DispatcherTimer _timer;
        private IScenario _currentScenario;
        
        public IScenario CurrentScenario => _currentScenario;

        public SimulationControl()
        {
            _world = new PhysicsWorld();
            
            // Default Scenario
            LoadScenario(new ScenarioPyramid());

            // Game Loop
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
            _timer.Tick += OnTick;
            _timer.Start();

            // Enable focus for input
            Focusable = true;
        }

        public void LoadScenario(IScenario scenario)
        {
            _currentScenario = scenario;
            _world.Clear();
            _world.Gravity = new Vector2(0, 9.8f * 50f); // Default gravity, scenario can override
            scenario.Setup(_world);
        }

        public void HandleInput(string key, bool pressed)
        {
            if (_currentScenario != null)
            {
                _currentScenario.OnInput(key, pressed);
            }
        }

        private void OnTick(object? sender, EventArgs e)
        {
            float dt = 1.0f / 60.0f; 

            // Scenario specific updates
            if (_currentScenario != null)
            {
                _currentScenario.Update(_world, dt);
            }

            _world.Step(dt);
            InvalidateVisual();
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            // Draw Background
            context.FillRectangle(Brushes.Black, Bounds);

            // Draw Joints
            foreach(var joint in _world.Joints)
            {
                var p1 = new Point(joint.BodyA.Position.X, joint.BodyA.Position.Y);
                var p2 = new Point(joint.BodyB.Position.X, joint.BodyB.Position.Y);
                context.DrawLine(new Pen(Brushes.White, 2), p1, p2);
            }

            // Draw Bodies
            foreach (var body in _world.Bodies)
            {
                var transform = context.PushTransform(Matrix.CreateRotation(body.Rotation) * Matrix.CreateTranslation(body.Position.X, body.Position.Y));

                if (body.Shape is CircleShape c)
                {
                    context.DrawEllipse(body.Color, null, new Point(0, 0), c.Radius, c.Radius);
                }
                else if (body.Shape is BoxShape b)
                {
                    context.DrawRectangle(body.Color, null, new Rect(-b.Width/2, -b.Height/2, b.Width, b.Height));
                }

                transform.Dispose();
            }
            
            // Draw Info
            string scenarioName = _currentScenario?.Name ?? "None";
            string desc = _currentScenario?.Description ?? "";
            
            context.DrawText(new FormattedText($"Scenario: {scenarioName}", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 16, Brushes.White), new Point(10, 10));
            context.DrawText(new FormattedText(desc, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 12, Brushes.LightGray), new Point(10, 30));
            context.DrawText(new FormattedText($"Bodies: {_world.Bodies.Count}", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 12, Brushes.Gray), new Point(10, 50));
        }
    }
}