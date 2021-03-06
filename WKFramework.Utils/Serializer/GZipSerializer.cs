﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WKFramework.Utils.Serializer
{
    public class GZipSerializer : BinarySerializer
    {
        public GZipSerializer()
        {
        }

        public override byte[] Serialize(object obj)
        {
            if (obj == null)
                return null;

            //There is an issue with GZipStream. It has to be closed first to be able to read compressed stream.
            using (var compressedStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Compress))
                {
                    var bytes = base.Serialize(obj);
                    gzipStream.Write(bytes, 0, bytes.Length); 
                }

                return compressedStream.ToArray();
            }
        }

        public override TResult Deserialize<TResult>(byte[] data)
        {
            if (data == null)
                return default(TResult);

            using (var compressedStream = new MemoryStream(data))
            using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var decompressedStream = new MemoryStream())
            {
                gzipStream.CopyTo(decompressedStream);
                var bytes = decompressedStream.ToArray();

                return base.Deserialize<TResult>(bytes);
            }
        }
    }
}
