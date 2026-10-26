using System;
using System.Drawing;
using System.Windows.Forms;
using AntdUILab.App;

namespace AntdUILab.Ui
{
    /// <summary>圆角小标签（分组/精讲/英文名）。</summary>
    public class Chip : Control
    {
        public int Radius = 4;
        public Color FillColor = Color.Empty;
        public Color StrokeColor = Color.Empty;
        public bool AutoWidth = true;

        public Chip()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            Font = Theme.Small;
            ForeColor = Theme.Sub;
            Height = 20;
            Width = 40;
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            if (AutoWidth) Width = TextRenderer.MeasureText(Text, Font).Width + 14;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(0, 0, Width - 1, Height - 1);
            Color fill = FillColor.IsEmpty ? Theme.Soft(ForeColor) : FillColor;
            Theme.FillRound(g, r, Radius, fill);
            if (!StrokeColor.IsEmpty) Theme.StrokeRound(g, r, Radius, StrokeColor);
            Theme.Draw(g, Text, r, Font, ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
        }
    }

    /// <summary>扁平按钮（复制代码、恢复默认等）。</summary>
    public class FlatButton : Control
    {
        public int Radius = 6;
        public Color FillColor = Theme.Blue;
        public Color HoverColor = Theme.BlueDeep;
        public Color TextColor = Color.White;
        public int Index;
        public bool Outline;

        private bool _hover;

        public FlatButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            Font = Theme.Base;
            Height = 32;
            Width = 96;
            Cursor = Cursors.Hand;
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(0, 0, Width - 1, Height - 1);
            if (Outline)
            {
                Theme.FillRound(g, r, Radius, _hover ? Theme.BlueSoft : Color.White);
                Theme.StrokeRound(g, r, Radius, _hover ? Theme.Blue : Theme.Line);
                Theme.Draw(g, Text, r, Font, _hover ? Theme.Blue : Theme.Sub, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }
            else
            {
                Theme.FillRound(g, r, Radius, _hover ? HoverColor : FillColor);
                Theme.Draw(g, Text, r, Font, Enabled ? TextColor : Color.FromArgb(200, 220, 255), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }
        }
    }

    /// <summary>下划线页签条（属性 / 事件 / 代码）。</summary>
    public class TabStrip : Control
    {
        public string[] Items = new string[0];
        public int Selected = 0;
        public event EventHandler SelectedChanged;

        private int _hover = -1;
        private int[] _widths;

        public TabStrip()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            Font = Theme.Base;
            Height = 42;
            BackColor = Color.White;
        }

        public void SetItems(params string[] items)
        {
            Items = items;
            _widths = null;
            if (Selected >= Items.Length) Selected = 0;
            Invalidate();
        }

        private int[] Widths()
        {
            if (_widths != null && _widths.Length == Items.Length) return _widths;
            _widths = new int[Items.Length];
            for (int i = 0; i < Items.Length; i++)
                _widths[i] = TextRenderer.MeasureText(Items[i], Font).Width + 34;
            return _widths;
        }

        public int HitTest(Point p)
        {
            int x = 0;
            int[] w = Widths();
            for (int i = 0; i < w.Length; i++)
            {
                if (p.X >= x && p.X < x + w[i]) return i;
                x += w[i];
            }
            return -1;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            int i = HitTest(e.Location);
            if (i != _hover) { _hover = i; Invalidate(); }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hover = -1;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            int i = HitTest(e.Location);
            if (i >= 0 && i != Selected)
            {
                Selected = i;
                Invalidate();
                if (SelectedChanged != null) SelectedChanged(this, EventArgs.Empty);
            }
            base.OnMouseClick(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            using (var b = new SolidBrush(Color.White)) g.FillRectangle(b, ClientRectangle);
            using (var pen = new Pen(Theme.Line))
                g.DrawLine(pen, 0, Height - 1, Width, Height - 1);

            int x = 0;
            int[] w = Widths();
            for (int i = 0; i < Items.Length; i++)
            {
                var r = new Rectangle(x, 0, w[i], Height - 1);
                Color c = i == Selected ? Theme.Blue : i == _hover ? Theme.Text : Theme.Sub;
                Theme.Draw(g, Items[i], r, i == Selected ? Theme.Bold : Theme.Base, c, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                if (i == Selected)
                {
                    using (var pen = new Pen(Theme.Blue, 2f))
                        g.DrawLine(pen, x + 12, Height - 2, x + w[i] - 12, Height - 2);
                }
                x += w[i];
            }
        }
    }

    /// <summary>细水平分隔线。</summary>
    public class HairLine : Control
    {
        public HairLine()
        {
            Height = 1;
            SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Line;
        }
    }

    /// <summary>可拖动的分隔条：竖条改左右宽度，横条改上下高度。拖动时回调总位移（像素）。</summary>
    public class SplitBar : Control
    {
        public bool Vertical = true;
        public event Action Started;
        public event Action<int> Moved;

        private bool _hot;
        private bool _drag;
        private Point _from;

        public SplitBar()
        {
            Width = 6;
            Height = 6;
            BackColor = Theme.Bg;
            Cursor = Cursors.VSplit;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            using (var b = new SolidBrush(_hot || _drag ? Theme.BlueSoft : Theme.Bg)) g.FillRectangle(b, ClientRectangle);
            using (var pen = new Pen(_hot || _drag ? Theme.Blue : Theme.Line))
            {
                if (Vertical) g.DrawLine(pen, Width / 2, 0, Width / 2, Height);
                else g.DrawLine(pen, 0, Height / 2, Width, Height / 2);
            }
            // 中间三个小点，暗示可以拖
            if (Vertical && Height > 40)
            {
                using (var b = new SolidBrush(_hot || _drag ? Theme.Blue : Theme.Faint))
                {
                    int cx = Width / 2, cy = Height / 2;
                    for (int i = -6; i <= 6; i += 6) g.FillEllipse(b, cx - 1, cy + i - 1, 2, 2);
                }
            }
        }

        protected override void OnMouseEnter(EventArgs e) { _hot = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hot = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            _drag = true;
            _from = PointToScreen(e.Location);
            Capture = true;
            if (Started != null) Started();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!_drag) return;
            Point p = PointToScreen(e.Location);
            int d = Vertical ? p.X - _from.X : p.Y - _from.Y;
            if (Moved != null) Moved(d);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (!_drag) return;
            _drag = false;
            Capture = false;
            Invalidate();
        }
    }
}
