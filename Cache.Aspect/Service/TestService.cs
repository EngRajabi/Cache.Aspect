using System;

namespace Cache.Aspect.Service
{
    public interface ITestService
    {
        string GetName(Param1 param);
    }

    public class TestService : ITestService
    {
        public TestService()
        {
            
        }

        [CacheMethod]
        public string GetName(Param1 param)
        {
            return "Mohsen";
        }
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
