using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace AntdUILab.App
{
    /// <summary>解析 AntdUI.xml（UTF-8，个别节点 XML 不合法，故用正则逐成员抽取中文注释）。</summary>
    public static class XmlDocs
    {
        private static readonly Dictionary<string, string> Map = new Dictionary<string, string>(StringComparer.Ordinal);

        /// <summary>「P|成员名」→ 摘要：只收接口（IControl 等）上的注释，用于查不到的成员的兜底。</summary>
        private static readonly Dictionary<string, string> IfMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public static bool Loaded { get; private set; }

        public static void Load(string path)
        {
            Map.Clear();
            IfMap.Clear();
            Loaded = false;
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

            string text = File.ReadAllText(path, Encoding.UTF8);
            foreach (Match m in Regex.Matches(text, "<member name=\"([^\"]+)\">([\\s\\S]*?)</member>"))
            {
                Match s = Regex.Match(m.Groups[2].Value, "<summary>([\\s\\S]*?)</summary>");
                if (!s.Success) continue;
                string txt = Clean(s.Groups[1].Value);
                if (txt.Length > 0) Map[m.Groups[1].Value] = txt;
            }

            // 接口（IControl / IButton …）上的注释按成员名建索引：
            // 反射拿到的是实现类上的成员，按「类型.成员」查不到，但语义是同一个。
            foreach (KeyValuePair<string, string> kv in Map)
            {
                string key = kv.Key;
                if (key.Length < 4) continue;
                char kind = key[0];
                if (kind != 'P' && kind != 'E' && kind != 'F') continue;
                if (!key.StartsWith(kind + ":AntdUI.", StringComparison.Ordinal)) continue;
                string body = key.Substring(9);              // 去掉 "P:AntdUI."
                int dot = body.LastIndexOf('.');
                if (dot <= 0) continue;
                string typeName = body.Substring(0, dot);
                if (typeName.Length < 2 || typeName[0] != 'I' || !char.IsUpper(typeName[1])) continue;
                string member = body.Substring(dot + 1);
                if (member.IndexOf('(') >= 0) continue;
                string ik = kind + "|" + member;
                if (!IfMap.ContainsKey(ik)) IfMap[ik] = kv.Value;
            }
            Loaded = Map.Count > 0;
        }

        public static string Raw(string memberKey)
        {
            string v;
            return Map.TryGetValue(memberKey, out v) ? v : null;
        }

        public static string Type(string typeName)
        {
            return Raw("T:AntdUI." + typeName);
        }

        public static string Prop(string typeName, string prop)
        {
            string v = Raw("P:AntdUI." + typeName + "." + prop);
            if (v == null) v = Raw("P:AntdUI." + prop);
            if (v == null) IfMap.TryGetValue("P|" + prop, out v);
            return v;
        }

        public static string Event(string typeName, string ev)
        {
            string v = Raw("E:AntdUI." + typeName + "." + ev);
            if (v == null) v = Raw("E:AntdUI." + ev);
            if (v == null) IfMap.TryGetValue("E|" + ev, out v);
            return v;
        }

        public static string Field(string typeName, string field)
        {
            return Raw("F:AntdUI." + typeName + "." + field);
        }

        private static string Clean(string s)
        {
            s = Regex.Replace(s, "<see cref=\"([^\"]+)\"\\s*/>", m => ShortName(m.Groups[1].Value));
            s = Regex.Replace(s, "<seealso cref=\"([^\"]+)\"\\s*/>", m => ShortName(m.Groups[1].Value));
            s = Regex.Replace(s, "<paramref name=\"([^\"]+)\"\\s*/>", "$1");
            s = Regex.Replace(s, "<typeparamref name=\"([^\"]+)\"\\s*/>", "$1");
            s = Regex.Replace(s, "<c>([\\s\\S]*?)</c>", "$1");
            s = Regex.Replace(s, "<(br|para)\\s*/?>", " ");
            s = Regex.Replace(s, "<[^>]+>", " ");
            s = s.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"")
                 .Replace("&apos;", "'").Replace("&amp;", "&");
            s = Regex.Replace(s, "\\s+", " ").Trim();
            return s;
        }

        private static string ShortName(string cref)
        {
            if (cref.StartsWith("T:", StringComparison.Ordinal)) cref = cref.Substring(2);
            int i = cref.LastIndexOf('.');
            string s = i >= 0 ? cref.Substring(i + 1) : cref;
            int p = s.IndexOf('(');
            if (p >= 0) s = s.Substring(0, p);
            return s;
        }
    }
}
