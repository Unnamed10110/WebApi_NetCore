using System.ComponentModel.DataAnnotations;
using WebApiAutores.Validaciones;

namespace WebApiAutores.DTOs.DTOBase
{
    public class AutorCreacionDTOGraphQL
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")] //reglas de validacion por atributo
        [StringLength(maximumLength: 200, ErrorMessage = "El campo {0} no debe tener más de {1} caracteres.")]
        [PrimeraLetraMayuscula] // validacion desde la clase de validaciones
        public string NombreCompleto { get; set; }
        [PesoArchivoValidacion(PesoMaximoEnMegaBytes: 4)]
        [TipoArchivoValidacion(grupoTipoArchivo: GrupoTipoArchivo.Imagen)]
        public string Imagen { get; set; } // fromform en el post
    }
}
