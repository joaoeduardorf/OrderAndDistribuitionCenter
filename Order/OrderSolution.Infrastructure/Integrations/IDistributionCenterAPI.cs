using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderSolution.Infrastructure.Integrations
{
    public interface IDistributionCenterAPI
    {
        Task<string> GetDistributionCentersByItemAsync(int idSku, string correlationId);
    }
}
