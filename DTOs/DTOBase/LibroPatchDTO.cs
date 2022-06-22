using System.ComponentModel.DataAnnotations;
using WebApiAutores.Validaciones;

namespace WebApiAutores.DTOs.DTOBase
{
    public class LibroPatchDTO
    {
        [PrimeraLetraMayuscula]
        [Required]
        public string Titulo { get; set; }

        public List<int> AutoresIds { get; set; }

    }
}
