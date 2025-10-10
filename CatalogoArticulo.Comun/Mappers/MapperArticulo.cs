using CatalogoArticulos.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CatalogoArticulos.Dominio.Entidades;

namespace CatalogoArticulo.Comun.Mappers
{
    /// <summary>
    /// Esta clase está dedicada a realizar el "mapeo" de entidades a DTOs, ya que el controlador es el único
    /// que puede conocer los DTOs, la capa Negocio sólo conoce
    /// entidades, de esta manera podemos cumplir con principios de responsabilidad única y desacoplamiento de 
    /// las capas: si cambia un DTO sólo se cambia la capa de DTOs y el Mapper.
    /// </summary>
    public static class MapperArticulo
    {
        /// <summary>
        /// Método para mapear el DTO <c>ArticuloGuardarDTO</c> del endpoint Guardar a la entidad <c>Articulo</c>
        /// </summary>
        /// <param name="dto">El DTO a convertir.</param>
        /// <returns>La entidad Articulo</returns>
        public static Articulo ToDominio(ArticuloGuardarDTO dto)
        {
            return new Articulo
            {
                Codigo = dto.Codigo,
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Precio = dto.Precio,
                Marca = new Marca { Id = dto.IdMarca },
                Categoria = new Categoria { Id = dto.IdCategoria },
                Imagenes = new List<Imagen>()
            };
        }

        /// <summary>
        /// Método para mapear el DTO <c>ArticuloAgregarImagenesDTO</c> del endpoint 
        /// Guardar Imágenes a una lista de la entidad <c>Imagen</c>
        /// </summary>
        /// <param name="dto">El DTO a convertir.</param>
        /// <returns>Listado de la entidad Imagen</returns>
        public static List<Imagen> ToDominio(ArticuloAgregarImagenesDTO dto)
        {
            var imagenes = new List<Imagen>();
            foreach (var url in dto.UrlImagenes)
            {
                imagenes.Add(new Imagen { Url = url });
            }
            return imagenes;
        }

        /// <summary>
        /// Método para mapear un listado de la entidad <c>Articulo</c> del endpoint 
        /// Listar Artículos a una lista del DTO <c>ArticuloListadoDTO</c>
        /// </summary>
        /// <param name="dto">El DTO a convertir.</param>
        /// <returns>Listado de del DTO ArticuloListadoDTO</returns>
        public static List<ArticuloListadoDTO> ToDominio(List<Articulo> listaArticulos)
        {
            List<ArticuloListadoDTO> listadoArticulosDto = new List<ArticuloListadoDTO>();
            foreach (Articulo articulo in listaArticulos)
            {
                List<string> urls = new List<string>();
                foreach (Imagen imagen in articulo.Imagenes)
                {
                    urls.Add(imagen.Url);
                }

                ArticuloListadoDTO articuloActual = new ArticuloListadoDTO
                {
                    IdArticulo = articulo.Id,
                    Codigo = articulo.Codigo,
                    Nombre = articulo.Nombre,
                    Descripcion = articulo.Descripcion,
                    IdMarca = articulo.Marca.Id,
                    Marca = articulo.Marca.Descripcion,
                    IdCategoria = articulo.Categoria.Id,
                    Categoria = articulo.Categoria.Descripcion,
                    Precio = articulo.Precio,
                    UrlImagenes = urls
                };
                listadoArticulosDto.Add(articuloActual);
            }

            return listadoArticulosDto;
        }
    }
}
