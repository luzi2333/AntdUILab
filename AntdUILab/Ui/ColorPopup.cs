using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AntdUILab.App;

namespace AntdUILab.Ui
{
    /// <summary>色块按钮：左边一个颜色方块，右边显示 #RRGGBB。点一下选颜色。</summary>
    public class ColorButton : Control
    {
        public Color Value = Color.Empty;
        public string EmptyText = "选择颜色";

        private bool _hot;

        public ColorButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            Cursor = Cursors.Hand;
            Font = Theme.Small;
            Height = 30;
        }

        protected override void OnMouseEnter(EventArgs e) { _hot = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hot = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(0, 0, Width - 1, Height - 1);
            Theme.FillRound(g, r, 6, _hot ? Theme.BlueSoft : Color.White);
            Theme.StrokeRound(g, r, 3, _hot ? Theme.Blue : Theme.Line);

            var box = new Rectangle(5, (Height - 16) / 2, 16, 16);
            if (Value.IsEmpty)
            {
                using (var pen = new Pen(Theme.Faint, 1))
                using (var dash = new Pen(Theme.Faint, 1) { DashStyle = DashStyle.Dot })
                    g.DrawRectangle(dash, box);
                Theme.Draw(g, EmptyText, new Rectangle(28, 0, Width - 32, Height), Font, Theme.Sub, TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }
            else
            {
                using (var b = new SolidBrush(Value)) g.FillRectangle(b, box);
                using (var pen = new Pen(Color.FromArgb(40, 0, 0, 0))) g.DrawRectangle(pen, box);
                Theme.Draw(g, Theme.Hex(Value), new Rectangle(28, 0, Width - 32, Height), Font, Theme.Sub, TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }
        }
    }

    /// <summary>取色弹层：35 个常用色 + 自定义（系统取色器）。</summary>
    public class ColorPopup : Form
    {
        private static readonly Color[] Palette =
        {
            // 灰阶
            Color.FromArgb(255,255,255), Color.FromArgb(245,245,245), Color.FromArgb(217,217,217), Color.FromArgb(140,140,140),
            Color.FromArgb(89,89,89), Color.FromArgb(38,38,38), Color.FromArgb(0,0,0),
            // 主色蓝
            Color.FromArgb(22,119,255), Color.FromArgb(64,150,255), Color.FromArgb(105,177,255), Color.FromArgb(145,202,255),
            Color.FromArgb(186,224,255), Color.FromArgb(230,244,255), Color.FromArgb(240,245,255),
            // 成功绿
            Color.FromArgb(82,196,26), Color.FromArgb(115,209,61), Color.FromArgb(149,222,100), Color.FromArgb(183,235,143),
            Color.FromArgb(217,247,190), Color.FromArgb(246,255,237), Color.FromArgb(35,120,4),
            // 警告 / 错误
            Color.FromArgb(250,173,20), Color.FromArgb(255,197,61), Color.FromArgb(255,229,143), Color.FromArgb(255,251,230),
            Color.FromArgb(255,77,79), Color.FromArgb(255,120,117), Color.FromArgb(255,241,240),
            // 紫 / 青
            Color.FromArgb(114,46,209), Color.FromArgb(146,84,222), Color.FromArgb(211,173,247), Color.FromArgb(249,240,255),
            Color.FromArgb(19,194,194), Color.FromArgb(135,232,222), Color.FromArgb(230,255,251)
        };

        private const int Cols = 7;
        private const int Tile = 28;
        private const int Gap = 6;

        public Color Picked = Color.Empty;

        private readonly SwatchGrid _grid;
        private readonly Label _hex;
        private bool _inDialog;

        public ColorPopup(Color current)
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            BackColor = Color.White;
            Font = Theme.Base;
            DoubleBuffered = true;
            ClientSize = new Size(Cols * Tile + (Cols - 1) * Gap + 24, 34 + 5 * Tile + 4 * Gap + 44);
            _grid = new SwatchGrid { Colors = Palette, Location = new Point(12, 34), Size = new Size(Cols * Tile + (Cols - 1) * Gap, 5 * Tile + 4 * Gap) };
            _grid.Current = current;
            _grid.Pick += idx =>
            {
                Picked = Palette[idx];
                DialogResult = DialogResult.OK;
                Close();
            };
            Controls.Add(_grid);

            var title = new Label
            {
                Text = "选择颜色 · Pick a color",
                Font = Theme.SmallBold,
                ForeColor = Theme.Text,
                AutoSize = false,
                Location = new Point(12, 8),
                Size = new Size(ClientSize.Width - 24, 20)
            };
            Controls.Add(title);

            _hex = new Label
            {
                Text = current.IsEmpty ? "当前：未设置" : "当前：" + Theme.Hex(current),
                Font = Theme.Tiny,
                ForeColor = Theme.Faint,
                AutoSize = false,
                Location = new Point(12, _grid.Bottom + 6),
                Size = new Size(120, 20)
            };
            Controls.Add(_hex);

            var more = new FlatButton
            {
                Text = "自定义…",
                Size = new Size(78, 26),
                Outline = true,
                Font = Theme.Tiny,
                Location = new Point(100, _grid.Bottom + 3)
            };
            more.Click += (s, e) => Custom();
            Controls.Add(more);

            var cancel = new FlatButton
            {
                Text = "取消",
                Size = new Size(56, 26),
                Outline = true,
                Font = Theme.Tiny,
                Location = new Point(ClientSize.Width - 68, _grid.Bottom + 3)
            };
            cancel.Click += (s, e) => Close();
            Controls.Add(cancel);

            KeyPreview = true;
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };
        }

        private void Custom()
        {
            _inDialog = true;
            try
            {
                using (var dlg = new ColorDialog { Color = Picked.IsEmpty ? Color.White : Picked, FullOpen = true })
                {
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        Picked = dlg.Color;
                        DialogResult = DialogResult.OK;
                        Close();
                        return;
                    }
                }
            }
            finally { _inDialog = false; }
            if (!IsDisposed) _hex.Text = "当前：" + (Picked.IsEmpty ? "未设置" : Theme.Hex(Picked));
        }

        /// <summary>把弹层放到按钮下面，超出屏幕就往回收。</summary>
        public void ShowAt(Point screen)
        {
            Rectangle wa = Screen.FromPoint(screen).WorkingArea;
            int x = Math.Min(screen.X, wa.Right - Width - 8);
            int y = screen.Y + Height > wa.Bottom ? screen.Y - Height - 40 : screen.Y;
            Location = new Point(Math.Max(wa.Left + 8, x), Math.Max(wa.Top + 8, y));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var pen = new Pen(Color.FromArgb(60, 0, 0, 0)))
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }

        /// <summary>色块网格。</summary>
        private class SwatchGrid : Control
        {
            public Color[] Colors = new Color[0];
            public Color Current = Color.Empty;
            public event Action<int> Pick;

            private int _hot = -1;

            public SwatchGrid()
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
                Cursor = Cursors.Hand;
            }

            private Rectangle RectOf(int i)
            {
                int c = i % Cols, r = i / Cols;
                return new Rectangle(c * (Tile + Gap), r * (Tile + Gap), Tile, Tile);
            }

            private int IndexAt(Point p)
            {
                for (int i = 0; i < Colors.Length; i++)
                    if (RectOf(i).Contains(p)) return i;
                return -1;
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                int i = IndexAt(e.Location);
                if (i != _hot) { _hot = i; Invalidate(); }
                base.OnMouseMove(e);
            }

            protected override void OnMouseLeave(EventArgs e) { _hot = -1; Invalidate(); base.OnMouseLeave(e); }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                int i = IndexAt(e.Location);
                if (i >= 0 && e.Button == MouseButtons.Left && Pick != null) Pick(i);
                base.OnMouseDown(e);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                for (int i = 0; i < Colors.Length; i++)
                {
                    Rectangle r = RectOf(i);
                    r.Width -= 1; r.Height -= 1;
                    using (var path = Theme.Round(r, 4))
                    {
                        using (var b = new SolidBrush(Colors[i])) g.FillPath(b, path);
                        bool cur = !Current.IsEmpty && Colors[i].ToArgb() == Current.ToArgb();
                        using (var pen = new Pen(cur ? Theme.Blue : Color.FromArgb(70, 0, 0, 0), cur ? 2f : 1f)) g.DrawPath(pen, path);
                    }
                    if (i == _hot)
                    {
                        using (var pen = new Pen(Theme.BlueDeep, 2f))
                        using (var path = Theme.Round(new Rectangle(r.X - 1, r.Y - 1, r.Width + 2, r.Height + 2), 5))
                            g.DrawPath(pen, path);
                    }
                }
            }
        }
    }
}
