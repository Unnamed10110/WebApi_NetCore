﻿using System.Runtime.Serialization;

namespace WebApiAutores.DTOs.DTOHATEOAS
{

    public class Recurso
    {

        public List<DatoHATEOAS> Enlaces { get; set; } = new List<DatoHATEOAS>();
    }
}
