using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Xyapper
{
    /// <summary>
    /// Interface for classed which must be deserialized manually
    /// </summary>
    public interface ICustomDeserialized
    {
        void Deserialize(IDataRecord record);
    }
}
