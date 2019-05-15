using System;
using System.Collections.Generic;
using System.Text;

namespace Xyapper
{
    /// <summary>
    /// Generic exception for Xyapper
    /// </summary>
    public class XyapperException : Exception
    {
        public XyapperException(string message) : base (message)
        {
        }
    }
}
