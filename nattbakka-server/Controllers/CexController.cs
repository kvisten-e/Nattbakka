using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using nattbakka_server.Data;
using nattbakka_server.Models;

namespace nattbakka_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CexController : ControllerBase
    {
        private readonly DatabaseComponents _databaseComponents;

        public CexController(DatabaseComponents databaseComponents)
        {
            _databaseComponents = databaseComponents;
        }

        // GET: api/Groups
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cex>>> Getcex()
        {
            return await _databaseComponents.GetCexesAsync();
        }
    }
}
