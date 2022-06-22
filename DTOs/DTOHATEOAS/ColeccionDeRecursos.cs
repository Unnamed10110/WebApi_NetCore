namespace WebApiAutores.DTOs.DTOHATEOAS
{
    public class ColeccionDeRecursos<T> : Recurso where T : Recurso
    {
        public List<T> Valores { get; set; }
    }
}
