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
            CreateMap<Autor, AutorDTO>().ForMember(destino => destino.NombreCompleto, origen => origen.MapFrom(s => s.Nombre));
            CreateMap<Autor, AutorDTOConLibros>().ForMember(destino => destino.Libros, origen => origen.MapFrom(MapAutorDTOLibros));

            // POST
            CreateMap<AutorCreacionDTO, Autor>()
                .ForMember(destino => destino.Nombre, origen => origen.MapFrom(s => s.NombreCompleto))
                .ForMember(destino => destino.Nombre, origen => origen.MapFrom(c=>c.NombreCompleto));

            // PUT
            CreateMap<AutorDTOPUT, Autor>().ForMember(destino => destino.Nombre, origen => origen.MapFrom(s=>s.NombreCompleto));

            
            // LIBROS
            // GET libro
            CreateMap<Libro, LibroDTO>();
            CreateMap<Libro, LibroDTOConAutores>().ForMember(LibroDTO => LibroDTO.Autores, origen => origen.MapFrom(MapLibroDTOAutores));
            
            // POST libro
            CreateMap<LibroCreacionDTO, Libro>().ForMember(destino => destino.AutoresLibros, origen => origen.MapFrom(MapAutoresLibros));

            // PUT PARCIAL libro
            CreateMap<LibroPutParcialDTO, Libro>();


            // COMENTARIOS
            // POST comentario
            CreateMap<ComentarioCreacionDTO, Comentario>().ForMember(destino => destino.Contenido, origen => origen.MapFrom(c=>c.ContenidoLibro));

            // GET comentario
            CreateMap<Comentario, ComentarioDTO>().ForMember(destino => destino.ContenidoComentario, origen => origen.MapFrom(c=>c.Contenido));



            // PATCH libro
            CreateMap<LibroPatchDTO, Libro>().ReverseMap();



        }


        // GET Autores (member: List<LibroDTO> Libros)
        // List<AutorLibro> AutorLibros -> List<LibroDTO>
        // CreateMap<Autor, AutorDTOConLibros>().ForMember(d => d.Libros, o => o.MapFrom(MapAutorDTOLibros));
        private List<LibroDTO> MapAutorDTOLibros(Autor autor, AutorDTO autorDTO)
        {
            var resultado = new List<LibroDTO>();

            if (autor.AutorLibros == null)
            {
                return resultado;
            }

            foreach(var autorlibro in autor.AutorLibros)
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
        // List<AutorLibro> AutorLibros -> List<AutorDTO>
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


        // POST libro (member: List<AutorLibro> AutorLibros)
        // List<int> AutoresIds -> List<AutorLibro> AutorLibros
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
