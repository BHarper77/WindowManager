using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using HotKeyLibrary;

namespace HotKeysDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        [Flags()]
        public enum DisplayDeviceStateFlags : int
        {
            /// <summary>The device is part of the desktop.</summary>
            AttachedToDesktop = 0x1,
            MultiDriver = 0x2,
            /// <summary>The device is part of the desktop.</summary>
            PrimaryDevice = 0x4,
            /// <summary>Represents a pseudo device used to mirror application drawing for remoting or other purposes.</summary>
            MirroringDriver = 0x8,
            /// <summary>The device is VGA compatible.</summary>
            VGACompatible = 0x10,
            /// <summary>The device is removable; it cannot be the primary display.</summary>
            Removable = 0x20,
            /// <summary>The device has more display modes than its output devices support.</summary>
            ModesPruned = 0x8000000,
            Remote = 0x4000000,
            Disconnect = 0x2000000
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DISPLAY_DEVICE
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            [MarshalAs(UnmanagedType.U4)]
            public DisplayDeviceStateFlags StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        [DllImport("user32.dll")]
        private static extern int GetLastWin32Error();

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        /// <see cref="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-enumdisplaydevicesa"/>
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        // Delegate to filter which windows to include 
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        protected override void OnSourceInitialized(EventArgs e)
        {
            var config = new HotKeyConfig(System.Windows.Input.Key.Space, ModKeys.Ctrl, new WindowInteropHelper(this), async delegate
            {
                // get list of all connected monitors
                //List<string> monitors = new();

                //foreach (var screen in Screen.AllScreens)
                //{
                //    monitors.Add(screen.ToString());
                //}

                //var windows = GetAllWindows();

                ////await File.WriteAllTextAsync("C:/Users/brady/Documents/targetWindow.txt", targetWindow.ToString());
                //await File.WriteAllLinesAsync("C:/Users/brady/Documents/monitorsV2.txt", monitors);
                //await File.WriteAllLinesAsync("C:/Users/brady/Documents/windowsV2.txt", windows);

                var device = new DISPLAY_DEVICE();
                device.cb = Marshal.SizeOf(device);

                // ref = passed by reference (pointer)
                for (uint id = 0; EnumDisplayDevices(null, id, ref device, 0); id++)
                {
                    if (device.StateFlags.HasFlag(DisplayDeviceStateFlags.AttachedToDesktop))
                    {
                        Console.WriteLine(String.Format("{0}, {1}, {2}, {3}, {4}, {5}",
                            id,
                            device.DeviceName,
                            device.DeviceString,
                            device.StateFlags,
                            device.DeviceID,
                            device.DeviceKey
                        ));

                        device.cb = Marshal.SizeOf(device);
                        EnumDisplayDevices(device.DeviceName, 0, ref device, 0);

                        Console.WriteLine(String.Format("{0}, {1}",
                            device.DeviceName,
                            device.DeviceString
                        ));
                    }

                    device.cb = Marshal.SizeOf(device);
                }

                return true;
            });
            
            bool isSuccess = HotKeyManager.AddHotKey(config);

            if (!isSuccess)
            {
                int error = Marshal.GetLastWin32Error();
                System.Windows.MessageBox.Show($"Error adding hotkey: {error}");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            var resultsList = HotKeyManager.RemoveAllHotKeys();

            foreach (var (id, isSuccess) in resultsList)
            {
                if (isSuccess == false)
                {
                    int error = Marshal.GetLastWin32Error();
                    System.Windows.MessageBox.Show($"Error removing hotkey {id}: {error}");
                }
            }
        }

        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOZORDER = 0x0004;

        public static IntPtr GetWindowByName(string name)
        {
            IntPtr hWnd = FindWindow(name, null);
            return hWnd;
        }

        public static void PositionWindow(IntPtr hWnd)
        {
            SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
        }

        public static IEnumerable<string> GetAllWindows()
        {
            List<string> windows = new();

            EnumWindows(delegate(IntPtr window, IntPtr param)
            {
                string windowText = GetWindowText(window);
                windows.Add(windowText);
                return true;
            }, IntPtr.Zero);

            return windows;
        }

        public static string GetWindowText(IntPtr hWnd)
        {
            int size = GetWindowTextLength(hWnd);
            if (size > 0)
            {
                var builder = new StringBuilder(size + 1);
                GetWindowText(hWnd, builder, builder.Capacity);
                return builder.ToString();
            }

            return String.Empty;
        }
    }
}
