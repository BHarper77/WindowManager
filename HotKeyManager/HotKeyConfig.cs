using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interop;

namespace HotKeyLibrary
{
    /// <summary>
    /// Class <c>HotKeyConfig</c> represents a hot key configuration used by the <c>HotKeyManager</c> to implement hotkeys and their implementations
    /// </summary>
    public class HotKeyConfig
    {
        public readonly uint PrimaryKey, SecondaryKey;
        public readonly int HotKeyId;
        public HwndSource? Source { get; set; }
        public readonly WindowInteropHelper Helper;
        public readonly Func<Task<bool>> OnHotKeyPressed;

        /// <param name="primaryKey">Primary hotkey</param>
        /// <param name="secondaryKey">Modifier hotkey</param>
        /// <param name="helper">The Interop helper for the window that will handle events</param>
        /// <param name="onHotKeyPressed">Function to be executed on hotkey press</param>
        public HotKeyConfig(Key primaryKey, ModKeys secondaryKey, WindowInteropHelper helper, Func<Task<bool>> onHotKeyPressed)
        {
            this.HotKeyId = new Random().Next(10000, 99999);
            this.PrimaryKey = (uint)KeyInterop.VirtualKeyFromKey(primaryKey);
            this.SecondaryKey = (uint)secondaryKey;
            this.Helper = helper;
            this.Source = HwndSource.FromHwnd(helper.Handle);
            this.OnHotKeyPressed = onHotKeyPressed;

        }
    }
}
