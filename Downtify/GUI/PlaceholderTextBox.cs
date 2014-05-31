using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Downtify.GUI
{
    public class PlaceholderTextBox : TextBox
    {
        private const uint ECM_FIRST = 0x1500;
        private const uint EM_SETCUEBANNER = ECM_FIRST + 1;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, uint wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        private string placeholder;
        public string Placeholder
        {
            get
            {
                return placeholder;
            }
            set
            {
                placeholder = value;
                SetPlaceholder();
            }
        }

        private void SetPlaceholder()
        {
            SendMessage(this.Handle, EM_SETCUEBANNER, 0, placeholder);
        }

    }
}