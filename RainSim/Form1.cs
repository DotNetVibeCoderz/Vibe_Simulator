using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RainSim
{
    public partial class Form1 : Form
    {
        // Variabel Simulasi
        private List<RainDrop> drops = new List<RainDrop>();
        private System.Windows.Forms.Timer gameLoopTimer; // Explicitly specify the Timer namespace
        private float windSpeed = 0;
        private float gravity = 5;
        private int maxDrops = 500;
        
        // Komponen UI
        private SplitContainer splitContainer;
        private PictureBox canvas;
        private Panel settingsPanel;
        
        // Controls
        private TrackBar tbWind;
        private TrackBar tbGravity;
        private TrackBar tbAmount;
        private Label lblWindVal;
        private Label lblGravVal;
        private Label lblAmountVal;

        public Form1()
        {
            InitializeCustomComponents();
            InitializeSimulation();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "RainSim - Jacky The Code Bender";
            this.Size = new Size(1000, 600);
            this.DoubleBuffered = true; // Mengurangi flicker

            // 1. Setup Split Container
            splitContainer = new SplitContainer();
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.FixedPanel = FixedPanel.Panel2; // Panel kanan fix size
            splitContainer.SplitterDistance = 750;
            this.Controls.Add(splitContainer);

            // 2. Setup Canvas (Kiri)
            canvas = new PictureBox();
            canvas.Dock = DockStyle.Fill;
            canvas.BackColor = Color.FromArgb(10, 10, 30); // Langit malam gelap
            canvas.Paint += Canvas_Paint;
            canvas.Resize += Canvas_Resize;
            splitContainer.Panel1.Controls.Add(canvas);

            // 3. Setup Settings Panel (Kanan)
            settingsPanel = new Panel();
            settingsPanel.Dock = DockStyle.Fill;
            settingsPanel.BackColor = Color.FromArgb(40, 40, 40);
            settingsPanel.Padding = new Padding(10);
            splitContainer.Panel2.Controls.Add(settingsPanel);

            // Helper untuk membuat control settings
            int yPos = 20;
            
            // --- Wind Control ---
            CreateSettingControl("Kecepatan Angin", -20, 20, (int)windSpeed, ref yPos, out tbWind, out lblWindVal);
            tbWind.ValueChanged += (s, e) => { 
                windSpeed = tbWind.Value; 
                lblWindVal.Text = windSpeed.ToString(); 
            };

            // --- Gravity Control ---
            CreateSettingControl("Gravitasi / Kecepatan Jatuh", 1, 30, (int)gravity, ref yPos, out tbGravity, out lblGravVal);
            tbGravity.ValueChanged += (s, e) => { 
                gravity = tbGravity.Value; 
                lblGravVal.Text = gravity.ToString(); 
            };

            // --- Amount Control ---
            CreateSettingControl("Intensitas Hujan", 100, 3000, maxDrops, ref yPos, out tbAmount, out lblAmountVal);
            tbAmount.ValueChanged += (s, e) => { 
                maxDrops = tbAmount.Value; 
                lblAmountVal.Text = maxDrops.ToString(); 
                AdjustDropCount();
            };
            
            // Tambahkan Info
            Label lblInfo = new Label();
            lblInfo.Text = "Dibuat oleh Jacky\nThe Code Bender";
            lblInfo.ForeColor = Color.Yellow;
            lblInfo.AutoSize = true;
            lblInfo.Location = new Point(10, yPos + 20);
            settingsPanel.Controls.Add(lblInfo);
        }

        private void CreateSettingControl(string title, int min, int max, int def, ref int y, out TrackBar tb, out Label lblVal)
        {
            // Label Judul
            Label lblTitle = new Label();
            lblTitle.Text = title;
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(10, y);
            lblTitle.AutoSize = true;
            settingsPanel.Controls.Add(lblTitle);

            y += 25;

            // TrackBar
            tb = new TrackBar();
            tb.Minimum = min;
            tb.Maximum = max;
            tb.Value = def;
            tb.Location = new Point(10, y);
            tb.Width = 200;
            settingsPanel.Controls.Add(tb);

            // Label Nilai
            lblVal = new Label();
            lblVal.Text = def.ToString();
            lblVal.ForeColor = Color.Cyan;
            lblVal.Location = new Point(220, y);
            lblVal.AutoSize = true;
            settingsPanel.Controls.Add(lblVal);

            y += 50;
        }

        private void InitializeSimulation()
        {
            // Inisialisasi Timer Animasi
            gameLoopTimer = new System.Windows.Forms.Timer();
            gameLoopTimer.Interval = 16; // ~60 FPS
            gameLoopTimer.Tick += GameLoopTimer_Tick;
            gameLoopTimer.Start();

            // Inisialisasi tetesan awal
            AdjustDropCount();
        }

        private void AdjustDropCount()
        {
            // Tambah jika kurang
            if (drops.Count < maxDrops)
            {
                int diff = maxDrops - drops.Count;
                for (int i = 0; i < diff; i++)
                {
                    drops.Add(new RainDrop(canvas.Width, canvas.Height));
                }
            }
            // Hapus jika berlebih
            else if (drops.Count > maxDrops)
            {
                int removeCount = drops.Count - maxDrops;
                drops.RemoveRange(0, removeCount);
            }
        }

        private void Canvas_Resize(object sender, EventArgs e)
        {
            // Reset drops ketika resize agar tidak hilang
            drops.Clear();
            AdjustDropCount();
        }

        private void GameLoopTimer_Tick(object sender, EventArgs e)
        {
            // Update logic semua tetesan
            foreach (var drop in drops)
            {
                drop.Update(gravity, windSpeed);
            }

            // Redraw
            canvas.Invalidate();
        }

        private void Canvas_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;

            // Gambar setiap tetesan
            // Kita pakai Pen u/ menggambar garis
            // Warna disesuaikan dengan Z (semakin jauh semakin transparan/gelap)
            
            foreach (var drop in drops)
            {
                // Hitung transparansi/ketebalan berdasarkan Z depth
                // Z kecil = jauh (lebih pudar/tipis), Z besar = dekat
                
                int alpha = (int)Math.Min(255, Math.Max(50, drop.Z * 10)); // Simple mapping
                int thickness = (int)Math.Max(1, drop.Z / 5);
                
                Color rainColor = Color.FromArgb(alpha, 170, 210, 255); // Biru muda
                
                using (Pen p = new Pen(rainColor, thickness))
                {
                    // Hitung endpoint berdasarkan angin agar garis miring
                    float endX = drop.X + windSpeed; // Sederhana: miring sesuai angin
                    float endY = drop.Y + drop.Length;

                    g.DrawLine(p, drop.X, drop.Y, endX, endY);
                }
            }
        }
    }
}
