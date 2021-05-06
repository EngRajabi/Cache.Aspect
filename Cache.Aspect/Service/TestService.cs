using System;

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
    public class Result
    {
        public string A { get; set; }
        public Param1 Param1 { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now;
    }

    [Serializable]
    public class Param1
    {
        public string Name { get; set; }
        public Param2 Param2 { get; set; }
    }
    [Serializable]
    public class Param2
    {
        public string Name { get; set; }
    }

}
