using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AntdUILab.App;

namespace AntdUILab.Ui
{
    /// <summary>事件页下半部分：把同一只手势触发的事件按顺序列出来（带 +Δms）。</summary>
    public class GesturePanel : Panel
    {
        private class Rec
        {
            public DateTime At;
            public string Name;
            public string Detail;
            public int Gid;
        }

        private readonly List<Rec> _recs = new List<Rec>();
        private int _gid;
        private DateTime _lastAt = DateTime.MinValue;
        private int _fired;

        public GesturePanel()
        {
            BackColor = Color.White;
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        }

        public void Add(EventEntry e)
        {
            _fired++;
            if (_lastAt == DateTime.MinValue || (e.At - _lastAt).TotalMilliseconds > 260) _gid++;
            _lastAt = e.At;
            _recs.Add(new Rec { At = e.At, Name = e.Title, Detail = e.Detail, Gid = _gid });
            while (_recs.Count > 120) _recs.RemoveAt(0);
            Invalidate();
        }

        public void Clear()
        {
            _recs.Clear();
            _fired = 0;
            _gid = 0;
            _lastAt = DateTime.MinValue;
            Invalidate();
        }

        private List<List<Rec>> Groups()
        {
            var list = new List<List<Rec>>();
            int cur = -1;
            foreach (Rec r in _recs)
            {
                if (list.Count == 0 || r.Gid != cur) { list.Add(new List<Rec>()); cur = r.Gid; }
                list[list.Count - 1].Add(r);
            }
            return list;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            using (var b = new SolidBrush(Color.White)) g.FillRectangle(b, ClientRectangle);
            using (var pen = new Pen(Theme.LineSoft)) g.DrawLine(pen, 0, 0, Width, 0);

            Theme.Draw(g, "手势时间轴 Gesture timeline", new Rectangle(12, 8, 220, 16), Theme.SmallBold, Theme.Text, TextFormatFlags.NoPadding);
            string count = _fired == 0 ? "还没有触发" : ("共 " + _fired + " 次触发 · " + Groups().Count + " 个手势");
            Theme.Draw(g, count, new Rectangle(Width - 200, 9, 188, 16), Theme.Tiny, Theme.Faint, TextFormatFlags.Right | TextFormatFlags.NoPadding);

            List<List<Rec>> gs = Groups();
            if (gs.Count == 0)
            {
                Theme.Draw(g, "在上面勾选事件，然后到舞台上点一点控件（双击、右键、按键都试试），",
                    new Rectangle(12, 32, Width - 24, 18), Theme.Small, Theme.Sub, TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
                Theme.Draw(g, "这里会把同一次操作触发的事件按先后顺序列出来。",
                    new Rectangle(12, 52, Width - 24, 18), Theme.Small, Theme.Sub, TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
                Theme.Draw(g, "小技巧：一次点击通常依次触发",
                    new Rectangle(12, 78, Width - 24, 16), Theme.Tiny, Theme.Faint, TextFormatFlags.NoPadding);
                Theme.Draw(g, "MouseDown → MouseUp → Click → MouseClick",
                    new Rectangle(12, 96, Width - 24, 16), Theme.Tiny, Theme.Blue, TextFormatFlags.NoPadding);
                return;
            }

            List<Rec> last = gs[gs.Count - 1];
            DateTime t0 = last[0].At;
            string head = "最近一次手势 · " + last.Count + " 个事件 · " + t0.ToString("HH:mm:ss.fff");
            Theme.Draw(g, head, new Rectangle(12, 30, Width - 24, 16), Theme.Tiny, Theme.Blue, TextFormatFlags.NoPadding);

            int y = 50;
            int rowH = 18;
            int maxRows = Math.Max(2, (Height - 78) / rowH);
            int start = Math.Max(0, last.Count - maxRows);
            if (start > 0)
                Theme.Draw(g, "… 前面还有 " + start + " 个（只显示最近 " + maxRows + " 个）", new Rectangle(24, y - 2, Width - 48, 14), Theme.Tiny, Theme.Faint, TextFormatFlags.NoPadding);

            for (int i = start; i < last.Count; i++)
            {
                Rec r = last[i];
                double ms = (r.At - t0).TotalMilliseconds;
                string no = "①②③④⑤⑥⑦⑧⑨"[i % 9].ToString();
                Theme.Draw(g, no, new Rectangle(20, y, 16, 16), Theme.Tiny, Theme.Blue, TextFormatFlags.NoPadding);
                Theme.Draw(g, r.Name, new Rectangle(38, y, 104, 16), Theme.SmallBold, Theme.Text, TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
                Theme.Draw(g, "+" + ms.ToString("0") + "ms", new Rectangle(146, y + 1, 62, 14), Theme.Tiny, Theme.Faint, TextFormatFlags.NoPadding);
                if (!string.IsNullOrEmpty(r.Detail) && r.Detail != "（无参数）")
                    Theme.Draw(g, r.Detail, new Rectangle(212, y + 1, Math.Max(40, Width - 224), 14), Theme.Tiny, Theme.Sub, TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
                y += rowH;
            }

            // 上一手势摘要
            if (gs.Count > 1 && y + 20 < Height)
            {
                List<Rec> prev = gs[gs.Count - 2];
                var names = new List<string>();
                foreach (Rec r in prev) names.Add(r.Name);
                string s = "上一手势：" + string.Join(" → ", names.ToArray()) + "（" + prev.Count + " 个）";
                using (var pen = new Pen(Theme.LineSoft) { DashStyle = DashStyle.Dot })
                    g.DrawLine(pen, 12, y + 6, Width - 12, y + 6);
                Theme.Draw(g, s, new Rectangle(12, y + 10, Width - 24, 14), Theme.Tiny, Theme.Faint, TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
            }
        }
    }
}
