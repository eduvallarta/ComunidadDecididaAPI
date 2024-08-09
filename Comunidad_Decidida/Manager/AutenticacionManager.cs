using Comunidad_Decidida.EDM.Models.Login;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Comunidad_Decidida.DTO.Login;
using Comunidad_Decidida.Repository;
using System.Security.Cryptography;

namespace Comunidad_Decidida.Manager
{
    /// <summary>
    /// Gestiona la autenticación y operaciones relacionadas para usuarios.
    /// </summary>
    public class AutenticacionManager
    {

        #region " Members "
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        string token = "";
        private readonly UserRepository _userRepository;

        #endregion

        #region " Constructor "

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="AutenticacionManager"/>.
        /// </summary>
        /// <param name="httpClient">Cliente HTTP para realizar solicitudes.</param>
        /// <param name="configuration">Configuración de la aplicación.</param>
        /// <created>Eduardo Alfonso Vallarta Zarate</created>

        public AutenticacionManager(HttpClient httpClient, IConfiguration configuration, UserRepository userRepository)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _userRepository = userRepository;
        }
        #endregion

        #region " Functions "

        /// <summary>
        /// Autentica a un usuario y devuelve un token si la autenticación es exitosa.
        /// </summary>
        /// <param name="request">Datos de inicio de sesión del usuario.</param>
        /// <returns>Un string con el token si la autenticación es exitosa, de lo contrario string.Empty.</returns>
        /// <created>Eduardo Alfonso Vallarta Zarate</created>
        public async Task<string> Authentication(LoginRequest request)
        {
            try
            {
                return await AutenticarAsync(request).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Consider logging the exception
                // Log the exception (e.g., _logger.LogError(ex, "Error in Authentication"));
                return string.Empty;
            }
        }

        /// <summary>
        /// Realiza la autenticación con la API de manera asincrónica.
        /// </summary>
        /// <param name="request">Datos de inicio de sesión del usuario.</param>
        /// <returns>Token de autenticación si es exitoso.</returns>
        /// <created>Eduardo Alfonso Vallarta Zarate</created>
        private async Task<string> AutenticarAsync(LoginRequest request)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("servicio/autenticar", request).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var tokenAux = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return tokenAux; // Assuming the response content is just a token string
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Realiza el login del usuario y si tiene permisos para la apliacion que lo solicita
        /// </summary>
        /// <param name="usuario">Un objeto de tipo UsuarioAppRequest</param>
        /// <returns>Un objeto de tipo AuthenticationRequest</returns>
        /// <created>Eduardo Alfonso Vallarta Zarate</created>
        public async Task<AutenticacionResponse> LoginAndApp(UsuarioAppRequest usuario)
        {
            AutenticacionResponse result = new AutenticacionResponse();
            try
            {
                string newPassword = await EncryptPassword(usuario.Password).ConfigureAwait(false);

                List<UsuarioDTO> resultRepo = await _userRepository.GetUsuario(usuario.User, newPassword).ConfigureAwait(false);
                if (resultRepo.Count > 0 )
                {
                    result.Success = true;

                }
                else
                {

                    result.Success = false;
                    result.Razon = "Contraseña incorrecta.";

                    //result.Message = "No hay información por mostrar";
                    //result.Success = false;
                    //result.TypeError = VentaEmpleadoEDM.Enums.TipoError.Datos;
                }
            }
            catch (Exception ex)
            {
                result.Exception = "Error en la API.";
                //result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }

        /// <summary>
        /// Obtiene la validación del usuario administrador.
        /// </summary>
        /// <returns>Una tarea que resulta en un objeto <see cref="ReturnDataRequestEn{List{UserRequest}}"/>,
        /// validando la contraseña.</returns>
        /// <created>Eduardo Alfonso Vallarta Zarate</created>
        private async Task<AutenticacionResponse> GetUser(string usuario, string password)
        {
            AutenticacionResponse result = new AutenticacionResponse();
            try
            {
                List<UsuarioDTO> resultRepo = await _userRepository.GetUsuario(usuario, password).ConfigureAwait(false);
                if (resultRepo.Count > 0)
                {
                    result.Success = true;

                }
                else
                {

                    result.Success = false;
                    result.Exception = "Error en la API.";

                    //result.Message = "No hay información por mostrar";
                    //result.Success = false;
                    //result.TypeError = VentaEmpleadoEDM.Enums.TipoError.Datos;
                }
            }
            catch (Exception ex)
            {
                result.Exception = "Error en la API.";
                //result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }

        /// <summary>
        /// Obtiene el roleId del usuario.
        /// </summary>
        /// <returns>Una tarea que resulta en un objeto <see cref="ReturnDataRequestEn{List{UserRequest}}"/>,
        /// validando la contraseña.</returns>
        /// <created>Eduardo Alfonso Vallarta Zarate</created>
        private async Task<AutenticacionResponse> GetUserRole(string usuario)
        {
            AutenticacionResponse result = new AutenticacionResponse();
            try
            {
                List<UsuarioDTO> resultRepo = await _userRepository.GetUsuarioRole(usuario).ConfigureAwait(false);
                if (resultRepo.Count > 0)
                {
                    if (resultRepo.Count > 0 && resultRepo[0].Estatus == 0)
                    {
                        result.Success = true;

                    }
                    else
                    {
                        if (resultRepo[0].Estatus != 0)
                        {
                            result.Success = false;
                            result.Razon = "El usuario esta inactivo.";
                            return result;
                        }

                        result.Success = false;
                        result.Exception = "Error en la API.";

                    }
                }
                else
                {
                    result.Success = true;
                }
            }
            catch (Exception ex)
            {
                result.Exception = "Error en la API.";
                //result.Message = ex.Message;
                result.Success = false;
            }
            return result;
        }


        /// <summary>
        /// Encripta la contraseña
        /// </summary>
        /// <param name="pass"></param>
        /// <returns></returns>
        /// <created>Eduardo Vallarta</created>
        public async Task<string> EncryptPassword(string password)
        {
            string result = string.Empty;
            try
            {
                //Claves y vectores manuales          
                byte[] key = UTF8Encoding.UTF8.GetBytes("ControlTags");
                byte[] iv = UTF8Encoding.UTF8.GetBytes("COMUNIDADDECIDIDA");

                int keySize = 32;
                int ivSize = 16;
                Array.Resize(ref key, keySize);
                Array.Resize(ref iv, ivSize);
                result = await EncryptString(password, key, iv).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// Desencripta la contraseña
        /// </summary>
        /// <param name="pass"></param>
        /// <returns></returns>
        /// <created>Eduardo Vallarta</created>
        public async Task<string> DecryptPassword(string pass)
        {
            string result = string.Empty;
            try
            {
                //Claves y vectores manuales          
                byte[] key = UTF8Encoding.UTF8.GetBytes("ControlTags");
                byte[] iv = UTF8Encoding.UTF8.GetBytes("COMUNIDADDECIDIDA");
                int keySize = 32;
                int ivSize = 16;
                Array.Resize(ref key, keySize);
                Array.Resize(ref iv, ivSize);
                result = await DecryptString(pass, key, iv).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            await Task.Delay(0).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Encripta el string final
        /// </summary>
        /// <param name="encryptedMessage"></param>
        /// <param name="Key"></param>
        /// <param name="IV"></param>
        /// <returns></returns>
        /// <created>Eduardo Vallarta</created>
        private async Task<string> EncryptString(String plainMessage, byte[] Key, byte[] IV)
        {
            // Crear una instancia del algoritmo de Rijndael
            Rijndael RijndaelAlg = Rijndael.Create();
            // Establecer un flujo en memoria para el cifrado
            MemoryStream memoryStream = new MemoryStream();
            // Crear un flujo de cifrado basado en el flujo de los datos
            CryptoStream cryptoStream = new CryptoStream(memoryStream,
                RijndaelAlg.CreateEncryptor(Key, IV),
                CryptoStreamMode.Write);
            // Obtener la representación en bytes de la información a cifrar
            byte[] plainMessageBytes = UTF8Encoding.UTF8.GetBytes(plainMessage);
            // Cifrar los datos enviándolos al flujo de cifrado
            cryptoStream.Write(plainMessageBytes, 0, plainMessageBytes.Length);
            cryptoStream.FlushFinalBlock();
            // Obtener los datos datos cifrados como un arreglo de bytes
            byte[] cipherMessageBytes = memoryStream.ToArray();
            // Cerrar los flujos utilizados
            memoryStream.Close();
            cryptoStream.Close();
            await Task.Delay(0).ConfigureAwait(false);
            // Retornar la representación de texto de los datos cifrados
            return Convert.ToBase64String(cipherMessageBytes);

        }

        /// <summary>
        /// Desencripta el string final
        /// </summary>
        /// <param name="encryptedMessage"></param>
        /// <param name="Key"></param>
        /// <param name="IV"></param>
        /// <returns></returns>
        /// <created>Eduardo Vallarta</created>
        private async Task<string> DecryptString(String encryptedMessage, byte[] Key, byte[] IV)
        {
            // Obtener la representación en bytes del texto cifrado
            byte[] cipherTextBytes = Convert.FromBase64String(encryptedMessage);
            // Crear un arreglo de bytes para almacenar los datos descifrados
            byte[] plainTextBytes = new byte[cipherTextBytes.Length];
            // Crear una instancia del algoritmo de Rijndael
            Rijndael RijndaelAlg = Rijndael.Create();
            // Crear un flujo en memoria con la representación de bytes de la información cifrada
            MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
            // Crear un flujo de descifrado basado en el flujo de los datos
            CryptoStream cryptoStream = new CryptoStream(memoryStream,
                RijndaelAlg.CreateDecryptor(Key, IV),
                CryptoStreamMode.Read);
            // Obtener los datos descifrados obteniéndolos del flujo de descifrado
            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            // Cerrar los flujos utilizados
            memoryStream.Close();
            cryptoStream.Close();
            await Task.Delay(0).ConfigureAwait(false);
            // Retornar la representación de texto de los datos descifrados
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);

        }

        #endregion
    }
}
