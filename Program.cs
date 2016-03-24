using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;

namespace SetWindowPos
{
    class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int width, int height, uint uFlags);
        [DllImport("User32.Dll")]
        private static extern bool IsIconic(IntPtr hWnd);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        
        static void Main(string[] args)
        {
            const int SWP_SHOWWINDOW = 0x0040;
            const int SW_RESTORE = 9;
            int xpos = 0;
            int ypos = 0;
            int width = 0;
            int height = 0;
            bool regExFail = false;
            bool processFound = false;
            Regex regexInt = new Regex(@"^-?\d+$");
            for (int i = 1; i < 5; i++)
            {
                if (!regexInt.IsMatch(args[i]))
                    regExFail = true;
            }
            if (args == null || args.Length != 5 || regExFail)
            {
                Console.WriteLine("Usage: SetWindowPos.exe \"<Application Titlename>\" <xpos> <ypos> <width> <height>");
            }
            else
            {
                try
                {
                    int.TryParse(args[1], out xpos);
                    int.TryParse(args[2], out ypos);
                    int.TryParse(args[3], out width);
                    int.TryParse(args[4], out height);
                }
                catch (Exception)
                {
                    Console.WriteLine("Position and size must be numbers.");
                }

                Process[] processlist = Process.GetProcesses();
                foreach (Process process in processlist)
                {
                    if (!String.IsNullOrEmpty(process.MainWindowTitle))
                    {
                        if (process.MainWindowTitle.ToLower() == args[0].ToLower())
                        {
                            processFound = true;
                            if (IsIconic(process.MainWindowHandle))
                            {
                                ShowWindow(process.MainWindowHandle, SW_RESTORE);
                                Thread.Sleep(500); // Wait 500ms for the window status to update
                            }
                            try
                            {
                                IntPtr Top = new IntPtr(0);
                                SetWindowPos(process.MainWindowHandle, Top, xpos, ypos, width, height, SWP_SHOWWINDOW);
                                Console.WriteLine("\"{0}\" updated.", process.MainWindowTitle);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }
                        }
                    }
                }
                if (!processFound)
                    Console.WriteLine("No application with the title \"{0}\" found.", args[0]);
            }
        }
    }
}
