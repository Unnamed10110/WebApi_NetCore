using AutoMapper;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.ServiceModel.Channels;
using WebApiAutores.DTOs.DTOBase;
using WebApiAutores.Entidades;
using WebApiAutores.Servicios;
using WebApiAutores.Utilidades.CustomExceptions;
using static GraphQL.Instrumentation.Metrics;

namespace WebApiAutores.GraphQL
{
    public class GraphQLErrorFilter : IErrorFilter
    {
        public IError OnError(IError error)
        {
            return error.WithMessage(error.Exception.Message);
        }
    }
    public class MutationsAutores
    {//---------------------------------------
     // mutations
        public MutationsAutores()
        {

        }
        public async Task<AutorDTO> PostAutor([Service] ApplicationDbContext context, [Service] IMapper mapper, [Service] IHttpContextAccessor httpContextAccessor,/* [Service] AlmacenadorArchivosLocal almacenadorArchivosLocal,*/ AutorCreacionDTOGraphQL autorCreacionDTO)
        {
            string contenedor = "autores";
            var existeAutorMismoNombre = await context.Autores.AnyAsync(x => x.Nombre == autorCreacionDTO.NombreCompleto);

        if (existeAutorMismoNombre)
        {
            throw new Exception("User already exist!");
            //return BadRequest(new
            //{
            //    id = 1,
            //    erroMessage = "Error - Ya existe un autor con ese nombre"
            //});//$"Ya existe un autor con el nombre {autorCreacionDTO.NombreCompleto}");
        }

        // el dto no se le puede pasar a EF por lo que se crea el tipo valido con los datos del DTO
        //var autor = new Autor()
        //{
        //    Nombre = autorDTO.Nombre
        //};

        // otra solucion al dto es usar el objeto mapper (instalar por nuget)
        autorCreacionDTO.Imagen = "--";
            var autor = mapper.Map<Autor>(autorCreacionDTO);

        //if (autorCreacionDTO.Imagen != null)
        //{
        //    using (var memoryStream = new MemoryStream())
        //    {
        //        await autorCreacionDTO.Imagen.CopyToAsync(memoryStream);
        //        var contenido = memoryStream.ToArray();
        //        var extension = System.IO.Path.GetExtension(autorCreacionDTO.Imagen.FileName);
        //        autor.Imagen = await almacenadorArchivosLocal.GuardarArchivo(contenido, extension, contenedor, autorCreacionDTO.Imagen.ContentType);
        //    }
        //}
        autor.Imagen = "doesn't work with grapql directly? (IFormFile)";
            context.Add(autor);
            await context.SaveChangesAsync();

            var autorDTOAux = mapper.Map<AutorDTO>(autor);

            //return CreatedAtRoute("obtenerAutorv1", new { id = autor.Id }, autorDTOAux);
            return autorDTOAux;

        }

        public async Task<AutorDTO> PutAutor([Service] ApplicationDbContext context, [Service] IMapper mapper, [Service] IHttpContextAccessor httpContextAccessor, int id, AutorDTOPUTGraphQL autorDTOPUTGraphQL)
        {
            var existe = await context.Autores.AnyAsync(x => x.Id == id);
            if (!existe)
            {
                throw new Exception("Invalid ID!");
            }

            var autor=await context.Autores.FirstOrDefaultAsync(x => x.Id == id);
            autor.Nombre = autorDTOPUTGraphQL.NombreCompleto;

            var autor2 = mapper.Map<AutorDTO>(autor);
            
            context.Update(autor);

            await context.SaveChangesAsync();
            return autor2;
        }
        public async Task<CustomResponse> DeleteAutor([Service] ApplicationDbContext context, [Service] IMapper mapper, [Service] IHttpContextAccessor httpContextAccessor, int id)
        {
            var existe = await context.Autores.AnyAsync(x => x.Id == id);
            if (!existe)
            {
                throw new Exception("Invalid ID!");
            }

            var autor=await context.Autores.FirstOrDefaultAsync(x => x.Id == id);
            context.Autores.Remove(autor);

            await context.SaveChangesAsync();

            await context.SaveChangesAsync();


            var MessageDel = $"Deleted record with id:{id}";
           
            var o = new CustomResponse();

            o.Message = $"Deleted record with id:{id}";

            return o;
            //return MessageDel;

        }

        public class CustomResponse
        {
            public string Message{ get; set; }
        }


    }


}
