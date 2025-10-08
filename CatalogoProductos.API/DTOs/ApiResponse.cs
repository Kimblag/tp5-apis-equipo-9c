using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CatalogoArticulos.API.DTOs
{
    public class ApiResponse<T>
    {
        // Esta clase permite que tengamos una estructura definida para
        // las repsuestas ya que solo estabamos obteniendo valores sueltos sin contexto
        public int Status { get; set; }
        public string Mensaje { get; set; }
        public T Resultado { get; set; } 
    }
}