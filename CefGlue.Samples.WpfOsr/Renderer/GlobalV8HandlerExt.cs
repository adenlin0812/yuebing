using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xilium.CefGlue.Samples.WpfOsr.Renderer
{
    class GlobalV8HandlerExt : CefV8Handler
    {
        private string _executeGetElementPosJsCode = @"
            var selectDom = document.getElementsByClassName(""{0}"");
            for(var i = 0; i<collection.length; i++)
            { 
                if(collection[i].id==""{1}"")
                {
                    var rect = collection[i].getBoundingClientRect();
                    w = rect.width || rect.right - rect.left;
                    h = rect.height || rect.bottom - rect.top;
                    ww = document.documentElement.clientWidth; 
                    hh = document.documentElement.clientHeight;
                    return { 
                        top: Math.floor(rect.top), 
                        bottom: Math.floor(hh - rect.bottom), 
                        left: Math.floor(rect.left), 
                        right: Math.floor(ww - rect.right) 
                    };
                }
            }";

        private CefBrowser _browser = null;
        public GlobalV8HandlerExt(CefBrowser browser)
        {
            _browser = browser;
        }

        protected override bool Execute(string name, CefV8Value obj, CefV8Value[] arguments, out CefV8Value returnValue, out string exception)
        {
            returnValue = null;
            exception = null;
            if (name == "getElementPosition")
            {
                CefV8Value value = CefV8Value.CreateObject();
                CefV8Exception exp;
                _browser.GetMainFrame().V8Context.TryEval(string.Format(_executeGetElementPosJsCode, arguments[0].GetStringValue(), arguments[1].GetStringValue()),
                    "", 0, out value, out exp);

                var top = value.GetValue("top").GetIntValue();
                var bottom = value.GetValue("bottom").GetIntValue();
                var left = value.GetValue("left").GetIntValue();
                var right = value.GetValue("right").GetIntValue();

                return true;

                //// 构造消息及参数
                //CefProcessMessage msg = CefProcessMessage.Create("SetUserAgent");
                //using (CefListValue cefListVal = msg.Arguments)
                //{
                //    cefListVal.SetString(0, arguments[0].GetStringValue());
                //}
                //_browser.SendProcessMessage(CefProcessId.Browser, msg);
            }

            return false;
        }
    }
}
