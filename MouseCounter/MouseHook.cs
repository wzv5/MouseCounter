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

    public static class MouseHook
    {
        private static int hHook = 0;
        private const int WH_MOUSE_LL = 14;
        private static Win32Api.HookProc hProc = MouseHookProc;

        public static int SetHook()
        {
            if (hHook != 0)
                return hHook;
            hHook = Win32Api.SetWindowsHookEx(WH_MOUSE_LL, hProc, IntPtr.Zero, 0);
            return hHook;
        }
        public static void UnHook()
        {
            if (hHook == 0)
                return;
            Win32Api.UnhookWindowsHookEx(hHook);
            hHook = 0;
        }
        private static int MouseHookProc(int nCode, int wParam, IntPtr lParam)
        {
            if (nCode == 0)
            {
                if (MouseDownEvent != null)
                {
                    try
                    {
                        MouseDownEvent(wParam);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "钩子事件异常");
                    }
                    
                }
            }
            return Win32Api.CallNextHookEx(hHook, nCode, wParam, lParam);
        }

        public delegate void MouseDownHandler(int btnmsg);
        public static event MouseDownHandler MouseDownEvent;
    }
}
