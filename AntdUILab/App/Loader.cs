using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace AntdUILab.App
{
    /// <summary>运行时加载 AntdUI（本机 LoadFrom 被拦截，只能按字节 Load）。</summary>
    public static class Loader
    {
        public static Assembly Antd { get; private set; }
        public static string BaseDir { get; private set; }
        public static Dictionary<string, Type> ByName { get; private set; }
        public static List<Type> ControlTypes { get; private set; }
        public static string VersionText { get; private set; }

        public static void Init(string baseDir)
        {
            BaseDir = baseDir;
            string dll = Path.Combine(baseDir, "AntdUI.dll");
            if (!File.Exists(dll))
                throw new FileNotFoundException("未找到 AntdUI.dll", dll);

            Antd = Assembly.Load(File.ReadAllBytes(dll));
            VersionText = Antd.GetName().Version.ToString();

            XmlDocs.Load(Path.Combine(baseDir, "AntdUI.xml"));

            ByName = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            foreach (Type t in Antd.GetTypes())
            {
                if (!t.IsPublic || t.IsNested) continue;
                if (!ByName.ContainsKey(t.Name)) ByName[t.Name] = t;
            }

            ControlTypes = Antd.GetTypes()
                .Where(t => t.IsPublic && !t.IsAbstract && typeof(Control).IsAssignableFrom(t))
                .Where(t => t.GetConstructor(Type.EmptyTypes) != null)
                .OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            DisableAnimation();
        }

        /// <summary>
        /// 关掉 AntdUI 的动画。弹层（Select/DatePicker/TimePicker/ColorPicker 等）打开时先建一个
        /// 6×6 的透明层，再靠动画把它撑到目标大小；在没有 DWM 合成的环境里动画不会跑，
        /// 结果弹层就卡在 6×6、点不到。关掉动画后弹层直接按最终尺寸出现。
        /// </summary>
        private static void DisableAnimation()
        {
            try
            {
                Type cfg = Antd.GetType("AntdUI.Config", false);
                if (cfg == null) return;
                PropertyInfo p = cfg.GetProperty("Animation", BindingFlags.Public | BindingFlags.Static);
                if (p != null && p.CanWrite)
                {
                    p.SetValue(null, false, null);
                    AnimationDisabled = true;
                }
            }
            catch { /* 老版本没有这个属性就保持默认 */ }
        }

        /// <summary>是否成功关掉了 AntdUI 动画（用于界面提示）。</summary>
        public static bool AnimationDisabled { get; private set; }

        public static Type Get(string simpleName)
        {
            if (string.IsNullOrEmpty(simpleName)) return null;
            Type t;
            if (ByName != null && ByName.TryGetValue(simpleName, out t)) return t;
            return Antd == null ? null : Antd.GetType("AntdUI." + simpleName, false);
        }

        public static bool IsAntd(Type t)
        {
            return t != null && Antd != null && t.Assembly == Antd;
        }
    }
}
