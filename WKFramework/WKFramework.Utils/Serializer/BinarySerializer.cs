using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace WKFramework.Utils.Serializer
{
    public class BinarySerializer : Serializer<byte[]>
    {
        public override byte[] Serialize<TSource>(TSource obj)
        {
            if (obj == null)
                return null;

            using (var stream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(stream, obj);
                return stream.ToArray();
            }
        }

        public override TResult Deserialize<TResult>(byte[] data)
        {
            if (data == null)
                return default(TResult) ;

            using (var stream = new MemoryStream(data))
            {
                return (TResult)new BinaryFormatter().Deserialize(stream);
            }
        }
    }
}
