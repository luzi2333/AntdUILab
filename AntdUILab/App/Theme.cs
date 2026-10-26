using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AntdUILab.App
{
    /// <summary>配色、字体与绘图小工具，整个程序共用一套视觉语言。</summary>
    public static class Theme
    {
        public static readonly Color Bg = Color.FromArgb(244, 246, 249);
        public static readonly Color Card = Color.White;
        public static readonly Color Line = Color.FromArgb(230, 234, 240);
        public static readonly Color LineSoft = Color.FromArgb(241, 243, 247);
        public static readonly Color Text = Color.FromArgb(30, 34, 40);
        public static readonly Color Sub = Color.FromArgb(108, 117, 130);
        public static readonly Color Faint = Color.FromArgb(158, 166, 178);
        public static readonly Color Blue = Color.FromArgb(22, 119, 255);
        public static readonly Color BlueSoft = Color.FromArgb(233, 243, 255);
        public static readonly Color BlueDeep = Color.FromArgb(9, 88, 217);
        public static readonly Color Green = Color.FromArgb(0, 170, 60);
        public static readonly Color GreenSoft = Color.FromArgb(230, 248, 237);
        public static readonly Color Red = Color.FromArgb(240, 62, 66);
        public static readonly Color RedSoft = Color.FromArgb(255, 237, 238);
        public static readonly Color Orange = Color.FromArgb(230, 150, 20);
        public static readonly Color OrangeSoft = Color.FromArgb(255, 247, 230);
        public static readonly Color Purple = Color.FromArgb(114, 88, 214);
        public static readonly Color PurpleSoft = Color.FromArgb(240, 236, 255);
        public static readonly Color Dark = Color.FromArgb(28, 32, 40);
        public static readonly Color DarkBg = Color.FromArgb(22, 26, 33);
        public static readonly Color DarkLine = Color.FromArgb(44, 50, 60);
        public static readonly Color DarkText = Color.FromArgb(216, 222, 232);
        public static readonly Color DarkSub = Color.FromArgb(130, 140, 155);
        public static readonly Color Cyan = Color.FromArgb(19, 194, 194);

        public static readonly Font Base;
        public static readonly Font Bold;
        public static readonly Font Small;
        public static readonly Font SmallBold;
        public static readonly Font Tiny;
        public static readonly Font H1;
        public static readonly Font H2;
        public static readonly Font Mono;
        public static readonly Font MonoSmall;

        static Theme()
        {
            Base = new Font("Microsoft YaHei", 10f);
            Bold = new Font("Microsoft YaHei", 10f, FontStyle.Bold);
            Small = new Font("Microsoft YaHei", 8.5f);
            SmallBold = new Font("Microsoft YaHei", 8.5f, FontStyle.Bold);
            Tiny = new Font("Microsoft YaHei", 8f);
            H1 = new Font("Microsoft YaHei", 16f, FontStyle.Bold);
            H2 = new Font("Microsoft YaHei", 12f, FontStyle.Bold);
            Mono = new Font("Consolas", 9.5f);
            MonoSmall = new Font("Consolas", 8.5f);
        }

        public static Color Mix(Color a, Color b, double t)
        {
            t = t < 0 ? 0 : t > 1 ? 1 : t;
            return Color.FromArgb(
                (int)Math.Round(a.R + (b.R - a.R) * t),
                (int)Math.Round(a.G + (b.G - a.G) * t),
                (int)Math.Round(a.B + (b.B - a.B) * t));
        }

        public static Color Soft(Color c)
        {
            return Mix(Color.White, c, 0.10);
        }

        public static string Hex(Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        /// <summary>圆角矩形路径。</summary>
        public static GraphicsPath Round(Rectangle r, int radius)
        {
            var p = new GraphicsPath();
            if (radius <= 0)
            {
                p.AddRectangle(r);
                return p;
            }
            int d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            if (d <= 0)
            {
                p.AddRectangle(r);
                return p;
            }
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        public static void FillRound(Graphics g, Rectangle r, int radius, Color color)
        {
            using (GraphicsPath p = Round(r, radius))
            using (var b = new SolidBrush(color))
                g.FillPath(b, p);
        }

        public static void StrokeRound(Graphics g, Rectangle r, int radius, Color color, float width = 1f)
        {
            using (GraphicsPath p = Round(r, radius))
            using (var pen = new Pen(color, width))
                g.DrawPath(pen, p);
        }

        public static void Draw(Graphics g, string s, Rectangle r, Font f, Color c, TextFormatFlags flags)
        {
            TextRenderer.DrawText(g, s ?? "", f, r, c, flags);
        }

        public static string ChipText(string titleZh, string titleEn)
        {
            return titleZh + "  " + titleEn;
        }
    }
}
