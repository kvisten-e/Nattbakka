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
    public class TransactionGroupController : ControllerBase
    {
        private readonly InMemoryDataContext _context;

        public TransactionGroupController(InMemoryDataContext context)
        {
            _context = context;
        }

        // GET: api/TransactionGroup
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransactionGroup>>> GetTransactionGroup()
        {
            var transactions = await _context.Transaction.ToListAsync();
            var groups = await _context.Group.ToListAsync();

            var transactionGroups = groups.Select(group => new TransactionGroup
            {
                Id = group.Id,
                Created = group.Created,
                TimeDifferentUnix = group.TimeDifferentUnix,
                Transactions = transactions.Where(t => t.GroupId == group.Id).ToList(),
            }).ToList();

            return transactionGroups;
        }

        // GET: api/TransactionGroup/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionGroup>> GetTransactionGroup(string id)
        {
            var group = await _context.Group.FindAsync(id);

            if (group == null)
            {
                return NotFound();
            }

            var transactions = await _context.Transaction
                .Where(t => t.GroupId == id)
                .ToListAsync();

            var transactionGroup = new TransactionGroup
            {
                Id = group.Id,
                Created = group.Created,
                TimeDifferentUnix = group.TimeDifferentUnix,
                Transactions = transactions
            };

            return transactionGroup;
        }

    }
}
