using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using AntdUILab.App;

namespace AntdUILab
{
    /// <summary>--shot 模式：无人工桌面时批量截图，便于自动核对渲染效果。</summary>
    public static class ShotMode
    {
        public static bool IsShot;
        private static string _dir = "";
        public static void Run(string[] args)
        {
            string dir = args != null && args.Length > 1
                ? args[1]
                : Path.Combine(Path.GetTempPath(), "antduilab-shots");
            Directory.CreateDirectory(dir);
            _dir = dir;
            IsShot = true;

            AuditDocs(dir);

            var form = new MainForm();
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(0, 0);
            form.Size = new Size(1460, 940);
            form.Show();
            Pump(400);
            Save(form, Path.Combine(dir, "00-overview.png"));

            int index = 0;
            foreach (Meta m in Catalog.All())
            {
                if (m.Star == 0) continue;
                index++;
                form.Pick(m);
                Pump(180);
                Save(form, Path.Combine(dir, index.ToString("00") + "-" + m.Type + ".png"));
            }

            Meta sample = null;
            if (Catalog.ByName.ContainsKey("Button")) sample = Catalog.ByName["Button"];
            if (sample != null)
            {
                form.Pick(sample);
                Pump(150);

                form.Inspect.SelectPage(1);
                Pump(150);
                Save(form, Path.Combine(dir, "90-events.png"));

                form.Inspect.SelectPage(2);
                Pump(150);
                Save(form, Path.Combine(dir, "91-code.png"));
                form.Inspect.SelectPage(0);

                form.Preview.ToggleWall();
                Pump(150);
                Save(form, Path.Combine(dir, "92-wall.png"));
                form.Preview.ToggleWall();
            }

            // 事件页跑一次真实点击：仪表盘计数 + 手势时间轴 + 舞台脉冲
            if (sample != null)
            {
                form.Pick(sample);
                form.Inspect.SelectPage(1);
                Pump(150);
                form.Inspect.SubscribeDemo();
                Pump(120);
                form.Inspect.FireDemoClick();
                Pump(220);
                Save(form, Path.Combine(dir, "9a-event-fire.png"));
                form.Inspect.SelectPage(0);
            }

            // 没有事件的控件（Tag）：点一下也能看到「收到点击」的水波
            Meta tag = null;
            if (Catalog.ByName.ContainsKey("Tag")) tag = Catalog.ByName["Tag"];
            if (tag != null)
            {
                form.Pick(tag);
                form.Inspect.SelectPage(1);
                Pump(150);
                form.Inspect.FireDemoClick();
                Pump(150);
                Save(form, Path.Combine(dir, "9b-tap.png"));
                form.Inspect.SelectPage(0);
            }

            Meta table = null;
            if (Catalog.ByName.ContainsKey("Table")) table = Catalog.ByName["Table"];
            if (table != null)
            {
                form.Pick(table);
                Pump(150);
                for (int i = 0; i < 41; i++) form.Inspect.ScrollBy(-120);
                Pump(200);
                Save(form, Path.Combine(dir, "89-props-groups.png"));
                form.Inspect.SelectPage(0);
            }

            // 分隔条：把左导航 / 检查器 / 日志调到另一组宽度，确认重排没问题
            if (sample != null)
            {
                form.Pick(sample);
                form.Inspect.SelectPage(0);
                form.Nav.Width = 300;
                form.Inspect.Width = 520;
                form.Log.Height = 150;
                Pump(250);
                Save(form, Path.Combine(dir, "87-panels.png"));
                form.Nav.Width = 250;
                form.Inspect.Width = 424;
                form.Log.Height = 208;
                Pump(120);
            }

            if (sample != null)
            {
                form.Pick(sample);
                form.Inspect.SelectPage(0);
                Pump(150);
                form.Inspect.ScrollToCategory("布局");
                Pump(200);
                Save(form, Path.Combine(dir, "88-layout-group.png"));
            }

            form.Close();
            var pop = new AntdUILab.Ui.ColorPopup(System.Drawing.Color.Empty);
            pop.StartPosition = FormStartPosition.Manual;
            pop.Location = new Point(760, 360);
            pop.Show();
            Pump(250);
            Save(pop, Path.Combine(dir, "93-color-popup.png"));
            pop.Close();
            var help = new AntdUILab.Ui.HelpForm();
            help.StartPosition = FormStartPosition.Manual;
            help.Location = new Point(300, 160);
            help.Show();
            Pump(300);
            Save(help, Path.Combine(dir, "99-help.png"));
            help.Close();
            Console.WriteLine("shots -> " + dir);
        }

        /// <summary>把「没有中文名」的成员列进 docs-audit.txt，方便补说明。</summary>
        private static void AuditDocs(string dir)
        {
            try
            {
                using (var w = new StreamWriter(Path.Combine(dir, "docs-audit.txt"), false, System.Text.Encoding.UTF8))
                {
                    int props = 0, events = 0;
                    foreach (Meta m in Catalog.All())
                    {
                        Type rt = Loader.Get(m.Type);
                        if (rt == null) continue;
                        foreach (System.Reflection.PropertyInfo p in Ui.Inspector.VisibleProps(rt))
                        {
                            Doc d = Docs.ForProp(rt, p.Name);
                            Docs.FillFromXml(rt, p.Name, d, false);
                            if (string.Equals(d.Zh, d.En, StringComparison.OrdinalIgnoreCase))
                            {
                                w.WriteLine("PROP  " + m.Type + "." + p.Name + "   (" + R.Friendly(p.PropertyType) + ")");
                                props++;
                            }
                            else if (string.IsNullOrEmpty(d.EffectZh))
                            {
                                w.WriteLine("NOEFF " + m.Type + "." + p.Name);
                            }
                        }
                        foreach (System.Reflection.EventInfo ev in Ui.Inspector.VisibleEvents(rt))
                        {
                            Doc d = Docs.ForEvent(rt, ev.Name);
                            Docs.FillFromXml(rt, ev.Name, d, true);
                            if (string.Equals(d.Zh, d.En, StringComparison.OrdinalIgnoreCase))
                            {
                                w.WriteLine("EVENT " + m.Type + "." + ev.Name);
                                events++;
                            }
                            else if (string.IsNullOrEmpty(d.EffectZh))
                            {
                                w.WriteLine("NOEFE " + m.Type + "." + ev.Name);
                            }
                        }
                    }
                    w.WriteLine("-- total: props=" + props + " events=" + events);
                    Console.WriteLine("audit: props=" + props + " events=" + events);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("audit failed: " + ex.Message);
            }
        }

        private static void Pump(int ms)
        {            DateTime until = DateTime.Now.AddMilliseconds(ms);
            while (DateTime.Now < until)
            {
                Application.DoEvents();
                Thread.Sleep(10);
            }
        }

        private static void Save(Form form, string path)
        {
            for (int i = 0; i < 3; i++) Application.DoEvents();
            using (var bmp = new Bitmap(form.Width, form.Height))
            {
                form.DrawToBitmap(bmp, new Rectangle(0, 0, form.Width, form.Height));
                bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            }
            Console.WriteLine("saved " + Path.GetFileName(path));
        }
    }
}
