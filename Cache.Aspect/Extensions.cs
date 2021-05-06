using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using K4os.Compression.LZ4;
using MessagePack;

namespace Cache.Aspect
{
    public static class Extensions
    {
        public static byte[] ToByteArray(this object obj)
        {
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static object ToObject(this byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                return obj;
            }
        }

        public static byte[] Lz4Compress(this byte[] bytes, LZ4Level level = LZ4Level.L09_HC)
        {
            var source = bytes.AsSpan();
            var target = new byte[LZ4Codec.MaximumOutputSize(source.Length) + 4].AsSpan();
            var size = BitConverter.GetBytes(source.Length).AsSpan();
            size.CopyTo(target);
            var compressedBytesSize = LZ4Codec.Encode(source, target.Slice(4), level);
            return target.Slice(0, compressedBytesSize + 4).ToArray();
        }

        public static byte[] Lz4Decompress(this byte[] compressedBytes)
        {
            var source = compressedBytes.AsSpan();
            var size = source.Slice(0, 4).ToArray();
            var length = BitConverter.ToInt32(size, 0);
            var target = new byte[length].AsSpan();
            var decoded = LZ4Codec.Decode(source.Slice(4), target);
            return target.Slice(0, decoded).ToArray();
        }

        public static byte[] SerializeMessagePackLz4<T>(this T value)
        {
            var lz4Options =
                MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
            return MessagePackSerializer.Serialize(value, lz4Options);
        }

        public static T DeserializeMessagePackLz4<T>(this byte[] bytes)
        {
            var lz4Options =
                MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
            return MessagePackSerializer.Deserialize<T>(bytes, lz4Options);
        }

        public static string ToBase64Encode(this string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string ToBase64Encode(this byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        public static string ToBase64Decode(this string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}