using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace FireworkSim
{
    public partial class Form1 : Form
    {
        private List<Firework> fireworks = new List<Firework>();
        private Random rnd = new Random();
        
        // Settings
        private float gravity = 0.1f;
        private float explosionForce = 5.0f;
        private int particleCount = 50;
        private float particleSize = 3.0f;
        private bool autoFire = false;
        private int autoFireTimer = 0;
        private int autoFireInterval = 20;

        // UI Controls
        private Panel controlPanel;
        private CheckBox chkAutoFire;
        private TrackBar trkGravity;
        private TrackBar trkExplosion;
        private TrackBar trkParticleCount;
        private Button btnClear;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Initialize Control Panel
            controlPanel = new Panel();
            controlPanel.Dock = DockStyle.Bottom;
            controlPanel.Height = 100;
            controlPanel.BackColor = Color.FromArgb(40, 40, 40);
            this.Controls.Add(controlPanel);

            // Auto Fire Checkbox
            chkAutoFire = new CheckBox();
            chkAutoFire.Text = "Auto Fire";
            chkAutoFire.ForeColor = Color.White;
            chkAutoFire.Location = new Point(20, 20);
            chkAutoFire.CheckedChanged += (s, ev) => { autoFire = chkAutoFire.Checked; };
            controlPanel.Controls.Add(chkAutoFire);

            // Gravity Slider
            Label lblGravity = new Label();
            lblGravity.Text = "Gravity";
            lblGravity.ForeColor = Color.White;
            lblGravity.Location = new Point(120, 10);
            lblGravity.AutoSize = true;
            controlPanel.Controls.Add(lblGravity);

            trkGravity = new TrackBar();
            trkGravity.Minimum = 1;
            trkGravity.Maximum = 50;
            trkGravity.Value = 10;
            trkGravity.TickStyle = TickStyle.None;
            trkGravity.Location = new Point(120, 30);
            trkGravity.Width = 150;
            trkGravity.ValueChanged += (s, ev) => { gravity = trkGravity.Value / 100.0f; };
            controlPanel.Controls.Add(trkGravity);

            // Explosion Force Slider
            Label lblExplosion = new Label();
            lblExplosion.Text = "Explosion Force";
            lblExplosion.ForeColor = Color.White;
            lblExplosion.Location = new Point(280, 10);
            lblExplosion.AutoSize = true;
            controlPanel.Controls.Add(lblExplosion);

            trkExplosion = new TrackBar();
            trkExplosion.Minimum = 1;
            trkExplosion.Maximum = 20;
            trkExplosion.Value = 5;
            trkExplosion.TickStyle = TickStyle.None;
            trkExplosion.Location = new Point(280, 30);
            trkExplosion.Width = 150;
            trkExplosion.ValueChanged += (s, ev) => { explosionForce = trkExplosion.Value; };
            controlPanel.Controls.Add(trkExplosion);

            // Particle Count Slider
            Label lblCount = new Label();
            lblCount.Text = "Particles";
            lblCount.ForeColor = Color.White;
            lblCount.Location = new Point(440, 10);
            lblCount.AutoSize = true;
            controlPanel.Controls.Add(lblCount);

            trkParticleCount = new TrackBar();
            trkParticleCount.Minimum = 10;
            trkParticleCount.Maximum = 300;
            trkParticleCount.Value = 50;
            trkParticleCount.TickStyle = TickStyle.None;
            trkParticleCount.Location = new Point(440, 30);
            trkParticleCount.Width = 150;
            trkParticleCount.ValueChanged += (s, ev) => { particleCount = trkParticleCount.Value; };
            controlPanel.Controls.Add(trkParticleCount);

            // Clear Button
            btnClear = new Button();
            btnClear.Text = "Clear";
            btnClear.Location = new Point(620, 30);
            btnClear.Click += (s, ev) => { fireworks.Clear(); };
            controlPanel.Controls.Add(btnClear);

            // Instructions
            Label lblInfo = new Label();
            lblInfo.Text = "Click anywhere to launch!";
            lblInfo.ForeColor = Color.Yellow;
            lblInfo.AutoSize = true;
            lblInfo.Font = new Font("Arial", 10, FontStyle.Bold);
            lblInfo.Location = new Point(720, 35);
            controlPanel.Controls.Add(lblInfo);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // Auto Fire Logic
            if (autoFire)
            {
                autoFireTimer++;
                if (autoFireTimer >= autoFireInterval)
                {
                    autoFireTimer = 0;
                    LaunchFirework(rnd.Next(50, this.ClientSize.Width - 50), this.ClientSize.Height);
                    autoFireInterval = rnd.Next(5, 30);
                }
            }

            // Update Fireworks
            for (int i = fireworks.Count - 1; i >= 0; i--)
            {
                fireworks[i].Update(gravity, explosionForce, particleCount, particleSize);
                if (fireworks[i].IsDead())
                {
                    fireworks.RemoveAt(i);
                }
            }

            this.Invalidate();
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            // Launch from bottom, targeting the mouse Y roughly, but currently firework logic is purely physics based.
            // Let's spawn it at bottom X aligned with mouse, and give it speed based on height
            
            float startX = e.X;
            float startY = this.ClientSize.Height - controlPanel.Height;
            
            // Calculate necessary initial velocity to reach height Y
            // v^2 = u^2 + 2as. v=0 at top. 
            // u = sqrt(-2 * gravity * displacement)
            // displacement = (MouseY - StartY) -> Negative value
            
            // To make it fun, let's just launch it.
            LaunchFirework(startX, startY);
        }

        private void LaunchFirework(float x, float y)
        {
            Color c = ColorHelper.FromHsl(rnd.NextDouble(), 1.0, 0.5);
            Firework fw = new Firework(x, y, 0, c);
            
            // Override the default random velocity in constructor to something slightly more controlled if needed
            // But the constructor logic is fine for random bursts.
            
            fireworks.Add(fw);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            foreach (var fw in fireworks)
            {
                fw.Draw(g);
            }
        }
    }
}