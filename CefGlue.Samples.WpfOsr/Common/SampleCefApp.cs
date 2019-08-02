namespace Xilium.CefGlue.Samples.WpfOsr
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Xilium.CefGlue;

    internal sealed class SampleCefApp : CefApp
    {
        private static SampleCefApp _instance = null;

        private CefRenderProcessHandler _renderProcessHandler = new DemoRenderProcessHandler();


        public static SampleCefApp GetInstance()
        {
            // 如果类的实例不存在则创建，否则直接返回
            if (_instance == null)
            {
                _instance = new SampleCefApp();
            }
            return _instance;
        }

        private SampleCefApp()
        {

        }

        protected override void OnBeforeCommandLineProcessing(string processType, CefCommandLine commandLine)
        {
        }

        protected override CefRenderProcessHandler GetRenderProcessHandler()
        {
            return _renderProcessHandler;
        }
    }
}
