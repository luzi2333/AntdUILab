using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using AntdUILab.App;

namespace AntdUILab.Ui
{
    /// <summary>中间预览区：标题说明 + 尺寸/场景工具栏 + 舞台 + 变体墙。</summary>
    public class PreviewPane : Panel
    {
        public event Action<string, string> Notify;

        public readonly StagePanel Stage;

        private readonly HeaderView _head;
        private readonly Panel _toolbar;
        private readonly Panel _wall;
        private readonly List<FlatButton> _sizeChips = new List<FlatButton>();
        private readonly List<FlatButton> _sceneChips = new List<FlatButton>();
        private readonly FlatButton _wallToggle;
        private readonly FlatButton _dragChip;
        private readonly Label _wallTitle;
        private readonly Label _wallHint;

        private Meta _meta;
        private object _ctl;
        private int _scene = 0;

        public PreviewPane()
        {
            BackColor = Color.White;
            DoubleBuffered = true;

            _head = new HeaderView { Dock = DockStyle.Top, Height = 104 };

            _toolbar = new Panel { Dock = DockStyle.Top, Height = 46, BackColor = Color.White };
            _toolbar.Paint += (s, e) =>
            {
                using (var pen = new Pen(Theme.LineSoft)) e.Graphics.DrawLine(pen, 0, _toolbar.Height - 1, _toolbar.Width, _toolbar.Height - 1);
            };

            _wallTitle = new Label { Text = "变体墙", Font = Theme.SmallBold, ForeColor = Theme.Text, AutoSize = true, Location = new Point(16, 10) };
            _wallHint = new Label
            {
                Text = "同一个控件的枚举属性取不同值时的样子（点右侧「变体墙」开关显示/隐藏）",
                Font = Theme.Tiny,
                ForeColor = Theme.Faint,
                AutoSize = false,
                Location = new Point(190, 12),
                Size = new Size(560, 16),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
            };
            _wall = new Panel { Dock = DockStyle.Bottom, Height = 168, BackColor = Color.FromArgb(250, 251, 253), Visible = false };
            _wall.Controls.Add(_wallHint);
            _wall.Controls.Add(_wallTitle);
            _wall.Paint += (s, e) =>
            {
                using (var pen = new Pen(Theme.LineSoft)) e.Graphics.DrawLine(pen, 0, 0, _wall.Width, 0);
            };

            _wallToggle = new FlatButton { Text = "变体墙", Size = new Size(72, 26), Outline = true, Font = Theme.Small, Location = new Point(0, 10) };
            _wallToggle.Click += (s, e) => ToggleWall();
            _wall.Resize += (s, e) => { if (_wall.Visible) BuildWall(); };
            _toolbar.Controls.Add(_wallToggle);

            Stage = new StagePanel { Dock = DockStyle.Fill };
            _dragChip = new FlatButton { Text = "✥ 拖动", Size = new Size(72, 26), Outline = true, Font = Theme.Small, Location = new Point(0, 10) };
            _dragChip.Click += (s, e) => Stage.SetDragMode(!Stage.DragMode);
            Stage.Changed += msg => { UpdateDragChip(); if (Notify != null) Notify("舞台", msg); };
            _toolbar.Controls.Add(_dragChip);
            Controls.Add(Stage);
            Controls.Add(_wall);
            Controls.Add(_toolbar);
            Controls.Add(_head);

            _toolbar.SizeChanged += (s, e) => LayoutToolbar();
        }

        public void ToggleWall()
        {
            _wall.Visible = !_wall.Visible;
            _wallToggle.FillColor = _wall.Visible ? Theme.BlueSoft : Color.White;
            _wallToggle.TextColor = _wall.Visible ? Theme.BlueDeep : Theme.Sub;
            _wallToggle.Invalidate();
            if (_wall.Visible) BuildWall();
        }

        private void LayoutToolbar()
        {
            int w = _toolbar.ClientSize.Width;
            int x = 16;
            int y = 10;
            int rowH = 34;

            // 尺寸预设
            x = PlaceChips(_sizeChips, x, y);
            x += 26;

            // 场景：放不下就整体收起（窄窗口时优先保住尺寸、拖动、变体墙）
            int sceneW = 0;
            foreach (FlatButton b in _sceneChips) sceneW += b.Width + 6;
            bool showScene = x + sceneW + 8 < w - 96;
            foreach (FlatButton b in _sceneChips) b.Visible = showScene;
            if (showScene) { x = PlaceChips(_sceneChips, x, y); x += 26; }

            // 拖动 / 变体墙：空间不够就换到第二行
            int need = _dragChip.Width + 12 + _wallToggle.Width + 16;
            if (x + need > w && x > 16) { x = 16; y += rowH; }

            _dragChip.Location = new Point(x, y);
            _wallToggle.Location = new Point(Math.Max(x + _dragChip.Width + 12, w - _wallToggle.Width - 16), y);

            int h = y + 26 + 10;
            if (_toolbar.Height != h) _toolbar.Height = h;
        }

        private void UpdateDragChip()
        {
            bool on = Stage.DragMode;
            _dragChip.Text = on ? "✥ 拖动中" : "✥ 拖动";
            _dragChip.FillColor = on ? Theme.Blue : Color.White;
            _dragChip.TextColor = on ? Color.White : Theme.BlueDeep;
            _dragChip.Invalidate();
        }

        private static int PlaceChips(List<FlatButton> chips, int x, int y)
        {
            foreach (FlatButton b in chips)
            {
                b.Location = new Point(x, y);
                x += b.Width + 6;
            }
            return x;
        }

        public void Show(object ctl, Meta meta)
        {
            _ctl = ctl;
            _meta = meta;
            _head.Meta = meta;
            _head.Invalidate();

            foreach (FlatButton b in _sizeChips) { _toolbar.Controls.Remove(b); b.Dispose(); }
            _sizeChips.Clear();

            if (meta != null)
            {
                int i = 0;
                foreach (Size s in meta.Sizes)
                {
                    Size s1 = s;
                    var b = new FlatButton { Text = s.Width + " × " + s.Height, Size = new Size(84, 26), Outline = true, Font = Theme.Small, Index = i };
                    b.Click += (s2, e2) => ApplySize(s1);
                    _sizeChips.Add(b);
                    _toolbar.Controls.Add(b);
                    i++;
                }
            }

            if (_sceneChips.Count == 0)
            {
                AddScene("自由", 0, new Size(0, 0));
                AddScene("桌面", 1, new Size(900, 540));
                AddScene("手机", 2, new Size(360, 620));
                AddScene("弹窗", 3, new Size(480, 300));
                HighlightScenes();
            }

            LayoutToolbar();
            Stage.SceneSize = _sceneChips[_scene].Tag is Size ? (Size)_sceneChips[_scene].Tag : Size.Empty;
            if (Stage.DragMode) Stage.SetDragMode(false, true);
            UpdateDragChip();
            Stage.SetControl(ctl, meta);
            if (_wall.Visible) BuildWall();
        }

        private void AddScene(string text, int idx, Size size)
        {
            var b = new FlatButton { Text = text, Size = new Size(56, 26), Outline = true, Font = Theme.Small, Index = idx, Tag = size };
            b.Click += (s, e) =>
            {
                _scene = idx;
                Stage.SceneSize = size;
                Stage.Relayout();
                Stage.Invalidate();
                HighlightScenes();
            };
            _sceneChips.Add(b);
            _toolbar.Controls.Add(b);
        }

        private void HighlightScenes()
        {
            foreach (FlatButton b in _sceneChips)
            {
                bool on = b.Index == _scene;
                b.FillColor = on ? Theme.Blue : Color.White;
                b.TextColor = on ? Color.White : Theme.Sub;
                b.Invalidate();
            }
        }

        private void ApplySize(Size s)
        {
            if (!(_ctl is Control)) return;
            var c = (Control)_ctl;
            c.Size = s;
            Stage.Relayout();
            Stage.Invalidate();
            if (Notify != null) Notify("尺寸", c.Width + " × " + c.Height);
        }

        public void Relayout()
        {
            Stage.Relayout();
            Stage.Invalidate();
        }

        /// <summary>属性面板改了 Location / Size 之后：保留新位置，只把它夹进取景框。</summary>
        public void ApplyGeometry()
        {
            Stage.ApplyControlGeometry();
            Stage.Invalidate();
        }

        /// <summary>事件触发：舞台上脉冲一下 + 浮签显示事件名与参数。</summary>
        public void FlashEvent(string name, string info)
        {
            if (Stage != null) Stage.FlashEvent(name, info);
        }

        private bool _inWall;

        private void BuildWall()
        {
            if (_inWall) return;
            _inWall = true;
            try { BuildWallCore(); } finally { _inWall = false; }
        }

        private void BuildWallCore()
        {
            foreach (Control c in _wall.Controls.Cast<Control>().ToList())
            {
                if (c == _wallTitle || c == _wallHint) continue;
                _wall.Controls.Remove(c);
                c.Dispose();
            }
            if (_meta == null) return;

            PropertyInfo prop = FindEnumProperty();
            if (prop == null)
            {
                var none = new Label { Text = "该控件没有可视化枚举属性", Font = Theme.Small, ForeColor = Theme.Faint, Location = new Point(16, 34), AutoSize = true };
                _wall.Controls.Add(none);
                return;
            }

            _wallTitle.Text = "变体墙 · " + prop.Name;

            string[] names = Enum.GetNames(prop.PropertyType).Take(6).ToArray();
            const int cardW = 150;
            const int cardH = 98;
            const int gap = 8;
            int cols = Math.Max(1, (_wall.ClientSize.Width - 16) / (cardW + gap));
            int row = 0;
            int perRow = Math.Max(1, Math.Min(cols, names.Length));
            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];
                int col = i % perRow;
                row = i / perRow;
                int cxp = 16 + col * (cardW + gap);
                var card = new Panel { Location = new Point(cxp, 30 + row * (cardH + gap)), Size = new Size(cardW, cardH), BackColor = Color.White };
                card.Paint += (s, e) =>
                {
                    using (var pen = new Pen(Theme.LineSoft)) e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                };
                var cap = new Label { Text = name, Font = Theme.Tiny, ForeColor = Theme.Sub, Location = new Point(6, 3), AutoSize = true };
                card.Controls.Add(cap);

                try
                {
                    object inst = Catalog.New(_meta.Type);
                    if (_meta.Init != null) _meta.Init(inst);
                    R.Set(inst, prop.Name, name);
                    var cc = (Control)inst;
                    Size full = Catalog.DefaultSize(_meta);
                    Size fit = FitSize(full, cardW - 16, 60);
                    cc.Size = fit;
                    cc.Location = new Point((cardW - fit.Width) / 2, Math.Max(20, 18 + (62 - fit.Height) / 2));
                    card.Controls.Add(cc);
                }
                catch (Exception ex)
                {
                    var err = new Label { Text = ex.Message, Font = Theme.Tiny, ForeColor = Theme.Red, Location = new Point(6, 40), Size = new Size(cardW - 12, 50) };
                    card.Controls.Add(err);
                }

                _wall.Controls.Add(card);
            }
            _wall.Height = 30 + (row + 1) * (cardH + gap) + 8;
            _wallTitle.Text = "变体墙 · " + prop.Name + "（" + names.Length + " 种）";
        }

        private static Size FitSize(Size full, int maxW, int maxH)
        {
            int w = Math.Min(full.Width, maxW);
            int h = Math.Min(full.Height, maxH);
            if (full.Height > 0 && w == maxW) h = Math.Min(maxH, Math.Max(18, full.Height * w / Math.Max(1, full.Width)));
            return new Size(w, h);
        }

        private PropertyInfo FindEnumProperty()
        {
            Type t = Loader.Get(_meta.Type);
            if (t == null) return null;
            string[] prefer = { "Type", "Shape", "Status", "State", "Variant", "Align", "Placement", "Icon", "Mode" };
            foreach (string n in prefer)
            {
                PropertyInfo p = t.GetProperty(n);
                if (p != null && p.PropertyType.IsEnum && p.CanWrite && Enum.GetNames(p.PropertyType).Length >= 3)
                    return p;
            }
            return t.GetProperties().FirstOrDefault(p => p.PropertyType.IsEnum && p.CanWrite && p.DeclaringType != null && Loader.IsAntd(p.DeclaringType) && Enum.GetNames(p.PropertyType).Length >= 3);
        }
    }

    internal class HeaderView : Control
    {
        public Meta Meta;

        public HeaderView()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            BackColor = Color.White;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.White);
            if (Meta == null)
            {
                Theme.Draw(g, "AntdUI 控件实验室", new Rectangle(18, 30, Width - 36, 28), Theme.H1, Theme.Text, TextFormatFlags.NoPadding);
                Theme.Draw(g, "左侧选一个控件，右侧改属性/勾事件，底部看日志。", new Rectangle(18, 64, Width - 36, 20), Theme.Small, Theme.Faint, TextFormatFlags.NoPadding);
                return;
            }

            int x = 18;
            string badge = Meta.Star > 0 ? "★ 精讲 " + Meta.Star.ToString("00") : "自动收录";
            int bw = TextRenderer.MeasureText(g, badge, Theme.Tiny).Width + 14;
            var br = new Rectangle(x, 18, bw, 18);
            using (var b = new SolidBrush(Meta.Star > 0 ? Theme.OrangeSoft : Theme.LineSoft)) g.FillRectangle(b, br);
            Theme.Draw(g, badge, br, Theme.Tiny, Meta.Star > 0 ? Theme.Orange : Theme.Sub, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

            int w1 = TextRenderer.MeasureText(g, Meta.Zh, Theme.H1).Width;
            Theme.Draw(g, Meta.Zh, new Rectangle(x + bw + 10, 14, Math.Max(40, Width - x - bw - 20), 26), Theme.H1, Theme.Text, TextFormatFlags.NoPadding);
            Theme.Draw(g, Meta.En + "  ·  AntdUI." + Meta.Type, new Rectangle(x + bw + 12 + w1, 21, Math.Max(40, Width - x - bw - 12 - w1 - 10), 18), Theme.Small, Theme.Faint, TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);

            Theme.Draw(g, Meta.IntroZh, new Rectangle(18, 46, Width - 36, 36), Theme.Small, Theme.Sub, TextFormatFlags.NoPadding | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis);
            Theme.Draw(g, Meta.IntroEn, new Rectangle(18, 80, Width - 36, 18), Theme.Tiny, Theme.Faint, TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis);

            using (var pen = new Pen(Theme.LineSoft)) g.DrawLine(pen, 0, Height - 1, Width, Height - 1);
        }
    }
}
