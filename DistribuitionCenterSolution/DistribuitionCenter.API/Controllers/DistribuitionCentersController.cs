using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DistribuitionCenter.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DistribuitionCentersController : ControllerBase
    {
        private static readonly List<string> PossibleCenters = new List<string>
    {
        "CD1", "CD2", "CD3", "CD4", "CD5", "CD6", "CD7", "CD8", "CD9", "CD10"
    };

        private readonly Random _random = new();

        [HttpGet]
        public IActionResult GetDistribuitionCenters([FromQuery] int itemId)
        {
            // Gera um número aleatório de centros de distribuição entre 1 e o número máximo de centros disponíveis
            int numCenters = _random.Next(1, PossibleCenters.Count + 1);

            // Seleciona aleatoriamente um número de centros de distribuição sem repetição
            var selectedCenters = PossibleCenters.OrderBy(x => _random.Next())
                                                 .Take(numCenters)
                                                 .ToList();

            var response = new DistribuitionCenterResponse
            {
                DistribuitionCenters = selectedCenters
            };

            return Ok(response);
        }
    }
}
