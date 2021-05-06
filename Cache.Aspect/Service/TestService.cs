using System;
using MessagePack;

namespace Cache.Aspect.Service
{
    public interface ITestService
    {
        Result GetName(Param1 param);
    }

    public class TestService : ITestService
    {
        public TestService()
        {
            
        }

        [CacheMethod(100)]
        public Result GetName(Param1 param)
        {
            return new Result{A = "A", Param1 = param};
        }
    }

    [Serializable]
    [MessagePackObject]
    public class Result
    {
        [Key(0)]
        public string A { get; set; }

        [Key(1)]
        public Param1 Param1 { get; set; }

        [Key(3)]
        public DateTime DateTime { get; set; } = DateTime.Now;
    }

    [Serializable]
    [MessagePackObject]
    public class Param1
    {
        [Key(0)]
        public string Name { get; set; }

        [Key(1)]
        public Param2 Param2 { get; set; }
    }

    [Serializable]
    [MessagePackObject]
    public class Param2
    {
        [Key(0)]
        public string Name { get; set; }
    }

}
