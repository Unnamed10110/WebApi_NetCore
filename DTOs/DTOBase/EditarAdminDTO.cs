using System.ComponentModel.DataAnnotations;

namespace WebApiAutores.DTOs.DTOBase
{
    public class EditarAdminDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
