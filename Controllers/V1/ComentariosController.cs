using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebApiAutores.DTOs;
using WebApiAutores.DTOs.DTOBase;
using WebApiAutores.DTOs.DTOPaginacion;
using WebApiAutores.Entidades;
using WebApiAutores.Utilidades.Paginacion;

namespace WebApiAutores.Controllers.V1
{
    [ApiController]
    [Route("api/V1/libros/{libroId:int}/comentarios")]
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

        [HttpGet(Name = "obtenerComentariosLibros")]
        public async Task<ActionResult<List<ComentarioDTO>>> Get(int libroId, [FromQuery] PaginacionDTO paginacionDTO)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId);
            if (!existeLibro)
            {
                return NotFound();
            }
            var queryable = context.Comentarios.Where(comentarioDB => comentarioDB.LibroId == libroId).AsQueryable();
            await HttpContext.InsertarParametrosPaginacionEnCabecera(queryable);
            var comentarios = await queryable.OrderBy(comentario => comentario.Id).Paginar(paginacionDTO).ToListAsync();

            if (context.Comentarios.Where(comentarioDB => comentarioDB.LibroId == libroId).Count() == 0)
            {
                return NoContent();
            }

            return mapper.Map<List<ComentarioDTO>>(comentarios);

        }

        [HttpGet("{id:int}", Name = "obtenerComentario")]
        public async Task<ActionResult<ComentarioDTO>> GetById(int id)
        {
            var comentario = await context.Comentarios.FirstOrDefaultAsync(x => x.Id == id);

            if (comentario == null)
            {
                return NotFound("No existe un comentaio con ese id.");
            }

            return mapper.Map<ComentarioDTO>(comentario);

        }

        [HttpPost(Name = "crearComentario")]
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
            return CreatedAtRoute("obtenerComentario", new { id = comentario.Id, libroId }, comentarioDTO);

            //return Ok();
        }


        [HttpPut("{id:int}", Name = "actualizarComentario")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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

            var usuarioId = HttpContext.User.Claims.Where(x => x.Type == "UsuarioId").FirstOrDefault().Value;

            
            var comentario = mapper.Map<Comentario>(comentarioCreacionDTO);

            if (comentario.UsuarioId == usuarioId)
            {
                comentario.Id = id;
                comentario.LibroId = libroId;


                context.Update(comentario);
                await context.SaveChangesAsync();
                return Ok();
            }
            else
            {
                var a = new ForbidDTO() { Error = 403, Mensaje = "No le pertenece este comentario." };
                //var json = JsonConvert.SerializeObject(ForbidDTO);

                
                return new ObjectResult(a) { StatusCode = 403 };
            }


        }
    }
}
