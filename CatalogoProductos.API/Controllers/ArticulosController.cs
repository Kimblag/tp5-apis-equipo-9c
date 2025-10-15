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

        // PUT: api/Articulos/
        public IHttpActionResult Put(int id, [FromBody] ArticuloActualizarDTO articuloModificadoDTO)
        {
            try
            {
                if (articuloModificadoDTO == null)
                {
                    return BadRequest("El cuerpo de la solicitud no puede estar vacío.");
                }
                // Esto captura errores de conversión como "letras en un campo de número".
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                ValidadorDTO.ValidarArticuloActualizarDTO(articuloModificadoDTO);
                Articulo articuloActual = _negocioArticulo.ListarPorId(id);

                if (articuloActual == null)
                {
                    return NotFound();
                }

                articuloActual.Codigo = articuloModificadoDTO.Codigo;
                articuloActual.Nombre = articuloModificadoDTO.Nombre;
                articuloActual.Descripcion = articuloModificadoDTO.Descripcion;
                articuloActual.Precio = articuloModificadoDTO.Precio;
                articuloActual.Marca.Id = articuloModificadoDTO.IdMarca;
                articuloActual.Categoria.Id = articuloModificadoDTO.IdCategoria;

                if (articuloModificadoDTO.UrlImagenes != null)
                {
                    articuloActual.Imagenes = articuloModificadoDTO.UrlImagenes
                        .Select(url => new Imagen { Url = url }).ToList();
                }

                _negocioArticulo.Modificar(articuloActual);

                var respuestaApi = new ApiResponse<int?>
                {
                    Status = 200,
                    Mensaje = "Artículo modificado exitosamente.",
                    Resultado = null
                };
                return Ok(respuestaApi);
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


        // DELETE: api/Articulos/5
        public IHttpActionResult Delete(int id)
        {
            try
            {
                _negocioArticulo.Eliminar(id);


                return StatusCode(System.Net.HttpStatusCode.NoContent);
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
    }
}
