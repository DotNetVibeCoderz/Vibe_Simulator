using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RagdollSim
{
    public partial class Form1 : Form
    {
        private List<Point> points = new List<Point>();
        private List<Stick> sticks = new List<Stick>();
        private System.Windows.Forms.Timer gameTimer;
        
        // Physics constants
        private float gravity = 0.5f;
        private float friction = 0.999f;
        
        // Interaction
        private Point draggedPoint = null;
        private bool isDragging = false;

        public Form1()
        {
            InitializeComponent();
            
            this.Text = "Ragdoll Simulation - Jacky The Code Bender";
            this.Size = new Size(800, 600);
            this.DoubleBuffered = true; // Prevent flickering
            this.BackColor = Color.FromArgb(30, 30, 30);

            InitializeRagdoll(400, 100);

            // Setup game loop
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 16; // ~60 FPS
            gameTimer.Tick += GameUpdate;
            gameTimer.Start();

            // Event handlers
            this.MouseDown += Form1_MouseDown;
            this.MouseUp += Form1_MouseUp;
            this.MouseMove += Form1_MouseMove;
        }

        private void InitializeRagdoll(float startX, float startY)
        {
            // Create Points (Head, Body, Limbs)
            Point head = new Point(startX, startY);
            Point neck = new Point(startX, startY + 50);
            Point torso = new Point(startX, startY + 150);
            
            Point lElbow = new Point(startX - 30, startY + 60);
            Point lHand = new Point(startX - 60, startY + 70);
            
            Point rElbow = new Point(startX + 30, startY + 60);
            Point rHand = new Point(startX + 60, startY + 70);
            
            Point pelvis = new Point(startX, startY + 200);
            
            Point lKnee = new Point(startX - 20, startY + 250);
            Point lFoot = new Point(startX - 20, startY + 300);
            
            Point rKnee = new Point(startX + 20, startY + 250);
            Point rFoot = new Point(startX + 20, startY + 300);

            points.AddRange(new[] { head, neck, torso, lElbow, lHand, rElbow, rHand, pelvis, lKnee, lFoot, rKnee, rFoot });

            // Create Sticks (Connections)
            AddStick(head, neck);
            AddStick(neck, torso);
            AddStick(torso, pelvis);
            
            // Left Arm
            AddStick(neck, lElbow);
            AddStick(lElbow, lHand);
            
            // Right Arm
            AddStick(neck, rElbow);
            AddStick(rElbow, rHand);
            
            // Left Leg
            AddStick(pelvis, lKnee);
            AddStick(lKnee, lFoot);
            
            // Right Leg
            AddStick(pelvis, rKnee);
            AddStick(rKnee, rFoot);
            
            // Structural stability (hidden sticks to keep shape better)
            AddStick(lElbow, torso);
            AddStick(rElbow, torso);
            AddStick(head, torso); 
        }

        private void AddStick(Point p0, Point p1)
        {
            sticks.Add(new Stick(p0, p1));
        }

        private void GameUpdate(object sender, EventArgs e)
        {
            UpdatePhysics();
            this.Invalidate(); // Trigger redraw
        }

        private void UpdatePhysics()
        {
            // precise interaction while physics runs
            if (isDragging && draggedPoint != null)
            {
               draggedPoint.SetPosition(this.PointToClient(Cursor.Position).X, this.PointToClient(Cursor.Position).Y);
            }

            // Update points
            foreach (var p in points)
            {
                p.Update(friction, gravity, this.ClientSize.Width, this.ClientSize.Height);
            }

            // Update sticks (Constraints) - multiple iterations for stability
            for (int i = 0; i < 5; i++)
            {
                foreach (var s in sticks)
                {
                    s.Update();
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Pen stickPen = new Pen(Color.WhiteSmoke, 4);
            Brush pointBrush = Brushes.Cyan;
            Brush headBrush = Brushes.Yellow;

            // Draw sticks
            foreach (var s in sticks)
            {
                s.Draw(g, stickPen);
            }

            // Draw points
            foreach (var p in points)
            {
                // Draw head differently
                if(points.IndexOf(p) == 0) 
                    g.FillEllipse(headBrush, p.X - 10, p.Y - 10, 20, 20);
                else
                    g.FillEllipse(pointBrush, p.X - 5, p.Y - 5, 10, 10);
            }
            
            // Instructions
            g.DrawString("Drag points to interact with the Ragdoll!", this.Font, Brushes.White, 10, 10);
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Find nearest point
                float minDist = 50f;
                Point nearest = null;

                foreach(var p in points)
                {
                    float dx = p.X - e.X;
                    float dy = p.Y - e.Y;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                    
                    if(dist < minDist)
                    {
                        minDist = dist;
                        nearest = p;
                    }
                }

                if(nearest != null)
                {
                    draggedPoint = nearest;
                    isDragging = true;
                }
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
            draggedPoint = null;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            // Logic handled in UpdatePhysics for smoother interaction with Verlet integration
        }
    }
}
