using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WKFramework.Utils.Serializer
{
    public abstract class Serializer<TSerialized> : ISerializer<TSerialized>
    {
        public abstract TSerialized Serialize<TSource>(TSource obj);

        public abstract TResult Deserialize<TResult>(TSerialized obj);

        public virtual object Serialize(object obj)
        {
            return Serialize<object>(obj);
        }

        public virtual object Deserialize(object obj)
        {
            return Deserialize<object>((TSerialized)obj);
        }
    }
}
