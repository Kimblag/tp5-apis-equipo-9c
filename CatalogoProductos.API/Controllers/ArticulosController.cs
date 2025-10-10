using CatalogoArticulos.Dominio.Entidades;
using CatalogoArticulos.DTOs;
using CatalogoArticulos.Negocio;
using CatalogoArticulos.API.DTOs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CatalogoArticulo.Comun.Helpers;
using CatalogoArticulo.Comun.Mappers;

namespace CatalogoArticulos.API.Controllers
{
    public class ArticulosController : ApiController
    {
        private readonly NegocioArticulo _negocioArticulo = new NegocioArticulo();

        // GET: api/Articulos
        public IHttpActionResult Get()
        {
            try
            {
                List<Articulo> listaArticulos = _negocioArticulo.Listar();

                var respuestaApi = new ApiResponse<List<ArticuloListadoDTO>>
                {
                    Status = 200,
                    Mensaje = null,
                    Resultado = MapperArticulo.ToDominio(listaArticulos)
                };

                return Ok(respuestaApi);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/Articulos/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Articulos
        public IHttpActionResult Post([FromBody] ArticuloGuardarDTO nuevoArticuloDTO)
        {
            try
            {
                ValidadorDTO.ValidarArticuloGuardarDTO(nuevoArticuloDTO);
                Articulo articulo = MapperArticulo.ToDominio(nuevoArticuloDTO);
                int idNuevoArticulo = _negocioArticulo.Guardar(articulo);

                var respuestaApi = new ApiResponse<int>
                {
                    Status = 201,
                    Mensaje = "Artículo creado exitosamente.",
                    Resultado = idNuevoArticulo
                };
                return Created($"{Request.RequestUri}/{idNuevoArticulo}", respuestaApi);
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

        // POST: api/Articulos/5/Imagenes
        [Route("api/Articulos/{idArticulo}/Imagenes")]
        public IHttpActionResult Post(int idArticulo, [FromBody] ArticuloAgregarImagenesDTO imagenesDTO)
        {
            try
            {
                ValidadorDTO.ValidarArticuloAgregarImagenesDTO(imagenesDTO);
                List<Imagen> nuevasImagenes = MapperArticulo.ToDominio(imagenesDTO);
                _negocioArticulo.GuardarImagenes(idArticulo, nuevasImagenes);

                var respuestaApi = new ApiResponse<int?>
                {
                    Status = 201,
                    Mensaje = "Imágenes agregadas exitosamente.",
                    Resultado = null
                };
                return Created(Request.RequestUri, respuestaApi);
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
