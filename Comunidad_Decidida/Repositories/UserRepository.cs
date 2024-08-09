using System.Data.Common;
using System.Data;
using Comunidad_Decidida.DTO.Login;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Comunidad_Decidida.Infrastructure;


namespace Comunidad_Decidida.Repository
{
    /// <summary>
    /// Repositorio para acceder a los datos de Almacén en la base de datos.
    /// </summary>
    public class UserRepository
    {
        private readonly ApiDbContext _context;

        public UserRepository(ApiDbContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Obtiene una lista de objetos AlmacenDTO que representan los datos de almacén.
        /// </summary>
        /// <returns>Una lista de objetos AlmacenDTO.</returns>
        /// <created>Eduardo Alfonso Vallarta Zarate</created>
        public async Task<List<UsuarioDTO>> GetUsuario(string usuario, string password)
        {
            List<UsuarioDTO> result = new List<UsuarioDTO>();

            try
            {
                var connection = _context.Database.GetDbConnection();

                await using (var command = connection.CreateCommand())
                {
                    command.CommandText = "zsp_Sel_Users";
                    command.CommandType = CommandType.StoredProcedure;
                    // Agrega los parámetros directamente al comando
                    command.Parameters.Add(new SqlParameter("@USER", usuario));
                    command.Parameters.Add(new SqlParameter("@PASSWORD", password));

                    await connection.OpenAsync().ConfigureAwait(false);
                    await using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            UsuarioDTO item = new UsuarioDTO
                            {
                                IdUser = Convert.ToInt32(reader["IdUser"]),
                                FullName = reader["FullName"].ToString(),
                                UserName = reader["UserName"].ToString(),
                                IdRole = Convert.ToInt32(reader["IdRole"]),
                                Role = reader["Role"].ToString(),
                                Estatus = Convert.ToInt32(reader["Estatus"]),

                            };
                            result.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Considera manejar la excepción de manera más adecuada
                throw ex;
            }

            return result;
        }


        /// <summary>
        /// Obtiene el resultado de la validación de acuerdo al Role_Id del usuario.
        /// </summary>
        /// <returns>Un objeto tipo UsuarioDTO.</returns>
        /// <created>Eduardo Alfonso Vallarta Zarate</created>
        public async Task<List<UsuarioDTO>> GetUsuarioRole(string usuario)
        {
            List<UsuarioDTO> result = new List<UsuarioDTO>();

            try
            {
                var connection = _context.Database.GetDbConnection();

                await using (var command = connection.CreateCommand())
                {
                    command.CommandText = "zsp_Sel_Users_Role";
                    command.CommandType = CommandType.StoredProcedure;
                    // Agrega los parámetros directamente al comando
                    command.Parameters.Add(new SqlParameter("@USER", usuario));

                    await connection.OpenAsync().ConfigureAwait(false);
                    await using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            UsuarioDTO item = new UsuarioDTO
                            {
                                IdUser = Convert.ToInt32(reader["IdUser"]),
                                FullName = reader["FullName"].ToString(),
                                UserName = reader["UserName"].ToString(),
                                IdRole = Convert.ToInt32(reader["IdRole"]),
                                Role = reader["Role"].ToString(),
                                Estatus = Convert.ToInt32(reader["Estatus"]),

                            };
                            result.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Considera manejar la excepción de manera más adecuada
                throw ex;
            }

            return result;
        }
    }
}
