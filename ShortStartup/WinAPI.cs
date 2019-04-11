using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Input;

namespace ShortStartup
{
    //引入函数(注册热键)
    internal class HotKeyWinApi
    {
        public const int WmHotKey = 0x0312;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, ModifierKeys fsModifiers, Keys vk);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }

    //引入函数(读取程序图标)
    internal class ExtractIconWinApi
    {
        [System.Runtime.InteropServices.DllImport("shell32.dll")]
        public static extern int ExtractIconEx(string lpszFile, int niconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, int nIcons);

        [System.Runtime.InteropServices.DllImport("shell32.dll")]
        public static extern bool DestroyIcon(IntPtr handle);
    }

    //引入函数(弹框自动关闭)
    internal class MessageBoxTimeoutWinApi
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int MessageBoxTimeoutA(IntPtr hWnd, string msg, string Caps, int type, int Id, int time);
    }
}
