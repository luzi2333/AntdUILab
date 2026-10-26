using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace AntdUILab.App
{
    /// <summary>把事件参数翻译成人话：鼠标坐标、按键、自定义事件参数的公共属性。</summary>
    public static class EventArgsText
    {
        /// <summary>args[0] 是 sender，从 1 开始描述；没有参数返回空串。</summary>
        public static string Pretty(EventInfo ev, object[] args)
        {
            if (args == null || args.Length <= 1) return "";
            var parts = new List<string>();
            for (int i = 1; i < args.Length; i++)
            {
                string p = One(args[i]);
                if (!string.IsNullOrEmpty(p)) parts.Add(p);
            }
            string s = string.Join(" · ", parts);
            if (s.Length > 120) s = s.Substring(0, 117) + "...";
            return s;
        }

        public static string One(object a)
        {
            if (a == null) return "null";
            if (a.GetType() == typeof(EventArgs)) return "";

            var me = a as MouseEventArgs;
            if (me != null)
            {
                var sb = new StringBuilder();
                sb.Append("(").Append(me.X).Append(", ").Append(me.Y).Append(")");
                if (me.Button != MouseButtons.None) sb.Append(" · ").Append(ButtonZh(me.Button));
                if (me.Clicks > 1) sb.Append(" · 连击 ").Append(me.Clicks);
                if (me.Delta != 0) sb.Append(" · 滚轮 ").Append(me.Delta);
                return sb.ToString();
            }

            var ke = a as KeyEventArgs;
            if (ke != null)
            {
                string mods = Mods(ke.Control, ke.Shift, ke.Alt);
                return mods.Length == 0 ? ke.KeyCode.ToString() : ke.KeyCode + " · " + mods;
            }

            var kp = a as KeyPressEventArgs;
            if (kp != null)
            {
                char c = kp.KeyChar;
                string shown = c == '\r' ? "Enter" : (c == '\t' ? "Tab" : (c == (char)27 ? "Esc" : c.ToString()));
                return "字符 '" + shown + "'";
            }

            var props = Props(a);
            if (props.Length > 0) return props;

            string d = R.Describe(a);
            return d == null ? "" : d;
        }

        /// <summary>自定义事件参数：列出前几个公共简单属性（Checked=True、Index=2 …）。</summary>
        private static string Props(object a)
        {
            if (a is EventArgs) return "";
            var parts = new List<string>();
            try
            {
                foreach (PropertyInfo p in a.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (!p.CanRead || p.GetIndexParameters().Length > 0) continue;
                    string n = p.Name;
                    if (n == "Empty" || n.EndsWith("Current") || m_skip.Contains(n)) continue;
                    if (!IsSimple(p.PropertyType)) continue;
                    object v;
                    try { v = p.GetValue(a, null); } catch { continue; }
                    if (v == null) continue;
                    parts.Add(n + "=" + R.Describe(v));
                    if (parts.Count >= 4) break;
                }
            }
            catch { }
            return string.Join(" · ", parts.ToArray());
        }

        private static readonly HashSet<string> m_skip = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Sender", "Handled", "Target", "Source", "Kind", "EventArgs"
        };

        private static bool IsSimple(Type t)
        {
            if (t.IsEnum) return true;
            if (t == typeof(bool) || t == typeof(int) || t == typeof(long) || t == typeof(float) || t == typeof(double) || t == typeof(decimal)
                || t == typeof(string) || t == typeof(char) || t == typeof(DateTime) || t == typeof(System.Drawing.Color)) return true;
            Type u = Nullable.GetUnderlyingType(t);
            return u != null && IsSimple(u);
        }

        public static string Mods(bool ctrl, bool shift, bool alt)
        {
            var s = new List<string>();
            if (ctrl) s.Add("Ctrl");
            if (shift) s.Add("Shift");
            if (alt) s.Add("Alt");
            return string.Join("+", s.ToArray());
        }

        private static string ButtonZh(MouseButtons b)
        {
            switch (b)
            {
                case MouseButtons.Left: return "左键";
                case MouseButtons.Right: return "右键";
                case MouseButtons.Middle: return "中键";
                default: return b.ToString();
            }
        }
    }
}
