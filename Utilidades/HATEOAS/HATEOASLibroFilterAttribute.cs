using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebApiAutores.DTOs.DTOBase;
using WebApiAutores.Servicios;

namespace WebApiAutores.Utilidades.HATEOAS
{
    internal class HATEOASLibroFilterAttribute:HATEOASFilterAttribute
    {
        private readonly GeneradorEnlacesLibros generadorEnlacesLibro;

        public HATEOASLibroFilterAttribute(GeneradorEnlacesLibros generadorEnlacesLibro)
        {
            this.generadorEnlacesLibro = generadorEnlacesLibro;
        }

        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var debeIncluir = DebeIncluirHATEOAS(context);

            if (!debeIncluir)
            {
                await next();
                return;
            }

            var resultado = context.Result as ObjectResult;

            var libroDTO = resultado.Value as LibroDTO;
            if (libroDTO == null)
            {
                var librosDTO = resultado.Value as List<LibroDTO> ??
                    throw new ArgumentException("Se esperaba una instancia de LibroDTO o List<LibroDTO>");

                librosDTO.ForEach(async libro => await generadorEnlacesLibro.GenerarEnlacesLibro(libro));
                resultado.Value = librosDTO;
            }
            else
            {
                await generadorEnlacesLibro.GenerarEnlacesLibro(libroDTO);
            }

            await next();
        }
    }
}