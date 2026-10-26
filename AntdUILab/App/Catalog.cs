using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace AntdUILab.App
{
    /// <summary>一个可预览控件的元数据：名称、说明、预设尺寸、示例内容初始化。</summary>
    public class Meta
    {
        public string Type;      // 类型简单名
        public string Zh;        // 中文名
        public string En;        // 英文名
        public string Group;     // 分组（中英）
        public string IntroZh;
        public string IntroEn;
        public Size[] Sizes;
        public Action<object> Init;
        public int Star;         // 1..15 = 精讲顺序，0 = 自动收录

        public bool Curated { get { return Star > 0; } }
    }

    /// <summary>控件目录：精讲 15 个 + 常用若干，其余类型自动收录。</summary>
    public static class Catalog
    {
        public static readonly List<Meta> Metas = new List<Meta>();
        public static readonly Dictionary<string, Meta> ByName = new Dictionary<string, Meta>(StringComparer.OrdinalIgnoreCase);

        public const string GButton = "按钮 Buttons";
        public const string GInput = "输入 Inputs";
        public const string GData = "数据 Data";
        public const string GDisplay = "展示 Display";
        public const string GNav = "导航 Navigation";
        public const string GOther = "其它 Others";

        public static readonly string[] Groups = { GButton, GInput, GData, GDisplay, GNav, GOther };

        static Catalog()
        {
            Star("Button", "按钮", "Button", GButton, "最常用的操作入口：风格、形状、图标、加载中、波纹与切换态都在这里。", "The primary action trigger: style, shape, icon, loading, ripple and toggle states.", new[] { new Size(120, 36), new Size(160, 40), new Size(96, 32) }, o =>
            {
                Set(o, "Text", "按钮");
                Set(o, "Type", "Primary");
            });

            Star("Input", "输入框", "Input", GInput, "单行/多行文本输入，带前后缀、清空按钮、状态色与下划线变体。", "Single/multi-line text field with prefix, suffix, clear button, status colors and underlined variant.", new[] { new Size(240, 36), new Size(320, 40), new Size(240, 90) }, o =>
            {
                Set(o, "PlaceholderText", "请输入内容");
                Set(o, "AllowClear", true);
            });

            Star("Select", "下拉选择", "Select", GInput, "从固定选项里单选一项，支持展开方向、图标、过滤与多选数量。", "Pick one item from a fixed list, with placement, icons, filtering and max count.", new[] { new Size(200, 36), new Size(260, 40) }, o =>
            {
                Items(o, "Items", "选项一", "选项二", "选项三");
                Set(o, "SelectedIndex", 0);
            });

            Star("Checkbox", "复选框", "Checkbox", GInput, "在一组选项里多选；支持三态（半选）与自定义选中填充色。", "Multi-select in a group, with tri-state (indeterminate) and custom fill color.", new[] { new Size(160, 24), new Size(200, 28) }, o =>
            {
                Set(o, "Text", "同意条款");
                Set(o, "Checked", true);
            });

            Star("Switch", "开关", "Switch", GInput, "表示开/关的滑动开关，适合立即生效的设置项。", "A sliding on/off toggle for settings that take effect immediately.", new[] { new Size(44, 22), new Size(60, 28) }, o =>
            {
                Set(o, "Checked", true);
                Set(o, "CheckedText", "");
                Set(o, "UnCheckedText", "");
            });

            Star("Slider", "滑动条", "Slider", GInput, "用拖动方式在连续区间内取值，可显示刻度点与标记。", "Drag to pick a value in a continuous range, with dots and marks.", new[] { new Size(240, 36), new Size(320, 36) }, o =>
            {
                Set(o, "MinValue", 0);
                Set(o, "MaxValue", 100);
                Set(o, "Value", 40);
                Set(o, "ShowValue", true);
            });

            Star("DatePicker", "日期选择", "DatePicker", GInput, "输入或弹出日历选择日期，可切换到月/年视图。", "Type or pick a date from a calendar popup; switches to month/year views.", new[] { new Size(200, 36), new Size(240, 40) }, o =>
            {
                Set(o, "Value", DateTime.Today);
                Set(o, "Format", "yyyy-MM-dd");
            });

            Star("Progress", "进度条", "Progress", GDisplay, "展示任务完成百分比，有线性、圆形、步骤等多种形态。", "Shows completion percentage in linear, circular or step shapes.", new[] { new Size(240, 36), new Size(120, 120) }, o =>
            {
                Set(o, "Value", 0.75f);
            });

            Star("Tag", "标签", "Tag", GDisplay, "用小块彩色标签标注分类、状态，可带关闭按钮。", "Small colored chips for categories/status, optionally closable.", new[] { new Size(80, 24), new Size(100, 28) }, o =>
            {
                Set(o, "Text", "标签");
                Set(o, "Type", "Primary");
            });

            Star("Badge", "徽标", "Badge", GDisplay, "数字或小圆点提醒，常叠在图标/页签右上角。", "A number or dot reminder, usually on the corner of an icon or tab.", new[] { new Size(64, 22), new Size(80, 26) }, o =>
            {
                Set(o, "Text", "99+");
                Set(o, "State", "Error");
            });

            Star("Avatar", "头像", "Avatar", GDisplay, "用户头像：文字、图片或 SVG 图标，支持圆形、方形与悬停效果。", "User avatar with text, image or SVG icon; circle, square and hover effects.", new[] { new Size(44, 44), new Size(64, 64) }, o =>
            {
                Set(o, "Text", "A");
                Set(o, "Round", true);
            });

            Star("Tabs", "选项卡", "Tabs", GNav, "在同一区域切换多组内容，支持线条/卡片样式与页签滚动。", "Switch between groups of content in one area; line/card styles and scrolling.", new[] { new Size(520, 260), new Size(680, 320) }, o =>
            {
                for (int i = 1; i <= 3; i++)
                {
                    object tp = New("TabPage");
                    Set(tp, "Text", "选项卡 " + i);
                    var lbl = new Label { Text = "第 " + i + " 页的内容区域", Location = new Point(16, 14), AutoSize = true, ForeColor = Theme.Text };
                    ((Control)tp).Controls.Add(lbl);
                    R.AddToCollection(o, "Pages", tp);
                }
                Set(o, "SelectedIndex", 0);
            });

            Star("Table", "表格", "Table", GData, "展示行列数据：列定义、DataTable 数据源、固定表头、排序与编辑。", "Show rows and columns: column definitions, DataTable source, fixed header, sort and edit.", new[] { new Size(700, 280), new Size(900, 360) }, o =>
            {
                AddColumn(o, "Name", "姓名");
                AddColumn(o, "Age", "年龄");
                AddColumn(o, "City", "城市");
                var dt = new DataTable();
                dt.Columns.Add("Name", typeof(string));
                dt.Columns.Add("Age", typeof(int));
                dt.Columns.Add("City", typeof(string));
                dt.Rows.Add("张三", 28, "北京");
                dt.Rows.Add("李四", 34, "上海");
                dt.Rows.Add("王五", 19, "广州");
                dt.Rows.Add("赵六", 41, "深圳");
                dt.Rows.Add("钱七", 25, "杭州");
                Set(o, "DataSource", dt);
                Set(o, "AutoSizeColumnsMode", "Fill");
            });

            Star("Alert", "警告提示", "Alert", GDisplay, "在页面里给出成功/信息/警告/错误提示，可带标题、图标与关闭按钮。", "Inline success/info/warning/error message with title, icon and close button.", new[] { new Size(380, 48), new Size(480, 56) }, o =>
            {
                Set(o, "Text", "这是一条提示信息，用于告知用户当前状态。");
                Set(o, "TextTitle", "提示");
                Set(o, "Icon", "Info");
            });

            Star("Pagination", "分页", "Pagination", GNav, "长列表分页器：总条数、每页条数、当前页与快速跳转。", "Pager for long lists: total, page size, current page and quick jump.", new[] { new Size(340, 36), new Size(420, 40) }, o =>
            {
                Set(o, "Total", 200);
                Set(o, "PageSize", 10);
                Set(o, "Current", 1);
            });

            // ---- 其余常用控件（自动收录，不精讲） ----
            Auto("InputNumber", "数字输入框", "InputNumber", GInput, "只能输入数字，可带加减步进按钮。", "Numeric-only input with step buttons.", new[] { new Size(160, 36) }, o => Set(o, "Value", 12m));
            Auto("SelectMultiple", "多选下拉", "SelectMultiple", GInput, "下拉里勾选多个选项，选中项以标签显示。", "Multi-select drop-down; selected items shown as tags.", new[] { new Size(260, 36) }, o => Items(o, "Items", "选项一", "选项二", "选项三"));
            Auto("Radio", "单选框", "Radio", GInput, "在一组选项里只能选一个。", "Only one option can be selected in a group.", new[] { new Size(160, 24) }, o => { Set(o, "Text", "选项 A"); Set(o, "Checked", true); });
            Auto("Rate", "评分", "Rate", GInput, "星级评分，可半星、可清除。", "Star rating with half-star and clear support.", new[] { new Size(180, 32) }, o => Set(o, "Value", 3f));
            Auto("Segmented", "分段控制器", "Segmented", GInput, "在一排选项间切换，适合视图/筛选切换。", "Switch between inline options, for view or filter modes.", new[] { new Size(260, 36) }, o =>
            {
                foreach (string s in new[] { "选项一", "选项二", "选项三" })
                {
                    object it = New("SegmentedItem");
                    Set(it, "Text", s);
                    R.AddToCollection(o, "Items", it);
                }
                Set(o, "SelectIndex", 0);
            });
            Auto("TimePicker", "时间选择", "TimePicker", GInput, "选择时分秒，可输入或弹出面板。", "Pick a time of day from input or popup panel.", new[] { new Size(180, 36) }, o => Set(o, "Value", new DateTime(2026, 10, 26, 9, 30, 0)));
            Auto("ColorPicker", "颜色选择", "ColorPicker", GInput, "弹出取色面板选择颜色，可显示色值文本。", "Popup color palette; can show the color value text.", new[] { new Size(180, 36) }, o => { Set(o, "Value", Color.FromArgb(22, 119, 255)); Set(o, "ShowText", true); });
            Auto("Label", "文本标签", "Label", GDisplay, "显示一行文字，支持前缀/后缀、高亮与省略号。", "Shows a line of text with prefix/suffix, highlight and ellipsis.", new[] { new Size(180, 24), new Size(260, 32) }, o => Set(o, "Text", "标签文字"));
            Auto("Divider", "分割线", "Divider", GDisplay, "用横线隔开内容，可带居中文案。", "A horizontal line separating content, optionally with text.", new[] { new Size(300, 24) }, o => Set(o, "Text", "分割线"));
            Auto("Battery", "电量", "Battery", GDisplay, "以电池图形展示 0-100 的电量/容量。", "Battery-shaped gauge for 0-100 capacity.", new[] { new Size(120, 36) }, o => { Set(o, "Value", 75); Set(o, "ShowText", true); });
            Auto("Spin", "加载中", "Spin", GDisplay, "加载等待动画，可带文字说明。", "Loading spinner with optional text.", new[] { new Size(120, 120) }, o => Set(o, "Text", "加载中"));
            Auto("Breadcrumb", "面包屑", "Breadcrumb", GNav, "显示当前位置的层级路径，可点击跳转。", "Shows the hierarchy path of the current location.", new[] { new Size(320, 24) }, o =>
            {
                foreach (string s in new[] { "首页", "列表页", "详情页" })
                {
                    object it = New("BreadcrumbItem");
                    Set(it, "Text", s);
                    R.AddToCollection(o, "Items", it);
                }
            });
            Auto("Steps", "步骤条", "Steps", GNav, "展示多步骤流程的当前进度。", "Shows progress through a multi-step flow.", new[] { new Size(480, 80), new Size(480, 160) }, o =>
            {
                R.AddToCollection(o, "Items", NewItem("StepsItem", "已下单", "2026-10-20 09:00", "订单已创建"));
                R.AddToCollection(o, "Items", NewItem("StepsItem", "已付款", "2026-10-20 09:30", "等待发货"));
                R.AddToCollection(o, "Items", NewItem("StepsItem", "已发货", "2026-10-21 14:00", "运输中"));
                Set(o, "Current", 1);
            });
            Auto("Timeline", "时间轴", "Timeline", GDisplay, "按时间顺序展示事件列表。", "Chronological list of events.", new[] { new Size(320, 160) }, o =>
            {
                R.AddToCollection(o, "Items", NewItem("TimelineItem", "创建项目", "2026-10-20 09:00"));
                R.AddToCollection(o, "Items", NewItem("TimelineItem", "完成原型", "2026-10-24 18:00"));
                R.AddToCollection(o, "Items", NewItem("TimelineItem", "上线发布", "2026-10-26 09:00"));
            });

            Overrides();
        }

        private static Meta Star(string type, string zh, string en, string group, string introZh, string introEn, Size[] sizes, Action<object> init)
        {
            var m = new Meta { Type = type, Zh = zh, En = en, Group = group, IntroZh = introZh, IntroEn = introEn, Sizes = sizes, Init = init, Star = Metas.Count(x => x.Star > 0) + 1 };
            Metas.Add(m);
            ByName[type] = m;
            return m;
        }

        private static Meta Auto(string type, string zh, string en, string group, string introZh, string introEn, Size[] sizes, Action<object> init)
        {
            var m = new Meta { Type = type, Zh = zh, En = en, Group = group, IntroZh = introZh, IntroEn = introEn, Sizes = sizes, Init = init, Star = 0 };
            Metas.Add(m);
            ByName[type] = m;
            return m;
        }

        public static object New(string typeName, params object[] args)
        {
            Type t = Loader.Get(typeName);
            if (t == null) throw new InvalidOperationException("找不到类型 AntdUI." + typeName);
            return args == null || args.Length == 0 ? Activator.CreateInstance(t) : Activator.CreateInstance(t, args);
        }

        private static object NewItem(string typeName, params object[] args)
        {
            Type t = Loader.Get(typeName);
            return t == null ? null : Activator.CreateInstance(t, args);
        }

        private static void Set(object o, string prop, object v)
        {
            R.Set(o, prop, v);
        }

        private static void Items(object owner, string prop, params string[] texts)
        {
            foreach (string s in texts) R.AddToCollection(owner, prop, s);
        }

        private static void AddColumn(object table, string key, string title)
        {
            object col = New("Column", key, title);
            R.AddToCollection(table, "Columns", col);
        }

        /// <summary>所有可预览的类型名（含自动收录），按分组排序。</summary>
        public static IEnumerable<Meta> All()
        {
            return Metas.OrderBy(m => m.Group == GButton ? 0 : m.Group == GInput ? 1 : m.Group == GData ? 2 : m.Group == GDisplay ? 3 : m.Group == GNav ? 4 : 5)
                        .ThenByDescending(m => m.Curated).ThenBy(m => m.Star == 0 ? 999 : m.Star);
        }

        /// <summary>控件的默认尺寸（取预设第一个）。</summary>
        public static Size DefaultSize(Meta m)
        {
            return m != null && m.Sizes.Length > 0 ? m.Sizes[0] : new Size(240, 40);
        }

        /// <summary>精讲控件的逐属性/逐事件说明。</summary>
        private static void Overrides()
        {
            Docs.Over("Button", "Type", "风格", "按钮预设配色：Default 灰底、Primary 蓝底、Success 绿、Warn 黄、Error 红、Info 浅蓝。", "Preset color scheme: Default grey, Primary blue, Success green, Warn yellow, Error red, Info light blue.");
            Docs.Over("Button", "Shape", "形状", "Default 直角矩形、Round 胶囊形、Circle 正圆（配图标用）。", "Default rectangle, Round pill, Circle (for icon-only buttons).");
            Docs.Over("Button", "Ghost", "幽灵按钮", "true 时改成描边+透明底，适合放在彩色背景上。", "Ghost style: outline with transparent fill, good on colored backgrounds.");
            Docs.Over("Button", "Loading", "加载中", "按钮里出现转圈并禁用点击；异步提交时先置 true，完成后置 false。", "Shows a spinner and blocks clicks while an async action runs.", "记得在 finally 里把 Loading 关掉，否则按钮永久转圈。");
            Docs.Over("Button", "WaveSize", "波纹半径", "点击时水波纹的扩散半径（像素），0 关闭波纹。", "Ripple radius in pixels on click; 0 disables it.");
            Docs.Over("Button", "IconSvg", "图标", "按钮文字的 SVG 图标，配合 IconPosition 决定在左/右/上/下。", "SVG icon for the button; IconPosition decides which side it sits on.");
            Docs.Over("Button", "IconPosition", "图标位置", "Left / Right / Top / Bottom 之一，控制图标相对文字的位置。", "Left / Right / Top / Bottom, relative to the text.");
            Docs.Over("Button", "JoinMode", "拼接模式", "把相邻按钮合并成按钮组的接缝方向（Left/Right/LR/Top/Bottom）。", "Joins adjacent buttons into a group by removing the touching corners.");
            Docs.Over("Button", "DisplayStyle", "显示方式", "Default 默认、Text 纯文字、Link 链接样式，决定底色与交互反馈。", "Default / Text / Link appearance, which controls fill and hover feedback.");
            Docs.Over("Button", "ShowArrow", "显示箭头", "在文字后加一个 › 箭头，暗示会展开/跳转。", "Adds a trailing › arrow suggesting navigation or expansion.");
            Docs.Over("Button", "IsLink", "链接模式", "同 DisplayStyle=Link，显示为可点击链接。", "Renders as a clickable link (same as DisplayStyle=Link).");
            Docs.Over("Button", "Toggle", "切换态", "配合 AutoToggle，把按钮当开关用：true 表示已选中。", "With AutoToggle, treats the button as a toggle; true = selected.");
            Docs.Over("Button", "ToggleText", "切换文字", "切换态下显示的另一段文字，未设置则沿用 Text。", "Alternate text shown in the toggled state; falls back to Text.");
            Docs.OverEvent("Button", "ToggleChanged", "切换变化", "按钮在选中/未选中之间切换时触发，参数 RBoolEventArgs 带新值。", "Fires when the toggle state flips; RBoolEventArgs carries the new value.");
            Docs.OverEvent("Button", "DoubleClick", "双击", "快速点两下左键触发，注意会先触发一次 Click。", "Fires on a quick double left-click; a Click fires first.");

            Docs.Over("Input", "Status", "校验状态", "None/Success/Info/Warn/Error，把边框变绿/黄/红以提示校验结果。", "None/Success/Info/Warn/Error colors the border to signal validation.");
            Docs.Over("Input", "Variant", "输入框样式", "Outlined 四边框（默认），Underlined 只有下划线，像纸面表单。", "Outlined (full border, default) or Underlined (bottom line only).");
            Docs.Over("Input", "AllowClear", "清空按钮", "有内容时右侧出现 ⊗，点击清空并触发 ClearClick。", "Shows a ⊗ button when text exists; clicking clears and raises ClearClick.");
            Docs.Over("Input", "Multiline", "多行输入", "改成多行文本域，适合备注/描述，配合高度使用。", "Turns the field into a multi-line text area for notes/descriptions.");
            Docs.Over("Input", "UseSystemPasswordChar", "密码掩码", "把字符显示成 ●，用于密码输入。", "Masks characters as ● for password entry.");
            Docs.Over("Input", "MaxLength", "最大长度", "限制输入字符数，超出部分会被拦下。", "Caps the number of characters; extra input is rejected.");
            Docs.OverEvent("Input", "ClearClick", "清空点击", "点击清空按钮时触发，可在这里重置联动状态。", "Fires when the clear button is clicked; reset linked state here.");
            Docs.OverEvent("Input", "VerifyChar", "字符校验", "每输入一个字符时触发，可返回 false 拦截非法字符。", "Fires per typed character; return false to reject it.");

            Docs.Over("Select", "Items", "选项集合", "BaseCollection，代码里 Add(\"文本\") 或 Add(对象)，元素可以是任意对象。", "BaseCollection; add strings or objects in code, any object works.");
            Docs.Over("Select", "SelectedIndex", "选中下标", "当前选项在 Items 里的位置，-1 表示未选。", "Index in Items of the current selection; -1 = none.");
            Docs.Over("Select", "Placement", "弹出方向", "下拉面板相对控件的方向：Bottom 下方、Top 上方、TL/TR/BL/BR。", "Where the drop-down opens: Bottom, Top, TL/TR/BL/BR.");
            Docs.Over("Select", "MaxCount", "最大选中数", "多选时最多可选几项，0 表示不限。", "Max items for multi-select; 0 = unlimited.");
            Docs.OverEvent("Select", "SelectedIndexChanged", "选中变化", "用户选中另一项后触发，参数里带新下标。", "Fires after the user picks another item; args carry the new index.");
            Docs.OverEvent("Select", "ExpandDropChanged", "展开变化", "下拉面板展开/收起时触发，可用来懒加载数据。", "Fires when the drop-down opens/closes; useful for lazy loading.");

            Docs.Over("Checkbox", "Checked", "选中", "是否勾选；设为 true 会立刻画出对勾。", "Whether it is checked; setting true draws the tick immediately.");
            Docs.Over("Checkbox", "CheckState", "三态", "Checked/Unchecked/Indeterminate，用来表示“部分选中”。", "Checked / Unchecked / Indeterminate, for partial selection.");
            Docs.Over("Checkbox", "Fill", "选中颜色", "勾选方块/圆圈的颜色，默认品牌蓝。", "Fill color of the checked box/circle; default brand blue.");
            Docs.OverEvent("Checkbox", "CheckedChanged", "勾选变化", "勾选状态改变时触发，是复选框最常用的事件。", "Fires when the checked state changes — the most used event.");

            Docs.Over("Switch", "Checked", "开关状态", "true 为开（滑块在右），false 为关。", "True = on (knob right), false = off.");
            Docs.Over("Switch", "CheckedText", "开状态文字", "开关打开时显示在开关内部的文字，如“开”。", "Text inside the switch when it is on, e.g. \"ON\".");
            Docs.Over("Switch", "UnCheckedText", "关状态文字", "开关关闭时显示的文字，如“关”。", "Text inside the switch when it is off.");
            Docs.Over("Switch", "Gap", "滑块间距", "滑块与轨道边缘的像素间距，影响开关的紧凑感。", "Pixel gap between knob and track edge.");
            Docs.OverEvent("Switch", "CheckedChanged", "开关变化", "每次拨动开关触发，参数带新的 bool 值。", "Fires on every toggle; args carry the new bool value.");

            Docs.Over("Slider", "MinValue", "最小值", "滑块左端对应的值。", "Value at the left end of the track.");
            Docs.Over("Slider", "MaxValue", "最大值", "滑块右端对应的值。", "Value at the right end of the track.");
            Docs.Over("Slider", "ShowValue", "显示数值", "true 时在滑块旁显示当前数字。", "Shows the current number next to the handle.");
            Docs.Over("Slider", "Dots", "刻度点", "true 时在轨道上画出每个刻度的圆点。", "Draws a dot for each step on the track.");
            Docs.Over("Slider", "Marks", "标记文字", "在指定位置写字（如“慢/快”），SliderMarkItem 集合。", "Labels at given positions (e.g. Slow/Fast) via SliderMarkItem.");
            Docs.OverEvent("Slider", "ValueChanged", "值变化", "拖动过程中持续触发，参数带新数值。", "Fires continuously while dragging; args carry the new value.");

            Docs.Over("DatePicker", "Value", "日期值", "DateTime? 类型，null 表示清空。", "DateTime? value; null clears it.");
            Docs.Over("DatePicker", "Format", "显示格式", ".NET 日期格式串，例如 yyyy-MM-dd、yyyy年MM月dd日。", ".NET date format string, e.g. yyyy-MM-dd.");
            Docs.Over("DatePicker", "Picker", "选择粒度", "Date 选日、Month 选月、Year 选年。", "Date / Month / Year granularity.");
            Docs.Over("DatePicker", "Presets", "快捷选项", "面板底部的快捷日期（今天/昨天/近7天），DatePickerPresets 集合。", "Quick ranges at the panel bottom (today / yesterday / last 7 days).");
            Docs.OverEvent("DatePicker", "ValueChanged", "日期变化", "用户选定日期后触发，参数带 DateTime? 新值。", "Fires after a date is picked; args carry the DateTime? value.");

            Docs.Over("Progress", "Value", "进度值", "0~1 的 float，0.75 表示 75%。", "Float from 0 to 1; 0.75 = 75%.");
            Docs.Over("Progress", "Shape", "形态", "Default 圆角条、Circle 圆环、Round 细圆条、Mini 迷你条、Steps 分段。", "Default bar, Circle ring, Round thin bar, Mini bar, Steps segments.");
            Docs.Over("Progress", "State", "状态色", "None 跟随主题，Success 绿、Error 红、Warn 黄。", "None follows the theme; Success green, Error red, Warn yellow.");
            Docs.Over("Progress", "Steps", "分段数", "Shape=Steps 时的总段数，配合 StepsWidth/StepsGap 使用。", "Segment count when Shape=Steps.");
            Docs.Over("Progress", "GapDegree", "圆环缺口", "Shape=Circle 时的缺口角度，0~360，常用 90 做仪表盘。", "Gap angle for the Circle shape, 0–360; 90 = gauge style.");
            Docs.Over("Progress", "UseTextCenter", "文字居中", "true 时百分比文字画在进度条/圆环内部。", "Draws the percentage text inside the bar/ring.");

            Docs.Over("Tag", "Type", "标签风格", "Default 灰、Primary 蓝、Success 绿、Error 红、Warn 黄、Info 淡蓝。", "Default grey, Primary blue, Success green, Error red, Warn yellow, Info light blue.");
            Docs.Over("Tag", "CloseIcon", "关闭按钮", "true 时右侧显示 ×，点击触发 CloseChanged，可用来删除标签。", "Shows a × on the right; clicking raises CloseChanged (remove the tag).");
            Docs.OverEvent("Tag", "CloseChanged", "关闭点击", "点击标签的 × 时触发；通常在事件里把标签从父容器移除。", "Fires when the × is clicked; usually removes the tag from its parent.");

            Docs.Over("Badge", "Text", "徽标文字", "显示的数字或短文本，如 99+、New。", "The number or short text shown, e.g. 99+ or New.");
            Docs.Over("Badge", "State", "状态色", "Primary/Default/Success/Error/Processing/Warn，决定底色。", "Primary/Default/Success/Error/Processing/Warn — sets the fill color.");
            Docs.Over("Badge", "DotRatio", "圆点比例", "只显示小圆点时，圆点相对控件高度的比例。", "Dot size ratio when only a dot is shown.");

            Docs.Over("Avatar", "Round", "圆形头像", "true 时裁成圆形；否则是圆角方形。", "Clips to a circle; otherwise a rounded square.");
            Docs.Over("Avatar", "ImageSvg", "SVG 头像", "用矢量图标代替图片，颜色可跟随主题。", "Vector icon instead of a bitmap; color can follow the theme.");
            Docs.Over("Avatar", "Shadow", "阴影", "给头像加投影，让它从背景中浮起。", "Adds a drop shadow so the avatar lifts off the background.");
            Docs.Over("Avatar", "EnableHover", "悬停效果", "true 时鼠标移入显示悬停底色/替换图。", "Enables hover background / hover image.");

            Docs.Over("Tabs", "Pages", "页面集合", "TabCollection，用 Pages.Add(new TabPage{Text=\"名称\"}) 添加页。", "TabCollection; add pages with Pages.Add(new TabPage{Text=\"name\"}).");
            Docs.Over("Tabs", "SelectedIndex", "当前页下标", "从 0 开始的当前页；设为 -1 会隐藏所有页。", "Zero-based index of the active page; -1 hides all pages.");
            Docs.Over("Tabs", "Type", "页签样式", "Line 下划线、Card 卡片、Card2 另一种卡片，决定页签外观。", "Line, Card, Card2 — the visual style of the tab headers.");
            Docs.Over("Tabs", "TypExceed", "溢出处理", "页签太多时的处理：None 压缩、Button 显示箭头、LR 左右滚动。", "Behavior when tabs overflow: None, Button arrows, LR scrolling.");
            Docs.Over("Tabs", "Centered", "居中", "true 时页签整排居中，否则靠左。", "Centers the row of tabs instead of aligning left.");
            Docs.Over("Tabs", "ItemSize", "页签尺寸", "固定每个页签的宽高，null 表示按内容自适应。", "Fixed size of each tab; null = fit content.");
            Docs.OverEvent("Tabs", "SelectedIndexChanged", "页签切换", "切换到另一页后触发，可在这里懒加载该页数据。", "Fires after switching pages — a good place to lazy-load data.");
            Docs.OverEvent("Tabs", "ClosingPage", "关闭页签", "页签可关闭时，点 × 触发，可取消关闭。", "Fires when a closable tab's × is clicked; can cancel closing.");

            Docs.Over("Table", "Columns", "列定义", "Column(key, title) 集合，key 对应 DataTable 列名/对象属性名。", "Collection of Column(key, title); key maps to a DataTable column or property name.");
            Docs.Over("Table", "DataSource", "数据源", "可以是 DataTable、DataView 或对象列表（每项一行）。", "A DataTable, DataView or list of objects (one row each).");
            Docs.Over("Table", "Bordered", "单元格边框", "true 时每个单元格都画边框，否则只有行分隔线。", "Draws a border around every cell instead of just row separators.");
            Docs.Over("Table", "FixedHeader", "固定表头", "数据超出高度时可滚动，表头保持可见。", "Keeps the header visible while the body scrolls.");
            Docs.Over("Table", "RowHeight", "行高", "固定每行高度（像素），null 表示按字体自动计算。", "Fixed row height in pixels; null = auto from font.");
            Docs.Over("Table", "EmptyText", "空数据文字", "没有数据时显示的占位文字。", "Placeholder text shown when there is no data.");
            Docs.Over("Table", "SortMode", "排序模式", "NONE 不排序、AUTO 点击表头自动排序、HANDLE 交给事件处理。", "NONE / AUTO (click header) / HANDLE (you sort in the event).");
            Docs.OverEvent("Table", "CellClick", "单元格点击", "点击任意单元格触发，参数带行、列与值，常用于打开详情。", "Fires on a cell click with row/column/value — often opens details.");
            Docs.OverEvent("Table", "SelectIndexChanged", "选中行变化", "选中的行变化时触发（MultipleRows 时可多行）。", "Fires when the selected row(s) change.");
            Docs.OverEvent("Table", "CellButtonClick", "单元格按钮", "点击单元格里的按钮（Column 里配置按钮）时触发。", "Fires when a button inside a cell is clicked.");

            Docs.Over("Alert", "Text", "提示正文", "提示的主体文字，可多行自动换行。", "Main message text; wraps automatically.");
            Docs.Over("Alert", "TextTitle", "提示标题", "加粗的标题行，可留空只显示正文。", "Bold title line; leave empty for message-only alerts.");
            Docs.Over("Alert", "Icon", "图标类型", "左侧图标：None/Success/Info/Warn/Error/Processing，可换自定义 SVG。", "Leading icon type: None/Success/Info/Warn/Error, or a custom SVG.");
            Docs.Over("Alert", "CloseIcon", "关闭按钮", "true 时右侧出现 ×，用户可自行关闭提示。", "Shows a × so the user can dismiss the alert.");
            Docs.Over("Alert", "Loop", "跑马灯", "true 时文字过长会循环滚动显示。", "Scrolls the text in a loop when it overflows.");
            Docs.OverEvent("Alert", "CloseChanged", "关闭点击", "点击 × 时触发，通常在事件里把 Alert 隐藏或移除。", "Fires when × is clicked; hide or remove the alert here.");

            Docs.Over("Pagination", "Total", "总条数", "数据总条数，分页器据此算出总页数。", "Total record count; the pager derives the page count.");
            Docs.Over("Pagination", "PageSize", "每页条数", "一页显示多少条，改变时会触发 ValueChanged。", "Records per page; changing it raises ValueChanged.");
            Docs.Over("Pagination", "Current", "当前页", "1 开始的当前页码。", "Current page number, starting at 1.");
            Docs.Over("Pagination", "ShowSizeChanger", "每页条数选择", "true 时显示“10 条/页”的下拉框。", "Shows the page-size drop-down (10 / page).");
            Docs.OverEvent("Pagination", "ValueChanged", "翻页", "页码或每页条数改变时触发，参数带新的页码/条数。", "Fires when page or page size changes.");
        }
    }
}
