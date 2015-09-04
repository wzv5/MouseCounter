using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MouseCounter
{
    public class Win32Api
    {
        public delegate int HookProc(int nCode, int wParam, IntPtr lParam);
        //安装钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);
        //卸载钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);
        //调用下一个钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, int wParam, IntPtr lParam);
    }

    public class MouseHook
    {
        private int hHook = 0;
        private const int WH_MOUSE_LL = 14;

        public int SetHook()
        {
            if (hHook != 0)
                return hHook;

            var hProc = new Win32Api.HookProc(MouseHookProc);
            hHook = Win32Api.SetWindowsHookEx(WH_MOUSE_LL, hProc, IntPtr.Zero, 0);
            return hHook;
        }
        public void UnHook()
        {
            if (hHook == 0)
                return;
            Win32Api.UnhookWindowsHookEx(hHook);
            hHook = 0;
        }
        private int MouseHookProc(int nCode, int wParam, IntPtr lParam)
        {
            if (nCode == 0)
            {
                if (MouseDownEvent != null)
                {
                    MouseDownEvent(wParam);
                }
            }
            return Win32Api.CallNextHookEx(hHook, nCode, wParam, lParam);
        }
        //委托+事件（把钩到的消息封装为事件，由调用者处理）
        public delegate void MouseDownHandler(int btnmsg);
        public event MouseDownHandler MouseDownEvent;
    }
}
