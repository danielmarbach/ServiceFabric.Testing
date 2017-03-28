using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace TestRunner
{
    public interface ITestRunner : IService
    {
        Task<string[]> Tests();

        Task<Result> Run(string testName);
    }
}