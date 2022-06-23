using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebApiAutores.DTOs.DTOBase;
using WebApiAutores.Servicios;

namespace WebApiAutores.Utilidades.HATEOAS
{
    public class HATEOASAutorFilterAttribute : HATEOASFilterAttribute
    {
        private readonly GeneradorEnlaces generadorEnlaces;

        public HATEOASAutorFilterAttribute(GeneradorEnlaces generadorEnlaces)
        {
            this.generadorEnlaces = generadorEnlaces;
        }

        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            string ver = "";
            var cc = context.HttpContext.Request.Path.Value.Split("/").Contains("V1");
            if(cc)
            {
                ver = "v1";
            }
            else
            {
                ver = "v2";
            }

            var debeIncluir = DebeIncluirHATEOAS(context);

            if (!debeIncluir)
            {
                await next();
                return;
            }
            
            var resultado = context.Result as ObjectResult;

            var autorDTO = resultado.Value as AutorDTO;
            if (autorDTO == null)
            {
                var autoresDTO = resultado.Value as List<AutorDTO> ??
                    throw new ArgumentException("Se esperaba una instancia de AutorDTO o List<AutorDTO>");

                autoresDTO.ForEach(async autor => await generadorEnlaces.GenerarEnlaces(autor,ver));
                resultado.Value = autoresDTO;
            }
            else
            {
                await generadorEnlaces.GenerarEnlaces(autorDTO,ver);
            }

            await next();
        }
    }
}
