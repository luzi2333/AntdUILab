using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Forms;

namespace AntdUILab.App
{
    /// <summary>反射小工具：取值、赋值、类型转换、事件挂接。</summary>
    public static class R
    {
        private const BindingFlags Flags = BindingFlags.Public | BindingFlags.Instance;

        public static PropertyInfo Prop(Type t, string name)
        {
            if (t == null || string.IsNullOrEmpty(name)) return null;
            for (Type cur = t; cur != null; cur = cur.BaseType)
            {
                PropertyInfo hit = null;
                foreach (PropertyInfo p in cur.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                {
                    if (!string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)) continue;
                    if (p.GetIndexParameters().Length > 0) continue;
                    if (hit == null) hit = p;
                }
                if (hit != null) return hit;
            }
            return null;
        }

        public static object Get(object o, string name)
        {
            if (o == null) return null;
            try
            {
                PropertyInfo p = Prop(o.GetType(), name);
                return p == null || !p.CanRead || p.GetIndexParameters().Length > 0 ? null : p.GetValue(o, null);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>设置属性；成功返回 null，失败返回错误信息。</summary>
        public static string Set(object o, string name, object value)
        {
            if (o == null) return "对象为空";
            PropertyInfo p = Prop(o.GetType(), name);
            if (p == null) return "没有属性 " + name;
            if (!p.CanWrite) return "属性 " + name + " 是只读的";

            string err;
            object v = Coerce(p.PropertyType, value, out err);
            if (err != null) return err;
            try
            {
                p.SetValue(o, v, null);
                return null;
            }
            catch (Exception ex)
            {
                Exception inner = ex;
                if (ex is TargetInvocationException && ex.InnerException != null) inner = ex.InnerException;
                return "设置失败：" + inner.Message;
            }
        }

        public static object Coerce(Type target, object value, out string error)
        {
            error = null;
            if (target == null) return value;

            Type under = Nullable.GetUnderlyingType(target);
            if (under != null)
            {
                if (value == null) return null;
                return Coerce(under, value, out error);
            }

            if (value == null)
            {
                if (target.IsValueType)
                {
                    error = "该属性不能为空（类型 " + Friendly(target) + "）";
                    return null;
                }
                return null;
            }

            if (target.IsInstanceOfType(value) && !(value is string)) return value;

            try
            {
                string s = value as string;
                if (target == typeof(string)) return value.ToString();
                if (target.IsEnum)
                {
                    if (value is string)
                    {
                        string txt = ((string)value).Trim();
                        foreach (string n in Enum.GetNames(target))
                            if (string.Equals(n, txt, StringComparison.OrdinalIgnoreCase)) return Enum.Parse(target, n);
                        int num;
                        if (int.TryParse(txt, out num)) return Enum.ToObject(target, num);
                        error = "枚举值无效：" + txt + "（可选 " + string.Join("/", Enum.GetNames(target)) + "）";
                        return null;
                    }
                    return Enum.ToObject(target, Convert.ChangeType(value, Enum.GetUnderlyingType(target), CultureInfo.InvariantCulture));
                }
                if (target == typeof(bool)) return Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                if (target == typeof(Color)) return ToColor(value);
                if (target == typeof(DateTime)) return value is DateTime ? value : (object)DateTime.Parse(value.ToString(), CultureInfo.CurrentCulture);
                if (target == typeof(Size)) return value is Size ? value : (object)ParseSize(value.ToString(), false);
                if (target == typeof(Point)) return value is Point ? value : (object)ParseSize(value.ToString(), true);
                if (target == typeof(Padding)) return value is Padding ? value : (object)ParsePadding(value.ToString());
                if (target == typeof(decimal)) return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                if (target == typeof(float)) return Convert.ToSingle(value, CultureInfo.InvariantCulture);
                if (target == typeof(double)) return Convert.ToDouble(value, CultureInfo.InvariantCulture);
                if (target.IsPrimitive) return Convert.ChangeType(value, target, CultureInfo.InvariantCulture);
                if (target.IsAssignableFrom(value.GetType())) return value;
            }
            catch (Exception ex)
            {
                error = "值格式不正确：" + ex.Message;
                return null;
            }

            error = "无法把 " + (value == null ? "null" : value.GetType().Name) + " 转为 " + Friendly(target);
            return null;
        }

        private static Color ToColor(object value)
        {
            if (value is Color) return (Color)value;
            string s = value.ToString().Trim();
            Color c;
            if (s.StartsWith("#"))
            {
                string h = s.Substring(1);
                if (h.Length == 3) h = "" + h[0] + h[0] + h[1] + h[1] + h[2] + h[2];
                int rgb;
                if (int.TryParse(h, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out rgb))
                    return Color.FromArgb((rgb >> 16) & 255, (rgb >> 8) & 255, rgb & 255);
            }
            string[] parts = s.Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3)
            {
                int r, g, b;
                if (int.TryParse(parts[0], out r) && int.TryParse(parts[1], out g) && int.TryParse(parts[2], out b))
                    return Color.FromArgb(Clamp(r), Clamp(g), Clamp(b));
            }
            if (s.Length > 0)
            {
                c = Color.FromName(s);
                if (c.IsKnownColor || c.IsNamedColor) return c;
            }
            return Color.Empty;
        }

        private static int Clamp(int v)
        {
            return v < 0 ? 0 : v > 255 ? 255 : v;
        }

        private static object ParseSize(string s, bool point)
        {
            string t = (s ?? "").Replace("{", "").Replace("}", "").Replace("X=", "").Replace("Y=", "").Replace("Width=", "").Replace("Height=", "");
            string[] p = t.Split(new[] { ',', 'x', 'X', '×', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (p.Length >= 2)
            {
                int a = int.Parse(p[0].Trim()), b = int.Parse(p[1].Trim());
                return point ? (object)new Point(a, b) : (object)new Size(a, b);
            }
            throw new FormatException(point ? "需要 \"x, y\" 格式" : "需要 \"宽, 高\" 格式");
        }

        private static Padding ParsePadding(string s)
        {
            string[] p = (s ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (p.Length == 1) return new Padding(int.Parse(p[0].Trim()));
            if (p.Length == 2) return new Padding(int.Parse(p[0].Trim()), int.Parse(p[1].Trim()), int.Parse(p[0].Trim()), int.Parse(p[1].Trim()));
            if (p.Length >= 4) return new Padding(int.Parse(p[0].Trim()), int.Parse(p[1].Trim()), int.Parse(p[2].Trim()), int.Parse(p[3].Trim()));
            throw new FormatException("需要 \"左, 上, 右, 下\" 格式");
        }

        public static bool IsSimple(Type t)
        {
            if (t == null) return false;
            Type u = Nullable.GetUnderlyingType(t) ?? t;
            return u.IsPrimitive || u.IsEnum || u == typeof(string) || u == typeof(decimal)
                   || u == typeof(DateTime) || u == typeof(Color) || u == typeof(Size)
                   || u == typeof(Point) || u == typeof(Padding);
        }

        public static string Friendly(Type t)
        {
            if (t == null) return "?";
            Type u = Nullable.GetUnderlyingType(t);
            if (u != null) return Friendly(u) + "?";
            if (t == typeof(void)) return "void";
            if (t == typeof(int)) return "int";
            if (t == typeof(long)) return "long";
            if (t == typeof(short)) return "short";
            if (t == typeof(byte)) return "byte";
            if (t == typeof(bool)) return "bool";
            if (t == typeof(string)) return "string";
            if (t == typeof(float)) return "float";
            if (t == typeof(double)) return "double";
            if (t == typeof(decimal)) return "decimal";
            if (t == typeof(object)) return "object";
            if (t == typeof(Color)) return "Color";
            if (t == typeof(Font)) return "Font";
            if (t == typeof(Size)) return "Size";
            if (t == typeof(Point)) return "Point";
            if (t == typeof(Padding)) return "Padding";
            if (t.IsArray) return Friendly(t.GetElementType()) + "[]";
            if (t.IsGenericType)
            {
                string n = t.Name;
                int tick = n.IndexOf('`');
                if (tick > 0) n = n.Substring(0, tick);
                return n + "<" + string.Join(", ", t.GetGenericArguments().Select(Friendly)) + ">";
            }
            return t.Name;
        }

        public static string Signature(EventInfo ev)
        {
            if (ev == null || ev.EventHandlerType == null) return "";
            MethodInfo invoke = ev.EventHandlerType.GetMethod("Invoke");
            if (invoke == null) return "";
            string ps = string.Join(", ", invoke.GetParameters().Select(p => Friendly(p.ParameterType) + " " + (string.IsNullOrEmpty(p.Name) ? "arg" : p.Name)));
            return Friendly(invoke.ReturnType) + " (" + ps + ")";
        }

        public static string Describe(object v)
        {
            if (v == null) return "null";
            if (v is string) return "\"" + v + "\"";
            if (v is bool) return (bool)v ? "true" : "false";
            if (v is Color) return "#" + ((Color)v).R.ToString("X2") + ((Color)v).G.ToString("X2") + ((Color)v).B.ToString("X2");
            if (v is Size) return ((Size)v).Width + " × " + ((Size)v).Height;
            if (v is Point) return ((Point)v).X + ", " + ((Point)v).Y;
            if (v is Padding)
            {
                Padding pd = (Padding)v;
                return pd.Left + ", " + pd.Top + ", " + pd.Right + ", " + pd.Bottom;
            }
            if (v is Font) return "Font(" + ((Font)v).Name + ", " + ((Font)v).Size + (int)((Font)v).Style + ")";
            if (v.GetType().IsEnum) return v.GetType().Name + "." + v;
            string s = Convert.ToString(v, CultureInfo.CurrentCulture);
            if (s != null && s.Length > 70) s = s.Substring(0, 67) + "...";
            return s;
        }

        /// <summary>为任意签名的事件创建委托，回调到 sink(事件名, sender, 参数数组)。</summary>
        public static Delegate Handler(EventInfo ev, Action<string, object, object[]> sink)
        {
            if (ev == null || ev.EventHandlerType == null || sink == null) return null;
            MethodInfo invoke = ev.EventHandlerType.GetMethod("Invoke");
            if (invoke == null) return null;

            ParameterInfo[] ps = invoke.GetParameters();
            ParameterExpression[] pars = new ParameterExpression[ps.Length];
            for (int i = 0; i < ps.Length; i++)
                pars[i] = Expression.Parameter(ps[i].ParameterType, "p" + i);

            Expression args = Expression.NewArrayInit(typeof(object), pars.Select(p => (Expression)Expression.Convert(p, typeof(object))));
            Expression sender = pars.Length > 0 ? (Expression)Expression.Convert(pars[0], typeof(object)) : (Expression)Expression.Constant(null, typeof(object));
            MethodInfo callM = typeof(Action<string, object, object[]>).GetMethod("Invoke");
            Expression call = Expression.Call(Expression.Constant(sink), callM, Expression.Constant(ev.Name), sender, args);
            Expression body = invoke.ReturnType == typeof(void)
                ? call
                : (Expression)Expression.Block(call, Expression.Default(invoke.ReturnType));

            try
            {
                return Expression.Lambda(ev.EventHandlerType, body, pars).Compile();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>向集合属性（如 Select.Items / Tabs.Pages）追加元素。</summary>
        public static object AddToCollection(object owner, string propName, object item)
        {
            object coll = Get(owner, propName);
            if (coll == null) return null;
            MethodInfo add = coll.GetType().GetMethods()
                .Where(m => m.Name == "Add" && m.GetParameters().Length == 1)
                .OrderBy(m => m.GetParameters()[0].ParameterType == typeof(object) ? 1 : 0)
                .FirstOrDefault();
            if (add == null) return null;
            add.Invoke(coll, new[] { item });
            return coll;
        }

        public static int CollectionCount(object coll)
        {
            if (coll == null) return 0;
            try
            {
                PropertyInfo p = coll.GetType().GetProperty("Count");
                return p == null ? 0 : Convert.ToInt32(p.GetValue(coll, null));
            }
            catch
            {
                return 0;
            }
        }

        public static IEnumerable<PropertyInfo> PublicProps(Type t)
        {
            return t == null ? Enumerable.Empty<PropertyInfo>() : t.GetProperties(Flags);
        }

        public static IEnumerable<EventInfo> AllEvents(Type t)
        {
            return t == null ? Enumerable.Empty<EventInfo>() : t.GetEvents(Flags);
        }
    }
}
