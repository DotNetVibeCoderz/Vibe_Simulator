using Avalonia;
using Avalonia.Media;
using System;

namespace PhysicsSimulator.Simulations
{
    public class WaveSimulation : SimulationBase
    {
        public override string Name => "Wave Interference";
        public override string Description => "Ripple Tank. Click/Drag to create waves.";

        private int _cols = 80;
        private int _rows = 60;
        private double[,] _current;
        private double[,] _previous;
        private double _damping = 0.96;
        private double _cellWidth;
        private double _cellHeight;

        public WaveSimulation()
        {
            _current = new double[_cols, _rows];
            _previous = new double[_cols, _rows];
        }

        public override void Reset()
        {
             _current = new double[_cols, _rows];
            _previous = new double[_cols, _rows];
        }

        public override void OnPointerPressed(Avalonia.Input.PointerPoint point, Size bounds)
        {
            Disturb(point, bounds);
        }

        public override void OnPointerMoved(Avalonia.Input.PointerPoint point, Size bounds)
        {
             if (point.Properties.IsLeftButtonPressed)
                Disturb(point, bounds);
        }

        private void Disturb(Avalonia.Input.PointerPoint point, Size bounds)
        {
             int x = (int)(point.Position.X / _cellWidth);
             int y = (int)(point.Position.Y / _cellHeight);

             if (x > 1 && x < _cols - 2 && y > 1 && y < _rows - 2)
             {
                 _current[x, y] = 500;
             }
        }

        public override void Update(double deltaTime, Size bounds)
        {
             _cellWidth = bounds.Width / _cols;
            _cellHeight = bounds.Height / _rows;

            for (int i = 1; i < _cols - 1; i++)
            {
                for (int j = 1; j < _rows - 1; j++)
                {
                    _current[i, j] = (_previous[i - 1, j] +
                                      _previous[i + 1, j] +
                                      _previous[i, j - 1] +
                                      _previous[i, j + 1]) / 2 -
                                     _current[i, j];
                    _current[i, j] *= _damping;
                }
            }

            // Swap buffers
            var temp = _previous;
            _previous = _current;
            _current = temp;
        }

        public override void Draw(DrawingContext context, Size bounds)
        {
            // Drawing thousands of rectangles is heavy in Avalonia directly.
            // Ideally we use WriteableBitmap, but for simplicity of code generation without complex setup:
            // We draw rectangles.
            
            for (int i = 0; i < _cols; i++)
            {
                for (int j = 0; j < _rows; j++)
                {
                    double val = _current[i, j]; // Use _previous since we swapped? Actually they are swapped every frame.
                    // Clamp
                    if (val > 255) val = 255;
                    if (val < -255) val = -255;
                    
                    int colorVal = (int)Math.Abs(val);
                    if (colorVal < 10) continue; // Optimization: Don't draw black/empty

                    byte b = (byte)colorVal;
                    // Blueish tint for waves
                    var brush = new SolidColorBrush(Color.FromArgb(255, b, b, 255));
                    
                    context.FillRectangle(brush, new Rect(i * _cellWidth, j * _cellHeight, _cellWidth, _cellHeight));
                }
            }
        }
    }
}