using Comunidad_Decidida.Models;
using System.Threading.Tasks;

namespace Comunidad_Decidida.Repositories
{
    public interface ITagRepository
    {
        Task<Tag> GetTagByIdAsync(int id);
        Task<string> GetUltimaTagAsync();
        Task<Tag> GetTagByAsociadoIdAndEtiquetaAsync(int asociadoId, string etiqueta);
        Task<Tag> AgregarTagAsociado(TagRequest tag);
        Task UpdateTagAsync(Tag tag);
    }
}
