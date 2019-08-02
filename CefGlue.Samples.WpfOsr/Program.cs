namespace Xilium.CefGlue.Samples.WpfOsr
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;

    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            try
            {
                CefRuntime.Load();
            }
            catch (DllNotFoundException ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return 1;
            }
            catch (CefRuntimeException ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return 2;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return 3;
            }

            var mainArgs = new CefMainArgs(args);

            var exitCode = CefRuntime.ExecuteProcess(mainArgs, SampleCefApp.GetInstance(), IntPtr.Zero);
            if (exitCode != -1) { return exitCode; }


            var cefSettings = new CefSettings
            {
                // BrowserSubprocessPath = browserSubprocessPath,
                UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; InfoPath.3)",
                SingleProcess = true,
                WindowlessRenderingEnabled = true,
                MultiThreadedMessageLoop = true,
                LogSeverity = CefLogSeverity.Verbose,
                LogFile = "cef.log",
            };

            try
            {
                CefRuntime.Initialize(mainArgs, cefSettings, SampleCefApp.GetInstance(),IntPtr.Zero);
            }
            catch (CefRuntimeException ex)
            {
                MessageBox.Show(ex.ToString(), "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return 4;
            }


            var app = new Xilium.CefGlue.Samples.WpfOsr.App();
            app.InitializeComponent();
            app.Run();

            // shutdown CEF
            CefRuntime.Shutdown();

            return 0;
        }
    }
}
