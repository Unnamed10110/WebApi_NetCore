using System.ComponentModel.DataAnnotations;
using WebApiAutores.Validaciones;

namespace WebApiAutores.Entidades
{
    public class Libro
    {
        public int Id { get; set; }
        [PrimeraLetraMayuscula]
        [Required]
        public string Titulo { get; set; }
        //public int AutorId { get; set; }

        //public Autor Autor{ get; set; }

        public List<Comentario> Comentarios { get; set; }

        public List<AutorLibro> AutoresLibros{ get; set; }

        public DateTime? FechaPublicacion { get; set; }
    }
}
