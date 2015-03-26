using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WKFramework.Utils.Serializer
{
    public abstract class Serializer<TSerialized> : ISerializer<TSerialized>
    {
        public abstract TSerialized Serialize(object obj);

        public abstract TResult Deserialize<TResult>(TSerialized obj);

        TSerialized ISerializer<TSerialized>.Serialize(object obj)
        {
            return Serialize(obj);
        }

        TResult ISerializer<TSerialized>.Deserialize<TResult>(TSerialized obj)
        {
            return Deserialize<TResult>(obj);
        }

        object ISerializer.Serialize(object obj)
        {
            return Serialize(obj);
        }

        object ISerializer.Deserialize(object obj)
        {
            return Deserialize<object>((TSerialized)obj);
        }
    }
}
