using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs.DTOBase;
using WebApiAutores.DTOs.DTOPaginacion;
using WebApiAutores.Entidades;
using WebApiAutores.Utilidades.Paginacion;

namespace WebApiAutores.Controllers.V2
{
    [ApiController]
    [Route("api/V2/libros/{libroId:int}/comentarios")]
    public class ComentariosController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly UserManager<IdentityUser> userManager;

        public ComentariosController(ApplicationDbContext context, IMapper mapper, UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.mapper = mapper;
            this.userManager = userManager;
        }

        [HttpGet(Name = "obtenerComentariosLibrosv2")]
        public async Task<ActionResult<List<ComentarioDTO>>> Get(int libroId, [FromQuery] PaginacionDTO paginacionDTO)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId);
            if (!existeLibro)
            {
                return NotFound();
            }
            var queryable = context.Comentarios.Where(comentarioDB => comentarioDB.LibroId == libroId).AsQueryable();
            //await HttpContext.InsertarParametrosPaginacionEnCabecera(queryable);


            double cantidad = await queryable.CountAsync();
            HttpContext.Response.Headers.Add("cantidadTotalRegistros", cantidad.ToString());

            //var comentarios = await queryable.OrderBy(comentario => comentario.Id).Paginar(paginacionDTO).ToListAsync();

            var comentarios= queryable.Skip((paginacionDTO.Pagina - 1) * paginacionDTO.RecordsPorPagina).Take(paginacionDTO.RecordsPorPagina);

            if (context.Comentarios.Where(comentarioDB => comentarioDB.LibroId == libroId).Count() == 0)
            {
                return NoContent();
            }

            return mapper.Map<List<ComentarioDTO>>(comentarios);

        }

        [HttpGet("{id:int}", Name = "obtenerComentariov2")]
        public async Task<ActionResult<ComentarioDTO>> GetById(int id)
        {
            var comentario = await context.Comentarios.FirstOrDefaultAsync(x => x.Id == id);

            if (comentario == null)
            {
                return NotFound("No existe un comentaio con ese id.");
            }

            return mapper.Map<ComentarioDTO>(comentario);

        }

        [HttpPost(Name = "crearComentariov2")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Post(int libroId, ComentarioCreacionDTO comentarioCrearDTO)
        {
            //--------------------------------------------
            // esto solo se puede hacer bajo un authorize
            var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type.Contains("emailaddres")).FirstOrDefault(); // se debe limpiar el mapeo de los claims en startup para que no retorne null
            var email = emailClaim.Value;

            var usuario = await userManager.FindByEmailAsync(email);
            var usuarioId = usuario.Id;

            //--------------------------------------------
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId);
            if (!existeLibro)
            {
                return NotFound();
            }
            var comentario = mapper.Map<Comentario>(comentarioCrearDTO);
            comentario.LibroId = libroId;

            comentario.UsuarioId = usuarioId;

            context.Add(comentario);
            await context.SaveChangesAsync();

            var comentarioDTO = mapper.Map<ComentarioDTO>(comentario);
            return CreatedAtRoute("obtenerComentariov2", new { id = comentario.Id, libroId }, comentarioDTO);

            //return Ok();
        }


        [HttpPut("{id:int}", Name = "actualizarComentariov2")]
        public async Task<ActionResult> Put(int libroId, int id, ComentarioCreacionDTO comentarioCreacionDTO)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId);
            if (!existeLibro)
            {
                return NotFound();
            }

            var existeComentario = await context.Comentarios.AnyAsync(x => x.Id == id);

            if (!existeComentario)
            {
                return NotFound("No existe el comentario con ese id.");
            }


            var comentario = mapper.Map<Comentario>(comentarioCreacionDTO);
            comentario.Id = id;
            comentario.LibroId = libroId;

            context.Update(comentario);
            await context.SaveChangesAsync();
            return Ok();

        }
    }
}
