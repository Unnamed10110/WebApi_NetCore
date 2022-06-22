using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using WebApiAutores.DTOs;
using WebApiAutores.DTOs.DTOBase;
using WebApiAutores.DTOs.DTOHATEOAS;

namespace WebApiAutores.Servicios
{
    public class GeneradorEnlacesLibros
    {
        private readonly IAuthorizationService authorizationService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IActionContextAccessor actionContextAccessor;

        public GeneradorEnlacesLibros(IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor,
            IActionContextAccessor actionContextAccessor)
        {
            this.authorizationService = authorizationService;
            this.httpContextAccessor = httpContextAccessor;
            this.actionContextAccessor = actionContextAccessor;
        }

        private IUrlHelper ConstruirURLHelper()
        {
            var factoria = httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();
            return factoria.GetUrlHelper(actionContextAccessor.ActionContext);
        }

        private async Task<bool> EsAdmin()
        {
            var httpContext = httpContextAccessor.HttpContext;
            var resultado = await authorizationService.AuthorizeAsync(httpContext.User, "esAdmin");
            return resultado.Succeeded;
        }


        public async Task GenerarEnlacesLibro(LibroDTO libroDTO)
        {
            var esAdmin = await EsAdmin();
            var Url = ConstruirURLHelper();

            libroDTO.Enlaces.Add(new DatoHATEOAS( enlace: Url.Link("obtenerLibro", new { id = libroDTO.Id }), descripcion: "self", metodo: "GET"));

            if (esAdmin)
            {
                libroDTO.Enlaces.Add(new DatoHATEOAS( enlace: Url.Link("actualizarLibro", new { id = libroDTO.Id }), descripcion: "autor-actualizar", metodo: "PUT"));

                libroDTO.Enlaces.Add(new DatoHATEOAS( enlace: Url.Link("borrarLibro", new { id = libroDTO.Id }), descripcion: "self", metodo: "DELETE"));
            }


        }

    }
}
