using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogoArticulos.DTOs
{
    public class ArticuloListadoDTO
    {
        public int IdArticulo { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public int IdMarca { get; set; }
        public string Marca { get; set; }
        public int IdCategoria { get; set; }
        public string Categoria { get; set; }
        public decimal? Precio { get; set; }
        public List<string> UrlImagenes { get; set; }
    }
}
