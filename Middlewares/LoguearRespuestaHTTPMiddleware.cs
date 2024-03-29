﻿using Newtonsoft.Json.Linq;

namespace WebApiAutores.Middlewares
{
    //clase estática para no exponer la clase del middleware en startup
    public static class LoguearRespuestaHTTPMiddlewareExtensions
    {   
        public static IApplicationBuilder UseLoguearRespuestaHTTP(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LoguearRespuestaHTTPMiddleware>();
        }
    }

    public class LoguearRespuestaHTTPMiddleware
    {
        private readonly RequestDelegate siguiente;
        private readonly ILogger<LoguearRespuestaHTTPMiddleware> logger;

        public LoguearRespuestaHTTPMiddleware(RequestDelegate siguiente, ILogger<LoguearRespuestaHTTPMiddleware> logger)
        {
            this.siguiente = siguiente;
            this.logger = logger;
        }
        // invoke o invokeAsync
        public async Task InvokeAsync(HttpContext contexto)
        {  
            
            // guarda en logs todas las repuestas de cada controlador
            using (var ms = new MemoryStream())
            {
                
                var cuerpoOriginalRespuesta = contexto.Response.Body;
                contexto.Response.Body = ms;
                // before middleware
                await siguiente(contexto);//+++++++++++++++++++++++++++++++++
                // after middleware

                ms.Seek(0, SeekOrigin.Begin);
                string respuesta = new StreamReader(ms).ReadToEnd();
                ms.Seek(0, SeekOrigin.Begin);

                await ms.CopyToAsync(cuerpoOriginalRespuesta);
                contexto.Response.Body = cuerpoOriginalRespuesta;
                
                //var res=JObject.Parse(respuesta);
                if(!contexto.Request.Path.Value.ToString().Contains("/swagger/"))
                {
                    logger.LogInformation(respuesta + "\n -------->> Log from LoguearRespuestaHTTP Middleware! <<--------");
                }
                
            }
        }
    }
}
