using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ProjectileSim.ViewModels;
using System;

namespace ProjectileSim.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel _vm;
        private Polyline _trajectoryLine;
        private Ellipse _projectileShape;
        private Canvas _simCanvas;
        private double _scale = 4.0; // pixels per meter

        public MainWindow()
        {
            InitializeComponent();
            _vm = new MainViewModel();
            DataContext = _vm;
            
            _simCanvas = this.FindControl<Canvas>("SimCanvas");
            _trajectoryLine = this.FindControl<Polyline>("TrajectoryLine");
            _projectileShape = this.FindControl<Ellipse>("ProjectileShape");

            if (_vm != null)
            {
                _vm.SimulationUpdated += () => Dispatcher.UIThread.InvokeAsync(UpdateVisualization);
                
                _vm.PropertyChanged += (s, e) => 
                {
                    if (e.PropertyName == nameof(MainViewModel.CurrentTime) && _vm.CurrentTime == 0)
                    {
                         Dispatcher.UIThread.InvokeAsync(() => 
                         {
                             if (_trajectoryLine != null) _trajectoryLine.Points.Clear();
                             UpdateVisualization();
                         });
                    }
                };
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void UpdateVisualization()
        {
            if (_simCanvas == null || _projectileShape == null || _trajectoryLine == null) return;
            
            // Get Canvas Height
            double canvasHeight = _simCanvas.Bounds.Height;
            if (canvasHeight <= 0) canvasHeight = 500; // default fallback if not rendered yet

            double x = _vm.CurrentPositionX * _scale;
            double y = canvasHeight - (_vm.CurrentPositionY * _scale) - 20; // 20 ground offset

            // Center the ball
            Canvas.SetLeft(_projectileShape, x - _projectileShape.Width / 2);
            Canvas.SetTop(_projectileShape, y - _projectileShape.Height / 2);

            // Add to trajectory
            _trajectoryLine.Points.Add(new Point(x, y));
        }
    }
}