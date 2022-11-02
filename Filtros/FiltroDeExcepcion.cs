using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using WebApiAutores.Utilidades.CustomExceptions;

namespace WebApiAutores.Filtros
{
    public class FiltroDeExcepcion:ExceptionFilterAttribute
    {
        private readonly ILogger<FiltroDeExcepcion> logger;
        private readonly IWebHostEnvironment env;
        private readonly IModelMetadataProvider modelMetadataProvider; // obtener datos de la solicitud

        public FiltroDeExcepcion(ILogger<FiltroDeExcepcion> logger, IWebHostEnvironment env, IModelMetadataProvider modelMetadataProvider)
        {
            this.logger = logger;
            this.env = env;
            this.modelMetadataProvider = modelMetadataProvider;
        }

        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is MyException)
            {
                context.HttpContext.Response.StatusCode = 500;
                context.Result = new JsonResult($"Error capturado con el filtro de exception, Retornando mensaje sin trace de error en la respuesta [" +
                $" Exception type: {context.Exception.GetType()}]");
            }
            
            logger.LogError(context.Exception, context.Exception.Message+ " Fallo capturado en filtro de exception +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            base.OnException(context);
            
        }
    }
}
