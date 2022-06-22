using System.ComponentModel.DataAnnotations;
using WebApiAutores.Validaciones;

namespace WebApiAutores.DTOs.DTOBase
{
    public class LibroCreacionDTO
    {
        [PrimeraLetraMayuscula]
        [Required]
        public string Titulo { get; set; }

        public List<int> AutoresIds { get; set; }

        public DateTime FechaPublicacion { get; set; }

    }

}
