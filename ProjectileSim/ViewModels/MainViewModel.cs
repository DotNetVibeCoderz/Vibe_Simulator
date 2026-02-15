using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace ProjectileSim.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        // Simulation Parameters
        [ObservableProperty] private double initialVelocity = 50.0;
        [ObservableProperty] private double launchAngle = 45.0;
        [ObservableProperty] private double initialHeight = 0.0;
        [ObservableProperty] private double mass = 1.0;
        [ObservableProperty] private double gravity = 9.81;
        [ObservableProperty] private double dragCoefficient = 0.47;
        [ObservableProperty] private bool enableDrag = false;

        // Current State
        [ObservableProperty] private double currentTime;
        [ObservableProperty] private double currentPositionX;
        [ObservableProperty] private double currentPositionY;
        [ObservableProperty] private double currentVelocityX;
        [ObservableProperty] private double currentVelocityY;
        [ObservableProperty] private double kineticEnergy;
        [ObservableProperty] private double potentialEnergy;
        [ObservableProperty] private double currentSpeed;

        // Statistics
        [ObservableProperty] private double maxHeight;
        [ObservableProperty] private double range;
        [ObservableProperty] private double flightTime;

        // Event for View to update UI
        public event Action? SimulationUpdated;

        private DispatcherTimer _timer;
        private double _dt = 0.016; // 60 FPS approx
        private bool _isRunning = false;

        public MainViewModel()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(_dt)
            };
            _timer.Tick += (s, e) => UpdateSimulation();
            Reset();
        }

        [RelayCommand]
        public void Start()
        {
            if (_isRunning) return;
            _isRunning = true;
            _timer.Start();
        }

        [RelayCommand]
        public void Pause()
        {
            _isRunning = false;
            _timer.Stop();
        }

        [RelayCommand]
        public void Reset()
        {
            Pause();
            CurrentTime = 0;
            CurrentPositionX = 0;
            CurrentPositionY = InitialHeight;
            
            double rad = LaunchAngle * (Math.PI / 180.0);
            CurrentVelocityX = InitialVelocity * Math.Cos(rad);
            CurrentVelocityY = InitialVelocity * Math.Sin(rad);
            
            MaxHeight = InitialHeight;
            Range = 0;
            FlightTime = 0;
            UpdateEnergy();
            
            // Notify view to clear trajectory
            SimulationUpdated?.Invoke();
        }

        private void UpdateSimulation()
        {
            // Simple Euler integration
            
            // Drag Force
            double dragFx = 0;
            double dragFy = 0;

            if (EnableDrag)
            {
                double v = Math.Sqrt(CurrentVelocityX * CurrentVelocityX + CurrentVelocityY * CurrentVelocityY);
                double rho = 1.225; // Air density
                double Area = 0.1; // Cross-sectional area
                double force = 0.5 * rho * v * v * DragCoefficient * Area;

                if (v > 0)
                {
                    // Direction is opposite to velocity
                    dragFx = -force * (CurrentVelocityX / v);
                    dragFy = -force * (CurrentVelocityY / v);
                }
            }

            double ax = dragFx / Mass;
            double ay = -Gravity + (dragFy / Mass);

            CurrentVelocityX += ax * _dt;
            CurrentVelocityY += ay * _dt;

            CurrentPositionX += CurrentVelocityX * _dt;
            CurrentPositionY += CurrentVelocityY * _dt;
            CurrentTime += _dt;

            // Ground collision
            if (CurrentPositionY <= 0)
            {
                CurrentPositionY = 0;
                CurrentVelocityX = 0;
                CurrentVelocityY = 0;
                Pause();
                FlightTime = CurrentTime;
                Range = CurrentPositionX;
            }
            else
            {
                FlightTime = CurrentTime; // While flying
                Range = CurrentPositionX;
            }

            if (CurrentPositionY > MaxHeight) MaxHeight = CurrentPositionY;

            UpdateEnergy();
            SimulationUpdated?.Invoke();
        }

        private void UpdateEnergy()
        {
            CurrentSpeed = Math.Sqrt(CurrentVelocityX * CurrentVelocityX + CurrentVelocityY * CurrentVelocityY);
            KineticEnergy = 0.5 * Mass * CurrentSpeed * CurrentSpeed;
            PotentialEnergy = Mass * Gravity * CurrentPositionY;
        }
    }
}