using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using PhysicsSimulator.Simulations;
using System;
using System.Diagnostics;

namespace PhysicsSimulator.Controls
{
    public class SimulationCanvas : Control
    {
        private ISimulation? _simulation;
        private readonly DispatcherTimer _timer;
        private readonly Stopwatch _stopwatch;
        private double _lastTime;

        public static readonly StyledProperty<ISimulation?> SimulationProperty =
            AvaloniaProperty.Register<SimulationCanvas, ISimulation?>(nameof(Simulation));

        public ISimulation? Simulation
        {
            get => GetValue(SimulationProperty);
            set => SetValue(SimulationProperty, value);
        }

        public SimulationCanvas()
        {
            _stopwatch = Stopwatch.StartNew();
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
            };
            _timer.Tick += OnTick;
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == SimulationProperty)
            {
                _simulation = change.NewValue as ISimulation;
                _simulation?.Reset();
                if (_simulation != null && !_timer.IsEnabled)
                {
                    _timer.Start();
                }
            }
        }

        private void OnTick(object? sender, EventArgs e)
        {
            if (_simulation == null) return;

            double currentTime = _stopwatch.Elapsed.TotalSeconds;
            double deltaTime = currentTime - _lastTime;
            _lastTime = currentTime;

            if (deltaTime > 0.05) deltaTime = 0.05;

            _simulation.Update(deltaTime, Bounds.Size);
            InvalidateVisual();
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            context.FillRectangle(Brushes.Black, new Rect(Bounds.Size));

            if (_simulation != null)
            {
                _simulation.Draw(context, Bounds.Size);
            }
            else
            {
                var text = new FormattedText(
                    "Select a simulation from the menu",
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    Typeface.Default,
                    24,
                    Brushes.White
                );
                context.DrawText(text, new Point(Bounds.Width / 2 - text.Width / 2, Bounds.Height / 2 - text.Height / 2));
            }
        }

        protected override void OnPointerPressed(Avalonia.Input.PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            var point = e.GetCurrentPoint(this);
            _simulation?.OnPointerPressed(point, Bounds.Size);
        }

        protected override void OnPointerMoved(Avalonia.Input.PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            var point = e.GetCurrentPoint(this);
            _simulation?.OnPointerMoved(point, Bounds.Size);
        }

        protected override void OnPointerReleased(Avalonia.Input.PointerReleasedEventArgs e)
        {
             base.OnPointerReleased(e);
             var point = e.GetCurrentPoint(this);
             _simulation?.OnPointerReleased(point, Bounds.Size);
        }
    }
}