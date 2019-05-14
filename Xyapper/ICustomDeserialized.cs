using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Xyapper
{
    public interface ICustomDeserialized
    {
        void Deserialize(IDataRecord record);
    }
}
