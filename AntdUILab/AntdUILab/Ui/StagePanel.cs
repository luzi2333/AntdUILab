using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AntdUILab.App;

namespace AntdUILab.Ui
{
    /// <summary>中央舞台：点阵背景 + 设备取景框 + 居中承载真实控件；支持拖动与缩放。</summary>
    public class StagePanel : Panel
    {
        public object Control;
        public Meta Meta;
        public Size SceneSize = Size.Empty;

        /// <summary>位置 / 尺寸变化（给主窗体写日志、刷新代码页）。</summary>
        public event Action<string> Changed;

        public bool DragMode { get { return _dragMode; } }

        // 事件触发时的可视反馈
        private string _fxName = "";
        private string _fxInfo = "";
        private DateTime _fxStart = DateTime.MinValue;
        private DateTime _fxUntil = DateTime.MinValue;
        private readonly Timer _fxTimer;

        private bool _dragMode;
        private bool _moving;
        private Point _moveScreenStart;
        private Point _ctlStart;

        private readonly Control _sizeHandle;
        private bool _resizing;
        private Point _resizeScreenStart;
        private Size _sizeStart;

        public StagePanel()
        {
            BackColor = Color.White;
            DoubleBuffered = true;
            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Padding = new Padding(0);

            _sizeHandle = new SizeHandle();
            _sizeHandle.Visible = false;
            _sizeHandle.MouseDown += SizeDown;
            _sizeHandle.MouseMove += SizeMove;
            _sizeHandle.MouseUp += SizeUp;

            _fxTimer = new Timer { Interval = 33 };
            _fxTimer.Tick += (s, e) =>
            {
                if (DateTime.Now >= _fxUntil) _fxTimer.Stop();
                Invalidate();
            };
        }

        /// <summary>事件触发时在控件外圈脉冲一下，并在旁边浮出「事件名 + 参数」。</summary>
        public void FlashEvent(string name, string info)
        {
            _fxName = name ?? "";
            _fxInfo = info ?? "";
            _fxStart = DateTime.Now;
            _fxUntil = _fxStart.AddMilliseconds(1300);
            _fxTimer.Start();
            Invalidate();
        }

        public void SetControl(object ctl, Meta meta)
        {
            Controls.Clear();
            Control = ctl;
            Meta = meta;
            if (ctl is Control)
            {
                var c = (Control)ctl;
                Controls.Add(c);
            }
            if (_sizeHandle != null)
            {
                Controls.Add(_sizeHandle);
                _sizeHandle.BringToFront();
            }
            Relayout();
        }

        /// <summary>打开后控件不再接收鼠标，舞台上按住即可拖动。</summary>
        public void SetDragMode(bool on, bool quiet = false)
        {
            if (_dragMode == on) return;
            _dragMode = on;
            var c = Control as Control;
            if (c != null) c.Enabled = !on;
            Cursor = on ? Cursors.SizeAll : Cursors.Default;
            PlaceHandles();
            if (Changed != null && !quiet) Changed(on ? "进入拖动模式：按住控件移动，点工具栏「✥ 拖动」退出" : "退出拖动模式：控件恢复响应");
            Invalidate();
        }

        public void Relayout()
        {
            var c = Control as Control;
            if (c == null) { Invalidate(); return; }
            Rectangle device = DeviceRect(c);

            int w = c.Width;
            int h = c.Height;
            var pos = new Point(device.Left + (device.Width - w) / 2, device.Top + (device.Height - h) / 2);
            if (pos.X < device.Left + 8) pos.X = device.Left + 8;
            if (pos.Y < device.Top + 8) pos.Y = device.Top + 8;
            c.Location = pos;
            c.BringToFront();
            if (_sizeHandle != null) _sizeHandle.BringToFront();
            PlaceHandles();
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Relayout();
        }

        /// <summary>属性面板里改了 Location / Size（或外部改了控件几何）后，把控件夹回取景框内并刷新手柄。</summary>
        public void ApplyControlGeometry()
        {
            var c = Control as Control;
            if (c == null) return;
            Rectangle device = DeviceRect(c);
            int x = Math.Max(device.Left + 4, Math.Min(c.Left, device.Right - 4 - c.Width));
            int y = Math.Max(device.Top + 4, Math.Min(c.Top, device.Bottom - 4 - c.Height));
            if (x != c.Left || y != c.Top) c.Location = new Point(x, y);
            c.BringToFront();
            if (_sizeHandle != null) _sizeHandle.BringToFront();
            PlaceHandles();
            Invalidate();
        }

        private void PlaceHandles()
        {
            var c = Control as Control;
            if (_sizeHandle == null) return;
            if (c == null) { _sizeHandle.Visible = false; return; }
            _sizeHandle.Visible = true;
            _sizeHandle.Bounds = new Rectangle(c.Right - 7, c.Bottom - 7, 14, 14);
            _sizeHandle.BringToFront();
        }

        private Rectangle DeviceRect(Control c)
        {
            int availW = Math.Max(200, ClientSize.Width);
            int availH = Math.Max(160, ClientSize.Height);

            int w = SceneSize.Width > 0 ? SceneSize.Width : Math.Min(availW - 64, Math.Max(c.Width + 160, 560));
            int h = SceneSize.Height > 0 ? SceneSize.Height : Math.Min(availH - 96, Math.Max(c.Height + 140, 360));

            // 控件比取景框还大时，把框放大到舞台能给的极限，别让控件被裁掉（大表格、大选项卡很常见）
            int maxW = Math.Max(200, availW - 8);
            int maxH = Math.Max(160, availH - 16);
            if (w > maxW) w = maxW;
            if (h > maxH) h = maxH;
            if (w < c.Width + 24) w = Math.Min(maxW, c.Width + 24);
            if (h < c.Height + 24) h = Math.Min(maxH, c.Height + 24);

            int x = Math.Max(4, (availW - w) / 2);
            int y = Math.Max(4, (availH - h) / 2 + 4);
            return new Rectangle(x, y, w, h);
        }

        // ---------- 拖动 ----------
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            var c = Control as Control;
            if (!_dragMode || c == null || c.Width <= 0) return;
            if (!DeviceRect(c).Contains(e.Location)) return;
            _moving = true;
            _moveScreenStart = PointToScreen(e.Location);
            _ctlStart = c.Location;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!_moving) return;
            var c = Control as Control;
            if (c == null) return;
            Point now = PointToScreen(e.Location);
            Rectangle device = DeviceRect(c);
            int x = _ctlStart.X + (now.X - _moveScreenStart.X);
            int y = _ctlStart.Y + (now.Y - _moveScreenStart.Y);
            x = Math.Max(device.Left + 4, Math.Min(Math.Max(device.Left + 4, device.Right - c.Width - 4), x));
            y = Math.Max(device.Top + 4, Math.Min(Math.Max(device.Top + 4, device.Bottom - c.Height - 4), y));
            c.Location = new Point(x, y);
            PlaceHandles();
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (!_moving) return;
            _moving = false;
            var c = Control as Control;
            if (c != null && Changed != null) Changed("位置 → " + c.Left + ", " + c.Top);
            Invalidate();
        }

        // ---------- 缩放 ----------
        private void SizeDown(object sender, MouseEventArgs e)
        {
            var c = Control as Control;
            if (c == null || e.Button != MouseButtons.Left) return;
            _resizing = true;
            _resizeScreenStart = PointToScreen(e.Location);
            _sizeStart = c.Size;
        }

        private void SizeMove(object sender, MouseEventArgs e)
        {
            if (!_resizing) return;
            var c = Control as Control;
            if (c == null) return;
            Point now = PointToScreen(e.Location);
            Rectangle device = DeviceRect(c);
            int w = _sizeStart.Width + (now.X - _resizeScreenStart.X);
            int h = _sizeStart.Height + (now.Y - _resizeScreenStart.Y);
            int maxW = Math.Max(8, device.Right - 4 - c.Left);
            int maxH = Math.Max(8, device.Bottom - 4 - c.Top);
            w = Math.Max(8, Math.Min(maxW, w));
            h = Math.Max(8, Math.Min(maxH, h));
            c.Size = new Size(w, h);
            PlaceHandles();
            Invalidate();
        }

        private void SizeUp(object sender, MouseEventArgs e)
        {
            if (!_resizing) return;
            _resizing = false;
            var c = Control as Control;
            if (c != null && Changed != null) Changed("尺寸 → " + c.Width + " × " + c.Height);
            Invalidate();
        }

        // ---------- 绘制 ----------
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);
            using (var b = new SolidBrush(Theme.LineSoft))
            {
                for (int y = 8; y < ClientSize.Height; y += 18)
                    for (int x = 8; x < ClientSize.Width; x += 18)
                        g.FillRectangle(b, x, y, 1, 1);
            }

            var c = Control as Control;
            if (c != null)
            {
                Rectangle device = DeviceRect(c);

                if (SceneSize.Width > 0)
                {
                    using (var path = Theme.Round(device, 14))
                    {
                        using (var shadow = new SolidBrush(Color.FromArgb(16, 0, 0, 0)))
                            g.FillPath(shadow, Theme.Round(new Rectangle(device.X + 4, device.Y + 6, device.Width, device.Height), 14));
                        using (var white = new SolidBrush(Color.White)) g.FillPath(white, path);
                        using (var pen = new Pen(Theme.Line)) g.DrawPath(pen, path);
                    }
                    string label = "场景 " + device.Width + " × " + device.Height;
                    Theme.Draw(g, label, new Rectangle(device.Left, device.Bottom + 8, device.Width, 18), Theme.Tiny, Theme.Faint, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
                }
                else
                {
                    Rectangle box = device;
                    using (var pen = new Pen(_dragMode ? Theme.Blue : Color.FromArgb(210, 220, 235)) { DashStyle = DashStyle.Dash, DashPattern = new[] { 4f, 4f } })
                        g.DrawRectangle(pen, box);
                    string hint = "自由舞台 · 控件 " + c.Width + " × " + c.Height + " @ " + c.Left + ", " + c.Top;
                    Theme.Draw(g, hint, new Rectangle(box.Left, box.Top - 22, box.Width, 18), Theme.Tiny, Theme.Faint, TextFormatFlags.NoPadding);
                }

                string tip = _dragMode
                    ? "拖动模式：按住控件即可移动（控件暂时不响应点击）；点「✥ 拖动」退出"
                    : "改属性即时生效 · 勾「事件」后操作控件看日志 · 点「✥ 拖动」可移动控件，拖右下角 ◢ 改尺寸";
                Theme.Draw(g, tip, new Rectangle(16, ClientSize.Height - 26, ClientSize.Width - 32, 18), Theme.Tiny, Theme.Faint, TextFormatFlags.NoPadding);

                DrawFlash(g, c);
            }
            else
            {
                Theme.Draw(g, "从左侧选择一个控件", new Rectangle(0, ClientSize.Height / 2 - 10, ClientSize.Width, 22), Theme.H2, Theme.Faint, TextFormatFlags.HorizontalCenter);
            }
        }

        /// <summary>事件触发的脉冲圈 + 事件名浮签（画在控件外侧，不会挡住控件本身）。</summary>
        private void DrawFlash(Graphics g, Control c)
        {
            DateTime now = DateTime.Now;
            if (now >= _fxUntil) return;
            double t = (now - _fxStart).TotalMilliseconds / 1300.0;
            if (t < 0) t = 0;
            if (t > 1) t = 1;

            Rectangle r = c.Bounds;
            int grow = (int)(t * 16);
            int alpha = (int)(170 * (1 - t));
            if (alpha > 0 && grow >= 0)
            {
                using (var pen = new Pen(Color.FromArgb(alpha, Theme.Blue), 2f))
                    g.DrawRectangle(pen, r.X - grow, r.Y - grow, r.Width + grow * 2, r.Height + grow * 2);
            }

            string chip = _fxName;
            if (!string.IsNullOrEmpty(_fxInfo) && _fxInfo != "（无参数）") chip += "  " + _fxInfo;
            Size sz = TextRenderer.MeasureText(chip, Theme.Tiny);
            int cw = Math.Min(Math.Max(140, ClientSize.Width - 20), sz.Width + 20);
            int cx = Math.Max(8, Math.Min(r.Right + 10, ClientSize.Width - cw - 8));
            int cy = r.Top - 30;
            if (cy < 6) cy = r.Bottom + 8;
            if (cy + 24 > ClientSize.Height - 30) cy = Math.Max(6, r.Top - 30);
            using (var path = Theme.Round(new Rectangle(cx, cy, cw, 22), 6))
            using (var b = new SolidBrush(Color.FromArgb(235, Theme.BlueDeep)))
                g.FillPath(b, path);
            Theme.Draw(g, chip, new Rectangle(cx + 10, cy + 4, cw - 20, 16), Theme.Tiny, Color.White, TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
        }

        /// <summary>右下角缩放手柄。</summary>
        private class SizeHandle : Control
        {
            private bool _hot;

            public SizeHandle()
            {
                Size = new Size(14, 14);
                Cursor = Cursors.SizeNWSE;
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            }

            protected override void OnMouseEnter(EventArgs e) { _hot = true; Invalidate(); base.OnMouseEnter(e); }
            protected override void OnMouseLeave(EventArgs e) { _hot = false; Invalidate(); base.OnMouseLeave(e); }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var b = new SolidBrush(_hot ? Theme.BlueDeep : Theme.Blue))
                    g.FillEllipse(b, 0, 0, Width - 1, Height - 1);
                using (var pen = new Pen(Color.White, 1.6f))
                {
                    g.DrawLine(pen, 4, 10, 10, 4);
                    g.DrawLine(pen, 7, 10, 10, 7);
                }
            }
        }
    }
}
