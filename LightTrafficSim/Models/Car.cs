using ReactiveUI;
using Avalonia.Media;

namespace LightTrafficSim.Models
{
    public class Car : ReactiveObject
    {
        private double _x;
        public double X 
        { 
            get => _x; 
            set => this.RaiseAndSetIfChanged(ref _x, value); 
        }

        private double _y; // Vertical position (lane)
        public double Y
        {
            get => _y;
            set => this.RaiseAndSetIfChanged(ref _y, value);
        }

        private bool _isStopped;
        public bool IsStopped 
        {
             get => _isStopped;
             set => this.RaiseAndSetIfChanged(ref _isStopped, value);
        }

        public double Speed { get; set; } = 2.0;
        public IBrush Color { get; set; }

        public Car(double startX, double startY, IBrush color, double speed)
        {
            X = startX;
            Y = startY;
            Color = color;
            Speed = speed;
        }
    }
}