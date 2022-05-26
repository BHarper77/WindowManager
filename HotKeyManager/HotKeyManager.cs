using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace HotKeyLibrary
{
    public static class HotKeyManager
    {
        /// <summary>
        /// Adds a hot key
        /// </summary>
        /// <see cref="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerhotkey"/>
        /// <param name="hWnd">Handle of the window to register the hot key to</param>
        /// <param name="id">ID of the hot key</param>
        /// <param name="fsModifiers">Hot key modifier key</param>
        /// <param name="vk">Hot key primary key</param>
        /// <returns></returns>
        [DllImport("User32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey([In] IntPtr hWnd, [In] int id, [In] uint fsModifiers, [In] uint vk);

        /// <summary>
        /// Removes a hot key
        /// </summary>
        /// <see cref="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-unregisterhotkey"/>
        /// <param name="hWnd">Window handle where the hot key was registered</param>
        /// <param name="id">Hot key ID</param>
        /// <returns></returns>
        [DllImport("User32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        private static readonly List<HotKeyConfig> HotKeyConfigs = new();

        /// <summary>
        /// This method acts as the global hotkey handler and executes the relevant hotkey execution method.
        /// Called by the operating system whenever the hotkey is pressed
        /// </summary>
        /// <param name="hwnd">The window handle</param>
        /// <param name="msg">The ID of the message received from Windows</param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <param name="handled">Boolean value repesenting if message has been handled</param>
        /// <returns></returns>
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
                            // TODO: add error handling here
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
            config.Source?.AddHook(HotKeyManager.HwndHook);
            HotKeyManager.HotKeyConfigs.Add(config);

            // TODO: Keys not being converted to correct uint format (like above)
            // uint casting is temp
            //return RegisterHotKey(config.Helper.Handle, config.HotKeyId, MOD_CTRL, VK_F10);
            return RegisterHotKey(config.Helper.Handle, config.HotKeyId, config.SecondaryKey, config.PrimaryKey);
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
