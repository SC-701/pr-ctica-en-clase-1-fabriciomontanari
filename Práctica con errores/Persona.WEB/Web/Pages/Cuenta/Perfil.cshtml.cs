using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace Web.Pages.Cuenta
{
    public class PerfilModel : PageModel
    {
        private IConfiguracion _configuracion;
        private IWebHostEnvironment _environment;
        [BindProperty]
        public PerfilRequest perfil { get; set; } = default!;
        [BindProperty]
        public IFormFile foto { get; set; }
        [BindProperty]
        public IFormFile curriculum { get; set; }
        [BindProperty]
        public HttpStatusCode envioEstado { get; set; }
        public PerfilModel(IConfiguracion configuracion, IWebHostEnvironment environment)
        {
            _configuracion = configuracion;
            _environment = environment;
        }
        public async Task OnGetAsync()
        {
            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerPerfil");
            var cliente = new HttpClient();
            var IdUsuario = HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
            cliente.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", HttpContext.User.Claims.Where(c => c.Type == "Token").FirstOrDefault().Value);
            var solicitud = new HttpRequestMessage(HttpMethod.Get, string.Format(endpoint, IdUsuario));
            var respuesta = await cliente.SendAsync(solicitud);           
            if (respuesta.StatusCode == HttpStatusCode.OK)
            {
                var resultado = await respuesta.Content.ReadAsStringAsync();
                var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                perfil = JsonSerializer.Deserialize<PerfilRequest>(resultado, opciones);
            }
        }
        public async Task<DocumentoContenido> obtenerDocumentoAsync(IFormFile archivo)
        {
            var file = Path.Combine(_environment.WebRootPath, "Documentos", archivo.FileName);
            using (var fileStream = new FileStream(file, FileMode.Create))
            {
                await archivo.CopyToAsync(fileStream);
            }
            byte[] contenido = System.IO.File.ReadAllBytes(file);
            System.IO.File.Delete(file);
            DocumentoContenido documento = new DocumentoContenido() { Nombre = archivo.FileName, Contenido = contenido };
            return documento;
        }


    }
}
