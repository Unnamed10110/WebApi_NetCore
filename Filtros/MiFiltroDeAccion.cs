﻿using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApiAutores.Filtros
{
    public class MiFiltroDeAccion : IActionFilter
    {
        private readonly ILogger<MiFiltroDeAccion> logger;
        

        public MiFiltroDeAccion(ILogger<MiFiltroDeAccion> logger)
        {
            this.logger = logger;
            
        }
        public void OnActionExecuting(ActionExecutingContext context)
        {
            logger.LogInformation("Antes de ejecutar la accion");
            
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {

            logger.LogInformation("Despues de ejecutar la accion");
            if (context.HttpContext.Request.Method.ToString() == "GET")
            {
                context.HttpContext.Response.Headers.Add("HeaderFromFilter", "Hello");
            }
        }

        
    }
}
