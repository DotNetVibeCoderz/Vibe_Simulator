using System.Numerics;

namespace BouncingBalls;

public partial class Form1 : Form
{
    // ========= CONFIG & STATE =========
    private readonly List<Ball> _balls = new();
    private readonly Random _rand = new();

    private int _sides = 6; // default hexagon
    private float _rotationSpeedDeg = 20f; // degrees per second
    private float _rotationAngleRad = 0f;

    private readonly System.Windows.Forms.Timer _timer;
    private readonly System.Diagnostics.Stopwatch _stopwatch = new();

    // ========= UI CONTROLS =========
    private FlowLayoutPanel topBar = null!;
    private DrawingPanel canvas = null!;
    private Button btnAdd = null!;
    private Button btnRemove = null!;
    private NumericUpDown nudBalls = null!;
    private NumericUpDown nudSides = null!;
    private NumericUpDown nudRotation = null!;
    private Label lblBalls = null!;
    private Label lblSides = null!;
    private Label lblRotation = null!;

    public Form1()
    {
        InitializeComponent();
        BuildUi();

        // Start with some balls
        SetBallCount(8);

        _stopwatch.Start();
        _timer = new System.Windows.Forms.Timer
        {
            Interval = 16 // ~60 fps
        };
        _timer.Tick += (_, _) => TickFrame();
        _timer.Start();
    }

    // ========= UI =========
    private void BuildUi()
    {
        Text = "Bouncing Balls in Rotating Polygon";
        Width = 1000;
        Height = 700;

        // Top control bar
        topBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 60,
            Padding = new Padding(10, 10, 10, 10),
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };

        lblBalls = new Label { Text = "Balls:", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0, 6, 6, 0) };
        nudBalls = new NumericUpDown { Minimum = 0, Maximum = 100, Value = 8, Width = 60 };
        btnAdd = new Button { Text = "+", Width = 32, Height = 26, Margin = new Padding(6, 2, 6, 2) };
        btnRemove = new Button { Text = "-", Width = 32, Height = 26, Margin = new Padding(0, 2, 12, 2) };

        lblSides = new Label { Text = "Polygon Sides:", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(0, 6, 6, 0) };
        nudSides = new NumericUpDown { Minimum = 3, Maximum = 10, Value = 6, Width = 60 };

        lblRotation = new Label { Text = "Rotation Speed (deg/s):", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Margin = new Padding(12, 6, 6, 0) };
        nudRotation = new NumericUpDown { Minimum = -360, Maximum = 360, Value = 20, Width = 70, DecimalPlaces = 0, Increment = 5 };

        topBar.Controls.AddRange(new Control[]
        {
            lblBalls, nudBalls, btnAdd, btnRemove,
            lblSides, nudSides,
            lblRotation, nudRotation
        });

        // Canvas
        canvas = new DrawingPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(20, 20, 24)
        };
        canvas.Paint += (_, e) => DrawScene(e.Graphics);

        Controls.Add(canvas);
        Controls.Add(topBar);

        // Events
        btnAdd.Click += (_, _) => { nudBalls.Value = Math.Min(nudBalls.Maximum, nudBalls.Value + 1); };
        btnRemove.Click += (_, _) => { nudBalls.Value = Math.Max(nudBalls.Minimum, nudBalls.Value - 1); };
        nudBalls.ValueChanged += (_, _) => SetBallCount((int)nudBalls.Value);
        nudSides.ValueChanged += (_, _) => _sides = (int)nudSides.Value;
        nudRotation.ValueChanged += (_, _) => _rotationSpeedDeg = (float)nudRotation.Value;
    }

    // ========= MAIN LOOP =========
    private void TickFrame()
    {
        float dt = (float)_stopwatch.Elapsed.TotalSeconds;
        _stopwatch.Restart();

        if (dt <= 0f) return;

        // Update rotation
        _rotationAngleRad += MathF.PI / 180f * _rotationSpeedDeg * dt;

        // Update balls
        UpdateBalls(dt);

        // Redraw
        canvas.Invalidate();
    }

    // ========= PHYSICS =========
    private void UpdateBalls(float dt)
    {
        var bounds = canvas.ClientRectangle;
        if (bounds.Width <= 0 || bounds.Height <= 0) return;

        float radius = MathF.Min(bounds.Width, bounds.Height) * 0.40f; // polygon radius
        Vector2 center = new(bounds.Width / 2f, bounds.Height / 2f);

        // Move
        foreach (var b in _balls)
        {
            b.Position += b.Velocity * dt;
        }

        // Ball-ball collisions (simple elastic, equal mass)
        for (int i = 0; i < _balls.Count; i++)
        {
            for (int j = i + 1; j < _balls.Count; j++)
            {
                ResolveBallCollision(_balls[i], _balls[j]);
            }
        }

        // Polygon collisions
        var poly = GetPolygonVertices(center, radius, _sides, _rotationAngleRad);
        for (int i = 0; i < _balls.Count; i++)
        {
            ResolvePolygonCollision(_balls[i], poly);
        }
    }

    private void ResolveBallCollision(Ball a, Ball b)
    {
        Vector2 delta = b.Position - a.Position;
        float dist = delta.Length();
        float minDist = a.Radius + b.Radius;
        if (dist <= 0.0001f || dist >= minDist) return;

        Vector2 n = delta / dist;
        float overlap = minDist - dist;

        // Separate
        a.Position -= n * (overlap / 2f);
        b.Position += n * (overlap / 2f);

        // Elastic impulse
        float vRel = Vector2.Dot(a.Velocity - b.Velocity, n);
        if (vRel > 0) return; // already separating

        float p = vRel; // equal mass
        a.Velocity -= p * n;
        b.Velocity += p * n;
    }

    private void ResolvePolygonCollision(Ball ball, List<Vector2> poly)
    {
        int count = poly.Count;
        for (int i = 0; i < count; i++)
        {
            Vector2 v1 = poly[i];
            Vector2 v2 = poly[(i + 1) % count];
            Vector2 edge = v2 - v1;

            // Inward normal for CCW polygon
            Vector2 n = new(-edge.Y, edge.X);
            n = Vector2.Normalize(n);

            float dist = Vector2.Dot(ball.Position - v1, n);
            if (dist < ball.Radius)
            {
                float penetration = ball.Radius - dist;
                ball.Position += n * penetration;

                // Reflect velocity
                float vn = Vector2.Dot(ball.Velocity, n);
                ball.Velocity -= 2f * vn * n;
            }
        }
    }

    // ========= DRAWING =========
    private void DrawScene(Graphics g)
    {
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var bounds = canvas.ClientRectangle;
        if (bounds.Width <= 0 || bounds.Height <= 0) return;

        float radius = MathF.Min(bounds.Width, bounds.Height) * 0.40f; // polygon radius
        Vector2 center = new(bounds.Width / 2f, bounds.Height / 2f);

        // Draw polygon
        var poly = GetPolygonVertices(center, radius, _sides, _rotationAngleRad);
        using var pen = new Pen(Color.FromArgb(200, 230, 230, 255), 2f);
        g.DrawPolygon(pen, poly.Select(v => new PointF(v.X, v.Y)).ToArray());

        // Draw balls
        foreach (var b in _balls)
        {
            using var brush = new SolidBrush(b.Color);
            g.FillEllipse(brush, b.Position.X - b.Radius, b.Position.Y - b.Radius, b.Radius * 2, b.Radius * 2);

            // small highlight
            using var highlight = new SolidBrush(Color.FromArgb(120, 255, 255, 255));
            g.FillEllipse(highlight, b.Position.X - b.Radius * 0.4f, b.Position.Y - b.Radius * 0.4f, b.Radius * 0.5f, b.Radius * 0.5f);
        }
    }

    // ========= HELPERS =========
    private List<Vector2> GetPolygonVertices(Vector2 center, float radius, int sides, float rotationRad)
    {
        var list = new List<Vector2>(sides);
        for (int i = 0; i < sides; i++)
        {
            float angle = rotationRad + (MathF.PI * 2f) * i / sides;
            float x = center.X + MathF.Cos(angle) * radius;
            float y = center.Y + MathF.Sin(angle) * radius;
            list.Add(new Vector2(x, y));
        }
        return list;
    }

    private void SetBallCount(int count)
    {
        if (count < 0) count = 0;

        while (_balls.Count < count)
        {
            _balls.Add(CreateRandomBall());
        }
        while (_balls.Count > count)
        {
            _balls.RemoveAt(_balls.Count - 1);
        }
    }

    private Ball CreateRandomBall()
    {
        float radius = _rand.Next(8, 18);
        Vector2 pos = RandomPointInsidePolygon(_sides, radius + 2f);

        // random velocity
        float speed = _rand.Next(60, 150);
        float ang = (float)(_rand.NextDouble() * Math.PI * 2.0);
        Vector2 vel = new(MathF.Cos(ang) * speed, MathF.Sin(ang) * speed);

        Color color = Color.FromArgb(220,
            _rand.Next(50, 255),
            _rand.Next(50, 255),
            _rand.Next(50, 255));

        return new Ball { Position = pos, Velocity = vel, Radius = radius, Color = color };
    }

    private Vector2 RandomPointInsidePolygon(int sides, float margin)
    {
        // Rejection sampling inside polygon bounding circle
        var bounds = canvas.ClientRectangle;
        float radius = MathF.Min(bounds.Width, bounds.Height) * 0.40f;
        Vector2 center = new(bounds.Width / 2f, bounds.Height / 2f);

        var poly = GetPolygonVertices(center, radius, sides, _rotationAngleRad);

        for (int attempt = 0; attempt < 1000; attempt++)
        {
            float x = (float)(_rand.NextDouble() * 2 - 1) * (radius - margin) + center.X;
            float y = (float)(_rand.NextDouble() * 2 - 1) * (radius - margin) + center.Y;
            Vector2 p = new(x, y);

            if (PointInsidePolygon(p, poly, margin))
                return p;
        }

        // fallback: center
        return center;
    }

    private bool PointInsidePolygon(Vector2 p, List<Vector2> poly, float margin)
    {
        int count = poly.Count;
        for (int i = 0; i < count; i++)
        {
            Vector2 v1 = poly[i];
            Vector2 v2 = poly[(i + 1) % count];
            Vector2 edge = v2 - v1;
            Vector2 n = Vector2.Normalize(new Vector2(-edge.Y, edge.X));
            float dist = Vector2.Dot(p - v1, n);
            if (dist < margin) return false; // outside or too close
        }
        return true;
    }

    // ========= NESTED TYPES =========
    private class Ball
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Radius;
        public Color Color;
    }

    private class DrawingPanel : Panel
    {
        public DrawingPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }
    }
}
