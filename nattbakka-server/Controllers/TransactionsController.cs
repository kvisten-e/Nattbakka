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
    public class TransactionsController : ControllerBase
    {
        private readonly InMemoryDataContext _context;

        public TransactionsController(InMemoryDataContext context)
        {
            _context = context;
        }

        // GET: api/Transactions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transaction>>> Gettransaction()
        {
            return await _context.transaction.ToListAsync();
        }

        // GET: api/Transactions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<List<Transaction>>> GetTransactionsByGroup(int id)
        {
            var transactionsByGroup = await _context.transaction
                .Where(x => x.group_id == id)
                .ToListAsync();

            if (transactionsByGroup == null || !transactionsByGroup.Any())
            {
                return NotFound();
            }

            return transactionsByGroup;
        }


    }
}
