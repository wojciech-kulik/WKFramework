using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WKFramework.Utils.Serializer;

namespace WKFramework.UnitTests.SettingsTests.TestClasses
{
    public class ToStringSerializer : ISerializer
    {
        public object Serialize(object obj)
        {
            return obj.GetType().Name + "|" +  obj.ToString();
        }

        public object Deserialize(object obj)
        {
            var serialized = obj as string;
            var data = serialized.Split('|');

            if (data[0] == "Int32")
            {
                return Convert.ToInt32(data[1]);
            }
            else if (data[0] == "TestValueEnum")
            {
                return Enum.Parse(typeof(TestValueEnum), data[1]);
            }

            return data[1];
        }
    }
}
