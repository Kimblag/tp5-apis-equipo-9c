using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogoProductos.Dominio.Entidades
{
    public class Articulo
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }

        // Relaciones
        public Marca Marca{ get; set; }
        public Categoria Categoria { get; set; }
        public List<Imagen> Imagenes { get; set; } = new List<Imagen>();
    }
}
