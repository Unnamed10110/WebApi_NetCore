using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApiAutores.Validaciones;

namespace WebApiAutores.Entidades
{
    public class Autor: IValidatableObject // IValidatableObject -> para validar en el modelo IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")] //reglas de validacion por atributo
        [StringLength(maximumLength: 200, ErrorMessage = "El campo {0} no debe tener más de {1} caracteres.")]
        [PrimeraLetraMayuscula] // validacion desde la clase de validaciones
        public string Nombre { get; set; }

        public List<AutorLibro> AutorLibros { get; set; }





        //[Range(18, 120)]
        //[NotMapped] // el campo no se corresponderá a una propiedad en la BD, de lo contrario se tendría que hacer una migration
        //public int Edad { get; set; }

        //[CreditCard]
        //[NotMapped] // el campo no se corresponderá a una propiedad en la BD, de lo contrario se tendría que hacer una migration
        //public string TarjetaDeCredito{ get; set; }

        //[Url]
        //[NotMapped] // el campo no se corresponderá a una propiedad en la BD, de lo contrario se tendría que hacer una migration
        //public string URL { get; set; }

        //[NotMapped]
        //public int Menor { get; set; }

        //[NotMapped]
        //public int Mayor { get; set; }

        //public List<Libro> Libros { get; set; }

        // validacion por modelo
        // para que las validaciones por modelo se ejecuten -> deben pasar todas las validaciones de atributo
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(Nombre))
            {
                var primeraLetra = Nombre.ToString()[0].ToString();
                if (primeraLetra != primeraLetra.ToUpper())
                {
                    yield return new ValidationResult("La primera letra debe ser mayuscula.", new string[] {nameof(Nombre)});
                }
            }

            //if (Menor > Mayor)
            //{
            //    yield return new ValidationResult("Este valor no puede ser mayor que el campo Mayor");
            //}
            
        }
    }
}
