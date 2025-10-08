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
    public static class MapperArticulo
    {
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

        public static List<Imagen> ToDominio(ArticuloAgregarImagenesDTO dto)
        {
            var imagenes = new List<Imagen>();
            foreach (var url in dto.UrlImagenes)
            {
                imagenes.Add(new Imagen { Url = url });
            }
            return imagenes;
        }
    }
}
