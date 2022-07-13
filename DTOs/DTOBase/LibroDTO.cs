using WebApiAutores.DTOs.DTOHATEOAS;
using WebApiAutores.Entidades;

namespace WebApiAutores.DTOs.DTOBase
{
    public class LibroDTO : Recurso
    {
        public int Id { get; set; }

        public string Titulo { get; set; }

        public List<ComentarioDTO> Comentarios { get; set; }

        public DateTime FechaPublicacion { get; set; }


        // model binders personalizados
        //[ModelBinder(BinderType = typeof(ModelBinderPersonalizado<List<int>>))]
        //public List<int> GenerosIDs { get; set; }

        //[ModelBinder(BinderType = typeof(ModelBinderPersonalizado<List<ActorPeliculasCreacionDTO>>))]
        //public List<ActorPeliculasCreacionDTO> Actores { get; set; }

    }
}
