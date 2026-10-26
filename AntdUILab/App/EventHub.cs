using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace AntdUILab.App
{
    public class EventEntry
    {
        public DateTime At;
        public string Category;
        public string Title;
        public string Detail;

        public string TimeText { get { return At.ToString("HH:mm:ss.fff"); } }
    }

    /// <summary>事件订阅中心：勾选事件 → 反射挂接 → 实时写入日志。</summary>
    public class EventHub
    {
        public event Action<EventEntry> Added;

        public readonly List<EventEntry> Entries = new List<EventEntry>();
        public readonly List<string> Subscribed = new List<string>();

        /// <summary>最近一次触发的事件（给检查器、舞台做即时反馈）。</summary>
        public EventInfo LastInfo;
        public object[] LastArgs;
        public string LastText = "";

        private object _ctl;
        private readonly Dictionary<string, Delegate> _handlers = new Dictionary<string, Delegate>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, EventInfo> _infos = new Dictionary<string, EventInfo>(StringComparer.OrdinalIgnoreCase);

        public object Target { get { return _ctl; } }

        public void Bind(object ctl)
        {
            DetachAll();
            _ctl = ctl;
            Subscribed.Clear();
        }

        public IEnumerable<EventInfo> Events()
        {
            if (_ctl == null) return Enumerable.Empty<EventInfo>();
            return R.AllEvents(_ctl.GetType()).OrderBy(e => e.Name);
        }

        public bool IsOn(string name)
        {
            return Subscribed.Contains(name, StringComparer.OrdinalIgnoreCase);
        }

        public string Toggle(string name, bool on)
        {
            if (_ctl == null) return "没有绑定控件";
            if (on == IsOn(name)) return null;
            EventInfo ev = _ctl.GetType().GetEvent(name, BindingFlags.Public | BindingFlags.Instance);
            if (ev == null) return "没有事件 " + name;

            if (!on)
            {
                Delegate old;
                if (_handlers.TryGetValue(name, out old))
                {
                    try { ev.RemoveEventHandler(_ctl, old); } catch { }
                    _handlers.Remove(name);
                }
                Subscribed.RemoveAll(x => string.Equals(x, name, StringComparison.OrdinalIgnoreCase));
                return null;
            }

            Delegate d = R.Handler(ev, OnFired);
            if (d == null) return "无法为该事件创建委托：" + name;
            try
            {
                ev.AddEventHandler(_ctl, d);
            }
            catch (Exception ex)
            {
                return "订阅失败：" + (ex.InnerException ?? ex).Message;
            }
            _handlers[name] = d;
            _infos[name] = ev;
            Subscribed.Add(name);
            return null;
        }

        public void DetachAll()
        {
            foreach (KeyValuePair<string, Delegate> kv in _handlers)
            {
                EventInfo ev;
                if (_infos.TryGetValue(kv.Key, out ev) && _ctl != null)
                {
                    try { ev.RemoveEventHandler(_ctl, kv.Value); } catch { }
                }
            }
            _handlers.Clear();
            _infos.Clear();
            Subscribed.Clear();
        }

        public void Clear()
        {
            Entries.Clear();
        }

        private void OnFired(string name, object sender, object[] args)
        {
            EventInfo ev;
            _infos.TryGetValue(name, out ev);
            string detail = EventArgsText.Pretty(ev, args);
            if (detail.Length == 0) detail = "（无参数）";
            LastInfo = ev;
            LastArgs = args;
            LastText = detail;
            Add("事件", name, detail);
        }

        public void Add(string category, string title, string detail)
        {
            var e = new EventEntry { At = DateTime.Now, Category = category, Title = title, Detail = detail };
            Entries.Add(e);
            while (Entries.Count > 500) Entries.RemoveAt(0);
            if (Added != null) Added(e);
        }

        public void Log(string title, string detail)
        {
            Add("操作", title, detail);
        }
    }
}
