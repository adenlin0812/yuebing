using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xilium.CefGlue.Samples.WpfOsr.Renderer
{
    class GlobalV8HandlerExt : CefV8Handler
    {
        private string _executeGetElementPosJsCode = @"function test(){{
            var collection = document.getElementsByClassName(""{0}"");
                for(var i = 0; i < collection.length; i++)
                {{ 
                    if( collection[i].id == ""{1}"" || ""{1}"" == """")
                    {{
                        collection[i].scrollIntoView();
                        var rect = collection[i].getBoundingClientRect();
                        return rect;
                    }}
                }}
            }}
            test();";

        private string _executeGetElementPosByIdJsCode = @"function test2(){{
                var element = document.getElementById(""{0}"");
                element.scrollIntoView();
                document.documentElement.scrollTo(0, document.documentElement.scrollTop - 65);
                return element.getBoundingClientRect();
            }}
            test2();";

        private CefBrowser _browser = null;
        public GlobalV8HandlerExt(CefBrowser browser)
        {
            _browser = browser;
        }

        protected override bool Execute(string name, CefV8Value obj, CefV8Value[] arguments, out CefV8Value returnValue, out string exception)
        {
            returnValue = CefV8Value.CreateNull();
            exception = null;

            if (name == "getElementPosition")
            {
                CefV8Value retVal = CefV8Value.CreateObject();
                CefV8Exception exp;
                _browser.GetMainFrame().V8Context.TryEval(
                    string.Format(_executeGetElementPosJsCode, arguments[0].GetStringValue(), arguments[1].GetStringValue()),
                    "", 0, out retVal, out exp);

                returnValue = retVal;
                return true;
            }

            if (name == "getElementPositionByInnerText")
            {
                CefV8Value retVal = CefV8Value.CreateObject();
                CefV8Exception exp;
                _browser.GetMainFrame().V8Context.TryEval("document.getElementById(\"content_left\").innerHTML", "", 0, out retVal, out exp);

                if (retVal.IsString)
                {
                    var sourceString = retVal.GetStringValue();

                    var textToFind = arguments[0].GetStringValue();
                    var index = sourceString.IndexOf(textToFind);
                    if (index != -1)
                    {
                        var tmpSrcString = sourceString.Substring(0, index);
                        tmpSrcString = tmpSrcString.Substring(tmpSrcString.LastIndexOf(" id=\""), 
                            tmpSrcString.Length - tmpSrcString.LastIndexOf(" id=\""));
                        var classNameBegin = tmpSrcString.IndexOf("\"") + 1;
                        var idName = tmpSrcString.Substring(classNameBegin, 
                            tmpSrcString.IndexOf("\"", classNameBegin) - classNameBegin);

                        _browser.GetMainFrame().V8Context.TryEval(string.Format(_executeGetElementPosByIdJsCode, idName),
                            "", 0, out retVal, out exp);
                        returnValue = retVal;

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
