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

        [DllImport("user32.dll")]
        private static extern int GetLastWin32Error();

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        // Delegate to filter which windows to include 
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        protected override void OnSourceInitialized(EventArgs e)
        {
            var config = new HotKeyConfig(System.Windows.Input.Key.Space, System.Windows.Input.Key.LeftCtrl, new WindowInteropHelper(this), async delegate
            {
                // get list of all connected monitors
                List<string> monitors = new();

                foreach (var screen in Screen.AllScreens)
                {
                    monitors.Add(screen.ToString());
                }

                var windows = GetAllWindows();

                //await File.WriteAllTextAsync("C:/Users/brady/Documents/targetWindow.txt", targetWindow.ToString());
                await File.WriteAllLinesAsync("C:/Users/brady/Documents/monitorsV2.txt", monitors);
                await File.WriteAllLinesAsync("C:/Users/brady/Documents/windowsV2.txt", windows);
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
