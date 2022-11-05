using System.ComponentModel.DataAnnotations;
using WebApiAutores.DTOs.DTOHATEOAS;
using WebApiAutores.Entidades;
using WebApiAutores.Validaciones;

namespace WebApiAutores.DTOs.DTOBase
{
    public class AutorDTO : Recurso
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; }

        public string Imagen { get; set; }

        public static implicit operator AutorDTO(string v)
        {
            throw new NotImplementedException();
        }
    }




    public class AutorDTOPUT
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")] //reglas de validacion por atributo
        [StringLength(maximumLength: 200, ErrorMessage = "El campo {0} no debe tener más de {1} caracteres.")]
        [PrimeraLetraMayuscula] // validacion desde la clase de validaciones
        public string NombreCompleto { get; set; }

        public IFormFile Imagen { get; set; } // fromform en el post



    }
}
