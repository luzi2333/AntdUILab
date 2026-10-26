using System;
using System.Collections.Generic;
using System.Reflection;

namespace AntdUILab.App
{
    /// <summary>一条属性 / 事件的说明：中文名、英文名、作用（中英）、注意点。</summary>
    public class Doc
    {
        public string En = "";
        public string Zh = "";
        public string EffectZh = "";
        public string EffectEn = "";
        public string Pit = "";

        public Doc() { }

        public Doc(string en, string zh, string effectZh, string effectEn, string pit = "")
        {
            En = en; Zh = zh; EffectZh = effectZh; EffectEn = effectEn; Pit = pit;
        }

        public Doc Clone()
        {
            return new Doc(En, Zh, EffectZh, EffectEn, Pit);
        }
    }

    /// <summary>全局说明表 + 控件级覆盖；查不到的成员回退到 AntdUI.xml 的中文注释。</summary>
    public static class Docs
    {
        /// <summary>属性名 → 说明（所有控件通用成员）。</summary>
        public static readonly Dictionary<string, Doc> Props = new Dictionary<string, Doc>(StringComparer.OrdinalIgnoreCase);

        /// <summary>"类型.成员" → 说明（覆盖全局表）。</summary>
        public static readonly Dictionary<string, Doc> Overrides = new Dictionary<string, Doc>(StringComparer.OrdinalIgnoreCase);

        /// <summary>事件名 → 说明（所有控件通用事件）。</summary>
        public static readonly Dictionary<string, Doc> Events = new Dictionary<string, Doc>(StringComparer.OrdinalIgnoreCase);

        static Docs()
        {
            #region 通用属性 General
            Add("Name", "名称", "控件在代码中的名字，用来在事件里找到它。", "The identifier used to find this control in code.");
            Add("Text", "文本", "控件显示的文字内容，会随语言/内容变化改变外观。", "The text shown on the control; changing it changes its look.");
            Add("Enabled", "启用", "false 时控件变灰、无法点击，用于禁用表单。", "When false the control is greyed out and cannot be clicked.");
            Add("Visible", "可见", "false 时控件隐藏，但仍存在于内存中。", "When false the control is hidden but still exists.");
            Add("BackColor", "背景色", "控件背景的颜色（WinForms 标准属性）。", "Background color of the control.");
            Add("ForeColor", "文字颜色", "控件文字的颜色（WinForms 标准属性）。", "Text color of the control.");
            Add("Font", "字体", "控件使用的字体、字号与字形。", "Font family, size and style used by the control.");
            Add("Cursor", "鼠标指针", "鼠标移到控件上时的指针形状。", "Mouse cursor shape while hovering the control.");
            Add("Dock", "停靠", "让控件贴着父容器的一条边并自动拉伸，如 Top/Bottom/Fill。", "Dock the control to an edge of its parent and auto-resize.");
            Add("Anchor", "锚定", "窗口缩放时控件相对某个角/边的距离保持不变。", "Keep the control's distance to the anchored edges while resizing.");
            Add("Location", "位置", "控件左上角相对父容器的坐标。", "Top-left corner position inside the parent.", "舞台会同步移动控件；想用鼠标挪位置，点工具栏「✥ 拖动」。");
            Add("Size", "尺寸", "控件的宽度与高度，单位像素。", "Width and height of the control in pixels.", "拖舞台上控件右下角的圆点也能改。");
            Add("Margin", "外边距", "控件与相邻控件之间的间距（受父级 Layout 影响）。", "Space around the control used by its parent layout.");
            Add("Padding", "内边距", "控件内容与自身边框之间的距离。", "Space between the control border and its content.");
            Add("MinimumSize", "最小尺寸", "缩小时的下限，0 × 0 表示不限制。", "Lower bound when the control shrinks; 0 × 0 means unlimited.");
            Add("MaximumSize", "最大尺寸", "放大时的上限，0 × 0 表示不限制。", "Upper bound when the control grows; 0 × 0 means unlimited.");
            Add("TabIndex", "Tab 顺序", "按 Tab 键时获得焦点的先后顺序。", "Focus order when the user presses Tab.");
            Add("TabStop", "接受 Tab 键", "是否允许用 Tab 键把焦点移到本控件。", "Whether the Tab key can focus this control.");
            Add("Locked", "锁定", "设计器里锁定控件，运行时无影响。", "Locks editing in the designer; no effect at runtime.");
            Add("RightToLeft", "从右到左", "RTL 语言（阿拉伯语等）的排版方向。", "Right-to-left layout direction for RTL languages.");
            #endregion

            #region AntdUI 通用外观 AntdUI look
            Add("Radius", "圆角", "控件四个角的圆角半径，0 为直角，越大越圆。", "Corner radius in pixels; 0 = square, bigger = rounder.", "圆角过大时文字/图标可能贴边。");
            Add("Round", "全圆角", "true 时把胶囊形/圆形圆角开到最大，常用于按钮、输入框。", "Turns on the pill/circle corner shape (capsule-like).");
            Add("BorderWidth", "边框宽度", "描边线条的像素宽度，0 表示不画边框。", "Border stroke width in pixels; 0 = no border.");
            Add("WaveSize", "波纹大小", "点击时水波纹动画的半径，越大波纹扩散越远。", "Radius of the click ripple animation, in pixels.");
            Add("Loading", "加载中", "true 时显示转圈并阻止交互，用于异步按钮。", "Shows a spinner and blocks interaction, for async actions.");
            Add("Type", "风格", "预设视觉风格（Primary/Default/Success/Warn/Error/Info）。", "Preset visual style (Primary/Default/Success/Warn/Error/Info).");
            Add("Shape", "形状", "控件的几何预设（Default 方、Circle 圆、Round 胶囊）。", "Geometric preset: Default square, Circle, Round pill.");
            Add("Status", "状态色", "提示用户当前值是否合法：Success/Error/Warn。", "Colors the control by state: Success / Error / Warn.");
            Add("Variant", "变体", "外观变体：Outlined 有边框，Underlined 只有下划线。", "Appearance variant: Outlined (bordered) or Underlined (bottom line only).");
            Add("Fill", "主色", "选中/高亮状态使用的主颜色（默认品牌蓝）。", "Accent color used for selected / highlighted state.");
            Add("FillHover", "悬停色", "鼠标悬停时的填充颜色，用于提供反馈。", "Fill color while the mouse hovers, for feedback.");
            Add("Fore", "前景色", "文字与图标的主颜色。", "Main color for text and icons.");
            Add("Back", "背景色", "控件填充背景色。", "Fill / background color of the control.");
            Add("TextAlign", "内容对齐", "控件内部文字/内容的水平对齐方式。", "Horizontal alignment of the content inside the control.");
            #endregion

            #region 输入类 Inputs
            Add("PlaceholderText", "占位提示", "值为空时显示的浅色提示文字，说明该填什么。", "Hint text shown when the value is empty.");
            Add("ReadOnly", "只读", "true 时内容不可编辑，但仍可选中复制。", "When true the text cannot be edited but can be selected/copied.");
            Add("Multiline", "多行", "true 时输入框换行，可输入多行文本。", "Allows multiple lines of text.");
            Add("MaxLength", "最大长度", "允许输入的字符数上限，0 表示不限制。", "Maximum number of characters; 0 = unlimited.");
            Add("UseSystemPasswordChar", "密码模式", "把输入内容显示为圆点，用于密码框。", "Masks typed characters as dots (password field).");
            Add("AllowClear", "允许清空", "true 时右侧出现一键清空按钮。", "Shows a clear button on the right.");
            Add("PrefixText", "前缀文字", "输入框左侧固定文字，如 https://、¥。", "Fixed text before the input, e.g. https:// or ¥.");
            Add("SuffixText", "后缀文字", "输入框右侧固定文字，如 .com、元。", "Fixed text after the input, e.g. .com or kg.");
            Add("PrefixSvg", "前缀图标", "输入框左侧的 SVG 图标字符串。", "SVG icon string shown before the text.");
            Add("SuffixSvg", "后缀图标", "输入框右侧的 SVG 图标字符串。", "SVG icon string shown after the text.");
            Add("Items", "选项集合", "下拉/列表类控件的数据源，需要代码创建元素。", "Item collection for drop-down controls; built in code.");
            Add("SelectedIndex", "选中序号", "当前选中的条目下标，-1 表示未选中。", "Index of the selected item; -1 = nothing selected.");
            Add("SelectedValue", "选中值", "当前选中条目的对象值。", "Object value of the selected item.");
            Add("Checked", "选中", "true 表示勾选/打开状态。", "True = checked / on.");
            Add("CheckState", "勾选状态", "三态勾选：Unchecked / Checked / Indeterminate。", "Tri-state: Unchecked / Checked / Indeterminate.");
            Add("AutoCheck", "自动切换", "点击时是否自动翻转选中状态。", "Whether clicking toggles the checked state automatically.");
            Add("MinValue", "最小值", "可选取的范围下界。", "Lower bound of the selectable range.");
            Add("MaxValue", "最大值", "可选取的范围上界。", "Upper bound of the selectable range.");
            Add("Value", "当前值", "控件当前的值，读写都会触发 ValueChanged。", "Current value; changing it raises ValueChanged.");
            Add("ShowValue", "显示数值", "true 时在滑块/进度旁显示数字。", "Shows the numeric value next to the slider/progress.");
            Add("Format", "格式串", "日期/数字的显示格式，如 yyyy-MM-dd。", "Display format string, e.g. yyyy-MM-dd.");
            Add("Placement", "弹出方向", "下拉面板相对控件弹出的位置。", "Where the drop-down panel opens relative to the control.");
            Add("Gap", "间距", "子元素之间的像素间隙。", "Pixel gap between inner elements.");
            #endregion

            #region 展示/数据 Display & data
            Add("Columns", "列集合", "表格列定义集合，用 Column(key,title) 创建列。", "Table column definitions, created with Column(key, title).");
            Add("DataSource", "数据源", "表格数据：DataTable、DataView 或对象列表。", "Table data: DataTable, DataView or a list of objects.");
            Add("Bordered", "显示边框", "true 时给表格画上单元格边框。", "Draws cell borders when true.");
            Add("FixedHeader", "固定表头", "滚动数据时表头固定在顶部。", "Keeps the header fixed while scrolling.");
            Add("EmptyText", "空数据提示", "没有数据时显示的占位文字。", "Text shown when there is no data.");
            Add("Pages", "页面集合", "选项卡的页面集合，用 TabPage 添加。", "Tab page collection; add TabPage objects.");
            Add("SelectedTab", "当前页", "当前显示的 TabPage 对象。", "The currently displayed TabPage.");
            Add("Centered", "居中", "true 时页签/内容整体居中排列。", "Centers the tabs/content.");
            Add("Image", "图片", "控件显示的位图对象。", "Bitmap image shown by the control.");
            Add("ImageSvg", "SVG 图标", "用 SVG 字符串代替图片，矢量、随主题着色。", "SVG string instead of a bitmap; vector and theme-aware.");
            Add("TextTitle", "标题", "提示/卡片类控件的主标题。", "Title of an alert/card-like control.");
            Add("CloseIcon", "关闭图标", "true 时显示 × 关闭按钮。", "Shows a × close button.");
            Add("ShowIcon", "显示图标", "true 时显示左侧状态图标。", "Shows the leading status icon.");
            Add("Shadow", "阴影", "true 时给控件加投影，增强层次。", "Adds a drop shadow for depth.");
            Add("DotRatio", "圆点比例", "小圆点模式下圆点大小的比例。", "Size ratio of the dot mode.");
            Add("BorderColor", "边框颜色", "描边使用的颜色，默认取主题边线色。", "Border stroke color; defaults to the theme line color.");
            Add("ItemBorderColor", "子项边框色", "分页/菜单等内部子项的描边颜色。", "Border color of inner items (pager, menu …).");
            Add("ItemBorderWidth", "子项边框宽", "内部子项描边的像素宽度，0 为无边框。", "Stroke width of inner items in pixels; 0 = none.");
            Add("LineHeight", "行高", "多行文本每一行的像素高度。", "Pixel height of one text line.");
            Add("ShowSymbol", "显示符号", "true 时在数值前后显示 %、¥ 等符号。", "Shows symbols such as % or ¥ around the value.");
            Add("TextDesc", "总计文字", "分页右侧“共 N 条”一类说明文字的模板。", "Template of the summary text shown next to a pager.");
            Add("SizeChangerWidth", "页大小下拉宽", "每页条数下拉框的宽度（像素）。", "Width in pixels of the page-size selector.");
            Add("CirSize", "转圈直径", "加载转圈的直径（像素）。", "Diameter of the spinner in pixels.");
            Add("CirWidth", "转圈线宽", "加载转圈线条的粗细（像素）。", "Stroke width of the spinner in pixels.");
            Add("Indicator", "指示器", "自定义加载指示器的位图。", "Custom bitmap used as the loading indicator.");
            Add("IndicatorSvg", "指示器 SVG", "用 SVG 字符串自定义加载指示器，矢量不糊。", "Custom SVG string for the loading indicator.");
            Add("FontDescription", "描述文字字体", "子标题/描述文字使用的字体。", "Font used for the description / sub-title text.");
            Add("DialogResult", "对话框结果", "点击后给父窗体返回的 DialogResult（配合 ShowDialog）。", "DialogResult returned to the owner form when clicked (with ShowDialog).");
            Add("Dpi", "DPI 缩放", "控件当前使用的 DPI 缩放比例，1.0 = 100%。", "DPI scale factor in use; 1.0 = 100%.");
            Add("HandCursor", "手型指针", "true 时鼠标移入显示手型，提示可点击。", "Shows a hand cursor on hover when true.");
            Add("PauseLayout", "暂停布局", "批量改属性时先暂停排版，改完再恢复，减少闪烁。", "Suspends layout while changing many properties, then resumes.");
            Add("VirtualWidth", "虚拟宽度", "内容比控件宽时的虚拟滚动宽度。", "Virtual scroll width when content is wider than the control.");
            Add("BorderRenderMode", "边框绘制方式", "控制边框画在控件内侧还是外侧，影响它与相邻控件的贴合。", "Whether the border is drawn inside or outside the bounds.");
            Add("CellImpactHeight", "单元格影响高度", "表格单元格额外占用的高度，用于放按钮/多行内容。", "Extra cell height reserved for buttons or multi-line content.");
            Add("SummaryCustomize", "自定义汇总", "true 时汇总行由你自行绘制，而不是自动汇总。", "Draw the summary row yourself instead of the built-in one.");
            Add("SummaryRowFocused", "汇总行可聚焦", "true 时汇总行也能拿到焦点并参与键盘操作。", "Lets the summary row take focus and keyboard input.");
            Add("TabMenuVisible", "页签菜单按钮", "true 时在页签栏右侧显示“更多页签”的下拉按钮。", "Shows the more-tabs dropdown button on the tab strip.");
            Add("ColorScheme", "配色方案", "控件在浅色/深色下的取色方式，null 表示跟随全局 Config.Mode。", "Light/dark color scheme source; null follows the global Config.Mode.");
            Add("BadgeBorderWidth", "徽标边框宽", "角标描边的像素宽度，0 为无边框。", "Stroke width of the corner badge in pixels; 0 = no border.");
            Add("BadgeBorderColor", "徽标边框色", "角标描边的颜色。", "Border color of the corner badge.");
            Add("BadgeOffsetX", "徽标横向偏移", "角标相对默认位置的横向偏移（像素），正数向右。", "Horizontal offset of the corner badge in pixels.");
            Add("BadgeOffsetY", "徽标纵向偏移", "角标相对默认位置的纵向偏移（像素），正数向下。", "Vertical offset of the corner badge in pixels.");
            Add("IconRatio", "图标比例", "图标相对文字高度的缩放比例，1 表示同高。", "Icon size ratio against the text height; 1 = same height.");
            Add("IconRatioRight", "右侧图标比例", "右侧图标（下拉箭头等）相对文字的缩放比例。", "Size ratio of the right-side icon (e.g. chevron).");
            Add("IconAlign", "图标对齐", "图标在文字左侧还是右侧，以及是否跟随内容居中。", "Where the icon sits relative to the text.");
            Add("HasFocus", "是否有焦点", "只读：控件当前是否拥有键盘焦点。", "Read-only: whether the control currently has keyboard focus.");
            Add("IsTextEmpty", "文本是否为空", "只读：输入内容是否为空，可用来控制按钮可用性。", "Read-only: whether the input text is empty.");
            Add("JoinLeft", "左侧拼接", "true 时与左边同组控件拼接成一体（去中间圆角）。", "Joins with the left neighbor into one segmented control.");
            Add("JoinRight", "右侧拼接", "true 时与右边同组控件拼接成一体。", "Joins with the right neighbor into one segmented control.");
            Add("Prefix", "前缀内容", "输入框左侧的前缀内容（文字或图标），常用于 ¥、搜索等。", "Left-side prefix content of an input (text or icon).");
            Add("Suffix", "后缀内容", "输入框右侧的后缀内容，常用于单位、快捷按钮。", "Right-side suffix content of an input, e.g. a unit.");
            Add("PrefixWidth", "前缀宽度", "前缀区域占用的宽度，null 表示按内容自适应。", "Width reserved for the prefix area; null = auto.");
            Add("SuffixWidth", "后缀宽度", "后缀区域占用的宽度，null 表示按内容自适应。", "Width reserved for the suffix area; null = auto.");
            Add("PrefixFore", "前缀颜色", "前缀文字/图标的颜色，默认取次要文字色。", "Color of the prefix content; defaults to the secondary text color.");
            Add("SuffixFore", "后缀颜色", "后缀文字/图标的颜色。", "Color of the suffix content.");
            Add("PrefixFormat", "前缀格式", "前缀内容的格式字符串，可带入数值等占位符。", "Format string used to render the prefix.");
            Add("SuffixFormat", "后缀格式", "后缀内容的格式字符串。", "Format string used to render the suffix.");
            Add("PrefixIcon", "前缀图标", "前缀位置的图标（位图），与文字前缀二选一。", "Bitmap icon shown in the prefix area.");
            Add("SuffixIcon", "后缀图标", "后缀位置的图标（位图）。", "Bitmap icon shown in the suffix area.");
            Add("PasswordChar", "密码掩码字符", "密码模式下每个字符显示的替代符号，默认 ●。", "Replacement character shown in password mode; default ●.");
            Add("PasswordCopy", "允许复制密码", "true 时允许 Ctrl+C 复制密码内容。", "Allows copying the password text with Ctrl+C.");
            Add("PasswordPaste", "允许粘贴密码", "true 时允许把内容粘贴到密码框。", "Allows pasting into the password field.");
            Add("CaretVisible", "显示光标", "true 时绘制输入光标（插入符）。", "Draws the text caret when true.");
            Add("CaretColor", "光标颜色", "输入光标的颜色，默认主题蓝。", "Color of the text caret.");
            Add("CaretSpeed", "光标闪烁速度", "输入光标闪烁一次的毫秒数，越小闪得越快。", "Caret blink interval in milliseconds.");
            Add("SelectionStart", "选区起点", "选中文本的起始字符下标，0 表示最前面。", "Zero-based start index of the selected text.");
            Add("SelectionLength", "选区长度", "选中文本的字符个数，0 表示没有选中。", "Number of selected characters; 0 = no selection.");
            Add("SelectionColor", "选区颜色", "选中文本高亮使用的颜色。", "Highlight color of the selected text.");
            Add("HideSelection", "失焦隐藏选区", "true 时控件失去焦点后不再显示选中高亮。", "Hides the selection highlight when the control loses focus.");
            Add("SetTextSelectionEnd", "选区移到末尾", "true 时把光标/选区直接放到文本末尾，适合日志框。", "Moves the caret/selection to the end of the text.");
            Add("CanUndo", "可撤销", "只读：当前是否有可撤销的操作。", "Read-only: whether an undo action is available.");
            Add("CanRedo", "可重做", "只读：当前是否有可重做的操作。", "Read-only: whether a redo action is available.");
            Add("WordWrap", "自动换行", "true 时长文本按控件宽度折行显示。", "Wraps long text to the control width.");
            Add("AcceptsTab", "接受 Tab", "多行输入时 true 表示 Tab 键输入制表符而不是切焦点。", "In multiline mode, Tab inserts a tab instead of moving focus.");
            Add("AcceptsEscape", "接受 Esc", "true 时把 Esc 交给控件处理，而不是关闭所在的对话框。", "Let the control handle Esc instead of the dialog.");
            Add("EmojiFont", "Emoji 字体", "绘制 emoji 时使用的字体，保证彩色 emoji 显示正常。", "Font used to render emoji characters.");
            Add("TextTotalLine", "总行数", "只读：多行文本的总行数，可用于行号/滚动计算。", "Read-only: total line count of multiline text.");
            Add("PaddGap", "内边距间距", "内容与边框之间的额外间距（像素）。", "Extra gap between the border and the content.");
            Add("PaddingLR", "左右内边距", "控件左右两侧的内边距（像素）。", "Left/right padding in pixels.");
            Add("ShowText", "显示文字", "true 时显示数值文字（进度百分比、滑块值等）。", "Shows the value text, e.g. a percentage or slider value.");
            Add("Tooltips", "提示文字数组", "每个取值对应的悬停提示，用字符串数组一次配好。", "Hover tooltip per value, given as a string array.");
            Add("TooltipConfig", "提示配置", "Tooltip 的样式/位置配置对象，需代码构建。", "Tooltip style/position config object; set in code.");
            Add("Trigger", "触发方式", "面板弹出的触发方式（点击、悬停等）。", "How the popup is triggered: click, hover …");
            Add("DropDownArrow", "下拉箭头", "true 时显示下拉箭头图标。", "Shows the dropdown chevron.");
            Add("DropDownPadding", "下拉内边距", "下拉列表每一行的内边距。", "Padding of each dropdown row.");
            Add("DropDownFontRatio", "下拉字号比例", "下拉列表字号相对主控件的比例。", "Dropdown font size ratio against the control font.");
            Add("Minimum", "最小值", "数值/进度允许的最小值（同步到 MinValue）。", "Minimum allowed value (mirrors MinValue).");
            Add("Maximum", "最大值", "数值/进度允许的最大值（同步到 MaxValue）。", "Maximum allowed value (mirrors MaxValue).");
            Add("Orientation", "方向", "水平或垂直排列，用于分隔线、步骤条等。", "Horizontal or vertical orientation.");
            Add("Separator", "分隔符", "面包屑/文本之间显示的分隔符。", "Separator shown between crumbs/segments.");
            Add("Full", "撑满宽度", "true 时控件把可用宽度撑满。", "Stretches the control to the available width.");
            Add("Rotate", "旋转", "文字旋转角度（度），用于竖排标签。", "Text rotation angle in degrees.");
            Add("Highlight", "高亮", "true 时用品牌色突出显示（用于标题、关键词）。", "Highlights the text with the accent color.");
            Add("ImageFit", "图片填充方式", "图片在框内如何缩放：contain 完整显示 / cover 铺满裁切。", "How the image fits its box: contain or cover.");
            Add("PlaceholderColor", "占位文字颜色", "输入框占位提示文字的颜色。", "Color of the placeholder text.");
            Add("TrackColor", "轨道颜色", "滑块/开关未选中的轨道颜色。", "Track color of a slider/switch when off.");
            Add("LineSize", "线条粗细", "轨道/边框线条的像素粗细。", "Pixel thickness of the track or border line.");
            Add("Lines", "分割线条数", "文本中可见的分割线/行数。", "Number of divider lines rendered.");
            Add("Gaps", "间距集合", "多个间距值，用于分栏/网格布局。", "Spacing values used by multi-column layouts.");
            Add("GapCell", "单元格间距", "表格单元格之间的空隙。", "Gap between table cells.");
            Add("ItemGap", "子项间距", "子项（页签、分段项）之间的距离。", "Gap between items (tabs, segments …).");
            Add("StepGap", "步骤间距", "步骤条相邻步骤之间的距离。", "Gap between two steps.");
            Add("StepSize", "步骤圆点大小", "步骤条圆点的直径（像素）。", "Diameter of the step dot in pixels.");
            Add("SwitchSize", "开关尺寸", "开关滑块的尺寸预设或像素高度。", "Size of the switch thumb/height.");
            Add("TagBordered", "标签边框", "true 时标签显示描边（描边风格）。", "Draws a bordered tag when true.");
            Add("VirtualMode", "虚拟模式", "true 时只渲染可见区域的行，适合大数据量。", "Renders only visible rows; use it for large data.");
            Add("UseContextMenu", "使用右键菜单", "true 时启用控件内置的右键菜单（复制/粘贴等）。", "Enables the built-in context menu (copy/paste …).");
            Add("ListAutoWidth", "列表自适应宽度", "true 时下拉列表宽度跟随内容，而不是控件宽度。", "Dropdown width follows its content instead of the control.");
            Add("List", "下拉列表", "下拉内容控件（List），可塞自定义行模板。", "The dropdown content control (List) for custom rows.");
            Add("LoadingIcon", "加载图标", "加载转圈用的自定义位图图标。", "Custom bitmap used as the loading icon.");
            Add("LoadingFull", "加载铺满", "true 时加载遮罩铺满整个控件，而不是只盖内容区。", "Covers the whole control with the loading mask.");
            Add("LoadingProgress", "加载进度值", "加载遮罩上显示的进度（0-1）。", "Progress value (0-1) shown on the loading mask.");
            Add("LoadingRespondClick", "加载时可点击", "true 时加载中仍然响应点击。", "Keeps responding to clicks while loading.");
            Add("LostFocusClearSelection", "失焦清空选区", "true 时失去焦点就清除选中，而不是保留高亮。", "Clears the selection when focus is lost.");
            Add("BorderActive", "激活边框色", "按下/激活状态下的描边颜色。", "Border color in the active (pressed) state.");
            Add("BorderHover", "悬停边框色", "鼠标悬停时的描边颜色。", "Border color on hover.");
            Add("JoinMode", "拼接模式", "与同组控件拼接的方向：左、右、左右、上、下、上下。", "How the control joins its group: left/right/top/bottom …");
            Add("PlaceholderColorExtend", "占位色扩展", "占位文字在失焦/聚焦时的渐变色，用于高级样式。", "Extended placeholder color used for gradient states.");
            Add("TextMultiLine", "多行文本", "true 时按多行绘制，行数由控件高度决定。", "Draws the text on multiple lines.");
            Add("UseMnemonic", "使用助记符", "true 时把 & 后的字母当作快捷键（显示下划线）。", "Treats & as a mnemonic prefix and underlines it.");
            Add("ShadowColor", "阴影颜色", "投影的颜色，通常带透明度。", "Color of the drop shadow, usually semi-transparent.");
            Add("ShadowOffsetX", "阴影横向偏移", "投影相对控件的横向偏移（像素）。", "Horizontal shadow offset in pixels.");
            Add("ShadowOffsetY", "阴影纵向偏移", "投影相对控件的纵向偏移（像素）。", "Vertical shadow offset in pixels.");
            Add("ShadowOpacity", "阴影不透明度", "0-1 的阴影浓度，越大越明显。", "Shadow opacity from 0 to 1.");
            Add("WheelModifyEnabled", "滚轮改值", "true 时鼠标滚轮可以增减数值。", "Lets the mouse wheel change the value.");
            Add("KeyboardModifyEnabled", "键盘改值", "true 时可用上下方向键增减数值。", "Lets arrow keys change the value.");
            Add("ValueTimeHorizontal", "时间横向排列", "true 时时分秒横向排在一行，否则纵向。", "Lays the time units horizontally.");
            Add("IconCircle", "圆形图标底", "true 时给图标加一个圆形/圆角底色。", "Draws a circular background behind the icon.");
            Add("IconCircleColor", "圆底颜色", "圆形图标底使用的填充色。", "Fill color of the circular icon background.");
            Add("IconCircleAngle", "圆底圆角", "圆底的圆角半径，45 左右接近圆形。", "Corner radius of the icon background.");
            Add("IconCirclePadding", "圆底内边距", "图标与圆底边缘之间留出的距离。", "Padding between the icon and its circular background.");
            Add("IconSvgCircle", "圆形图标 SVG", "圆形底色上叠加的 SVG 图标。", "SVG icon drawn over the circular background.");
            Add("IconCustom", "自定义图标", "用回调自行绘制图标，完全交给代码控制。", "Custom icon drawn by your own callback.");
            Add("EmptyImage", "空状态图片", "没有数据时显示的空状态插图。", "Illustration shown when there is no data.");
            Add("EnableHeaderResizing", "允许调整列宽", "true 时可以用鼠标拖动表头分隔线改变列宽。", "Lets the user drag column borders to resize.");
            Add("EnableFocusNavigation", "焦点导航", "true 时可以用方向键在单元格之间移动焦点。", "Arrow keys move focus between cells.");
            Add("FocusNavigationAutoScroll", "焦点自动滚动", "焦点移动时自动滚动，让目标单元格保持可见。", "Auto-scrolls so the focused cell stays visible.");
            Add("EnablePageScrolling", "页签滚动", "true 时页签太多可以滚动查看，而不是压缩。", "Allows scrolling the tab strip when tabs overflow.");
            Add("EnablePageCloseByMouseMiddle", "中键关闭页签", "true 时用鼠标中键点击页签即可关闭。", "Closes a tab on middle click.");
            Add("EnablePageCloseByMouseDoubleClick", "双击关闭页签", "true 时双击页签即可关闭。", "Closes a tab on double click.");
            Add("CloseDisposePage", "关闭释放页面", "true 时关闭页签会 Dispose 页面控件，释放资源。", "Disposes the page when its tab closes.");
            Add("DragOrder", "拖动排序", "true 时可以用鼠标拖动来调整顺序。", "Lets the user drag to reorder items.");
            Add("ShowTip", "显示提示", "true 时在悬停/选中时显示文字提示。", "Shows a text tip on hover/selection.");
            Add("ShowCheckBg", "显示勾选背景", "true 时已勾选项带浅色背景，便于扫读。", "Draws a light background behind checked rows.");
            Add("FilterShowCheckBg", "筛选勾选背景", "筛选下拉里已勾选项是否显示背景。", "Shows a background for checked filter entries.");
            Add("FilterRealTime", "筛选实时生效", "true 时输入筛选词立刻过滤，不用按回车。", "Filters while typing instead of on Enter.");
            Add("FilterSortOrder", "筛选排序顺序", "筛选下拉里“升序/降序”按钮的排列顺序。", "Order of the ascending/descending buttons in the filter.");
            Add("MouseClickPenetration", "点击穿透", "true 时点击穿透到下层控件，自身不抢事件。", "Lets clicks pass through to the control below.");
            Add("MultipleRows", "多行单元格", "true 时单元格内容可以换行显示多行。", "Allows cell content to wrap on several lines.");
            Add("HasImage", "是否有图片", "只读：当前是否设置了图片。", "Read-only: whether an image is set.");
            Add("ScrollBack", "滚动条底色", "滚动条滑道的颜色。", "Color of the scrollbar track.");
            Add("ScrollBackHover", "滚动条底色悬停", "鼠标悬停时滑道的颜色。", "Scrollbar track color on hover.");
            Add("ScrollFore", "滚动条滑块色", "滚动条滑块（thumb）的颜色。", "Color of the scrollbar thumb.");
            Add("ScrollForeHover", "滚动条滑块悬停", "鼠标悬停时滑块的颜色。", "Scrollbar thumb color on hover.");
            Add("Summary", "汇总", "汇总行的文字或数值，传给表格底部那行。", "Text/values shown in the summary row.");
            Add("SortOrderSize", "排序图标大小", "表头排序箭头的像素尺寸。", "Pixel size of the sort arrow in the header.");
            Add("StackedHeaderRows", "堆叠表头行数", "多层表头占用的行数。", "Number of stacked header rows.");
            Add("SelectedIndexs", "多选索引", "多选模式下所有被选中行的索引集合。", "Indexes of all selected rows in multi-select mode.");
            Add("TreeArrowStyle", "树形箭头样式", "树节点展开箭头的样式（三角/加号等）。", "Style of the tree expander arrow.");
            Add("TreeButtonSize", "树形按钮大小", "树节点按钮的像素尺寸。", "Pixel size of tree node buttons.");
            Add("GapTree", "树形缩进", "每一级树节点的缩进宽度。", "Indent width per tree level.");
            Add("PageSizeOptions", "每页条数选项", "分页“每页 N 条”下拉里可选的条数数组。", "Page-size choices shown in the pager dropdown.");
            Add("Alignment", "对齐", "页签栏/标题相对控件的位置（上、左等）。", "Position of the tab strip inside the control.");
            Add("PrefixColor", "前缀颜色", "前缀文字的颜色。", "Color of the prefix text.");
            Add("SuffixColor", "后缀颜色", "后缀文字的颜色。", "Color of the suffix text.");
            Add("LoopPauseOnMouseEnter", "悬停暂停轮播", "true 时鼠标移上去暂停自动播放。", "Pauses the carousel while hovered.");
            Add("LoopInfinite", "无限轮播", "true 时播到最后一张继续循环。", "Keeps looping from the last item to the first.");
            Add("PlayGIF", "播放 GIF", "true 时播放 GIF 动画，否则只显示首帧。", "Plays animated GIFs instead of the first frame.");
            Add("FillWarn", "警告填充色", "电量/进度进入警告区时的填充色。", "Fill color when the value enters the warning range.");
            Add("FillDanger", "危险填充色", "电量/进度进入危险区时的填充色。", "Fill color when the value enters the danger range.");
            Add("FillFully", "满格填充色", "数值满格（100%）时的填充色。", "Fill color when the value is full.");
            Add("ValueWarn", "警告阈值", "低于该值时切换为警告配色。", "Threshold below which the warning color is used.");
            Add("ValueDanger", "危险阈值", "低于该值时切换为危险配色。", "Threshold below which the danger color is used.");
            Add("ColorSplit", "分割线颜色", "分隔线使用的颜色，默认取主题边线色。", "Color of divider lines.");
            Add("Thickness", "粗细", "线条/边框的像素粗细。", "Pixel thickness of the line or border.");
            Add("ToggleType", "选中态样式", "选中时使用的主题色（与 Type 相同的一套取值）。", "Accent used in the toggled-on state.");
            Add("ToggleBack", "选中态背景", "选中/开启状态下的背景色。", "Background color in the toggled-on state.");
            Add("ToggleBackHover", "选中态背景悬停", "选中态下鼠标悬停时的背景色。", "Toggled-on background on hover.");
            Add("ToggleBackActive", "选中态背景按下", "选中态下按下时的背景色。", "Toggled-on background while pressed.");
            Add("ToggleBackExtend", "选中态背景扩展", "选中态背景向四周扩展/渐变的范围。", "Extent of the toggled-on background gradient.");
            Add("ToggleFore", "选中态文字色", "选中/开启状态下的文字与图标颜色。", "Text/icon color in the toggled-on state.");
            Add("ToggleForeHover", "选中态文字悬停", "选中态下悬停时的文字颜色。", "Toggled-on text color on hover.");
            Add("ToggleForeActive", "选中态文字按下", "选中态下按下时的文字颜色。", "Toggled-on text color while pressed.");
            Add("ToggleIcon", "选中态图标", "选中状态下显示的图标。", "Icon shown in the toggled-on state.");
            Add("ToggleIconHover", "选中态图标悬停", "选中态下悬停时显示的图标。", "Icon shown in the toggled-on state on hover.");
            Add("ToggleIconSvg", "选中态图标 SVG", "选中状态下显示的 SVG 图标。", "SVG icon shown in the toggled-on state.");
            Add("ToggleIconHoverSvg", "选中态图标悬停 SVG", "选中态下悬停时显示的 SVG 图标。", "SVG icon shown in the toggled-on state on hover.");
            Add("HasToggleIcon", "有切换图标", "只读：是否配置了选中态图标。", "Read-only: whether a toggled-state icon exists.");
            Add("IconToggleAnimation", "图标切换动画", "true 时选中态图标切换带过渡动画。", "Animates the icon when toggling.");
            Add("ClickSwitchDropdown", "点击切换下拉", "true 时点击控件直接展开/收起下拉。", "Opens/closes the dropdown on click.");
            Add("DecimalPlaces", "小数位数", "数值显示保留的小数位，null 表示自动。", "Digits shown after the decimal point; null = auto.");
            Add("Hexadecimal", "十六进制", "true 时数值按 16 进制显示/输入。", "Shows and edits the number in hexadecimal.");
            Add("ThousandsSeparator", "千分位", "true 时数值按 1,234 的形式分组显示。", "Groups digits with thousands separators.");
            Add("MaxDate", "最大日期", "允许选择的最晚日期。", "Latest selectable date.");
            Add("MinDate", "最小日期", "允许选择的最早日期。", "Earliest selectable date.");
            Add("MarkTextGap", "标记文字间距", "刻度标记与文字之间的距离（像素）。", "Gap between a mark and its label.");
            Add("ShowButtonNow", "现在按钮", "true 时在面板底部显示“回到现在”的按钮。", "Shows a “now” shortcut button.");
            Add("ShowButtonToDay", "今天按钮", "true 时在面板底部显示“回到今天”的按钮。", "Shows a “today” shortcut button.");
            Add("ShowReset", "重置按钮", "true 时显示清除/重置按钮，可恢复为默认值。", "Shows a reset button that restores the default value.");
            Add("Character", "评分字符", "评分显示的字符，例如 ★ 或心形。", "Character used for rating items, e.g. ★.");
            Add("AllowHalf", "允许半星", "true 时可以点出半颗星/半格。", "Allows half steps when rating.");
            Add("DisabledAlpha", "禁用透明度", "禁用状态下控件的透明度（0-1）。", "Opacity of the control when disabled.");
            Add("BarColor", "指示条颜色", "分段控制器选中指示条的颜色。", "Color of the segmented indicator bar.");
            Add("BarBg", "指示条背景", "指示条所在轨道的背景色。", "Background color behind the indicator bar.");
            Add("BarPosition", "指示条位置", "指示条位于内容的上下左右哪一侧。", "Which side the indicator bar sits on.");
            Add("BarPadding", "指示条内边距", "指示条与分段边缘之间留出的距离。", "Padding between the bar and the segments.");
            Add("BarSize", "指示条粗细", "指示条的粗细（像素）。", "Thickness of the indicator bar in pixels.");
            Add("BarRadius", "指示条圆角", "指示条的圆角半径。", "Corner radius of the indicator bar.");
            Add("MaxChoiceCount", "最多可选", "多选下拉里一次最多能选中多少项。", "Maximum number of selections in multi-select.");
            Add("EditMode", "编辑模式", "表格进入编辑的方式：单击、双击或不可编辑。", "How a table cell enters edit mode.");
            Add("EditLostFocus", "失焦结束编辑", "true 时输入框失去焦点就提交编辑。", "Commits the edit when the editor loses focus.");
            Add("EditSelection", "编辑时全选", "true 时进入编辑会选中原有内容，方便整格覆盖。", "Selects existing content when editing starts.");
            Add("EditInputStyle", "编辑框样式", "编辑时使用的输入框样式参数。", "Style parameters of the in-cell editor.");
            Add("BorderHigh", "高亮边框", "true 时用品牌色加粗边框，突出当前控件。", "Highlights the border with the accent color.");
            Add("BorderCellWidth", "单元格边框宽", "单元格描边的像素宽度。", "Stroke width of table cell borders.");
            Add("CellFocusedBg", "单元格焦点背景", "当前焦点单元格的背景色。", "Background of the focused cell.");
            Add("CellFocusedBorder", "单元格焦点边框", "当前焦点单元格的描边色。", "Border color of the focused cell.");
            Add("CellFocusedStyle", "单元格焦点样式", "当前焦点单元格的描边样式（实线/虚线）。", "Border style of the focused cell.");
            Add("ClipboardCopy", "允许复制", "true 时选中内容可以复制到剪贴板。", "Allows copying the selection to the clipboard.");
            Add("ClipboardCopyFocusedCell", "复制焦点单元格", "true 时 Ctrl+C 只复制当前焦点单元格。", "Ctrl+C copies only the focused cell.");
            Add("ColumnFont", "列字体", "表头文字使用的字体。", "Font used by the header cells.");
            Add("DisplayRowCount", "显示行数", "控件高度内最多显示多少行，用于虚拟滚动计算。", "Rows visible at once; used for virtualizing.");
            Add("DefaultExpand", "默认展开", "true 时所有节点初始为展开状态。", "Expands all nodes by default.");
            Add("DefaultBack", "默认背景", "没有其它样式时使用的兜底背景色。", "Fallback background color.");
            Add("DefaultBorderColor", "默认边框色", "没有其它样式时使用的兜底描边色。", "Fallback border color.");
            Add("RowHoverBg", "行悬停背景", "鼠标悬停在表格行上时的背景色。", "Background of a table row on hover.");
            Add("RowSelectedBg", "选中行背景", "表格行被选中时的背景色。", "Background of a selected table row.");
            Add("SelectIndex", "选中下标", "分段控制器当前选中项的下标，从 0 开始。", "Zero-based index of the selected segment.");
            Add("Segments", "分段数组", "分段控制器每一段的文字/数值数组。", "Texts or values of each segment.");
            Add("TextUnit", "数值单位", "数值后面紧跟的单位文字，如 元、%。", "Unit text appended to the value, e.g. % or ¥.");
            Add("AutoHeight", "自动高度", "true 时根据内容自动计算控件高度。", "Computes the control height from its content.");
            Add("AutoSize", "自动尺寸", "true 时根据内容自动计算控件宽高。", "Computes the control size from its content.");
            Add("AutoToggle", "自动切换", "true 时点击按钮自动在选中/未选中之间切换。", "Toggles the button state on click.");
            Add("CanDelete", "允许删除", "true 时显示删除入口，允许删掉这个子项。", "Shows a delete affordance for the item.");
            Add("Icon", "图标", "控件显示的图标（位图）。", "Bitmap icon shown by the control.");
            Add("DragHandleSize", "拖拽把手尺寸", "拖动把手（⋮⋮）的像素尺寸。", "Pixel size of the drag handle.");
            Add("DragHandleIconSize", "拖拽把手图标尺寸", "拖动把手图标的像素尺寸。", "Pixel size of the drag-handle icon.");
            Add("MilestoneMode", "里程碑模式", "里程碑节点的显示方式（圆点/图标/文字）。", "How milestone nodes are rendered.");
            Add("MilestoneType", "里程碑类型", "里程碑使用的图形类型（旗、圆、勾等）。", "Shape used for milestone markers.");
            Add("MilestoneAnimation", "里程碑动画", "true 时里程碑状态变化带过渡动画。", "Animates milestone state changes.");
            Add("MilestoneCurrentCompleted", "当前即完成", "true 时把“当前”节点也画成已完成样式。", "Renders the current milestone as completed.");
            Add("MilestoneTimeFormat", "里程碑时间格式", "里程碑时间文字的格式字符串，如 MM-dd。", "Date format of milestone labels.");
            Add("MilestoneTimelineThickness", "里程碑轴线粗细", "里程碑时间轴连线的粗细（像素）。", "Thickness of the milestone timeline line.");
            Add("PrefixColour", "前缀颜色", "前缀文字的颜色（Colour 与 Color 同义）。", "Color of the prefix text (same as PrefixColor).");
            Add("SuffixColour", "后缀颜色", "后缀文字的颜色（Colour 与 Color 同义）。", "Color of the suffix text (same as SuffixColor).");
            #endregion

            #region 通用事件 General events
            Ev("Click", "点击", "鼠标左键点击控件时触发（Button、Tag、Label 等）。", "Fires when the control is left-clicked.");
            Ev("DoubleClick", "双击", "鼠标左键快速双击时触发。", "Fires on a left-button double click.");
            Ev("MouseEnter", "鼠标移入", "鼠标指针进入控件范围时触发一次。", "Fires once when the pointer enters the control.");
            Ev("MouseLeave", "鼠标移出", "鼠标指针离开控件范围时触发一次。", "Fires once when the pointer leaves the control.");
            Ev("MouseDown", "按下", "鼠标按键在控件上按下时触发。", "Fires when a mouse button goes down on the control.");
            Ev("MouseUp", "抬起", "鼠标按键在控件上松开时触发。", "Fires when a mouse button is released.");
            Ev("MouseMove", "移动", "鼠标在控件上移动时持续触发（频率高，慎用）。", "Fires continuously while moving over the control.");
            Ev("KeyDown", "按键按下", "控件有焦点时按下键盘按键触发，可拿到按键码。", "Fires on a key press while the control has focus.");
            Ev("KeyUp", "按键抬起", "按键松开时触发。", "Fires when a key is released.");
            Ev("TextChanged", "文本变化", "文字内容每次改变都会触发。", "Fires every time the text changes.");
            Ev("ValueChanged", "值变化", "数值/选项改变时触发，事件参数里带新旧值。", "Fires when the value changes; args carry new value.");
            Ev("SelectedIndexChanged", "选中项变化", "下拉/页签/列表的选中项改变时触发。", "Fires when the selected index changes.");
            Ev("CheckedChanged", "勾选变化", "复选框/开关的选中状态改变时触发。", "Fires when the checked state changes.");
            Ev("SizeChanged", "尺寸变化", "控件宽高改变后触发（窗口缩放时也会）。", "Fires after the control is resized.");
            Ev("Resize", "调整大小", "控件尺寸变化过程中触发。", "Fires while the control is being resized.");
            Ev("EnabledChanged", "启用状态变化", "Enabled 属性改变时触发。", "Fires when Enabled changes.");
            Ev("VisibleChanged", "可见性变化", "Visible 属性改变时触发。", "Fires when Visible changes.");
            Ev("MouseDoubleClick", "鼠标双击", "鼠标左键快速双击时触发，参数里带点击位置。", "Fires on a left-button double click; args carry the position.");
            Ev("MouseClick", "鼠标单击", "鼠标单击时触发（和 Click 的区别：Click 也可能由键盘或代码触发，MouseClick 只认鼠标）。", "Fires on a mouse click; unlike Click it is not raised by keyboard or code.");
            Ev("GotFocus", "获得焦点", "控件拿到键盘焦点时触发，可用于开始校验。", "Fires when the control receives keyboard focus.");
            Ev("LostFocus", "失去焦点", "控件失去键盘焦点时触发，常用于校验/保存。", "Fires when the control loses keyboard focus.");
            Ev("PrefixClick", "前缀点击", "点击输入框左侧前缀区域时触发。", "Fires when the left prefix area of an input is clicked.");
            Ev("SuffixClick", "后缀点击", "点击输入框右侧后缀区域时触发。", "Fires when the right suffix area of an input is clicked.");
            Ev("VerifyChar", "字符校验", "键入字符时触发，可返回 true/false 决定是否接受该字符。", "Fires per typed character; return true/false to accept or reject it.");
            Ev("VerifyKeyboard", "按键校验", "按下键盘时触发，用于拦截回车/方向键等特定按键。", "Fires on key press; use it to intercept Enter / arrows.");
            Ev("FilterPopupBegin", "筛选弹层出现", "表格列筛选下拉即将弹出时触发，可自定义筛选内容。", "Fires before a column filter popup opens.");
            #endregion
        }

        private static void Add(string name, string zh, string effectZh, string effectEn, string pit = "")
        {
            Props[name] = new Doc(name, zh, effectZh, effectEn, pit);
        }

        private static void Ev(string name, string zh, string effectZh, string effectEn, string pit = "")
        {
            Events[name] = new Doc(name, zh, effectZh, effectEn, pit);
        }

        /// <summary>登记顺序：决定检查器里“精讲成员”的排列先后。</summary>
        public static readonly Dictionary<string, List<string>> Order = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        private static void Track(string typeName, string member)
        {
            List<string> list;
            if (!Order.TryGetValue(typeName, out list))
            {
                list = new List<string>();
                Order[typeName] = list;
            }
            if (!list.Contains(member)) list.Add(member);
        }

        /// <summary>给某个类型的成员写覆盖说明。</summary>
        public static void Over(string typeName, string member, string zh, string effectZh, string effectEn, string pit = "", string en = null)
        {
            Track(typeName, member);
            Overrides[typeName + "." + member] = new Doc(en ?? member, zh, effectZh, effectEn, pit);
        }

        /// <summary>精讲排名：0 起，越小越靠前；没有人工说明的返回 MaxValue。</summary>
        public static int Rank(Type t, string member)
        {
            for (Type k = t; k != null; k = k.BaseType)
            {
                List<string> list;
                if (Order.TryGetValue(k.Name, out list))
                {
                    int i = list.IndexOf(member);
                    if (i >= 0) return i;
                }
            }
            return int.MaxValue;
        }

        public static Doc ForProp(Type t, string name)
        {
            Doc d;
            if (t != null && Overrides.TryGetValue(t.Name + "." + name, out d)) return d.Clone();
            if (Props.TryGetValue(name, out d)) return d.Clone();
            d = new Doc(name, name, "", "");
            return d;
        }

        public static Doc ForEvent(Type t, string name)
        {
            Doc d;
            if (t != null && Overrides.TryGetValue(t.Name + "!" + name, out d)) return d.Clone();
            if (Events.TryGetValue(name, out d)) return d.Clone();
            return new Doc(name, name, "", "");
        }

        /// <summary>事件覆盖：带 ! 前缀存，避免和属性同名冲突。</summary>
        public static void OverEvent(string typeName, string member, string zh, string whenZh, string whenEn, string pit = "")
        {
            Track(typeName + "!", member);
            Overrides[typeName + "!" + member] = new Doc(member, zh, whenZh, whenEn, pit);
        }

        /// <summary>事件精讲排名。</summary>
        public static int EventRank(Type t, string member)
        {
            for (Type k = t; k != null; k = k.BaseType)
            {
                List<string> list;
                if (Order.TryGetValue(k.Name + "!", out list))
                {
                    int i = list.IndexOf(member);
                    if (i >= 0) return i;
                }
            }
            return int.MaxValue;
        }

        /// <summary>给属性补上 AntdUI.xml 的中文说明（若还没有）：沿基类链与接口链找，再把中文摘要当短名。</summary>
        public static void FillFromXml(Type t, string name, Doc d, bool isEvent)
        {
            if (d == null) return;
            if (string.IsNullOrEmpty(d.EffectZh))
            {
                string xml = FindXml(t, name, isEvent);
                if (!string.IsNullOrEmpty(xml)) d.EffectZh = xml;
            }
            if (string.IsNullOrEmpty(d.Zh) || d.Zh == d.En)
            {
                string tr = Translate(d.En);
                if (!string.IsNullOrEmpty(tr) && !string.Equals(tr, d.En, StringComparison.Ordinal)) d.Zh = tr;
                else if (!string.IsNullOrEmpty(d.EffectZh)) d.Zh = Shorten(d.EffectZh);
            }
            // 中文名就是从这句摘要截出来的话，不再重复显示同一句话（事件稍后用通用提示补上）。
            if (!string.IsNullOrEmpty(d.EffectZh) &&
                string.Equals(d.EffectZh.Trim(), d.Zh.Trim(), StringComparison.OrdinalIgnoreCase))
                d.EffectZh = "";

            if (string.IsNullOrEmpty(d.EffectZh) && isEvent)
                d.EffectZh = "官方未提供注释；勾选后去操作控件，底部日志会显示它何时触发、带了什么参数。";
        }

        private static string FindXml(Type t, string name, bool isEvent)
        {
            for (Type k = t; k != null; k = k.BaseType)
            {
                string v = isEvent ? XmlDocs.Event(k.Name, name) : XmlDocs.Prop(k.Name, name);
                if (!string.IsNullOrEmpty(v)) return v;
            }
            if (t != null)
            {
                foreach (Type it in t.GetInterfaces())
                {
                    if (it.Name.Length < 2 || it.Name[0] != 'I') continue;
                    string v = isEvent ? XmlDocs.Event(it.Name, name) : XmlDocs.Prop(it.Name, name);
                    if (!string.IsNullOrEmpty(v)) return v;
                }
            }
            return null;
        }

        /// <summary>把一段中文说明压成短名（最多 14 字）。</summary>
        private static string Shorten(string s)
        {
            s = s.Trim();
            int cut = s.IndexOfAny(new[] { '：', '，', '。', '；', '、', ' ', '(', '（' });
            if (cut > 1) s = s.Substring(0, cut);
            if (s.Length > 14) s = s.Substring(0, 14) + "…";
            return s;
        }

        /// <summary>没有人工说明时，用单词表把英文名直译成中文名；只要有词不认识就返回原名，避免中英混排。</summary>
        public static string Translate(string en)
        {
            if (string.IsNullOrEmpty(en)) return "";
            List<string> words = SplitWords(en);
            var sb = new System.Text.StringBuilder();
            foreach (string w in words)
            {
                string zh;
                if (!WordZh.TryGetValue(w, out zh)) return en;
                sb.Append(zh);
            }
            return sb.Length == 0 ? en : sb.ToString();
        }

        private static List<string> SplitWords(string en)
        {
            var list = new List<string>();
            int start = 0;
            for (int i = 1; i < en.Length; i++)
            {
                bool up = char.IsUpper(en[i]);
                bool prevLowerOrDigit = char.IsLower(en[i - 1]) || char.IsDigit(en[i - 1]);
                bool nextLower = i + 1 < en.Length && char.IsLower(en[i + 1]);
                if (up && (prevLowerOrDigit || (char.IsUpper(en[i - 1]) && nextLower)))
                {
                    list.Add(en.Substring(start, i - start));
                    start = i;
                }
            }
            list.Add(en.Substring(start));
            return list;
        }

        private static readonly Dictionary<string, string> WordZh = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Back", "背景" }, { "Fore", "前景" }, { "Fill", "填充" }, { "Hover", "悬停" }, { "Active", "激活" },
            { "Color", "颜色" }, { "Width", "宽度" }, { "Height", "高度" }, { "Size", "尺寸" }, { "Radius", "圆角" },
            { "Align", "对齐" }, { "Position", "位置" }, { "Text", "文字" }, { "Title", "标题" }, { "Icon", "图标" },
            { "Image", "图片" }, { "Svg", "矢量图" }, { "Shadow", "阴影" }, { "Round", "圆形" }, { "Square", "方形" },
            { "Gap", "间距" }, { "Dot", "圆点" }, { "Ratio", "比例" }, { "Show", "显示" }, { "Auto", "自动" },
            { "Check", "勾选" }, { "Checked", "已选" }, { "State", "状态" }, { "Status", "状态" }, { "Mode", "模式" },
            { "Type", "类型" }, { "Shape", "形状" }, { "Value", "值" }, { "Min", "最小" }, { "Max", "最大" },
            { "Items", "列表项" }, { "Item", "项" }, { "Count", "数量" }, { "Data", "数据" }, { "Source", "数据源" },
            { "Columns", "列" }, { "Column", "列" }, { "Bordered", "边框" }, { "Fixed", "固定" }, { "Header", "表头" },
            { "Empty", "空" }, { "Rows", "行" }, { "Row", "行" }, { "Multiple", "多选" }, { "Edit", "编辑" },
            { "Sort", "排序" }, { "Expand", "展开" }, { "Loading", "加载" }, { "Wave", "波纹" }, { "Ghost", "幽灵" },
            { "Page", "页" }, { "Pages", "页面" }, { "Selected", "选中" }, { "Index", "索引" }, { "Current", "当前" },
            { "Total", "总数" }, { "Presets", "预设" }, { "Format", "格式" }, { "Picker", "选择器" }, { "Placement", "弹出方向" },
            { "Centered", "居中" }, { "Vertical", "垂直" }, { "Opacity", "透明度" }, { "Duration", "时长" },
            { "Animation", "动画" }, { "Time", "时间" }, { "Delay", "延迟" }, { "Speed", "速度" }, { "Loop", "循环" },
            { "Overflow", "溢出" }, { "Pause", "暂停" }, { "Enter", "进入" }, { "Mouse", "鼠标" }, { "Drag", "拖动" },
            { "Drop", "放下" }, { "Close", "关闭" }, { "Open", "打开" }, { "Extend", "扩展" }, { "Extra", "额外" },
            { "Prog", "进度" }, { "Ellipsis", "省略" }, { "Badge", "徽标" }, { "Enabled", "启用" }, { "Visible", "可见" },
            { "ReadOnly", "只读" }, { "Multiline", "多行" }, { "Password", "密码" }, { "Clear", "清空" },
            { "Click", "点击" }, { "Down", "按下" }, { "Up", "抬起" }, { "Leave", "离开" }, { "Move", "移动" },
            { "Key", "按键" }, { "Changed", "变化" }, { "Resize", "尺寸变化" }, { "Disposed", "释放" },
            { "GotFocus", "获得焦点" }, { "LostFocus", "失去焦点" }
        };
    }
}
