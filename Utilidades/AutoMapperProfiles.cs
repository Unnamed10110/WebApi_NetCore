using AutoMapper;
using WebApiAutores.DTOs.DTOBase;
using WebApiAutores.Entidades;


namespace WebApiAutores.Utilidades
{
    public class AutoMapperProfiles:Profile
    {
        public AutoMapperProfiles()
        {

            // AUTORES
            // con el formember se define con que campo se mapea, si tienen nombres diferentes
            // GET
            CreateMap<Autor, AutorDTO>().ForMember(d => d.NombreCompleto, o => o.MapFrom(s => s.Nombre));
            CreateMap<Autor, AutorDTOConLibros>().ForMember(d => d.Libros, o => o.MapFrom(MapAutorDTOLibros));

            // POST
            CreateMap<AutorCreacionDTO, Autor>()
                .ForMember(d => d.Nombre, o => o.MapFrom(s => s.NombreCompleto))
                .ForMember(a=>a.Nombre, o=>o.MapFrom(c=>c.NombreCompleto));

            // PUT
            CreateMap<AutorDTOPUT, Autor>().ForMember(d=>d.Nombre, o=>o.MapFrom(s=>s.NombreCompleto));

            
            // LIBROS
            // GET libro
            CreateMap<Libro, LibroDTO>();
            CreateMap<Libro, LibroDTOConAutores>().ForMember(LibroDTO => LibroDTO.Autores, o => o.MapFrom(MapLibroDTOAutores));
            
            // POST libro
            CreateMap<LibroCreacionDTO, Libro>().ForMember(a=>a.AutoresLibros,o=>o.MapFrom(MapAutoresLibros));

            // PUT PARCIAL libro
            CreateMap<LibroPutParcialDTO, Libro>();


            // COMENTARIOS
            // POST comentario
            CreateMap<ComentarioCreacionDTO, Comentario>().ForMember(a => a.Contenido, b => b.MapFrom(c=>c.ContenidoLibro));

            // GET comentario
            CreateMap<Comentario, ComentarioDTO>().ForMember(a => a.ContenidoComentario, o => o.MapFrom(c=>c.Contenido));



            // PATCH libro
            CreateMap<LibroPatchDTO, Libro>().ReverseMap();



        }


        // GET Autores (member: List<LibroDTO> Libros)
        // List<AutorLibro> AutoresLibros -> List<LibroDTO>
        // CreateMap<Autor, AutorDTOConLibros>().ForMember(d => d.Libros, o => o.MapFrom(MapAutorDTOLibros));
        private List<LibroDTO> MapAutorDTOLibros(Autor autor, AutorDTO autorDTO)
        {
            var resultado = new List<LibroDTO>();

            if (autor.AutoresLibros == null)
            {
                return resultado;
            }

            foreach(var autorlibro in autor.AutoresLibros)
            {
                resultado.Add(new LibroDTO()
                {
                    Id=autorlibro.LibroId,
                    Titulo=autorlibro.Libro.Titulo
                });
            }

            return resultado;
        }


        // GET libro (member: List<AutorDTO> Autores)
        // List<AutorLibro> AutoresLibros -> List<AutorDTO>
        // CreateMap<Libro, LibroDTOConAutores>().ForMember(LibroDTO => LibroDTO.Autores, o => o.MapFrom(MapLibroDTOAutores));
        private List<AutorDTO> MapLibroDTOAutores(Libro libro, LibroDTO libroDTO)
        {
            var resultado=new List<AutorDTO>();

            if (libro.AutoresLibros == null)
            {
                return resultado;
            }

            foreach(var autorlibro in libro.AutoresLibros)
            {
                resultado.Add(new AutorDTO()
                {
                    Id = autorlibro.AutorId,
                    NombreCompleto = autorlibro.Autor.Nombre
                });
            }

            return resultado;

        }


        // POST libro (member: List<AutorLibro> AutoresLibros)
        // List<int> AutoresIds -> List<AutorLibro> AutoresLibros
        private List<AutorLibro> MapAutoresLibros(LibroCreacionDTO librocreacionDTO, Libro libro)
        {
            var resultado = new List<AutorLibro>();
            if (librocreacionDTO.AutoresIds == null)
            {
                return resultado;
            }

            foreach(var autorId in librocreacionDTO.AutoresIds)
            {
                resultado.Add(new AutorLibro()
                {
                    AutorId=autorId
                });;
            }

            return resultado;
        }
    }
}
