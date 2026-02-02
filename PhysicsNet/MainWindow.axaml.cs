using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PhysicsNet.Core;
using PhysicsNet.UI;
using System.Collections.Generic;

namespace PhysicsNet
{
    public partial class MainWindow : Window
    {
        private List<IScenario> _scenarios;

        public MainWindow()
        {
            InitializeComponent();
            
            _scenarios = new List<IScenario>
            {
                new ScenarioPyramid(),
                new ScenarioWreckingBall(),
                new ScenarioPlatformer(),
                new ScenarioSoftBody(),
                new ScenarioNewtonsCradle(),
                new ScenarioZeroGravity()
            };

            // Removed ComboBox Logic
            
            // Load initial
            LoadScenario(0);
        }

        // FIX: Added handler for Buttons
        private void OnScenarioButtonClick(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tagStr)
            {
                if (int.TryParse(tagStr, out int index))
                {
                    LoadScenario(index);
                }
            }
        }

        private void LoadScenario(int index)
        {
            if (index < 0 || index >= _scenarios.Count) return;

            var scenario = _scenarios[index];
            var sim = this.FindControl<SimulationControl>("SimControl");
            var txt = this.FindControl<TextBlock>("InstructionText");

            if (sim != null) sim.LoadScenario(scenario);
            if (txt != null) txt.Text = scenario.Description;
            
            // Focus window to capture keys immediately
            this.Focus();
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            var sim = this.FindControl<SimulationControl>("SimControl");
            if (sim != null) sim.HandleInput(e.Key.ToString(), true);
        }

        private void OnKeyUp(object? sender, KeyEventArgs e)
        {
            var sim = this.FindControl<SimulationControl>("SimControl");
            if (sim != null) sim.HandleInput(e.Key.ToString(), false);
        }
    }
}