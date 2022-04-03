using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace HotKeyLibrary
{
    public static class HotKeyManager
    {
        // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerhotkey
        [DllImport("User32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey([In] IntPtr hWnd, [In] int id, [In] uint fsModifiers, [In] uint vk);

        [DllImport("User32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        private static readonly List<HotKeyConfig> HotKeyConfigs = new();

        private static IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;

            switch (msg)
            {
                case WM_HOTKEY:
                    foreach (var config in HotKeyManager.HotKeyConfigs)
                    {
                        if (config.HotKeyId == wParam.ToInt32())
                        {
                            config.OnHotKeyPressed();
                            handled = true;
                            break;
                        }
                    }

                    break;
            }

            return IntPtr.Zero;
        }

        public static bool AddHotKey(HotKeyConfig config)
        {
            const uint VK_F10 = 0x20; // spacebar
            const uint MOD_CTRL = 0x0002;

            config.Source?.AddHook(HotKeyManager.HwndHook);
            HotKeyManager.HotKeyConfigs.Add(config);
            // TODO: Keys not being converted to correct uint format (like above)
            // uint casting is temp
            return RegisterHotKey(config.Helper.Handle, config.HotKeyId, MOD_CTRL, VK_F10);
        }

        public static bool RemoveHotKeyById(int id)
        {
            var config = HotKeyManager.HotKeyConfigs.Find((config) => config.HotKeyId == id);
            
            if (config == null)
            {
                Console.WriteLine("Could not find matching hot key config to remove");
                return false;
            }

            config.Source?.RemoveHook(HotKeyManager.HwndHook);
            config.Source = null;
            return UnregisterHotKey(config.Helper.Handle, config.HotKeyId);
        }
        
        public static List<(int id, bool isSuccess)> RemoveAllHotKeys()
        {
            var results = new List<(int id, bool isSuccess)>();
            foreach (var config in HotKeyManager.HotKeyConfigs)      
            {
                bool result = HotKeyManager.RemoveHotKeyById(config.HotKeyId);
                results.Add((config.HotKeyId, result));
            }

            return results;
        }
    }
}
