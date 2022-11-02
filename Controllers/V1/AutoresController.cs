using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.Entidades;
using WebApiAutores.Filtros;
using WebApiAutores.Servicios;
using System.Linq;
using Npgsql;
using System.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using WebApiAutores.Utilidades;
using WebApiAutores.DTOs.DTOBase;
using WebApiAutores.Utilidades.HATEOAS;
using WebApiAutores.Utilidades.Paginacion;
using WebApiAutores.DTOs.DTOPaginacion;
using WebApiAutores.Utilidades.HEADERS;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using WebApiAutores.Utilidades.CustomExceptions;
using Newtonsoft.Json;

// mensajes de error-> Tipos:
// Critical - Error - Warning - Information - Debug - Trace
namespace WebApiAutores.Controllers.V1
{
    [ApiController] // -> para validaciones de los campos de los modelos
    //[Route("api/[controller]")] // [controller] referencia al nombre del controlador => api/Autores, reemplaza automaticamente
    [Route("api/V1/autores")]
    //[Route("api/autores")]
    //[CabeceraEstaPresente(cabecera:"x-version", valor:"1")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "EsAdmin")]
    //[Authorize] // autoriazacion a nivel de controlador
    [ApiConventionType(typeof(DefaultApiConventions))] // para los mensaje undocumented
    public class AutoresController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;
        private readonly IAuthorizationService authorizationService;
        private readonly IWebHostEnvironment env;
        private readonly AlmacenadorArchivosLocal almacenadorArchivosLocal;
        private readonly ServicioTransient servicioTransient;
        private readonly ServicioScoped servicioScoped;
        private readonly ServicioSingleton servicioSingleton;
        private readonly IServicio servicio;
        private readonly string contenedor = "autores";

        // inyeccion de dependencias
        public AutoresController
        (   ApplicationDbContext context, 
            IMapper mapper, 
            IConfiguration configuration,
            IAuthorizationService authorizationService, 
            IWebHostEnvironment env, 
            AlmacenadorArchivosLocal almacenadorArchivosLocal, 
            ServicioTransient servicioTransient,
            ServicioScoped servicioScoped, 
            ServicioSingleton servicioSingleton, 
            IServicio servicio
            )
        {
            this.context = context;
            this.mapper = mapper;
            this.configuration = configuration;
            this.authorizationService = authorizationService;// comment
            this.env = env;
            this.almacenadorArchivosLocal = almacenadorArchivosLocal;
            this.servicioTransient = servicioTransient;
            this.servicioScoped = servicioScoped;
            this.servicioSingleton = servicioSingleton;
            this.servicio = servicio;
        }

        [HttpGet("MyException")]
        [AllowAnonymous]
        public ActionResult GetExcepcion()
        {
            throw new MyException("Error forzado para probar exception personalizada y filtro");
        }

        [AllowAnonymous]
        [HttpGet("variosids")]
        public List<AutorDTO> GetIds([FromQuery] List<int> ids)
        {
            var autores = context.Autores.Where(x => ids.Contains(x.Id));
            var lst = mapper.Map<List<AutorDTO>>(autores);

            return lst;
        }


        [AllowAnonymous]
        [ServiceFilter(typeof(MiFiltroDeAccion),Order =1)] // filtro de accion
        [HttpGet("/api/v1/autores/GUID")]
        public ActionResult GetUID()
        {
            return Ok(new
            {
                AutoresController_Transient = servicioTransient.Guid,
                ServicioA_Transient = servicio.ObtenerTransient(),
                AutoresController_Scoped = servicioScoped.Guid,
                ServicioA_Scoped = servicio.ObtenerScoped(),
                AutoresController_Singleton = servicioSingleton.Guid,
                ServicioA_Singleton = servicio.ObtenerSingleton()

            });
        }

        [AllowAnonymous]
        [HttpGet("GetConfiguration")]
        public ActionResult<List<object>> ObtenerConfiguracionANDAnonymousObject()
        {

            //return configuration["env_var"].ToString();
            //return configuration["connectionstrings:ConexionSQL"].ToString();

            var listaLibroComentario = new List<object>();
            var aux = context.Libros.AsEnumerable();

            foreach (var libro in aux)
            {
                var comentarioslista = context.Comentarios.Where(x => x.LibroId == libro.Id).Select(a => new
                {
                    IdComentario = a.Id,
                    Comentario = a.Contenido
                });
                
                listaLibroComentario.Add(new
                {
                    Libro = libro.Id,
                    Titulo = libro.Titulo,
                    Comentarios = comentarioslista
                });

            }

            return Ok(listaLibroComentario);
        }

       

        /// <summary>
        /// Obtiene los autores
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "obtenerAutoresv1")] // api/autores
        //[AllowAnonymous]
        [ServiceFilter(typeof(HATEOASAutorFilterAttribute))]
        //public async Task<ActionResult> Get([FromQuery] bool incluirHATEOAS=true,[FromQuery] PaginacionDTO paginacionDTO)
        //public async Task<ActionResult<List<AutorDTO>>> Get([FromHeader] string incluirHATEOAS)
        public async Task<ActionResult<List<AutorDTO>>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {
            //throw new NotImplementedException();
            //logger.LogInformation("++++++++++++++++++++++++Obteniendo Autores (information log).++++++++++++++++++++++++++++");
            //logger.LogWarning("++++++++++++++++++++++++Obteniendo Autores (warning log).++++++++++++++++++++++++++++");
            //servicio.RealizarTarea();
            //var autores= await context.Autores/*.Include(x => x.Libros)*/.ToListAsync();

            //var dtos= mapper.Map<List<AutorDTO>>(autores.OrderBy(x=>x.Id).ToList());

            //if (incluirHATEOAS)
            //{
            //    var esAdmin = await authorizationService.AuthorizeAsync(User, "esAdmin");

            //    //dtos.ForEach(dto => GenerarEnlaces(dto, esAdmin.Succeeded));

            //    var resultado = new ColeccionDeRecursos<AutorDTO> { Valores = dtos };


            //    resultado.Enlaces.Add(new DatoHATEOAS(enlace: Url.Link("obtenerAutores", new { }), descripcion: "self", metodo: "GET"));

            //    if (esAdmin.Succeeded)
            //        resultado.Enlaces.Add(new DatoHATEOAS(enlace: Url.Link("crearAutor", new { }), descripcion: "autor-crear", metodo: "POST"));

            //    return Ok(resultado);
            //}

            //return Ok(dtos);


            //var autores = await context.Autores/*.Include(x => x.Libros)*/.ToListAsync();
            //return mapper.Map<List<AutorDTO>>(autores.OrderBy(x => x.Id).ToList());

            // paginacion
            var queryable = context.Autores.AsQueryable();
            await HttpContext.InsertarParametrosPaginacionEnCabecera(queryable);
            var autores = await queryable.OrderBy(autor => autor.Nombre).Paginar(paginacionDTO).ToListAsync();
            return mapper.Map<List<AutorDTO>>(autores);


        }


        //[HttpGet("primero")] // query-string -> api/autore/primero?nombre=pepe&apellido=lopes // query string a partir de ?
        //// modelbinding -> [FromHeader] indica de donde se obtiene el valor del parametro
        //public async Task<ActionResult<Autor>> PrimerAutor([FromHeader] int miValor, [FromQuery] string nombre)
        //{
        //    var autor= await context.Autores.FirstOrDefaultAsync();
        //    if (autor == null)
        //    {
        //        return NotFound();
        //    }
        //    return autor;
        //}


        [HttpGet("IActionResult/{id:int}")]// get con el id del autor
        public async Task<IActionResult> GetIActionResult(int id)
        {
            var autor = await context.Autores.FirstOrDefaultAsync(x => x.Id == id);
            if (autor == null)
            {
                return NotFound();
            }
            return Ok(autor);
        }



        [HttpGet("{nombre}", Name = "obtenerAutorPorNombrev1")] // get con el nombre del autor
        public async Task<ActionResult<List<AutorDTO>>> GetPorNombre([FromRoute] string nombre)
        {
            var autores = await context.Autores.Where(x => x.Nombre.Contains(nombre)).ToListAsync();
            //if (autores == null)
            //{
            //    return NotFound();
            //}

            return mapper.Map<List<AutorDTO>>(autores);
        }

        //[HttpGet("{id:int}/{param2=persona}")] // get con el id y el segundo parametro por defecto = persona
        //[HttpGet("{id:int}/{param2?}")] // get con el id y el segundo parametro opcional
        [HttpGet("{id:int}", Name = "obtenerAutorv1")]
        [AllowAnonymous]
        [ServiceFilter(typeof(HATEOASAutorFilterAttribute))]
        [ProducesResponseType(404)]
        [ProducesResponseType(200)]
        public async Task<ActionResult<AutorDTOConLibros>> Get(int id/*, string param2*/, [FromHeader] string incluirHATEOAS)
        {
            var autor = await context.Autores
                .Include(x => x.AutorLibros)
                .ThenInclude(a => a.Libro)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (autor == null)
            {
                return NotFound();
            }
            var dto = mapper.Map<AutorDTOConLibros>(autor);

            //var esAdmin = await authorizationService.AuthorizeAsync(User, "esAdmin");

            //GenerarEnlaces(dto, esAdmin.Succeeded);

            return dto;


        }





        [HttpPost(Name = "crearAutorv1")]
        [AllowAnonymous]
        [ServiceFilter(typeof(HATEOASAutorFilterAttribute))]
        public async Task<ActionResult> Post([FromForm] AutorCreacionDTO autorCreacionDTO)
        {
            var existeAutorMismoNombre = await context.Autores.AnyAsync(x => x.Nombre == autorCreacionDTO.NombreCompleto);

            if (existeAutorMismoNombre)
            {
                return BadRequest(new
                {
                    id=1,
                    erroMessage="Error - Ya existe un autor con ese nombre"
                });//$"Ya existe un autor con el nombre {autorCreacionDTO.NombreCompleto}");
            }

            // el dto no se le puede pasar a EF por lo que se crea el tipo valido con los datos del DTO
            //var autor = new Autor()
            //{
            //    Nombre = autorDTO.Nombre
            //};

            // otra solucion al dto es usar el objeto mapper (instalar por nuget)
            var autor = mapper.Map<Autor>(autorCreacionDTO);

            if (autorCreacionDTO.Imagen != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await autorCreacionDTO.Imagen.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();
                    var extension = Path.GetExtension(autorCreacionDTO.Imagen.FileName);
                    autor.Imagen = await almacenadorArchivosLocal.GuardarArchivo(contenido, extension, contenedor, autorCreacionDTO.Imagen.ContentType);
                }
            }

            context.Add(autor);
            await context.SaveChangesAsync();

            var autorDTOAux = mapper.Map<AutorDTO>(autor);

            return CreatedAtRoute("obtenerAutorv1", new { id = autor.Id }, autorDTOAux);
            //return Ok();

        }

        //actualizar
        [HttpPut("{id:int}", Name = "actualizarAutorv1")] // api/autores/1
        [AllowAnonymous]
        public async Task<ActionResult> Put([FromForm]AutorDTOPUT autorDTOPut, int id)
        {
            var existe = await context.Autores.AnyAsync(x => x.Id == id);
            if (!existe)
            {
                return NotFound();
            }

            //if (autor.Id != id)
            //{
            //    return BadRequest("El id del autor no coincide con el id de la url");
            //}

            //var autor2 = new Autor()
            //{
            //    Nombre = autor.Nombre,
            //    Id= id,
            //};
            var autor2 = mapper.Map<Autor>(autorDTOPut);
            if (autorDTOPut.Imagen != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await autorDTOPut.Imagen.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();
                    var extension = Path.GetExtension(autorDTOPut.Imagen.FileName);
                    autor2.Imagen = await almacenadorArchivosLocal.GuardarArchivo(contenido, extension, contenedor, autorDTOPut.Imagen.ContentType);
                }
            }
            context.Update(autor2);

            await context.SaveChangesAsync();
            return Ok();
        }
        /// <summary>
        /// Borra un autor
        /// </summary>
        /// <param name="id">Id del autor a borrar.</param>
        /// <returns></returns>
        [HttpDelete("{id:int}", Name = "borrarAutorv1")]
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Autores.AnyAsync(x => x.Id == id);
            if (!existe)
            {
                return NotFound();
            }

            context.Remove(new Autor() { Id = id });
            await context.SaveChangesAsync();
            return Ok();


        }

        [HttpGet("GetNoEF")]
        public async Task<ActionResult<List<AutorDTO>>> GetNoEF()
        {
            var resultado = new List<AutorDTO>();
            var connString = "Server=localhost;Port=5432;Database=WebApiAutores;User Id=postgres;Password=634510;";

            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();

            var query = "SELECT * FROM public.\"Autores\"";

            await using (var cmd = new NpgsqlCommand(query, conn))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    resultado.Add(new AutorDTO
                    {
                        Id = (int)reader.GetValue(0),
                        NombreCompleto = (string)reader.GetValue(1)
                    });
                }
            }
            return resultado;

        }

        [HttpGet("GetNoEFSerialized")]
        [AllowAnonymous]
        public async Task<ActionResult> GetNoEFSerialized()
        {
            //var resultado = new List<AutorDTO>();
            var connString = "Server=localhost;Port=5432;Database=WebApiAutores;User Id=postgres;Password=634510;";

            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();

            var query = "SELECT * FROM public.\"Autores\"";

            var values = new List<Dictionary<string, object>>(); // "tabla"


            await using (var cmd = new NpgsqlCommand(query, conn))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    // define the dictionary
                    var fieldValues = new Dictionary<string, object>();

                    // fill up each column and values on the dictionary                 
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        fieldValues.Add(reader.GetName(i), reader[i]);
                    }

                    // add the dictionary on the values list
                    values.Add(fieldValues);
                }
            }
            var resultado = JsonConvert.SerializeObject(values,Formatting.Indented);
            return Ok(resultado);
            
        }


        [HttpPost("PostNoEF")]
        [AllowAnonymous]
        public async Task<ActionResult> PostNoEF([FromBody] AutorCreacionDTO autorCreacionDTO)
        {
            var connString = "Server=localhost;Port=5432;Database=WebApiAutores;User Id=postgres;Password=634510;";


            await using (var conn = new NpgsqlConnection(connString))
            {
                await conn.OpenAsync();

                var transaction = conn.BeginTransaction();

                var query = "select add_autor_func(null,\'" + autorCreacionDTO.NombreCompleto + "\');";



                await using (var cmd = new NpgsqlCommand(query, conn, transaction))
                {
                    await using (var reader = await cmd.ExecuteReaderAsync()) ;
                }

                //throw new MyException("Error forzado para probar transaccion");

                //transaction.RollbackAsync();

                transaction.Commit();

            }

            return Ok();
        }

        [HttpPut("PutNoEF")]
        [AllowAnonymous]
        public async Task<ActionResult> PutNoEF([FromBody] AutorDTO autorDTO)
        {
            var connString = "Server=localhost;Port=5432;Database=WebApiAutores;User Id=postgres;Password=634510;";
            await using (var conn = new NpgsqlConnection(connString))
            {
                await conn.OpenAsync();
                var transaction = conn.BeginTransaction();


                var query = "call update_autor_sp(" + autorDTO.Id + ",\'" + autorDTO.NombreCompleto + "\');";


                await using (var cmd = new NpgsqlCommand(query, conn, transaction))
                {
                    await using (var reader = await cmd.ExecuteReaderAsync()) ;
                }


                //transaction.RollbackAsync();

                await transaction.CommitAsync();
            }
            

            return Ok();
        }

        [HttpDelete("DeleteNoEF/{id:int}")]
        public async Task<ActionResult> DeleteNoEF(int id)
        {
            var connString = "Server=localhost;Port=5432;Database=WebApiAutores;User Id=postgres;Password=634510;";
            await using (var conn = new NpgsqlConnection(connString))
            {
                await conn.OpenAsync();

                var query = "delete from public.\"Autores\" where public.\"Autores\".\"Id\" =" + id + ";";

                await using (var cmd = new NpgsqlCommand(query, conn))
                await using (var reader = await cmd.ExecuteReaderAsync());

            }
            return Ok();
        }
    }
}
