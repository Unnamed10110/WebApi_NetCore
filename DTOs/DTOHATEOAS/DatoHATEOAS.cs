using System.Runtime.Serialization;

namespace WebApiAutores.DTOs.DTOHATEOAS
{
    [Serializable]
    public class DatoHATEOAS
    {
        private string _Enlace;
        public string Enlace
        {
            get { return _Enlace; }
            set { _Enlace = value; }
        }

        private string _Descripcion;
        public string Descripcion
        {
            get { return _Descripcion; }
            set { _Descripcion = value; }
        }

        private string _Metodo;
        public string Metodo
        {
            get { return _Metodo; }
            set { _Metodo  = value; }
        }
        
        
        
        public DatoHATEOAS(string enlace, string descripcion, string metodo)
        {
            _Enlace = enlace;
            _Descripcion = descripcion;
            _Metodo = metodo;
        }
    }
}
