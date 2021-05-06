using System;
using System.Security.Cryptography;
using System.Text;
using Castle.DynamicProxy;
using Microsoft.Extensions.Caching.Distributed;

namespace Cache.Aspect
{
    public class CacheInterceptor : IInterceptor
    {
        private readonly IDistributedCache _distributedCache;

        public CacheInterceptor(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public void Intercept(IInvocation invocation)
        {
            var cacheMethodAttribute = GetCacheMethodAttribute(invocation);
            if (cacheMethodAttribute == null)
            {
                invocation.Proceed();
                return;
            }

            var cacheDuration = ((CacheMethodAttribute)cacheMethodAttribute).SecondsToCache;

            var cacheKey = GetCacheKey(invocation);

            var cachedResult = _distributedCache.Get(cacheKey);
            if (cachedResult != null)
            {
                var lz4Decompress = cachedResult.Lz4Decompress();
                invocation.ReturnValue = lz4Decompress.ToObject();
            }
            else
            {
                invocation.Proceed();
                if (invocation.ReturnValue == null)
                    return;

                var absoluteExpiration = new DateTimeOffset(DateTime.Now.AddMinutes(cacheDuration));
                var cacheEntryOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = absoluteExpiration
                };

                var byteArray = invocation.ReturnValue.ToByteArray();
                var lz4Compress = byteArray.Lz4Compress();
                _distributedCache.Set(cacheKey, lz4Compress, cacheEntryOptions);
            }
        }

        private static Attribute GetCacheMethodAttribute(IInvocation invocation)
        {
            var methodInfo = invocation.MethodInvocationTarget;
            if (methodInfo == null)
                methodInfo = invocation.Method;

            return Attribute.GetCustomAttribute(methodInfo, typeof(CacheMethodAttribute), true);
        }

        private static string GetCacheKey(IInvocation invocation)
        {
            var byteArray = invocation.Arguments.ToByteArray();
            var cacheKey = $"{invocation.TargetType.FullName}" +
                           $"{invocation.Method.Name}" +
                           $"{Encoding.UTF8.GetString(byteArray)}";

            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(cacheKey));
            return Encoding.UTF8.GetString(hash).ToBase64Encode();
        }
    }
}