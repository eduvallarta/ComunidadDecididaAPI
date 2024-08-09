using Comunidad_Decidida.Models;
using Comunidad_Decidida.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.Json;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using SkiaSharp;
using PdfSharp.Quality;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon;
using Amazon.Runtime;
using SixLabors.ImageSharp;

namespace Comunidad_Decidida.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AsociadoController : ControllerBase
    {
        private readonly IAmazonS3 _s3Client;
        private readonly AsociadoService _asociadoService;
        private readonly string _bucketName = "comunidad-decidida-cloud";
        private readonly RegionEndpoint _bucketRegion = RegionEndpoint.USWest2; // Cambia esto según tu región
        private readonly IConfiguration _configuration;

        public AsociadoController(IConfiguration configuration, AsociadoService asociadoService)
        {
            _asociadoService = asociadoService;

            // Inicializar el cliente S3 con las credenciales
            var awsSection = configuration.GetSection("AWS");
            var awsAccessKeyId = awsSection["AwsAccessKeyId"];
            var awsSecretAccessKey = awsSection["AwsSecretAccessKey"];

            var awsRegion = RegionEndpoint.USWest2; // Cambia esto según tu región

            _s3Client = new AmazonS3Client(awsAccessKeyId, awsSecretAccessKey, awsRegion);
            _asociadoService = asociadoService;
            _configuration = configuration;

        }

        [HttpGet("asociados-tags")]
        public async Task<ActionResult<IEnumerable<AsociadoTags>>> GetAsociadosWithTags()
        {
            var asociadosWithTags = await _asociadoService.GetAsociadosTagsAsync();
            return Ok(asociadosWithTags);
        }

        [HttpGet("ultima-tag")]
        public async Task<IActionResult> ObtenerUltimaTag()
        {
            var ultimaTag = await _asociadoService.GetUltimaTagAsync().ConfigureAwait(false);
            return Ok(ultimaTag);
        }

        [HttpPost("agregar-tag")]
        public async Task<IActionResult> Agregar([FromBody] TagRequest Tag)
        {
            if (Tag == null)
            {
                return BadRequest(ModelState);
            }

            var resultado = await _asociadoService.AgregarTagAsociado(Tag).ConfigureAwait(false);
            return Ok(resultado);
        }



        [HttpGet("get-cancelacion-wa")]
        public async Task<IActionResult> GetCancelacionWA([FromQuery] int id, [FromQuery] string etiqueta)
        {
            try
            {
                var tag = await _asociadoService.GetTagByAsociadoIdAndEtiquetaAsync(id, etiqueta);
                if (tag != null)
                {
                    var cancelacionPath = !string.IsNullOrEmpty(tag.CancelacionWA) ? tag.CancelacionWA : tag.DocCancelacion;
                    if (!string.IsNullOrEmpty(cancelacionPath))
                    {
                        var isImage = !string.IsNullOrEmpty(tag.CancelacionWA);
                        return Ok(new { path = cancelacionPath, isImage });
                    }
                }
                return NotFound("No resource found for the given ID and etiqueta.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPost("upload-cancelacion-wa")]
        public async Task<IActionResult> UploadCancelacionWA([FromQuery] int id, [FromQuery] string etiqueta, [FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // Define la ruta del archivo en S3
            // Define la ruta del archivo en S3 incluyendo el id y la etiqueta en el nombre del archivo
            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = $"Cancelacion_WhatsApp_{id}_{etiqueta}{fileExtension}";
            var s3Key = $"images/{fileName}";

            // Subir el archivo a S3
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);

                var putRequest = new PutObjectRequest
                {
                    BucketName = _bucketName, // Tu nombre de bucket S3
                    Key = s3Key,
                    InputStream = stream,
                    ContentType = file.ContentType
                };

                stream.Position = 0;
                await _s3Client.PutObjectAsync(putRequest);
            }

            // Construir la URL completa del archivo en S3
            var fileUrl = $"https://{_bucketName}.s3.{_bucketRegion.SystemName}.amazonaws.com/{s3Key}";

            await _asociadoService.SaveCancelacionWAPathAsync(id, fileUrl, etiqueta);

            return Ok(new { path = s3Key });
        }

        [HttpPost("upload-firma")]
        public async Task<IActionResult> UploadFirma([FromQuery] int id, [FromQuery] string etiqueta, [FromBody] JsonElement body)
        {
            try
            {
                // Deserializar el cuerpo de la solicitud
                var request = JsonSerializer.Deserialize<Signature>(body.GetRawText());

                // Verifica si el cuerpo de la solicitud contiene firmaBase64
                if (request == null || string.IsNullOrEmpty(request.FirmaBase64))
                {
                    return BadRequest("No firma uploaded.");
                }

                // Eliminar el prefijo de la cadena Base64 si existe
                var base64String = request.FirmaBase64;
                if (base64String.StartsWith("data:image/png;base64,"))
                {
                    base64String = base64String.Substring("data:image/png;base64,".Length);
                }

                // Limpiar y validar la cadena Base64
                base64String = CleanBase64String(base64String);

                // Debug: Mostrar la cadena Base64 después de la limpieza
                Console.WriteLine($"Base64 String After Cleaning: {base64String}");

                if (!IsBase64StringValid(base64String))
                {
                    return BadRequest("Invalid Base64 string format.");
                }

                // Convertir la cadena Base64 a un array de bytes
                byte[] firmaBytes = Convert.FromBase64String(base64String);

                // Usar SkiaSharp para guardar la imagen temporalmente
                var tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");
                using (var image = SKImage.FromEncodedData(firmaBytes))
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                using (var stream = System.IO.File.OpenWrite(tempFilePath))
                {
                    data.SaveTo(stream);
                }

                // Define la ruta del archivo PDF
                var fileName = $"Politica_Cancelacion_{id}_{etiqueta}.pdf";
                var s3Key = $"pdf/{fileName}";
                var path = Path.Combine("wwwroot/uploads", fileName);
                var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), path);

                // Crea el documento PDF y agrega la firma
                using (var document = new PdfDocument())
                {
                    var page = document.AddPage();
                    var gfx = XGraphics.FromPdfPage(page);
                    var fontHeader = new XFont("Verdana", 12, XFontStyle.Bold);
                    var fontTitle = new XFont("Verdana", 14, XFontStyle.Bold);
                    var fontText = new XFont("Verdana", 10);
                    var fontSignature = new XFont("Verdana", 14, XFontStyle.Bold);
                    var fontInfo = new XFont("Verdana", 10);

                    // Encabezado - imagen
                    var headerImagePath = Path.Combine(Directory.GetCurrentDirectory(), "Imagenes", "Encabezado.png"); // Ruta de la imagen del encabezado
                    using (var headerImageStream = new FileStream(headerImagePath, FileMode.Open, FileAccess.Read))
                    {
                        var headerImage = XImage.FromStream(() => headerImageStream);
                        gfx.DrawImage(headerImage, 40, 20, 500, 50); // Ajusta la posición y tamaño según necesites
                    }

                    // Información y documentación requerida
                    gfx.DrawString("INFORMACIÓN Y DOCUMENTACIÓN REQUERIDA", fontHeader, XBrushes.Black,
                        new XRect(40, 90, page.Width - 80, 30), XStringFormats.Center);
                    // Texto de información y documentación
                    var infoTextLines = new string[]
                    {
                        "1.-Debe de ser Asociado y estar al corriente del pago de mantenimiento.",
                        "2.-Copia de Identificación Oficial del propietario, y en su caso, también copia del arrendatario.",
                        "3.-Indicar el número de TAG o APP (Aplicación Movíl) a dar de baja.",
                        "4.-Firmar la política de cancelación."
                    };
                    int infoTextYPosition = 120;
                    foreach (var line in infoTextLines)
                    {
                        gfx.DrawString(line, fontText, XBrushes.Black,
                            new XRect(40, infoTextYPosition, page.Width - 80, 20), XStringFormats.TopLeft);
                        infoTextYPosition += 20;
                    }

                    // Título
                    gfx.DrawString("POLÍTICA DE CANCELACIÓN", fontTitle, XBrushes.Black,
                        new XRect(0, 190, page.Width, 50), XStringFormats.Center);

                    // Texto descriptivo centrado y justificado
                    var text = "Este documento explica la política de cancelación. Por favor, firma al final para confirmar la cancelación.";
                    var rect = new XRect(40, 240, page.Width - 80, 100);
                    gfx.DrawString(text, fontText, XBrushes.Black, rect, XStringFormats.TopLeft);


                    // Texto de firma
                    gfx.DrawString("Firma Propietario / Representante", fontSignature, XBrushes.Black,
                        new XRect(40, 620, 300, 20), XStringFormats.TopLeft);

                    gfx.DrawString("A T E N T A M E N T E", fontSignature, XBrushes.Black,
                        new XRect(40, 750, page.Width - 80, 20), XStringFormats.TopLeft);

                    gfx.DrawString("Administrador General y Consejo Directivo", fontSignature, XBrushes.Black,
                        new XRect(40, 770, page.Width - 80, 20), XStringFormats.TopLeft);

                    // Información adicional
                    gfx.DrawString("GESTIÓN 2024-2027", fontInfo, XBrushes.Black,
                        new XRect(40, 790, page.Width - 80, 20), XStringFormats.TopLeft);

                    gfx.DrawString("Informes y trámites administrativos: 55 76 99 86 20 // Whatsapp 55 37 85 09 54", fontInfo, XBrushes.Black,
                        new XRect(40, 810, page.Width - 80, 20), XStringFormats.TopLeft);

                    // Imagen de firma
                    using (var imageStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
                    {
                        var image = XImage.FromStream(() => imageStream);
                        gfx.DrawImage(image, 40, 640, 200, 100); // Ajusta la posición y tamaño según necesites
                    }

                    //document.Save(absolutePath);
                    // Guardar el documento en un MemoryStream
                    using (var stream = new MemoryStream())
                    {
                        document.Save(stream, false);

                        // Subir el archivo PDF a S3
                        var putRequest = new PutObjectRequest
                        {
                            BucketName = _bucketName, // Tu nombre de bucket S3
                            Key = s3Key,
                            InputStream = stream,
                            ContentType = "application/pdf"
                        };

                        stream.Position = 0;
                        await _s3Client.PutObjectAsync(putRequest);
                    }
                }

                // Eliminar el archivo temporal
                System.IO.File.Delete(tempFilePath);

                // Construir la URL completa del archivo en S3
                var fileUrl = $"https://{_bucketName}.s3.{_bucketRegion.SystemName}.amazonaws.com/{s3Key}";

                // Guarda la ruta relativa en la base de datos
                await _asociadoService.SaveCancelacionDocPDFPathAsync(id, fileUrl, etiqueta);

                return Ok(new { path });
            }
            catch (Exception ex)
            {
                // Registra el error para depuración
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Método para validar una cadena Base64
        private bool IsBase64StringValid(string base64)
        {
            if (string.IsNullOrEmpty(base64) || base64.Length % 4 != 0)
            {
                return false;
            }

            try
            {
                // Verificar si contiene caracteres válidos de Base64
                Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
                return Convert.TryFromBase64String(base64, buffer, out _);
            }
            catch (FormatException)
            {
                return false;
            }
        }


        // Método para limpiar una cadena Base64
        private string CleanBase64String(string base64)
        {
            return base64.Trim().Replace(" ", "").Replace("\n", "").Replace("\r", "").Replace("\t", "");
        }

    }
}
