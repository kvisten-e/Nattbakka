using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nattbakka_server.Data;
using nattbakka_server.Models;

namespace nattbakka_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionWithGroupsController : ControllerBase
    {
        private readonly InMemoryDataContext _context;

        public TransactionWithGroupsController(InMemoryDataContext context)
        {
            _context = context;
        }

        // GET: api/TransactionWithGroups
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransactionWithGroup>>> GetTransactionWithGroup()
        {
            return await _context.TransactionWithGroup.ToListAsync();
        }

        // GET: api/TransactionWithGroups/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionWithGroup>> GetTransactionWithGroup(int id)
        {
            var transactionWithGroup = await _context.TransactionWithGroup.FindAsync(id);

            if (transactionWithGroup == null)
            {
                return NotFound();
            }

            return transactionWithGroup;
        }


        private bool TransactionWithGroupExists(int id)
        {
            return _context.TransactionWithGroup.Any(e => e.id == id);
        }
    }
}
