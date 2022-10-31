using Grpc.Net.Client;
using GrpcService_01;
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
        public SOAP_gRPC()
        {
        }

        [HttpGet("soap", Name = "soap_service")]
        public async Task<ActionResult<List<AutorDTO>>> SOAP_Service()
        {
            var ws = new Soap_Service_1.Service2Client();
            var ws2=new Soap_Service_1.AutorDTO();
            

            var result = await ws.ListAutoresAsync();

            return Ok(Json(result));
        }


        [HttpGet("grpc", Name = "grpc_service")]
        public async Task<ActionResult<List<AutorDTO>>> Grpc_Service()
        {

            //------------------------------------------------------
            //------------------------------------------------------
            // el cliente grpc podria inicializarce en el contructor de la clase para que este disponible a los demas metodos

            var url = "https://localhost:7209";
            var canal = GrpcChannel.ForAddress(url);

            var cliente = new AutoresGrpc.AutoresGrpcClient(canal);

            var resultado = cliente.Autores(new RequestModel());
            return Ok(resultado);
        }

        //[HttpGet("grpcgreeter", Name = "grpc_servicegreeter")]
        //public async Task<ActionResult> Grpc_ServiceGreeter()
        //{

        //    //------------------------------------------------------
        //    //------------------------------------------------------
        //    // el cliente grpc podria inicializarce en el contructor de la clase para que este disponible a los demas metodos

        //    var url = "https://localhost:7209";
        //    var canal = GrpcChannel.ForAddress(url);

        //    var cliente = new Greeter.GreeterClient(canal);

        //    var resultado = cliente.SayHello(new HelloRequest()
        //    {
        //        Name = "----------------------------------"
        //    });
        //    return Ok(resultado);
        //}


    }
}
