using Comunidad_Decidida.Models;
using Comunidad_Decidida.Repositories;
using PdfSharp.UniversalAccessibility;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comunidad_Decidida.Services
{
    public class AsociadoService
    {
        private readonly IAsociadoRepository _asociadoRepository;
        private readonly ITagRepository _tagRepository;

        public AsociadoService(IAsociadoRepository asociadoRepository, ITagRepository tagRepository)
        {
            _asociadoRepository = asociadoRepository;
            _tagRepository = tagRepository;
        }

        public async Task<IEnumerable<AsociadoTags>> GetAsociadosTagsAsync()
        {
            return await _asociadoRepository.GetAsociadosTagsAsync();
        }

        public async Task<string> GetUltimaTagAsync()
        {
            return await _tagRepository.GetUltimaTagAsync();
        }

        public async Task<Tag> AgregarTagAsociado(TagRequest Tag)
        {
            return await _tagRepository.AgregarTagAsociado(Tag).ConfigureAwait(false);
        }



        public async Task<Asociado> GetAsociadoByIdAsync(int id)
        {
            return await _asociadoRepository.GetAsociadoByIdAsync(id);
        }

        public async Task<Tag> GetTagByAsociadoIdAndEtiquetaAsync(int asociadoId, string etiqueta)
        {
            return await _tagRepository.GetTagByAsociadoIdAndEtiquetaAsync(asociadoId, etiqueta);
        }

        public async Task SaveCancelacionWAPathAsync(int id, string path, string etiqueta)
        {
            try
            {
                var tag = await _tagRepository.GetTagByAsociadoIdAndEtiquetaAsync(id, etiqueta);
                if (tag != null)
                {
                    tag.CancelacionWA = path;
                    tag.Activa = 1;
                    await _tagRepository.UpdateTagAsync(tag);
                }
                else
                {
                    Console.WriteLine($"Tag not found for Asociado ID: {id} and Etiqueta: {etiqueta}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving CancelacionWA path: {ex.Message}");
                throw new Exception($"Error saving CancelacionWA path for Asociado ID {id}", ex);
            }
        }

        public async Task SaveCancelacionDocPDFPathAsync(int id, string path, string etiqueta)
        {
            try
            {
                var tag = await _tagRepository.GetTagByAsociadoIdAndEtiquetaAsync(id, etiqueta);
                if (tag != null)
                {
                    tag.DocCancelacion = path;
                    tag.Activa = 1;
                    await _tagRepository.UpdateTagAsync(tag);
                }
                else
                {
                    Console.WriteLine($"Tag not found for Asociado ID: {id} and Etiqueta: {etiqueta}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving CancelacionWA path: {ex.Message}");
                throw new Exception($"Error saving CancelacionWA path for Asociado ID {id}", ex);
            }
        }
    }
}
