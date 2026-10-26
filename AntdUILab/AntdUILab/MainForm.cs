using System;
using System.Drawing;
using System.Windows.Forms;
using AntdUILab.App;
using AntdUILab.Ui;

namespace AntdUILab
{
    public class MainForm : Form
    {
        public ControlNav Nav;
        public SplitBar NavSplit, InspSplit, LogSplit;
        public PreviewPane Preview;
        public Inspector Inspect;
        public LogPanel Log;
        public EventHub Hub;

        public MainForm()
        {
            Text = "AntdUI Lab · 控件属性与事件实验室";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1440, 900);
            MinimumSize = new Size(1180, 720);
            BackColor = Theme.Bg;
            Font = Theme.Base;
            DoubleBuffered = true;
            KeyPreview = true;

            Hub = new EventHub();

            // ---------- 顶栏 ----------
            var top = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.White };
            top.Paint += (s, e) =>
            {
                using (var pen = new Pen(Theme.Line)) e.Graphics.DrawLine(pen, 0, top.Height - 1, top.Width, top.Height - 1);
            };
            var logo = new Panel { Location = new Point(16, 14), Size = new Size(28, 28), BackColor = Color.White };
            logo.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                Theme.FillRound(g, new Rectangle(0, 0, 27, 27), 7, Theme.Blue);
                Theme.Draw(g, "A", new Rectangle(0, 0, 28, 28), Theme.Bold, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            };
            var title = new Label { Text = "AntdUI Lab", Font = Theme.H1, ForeColor = Theme.Text, Location = new Point(52, 10), AutoSize = true };
            var sub = new Label { Text = "控件属性与事件实验室 · by luzi", Font = Theme.Tiny, ForeColor = Theme.Faint, Location = new Point(54, 34), AutoSize = true };
            var ver = new Label { Text = "AntdUI " + Loader.VersionText, Font = Theme.Tiny, ForeColor = Theme.Sub, AutoSize = false, TextAlign = ContentAlignment.MiddleRight, Size = new Size(150, 18) };
            var rt = new Label
            {
                Text = ".NET Framework 4.7.2  ·  反射实时探索  ·  中 / EN",
                Font = Theme.Tiny,
                ForeColor = Theme.Faint,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleRight,
                Size = new Size(330, 18)
            };
            var help = new FlatButton
            {
                Text = "怎么用？",
                Size = new Size(76, 26),
                Radius = 6,
                FillColor = Color.White,
                HoverColor = Theme.BlueSoft,
                TextColor = Theme.BlueDeep,
                Font = Theme.Tiny,
                Outline = true,
                Cursor = Cursors.Hand
            };
            help.Click += (s, e) => { using (var f = new HelpForm()) f.ShowDialog(this); };
            top.Controls.Add(help);
            top.Controls.Add(ver);
            top.Controls.Add(rt);
            top.Controls.Add(sub);
            top.Controls.Add(title);
            top.Controls.Add(logo);
            top.Resize += (s, e) =>
            {
                rt.Location = new Point(top.Width - rt.Width - 16, 11);
                ver.Location = new Point(top.Width - rt.Width - ver.Width - 22, 11);
                help.Location = new Point(ver.Left - help.Width - 14, 14);
            };

            // ---------- 右侧区域：预览 + 检查器 + 日志 ----------
            var right = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

            Preview = new PreviewPane { Dock = DockStyle.Fill };
            Preview.Notify += (t, d) =>
            {
                Hub.Log(t, d);
                if (Inspect != null && (t == "舞台" || t == "尺寸")) Inspect.RefreshCode();
            };
            right.Controls.Add(Preview);

            // 预览 / 检查器之间的竖向分隔条：左右拖动改检查器宽度
            InspSplit = new SplitBar { Dock = DockStyle.Right };
            SplitBar inspSplit = InspSplit;
            right.Controls.Add(inspSplit);

            Inspect = new Inspector { Dock = DockStyle.Right, Width = 424 };
            Inspect.Hub = Hub;
            Inspect.Paint += (s, e) =>
            {
                using (var pen = new Pen(Theme.Line)) e.Graphics.DrawLine(pen, 0, 0, 0, Inspect.Height);
            };
            Inspect.Notify += (t, d) => Hub.Log(t, d);
            Inspect.PropertyApplied += (p, v) =>
            {
                if (p != null && (p.Name == "Location" || p.Name == "Size"))
                {
                    Preview.ApplyGeometry();
                    Inspect.RefreshValues();
                }
                else
                {
                    Preview.Relayout();
                }
            };
            right.Controls.Add(Inspect);

            // 预览 / 日志之间的横向分隔条：上下拖动改日志高度
            LogSplit = new SplitBar { Dock = DockStyle.Bottom, Vertical = false, Cursor = Cursors.HSplit };
            SplitBar logSplit = LogSplit;
            right.Controls.Add(logSplit);

            Log = new LogPanel { Dock = DockStyle.Bottom };
            Log.Hub = Hub;
            Hub.Added += Log.Append;
            // 事件触发时：检查器行计数/闪烁 + 舞台脉冲 + 手势时间轴
            Hub.Added += e =>
            {
                if (e.Category != "事件") return;
                if (Inspect != null) Inspect.OnEventFired(e);
                if (Preview != null) Preview.FlashEvent(e.Title, e.Detail);
            };
            right.Controls.Add(Log);

            // ---------- 左导航 ----------
            NavSplit = new SplitBar { Dock = DockStyle.Left, Width = 6 };
            SplitBar navSplit = NavSplit;
            Nav = new ControlNav { Dock = DockStyle.Left };
            Nav.Paint += (s, e) =>
            {
                using (var pen = new Pen(Theme.Line)) e.Graphics.DrawLine(pen, Nav.Width - 1, 0, Nav.Width - 1, Nav.Height);
            };
            Nav.Selected += Pick;

            // 舞台拖动 / 缩放后，右侧「布局」里的 Location、Size 跟着刷新
            Preview.Stage.Changed += msg => { if (Inspect != null) Inspect.RefreshValues(); };

            // 三条分隔条：拖动时改邻居的尺寸（左导航宽度 / 检查器宽度 / 日志高度）
            int navW = 0;
            navSplit.Started += () => navW = Nav.Width;
            navSplit.Moved += d =>
            {
                int w = Math.Max(180, Math.Min(480, navW + d));
                if (Nav.Width != w) Nav.Width = w;
            };
            int inspW = 0;
            inspSplit.Started += () => inspW = Inspect.Width;
            inspSplit.Moved += d =>
            {
                int w = Math.Max(320, Math.Min(760, inspW - d));
                if (Inspect.Width != w) Inspect.Width = w;
            };
            int logH = 0;
            logSplit.Started += () => logH = Log.Height;
            logSplit.Moved += d =>
            {
                int h = Math.Max(110, Math.Min(480, logH - d));
                if (Log.Height != h) Log.Height = h;
            };

            Controls.Add(right);
            Controls.Add(navSplit);
            Controls.Add(Nav);
            Controls.Add(top);

            Load += (s, e) =>
            {
                Hub.Log("系统", "AntdUI " + Loader.VersionText + " 已加载 · " + Loader.ControlTypes.Count + " 个控件类型 · 左侧选控件开始。");
                Meta first = null;
                foreach (Meta m in Catalog.All()) { if (m.Star == 1) { first = m; break; } }
                if (first == null && Catalog.Metas.Count > 0) first = Catalog.Metas[0];
                if (first != null) Pick(first);
            };
        }

        // 滚轮默认只发给焦点窗口；这里按鼠标位置转发给导航和检查器，保证"鼠标在哪就滚哪"
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            Point sp = Cursor.Position;
            if (Nav != null && Nav.Visible && Nav.RectangleToScreen(Nav.ClientRectangle).Contains(sp))
            {
                Nav.ScrollBy(-Math.Sign(e.Delta) * 72);
                return;
            }
            if (Inspect != null && Inspect.Visible && Inspect.RectangleToScreen(Inspect.ClientRectangle).Contains(sp))
            {
                Inspect.ScrollBy(e.Delta);
            }
        }

        public void Pick(Meta meta)
        {            if (meta == null) return;
            object inst = null;
            try
            {
                inst = Catalog.New(meta.Type);
                if (meta.Init != null) meta.Init(inst);
                var c = inst as Control;
                if (c != null)
                {
                    c.Size = Catalog.DefaultSize(meta);
                    c.Margin = new Padding(0);
                }
            }
            catch (Exception ex)
            {
                Hub.Log("错误", meta.Zh + " 创建失败：" + ex.GetType().Name + " " + ex.Message);
                return;
            }

            try
            {
                Hub.Bind(inst);
                Nav.Select(meta);
                Preview.Show(inst, meta);
                Inspect.ShowControl(inst, meta);
                Hub.Log("切换", meta.Zh + " · AntdUI." + meta.Type + " · " + meta.Group + (meta.Star > 0 ? " · 精讲 " + meta.Star.ToString("00") : " · 自动收录"));
                Text = "AntdUI Lab · " + meta.Zh + " " + meta.En;
            }
            catch (Exception ex)
            {
                Hub.Log("错误", "界面刷新失败：" + ex.GetType().Name + " " + ex.Message);
            }
        }
    }
}
