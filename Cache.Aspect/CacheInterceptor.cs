using System;
using System.Linq;
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
                // متد جاری توسط ویژگی کش شدن مزین نشده است
                // بنابراین آن‌را اجرا کرده و کار را خاتمه می‌دهیم
                invocation.Proceed();
                return;
            }

            // دراینجا مدت زمان کش شدن متد از ویژگی کش دریافت می‌شود
            var cacheDuration = ((CacheMethodAttribute)cacheMethodAttribute).SecondsToCache;

            // برای ذخیره سازی اطلاعات در کش نیاز است یک کلید منحصربفرد را
            //  بر اساس نام متد و پارامترهای ارسالی به آن تهیه کنیم
            var cacheKey = GetCacheKey(invocation);

            var cachedResult = _distributedCache.Get(cacheKey);
            if (cachedResult != null)
            {
                // اگر نتیجه بر اساس کلید تشکیل شده در کش موجود بود
                // همان را بازگشت می‌دهیم
                invocation.ReturnValue = cachedResult.ToObject();
            }
            else
            {
                // در غیر اینصورت ابتدا متد را اجرا کرده
                invocation.Proceed();
                if (invocation.ReturnValue == null)
                    return;

                // سپس نتیجه آن‌را کش می‌کنیم
                var absoluteExpiration = new DateTimeOffset(DateTime.Now.AddMinutes(cacheDuration));
                var cacheEntryOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = absoluteExpiration
                };

                _distributedCache.Set(cacheKey, invocation.ReturnValue.ToByteArray(), cacheEntryOptions);
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

            // کار کردن با هش سریعتر خواهد بود
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(cacheKey));
            return Encoding.UTF8.GetString(hash);
        }
    }
}