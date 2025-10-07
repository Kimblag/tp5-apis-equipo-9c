using CatalogoArticulos.API.DTOs;
using CatalogoArticulos.Negocio;
using CatalogoProductos.API.DTOs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CatalogoProductos.API.Controllers
{
    public class ArticulosController : ApiController
    {
        private readonly NegocioArticulo _negocioArticulo = new NegocioArticulo();

        // GET: api/Articulos
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Articulos/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Articulos
        public IHttpActionResult Post([FromBody] ArticuloGuardarDTO articulo)
        {
            try
            {
                int idNuevoArticulo = _negocioArticulo.Guardar(articulo);
                var respuestaApi = new ApiResponse<int>
                {
                    Status = 201,
                    Mensaje = "Artículo creado exitosamente.",
                    Resultado = idNuevoArticulo
                };
                return Created($"{Request.RequestUri}/{idNuevoArticulo.ToString()}", respuestaApi);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // PUT: api/Articulos/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/Articulos/5
        public void Delete(int id)
        {
        }
    }
}
