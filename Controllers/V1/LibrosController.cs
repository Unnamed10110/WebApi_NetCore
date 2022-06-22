using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs.DTOBase;
using WebApiAutores.DTOs.DTOHATEOAS;
using WebApiAutores.Entidades;
using WebApiAutores.Utilidades.HATEOAS;

namespace WebApiAutores.Controllers.V1
{
    [ApiController]
    [Route("api/V1/libros")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "EsAdmin")]
    public class LibrosController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;
        private readonly IAuthorizationService authorizationService;

        public LibrosController(ApplicationDbContext context, IMapper mapper, IConfiguration configuration, IAuthorizationService authorizationService)
        {
            this.context = context;
            this.mapper = mapper;
            this.configuration = configuration;
            this.authorizationService = authorizationService;
        }

        [HttpGet("Todos", Name ="obtenerLibros")]
        [ServiceFilter(typeof(HATEOASLibroFilterAttribute))]
        [AllowAnonymous]
        public async Task<ActionResult<List<LibroDTO>>> Get([FromHeader] bool incluirHATEOAS = true)
        {
            // Include para que retorne el objeto relacionado
            //return await context.Libros.Include(x => x.Autor).FirstOrDefaultAsync(x => x.Id == id);

            //var libros = await context.Libros.Include(a => a.Comentarios).ToListAsync();
            //var dtos = mapper.Map<List<LibroDTO>>(libros);
            //if (incluirHATEOAS)
            //{
            //    var esAdmin = await authorizationService.AuthorizeAsync(User, "esAdmin");

            //    //dtos.ForEach(dto => GenerarEnlaces(dto, esAdmin.Succeeded));

            //    var resultado = new ColeccionDeRecursos<LibroDTO> { Valores = dtos };


            //    resultado.Enlaces.Add(new DatoHATEOAS(enlace: Url.Link("obtenerLibros", new { }), descripcion: "self", metodo: "GET"));

            //    if (esAdmin.Succeeded)
            //        resultado.Enlaces.Add(new DatoHATEOAS(enlace: Url.Link("crearLibro", new { }), descripcion: "libro-crear", metodo: "POST"));

            //    return Ok(resultado);
            //}

            //return Ok(dtos);



            var libros = await context.Libros.Include(a => a.Comentarios).ToListAsync();

            var libroReturn = mapper.Map<List<LibroDTO>>(libros);
            return libroReturn;
        }

        [HttpGet("{id:int}", Name = "obtenerLibro")]
        public async Task<ActionResult<LibroDTOConAutores>> Get([FromRoute] int id)
        {
            // Include para que retorne el objeto relacionado
            //return await context.Libros.Include(x => x.Autor).FirstOrDefaultAsync(x => x.Id == id);

            var libro = await context.Libros
                .Include(libroDB => libroDB.AutoresLibros)
                .ThenInclude(autorLibroDB => autorLibroDB.Autor)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (libro == null)
            {
                return NotFound();
            }

            libro.AutoresLibros = libro.AutoresLibros.OrderBy(x => x.Orden).ToList();

            return mapper.Map<LibroDTOConAutores>(libro);
        }

        [HttpPost(Name = "crearLibro")]
        public async Task<ActionResult> Post([FromBody] LibroCreacionDTO libroCreacionDTO)
        {
            //var existeAutor = await context.Autores.AnyAsync(x => x.Id == libro.AutorId);
            //if (!existeAutor)
            //{
            //    return BadRequest($"No existe el autor de Id : {libro.AutorId}");
            //}
            if (libroCreacionDTO.AutoresIds == null)
            {
                return BadRequest("No se puede crear un libro sin autores.");
            }

            var autoresIds = await context.Autores.Where(x => libroCreacionDTO.AutoresIds.Contains(x.Id)).Select(a => a.Id).ToListAsync();

            if (autoresIds.Count != libroCreacionDTO.AutoresIds.Count)
            {
                return BadRequest("Uno o más de los autores enviados no existen.");
            }

            var libro = mapper.Map<Libro>(libroCreacionDTO);

            AsignarOrdenDeAutores(libro);

            context.Add(libro);
            await context.SaveChangesAsync();

            var libroDTOAux = mapper.Map<LibroDTO>(libro);

            return CreatedAtRoute("obtenerLibro", new { id = libro.Id }, libroDTOAux);

            //return Ok();
        }

        [HttpPut("{id:int}", Name = "actualizarLibro")]
        public async Task<ActionResult> Put([FromRoute] int id, [FromBody] LibroCreacionDTO libroCreacionDTO)
        {
            var libroDB = await context.Libros.Include(x => x.AutoresLibros).FirstOrDefaultAsync(x => x.Id == id);

            if (libroDB == null)
            {
                return NotFound("No se encontró el libro con el id indicado.");
            }

            libroDB = mapper.Map(libroCreacionDTO, libroDB);

            AsignarOrdenDeAutores(libroDB);

            await context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("PutParcial/{id:int}")]
        public async Task<ActionResult> PutParcial([FromRoute] int id, [FromBody] LibroPutParcialDTO libroPutParcialDTO)
        {
            var libroDB = await context.Libros.Include(x => x.AutoresLibros).FirstOrDefaultAsync(x => x.Id == id);

            if (libroDB == null)
            {
                return NotFound("No se encontró el libro con el id indicado.");
            }

            var auxlibro = libroDB;
            libroDB = mapper.Map(libroPutParcialDTO, libroDB);

            libroDB.AutoresLibros = auxlibro.AutoresLibros;
            libroDB.FechaPublicacion = auxlibro.FechaPublicacion;

            AsignarOrdenDeAutores(libroDB);

            await context.SaveChangesAsync();

            return Ok();
        }

        [HttpPatch("{id:int}", Name = "patchLibro")]
        public async Task<ActionResult> Patch(int id, JsonPatchDocument<LibroPatchDTO> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var libroDB = await context.Libros.FirstOrDefaultAsync(x => x.Id == id);
            if (libroDB == null)
            {
                return NotFound();
            }
            var libroDTO = mapper.Map<LibroPatchDTO>(libroDB);
            patchDocument.ApplyTo(libroDTO, ModelState);

            var esvalido = TryValidateModel(libroDTO);
            if (!esvalido)
            {
                return BadRequest(ModelState);
            }
            mapper.Map(libroDTO, libroDB);
            await context.SaveChangesAsync();
            return NoContent();

        }

        [HttpDelete("{id:int}", Name = "borrarLibro")]
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Libros.AnyAsync(x => x.Id == id);
            if (!existe)
            {
                return NotFound();
            }

            context.Remove(new Libro() { Id = id });
            await context.SaveChangesAsync();
            return Ok();


        }


        private static void AsignarOrdenDeAutores(Libro libro)
        {
            if (libro.AutoresLibros != null)
            {
                for (int i = 0; i < libro.AutoresLibros.Count; i++)
                {
                    libro.AutoresLibros[i].Orden = i;
                }
            }
        }


    }
}