using Grpc.Net.Client;
using GrpcService_01;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using WebApiAutores.DTOs.DTOBase;
using System.Net.Http;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json.Linq;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;
using NSwag.CodeGeneration.CodeGenerators.CSharp;

namespace WebApiAutores.Controllers.V1
{

    [ApiController] // -> para validaciones de los campos de los modelos
    //[Route("api/[controller]")] // [controller] referencia al nombre del controlador => api/Autores, reemplaza automaticamente
    [Route("api/V1/SOAP_gRPC_OpenAPI")]
    //[Authorize] // autoriazacion a nivel de controlador
    [ApiConventionType(typeof(DefaultApiConventions))] // para los mensaje undocumented
    public class SOAP_gRPC_OpenAPI : Controller
    {
        private readonly ILogger logger;
        private readonly IHttpClientFactory httpClientFactory;

        public SOAP_gRPC_OpenAPI(ILogger<SOAP_gRPC_OpenAPI> logger, IHttpClientFactory httpClientFactory)
        {
            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
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
        public  Task<ActionResult<List<AutorDTO>>> Grpc_Service()
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


        [HttpGet("openAPI", Name = "openAPI_service")]
        public async Task<ActionResult> OpenAPI_Service()
        {

            //Query string parameters
            var queryString = new Dictionary<string, string>()
                                      {
                                        { "Pagina", "1" },
                                        { "RecordsPorPagina", "6" }
                                      };

            var client2 = httpClientFactory.CreateClient();
            client2.BaseAddress = new Uri("https://localhost:7089/api/V1/autores");

            var requestUri = QueryHelpers.AddQueryString("autores", queryString);

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("incluirHATEOAS", "false");
            request.Headers.Add("Authorization", "bearer " + await GetToken());
            var resp = await client2.SendAsync(request);
           

            var responseBody = await resp.Content.ReadAsStringAsync();
            //var json = JsonConvert.SerializeObject(responseBody,Formatting.Indented);

            var json = JToken.Parse(responseBody);

            return Ok(json);
            

            
        }





        private async Task<string> GetToken()
        {

            var client3 = httpClientFactory.CreateClient();
            //client3.BaseAddress = new Uri("https://localhost:7089/api/V1/cuentas/login");

            //var requestUri = QueryHelpers.AddQueryString("autores", queryString);
            var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7089/api/V1/cuentas/login");
            request.Headers.Add("Accept", "application/json");

            request.Content = JsonContent.Create(new
            {
                email = "user0@gmail.com",
                password = "Aa.123456"
            });

            var resp = await client3.SendAsync(request);
            var a = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

            var json = JObject.Parse(a);

            var token = json.GetValue("token");

            string result = token.ToString();

            return result.Replace("\"", "");
        }


    }
}
