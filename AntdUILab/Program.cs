using System;
using System.Linq;
using System.Windows.Forms;
using AntdUILab.App;
using AntdUILab.Ui;

namespace AntdUILab
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += (s, e) => ShowCrash(e.Exception);

            try
            {
                Loader.Init(AppDomain.CurrentDomain.BaseDirectory);
            }
            catch (Exception ex)
            {
                MessageBox.Show("初始化失败：\r\n" + ex.Message + "\r\n\r\n请确认 AntdUI.dll 与 AntdUI.xml 位于程序目录。",
                    "AntdUI Lab", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                if (args.Any(a => string.Equals(a, "--shot", StringComparison.OrdinalIgnoreCase)))
                {
                    ShotMode.Run(args);
                    return;
                }

                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                ShowCrash(ex);
            }
        }

        private static void ShowCrash(Exception ex)
        {
            if (ShotMode.IsShot)
            {
                try { System.IO.File.AppendAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "antduilab-crash.txt"), DateTime.Now + "\r\n" + ex + "\r\n\r\n"); } catch { }
                Console.Error.WriteLine(ex.ToString());
                return;
            }
            try
            {
                MessageBox.Show(ex.ToString(), "AntdUI Lab 发生异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {
            }
        }
    }
}
