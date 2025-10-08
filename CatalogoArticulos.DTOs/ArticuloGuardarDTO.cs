using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CatalogoArticulos.DTOs
{
    public class ArticuloGuardarDTO
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public int IdMarca { get; set; }
        public int IdCategoria { get; set; }
        public decimal? Precio { get; set; }
        public List<string> UrlImagenes { get; set; }
    }
}