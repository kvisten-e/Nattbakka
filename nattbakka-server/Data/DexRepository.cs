using Microsoft.EntityFrameworkCore;
using nattbakka_server.Models;

namespace nattbakka_server.Data
{
    public class DexRepository
    {
        private readonly DataContext _context;

        public DexRepository(DataContext context) { 
        _context = context;
        }

        public async Task<List<Dex>> GetDexesAsync()
        {
            var data = await _context.dex.Where(d => d.active == true).ToListAsync();
            return data;
        }

    }
}
