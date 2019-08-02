using Xilium.CefGlue.Samples.WpfOsr.Renderer;
namespace Xilium.CefGlue.Samples.WpfOsr
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;
    using Xilium.CefGlue;
    using Xilium.CefGlue.Wrapper;

    class DemoRenderProcessHandler : CefRenderProcessHandler
    {
        internal static bool DumpProcessMessages { get; private set; }
        private CefV8Context _context;

        public DemoRenderProcessHandler()
        {
            MessageRouter = new CefMessageRouterRendererSide(new CefMessageRouterConfig());
        }

        internal CefMessageRouterRendererSide MessageRouter { get; private set; }

        protected override void OnContextCreated(CefBrowser browser, CefFrame frame, CefV8Context context)
        {
            if (browser.Identifier != 1)
            {
                return;
            }

            _context = context;
            using (var global = _context.GetGlobal())
            {
                try
                {
                    _context.Enter();
                    var handler = new GlobalV8HandlerExt(browser);
                    using (var extApi = CefV8Value.CreateObject())
                    {
                        using (var getElementPositionFunc = CefV8Value.CreateFunction("getElementPosition", handler))
                        {
                            extApi.SetValue("getElementPosition", getElementPositionFunc, 
                                CefV8PropertyAttribute.ReadOnly | CefV8PropertyAttribute.DontEnum | CefV8PropertyAttribute.DontDelete);
                        }
                        using (var getElementPositionByInnerTextFunc = CefV8Value.CreateFunction("getElementPositionByInnerText", handler))
                        {
                            extApi.SetValue("getElementPositionByInnerText", getElementPositionByInnerTextFunc,
                                CefV8PropertyAttribute.ReadOnly | CefV8PropertyAttribute.DontEnum | CefV8PropertyAttribute.DontDelete);
                        }
                        global.SetValue("extApi", extApi);
                    }
                    _context.Exit();
                }
                catch
                {
                    return;
                }
            }
        }

        protected override void OnContextReleased(CefBrowser browser, CefFrame frame, CefV8Context context)
        {

        }

        protected override bool OnProcessMessageReceived(CefBrowser browser, CefProcessId sourceProcess, CefProcessMessage message)
        {
            switch(message.Name)
            {
                case "getElementPosition":
                    {
                        _context.Enter();
                        string elementClass = message.Arguments.GetString(0);
                        string elementId = message.Arguments.GetString(1);
                        string jsCode = string.Format("extApi.getElementPosition(\"{0}\",\"{1}\")", elementClass, elementId);
                        int hash = message.Arguments.GetInt(message.Arguments.Count - 1);

                        CefV8Value outValue;
                        CefV8Exception exception;
                        bool result = browser.GetMainFrame().V8Context.TryEval(jsCode, "", 0, out outValue, out exception);
                        if (result)
                        {
                            //funcConsoleLog.ExecuteFunction(funcConsoleLog, new CefV8Value[] { outValue });
                            // 构造消息及参数
                            CefProcessMessage msg = CefProcessMessage.Create("getElementPosition");
                            using (CefListValue cefListVal = msg.Arguments)
                            {
                                cefListVal.SetDouble(0, outValue.GetValue("x").GetDoubleValue());
                                cefListVal.SetDouble(1, outValue.GetValue("y").GetDoubleValue());
                                cefListVal.SetInt(2, hash);
                            }
                            browser.SendProcessMessage(CefProcessId.Browser, msg);
                        }
                        _context.Exit();
                    }
                    break;

                case "getElementPositionByInnerText":
                    {
                        _context.Enter();
                        string innerTextToFind = message.Arguments.GetString(0);
                        string jsCode = string.Format("extApi.getElementPositionByInnerText(\"{0}\")", innerTextToFind);
                        int hash = message.Arguments.GetInt(message.Arguments.Count - 1);

                        CefV8Value outValue;
                        CefV8Exception exception;
                        bool result = browser.GetMainFrame().V8Context.TryEval(jsCode, "", 0, out outValue, out exception);
                        if (result)
                        {
                            //funcConsoleLog.ExecuteFunction(funcConsoleLog, new CefV8Value[] { outValue });
                            // 构造消息及参数
                            CefProcessMessage msg = CefProcessMessage.Create("getElementPositionByInnerText");
                            using (CefListValue cefListVal = msg.Arguments)
                            {
                                cefListVal.SetDouble(0, outValue.GetValue("x").GetDoubleValue());
                                cefListVal.SetDouble(1, outValue.GetValue("y").GetDoubleValue());
                                cefListVal.SetInt(2, hash);
                            }
                            browser.SendProcessMessage(CefProcessId.Browser, msg);
                        }
                        _context.Exit();
                    }
                    break;

                default:
                    break;
            }

            return false;
        }
    }
}
