using System;

namespace CitySimulator.Data
{
    public class CityEngine
    {
        public int Population { get; private set; } = 1000;
        public int Funds { get; private set; } = 50000;
        public int Happiness { get; private set; } = 80;
        public double TimeOfDay { get; private set; } = 12.0; // Starts at Noon
        public string Weather { get; private set; } = "Clear";
        
        private System.Timers.Timer _simulationTimer;
        private Random _rnd = new Random();

        public event Action? OnStateChanged;

        public CityEngine()
        {
            _simulationTimer = new System.Timers.Timer(1000); // 1 tick per second
            _simulationTimer.Elapsed += Tick;
            _simulationTimer.Start();
        }

        private void Tick(object? sender, System.Timers.ElapsedEventArgs e)
        {
            // Time logic
            TimeOfDay += 0.5; // Half an hour passes every real second
            if (TimeOfDay >= 24)
            {
                TimeOfDay = 0;
            }

            // Weather randomizer (1% chance to change every tick)
            if (_rnd.Next(100) < 1)
            {
                int w = _rnd.Next(3);
                if (w == 0) Weather = "Clear";
                else if (w == 1) Weather = "Rain";
                else Weather = "Fog";
            }

            // Economic/Pop logic
            if (Happiness > 50)
            {
                Population += _rnd.Next(1, 10);
                Funds += Population / 100; // Taxes
            }
            else
            {
                Population -= _rnd.Next(1, 5);
                if (Population < 0) Population = 0;
            }

            // Fluctuate happiness
            Happiness += _rnd.Next(-2, 3);
            if (Happiness > 100) Happiness = 100;
            if (Happiness < 0) Happiness = 0;

            OnStateChanged?.Invoke();
        }

        public void ChangeWeather(string newWeather)
        {
            Weather = newWeather;
            OnStateChanged?.Invoke();
        }
        
        // --- NEW METHODS FOR CONTROL PANEL ---
        
        public void SetTimeOfDay(double time)
        {
            TimeOfDay = time;
            OnStateChanged?.Invoke();
        }
        
        public void AddFunds(int amount)
        {
            Funds += amount;
            OnStateChanged?.Invoke();
        }
    }
}