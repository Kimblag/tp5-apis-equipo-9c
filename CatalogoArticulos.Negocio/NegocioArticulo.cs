using CatalogoArticulo.Comun;
using CatalogoArticulos.API.DTOs;
using CatalogoArticulos.Datos.Repositorios;
using CatalogoArticulos.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogoArticulos.Negocio
{
    public class NegocioArticulo
    {
        private readonly RepositorioArticulo _repositorioArticulo;
        private readonly RepositorioMarca _repositorioMarca;
        private readonly RepositorioCategoria _repositorioCategoria;

        public NegocioArticulo()
        {
            _repositorioArticulo = new RepositorioArticulo();
            _repositorioMarca = new RepositorioMarca();
            _repositorioCategoria = new RepositorioCategoria();
        }


        public int Guardar(ArticuloGuardarDTO nuevoDTO)
        {
            ValidarDatos(nuevoDTO);
            ValidarReglasNegocio(nuevoDTO);
            Articulo nuevoArticulo = MapearADominio(nuevoDTO);

            try
            {
                return _repositorioArticulo.Guardar(nuevoArticulo);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private Articulo MapearADominio(ArticuloGuardarDTO nuevoDTO)
        {
            Marca marcaArticulo = _repositorioMarca.ObtenerPorID(nuevoDTO.IdMarca);
            Categoria categoriaArticulo = _repositorioCategoria.ObtenerPorID(nuevoDTO.IdCategoria);
            List<Imagen> imagenes = new List<Imagen>();

            if (nuevoDTO.UrlImagenes != null && nuevoDTO.UrlImagenes.Count > 0)
            {
                foreach (string url in nuevoDTO.UrlImagenes)
                {
                    Imagen imagenActual = new Imagen
                    {
                        Url = url
                    };
                    imagenes.Add(imagenActual);
                }
            }

            Articulo nuevoArticulo = new Articulo
            {
                Codigo = nuevoDTO.Codigo,
                Nombre = nuevoDTO.Nombre,
                Descripcion = nuevoDTO.Descripcion,
                Precio = nuevoDTO.Precio,


                Marca = marcaArticulo,
                Categoria = categoriaArticulo,
                Imagenes = imagenes ?? new List<Imagen>()
            };
            return nuevoArticulo;
        }

        private void ValidarReglasNegocio(ArticuloGuardarDTO nuevoDTO)
        {
            if (!_repositorioMarca.Existe(nuevoDTO.IdMarca)) throw new InvalidOperationException("La marca indicada no existe.");
            if (!_repositorioCategoria.Existe(nuevoDTO.IdCategoria)) throw new InvalidOperationException("La categoría indicada no existe.");
            if (_repositorioArticulo.ExisteCodigoArticulo(nuevoDTO.Codigo)) throw new InvalidOperationException("Ya existe un artículo con el código indicado.");
        }

        private void ValidarDatos(ArticuloGuardarDTO nuevoDTO)
        {
            if (!ValidadorCampos.EsTextoValido(nuevoDTO.Codigo, 1, 50)) throw new InvalidOperationException("El Código es obligatorio y debe tener entre 1 y 50 caracteres.");
            if (!ValidadorCampos.EsTextoValido(nuevoDTO.Nombre, 3, 50)) throw new InvalidOperationException("El Nombre es obligatorio y debe tener entre 3 y 50 caracteres.");
            // descripción es opcional
            if (!string.IsNullOrEmpty(nuevoDTO.Descripcion) && !ValidadorCampos.EsTextoValido(nuevoDTO.Descripcion, 4, 150)) throw new InvalidOperationException("La descripción debe tener entre 4 y 50 caracteres.");
            if (!ValidadorCampos.EsPrecioValido(nuevoDTO.Precio)) throw new InvalidOperationException("El Precio es obligatorio y no puede ser un valor negativo");
            if (nuevoDTO.UrlImagenes != null && nuevoDTO.UrlImagenes.Count > 0)
            {
                foreach (string url in nuevoDTO.UrlImagenes)
                {
                    if (!ValidadorCampos.EsUrlValida(url)) throw new InvalidOperationException("Una de las URLs de imagen proporcionadas no tiene un formato válido.");
                }
            }
        }
    }
}
