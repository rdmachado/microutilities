using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Input;

namespace NetRunner2
{
    public sealed class HotkeyManager : NativeWindow
    {
        class RegisteredHotkey
        {
            public int Win32_ID;
            public Keys Key;
            public ModifierKeys Modifiers;
        }

        private static readonly HotkeyManager instance = new HotkeyManager();

        static HotkeyManager() { }

        private HotkeyManager()
        {
            this.CreateHandle(new CreateParams());
        }

        public static HotkeyManager Instance { get { return instance; } }

        #region System calls and definitions

        private const int WM_HOTKEY = 0x0312;
        private int system_id_current_max = 1000;
        private static readonly Dictionary<string, int> registeredHotkeyDict = new Dictionary<string, int>();

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, ModifierKeys fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        #endregion

        public event EventHandler<HotKeyEventArgs> HotKeyPressed;


        public bool RegisterHotKey(string id, KeyboardHotkey hotkey)
        {
            if (registeredHotkeyDict.ContainsKey(id))
                return false;

            int tmp_id = ++system_id_current_max;
                        
            bool ret = RegisterHotKey(this.Handle, tmp_id, hotkey.Mod1 | hotkey.Mod2, (uint)KeyInterop.VirtualKeyFromKey(hotkey.AN));

            if (ret)
            {
                registeredHotkeyDict.Add(id, tmp_id);
                return true;
            }
            else
                return false;
        }

        public bool UnregisterHotKey(string id)
        {
            if (registeredHotkeyDict.ContainsKey(id))
            {
                int tmp_id;
                if (registeredHotkeyDict.TryGetValue(id, out tmp_id) && UnregisterHotKey(IntPtr.Zero, tmp_id))
                    return true;
            }

            return false;
        }

        public void UnregisterAllHotKeys()
        {
            foreach(var id in registeredHotkeyDict.Values)
            {
                UnregisterHotKey(this.Handle, id);
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_HOTKEY)
            {
                int val = (int)m.WParam;
                if (registeredHotkeyDict.ContainsValue(val))
                {
                    string key = registeredHotkeyDict.Where(e => e.Value == val).Select(e => e.Key).First();

                    if (HotKeyPressed != null)
                        HotKeyPressed(this, new HotKeyEventArgs(key));
                }
            }
        }
    }

    public class HotKeyEventArgs
    {
        private string _id;
        public string id { get { return _id; } }

        public HotKeyEventArgs(string id)
        {
            _id = id;
        }
    }
}

