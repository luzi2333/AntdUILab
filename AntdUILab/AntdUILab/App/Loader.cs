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
        }

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
