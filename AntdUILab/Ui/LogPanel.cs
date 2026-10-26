using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AntdUILab.App;

namespace AntdUILab.Ui
{
    /// <summary>底部事件日志：勾选事件后，每次触发实时追加一行。</summary>
    public class LogPanel : Panel
    {
        public EventHub Hub;

        private readonly RichTextBox _box;
        private readonly Label _count;
        private readonly FlatButton _pause;
        private readonly FlatButton _clear;
        private readonly FlatButton _copy;
        private bool _paused;

        public LogPanel()
        {
            BackColor = Color.White;
            Height = 208;

            var head = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.White };
            var title = new Label
            {
                Text = "事件日志  Event log",
                Font = Theme.SmallBold,
                ForeColor = Theme.Text,
                Location = new Point(14, 12),
                AutoSize = true
            };
            var hint = new Label
            {
                Text = "在右侧「事件」页勾选后，操作控件即可看到触发顺序与参数",
                Font = Theme.Tiny,
                ForeColor = Theme.Faint,
                Location = new Point(150, 13),
                AutoSize = true
            };
            _count = new Label
            {
                Text = "0 条",
                Font = Theme.Tiny,
                ForeColor = Theme.Faint,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleRight,
                Location = new Point(0, 12),
                Size = new Size(60, 18)
            };
            _clear = new FlatButton { Text = "清空", Size = new Size(56, 26), Outline = true, Font = Theme.Small, Location = new Point(0, 7) };
            _pause = new FlatButton { Text = "暂停", Size = new Size(56, 26), Outline = true, Font = Theme.Small, Location = new Point(0, 7) };
            _copy = new FlatButton { Text = "复制", Size = new Size(56, 26), Outline = true, Font = Theme.Small, Location = new Point(0, 7) };

            _clear.Click += (s, e) => { Clear(); };
            _copy.Click += (s, e) =>
            {
                try { if (_box.TextLength > 0) Clipboard.SetText(_box.Text); } catch { }
            };
            _pause.Click += (s, e) =>
            {
                _paused = !_paused;
                _pause.Text = _paused ? "继续" : "暂停";
                _pause.FillColor = _paused ? Theme.Orange : Color.White;
                _pause.TextColor = _paused ? Color.White : Theme.Sub;
                _pause.Invalidate();
            };

            head.Controls.Add(_copy);
            head.Controls.Add(_clear);
            head.Controls.Add(_pause);
            head.Controls.Add(_count);
            head.Controls.Add(hint);
            head.Controls.Add(title);
            head.SizeChanged += (s, e) =>
            {
                int right = head.Width - 8;
                _copy.Location = new Point(right - 56, 7); right -= 62;
                _clear.Location = new Point(right - 56, 7); right -= 62;
                _pause.Location = new Point(right - 56, 7); right -= 62;
                _count.Location = new Point(right - 64, 12);
                hint.Visible = head.Width > 560;
            };
            var line = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Theme.LineSoft };

            _box = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.DarkBg,
                ForeColor = Theme.DarkText,
                BorderStyle = BorderStyle.None,
                Font = Theme.MonoSmall,
                ReadOnly = true,
                WordWrap = false,
                DetectUrls = false,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };
            Controls.Add(_box);
            Controls.Add(line);
            Controls.Add(head);

            AppendCulture();
        }

        private void AppendCulture()
        {
            AppendLine("系统", "就绪", "中英双语说明来自 AntdUI.xml 官方注释 + 内置人工标注；属性修改即时生效。", Theme.Purple);
        }

        public void Clear()
        {
            _box.Clear();
            _count.Text = "0 条";
            AppendCulture();
        }

        private int _lines;

        public void Append(EventEntry e)
        {
            if (_paused) return;
            Color c = ColorFor(e.Category);
            string head = e.At.ToString("HH:mm:ss.fff") + "  ";
            AppendLine(e.Category, e.Title + (string.IsNullOrEmpty(e.Detail) ? "" : "   " + e.Detail), null, c, head);
            _lines++;
            _count.Text = _lines + " 条";
        }

        private static Color ColorFor(string category)
        {
            switch (category)
            {
                case "事件": return Theme.Blue;
                case "属性": return Theme.Green;
                case "操作": return Theme.Orange;
                case "系统": return Theme.Purple;
                default: return Theme.Blue;
            }
        }

        private void AppendLine(string category, string text, string dim, Color c, string time = null)
        {
            if (_box.Lines.Length > 400)
            {
                int cut = _box.GetFirstCharIndexFromLine(100);
                if (cut > 0)
                {
                    _box.Select(0, cut);
                    _box.SelectedText = "";
                }
            }

            string prefix = time ?? "";
            int start = _box.TextLength;
            _box.AppendText(prefix);
            if (category.Length > 0)
            {
                _box.AppendText("[" + category + "] ");
            }
            _box.AppendText(text);
            if (!string.IsNullOrEmpty(dim)) _box.AppendText("  " + dim);
            _box.AppendText(Environment.NewLine);

            int len = _box.TextLength - start;
            if (len <= 0) return;
            _box.Select(start, len);
            _box.SelectionColor = Theme.DarkSub;
            int catStart = start + prefix.Length;
            if (category.Length > 0)
            {
                _box.Select(catStart, category.Length + 2);
                _box.SelectionColor = c;
                _box.SelectionFont = new Font(Theme.MonoSmall, FontStyle.Bold);
            }
            _box.Select(_box.TextLength, 0);
            _box.ScrollToCaret();
        }
    }
}
