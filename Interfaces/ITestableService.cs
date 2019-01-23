using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace ServiceFabric.Serialization.V2.Interfaces
{
    public interface ITestableService : IService
    {
        /// <summary>
        /// Tests the service to ensure it is working correctly
        /// </summary>
        /// <returns>True if the service is working correctly. False otherwise</returns>
        Task<bool> TestAsync();
    }
}