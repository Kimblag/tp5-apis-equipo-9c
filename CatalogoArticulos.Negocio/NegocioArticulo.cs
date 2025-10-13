using CatalogoArticulo.Comun;
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

        public int Guardar(Articulo nuevoArticulo)
        {
            ValidarReglasNegocioArticulo(nuevoArticulo);

            try
            {
                return _repositorioArticulo.Guardar(nuevoArticulo);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void GuardarImagenes(int idArticulo, List<Imagen> urlsImagenes)
        {
            ValidarReglasNegocioImagen(idArticulo, urlsImagenes);
            try
            {
                _repositorioArticulo.GuardarImagenes(idArticulo, urlsImagenes);
                return;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void ValidarReglasNegocioArticulo(Articulo nuevoArticulo)
        {
            if (!_repositorioMarca.Existe(nuevoArticulo.Marca.Id)) throw new InvalidOperationException("La marca indicada no existe.");
            if (!_repositorioCategoria.Existe(nuevoArticulo.Categoria.Id)) throw new InvalidOperationException("La categoría indicada no existe.");
            if (_repositorioArticulo.ExisteCodigoArticulo(nuevoArticulo.Codigo)) throw new InvalidOperationException("Ya existe un artículo con el código indicado.");
        }

        private void ValidarReglasNegocioImagen(int idArticulo, List<Imagen> imagenes)
        {
            if (!_repositorioArticulo.ExisteIdArticulo(idArticulo))
                throw new InvalidOperationException("El artículo ingresado no existe.");

            List<string> urlsActuales = _repositorioArticulo.ObtenerUrlsActuales(idArticulo);

            List<string> urlsVistas = new List<string>();
            List<string> urlsDuplicadasInternas = new List<string>();
            List<string> urlsDuplicadasEnBD = new List<string>();

            foreach (Imagen img in imagenes)
            {
                if (img.Url == null) continue;

                //verificar que no envíen las urls repetidas en la misma lista
                if (urlsVistas.Contains(img.Url))
                    urlsDuplicadasInternas.Add(img.Url);
                else
                    urlsVistas.Add(img.Url);

                // verificar que no envíen urls que ya existen en bd
                foreach (string url in urlsActuales)
                {
                    if (url == img.Url)
                    {
                        urlsDuplicadasEnBD.Add(img.Url);
                        break;
                    }
                }
            }


            if (urlsDuplicadasInternas.Count > 0)
                throw new InvalidOperationException("Las siguientes URLs están repetidas en la lista: " + string.Join(", ", urlsDuplicadasInternas));

            if (urlsDuplicadasEnBD.Count > 0)
                throw new InvalidOperationException("Las siguientes URLs ya existen para este artículo: " + string.Join(", ", urlsDuplicadasEnBD));
        }


        public List<Articulo> Listar()
        {
            return _repositorioArticulo.Listar();
        }

        public void Modificar(Articulo articuloModificado)
        {
            ValidarReglasNegocioArticuloModificacion(articuloModificado);

            try
            {
                _repositorioArticulo.Modificar(articuloModificado);
            }
            catch (Exception)
            {
                throw;
            }
        }


        private void ValidarReglasNegocioArticuloModificacion(Articulo articulo)
        {
            if (!_repositorioArticulo.ExisteIdArticulo(articulo.Id))
                throw new InvalidOperationException($"El artículo con ID {articulo.Id} no existe y no puede ser modificado.");

            if (!_repositorioMarca.Existe(articulo.Marca.Id))
                throw new InvalidOperationException("La marca indicada no existe.");

            if (!_repositorioCategoria.Existe(articulo.Categoria.Id))
                throw new InvalidOperationException("La categoría indicada no existe.");

            if (_repositorioArticulo.ExisteCodigoArticulo(articulo.Codigo, articulo.Id))
                throw new InvalidOperationException("Ya existe otro artículo con el código indicado.");
        }

        public void Eliminar(int idArticulo)
        {
 
            if (!_repositorioArticulo.ExisteIdArticulo(idArticulo))
                throw new InvalidOperationException($"El artículo con ID {idArticulo} no existe y no puede ser eliminado.");

            try
            {
                _repositorioArticulo.Eliminar(idArticulo);
            }
            catch (Exception)
            {
                throw;
            }
        }


    }
}
