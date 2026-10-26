using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace AntdUILab.App
{
    /// <summary>
    /// 成员分类：优先读程序集自带的 [Category]，其次按名字兜底。
    /// AntdUI 的属性/事件本来就带中文 Category（外观 / 布局 / 行为 / 徽标 …），
    /// 所以这里分组出来的结果和 VS 属性窗口的「分类」视图基本一致。
    /// </summary>
    public static class Categories
    {
        // 分类顺序：和 VS 中文版属性窗口的「分类」视图对齐 —— 布局排最上面，其余按常用程度往下排
        private static readonly string[] Preferred =
        {
            "布局", "设计", "外观", "边框", "阴影", "徽标", "行为", "数据", "值", "操作",
            "鼠标", "键", "焦点", "光标", "国际化", "加载", "切换", "拖放",
            "可访问性", "窗口样式", "动画", "任务栏", "面板", "警告", "属性已更改",
            "其他", "私有", "窗口"
        };

        // 「布局」组里成员自己的顺序：位置 / 尺寸 / 停靠 / 锚定 / 外边距 / 内边距 / 最小 / 最大
        private static readonly string[] LayoutMembers =
        {
            "Location", "Size", "Dock", "Anchor", "Margin", "Padding", "MinimumSize", "MaximumSize"
        };

        private static readonly Dictionary<string, string> English = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "设计", "Design" },
            { "外观", "Appearance" }, { "边框", "Border" }, { "阴影", "Shadow" }, { "徽标", "Badge" },
            { "布局", "Layout" }, { "行为", "Behavior" }, { "数据", "Data" }, { "值", "Value" },
            { "操作", "Action" }, { "光标", "Cursor" }, { "国际化", "Localization" },
            { "焦点", "Focus" }, { "鼠标", "Mouse" }, { "键", "Key" }, { "拖放", "Drag Drop" },
            { "属性已更改", "Property Changed" }, { "可访问性", "Accessibility" },
            { "窗口样式", "Window Style" }, { "切换", "Toggle" }, { "加载", "Loading" },
            { "动画", "Animation" }, { "任务栏", "Taskbar" }, { "面板", "Panel" },
            { "其他", "Misc" }, { "私有", "Private" }, { "警告", "Warning" },
            { "Docking", "Docking" }
        };

        /// <summary>成员所属分类（中文，如 "外观"）。</summary>
        public static string Of(MemberInfo m, bool isEvent)
        {
            if (m != null)
            {
                try
                {
                    object[] attrs = m.GetCustomAttributes(typeof(CategoryAttribute), true);
                    if (attrs != null && attrs.Length > 0)
                    {
                        string c = ((CategoryAttribute)attrs[0]).Category;
                        if (!string.IsNullOrEmpty(c)) return Translate(c);
                    }
                }
                catch { }
            }
            return Guess(m == null ? "" : m.Name, isEvent);
        }

        /// <summary>分类的显示名：中文 + English。</summary>
        public static string Label(string cat)
        {
            if (string.IsNullOrEmpty(cat)) cat = "其他";
            string en;
            if (English.TryGetValue(cat, out en)) return cat + " " + en;
            return cat;
        }

        /// <summary>排序键：按英文分类名排（Action / Appearance / Behavior …），跟 VS 分类视图的顺序一致。</summary>
        public static string SortKey(string cat)
        {
            if (string.IsNullOrEmpty(cat)) return "Misc";
            string en;
            if (English.TryGetValue(cat, out en)) return en;
            return cat;
        }

        /// <summary>组内成员顺序：布局组按「位置 / 尺寸 / 停靠 / 锚定 / 边距 / 最小 / 最大」排，其它组交给调用方。</summary>
        public static int MemberOrder(string cat, string member)
        {
            if (!string.Equals(cat, "布局", StringComparison.OrdinalIgnoreCase)) return 0;
            for (int i = 0; i < LayoutMembers.Length; i++)
                if (string.Equals(LayoutMembers[i], member, StringComparison.OrdinalIgnoreCase)) return i;
            return 50;
        }

        /// <summary>排序用的序号：已知分类按 Preferred 顺序，未知的排后面。</summary>
        public static int Order(string cat)
        {
            for (int i = 0; i < Preferred.Length; i++)
                if (string.Equals(Preferred[i], cat, StringComparison.OrdinalIgnoreCase)) return i;
            return 100;
        }

        private static string Translate(string c)
        {
            // 英文系统上 Category 会是 "Appearance" 之类，这里换回中文，显示更统一
            foreach (var kv in English)
                if (string.Equals(kv.Value, c, StringComparison.OrdinalIgnoreCase)) return kv.Key;
            return c;
        }

        private static string Guess(string name, bool isEvent)
        {
            if (isEvent)
            {
                switch (name)
                {
                    case "Click":
                    case "DoubleClick":
                    case "MouseClick":
                    case "MouseDoubleClick":
                        return "操作";
                    case "GotFocus":
                    case "LostFocus":
                    case "Enter":
                    case "Leave":
                    case "Validated":
                    case "Validating":
                        return "焦点";
                    case "KeyDown":
                    case "KeyUp":
                    case "KeyPress":
                        return "键";
                    case "MouseDown":
                    case "MouseUp":
                    case "MouseMove":
                    case "MouseEnter":
                    case "MouseLeave":
                    case "MouseHover":
                    case "MouseWheel":
                        return "鼠标";
                    case "Resize":
                    case "Layout":
                    case "Move":
                        return "布局";
                }
                return "属性已更改";
            }

            switch (name)
            {
                case "Name":
                    return "设计";
                case "Location":
                case "Size":
                case "Width":
                case "Height":
                case "Bounds":
                case "Dock":
                case "Anchor":
                case "Margin":
                case "Padding":
                case "MinimumSize":
                case "MaximumSize":
                case "AutoSize":
                case "AutoSizeMode":
                case "RightToLeft":
                case "ClientSize":
                    return "布局";
                case "Text":
                case "BackColor":
                case "ForeColor":
                case "Font":
                case "BackgroundImage":
                case "Image":
                case "ImageSvg":
                case "Cursor":
                    return "外观";
                case "Enabled":
                case "Visible":
                case "ReadOnly":
                case "TabStop":
                case "TabIndex":
                    return "行为";
            }

            if (name.EndsWith("Color", StringComparison.OrdinalIgnoreCase)
                || name.EndsWith("Svg", StringComparison.OrdinalIgnoreCase)
                || name.IndexOf("Border", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Radius", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Align", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Icon", StringComparison.OrdinalIgnoreCase) >= 0)
                return "外观";
            if (name.IndexOf("Check", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Select", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Value", StringComparison.OrdinalIgnoreCase) >= 0)
                return "行为";
            if (name.IndexOf("Data", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Items", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Columns", StringComparison.OrdinalIgnoreCase) >= 0)
                return "数据";
            return "其他";
        }
    }
}
