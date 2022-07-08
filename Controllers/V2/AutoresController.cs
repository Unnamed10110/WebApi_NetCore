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
using WebApiAutores.Utilidades.HEADERS;
using WebApiAutores.DTOs.DTOPaginacion;

// mensajes de error-> Tipos:
// Critical - Error - Warning - Information - Debug - Trace
namespace WebApiAutores.Controllers.V2
{
    [ApiController] // -> para validaciones de los campos de los modelos
    [Route("api/V2/autores")]
    //[Route("api/autores")]
    //[CabeceraEstaPresente(cabecera: "x-version", valor: "2")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "EsAdmin")]
    //[Authorize] // autoriazacion a nivel de controlador
    public class AutoresController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;
        private readonly IAuthorizationService authorizationService;
        private readonly IWebHostEnvironment env;
        private readonly IHttpContextAccessor httpContextAccessor;

        //private readonly IServicio servicio;
        //private readonly ServicioTransient servicioTransient;
        //private readonly ServicioScoped servicioScoped;
        //private readonly ServicioSingleton servicioSingleton;
        //private readonly ILogger<AutoresController> logger;

        // inyeccion de dependencias
        public AutoresController(ApplicationDbContext context/*, IServicio servicio, ServicioTransient servicioTransient,
            ServicioScoped servicioScoped, ServicioSingleton servicioSingleton, ILogger<AutoresController> logger*/, IMapper mapper, IConfiguration configuration,
            IAuthorizationService authorizationService, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            this.context = context;
            this.mapper = mapper;
            this.configuration = configuration;
            this.authorizationService = authorizationService;
            this.env = env;
            this.httpContextAccessor = httpContextAccessor;
            //this.servicio = servicio;
            //this.servicioTransient = servicioTransient;
            //this.servicioScoped = servicioScoped;
            //this.servicioSingleton = servicioSingleton;
            //this.logger = logger;
        }

        //[HttpGet("GUID")]
        ////[ResponseCache(Duration =10)] // indica la duracion del cache
        //[ServiceFilter(typeof(MiFiltroDeAccion))] // filtro de accion
        //public ActionResult ObtenerGuids()
        //{
        //    return Ok(new 
        //    {
        //        AutoresController_Transient=servicioTransient.Guid, 
        //        ServicioA_Transient= servicio.ObtenerTransient(),
        //        AutoresController_Scoped = servicioScoped.Guid,
        //        ServicioA_Scoped = servicio.ObtenerScoped(),
        //        AutoresController_Singleton=servicioSingleton.Guid,
        //        ServicioA_Singleton = servicio.ObtenerSingleton()
        //    });;
        //}
        [AllowAnonymous]
        [HttpGet("GetConfiguration")]
        public ActionResult<string> ObtenerConfiguracion()
        {

            return configuration["env_var"].ToString();
            //return configuration["connectionstrings:ConexionSQL"].ToString();


        }

        //[HttpGet("listado")] // api/autores/listado
        //[HttpGet("/listado")]// listado
        //[Authorize] // autorizacion a nivel de metodo
        //[ServiceFilter(typeof(MiFiltroDeAccion))] // filtro de accion

        [HttpGet(Name = "obtenerAutoresv2")] // api/autores
        [AllowAnonymous]
        [ServiceFilter(typeof(HATEOASAutorFilterAttribute))]
        //public async Task<ActionResult> Get([FromQuery] bool incluirHATEOAS=true)
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
            //autores.ForEach(autor => autor.Nombre=autor.Nombre.ToUpper());

            var queryable = context.Autores.AsQueryable();

            double cantidad = await queryable.CountAsync();
            HttpContext.Response.Headers.Add("cantidadTotalRegistros", cantidad.ToString());

            var autores2= queryable.Skip((paginacionDTO.Pagina - 1) * paginacionDTO.RecordsPorPagina).Take(paginacionDTO.RecordsPorPagina);
            await autores2.ForEachAsync(x => x.Nombre = x.Nombre.ToUpper());

            return mapper.Map<List<AutorDTO>>(autores2.OrderBy(x => x.Id).ToList());


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



        [HttpGet("{nombre}", Name = "obtenerAutorPorNombrev2")] // get con el nombre del autor
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
        [HttpGet("{id:int}", Name = "obtenerAutorv2")]
        [AllowAnonymous]
        [ServiceFilter(typeof(HATEOASAutorFilterAttribute))]
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





        [HttpPost(Name = "crearAutorv2")]
        [AllowAnonymous]
        public async Task<ActionResult> Post([FromForm] AutorCreacionDTO autorCreacionDTO)
        {
            var existeAutorMismoNombre = await context.Autores.AnyAsync(x => x.Nombre == autorCreacionDTO.NombreCompleto);

            if (existeAutorMismoNombre)
            {
                return BadRequest(new
                {
                    id = 1,
                    erroMessage = "Error - Ya existe un autor con ese nombre"
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

                    var nombreArchivo = $"{Guid.NewGuid()}{extension}";
                    string folder = Path.Combine(env.WebRootPath, "autores");

                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    string ruta = Path.Combine(folder, nombreArchivo);
                    //await File.WriteAllBytesAsync(ruta, contenido);
                    await autorCreacionDTO.Imagen.CopyToAsync(new FileStream(ruta, FileMode.Create));

                    var urlActual = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}";
                    var urlParaBD = Path.Combine(urlActual, "autores", nombreArchivo).Replace("\\", "/");
                    autor.Imagen = urlParaBD;

                }
            }

            context.Add(autor);
            await context.SaveChangesAsync();

            var autorDTOAux = mapper.Map<AutorDTO>(autor);

            return CreatedAtRoute("obtenerAutorv2", new { id = autor.Id }, autorDTOAux);
            //return Ok();

        }


        //actualizar
        [HttpPut("{id:int}", Name = "actualizarAutorv2")] // api/autores/1
        public async Task<ActionResult> Put(AutorDTOPUT autorDTOPut, int id)
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

                    var nombreArchivo = $"{Guid.NewGuid()}{extension}";
                    string folder = Path.Combine(env.WebRootPath, "autores");

                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    string ruta = Path.Combine(folder, nombreArchivo);
                    //await File.WriteAllBytesAsync(ruta, contenido);
                    await autorDTOPut.Imagen.CopyToAsync(new FileStream(ruta, FileMode.Create));

                    var urlActual = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}";
                    var urlParaBD = Path.Combine(urlActual, "autores", nombreArchivo).Replace("\\", "/");
                    autor2.Imagen = urlParaBD;

                }
            }

            context.Update(autor2);

            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id:int}", Name = "borrarAutorv2")]
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

        [HttpPost("PostNoEF")]
        public async Task<ActionResult> PostNoEF([FromBody] AutorCreacionDTO autorCreacionDTO)
        {
            var connString = "Server=localhost;Port=5432;Database=WebApiAutores;User Id=postgres;Password=634510;";
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();

            var query = "select add_autor_func(null,\'" + autorCreacionDTO.NombreCompleto + "\');";


            await using (var cmd = new NpgsqlCommand(query, conn))
            await using (var reader = await cmd.ExecuteReaderAsync())



                return Ok();
        }

        [HttpPut("PutNoEF")]
        public async Task<ActionResult> PutNoEF([FromBody] AutorDTO autorDTO)
        {
            var connString = "Server=localhost;Port=5432;Database=WebApiAutores;User Id=postgres;Password=634510;";
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();

            var query = "call update_autor_sp(" + autorDTO.Id + ",\'" + autorDTO.NombreCompleto + "\');";


            await using (var cmd = new NpgsqlCommand(query, conn))
            await using (var reader = await cmd.ExecuteReaderAsync())



                return Ok();
        }

        [HttpDelete("DeleteNoEF/{id:int}")]
        public async Task<ActionResult> DeleteNoEF(int id)
        {
            var connString = "Server=localhost;Port=5432;Database=WebApiAutores;User Id=postgres;Password=634510;";
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();

            var query = "delete from public.\"Autores\" where public.\"Autores\".\"Id\" =" + id + ";";



            await using (var cmd = new NpgsqlCommand(query, conn))
            await using (var reader = await cmd.ExecuteReaderAsync())



                return Ok();
        }
    }
}
