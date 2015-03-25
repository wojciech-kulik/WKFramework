using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WKFramework.Utils.Serializer;

namespace UnitTests.SettingsTests.TestClasses
{
    public class NoneSerializer : ISerializer
    {
        public object Serialize(object obj)
        {
            return obj;
        }

        public object Deserialize(object obj)
        {
            return obj;
        }
    }
}
