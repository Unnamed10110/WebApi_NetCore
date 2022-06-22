using System.ComponentModel.DataAnnotations;
using WebApiAutores.Validaciones;

namespace WebApiAutores.DTOs.DTOBase
{
    public class LibroPutParcialDTO
    {
        [PrimeraLetraMayuscula]
        [Required]
        public string Titulo { get; set; }
    }
}
