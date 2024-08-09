using Comunidad_Decidida.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comunidad_Decidida.Repositories
{
    public interface IAsociadoRepository
    {
        Task<IEnumerable<AsociadoTags>> GetAsociadosTagsAsync();
        Task<Asociado> GetAsociadoByIdAsync(int id);
        Task UpdateAsociadoAsync(Asociado asociado);
    }
}

