using System;
using System.Collections.Generic;
using System.Text;

namespace Xyapper
{
    public class XyapperException : Exception
    {
        public XyapperException(string message) : base (message)
        {
        }
    }
}
