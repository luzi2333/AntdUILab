using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace AntdUILab.App
{
    /// <summary>把舞台上的控件现状翻译成可直接粘贴的 C# 代码。</summary>
    public static class CodeGen
    {
        private static readonly HashSet<string> Skip = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Name", "Location", "Size", "Width", "Height", "Top", "Left", "Bounds", "ClientSize",
            "Anchor", "Dock", "TabIndex", "TabStop", "Margin", "Padding", "Cursor", "RightToLeft",
            "Site", "Parent", "Container", "Tag", "Handle", "IsHandleCreated", "RenderRegion",
            "DisplayRectangle", "ReadRectangle", "PSize", "HasIcon", "HasPrefix", "HasSuffix",
            "HasValue", "OriginalBackColor", "AutoScroll", "AccessibleName", "AccessibleDescription",
            "AccessibleRole", "ImeMode", "CausesValidation", "AutoSize", "AutoEllipsis",
            "BackgroundImageLayout", "UseWaitCursor", "ContextMenuStrip", "AllowDrop", "Enabled", "Visible"
        };

        private static readonly Dictionary<string, object> Defaults = new Dictionary<string, object>();

        public static string TypeName(Meta meta, object ctl)
        {
            return "AntdUI." + (meta != null ? meta.Type : ctl.GetType().Name);
        }

        public static string VarName(Meta meta)
        {
            string n = meta != null ? meta.Type : "control";
            if (n.Length == 0) return "control";
            return char.ToLowerInvariant(n[0]) + n.Substring(1);
        }

        public static string Build(object ctl, Meta meta, IEnumerable<string> events)
        {
            if (ctl == null) return "// 当前没有控件";
            string tn = TypeName(meta, ctl);
            string vn = VarName(meta);
            var code = new StringBuilder();

            code.AppendLine("// AntdUI 2.4.15 · .NET Framework 4.7.2 · 由 AntdUI Lab 生成");
            code.AppendLine("// 需 using System.Drawing; using System.Windows.Forms;");
            code.AppendLine("var " + vn + " = new " + tn + "();");
            var c = ctl as Control;
            if (c != null) code.AppendLine(vn + ".Size = new Size(" + c.Width + ", " + c.Height + ");");

            object def = DefaultOf(ctl.GetType());
            var values = new List<string>();
            foreach (PropertyInfo p in R.PublicProps(ctl.GetType()).OrderBy(p => p.Name))
            {
                if (!p.CanWrite || p.GetSetMethod() == null) continue;
                if (!R.IsSimple(p.PropertyType) && p.PropertyType != typeof(string)) continue;
                if (Skip.Contains(p.Name)) continue;
                if (p.GetIndexParameters().Length > 0) continue;

                object now;
                try { now = p.GetValue(ctl, null); } catch { continue; }
                Color col = now is Color ? (Color)now : Color.Empty;
                if (p.Name == "BackColor" && col.R == 255 && col.G == 255 && col.B == 255) continue;
                if (p.Name == "ForeColor" && col.R == 0 && col.G == 0 && col.B == 0) continue;
                object was = null;
                if (def != null)
                {
                    PropertyInfo dp = R.Prop(def.GetType(), p.Name);
                    if (dp != null && dp.CanRead)
                    {
                        try { was = dp.GetValue(def, null); } catch { }
                    }
                }
                if (Same(now, was)) continue;
                string lit = Literal(now, p.PropertyType);
                if (lit == null) continue;
                values.Add(vn + "." + p.Name + " = " + lit + ";");
            }

            foreach (string line in values) code.AppendLine(line);
            code.AppendLine("Controls.Add(" + vn + ");");

            var evs = events == null ? new List<string>() : events.ToList();
            if (evs.Count > 0)
            {
                code.AppendLine();
                code.AppendLine("// 已订阅的事件（把注释体换成你的处理逻辑）");
                foreach (string name in evs)
                {
                    EventInfo ev = ctl.GetType().GetEvent(name, BindingFlags.Public | BindingFlags.Instance);
                    if (ev == null) continue;
                    code.AppendLine(vn + "." + name + " += " + Lambda(ev) + ";");
                }
            }

            return code.ToString();
        }

        private static string Lambda(EventInfo ev)
        {
            MethodInfo invoke = ev.EventHandlerType == null ? null : ev.EventHandlerType.GetMethod("Invoke");
            int n = invoke == null ? 2 : invoke.GetParameters().Length;
            if (n <= 0) return "() => { /* TODO */ }";
            string[] names = { "s", "e", "a", "b", "c", "d" };
            var ps = new string[n];
            for (int i = 0; i < n; i++) ps[i] = names[Math.Min(i, names.Length - 1)];
            string list = n == 1 ? ps[0] : "(" + string.Join(", ", ps) + ")";
            return list + " => { /* " + ev.Name + " */ }";
        }

        private static object DefaultOf(Type t)
        {
            object o;
            if (Defaults.TryGetValue(t.FullName, out o)) return o;
            try
            {
                o = Activator.CreateInstance(t);
            }
            catch
            {
                o = null;
            }
            Defaults[t.FullName] = o;
            return o;
        }

        private static bool Same(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            return a.Equals(b);
        }

        public static string Literal(object v, Type t)
        {
            if (v == null) return "null";
            if (v is string) return "\"" + ((string)v).Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
            if (v is bool) return (bool)v ? "true" : "false";
            if (v is Color)
            {
                Color c = (Color)v;
                return "Color.FromArgb(" + c.R + ", " + c.G + ", " + c.B + ")";
            }
            if (v is Size) return "new Size(" + ((Size)v).Width + ", " + ((Size)v).Height + ")";
            if (v is DateTime) return "new DateTime(" + ((DateTime)v).Year + ", " + ((DateTime)v).Month + ", " + ((DateTime)v).Day + ")";
            if (v.GetType().IsEnum)
            {
                Type et = v.GetType();
                string q = Loader.IsAntd(et) ? "AntdUI." + et.Name : et.FullName.Replace('+', '.');
                return q + "." + v;
            }
            if (v is float) return Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture) + "f";
            if (v is decimal) return Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture) + "m";
            if (v is double) return Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture) + "d";
            if (t == typeof(int) || t == typeof(Nullable<int>)) return Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture);
            return null;
        }
    }
}
