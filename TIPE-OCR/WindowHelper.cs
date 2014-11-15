using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TIPE_OCR
{
    public class WindowHelper
    {
        public static void ShowError(string message, IWin32Window owner = null)
        {
            MessageBox.Show(owner, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
