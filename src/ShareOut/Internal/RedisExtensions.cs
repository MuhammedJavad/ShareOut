using System;
using System.Text.Json;
using StackExchange.Redis;

namespace ShareOut.Internal
{
    static class RedisExtensions
    {
        public static byte[] CompressValue<T>(this T value)
            where T : class, new()
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, new JsonSerializerOptions()
            {
                IgnoreNullValues = true,
                IncludeFields = true
            });

            return bytes;
        }

        public static byte[] DecompressValue(this RedisValue value)
        {
            return value.HasValue ? DecompressValue((byte[])value) : Array.Empty<byte>();
        }

        public static byte[] DecompressValue(this byte[] value)
        {
            return value is { Length: > 0 } ?
                value :
                Array.Empty<byte>();
        }

        public static T Deserialize<T>(this byte[] value) where T : class, new()
        {
            return value is { Length: > 0 } ? JsonSerializer.Deserialize<T>(value) : null;
        }
    }
}
