using System;

namespace Cache.Aspect
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CacheMethodAttribute : Attribute
    {
        public CacheMethodAttribute(int secondsToCache = 10)
        {
            SecondsToCache = secondsToCache;
        }

        public int SecondsToCache { get; set; }
    }
}