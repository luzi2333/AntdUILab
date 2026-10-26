using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AntdUILab.App;

namespace AntdUILab.Ui
{
    /// <summary>「怎么用」帮助窗口：一屏讲清四个区域和三条操作路径。</summary>
    public class HelpForm : Form
    {
        public HelpForm()
        {
            Text = "怎么用 AntdUI Lab";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(640, 700);
            BackColor = Color.White;
            Font = Theme.Base;

            var head = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Color.White };
            head.Paint += (s, e) =>
            {
                Theme.Draw(e.Graphics, "AntdUI Lab 使用说明", new Rectangle(20, 14, 600, 26), Theme.H2, Theme.Text, TextFormatFlags.NoPadding);
                Theme.Draw(e.Graphics, "29 个控件 · 15 个精讲 · 属性 / 事件 / 代码三件套", new Rectangle(20, 40, 600, 18), Theme.Tiny, Theme.Faint, TextFormatFlags.NoPadding);
                using (var pen = new Pen(Theme.LineSoft)) e.Graphics.DrawLine(pen, 0, 63, head.Width, 63);
            };

            var box = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 12, 20, 8) };
            var host = new Panel { Dock = DockStyle.Top, Height = 380, BackColor = Color.White };
            box.Controls.Add(host);
            int y = 0;
            y = Step(host, y, "1", "先选一个控件", "左侧列表按分组排列，带 ★ 的是 15 个精讲控件；其余 14 个自动收录。点一下，中间舞台立刻生成真实实例。");
            y = Step(host, y, "2", "在舞台上试尺寸、挪位置", "顶部 120×36 / 160×40 / 96×32 是尺寸预设；自由 / 桌面 / 手机 / 弹窗 是场景边框；「变体墙」把枚举的每个取值并排画出来。点「✥ 拖动」后按住控件就能挪位置，拖控件右下角的圆点改尺寸。三个区域之间的灰色分隔条可以拖动，调导航、检查器、日志的宽窄。");
            y = Step(host, y, "3", "右侧「属性」页改值", "属性按「布局 / 设计 / 外观 / 边框 / 徽标 / 行为 / 数据…」分组，布局排最上面，和 VS 属性窗口的分类视图一样；每行 = 中文名 + English + 作用，左侧蓝条是精讲属性。下面输入框改完立即生效并写日志，「只看精讲」可过滤。");
            y = Step(host, y, "4", "「事件」页勾选订阅", "事件按「操作 / 行为 / 鼠标…」分组。勾选后去点一下控件：该行会闪蓝并显示 ×次数 和最近一次的参数，下半部手势轴把同一次操作里的事件按顺序列出，舞台上的控件也会脉冲一圈。没订阅事件时点控件也会闪一下「收到点击」，方便确认输入到底有没有到控件上。");
            y = Step(host, y, "5", "「代码」页直接抄走", "右上角生成的就是当前控件的初始化代码（只输出被改过的属性），点「复制代码」即可粘到自己的 Form 里。");
            y = Step(host, y, "6", "两个小技巧", "鼠标悬停在属性 / 事件行上会弹出完整说明；标着「需代码」的是集合或复杂类型，只能用代码设置。颜色属性点开后是常用色板，右下「自定义…」打开系统取色器。");

            host.Height = y + 4;
            var tip = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Theme.BlueSoft };
            tip.Paint += (s, e) =>
            {
                Theme.Draw(e.Graphics, "提示：改坏了大不了再选一次控件；本程序只读 AntdUI.dll，不会改动你的代码。", new Rectangle(20, 13, 600, 20), Theme.Small, Theme.BlueDeep, TextFormatFlags.NoPadding);
            };

            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 56, BackColor = Color.White };
            bottom.Paint += (s, e) => { using (var pen = new Pen(Theme.LineSoft)) e.Graphics.DrawLine(pen, 0, 0, bottom.Width, 0); };
            var ok = new FlatButton
            {
                Text = "开始探索",
                Size = new Size(120, 34),
                Location = new Point(500, 11),
                Radius = 6,
                FillColor = Theme.Blue,
                HoverColor = Theme.BlueDeep,
                TextColor = Color.White,
                Font = Theme.SmallBold
            };
            ok.Click += (s, e) => Close();
            bottom.Controls.Add(ok);

            Controls.Add(box);
            Controls.Add(tip);
            Controls.Add(bottom);
            Controls.Add(head);
            AcceptButton = null;
            KeyPreview = true;
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };
        }

        private static int Step(Control host, int y, string no, string title, string body)
        {
            var badge = new Panel { Location = new Point(0, y + 2), Size = new Size(22, 22), BackColor = Color.White };
            badge.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Theme.FillRound(g, new Rectangle(0, 0, 21, 21), 6, Theme.Blue);
                Theme.Draw(g, no, new Rectangle(0, 0, 22, 22), Theme.Tiny, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            };
            var t = new Label { Text = title, Font = Theme.Bold, ForeColor = Theme.Text, Location = new Point(32, y), AutoSize = true };
            var b = new Label { Text = body, Font = Theme.Small, ForeColor = Theme.Sub, Location = new Point(32, y + 22), Size = new Size(548, 40) };
            host.Controls.Add(badge);
            host.Controls.Add(t);
            host.Controls.Add(b);
            int h = 76;
            if (b.GetPreferredSize(new Size(548, 0)).Height > 36) h = 92;
            b.Height = h - 26;
            return y + h;
        }
    }
}
