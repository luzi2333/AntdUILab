using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using AntdUILab.App;

namespace AntdUILab.Ui
{
    /// <summary>分类标题行：像 VS 属性窗口一样，用一条灰色分组条把成员切开。</summary>
    public class CategoryRow : Panel
    {
        public string Key;
        public int Count;

        public CategoryRow(string key)
        {
            Key = key;
            Height = 32;
            BackColor = Theme.Bg;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Theme.Bg);
            using (var pen = new Pen(Theme.LineSoft)) g.DrawLine(pen, 0, Height - 1, Width, Height - 1);
            Theme.Draw(g, Categories.Label(Key), new Rectangle(12, 0, Width - 70, Height),
                Theme.SmallBold, Theme.Sub, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            if (Count > 0)
                Theme.Draw(g, Count.ToString(), new Rectangle(Width - 58, 0, 40, Height),
                    Theme.Tiny, Theme.Faint, TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }

    /// <summary>属性行：左边中英说明，右边按类型自动生成的编辑器。</summary>
    public class PropRow : Panel
    {
        public PropertyInfo Info;
        public Doc D;
        public string ValueSummary = "";
        public bool Complex;
        public bool Curated;
        public string Category = "其他";
        public Control Editor;

        private bool _hover;

        public PropRow(PropertyInfo p, Doc doc)
        {
            Info = p;
            D = doc;
            Height = 76;
            BackColor = Color.White;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (Editor != null) Editor.SetBounds(Math.Max(120, Width - 140), 24, 128, 28);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            using (var b = new SolidBrush(_hover ? Theme.BlueSoft : Color.White)) g.FillRectangle(b, ClientRectangle);
            using (var pen = new Pen(Theme.LineSoft)) g.DrawLine(pen, 0, Height - 1, Width, Height - 1);
            if (Curated)
            {
                using (var b = new SolidBrush(Theme.Blue)) g.FillRectangle(b, 0, 8, 3, Height - 16);
            }

            int textW = Math.Max(60, Width - 160);
            string name = D.Zh.Length > 0 ? D.Zh : D.En;
            string en = D.En.Length > 0 && !string.Equals(D.En, name, StringComparison.OrdinalIgnoreCase) ? D.En : "";
            int w1 = TextRenderer.MeasureText(g, name, Theme.Bold).Width;
            Theme.Draw(g, name, new Rectangle(12, 8, textW, 20), Theme.Bold, Theme.Text, TextFormatFlags.NoPadding);
            if (en.Length > 0)
                Theme.Draw(g, en, new Rectangle(12 + w1 + 8, 10, Math.Max(10, textW - w1 - 8), 18), Theme.Small, Theme.Faint, TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);

            string zhEffect = D.EffectZh;
            if (Complex && ValueSummary.Length > 0) zhEffect = ValueSummary + (zhEffect.Length > 0 ? " · " + zhEffect : "");
            if (zhEffect.Length == 0 && string.IsNullOrEmpty(D.EffectEn))
                Theme.Draw(g, "官方无注释 · 改值后看舞台效果", new Rectangle(12, 28, textW, 18), Theme.Small, Theme.Faint, TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
            else
                Theme.Draw(g, zhEffect, new Rectangle(12, 28, textW, 18), Theme.Small, Theme.Sub, TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
            Theme.Draw(g, D.EffectEn, new Rectangle(12, 44, textW, 16), Theme.Tiny, Theme.Faint, TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);

            if (!string.IsNullOrEmpty(D.Pit))
                Theme.Draw(g, "注意：" + D.Pit, new Rectangle(12, 59, textW, 14), Theme.Tiny, Theme.Red, TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);

            string type = R.Friendly(Info.PropertyType);
            int tw = Math.Min(140, TextRenderer.MeasureText(g, type, Theme.Tiny).Width + 12);
            var tr = new Rectangle(Math.Max(4, Width - 140), 4, tw, 16);
            if (!Complex)
            {
                using (var b = new SolidBrush(Theme.LineSoft)) g.FillRectangle(b, tr);
                Theme.Draw(g, type, tr, Theme.Tiny, Theme.Sub, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
            }
            else
            {
                Theme.Draw(g, "需代码", tr, Theme.Tiny, Theme.Orange, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }
        }
    }

    /// <summary>事件行：勾选框 + 名称/签名 + 触发时机说明。</summary>
    public class EventRow : Panel
    {
        public EventInfo Info;
        public Doc D;
        public bool Curated;
        public string Category = "属性已更改";
        public CheckBox Check;

        // 实时仪表盘：触发次数、上一次时间、闪烁、最近参数
        public int Count;
        public DateTime Last;
        public DateTime FlashUntil;
        public string LastArgs = "";

        private bool _hover;

        public void Hit(string args)
        {
            Count++;
            Last = DateTime.Now;
            FlashUntil = Last.AddMilliseconds(900);
            if (!string.IsNullOrEmpty(args)) LastArgs = args;
            Invalidate();
        }

        public void ResetCount()
        {
            Count = 0;
            Last = DateTime.MinValue;
            FlashUntil = DateTime.MinValue;
            LastArgs = "";
            Invalidate();
        }

        /// <summary>「0.3s 前 / 12s 前 / 3 分前」。</summary>
        public string Ago()
        {
            if (Count == 0) return "";
            double ms = (DateTime.Now - Last).TotalMilliseconds;
            if (ms < 1200) return ms.ToString("0") + "ms 前";
            if (ms < 60000) return (ms / 1000).ToString("0.#") + "s 前";
            return (ms / 60000).ToString("0") + " 分前";
        }

        public EventRow(EventInfo ev, Doc doc, bool on)
        {
            Info = ev;
            D = doc;
            Height = 76;
            BackColor = Color.White;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            Check = new CheckBox
            {
                Text = "",
                AutoSize = false,
                Size = new Size(16, 16),
                Location = new Point(12, 9),
                Checked = on,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };
            Check.MouseEnter += (s, e) => { _hover = true; Invalidate(); };
            Check.MouseLeave += (s, e) => { _hover = false; Invalidate(); };
            Controls.Add(Check);
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Check.Checked = !Check.Checked;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            bool flash = DateTime.Now < FlashUntil;
            Color back = flash ? Theme.BlueSoft : (_hover ? Theme.BlueSoft : Color.White);
            using (var b = new SolidBrush(back)) g.FillRectangle(b, ClientRectangle);
            using (var pen = new Pen(Theme.LineSoft)) g.DrawLine(pen, 0, Height - 1, Width, Height - 1);
            if (Curated)
            {
                using (var b = new SolidBrush(Theme.Blue)) g.FillRectangle(b, 0, 8, 3, Height - 16);
            }

            int x = 38;
            int statW = Count > 0 ? 96 : 0;
            int w = Math.Max(60, Width - x - 12 - statW);
            string name = Info.Name;
            int nw = TextRenderer.MeasureText(g, name, Theme.Bold).Width;
            Theme.Draw(g, name, new Rectangle(x, 8, w, 20), Theme.Bold, Theme.Text, TextFormatFlags.NoPadding);
            if (D.Zh.Length > 0)
                Theme.Draw(g, D.Zh, new Rectangle(x + nw + 8, 10, Math.Max(20, w - nw - 8), 18), Theme.Small, Theme.Faint, TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);

            string sig = Info.EventHandlerType == null ? "" : Info.EventHandlerType.Name;
            if (Check.Checked)
                Theme.Draw(g, "● 已订阅  " + sig, new Rectangle(x, 28, w, 16), Theme.Tiny, Theme.Green, TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
            else
                Theme.Draw(g, "○ 未订阅  " + sig, new Rectangle(x, 28, w, 16), Theme.Tiny, Theme.Blue, TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
            string when = string.IsNullOrEmpty(D.EffectZh) ? "官方无注释 · 勾选后操作控件观察" : D.EffectZh;
            Theme.Draw(g, when, new Rectangle(x, 46, w, 16), Theme.Small, Theme.Sub, TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
            if (Count > 0 && LastArgs.Length > 0)
                Theme.Draw(g, "最近：" + LastArgs, new Rectangle(x, 61, w, 13), Theme.Tiny, Theme.BlueDeep, TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);
            else if (!string.IsNullOrEmpty(D.Pit))
                Theme.Draw(g, "注意：" + D.Pit, new Rectangle(x, 61, w, 13), Theme.Tiny, Theme.Red, TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);

            // 右侧实时计数：×N / 多久以前
            if (Count > 0)
            {
                var right = new Rectangle(Width - 108, 8, 96, 20);
                Theme.Draw(g, "×" + Count, right, Theme.Bold, Theme.Blue, TextFormatFlags.Right | TextFormatFlags.NoPadding);
                Theme.Draw(g, Ago(), new Rectangle(Width - 108, 30, 96, 14), Theme.Tiny, Theme.Faint, TextFormatFlags.Right | TextFormatFlags.NoPadding);
            }
            else if (Check.Checked)
            {
                Theme.Draw(g, "待触发", new Rectangle(Width - 108, 10, 96, 16), Theme.Tiny, Theme.Faint, TextFormatFlags.Right | TextFormatFlags.NoPadding);
            }
        }
    }

    /// <summary>右侧检查器：属性 / 事件 / 代码 三个页签。</summary>
    public class Inspector : Panel
    {
        public event Action<PropertyInfo, string> PropertyApplied;
        public event Action<string, string> Notify;
        public EventHub Hub;

        private readonly TabStrip _tabs;
        private readonly Panel _propPage;
        private readonly Panel _eventPage;
        private readonly Panel _codePage;
        private readonly TextBox _propSearch;
        private readonly TextBox _eventSearch;
        private readonly Panel _propList;
        private readonly Panel _eventList;
        private readonly RichTextBox _code;
        private readonly Label _codeHint;

        private readonly List<PropRow> _propRows = new List<PropRow>();
        private readonly List<EventRow> _eventRows = new List<EventRow>();
        private readonly List<CategoryRow> _propCats = new List<CategoryRow>();
        private readonly List<CategoryRow> _eventCats = new List<CategoryRow>();
        private readonly ToolTip _tip = new ToolTip { AutoPopDelay = 30000, InitialDelay = 350, ReshowDelay = 80, ShowAlways = true };

        private object _ctl;
        private Meta _meta;
        private string _propQuery = "";
        private string _eventQuery = "";
        private bool _curatedOnly;
        private FlatButton _curatedBtn;

        // 事件实时仪表盘
        private readonly GesturePanel _gesture;
        private readonly Panel _eventBar;
        private readonly Panel _eventWrap;
        private readonly FlatButton _subCuratedBtn;
        private readonly FlatButton _unsubAllBtn;
        private readonly FlatButton _resetCountBtn;
        private readonly FlatButton _firedOnlyBtn;
        private bool _firedOnly;
        private readonly Timer _uiTimer;

        public Inspector()
        {
            BackColor = Color.White;
            DoubleBuffered = true;

            _tabs = new TabStrip { Dock = DockStyle.Top };
            _tabs.SetItems("属性 Properties", "事件 Events", "代码 Code");
            _tabs.SelectedChanged += (s, e) => ShowPage(_tabs.Selected);

            _codePage = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Visible = false };
            _propPage = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            _eventPage = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Visible = false };
            Controls.Add(_codePage);
            Controls.Add(_eventPage);
            Controls.Add(_propPage);
            Controls.Add(_tabs);

            // ---------- 属性页 ----------
            _propList = new Panel { Dock = DockStyle.None, AutoScroll = true, BackColor = Color.White };
            Panel propWrap;
            _propSearch = MakeSearch("搜索属性 / filter properties…", out propWrap);
            propWrap.Dock = DockStyle.None;
            _propPage.Controls.Add(_propList);
            _propPage.Controls.Add(propWrap);
            _propSearch.TextChanged += (s, e) => { _propQuery = Query(_propSearch); FilterProps(); };
            _curatedBtn = new FlatButton
            {
                Dock = DockStyle.Right,
                Width = 86,
                Text = "只看精讲",
                Radius = 6,
                Font = Theme.Small,
                FillColor = Color.White,
                HoverColor = Theme.BlueSoft,
                TextColor = Theme.Sub
            };
            _curatedBtn.Click += (s, e) =>
            {
                _curatedOnly = !_curatedOnly;
                UpdateCuratedBtn();
                FilterProps();
                FilterEvents();
            };
            propWrap.Controls.Add(_curatedBtn);
            UpdateCuratedBtn();
            _propPage.Resize += (s, e) =>
            {
                propWrap.Bounds = new Rectangle(0, 0, _propPage.Width, 46);
                _propList.Bounds = new Rectangle(0, 46, _propPage.Width, Math.Max(0, _propPage.Height - 46));
            };

            // ---------- 事件页 ----------
            _eventList = new Panel { Dock = DockStyle.None, AutoScroll = true, BackColor = Color.White };
            Panel evWrap;
            _eventSearch = MakeSearch("搜索事件 / filter events…", out evWrap);
            evWrap.Dock = DockStyle.None;
            _eventWrap = evWrap;

            _eventBar = new Panel { Dock = DockStyle.None, BackColor = Color.White };
            _eventBar.Paint += (s, e) =>
            {
                using (var pen = new Pen(Theme.LineSoft)) e.Graphics.DrawLine(pen, 0, _eventBar.Height - 1, _eventBar.Width, _eventBar.Height - 1);
            };
            _subCuratedBtn = MakeTool("订阅精讲", 82);
            _unsubAllBtn = MakeTool("全部取消", 82);
            _resetCountBtn = MakeTool("清空计数", 82);
            _firedOnlyBtn = MakeTool("只看已触发", 98);
            _eventBar.Controls.Add(_subCuratedBtn);
            _eventBar.Controls.Add(_unsubAllBtn);
            _eventBar.Controls.Add(_resetCountBtn);
            _eventBar.Controls.Add(_firedOnlyBtn);
            _subCuratedBtn.Click += (s, e) => SubscribeCurated();
            _unsubAllBtn.Click += (s, e) => UnsubscribeAll();
            _resetCountBtn.Click += (s, e) =>
            {
                foreach (EventRow r in _eventRows) r.ResetCount();
                _gesture.Clear();
                if (_gesture != null) _gesture.Invalidate();
                if (_firedOnly) FilterEvents();
                if (Notify != null) Notify("事件计数", "已清空所有计数与手势时间轴");
            };
            _firedOnlyBtn.Click += (s, e) =>
            {
                _firedOnly = !_firedOnly;
                UpdateFiredBtn();
                FilterEvents();
            };

            _gesture = new GesturePanel { Dock = DockStyle.None };

            _eventPage.Controls.Add(_eventList);
            _eventPage.Controls.Add(_gesture);
            _eventPage.Controls.Add(_eventBar);
            _eventPage.Controls.Add(evWrap);
            _eventSearch.TextChanged += (s, e) => { _eventQuery = Query(_eventSearch); FilterEvents(); };
            _eventPage.Resize += (s, e) => LayoutEventPage();

            _uiTimer = new Timer { Interval = 250 };
            _uiTimer.Tick += (s, e) => TickUi();
            _uiTimer.Start();

            // ---------- 代码页 ----------
            var codeBar = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.White };
            _codeHint = new Label
            {
                Text = "当前控件配置 + 已勾选事件，可直接粘贴到你的 Form。",
                Font = Theme.Small,
                ForeColor = Theme.Sub,
                Location = new Point(14, 6),
                Size = new Size(270, 44)
            };
            var copy = new FlatButton { Text = "复制代码", Location = new Point(292, 10), Size = new Size(96, 32) };
            copy.Click += (s, e) =>
            {
                try
                {
                    if (_code.TextLength > 0) Clipboard.SetText(_code.Text);
                    if (Notify != null) Notify("复制代码", "已复制到剪贴板（" + _code.TextLength + " 字符）");
                }
                catch (Exception ex)
                {
                    if (Notify != null) Notify("复制失败", ex.Message);
                }
            };
            codeBar.Controls.Add(copy);
            codeBar.Controls.Add(_codeHint);
            codeBar.Resize += (s, e) =>
            {
                copy.Bounds = new Rectangle(Math.Max(4, codeBar.Width - 110), 12, 96, 32);
                _codeHint.Size = new Size(Math.Max(80, codeBar.Width - 128), 44);
            };
            _code = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.DarkBg,
                ForeColor = Theme.DarkText,
                Font = Theme.Mono,
                BorderStyle = BorderStyle.None,
                WordWrap = false,
                ReadOnly = true,
                DetectUrls = false,
                ScrollBars = RichTextBoxScrollBars.Both
            };
            _codePage.Controls.Add(_code);
            _codePage.Controls.Add(codeBar);
        }

        private static void EnsureCategory(List<CategoryRow> cats, Panel host, string cat, ref string lastCat, ref int y)
        {
            if (cat == lastCat) return;
            var h = new CategoryRow(cat) { Width = Math.Max(200, host.ClientSize.Width - 20) };
            h.Location = new Point(0, y);
            cats.Add(h);
            host.Controls.Add(h);
            y += h.Height;
            lastCat = cat;
        }

        private void LayoutPropRows()
        {
            int y = 0;
            string last = null;
            var shown = new HashSet<string>();
            var counts = new Dictionary<string, int>();
            foreach (PropRow r in _propRows)
                if (r.Visible) { int c; counts.TryGetValue(r.Category, out c); counts[r.Category] = c + 1; }
            foreach (PropRow r in _propRows)
            {
                if (!r.Visible) continue;
                if (r.Category != last)
                {
                    CategoryRow h = _propCats.Find(x => x.Key == r.Category);
                    if (h != null)
                    {
                        h.Visible = true;
                        h.Count = counts[r.Category];
                        h.Width = Math.Max(200, _propList.ClientSize.Width - 20);
                        h.Bounds = new Rectangle(0, y, h.Width, h.Height);
                        y += h.Height;
                        shown.Add(h.Key);
                    }
                    last = r.Category;
                }
                r.Location = new Point(0, y);
                y += r.Height;
            }
            foreach (CategoryRow h in _propCats) if (!shown.Contains(h.Key)) h.Visible = false;
        }

        private void LayoutEventRows()
        {
            int y = 0;
            string last = null;
            var shown = new HashSet<string>();
            var counts = new Dictionary<string, int>();
            foreach (EventRow r in _eventRows)
                if (r.Visible) { int c; counts.TryGetValue(r.Category, out c); counts[r.Category] = c + 1; }
            foreach (EventRow r in _eventRows)
            {
                if (!r.Visible) continue;
                if (r.Category != last)
                {
                    CategoryRow h = _eventCats.Find(x => x.Key == r.Category);
                    if (h != null)
                    {
                        h.Visible = true;
                        h.Count = counts[r.Category];
                        h.Width = Math.Max(200, _eventList.ClientSize.Width - 20);
                        h.Bounds = new Rectangle(0, y, h.Width, h.Height);
                        y += h.Height;
                        shown.Add(h.Key);
                    }
                    last = r.Category;
                }
                r.Location = new Point(0, y);
                y += r.Height;
            }
            foreach (CategoryRow h in _eventCats) if (!shown.Contains(h.Key)) h.Visible = false;
        }

        private void LayoutPages()
        {
            if (_propPage.Width > 0)
            {
                _propList.Bounds = new Rectangle(0, 46, _propPage.Width, Math.Max(0, _propPage.Height - 46));
                foreach (PropRow r in _propRows) r.Width = Math.Max(200, _propList.ClientSize.Width - 20);
            }
            if (_eventPage.Width > 0)
            {
                LayoutEventPage();
                foreach (EventRow r in _eventRows) r.Width = Math.Max(200, _eventList.ClientSize.Width - 20);
            }
            LayoutPropRows();
            LayoutEventRows();
        }

        /// <summary>事件页：搜索条 46 + 工具条 38 + 列表 + 手势时间轴（底部约 1/3）。</summary>
        private void LayoutEventPage()
        {
            if (_eventPage == null || _eventPage.Width <= 0) return;
            int w = _eventPage.Width, h = _eventPage.Height;
            _eventWrap.Bounds = new Rectangle(0, 0, w, 46);
            _eventBar.Bounds = new Rectangle(0, 46, w, 38);

            int x = 12, y = 7;
            int[] widths = { _subCuratedBtn.Width, _unsubAllBtn.Width, _resetCountBtn.Width, _firedOnlyBtn.Width };
            FlatButton[] btns = { _subCuratedBtn, _unsubAllBtn, _resetCountBtn, _firedOnlyBtn };
            for (int i = 0; i < btns.Length; i++)
            {
                btns[i].SetBounds(x, y, widths[i], 24);
                x += widths[i] + 6;
            }

            int gh = Math.Min(208, Math.Max(128, h / 3));
            _gesture.Bounds = new Rectangle(0, h - gh, w, gh);
            _eventList.Bounds = new Rectangle(0, 84, w, Math.Max(60, h - 84 - gh - 4));
        }

        private static string Query(TextBox box)
        {
            string t = box.Text.Trim();
            if (t.StartsWith("搜索")) return "";
            return t;
        }

        private TextBox MakeSearch(string hint, out Panel wrap)
        {
            wrap = new Panel { Dock = DockStyle.Top, Height = 46, BackColor = Color.White, Padding = new Padding(10, 8, 10, 8) };
            var inner = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Bg, Padding = new Padding(6, 0, 6, 0) };
            var box = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                Font = Theme.Base,
                ForeColor = Theme.Faint,
                Text = hint,
                BackColor = Theme.Bg
            };
            box.GotFocus += (s, e) =>
            {
                if (box.ForeColor == Theme.Faint)
                {
                    box.Text = "";
                    box.ForeColor = Theme.Text;
                }
            };
            box.LostFocus += (s, e) =>
            {
                if (box.Text.Trim().Length == 0)
                {
                    box.ForeColor = Theme.Faint;
                    box.Text = hint;
                }
            };
            inner.Controls.Add(box);
            wrap.Controls.Add(inner);
            return box;
        }

        private void ShowPage(int index)
        {
            _propPage.Visible = index == 0;
            _eventPage.Visible = index == 1;
            _codePage.Visible = index == 2;
            if (index == 2) RefreshCode();
            LayoutPages();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutPages();
        }

        public string DebugBounds()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("page=" + _propPage.Bounds + " list=" + _propList.Bounds + " wrap=" + (_propList.Parent == null ? "?" : ""));
            sb.Append(" rows=" + _propRows.Count);
            if (_propRows.Count > 0) sb.Append(" first=" + _propRows[0].Bounds);
            return sb.ToString();
        }

        public void SelectPage(int index)
        {
            _tabs.Selected = index;
            ShowPage(index);
            _tabs.Invalidate();
        }

        // 鼠标在属性/事件列表上滚动时，由 MainForm 把滚轮事件转发到这里。
        public void ScrollBy(int delta)
        {
            if (_tabs == null) return;
            if (_tabs.Selected == 2)
            {
                if (_code == null) return;
                int lines = Math.Max(1, Math.Abs(delta) / 120 * 3);
                int wparam = delta > 0 ? 0 : 1;      // SB_LINEUP / SB_LINEDOWN
                for (int i = 0; i < lines; i++) NativeScroll(_code, wparam);
                return;
            }
            Panel list = _tabs.Selected == 1 ? _eventList : _propList;
            if (list == null) return;
            ScrollPanel(list, -(delta / 120) * 72);   // delta<0（向下滚）→ 内容上移
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private static void NativeScroll(Control c, int sbCode)
        {
            const int WM_VSCROLL = 0x0115;
            SendMessage(c.Handle, WM_VSCROLL, (IntPtr)sbCode, IntPtr.Zero);
        }

        /// <summary>把某个分类的分组条滚到列表顶部（截图自检用，也可给"跳到分类"用）。</summary>
        public void ScrollToCategory(string key)
        {
            Panel list = _propPage != null && _propPage.Visible ? _propList : _eventList;
            List<CategoryRow> cats = _propPage != null && _propPage.Visible ? _propCats : _eventCats;
            if (list == null || cats == null) return;
            foreach (CategoryRow c in cats)
            {
                if (!c.Visible || !string.Equals(c.Key, key, StringComparison.OrdinalIgnoreCase)) continue;
                ScrollPanel(list, c.Top - 6);
                return;
            }
        }

        private static void ScrollPanel(Panel panel, int dy)
        {
            int maxTop = Math.Max(0, panel.DisplayRectangle.Height - panel.ClientSize.Height);
            int top = -panel.AutoScrollPosition.Y + dy;
            if (top < 0) top = 0;
            if (top > maxTop) top = maxTop;
            panel.AutoScrollPosition = new Point(0, top);
        }

        public void ShowControl(object ctl, Meta meta)
        {
            _ctl = ctl;
            _meta = meta;
            _firedOnly = false;
            UpdateFiredBtn();
            _gesture.Clear();
            BuildProps();
            BuildEvents();
            RefreshCode();
            LayoutPages();
        }

        private void ClearRows()
        {
            foreach (PropRow r in _propRows) { r.Parent = null; r.Dispose(); }
            _propRows.Clear();
            _propList.Controls.Clear();
            foreach (CategoryRow c in _propCats) { c.Parent = null; c.Dispose(); }
            _propCats.Clear();
            foreach (EventRow r in _eventRows) { r.Parent = null; r.Dispose(); }
            _eventRows.Clear();
            _eventList.Controls.Clear();
            foreach (CategoryRow c in _eventCats) { c.Parent = null; c.Dispose(); }
            _eventCats.Clear();
        }

        /// <summary>分类之间比较：先按 Categories.Order（布局最前），同序再按名字。</summary>
        private static int CompareCategory(string a, string b)
        {
            int oa = Categories.Order(a), ob = Categories.Order(b);
            if (oa != ob) return oa.CompareTo(ob);
            return string.Compare(a ?? "", b ?? "", StringComparison.OrdinalIgnoreCase);
        }

        private static bool InheritedWhitelist(string name)
        {
            switch (name)
            {
                case "Text":
                case "Enabled":
                case "Visible":
                case "Cursor":
                case "ForeColor":
                case "BackColor":
                // 布局 Layout：和 VS 属性窗口一样，位置/尺寸/停靠/边距都放出来（改完舞台会跟着动）
                case "Location":
                case "Size":
                case "Dock":
                case "Anchor":
                case "Margin":
                case "Padding":
                case "MinimumSize":
                case "MaximumSize":
                    return true;
            }
            return false;
        }

        private static bool InheritedEventWhitelist(string name)
        {
            switch (name)
            {
                case "Click":
                case "DoubleClick":
                case "MouseClick":
                case "MouseDoubleClick":
                case "MouseEnter":
                case "MouseLeave":
                case "MouseDown":
                case "MouseUp":
                case "MouseMove":
                case "KeyDown":
                case "KeyUp":
                case "TextChanged":
                case "EnabledChanged":
                case "VisibleChanged":
                case "SizeChanged":
                case "Resize":
                case "GotFocus":
                case "LostFocus":
                    return true;
            }
            return false;
        }

        private static int Depth(Type runtime, Type declaring)
        {
            int d = 0;
            for (Type t = runtime; t != null; t = t.BaseType, d++)
                if (t == declaring) return d;
            return 999;
        }

        private static readonly HashSet<string> Hidden = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Site", "Container", "Parent", "Handle", "IsHandleCreated", "BindingContext", "AccessibilityObject",
            "AccessibleRole", "AccessibleName", "AccessibleDescription", "Capture", "CausesValidation",
            "ClientRectangle", "ClientSize", "DisplayRectangle", "RenderRegion", "ReadRectangle", "PSize",
            "Bounds", "Top", "Left", "Right", "Bottom", "Width", "Height", "Name", "Tag",
            "TabIndex", "TabStop", "RightToLeft", "ImeMode",
            "Region", "BackgroundImage", "BackgroundImageLayout", "ContextMenuStrip", "AllowDrop",
            "AutoScroll", "AutoScrollMargin", "AutoScrollMinSize", "AutoScrollOffset", "AutoScrollPosition",
            "UseWaitCursor", "ParentForm", "TopLevelControl", "CompanyName", "ProductName", "ProductVersion",
            "IsMirrored", "Focused", "FontHeight", "IsDisposed", "Disposing", "PreferredSize",
            "ScaleChildren", "ShowFocusCues", "ShowKeyboardCues", "DoubleBuffered",
            "OriginalBackColor", "HasIcon", "HasPrefix", "HasSuffix", "HasValue", "DeviceDpi",
            "LocalizationText", "LocalizationTextTitle", "LocalizationDescription", "LocalizationSubTitle",
            "LocalizationTitle", "LocalizationPrefix", "LocalizationSuffix", "LocalizationCharacter",
            "LocalizationTooltip", "LocalizationTextDesc", "LocalizationPrefixColor", "LocalizationSuffixColor",
            "IsLink", "ShowArrow", "DisplayStyle"
        };

        /// <summary>属性页实际会展示的成员（ShotMode 的 docs 审计也用它，保证统计口径一致）。</summary>
        public static IEnumerable<PropertyInfo> VisibleProps(Type rt)
        {
            return R.PublicProps(rt)
                .Where(p => p.GetIndexParameters().Length == 0)
                .Where(p => !Hidden.Contains(p.Name))
                .Where(p => p.Name.IndexOf("Localization", StringComparison.OrdinalIgnoreCase) < 0)
                .Where(p => Loader.IsAntd(p.DeclaringType) || (p.DeclaringType == typeof(Control) && InheritedWhitelist(p.Name)))
                .Where(p => R.IsSimple(p.PropertyType) || p.PropertyType == typeof(string) || p.CanWrite)
                .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.OrderByDescending(p => Loader.IsAntd(p.DeclaringType)).ThenBy(p => Depth(rt, p.DeclaringType)).First())
                .OrderBy(p => Docs.Rank(rt, p.Name))
                .ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>事件页实际会展示的成员。</summary>
        public static IEnumerable<EventInfo> VisibleEvents(Type rt)
        {
            return R.AllEvents(rt)
                .Where(ev => Loader.IsAntd(ev.DeclaringType) || InheritedEventWhitelist(ev.Name))
                .Where(ev => ev.Name != "Disposed" && ev.Name != "BindingContextChanged" && ev.Name != "ParentChanged")
                .GroupBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.OrderByDescending(e => Loader.IsAntd(e.DeclaringType)).ThenBy(e => Depth(rt, e.DeclaringType)).First())
                .OrderBy(e => Docs.EventRank(rt, e.Name))
                .ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase);
        }

        private static string TipText(Doc d, string extra)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(d.Zh.Length > 0 ? d.Zh : d.En);
            if (d.En.Length > 0 && !string.Equals(d.En, d.Zh, StringComparison.OrdinalIgnoreCase)) sb.Append("   ").Append(d.En);
            if (d.EffectZh.Length > 0) sb.AppendLine().Append(d.EffectZh);
            if (d.EffectEn.Length > 0) sb.AppendLine().Append(d.EffectEn);
            if (!string.IsNullOrEmpty(d.Pit)) sb.AppendLine().Append("注意：").Append(d.Pit);
            if (!string.IsNullOrEmpty(extra)) sb.AppendLine().Append(extra);
            return sb.ToString();
        }

        private void BuildProps()
        {
            ClearRows();
            if (_ctl == null) return;
            Type rt = _ctl.GetType();
            List<PropertyInfo> props = VisibleProps(rt).ToList();

            // 先按「分类 → 布局组内顺序 → 精讲优先 → 名字」排好，后面分组条才有可能连续
            props.Sort((a, b) =>
            {
                string cta = Categories.Of(a, false), ctb = Categories.Of(b, false);
                int ca = CompareCategory(cta, ctb);
                if (ca != 0) return ca;
                int ma = Categories.MemberOrder(cta, a.Name), mb = Categories.MemberOrder(ctb, b.Name);
                if (ma != mb) return ma.CompareTo(mb);
                int ra = Docs.Rank(rt, a.Name), rb = Docs.Rank(rt, b.Name);
                if (ra != rb) return ra.CompareTo(rb);
                return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
            });

            int y = 0;
            string lastCat = null;
            foreach (PropertyInfo p in props)
            {
                Doc d = Docs.ForProp(rt, p.Name);
                bool simple = R.IsSimple(p.PropertyType) || p.PropertyType == typeof(string);
                Docs.FillFromXml(rt, p.Name, d, false);
                var row = new PropRow(p, d);
                row.Category = Categories.Of(p, false);
                row.Curated = Docs.Rank(rt, p.Name) != int.MaxValue;
                row.Complex = !simple;
                if (!simple)
                {
                    object coll = R.Get(_ctl, p.Name);
                    int count = R.CollectionCount(coll);
                    row.ValueSummary = coll == null ? "复杂类型" : (count > 0 ? count + " 项" : "空集合");
                }
                else
                {
                    row.ValueSummary = R.Describe(R.Get(_ctl, p.Name));
                    if (p.CanWrite) BuildEditor(row, R.Get(_ctl, p.Name));
                }
                _tip.SetToolTip(row, TipText(d, "类型：" + R.Friendly(p.PropertyType) + "   当前值：" + row.ValueSummary + "   分类：" + Categories.Label(row.Category) + FormatHint(p.PropertyType)));
                row.Width = Math.Max(200, _propList.ClientSize.Width - 20);
                EnsureCategory(_propCats, _propList, row.Category, ref lastCat, ref y);
                row.Location = new Point(0, y);
                y += row.Height;
                _propRows.Add(row);
                _propList.Controls.Add(row);
            }
            _propQuery = "";
            _propSearch.ForeColor = Theme.Faint;
            _propSearch.Text = "搜索属性 / filter properties…";
            _propList.AutoScrollPosition = new Point(0, 0);
            LayoutPages();
            LayoutPropRows();
        }

        private void BuildEditor(PropRow row, object val)
        {
            PropertyInfo p = row.Info;
            Type t = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;

            if (t == typeof(bool))
            {
                var cb = new CheckBox
                {
                    Checked = val is bool && (bool)val,
                    Text = "开 / 关",
                    Font = Theme.Tiny,
                    ForeColor = Theme.Sub,
                    AutoSize = false,
                    Size = new Size(96, 24),
                    Cursor = Cursors.Hand
                };
                cb.CheckedChanged += (s, e) => Apply(p, cb.Checked);
                AddEditor(row, cb);
            }
            else if (t == typeof(Color))
            {
                var b = new ColorButton { Value = val is Color ? (Color)val : Color.Empty, Size = new Size(138, 30) };
                b.Click += (s, e) =>
                {
                    using (var pop = new ColorPopup(val is Color ? (Color)val : Color.Empty))
                    {
                        Rectangle r = b.RectangleToScreen(b.ClientRectangle);
                        pop.ShowAt(new Point(r.Left, r.Bottom + 4));
                        if (pop.ShowDialog(FindForm()) == DialogResult.OK)
                        {
                            val = pop.Picked;
                            b.Value = pop.Picked;
                            b.Invalidate();
                            Apply(p, pop.Picked);
                        }
                    }
                };
                AddEditor(row, b);
            }
            else if (t == typeof(AnchorStyles))
            {
                var cb = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = Theme.Small, FlatStyle = FlatStyle.Standard, Cursor = Cursors.Hand };
                string[] labels = { "无 None", "靠上 Top", "靠下 Bottom", "靠左 Left", "靠右 Right", "上+左", "上+右", "下+左", "下+右", "左右拉伸", "上下拉伸", "四边" };
                AnchorStyles[] vals =
                {
                    AnchorStyles.None, AnchorStyles.Top, AnchorStyles.Bottom, AnchorStyles.Left, AnchorStyles.Right,
                    AnchorStyles.Top | AnchorStyles.Left, AnchorStyles.Top | AnchorStyles.Right,
                    AnchorStyles.Bottom | AnchorStyles.Left, AnchorStyles.Bottom | AnchorStyles.Right,
                    AnchorStyles.Left | AnchorStyles.Right, AnchorStyles.Top | AnchorStyles.Bottom,
                    AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
                };
                cb.Items.AddRange(labels);
                AnchorStyles cur = val is AnchorStyles ? (AnchorStyles)val : AnchorStyles.Top | AnchorStyles.Left;
                int idx = 0;
                for (int i = 0; i < vals.Length; i++) if (vals[i] == cur) { idx = i; break; }
                cb.SelectedIndex = idx;
                cb.SelectedIndexChanged += (s, e) =>
                {
                    if (cb.SelectedIndex >= 0) Apply(p, vals[cb.SelectedIndex]);
                };
                AddEditor(row, cb);
            }
            else if (t.IsEnum)
            {
                var cb = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = Theme.Small, FlatStyle = FlatStyle.Standard, Cursor = Cursors.Hand };
                cb.Items.AddRange(Enum.GetNames(t));
                if (val != null) cb.SelectedItem = val.ToString();
                cb.SelectedIndexChanged += (s, e) =>
                {
                    if (cb.SelectedItem != null) Apply(p, cb.SelectedItem.ToString());
                };
                AddEditor(row, cb);
            }
            else if (t == typeof(DateTime))
            {
                var tb = new TextBox { Text = val is DateTime ? ((DateTime)val).ToString("yyyy-MM-dd HH:mm:ss") : "", Font = Theme.Small, BorderStyle = BorderStyle.FixedSingle };
                WireDelayedApply(tb, p);
                AddEditor(row, tb);
            }
            else if (t == typeof(Size) || t == typeof(Point) || t == typeof(Padding))
            {
                var tb = new TextBox { Text = val == null ? "" : R.Describe(val), Font = Theme.Small, BorderStyle = BorderStyle.FixedSingle };
                WireDelayedApply(tb, p);
                AddEditor(row, tb);
            }
            else if (t == typeof(int) || t == typeof(float) || t == typeof(double) || t == typeof(decimal))
            {
                var tb = new TextBox { Text = val == null ? "" : Convert.ToString(val, System.Globalization.CultureInfo.InvariantCulture), Font = Theme.Small, BorderStyle = BorderStyle.FixedSingle };
                WireDelayedApply(tb, p);
                AddEditor(row, tb);
            }
            else if (t == typeof(string))
            {
                var tb = new TextBox { Text = val == null ? "" : val.ToString(), Font = Theme.Small, BorderStyle = BorderStyle.FixedSingle };
                string original = tb.Text;
                tb.TextChanged += (s, e) =>
                {
                    if (tb.Text == original) return;
                    original = tb.Text;
                    Apply(p, tb.Text);
                };
                AddEditor(row, tb);
            }
        }

        private void AddEditor(PropRow row, Control editor)
        {
            row.Editor = editor;
            editor.SetBounds(Math.Max(120, row.Width - 140), 24, 128, 28);
            row.Controls.Add(editor);
        }

        /// <summary>文本框编辑：回车 / 失焦 / 停手 0.5 秒，三种时机都会提交（边打边预览）。</summary>
        private void WireDelayedApply(TextBox tb, PropertyInfo p)
        {
            var timer = new Timer { Interval = 500 };
            timer.Tick += (s, e) => { timer.Stop(); TryApplyText(p, tb.Text); };
            tb.Tag = timer;
            tb.TextChanged += (s, e) =>
            {
                string err;
                R.Coerce(p.PropertyType, tb.Text, out err);
                if (err != null) { timer.Stop(); return; }   // 还没输完，先别动控件
                timer.Stop();
                timer.Start();
            };
            tb.KeyDown += (s, e) =>
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                timer.Stop();
                TryApplyText(p, tb.Text);
            };
            tb.LostFocus += (s, e) => { timer.Stop(); TryApplyText(p, tb.Text); };
            tb.Disposed += (s, e) => { timer.Stop(); timer.Dispose(); };
        }

        /// <summary>输入格式提示（悬停时显示）。</summary>
        private static string FormatHint(Type t)
        {
            t = Nullable.GetUnderlyingType(t) ?? t;
            if (t == typeof(Point)) return "   格式：x, y（如 120, 140）";
            if (t == typeof(Size)) return "   格式：宽 × 高（如 180 × 44）";
            if (t == typeof(Padding)) return "   格式：左, 上, 右, 下";
            if (t == typeof(DateTime)) return "   格式：yyyy-MM-dd HH:mm:ss";
            return "";
        }

        /// <summary>能解析就应用，解析不了（输了一半）就安静等着。</summary>
        private void TryApplyText(PropertyInfo p, string text)
        {
            if (_ctl == null) return;
            string err;
            object v = R.Coerce(p.PropertyType, text, out err);
            if (err != null) return;
            Apply(p, v);
        }

        private void Apply(PropertyInfo p, object value)
        {
            if (_ctl == null) return;
            string err = R.Set(_ctl, p.Name, value);
            if (err != null)
            {
                if (Notify != null) Notify("属性设置失败：" + p.Name, err);
                return;
            }
            object now = R.Get(_ctl, p.Name);
            if (Notify != null) Notify("属性 " + p.Name, R.Describe(now));
            RefreshCode();
            if (PropertyApplied != null) PropertyApplied(p, R.Describe(now));
        }

        private void BuildEvents()
        {
            if (_ctl == null) return;
            Type rt = _ctl.GetType();
            List<EventInfo> evs = VisibleEvents(rt).ToList();
            evs.Sort((a, b) =>
            {
                string cta = Categories.Of(a, true), ctb = Categories.Of(b, true);
                int ca = CompareCategory(cta, ctb);
                if (ca != 0) return ca;
                int ra = Docs.EventRank(rt, a.Name), rb = Docs.EventRank(rt, b.Name);
                if (ra != rb) return ra.CompareTo(rb);
                return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
            });
            int y = 0;
            string lastCat = null;
            foreach (EventInfo ev in evs)
            {
                Doc d = Docs.ForEvent(rt, ev.Name);
                Docs.FillFromXml(rt, ev.Name, d, true);
                if (string.IsNullOrEmpty(d.EffectEn) && d.EffectZh.Length == 0)
                    d.EffectEn = "Raised by " + rt.Name + ".";
                bool on = Hub != null && Hub.IsOn(ev.Name);
                var row = new EventRow(ev, d, on);
                row.Category = Categories.Of(ev, true);
                row.Curated = Docs.EventRank(rt, ev.Name) != int.MaxValue;
                row.Check.CheckedChanged += (s, e) =>
                {
                    bool want = row.Check.Checked;
                    if (Hub == null) return;
                    string err = Hub.Toggle(ev.Name, want);
                    if (err != null)
                    {
                        if (Notify != null) Notify("订阅事件失败：" + ev.Name, err);
                        row.Check.Checked = !want;
                        return;
                    }
                    if (Notify != null) Notify(want ? "订阅事件" : "取消事件", ev.Name + " · " + d.Zh + (d.EffectZh.Length > 0 ? " — " + d.EffectZh : ""));
                    RefreshCode();
                };
                _tip.SetToolTip(row, TipText(d, "委托：" + (ev.EventHandlerType == null ? "" : ev.EventHandlerType.Name) + "   分类：" + Categories.Label(row.Category)));
                row.Width = Math.Max(200, _eventList.ClientSize.Width - 20);
                EnsureCategory(_eventCats, _eventList, row.Category, ref lastCat, ref y);
                row.Location = new Point(0, y);
                y += row.Height;
                _eventRows.Add(row);
                _eventList.Controls.Add(row);
            }
            _eventQuery = "";
            _eventSearch.ForeColor = Theme.Faint;
            _eventSearch.Text = "搜索事件 / filter events…";
            _eventList.AutoScrollPosition = new Point(0, 0);
            LayoutPages();
            LayoutEventRows();
        }

        private void FilterProps()
        {
            foreach (PropRow r in _propRows)
            {
                r.Visible = (!_curatedOnly || r.Curated)
                            && (_propQuery.Length == 0
                            || (r.Info.Name + " " + r.D.Zh + " " + r.D.En + " " + r.D.EffectZh + " " + r.D.EffectEn)
                               .IndexOf(_propQuery, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            LayoutPropRows();
        }

        private void FilterEvents()
        {
            foreach (EventRow r in _eventRows)
            {
                r.Visible = (!_curatedOnly || r.Curated)
                            && (!_firedOnly || r.Count > 0)
                            && (_eventQuery.Length == 0
                            || (r.Info.Name + " " + r.D.Zh + " " + r.D.En + " " + r.D.EffectZh + " " + r.D.EffectEn)
                               .IndexOf(_eventQuery, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            LayoutEventRows();
        }

        private void UpdateCuratedBtn()
        {
            if (_curatedBtn == null) return;
            _curatedBtn.Text = _curatedOnly ? "★ 只看精讲" : "只看精讲";
            _curatedBtn.FillColor = _curatedOnly ? Theme.Blue : Color.White;
            _curatedBtn.HoverColor = _curatedOnly ? Theme.BlueDeep : Theme.BlueSoft;
            _curatedBtn.TextColor = _curatedOnly ? Color.White : Theme.Sub;
            _curatedBtn.Outline = true;
            _curatedBtn.Invalidate();
        }


        private FlatButton MakeTool(string text, int width)
        {
            return new FlatButton
            {
                Text = text,
                Size = new Size(width, 24),
                Radius = 5,
                Font = Theme.Tiny,
                FillColor = Color.White,
                HoverColor = Theme.BlueSoft,
                TextColor = Theme.Sub,
                Outline = true
            };
        }

        private void UpdateFiredBtn()
        {
            if (_firedOnlyBtn == null) return;
            _firedOnlyBtn.FillColor = _firedOnly ? Theme.Blue : Color.White;
            _firedOnlyBtn.HoverColor = _firedOnly ? Theme.BlueDeep : Theme.BlueSoft;
            _firedOnlyBtn.TextColor = _firedOnly ? Color.White : Theme.Sub;
            _firedOnlyBtn.Text = _firedOnly ? "★ 只看已触发" : "只看已触发";
            _firedOnlyBtn.Invalidate();
        }

        private void SubscribeCurated()
        {
            if (Hub == null || _ctl == null) return;
            int n = 0;
            foreach (EventRow r in _eventRows)
            {
                if (!r.Curated || r.Check.Checked) continue;
                r.Check.Checked = true;
                n++;
            }
            if (Notify != null) Notify("订阅事件", n == 0 ? "精讲事件已全部订阅" : "已订阅 " + n + " 个精讲事件");
        }

        private void UnsubscribeAll()
        {
            int n = 0;
            foreach (EventRow r in _eventRows)
            {
                if (!r.Check.Checked) continue;
                r.Check.Checked = false;
                n++;
            }
            if (Notify != null) Notify("取消事件", n == 0 ? "当前没有订阅的事件" : "已取消 " + n + " 个事件订阅");
        }

        private void TickUi()
        {
            if (_eventPage == null || !_eventPage.Visible) return;
            foreach (EventRow r in _eventRows)
                if (r.Count > 0) r.Invalidate();
        }

        /// <summary>收到一次事件触发：给对应行计数/闪烁，并写进手势时间轴。</summary>
        public void OnEventFired(EventEntry e)
        {
            if (e == null || string.IsNullOrEmpty(e.Title)) return;
            foreach (EventRow r in _eventRows)
            {
                if (string.Equals(r.Info.Name, e.Title, StringComparison.OrdinalIgnoreCase))
                {
                    r.Hit(e.Detail);
                    break;
                }
            }
            _gesture.Add(e);
            if (_firedOnly) FilterEvents();
        }

        /// <summary>演示 / 自检用：订阅鼠标类事件，然后往控件里发一条合成点击，看仪表盘与手势轴。</summary>
        public void SubscribeDemo()
        {
            if (_ctl == null || Hub == null) return;
            string[] names = { "MouseEnter", "MouseLeave", "MouseDown", "MouseUp", "Click", "MouseClick" };
            foreach (EventRow r in _eventRows)
            {
                foreach (string n in names)
                {
                    if (!string.Equals(r.Info.Name, n, StringComparison.OrdinalIgnoreCase)) continue;
                    if (!r.Check.Checked) r.Check.Checked = true;
                    break;
                }
            }
        }

        public void FireDemoClick()
        {
            var c = _ctl as Control;
            if (c == null) return;
            IntPtr h = c.Handle;
            int x = Math.Max(4, c.Width / 2), y = Math.Max(4, c.Height / 2);
            IntPtr lp = (IntPtr)((y << 16) | x);
            SendMessage(h, 0x0200, IntPtr.Zero, lp);
            SendMessage(h, 0x0201, (IntPtr)1, lp);
            SendMessage(h, 0x0202, IntPtr.Zero, lp);
        }

        /// <summary>舞台拖动 / 缩放后，把「布局」里的 Location、Size 行刷成最新值。</summary>
        public void RefreshValues()
        {
            if (_ctl == null) return;
            foreach (PropRow r in _propRows)
            {
                var tb = r.Editor as TextBox;
                if (tb == null || r.Info == null || tb.Focused) continue;
                string n = r.Info.Name;
                if (n != "Location" && n != "Size") continue;
                object v = null;
                try { v = R.Get(_ctl, n); }
                catch { }
                string s = v == null ? "" : R.Describe(v);
                if (tb.Text != s) tb.Text = s;
                r.Invalidate();
            }
        }

        public void RefreshCode()
        {
            if (_code == null) return;
            string code = _ctl == null ? "// 未选择控件" : CodeGen.Build(_ctl, _meta, Hub == null ? null : Hub.Subscribed);
            _code.Text = code;
            _codeHint.Text = "当前控件配置 + 已勾选事件（" + (Hub == null ? 0 : Hub.Subscribed.Count) + " 个），可直接粘贴到你的 Form。";
        }
    }
}
