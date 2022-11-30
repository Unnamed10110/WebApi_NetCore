using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs.DTOBase;
using WebApiAutores.DTOs.DTOPaginacion;
using WebApiAutores.Entidades;
using WebApiAutores.Utilidades.Paginacion;
using HotChocolate;

namespace WebApiAutores.GraphQL
{
    public class AutoresType : ObjectType<Autor>
    {
        public AutoresType()
        {

        }
    }

    public interface IAutoresProvider
    {
        Task<List<AutorDTO>> GetAutores();
    }

    public class AutoresProvider : ControllerBase, IAutoresProvider
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IHttpContextAccessor httpContextAccessor;

        public AutoresProvider(ApplicationDbContext context, IMapper mapper,IHttpContextAccessor httpContextAccessor)
        {
            this.context = context;
            this.mapper = mapper;
            this.httpContextAccessor = httpContextAccessor;
        }
        
        public async Task<List<AutorDTO>> GetAutores()
        {
            // get the header for pagination
            var pagina = httpContextAccessor.HttpContext.Request.Headers["pagina"];
            var cantidad1 = httpContextAccessor.HttpContext.Request.Headers["cantidad"];

            var pag = new PaginacionDTO()
            {
                Pagina = Int32.Parse(pagina),
                RecordsPorPagina = Int32.Parse(cantidad1)
            };

            var queryable = context.Autores.AsQueryable();

            

            var autores2 = queryable.Skip((pag.Pagina - 1) * pag.RecordsPorPagina).Take(pag.RecordsPorPagina);

            
            return mapper.Map<List<AutorDTO>>( await autores2.ToListAsync());



            
        }
        
    }

    

    [ExtendObjectType("Queries")]
    public class AutoresQuery
    {
        private readonly IAutoresProvider autoresProvider;

        public AutoresQuery(IAutoresProvider autoresProvider)
        {
            this.autoresProvider = autoresProvider;
        }


        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public async Task<List<AutorDTO>> Autores()
        {

            return  await autoresProvider.GetAutores();

        }
    }

}
