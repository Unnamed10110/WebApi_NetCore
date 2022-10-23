using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using WebApiAutores.DTOs.DTOBase;

namespace WebApiAutores.Controllers.V1
{

    [ApiController] // -> para validaciones de los campos de los modelos
    //[Route("api/[controller]")] // [controller] referencia al nombre del controlador => api/Autores, reemplaza automaticamente
    [Route("api/V1/SOAP_gRPC")]
    //[Authorize] // autoriazacion a nivel de controlador
    [ApiConventionType(typeof(DefaultApiConventions))] // para los mensaje undocumented
    public class SOAP_gRPC : Controller
    {
        [HttpGet("soap", Name = "soap_service")]
        public async Task<ActionResult<List<AutorDTO>>> SOAP_Service()
        {
            var ws = new Soap_Service_1.Service2Client();
            var ws2=new Soap_Service_1.AutorDTO();
            

            var result = await ws.ListAutoresAsync();

            return Ok(Json(result));
        }
    }
}
