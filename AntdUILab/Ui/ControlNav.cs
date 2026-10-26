using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AntdUILab.App;

namespace AntdUILab.Ui
{
    /// <summary>左侧控件导航：按分类分组，★ 为 15 个精讲控件；顶部可搜索。</summary>
    public class ControlNav : Panel
    {
        public event Action<Meta> Selected;

        private readonly TextBox _search;
        private readonly NavCanvas _canvas;
        private readonly Label _legend;
        private Meta _current;
        private string _query = "";

        public ControlNav()
        {
            BackColor = Color.White;
            Width = 250;

            var brand = new Label
            {
                Text = "控件 Control",
                Font = Theme.SmallBold,
                ForeColor = Theme.Text,
                Location = new Point(14, 12),
                AutoSize = true
            };
            var brandEn = new Label
            {
                Text = Catalog.Metas.Count + " 个控件 · " + Catalog.Metas.FindAll(m => m.Star > 0).Count + " 个精讲",
                Font = Theme.Tiny,
                ForeColor = Theme.Faint,
                Location = new Point(14, 30),
                AutoSize = true
            };

            var searchWrap = new Panel { Location = new Point(12, 54), Size = new Size(226, 34), BackColor = Theme.Bg };
            _search = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = Theme.Base,
                ForeColor = Theme.Faint,
                Text = "搜索控件 / search…",
                BackColor = Theme.Bg,
                Location = new Point(8, 8),
                Width = 206
            };
            _search.GotFocus += (s, e) =>
            {
                if (_search.ForeColor == Theme.Faint) { _search.Text = ""; _search.ForeColor = Theme.Text; }
            };
            _search.LostFocus += (s, e) =>
            {
                if (_search.Text.Trim().Length == 0) { _search.ForeColor = Theme.Faint; _search.Text = "搜索控件 / search…"; }
            };
            _search.TextChanged += (s, e) =>
            {
                _query = _search.ForeColor == Theme.Faint ? "" : _search.Text.Trim();
                Rebuild();
            };
            searchWrap.Controls.Add(_search);

            _legend = new Label
            {
                Text = "★ 精讲 " + Catalog.Metas.FindAll(m => m.Star > 0).Count + " · 自动收录 " + Catalog.Metas.FindAll(m => m.Star == 0).Count,
                Font = Theme.Tiny,
                ForeColor = Theme.Faint,
                Location = new Point(14, 94),
                AutoSize = true
            };

            _canvas = new NavCanvas { Location = new Point(0, 112), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
            _canvas.Pick += meta =>
            {
                if (meta == null) return;
                _current = meta;
                _canvas.SelectedMeta = meta;
                _canvas.Invalidate();
                if (Selected != null) Selected(meta);
            };

            Controls.Add(brand);
            Controls.Add(brandEn);
            Controls.Add(searchWrap);
            Controls.Add(_legend);
            Controls.Add(_canvas);

            SizeChanged += (s, e) =>
            {
                searchWrap.Width = Math.Max(120, Width - 24);
                _search.Width = Math.Max(80, searchWrap.Width - 16);
                _canvas.Width = Width;
                _canvas.Height = Math.Max(80, Height - 112);
                if (_canvas.Height > Height - 112) _canvas.Height = Math.Max(80, Height - 112);
            };

            Rebuild();
        }

        public void Select(Meta meta)
        {
            _current = meta;
            _canvas.SelectedMeta = meta;
            _canvas.Invalidate();
        }

        public void Rebuild()
        {
            _canvas.Build(_query, _current);
            _canvas.Invalidate();
        }

        // 供 MainForm 转发鼠标滚轮（焦点不在导航上时也能滚动）
        public void ScrollBy(int dy)
        {
            if (_canvas != null) _canvas.ScrollBy(dy);
        }
    }

    internal class NavItem
    {
        public bool Header;
        public string Text;
        public string Right;
        public Meta Meta;
        public int Y;
        public int H = 52;
    }

    internal class NavCanvas : Control
    {
        public event Action<Meta> Pick;
        public Meta SelectedMeta;

        private readonly List<NavItem> _items = new List<NavItem>();
        private readonly VScrollBar _bar;
        private int _top;
        private int _hover = -1;
        private int _content;

        public NavCanvas()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            BackColor = Color.White;
            _bar = new VScrollBar { Dock = DockStyle.Right, Visible = false, Width = 10 };
            _bar.ValueChanged += (s, e) => { _top = _bar.Value; Invalidate(); };
            Controls.Add(_bar);
        }

        public void Build(string query, Meta current)
        {
            _items.Clear();
            int y = 6;
            var all = Catalog.All();
            bool searching = !string.IsNullOrEmpty(query);

            foreach (string group in Catalog.Groups)
            {
                var list = all.Where(m => m.Group == group).ToList();
                if (searching)
                    list = list.Where(m => (m.Zh + " " + m.En + " " + m.Type).IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                if (list.Count == 0) continue;

                _items.Add(new NavItem { Header = true, Text = group + "  " + list.Count, Y = y, H = 30 });
                y += 30;
                foreach (Meta m in list)
                {
                    _items.Add(new NavItem { Text = m.Zh, Right = m.En, Meta = m, Y = y, H = 52 });
                    y += 52;
                }
                y += 6;
            }

            _content = y + 6;
            UpdateBar();
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateBar();
            Invalidate();
        }

        private void UpdateBar()
        {
            bool need = _content > ClientSize.Height;
            _bar.Visible = need;
            if (!need)
            {
                _top = 0;
                return;
            }
            _bar.Minimum = 0;
            _bar.LargeChange = Math.Max(20, ClientSize.Height - 24);
            _bar.Maximum = Math.Max(0, _content - 1);
            int maxTop = Math.Max(0, _content - ClientSize.Height);
            if (_top > maxTop) _top = maxTop;
            _bar.Value = Math.Min(_top, Math.Max(0, _bar.Maximum - _bar.LargeChange + 1));
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            ScrollBy(-Math.Sign(e.Delta) * 72);
        }

        // 供 MainForm 在鼠标位于导航区时转发滚轮（焦点不一定在导航上）
        public void ScrollBy(int dy)
        {
            if (!_bar.Visible) return;
            int maxTop = Math.Max(0, _content - ClientSize.Height);
            _top += dy;
            if (_top < 0) _top = 0;
            if (_top > maxTop) _top = maxTop;
            _bar.Value = Math.Min(_top, Math.Max(0, _bar.Maximum - _bar.LargeChange + 1));
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);
            g.SetClip(new Rectangle(0, 0, Width - (_bar.Visible ? _bar.Width : 0), Height));

            foreach (NavItem it in _items)
            {
                int y = it.Y - _top;
                if (y + it.H < 0 || y > Height) continue;
                if (it.Header)
                {
                    Theme.Draw(g, it.Text, new Rectangle(14, y + 10, Width - 30, 16), Theme.Tiny, Theme.Faint, TextFormatFlags.NoPadding);
                    continue;
                }

                var r = new Rectangle(6, y, Width - 22, it.H - 4);
                bool sel = it.Meta == SelectedMeta;
                bool hov = it.Meta == SelectedMeta || IsHover(it);
                if (sel)
                {
                    using (var p = Theme.Round(r, 8))
                    using (var b = new SolidBrush(Theme.BlueSoft)) g.FillPath(b, p);
                    using (var p2 = Theme.Round(new Rectangle(r.Left, r.Top + 10, 3, r.Height - 20), 2))
                    using (var b2 = new SolidBrush(Theme.Blue)) g.FillPath(b2, p2);
                }
                else if (hov)
                {
                    using (var p = Theme.Round(r, 8))
                    using (var b = new SolidBrush(Color.FromArgb(246, 249, 253))) g.FillPath(b, p);
                }

                int x = 18;
                if (it.Meta.Star > 0)
                {
                    Theme.Draw(g, "★", new Rectangle(x, y + 9, 14, 16), Theme.Small, Theme.Orange, TextFormatFlags.NoPadding);
                    x += 16;
                }
                Theme.Draw(g, it.Text, new Rectangle(x, y + 7, r.Width - 30, 18), Theme.Base, sel ? Theme.BlueDeep : Theme.Text, TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
                Theme.Draw(g, it.Right, new Rectangle(x, y + 26, r.Width - 30, 16), Theme.Tiny, Theme.Faint, TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
                if (it.Meta.Star == 0)
                    Theme.Draw(g, "自动", new Rectangle(r.Right - 42, y + 8, 36, 14), Theme.Tiny, Theme.Faint, TextFormatFlags.Right | TextFormatFlags.NoPadding);
            }
        }

        private bool IsHover(NavItem it) { return _hover >= 0 && _hover < _items.Count && _items[_hover] == it; }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int idx = Hit(e.Y);
            if (idx != _hover) { _hover = idx; Cursor = idx >= 0 && _items[idx].Meta != null ? Cursors.Hand : Cursors.Default; Invalidate(); }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hover != -1) { _hover = -1; Invalidate(); }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            int idx = Hit(e.Y);
            if (idx >= 0 && _items[idx].Meta != null && Pick != null) Pick(_items[idx].Meta);
        }

        private int Hit(int y)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                NavItem it = _items[i];
                int top = it.Y - _top;
                if (y >= top && y < top + it.H) return it.Header ? -1 : i;
            }
            return -1;
        }
    }
}
