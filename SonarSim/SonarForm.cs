using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace SonarSim
{
    // Custom Panel for Double Buffering to prevent flicker
    public class BufferedPanel : Panel
    {
        public BufferedPanel()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }
    }

    public class SonarForm : Form
    {
        private System.Windows.Forms.Timer _simulationTimer;
        private List<SonarEntity> _entities;
        private float _sweepAngle = 0;
        private float _sweepSpeed = 2.0f;
        private float _simulationSpeed = 1.0f;
        private int _perimeterRadius = 150;
        private bool _alarmTriggered = false;
        private Random _rnd = new Random();

        // UI Components
        private BufferedPanel _sonarPanel;
        private DataGridView _entityGrid;
        private TrackBar _speedTrackBar;
        private TrackBar _perimeterTrackBar;
        private Button _resetButton;
        private Label _lblSpeed;
        private Label _lblPerimeter;
        private Label _lblAlarm;

        public SonarForm()
        {
            this.Text = "SonarSim - Advanced Sonar Simulator";
            this.Size = new Size(1000, 700);
            this.DoubleBuffered = true;
            this.BackColor = Color.FromArgb(10, 10, 15); // Dark Theme

            // Init containers
            _entities = new List<SonarEntity>();
            
            InitializeComponents();
            ResetSimulation();

            _simulationTimer = new System.Windows.Forms.Timer();
            _simulationTimer.Interval = 20; // 50 FPS
            _simulationTimer.Tick += SimulationLoop;
            _simulationTimer.Start();
        }

        private void InitializeComponents()
        {
            // Panel Sonar (Kiri) using custom BufferedPanel
            _sonarPanel = new BufferedPanel();
            _sonarPanel.Size = new Size(600, 600);
            _sonarPanel.Location = new Point(20, 20);
            _sonarPanel.BackColor = Color.Black;
            _sonarPanel.Paint += SonarPanel_Paint;
            this.Controls.Add(_sonarPanel);

            // Controls Panel (Kanan)
            int controlX = 640;
            int controlWidth = 320;

            // Judul
            Label title = new Label() { Text = "SONAR CONTROL", ForeColor = Color.LimeGreen, Font = new Font("Consolas", 14, FontStyle.Bold), Location = new Point(controlX, 20), AutoSize = true };
            this.Controls.Add(title);

            // Speed Control
            _lblSpeed = new Label() { Text = "Sim Speed: 1.0x", ForeColor = Color.White, Location = new Point(controlX, 60), AutoSize = true };
            this.Controls.Add(_lblSpeed);
            _speedTrackBar = new TrackBar() { Minimum = 1, Maximum = 50, Value = 10, Location = new Point(controlX, 80), Size = new Size(controlWidth, 45), TickFrequency = 5 };
            _speedTrackBar.Scroll += (object? s, EventArgs e) => { _simulationSpeed = _speedTrackBar.Value / 10.0f; _lblSpeed.Text = $"Sim Speed: {_simulationSpeed:0.0}x"; };
            this.Controls.Add(_speedTrackBar);

            // Perimeter Control
            _lblPerimeter = new Label() { Text = "Perimeter: 150m", ForeColor = Color.White, Location = new Point(controlX, 130), AutoSize = true };
            this.Controls.Add(_lblPerimeter);
            _perimeterTrackBar = new TrackBar() { Minimum = 50, Maximum = 280, Value = 150, Location = new Point(controlX, 150), Size = new Size(controlWidth, 45), TickFrequency = 10 };
            _perimeterTrackBar.Scroll += (object? s, EventArgs e) => { _perimeterRadius = _perimeterTrackBar.Value; _lblPerimeter.Text = $"Perimeter: {_perimeterRadius}m"; _sonarPanel.Invalidate(); };
            this.Controls.Add(_perimeterTrackBar);

            // Alarm Status
            _lblAlarm = new Label() { Text = "STATUS: SAFE", ForeColor = Color.Lime, Font = new Font("Arial", 12, FontStyle.Bold), Location = new Point(controlX, 200), AutoSize = true, BackColor = Color.DarkGreen, Padding = new Padding(5) };
            this.Controls.Add(_lblAlarm);

            // Reset Button
            _resetButton = new Button() { Text = "RESET SIMULATION", Location = new Point(controlX + 150, 200), Size = new Size(150, 30), BackColor = Color.DarkRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            _resetButton.Click += (object? s, EventArgs e) => ResetSimulation();
            this.Controls.Add(_resetButton);

            // Data Grid View
            _entityGrid = new DataGridView();
            _entityGrid.Location = new Point(controlX, 250);
            _entityGrid.Size = new Size(controlWidth, 380);
            _entityGrid.BackgroundColor = Color.FromArgb(20, 20, 25);
            _entityGrid.ForeColor = Color.Black;
            _entityGrid.ReadOnly = true;
            _entityGrid.AllowUserToAddRows = false;
            _entityGrid.RowHeadersVisible = false;
            _entityGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _entityGrid.Columns.Add("ID", "ID");
            _entityGrid.Columns.Add("Type", "Type");
            _entityGrid.Columns.Add("Spd", "Spd");
            _entityGrid.Columns.Add("Dir", "Dir");
            _entityGrid.Columns.Add("Stat", "Status");
            
            // Grid Styles
            _entityGrid.DefaultCellStyle.BackColor = Color.Black;
            _entityGrid.DefaultCellStyle.ForeColor = Color.LimeGreen;
            _entityGrid.DefaultCellStyle.Font = new Font("Consolas", 9);
            _entityGrid.GridColor = Color.DarkGreen;

            this.Controls.Add(_entityGrid);
        }

        private void ResetSimulation()
        {
            _entities.Clear();
            _sweepAngle = 0;
            _alarmTriggered = false;
            UpdateAlarmUI(false);

            // Create Random Static Objects (Rocks)
            for (int i = 0; i < 5; i++)
            {
                SpawnObject(EntityType.Rock);
            }

            // Create Dynamic Objects
            for (int i = 0; i < 3; i++) // Start with 3 moving objects
            {
                SpawnRandomDynamicObject();
            }
        }

        private void SpawnObject(EntityType type)
        {
            PointF pos = GetRandomPosition();
            float speed = 0;
            float heading = 0;

            if (type != EntityType.Rock)
            {
                speed = (float)(_rnd.NextDouble() * 2.0 + 0.5); // Random speed 0.5 - 2.5
                heading = _rnd.Next(0, 360);
            }

            if (type == EntityType.Torpedo) speed = 4.0f; // Faster

            var entity = new SonarEntity(type, pos, speed, heading);
            _entities.Add(entity);
        }

        private void SpawnRandomDynamicObject()
        {
            EntityType[] types = { EntityType.Diver, EntityType.Fish, EntityType.Ship, EntityType.Torpedo };
            EntityType type = types[_rnd.Next(types.Length)];
            SpawnObject(type);
        }

        private PointF GetRandomPosition()
        {
            // Random position inside circle radius 280 (Panel is 600x600, center 300,300)
            double angle = _rnd.NextDouble() * Math.PI * 2;
            double radius = _rnd.Next(50, 280);
            float x = (float)(Math.Cos(angle) * radius);
            float y = (float)(Math.Sin(angle) * radius);
            return new PointF(x, y); // Relative to center (0,0) logic
        }

        private void SimulationLoop(object? sender, EventArgs e)
        {
            // 1. Update Objects
            Rectangle boundary = new Rectangle(-300, -300, 600, 600); // Logic boundary centered at 0,0

            bool alarmState = false;

            // Randomly spawn new object occasionally
            if (_rnd.Next(0, 500) == 0 && _entities.Count < 15)
            {
                SpawnRandomDynamicObject();
            }

            List<SonarEntity> detectedList = new List<SonarEntity>();

            foreach (var entity in _entities)
            {
                entity.Move(boundary, _simulationSpeed);
                entity.UpdateDistance(new PointF(0, 0));

                // Check Alarm Logic
                if (entity.Type != EntityType.Rock && entity.DistanceFromCenter < _perimeterRadius)
                {
                    alarmState = true;
                }

                // Update Detection Logic based on Sweep
                // Angle of object relative to center
                double angleRad = Math.Atan2(entity.Position.Y, entity.Position.X);
                double angleDeg = angleRad * (180 / Math.PI);
                if (angleDeg < 0) angleDeg += 360;

                // Check if sweep line hits object (with some tolerance)
                float angleDiff = Math.Abs((float)angleDeg - _sweepAngle);
                if (angleDiff > 180) angleDiff = 360 - angleDiff; // Handle wrap around 0/360

                if (angleDiff < 5.0f) // Detection Threshold
                {
                    if (!entity.IsDetected) // Just detected
                    {
                        // Play sound? (Optional, skip for simplicity)
                    }
                    entity.LastDetected = DateTime.Now;
                    entity.IsDetected = true;
                    entity.AddTrailPoint();
                }

                // If signal is old, it fades -> handled in Paint, but logic stays "Detected" for a while
                if ((DateTime.Now - entity.LastDetected).TotalSeconds > 4.0)
                {
                    entity.IsDetected = false;
                }

                if (entity.IsDetected)
                {
                    detectedList.Add(entity);
                }
            }

            // Update Alarm UI
            UpdateAlarmUI(alarmState);

            // Update Grid (Throttle update to prevent lag)
            UpdateGrid(detectedList);

            // Update Sweep Angle
            _sweepAngle += _sweepSpeed * _simulationSpeed;
            if (_sweepAngle >= 360) _sweepAngle -= 360;

            // Redraw
            _sonarPanel.Invalidate();
        }

        private void UpdateAlarmUI(bool active)
        {
            if (active != _alarmTriggered)
            {
                _alarmTriggered = active;
                if (active)
                {
                    _lblAlarm.Text = "WARNING: INTRUDER!";
                    _lblAlarm.ForeColor = Color.White;
                    _lblAlarm.BackColor = Color.Red;
                }
                else
                {
                    _lblAlarm.Text = "STATUS: SAFE";
                    _lblAlarm.ForeColor = Color.Lime;
                    _lblAlarm.BackColor = Color.DarkGreen;
                }
            }
        }

        private void UpdateGrid(List<SonarEntity> detected)
        {
            // Only update every 10 ticks or so if list is large, but here it's small.
            // Full refresh is simplest but can be slow. Let's do smart update?
            // For simplicity, clear and add is easiest but flickers.
            // Let's try to update existing rows or add new ones.
            
            // To prevent flickering and complexity, let's just refresh every 10 frames or 500ms
            if (DateTime.Now.Millisecond % 500 < 50) 
            {
                 // Simple full refresh for robustness
                _entityGrid.Rows.Clear();
                foreach (var d in detected)
                {
                    string statusStr = d.GetStatus().ToString();
                    _entityGrid.Rows.Add(d.Id.ToString().Substring(0, 4), d.Type.ToString(), d.Speed.ToString("0.0"), d.Heading.ToString("0"), statusStr);
                }
            }
        }

        private void SonarPanel_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            int w = _sonarPanel.Width;
            int h = _sonarPanel.Height;
            int cx = w / 2;
            int cy = h / 2;

            // Draw Background Grid (Circles)
            Pen gridPen = new Pen(Color.FromArgb(50, 0, 255, 0), 1);
            for (int r = 50; r < 300; r += 50)
            {
                g.DrawEllipse(gridPen, cx - r, cy - r, r * 2, r * 2);
            }

            // Draw Crosshair
            g.DrawLine(gridPen, cx, 0, cx, h);
            g.DrawLine(gridPen, 0, cy, w, cy);

            // Draw Perimeter
            Pen perimPen = new Pen(_alarmTriggered ? Color.Red : Color.FromArgb(100, 255, 100, 0), 2);
            if (_alarmTriggered) perimPen.DashStyle = DashStyle.Dash;
            g.DrawEllipse(perimPen, cx - _perimeterRadius, cy - _perimeterRadius, _perimeterRadius * 2, _perimeterRadius * 2);

            // Draw Sweep Line
            double rad = _sweepAngle * Math.PI / 180.0;
            float endX = cx + (float)(Math.Cos(rad) * 300);
            float endY = cy + (float)(Math.Sin(rad) * 300);
            
            // Gradient for sweep trail effect
            // Simulating a gradient sweep is hard with just lines, but we can draw a fan?
            // Let's just draw a line for now and maybe a small pie sector
            Brush sweepBrush = new LinearGradientBrush(new Point(cx, cy), new Point((int)endX, (int)endY), Color.FromArgb(200, 0, 255, 0), Color.Transparent);
            Pen sweepPen = new Pen(Color.Lime, 2);
            g.DrawLine(sweepPen, cx, cy, endX, endY);

            // Draw Objects
            foreach (var entity in _entities)
            {
                // Calculate opacity based on time since detection
                double timeSince = (DateTime.Now - entity.LastDetected).TotalSeconds;
                if (timeSince > 4.0) continue; // Too old, don't draw (invisible)

                int alpha = (int)(255 * (1.0 - (timeSince / 4.0)));
                if (alpha < 0) alpha = 0;
                
                // Position relative to center
                float drawX = cx + entity.Position.X;
                float drawY = cy + entity.Position.Y;

                // Color based on type
                Color objColor = Color.Lime;
                if (entity.Type == EntityType.Rock) objColor = Color.Gray;
                else if (entity.Type == EntityType.Torpedo) objColor = Color.Orange;
                else if (entity.Type == EntityType.Ship) objColor = Color.Yellow;

                Brush b = new SolidBrush(Color.FromArgb(alpha, objColor));
                
                // Draw Blip
                g.FillEllipse(b, drawX - 4, drawY - 4, 8, 8);

                // Draw Detection Box and Text if fresh detection
                if (timeSince < 2.0)
                {
                    Pen detectPen = new Pen(Color.FromArgb(alpha, objColor));
                    g.DrawRectangle(detectPen, drawX - 10, drawY - 10, 20, 20);
                    g.DrawString(entity.Type.ToString(), new Font("Arial", 8), b, drawX + 12, drawY - 10);
                }

                // Draw Trail
                if (entity.Trail.Count > 1)
                {
                    PointF[] trailPoints = entity.Trail.Select(p => new PointF(cx + p.X, cy + p.Y)).ToArray();
                    if (trailPoints.Length > 1)
                    {
                        Pen trailPen = new Pen(Color.FromArgb((int)(alpha * 0.5), objColor), 1);
                        g.DrawCurve(trailPen, trailPoints);
                    }
                }
            }
        }
    }
}