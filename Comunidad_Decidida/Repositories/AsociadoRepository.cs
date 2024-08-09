using Comunidad_Decidida.Infrastructure;
using Comunidad_Decidida.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comunidad_Decidida.Repositories
{
    public class AsociadoRepository : IAsociadoRepository
    {
        private readonly ApiDbContext _context;

        public AsociadoRepository(ApiDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AsociadoTags>> GetAsociadosTagsAsync()
        {
            return await _context.AsociadosTags.FromSqlRaw("EXEC sp_GetAsociadosConTag").ToListAsync();
        }

        public async Task<Asociado> GetAsociadoByIdAsync(int id)
        {
            return await _context.Asociado
                .Include(a => a.Tags)
                .Include(a => a.Direcciones)
                .FirstOrDefaultAsync(a => a.IDAsociado == id);
        }

        public async Task UpdateAsociadoAsync(Asociado asociado)
        {
            _context.Asociado.Update(asociado);
            await _context.SaveChangesAsync();
        }
    }
}
