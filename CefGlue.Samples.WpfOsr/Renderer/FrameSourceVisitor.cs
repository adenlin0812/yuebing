using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xilium.CefGlue.Samples.WpfOsr.Renderer
{
    class FrameSourceVisitor : CefStringVisitor
    {
        private string _source;

        protected override void Visit(string value)
        {
            _source = value;
        }

        public string GetString()
        {
            return _source;
        }
    }
}
