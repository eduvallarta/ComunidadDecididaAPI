using Comunidad_Decidida.Infrastructure;
using Comunidad_Decidida.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Comunidad_Decidida.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly ApiDbContext _context;

        public TagRepository(ApiDbContext context)
        {
            _context = context;
        }

        public async Task<Tag> GetTagByAsociadoIdAndEtiquetaAsync(int asociadoId, string etiqueta)
        {
            Tag tag = null;

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "GetTagByAsociadoIdAndEtiqueta";
                command.CommandType = CommandType.StoredProcedure;

                SqlParameter[] parametros = new SqlParameter[]
                {
            new SqlParameter("@IDSAE", asociadoId),
            new SqlParameter("@Etiqueta", etiqueta)
                };
                command.Parameters.AddRange(parametros);

                await _context.Database.OpenConnectionAsync().ConfigureAwait(false);

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    if (reader.Read())
                    {
                        tag = new Tag
                        {
                            IDTags = reader.GetInt32(reader.GetOrdinal("IDTags")),
                            IDSAE = reader.GetInt32(reader.GetOrdinal("IDSAE")),
                            Identificador = reader.GetString(reader.GetOrdinal("Identificador")),
                            Etiqueta = reader.GetString(reader.GetOrdinal("Etiqueta")),
                            Activa = reader.GetInt32(reader.GetOrdinal("Activa")),
                            CancelacionWA = reader.IsDBNull(reader.GetOrdinal("CancelacionWA")) ? null : reader.GetString(reader.GetOrdinal("CancelacionWA")),
                            DocCancelacion = reader.IsDBNull(reader.GetOrdinal("DocCancelacion")) ? null : reader.GetString(reader.GetOrdinal("DocCancelacion"))
                        };
                    }
                }
            }

            // Detach the tag entity if it is being tracked
            if (tag != null)
            {
                _context.Entry(tag).State = EntityState.Detached;
            }

            return tag;
        }


        public async Task<string> GetUltimaTagAsync()
        {
            string nuevaEtiqueta = null;

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "zsp_Sel_MaxTag";
                command.CommandType = CommandType.StoredProcedure;

                await _context.Database.OpenConnectionAsync().ConfigureAwait(false);

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    if (reader.Read())
                    {
                        nuevaEtiqueta = reader.GetString(reader.GetOrdinal("NuevaEtiqueta"));
                    }
                }
            }

            return nuevaEtiqueta;
        }


        public async Task<Tag> AgregarTagAsociado(TagRequest Tag)
        {
            Tag result = new Tag();

            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "zsp_Ins_TagAsociado";
                command.CommandType = CommandType.StoredProcedure;

                SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@IDSAE", Tag.IDSAE),
                        new SqlParameter("@Identificador", Tag.Identificador.Trim()),
                    };
                command.Parameters.AddRange(parameters);

                await _context.Database.OpenConnectionAsync().ConfigureAwait(false);

                await using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    if (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        result.IDSAE = Convert.ToInt32(reader["IDSAE"]);
                        result.Identificador = reader["IDENTIFICADOR"].ToString();
                    }
                }
            }

            // Detach the tag entity if it is being tracked
            if (result != null)
            {
                _context.Entry(result).State = EntityState.Detached;
            }

            return result;
        }



        public async Task<Tag> GetTagByIdAsync(int id)
        {
            return await _context.Tags.FindAsync(id);
        }

        public async Task UpdateTagAsync(Tag tag)
        {
            _context.Tags.Update(tag);
            await _context.SaveChangesAsync();
        }
    }
}
