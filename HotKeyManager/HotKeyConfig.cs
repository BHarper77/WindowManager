using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interop;

namespace HotKeyLibrary
{
    public class HotKeyConfig
    {
        public readonly int PrimaryKey, SecondaryKey;
        public readonly int HotKeyId;
        public HwndSource? Source { get; set; }
        public readonly WindowInteropHelper Helper;
        public readonly Func<Task<bool>> OnHotKeyPressed;

        public HotKeyConfig(Key primaryKey, Key secondaryKey, WindowInteropHelper helper, Func<Task<bool>> onHotKeyPressed)
        {
            this.HotKeyId = new Random().Next(10000, 99999);
            this.PrimaryKey = KeyInterop.VirtualKeyFromKey(primaryKey);
            this.SecondaryKey = KeyInterop.VirtualKeyFromKey(secondaryKey);
            this.Helper = helper;
            this.Source = HwndSource.FromHwnd(helper.Handle);
            this.OnHotKeyPressed = onHotKeyPressed;
        }
    }
}
