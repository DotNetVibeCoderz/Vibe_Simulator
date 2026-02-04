using Avalonia;
using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace PhysicsSimulator.Simulations
{
    public class PendulumSimulation : SimulationBase
    {
        public override string Name => "Pendulum & Springs";
        public override string Description => "Double Pendulum Chaos. Shows trace of the movement.";

        private double r1 = 125;
        private double r2 = 125;
        private double m1 = 20;
        private double m2 = 20;
        private double a1 = Math.PI / 2;
        private double a2 = Math.PI / 2;
        private double a1_v = 0;
        private double a2_v = 0;
        private double g = 1;

        private List<Point> _trace = new();
        private Point _offset;
        
        public PendulumSimulation()
        {
            Reset();
        }

        public override void Reset()
        {
            a1 = Math.PI / 2;
            a2 = Math.PI / 2;
            a1_v = 0;
            a2_v = 0;
            _trace.Clear();
        }

        public override void Update(double deltaTime, Size bounds)
        {
            _offset = new Point(bounds.Width / 2, 200);

            // Runge-Kutta is better, but let's stick to simple calculation for simplicity of code
            // Using logic from https://www.myphysicslab.com/pendulum/double-pendulum-en.html logic roughly
            
            // Speed up simulation
            // deltaTime *= 5; 

            double num1 = -g * (2 * m1 + m2) * Math.Sin(a1);
            double num2 = -m2 * g * Math.Sin(a1 - 2 * a2);
            double num3 = -2 * Math.Sin(a1 - a2) * m2;
            double num4 = a2_v * a2_v * r2 + a1_v * a1_v * r1 * Math.Cos(a1 - a2);
            double den = r1 * (2 * m1 + m2 - m2 * Math.Cos(2 * a1 - 2 * a2));
            double a1_a = (num1 + num2 + num3 * num4) / den;

            num1 = 2 * Math.Sin(a1 - a2);
            num2 = (a1_v * a1_v * r1 * (m1 + m2));
            num3 = g * (m1 + m2) * Math.Cos(a1);
            num4 = a2_v * a2_v * r2 * m2 * Math.Cos(a1 - a2);
            den = r2 * (2 * m1 + m2 - m2 * Math.Cos(2 * a1 - 2 * a2));
            double a2_a = (num1 * (num2 + num3 + num4)) / den;

            a1_v += a1_a;
            a2_v += a2_a;
            a1 += a1_v;
            a2 += a2_v;
            
            // Damping to prevent explosion
            a1_v *= 0.999;
            a2_v *= 0.999;

            double x1 = r1 * Math.Sin(a1);
            double y1 = r1 * Math.Cos(a1);
            double x2 = x1 + r2 * Math.Sin(a2);
            double y2 = y1 + r2 * Math.Cos(a2);

            _trace.Add(new Point(x2, y2) + _offset);
            if (_trace.Count > 500) _trace.RemoveAt(0);
        }

        public override void Draw(DrawingContext context, Size bounds)
        {
            double x1 = r1 * Math.Sin(a1);
            double y1 = r1 * Math.Cos(a1);
            double x2 = x1 + r2 * Math.Sin(a2);
            double y2 = y1 + r2 * Math.Cos(a2);

            Point p0 = _offset;
            Point p1 = new Point(x1, y1) + _offset;
            Point p2 = new Point(x2, y2) + _offset;

            // Draw arms
            context.DrawLine(new Pen(Brushes.White, 3), p0, p1);
            context.DrawLine(new Pen(Brushes.White, 3), p1, p2);

            // Draw masses
            context.DrawEllipse(Brushes.Cyan, null, p1, m1/2, m1/2);
            context.DrawEllipse(Brushes.Magenta, null, p2, m2/2, m2/2);

            // Draw trace
            if (_trace.Count > 1)
            {
                var geometry = new StreamGeometry();
                using (var ctx = geometry.Open())
                {
                    ctx.BeginFigure(_trace[0], false);
                    for (int i = 1; i < _trace.Count; i++)
                    {
                        ctx.LineTo(_trace[i]);
                    }
                    ctx.EndFigure(false);
                }
                context.DrawGeometry(null, new Pen(Brushes.Yellow, 1), geometry);
            }
        }
    }
}