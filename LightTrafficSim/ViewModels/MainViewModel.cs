using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media;
using Avalonia.Threading;
using LightTrafficSim.Models;
using ReactiveUI;
using System.Reactive.Linq;

namespace LightTrafficSim.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        private DispatcherTimer _timer;
        private Random _random = new Random();
        private double _stateTimer = 0;
        private double _spawnTimer = 0;

        // Configuration
        public double RoadLength { get; } = 800;
        public double StopLineX { get; } = 600;
        public double CarLength { get; } = 40;
        public double CarHeight { get; } = 20;
        public double MinGap { get; } = 15;
        
        // Two Lanes (Center Y positions)
        // Road is 200px high, starting at Top=100.
        // Lane 1: Top ~140
        // Lane 2: Top ~200
        private double[] _lanes = new double[] { 140, 200 };

        // Traffic Light State
        private TrafficLightState _currentLightState = TrafficLightState.Red;
        public TrafficLightState CurrentLightState
        {
            get => _currentLightState;
            set 
            {
                this.RaiseAndSetIfChanged(ref _currentLightState, value);
                this.RaisePropertyChanged(nameof(RedLightOpacity));
                this.RaisePropertyChanged(nameof(YellowLightOpacity));
                this.RaisePropertyChanged(nameof(GreenLightOpacity));
            }
        }

        // Adjustable Parameters
        private double _greenDuration = 5.0; 
        public double GreenDuration
        {
            get => _greenDuration;
            set => this.RaiseAndSetIfChanged(ref _greenDuration, value);
        }

        private double _yellowDuration = 2.0;
        public double YellowDuration
        {
            get => _yellowDuration;
            set => this.RaiseAndSetIfChanged(ref _yellowDuration, value);
        }

        private double _redDuration = 5.0;
        public double RedDuration
        {
            get => _redDuration;
            set => this.RaiseAndSetIfChanged(ref _redDuration, value);
        }

        private double _spawnRate = 1.0; 
        public double SpawnRate
        {
            get => _spawnRate;
            set => this.RaiseAndSetIfChanged(ref _spawnRate, value);
        }
        
        // Debugging
        private int _carCount = 0;
        public int CarCount
        {
             get => _carCount;
             set => this.RaiseAndSetIfChanged(ref _carCount, value);
        }

        public ObservableCollection<Car> Cars { get; } = new ObservableCollection<Car>();

        // UI Helpers
        public double RedLightOpacity => CurrentLightState == TrafficLightState.Red ? 1.0 : 0.2;
        public double YellowLightOpacity => CurrentLightState == TrafficLightState.Yellow ? 1.0 : 0.2;
        public double GreenLightOpacity => CurrentLightState == TrafficLightState.Green ? 1.0 : 0.2;

        public MainViewModel()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
            };
            _timer.Tick += SimulationTick;
            _timer.Start();
            
            // Initial spawn
            TrySpawnCar();
        }

        private void SimulationTick(object? sender, EventArgs e)
        {
            double dt = 0.016; 

            // 1. Update Traffic Light
            _stateTimer += dt;
            if (CurrentLightState == TrafficLightState.Green && _stateTimer >= GreenDuration)
            {
                CurrentLightState = TrafficLightState.Yellow;
                _stateTimer = 0;
            }
            else if (CurrentLightState == TrafficLightState.Yellow && _stateTimer >= YellowDuration)
            {
                CurrentLightState = TrafficLightState.Red;
                _stateTimer = 0;
            }
            else if (CurrentLightState == TrafficLightState.Red && _stateTimer >= RedDuration)
            {
                CurrentLightState = TrafficLightState.Green;
                _stateTimer = 0;
            }

            // 2. Spawn Cars
            _spawnTimer += dt;
            if (SpawnRate > 0)
            {
                double interval = 1.0 / SpawnRate;
                if (_spawnTimer >= interval)
                {
                    double jitter = (_random.NextDouble() * 0.4 - 0.2) * interval;
                    if (_spawnTimer >= interval + jitter)
                    {
                        TrySpawnCar();
                        _spawnTimer = 0;
                    }
                }
            }

            // 3. Move Cars
            MoveCars(dt);
            
            // Update Count
            CarCount = Cars.Count;
        }

        private void TrySpawnCar()
        {
            // Pick a lane randomly
            int laneIndex = _random.Next(0, _lanes.Length);
            double laneY = _lanes[laneIndex];

            // Check specifically for cars in THIS lane at the start
            var lastCarInLane = Cars.Where(c => Math.Abs(c.Y - laneY) < 1).OrderByDescending(c => c.X).FirstOrDefault();
            
            if (lastCarInLane != null)
            {
                // Is the start blocked? (X=0)
                // If last car is still near start (X < Length + Gap)
                if (lastCarInLane.X < CarLength + MinGap)
                {
                    // Lane is blocked, try the other lane? Or just skip spawn this time
                    // Simple logic: Skip
                    return;
                }
            }

            // Create Car
            var color = new SolidColorBrush(Color.FromRgb(
                (byte)_random.Next(50, 200), 
                (byte)_random.Next(50, 200), 
                (byte)_random.Next(50, 200)));
            
            double speed = _random.Next(150, 300); 

            Cars.Add(new Car(0, laneY, color, speed));
        }

        private void MoveCars(double dt)
        {
            // We sort cars by X descending to process them in order (though logic below handles independent checks)
            var sortedCars = Cars.OrderByDescending(c => c.X).ToList();

            foreach (var car in sortedCars)
            {
                double proposedMove = car.Speed * dt;
                double nextX = car.X + proposedMove;

                // Constraint 1: Traffic Light (applies to all lanes)
                bool stoppingForLight = false;
                double stopTarget = StopLineX - MinGap;

                // Check if light is Red or Yellow
                if (CurrentLightState == TrafficLightState.Red || CurrentLightState == TrafficLightState.Yellow)
                {
                    // Only stop if we are currently BEHIND (or AT) the stop line
                    // Using <= and a small epsilon for stability
                    if (car.X <= stopTarget + 0.1)
                    {
                        // If the next move would take us PAST the stop line, clamp it
                        if (nextX >= stopTarget)
                        {
                            nextX = stopTarget;
                            stoppingForLight = true;
                        }
                    }
                }

                if (stoppingForLight)
                {
                    car.IsStopped = true;
                }
                else
                {
                    car.IsStopped = false;
                }

                // Constraint 2: Car Ahead (in same lane)
                // Find the closest car ahead in the same lane
                var carAhead = sortedCars
                    .Where(c => Math.Abs(c.Y - car.Y) < 1 && c.X > car.X)
                    .OrderBy(c => c.X)
                    .FirstOrDefault();

                if (carAhead != null)
                {
                    double safeDistance = carAhead.X - MinGap - CarLength;
                    if (nextX > safeDistance)
                    {
                        nextX = safeDistance;
                        // If blocked by car ahead, effectively stopped (though we don't set IsStopped for visuals here)
                    }
                }

                car.X = nextX;
            }

            // Remove cars that have left the screen
            var toRemove = Cars.Where(c => c.X > RoadLength + 100).ToList();
            foreach (var c in toRemove)
            {
                Cars.Remove(c);
            }
        }
    }
}