using System;
using System.Windows.Forms;
using QuanLyThuVien.Forms;

namespace QuanLyThuVien
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Thay frmDangNhap bằng tên form mở đầu của chương trình (ví dụ: frmDangNhap hoặc frmMain)
            Application.Run(new frmDangNhap());
        }
    }
}