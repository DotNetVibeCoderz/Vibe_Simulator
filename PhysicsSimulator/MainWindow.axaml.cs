using Avalonia.Controls;
using Avalonia.Interactivity;
using PhysicsSimulator.Simulations;
using System.Collections.Generic;
using System.Linq;

namespace PhysicsSimulator
{
    public partial class MainWindow : Window
    {
        private List<ISimulation> _simulations = new();

        public MainWindow()
        {
            InitializeComponent();
            InitializeSimulations();
        }

        private void InitializeSimulations()
        {
            _simulations = new List<ISimulation>
            {
                new RigidBodySimulation(),
                new PendulumSimulation(),
                new FluidSimulation(),
                new GravitySimulation(),
                new WaveSimulation(),
                new CrowdSimulation()
            };

            SimList.ItemsSource = _simulations.Select(s => s.Name).ToList();
            SimList.SelectionChanged += OnSelectionChanged;
            SimList.SelectedIndex = 0;
        }

        private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (SimList.SelectedIndex >= 0 && SimList.SelectedIndex < _simulations.Count)
            {
                var sim = _simulations[SimList.SelectedIndex];
                SimCanvas.Simulation = sim;
                TitleText.Text = sim.Name;
                DescText.Text = sim.Description;
            }
        }

        private void OnResetClick(object? sender, RoutedEventArgs e)
        {
            SimCanvas.Simulation?.Reset();
        }
    }
}