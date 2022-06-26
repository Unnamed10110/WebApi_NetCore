namespace WebApiAutores.DTOs.DTOBase
{
    public class AutorDTOConLibros : AutorDTO
    {
        //public int Id { get; set; }
        //public string NombreCompleto { get; set; }
        public List<LibroDTO> Libros { get; set; }
    }
}
