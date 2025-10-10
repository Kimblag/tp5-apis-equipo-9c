using CatalogoArticulos.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CatalogoArticulos.Datos.Repositorios
{
    public class RepositorioArticulo
    {
        public List<Articulo> Listar()
        {
            List<Articulo> articulos = new List<Articulo>();

            using (AccesoDatos datos = new AccesoDatos())
            {
                try
                {
                    // Se recomienda LEFT JOIN para asegurar que el artículo se liste aunque no tenga imágenes 
                    datos.DefinirConsulta(@"
                        SELECT AR.Id, AR.Codigo, AR.Nombre, AR.Descripcion, AR.Precio,
                               IM.Id AS IdImagen, IM.ImagenUrl, 
                               CAT.Id AS IdCategoria, CAT.Descripcion AS Categoria, 
                               MARC.Id AS IdMarca, MARC.Descripcion AS Marca
                        FROM ARTICULOS AR
                        LEFT JOIN IMAGENES IM ON AR.Id = IM.IdArticulo 
                        INNER JOIN CATEGORIAS CAT ON AR.IdCategoria = CAT.Id
                        INNER JOIN MARCAS MARC ON AR.IdMarca = MARC.Id
                    ");

                    // ahora usamos using porque las buenas prácticas indican que esto nos evita dejar conexiones abiertas.
                    // se cierra automáticamente al finalizar el bloque using.
                    using (SqlDataReader lector = datos.EjecutarConsulta())
                    {
                        Articulo articuloActual = null;

                        while (lector.Read())
                        {
                            // usamos getordinal porque permite pedirle al lector el numero de la columna,
                            // de esa forma solo se busca la primera vez (a través del nombre de la columna) y luego
                            // solo usa el índice que hace que sea más rápido.
                            int idArticulo = lector.GetInt32(lector.GetOrdinal("Id"));

                            if (articuloActual == null || articuloActual.Id != idArticulo)
                            {
                                articuloActual = new Articulo
                                {
                                    Id = idArticulo,
                                    Codigo = lector.IsDBNull(lector.GetOrdinal("Codigo")) ? null : lector.GetString(lector.GetOrdinal("Codigo")),
                                    Nombre = lector.GetString(lector.GetOrdinal("Nombre")),
                                    Descripcion = lector.IsDBNull(lector.GetOrdinal("Descripcion")) ? null : lector.GetString(lector.GetOrdinal("Descripcion")),
                                    Precio = lector.IsDBNull(lector.GetOrdinal("Precio")) ? 0 : lector.GetDecimal(lector.GetOrdinal("Precio")),

                                    Categoria = new Categoria { Id = lector.GetInt32(lector.GetOrdinal("IdCategoria")), Descripcion = lector.GetString(lector.GetOrdinal("Categoria")) },
                                    Marca = new Marca { Id = lector.GetInt32(lector.GetOrdinal("IdMarca")), Descripcion = lector.GetString(lector.GetOrdinal("Marca")) },
                                    Imagenes = new List<Imagen>()
                                };

                                articulos.Add(articuloActual);
                            }

                            if (!lector.IsDBNull(lector.GetOrdinal("ImagenUrl")))
                            {
                                articuloActual.Imagenes.Add(
                                    new Imagen
                                    {
                                        Id = lector.GetInt32(lector.GetOrdinal("Id")),
                                        Url = lector.GetString(lector.GetOrdinal("ImagenUrl"))
                                    });
                            }
                        }
                    }

                    return articulos;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        private void CargarImagenes(List<Articulo> articulos)
        {
            if (articulos == null || articulos.Count == 0)
                return;

            List<string> ids = new List<string>();
            // usamos un diccionario para mapear mejor los articulos 
            Dictionary<int, Articulo> diccionarioArticulos = new Dictionary<int, Articulo>();

            foreach (var articulo in articulos)
            {
                ids.Add(articulo.Id.ToString());
                diccionarioArticulos.Add(articulo.Id, articulo);
            }

            // usamos placeholders para poder setear parámetros, esto es mas seguro que usar interpolacion Hay que evitar sql injection
            List<string> placeholders = new List<string>();

            for (int i = 0; i < articulos.Count; i++)
            {
                placeholders.Add($"@idArticulo{i}");
            }

            string listaPlaceholders = string.Join(",", placeholders);

            using (AccesoDatos datos = new AccesoDatos())
            {
                SqlDataReader lector = null;
                try
                {
                    datos.DefinirConsulta($"SELECT Id, IdArticulo, ImagenUrl FROM IMAGENES WHERE IdArticulo IN ({listaPlaceholders})");

                    for (int i = 0; i < articulos.Count; i++)
                    {
                        datos.SetearParametro($"@idArticulo{i}", articulos[i].Id);
                    }
                    lector = datos.EjecutarConsulta();

                    while (lector.Read())
                    {
                        int idImagen = lector.GetInt32(lector.GetOrdinal("Id"));
                        int idArticuloAsociacion = lector.GetInt32(lector.GetOrdinal("IdArticulo"));
                        string url = lector.GetString(lector.GetOrdinal("ImagenUrl"));

                        if (diccionarioArticulos.ContainsKey(idArticuloAsociacion))
                        {
                            Articulo articulo = diccionarioArticulos[idArticuloAsociacion];

                            if (articulo.Imagenes == null)
                            {
                                articulo.Imagenes = new List<Imagen>();
                            }

                            articulo.Imagenes.Add(new Imagen { Id = idImagen, Url = url });
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (lector != null && !lector.IsClosed)
                    {
                        lector.Close();
                    }
                }
            }
        }

        public List<Articulo> Buscar(string criterio)
        {
            List<Articulo> articulosEncontrados = new List<Articulo>();
            SqlDataReader lector = null;


            using (AccesoDatos datos = new AccesoDatos())
            {
                try
                {
                    string consulta = @"
                        SELECT 
                            A.Id, A.Codigo, A.Nombre, A.Descripcion, A.Precio,
                            M.Id AS IdMarca, M.Descripcion AS Marca,
                            C.Id AS IdCategoria, C.Descripcion AS Categoria
                        FROM ARTICULOS A
                        INNER JOIN MARCAS M ON A.IdMarca = M.Id
                        INNER JOIN CATEGORIAS C ON A.IdCategoria = C.Id
                        WHERE A.Codigo LIKE @Criterio 
                           OR A.Nombre LIKE @Criterio 
                           OR A.Descripcion LIKE @Criterio 
                           OR M.Descripcion LIKE @Criterio 
                           OR C.Descripcion LIKE @Criterio
                        ";
                    datos.DefinirConsulta(consulta);

                    //usamos comodines para que el LIKE busque parcialmente.
                    datos.SetearParametro("@Criterio", $"%{criterio}%");

                    lector = datos.EjecutarConsulta();

                    while (lector.Read())
                    {
                        Articulo aux = new Articulo
                        {
                            Id = lector.GetInt32(lector.GetOrdinal("Id")),
                            Codigo = lector.GetString(lector.GetOrdinal("Codigo")),
                            Nombre = lector.GetString(lector.GetOrdinal("Nombre")),
                            Descripcion = lector["Descripcion"] is DBNull ? string.Empty : (string)lector["Descripcion"],
                            Precio = lector.GetDecimal(lector.GetOrdinal("Precio")),

                            Marca = new Marca
                            {
                                Id = lector.GetInt32(lector.GetOrdinal("IdMarca")),
                                Descripcion = lector.GetString(lector.GetOrdinal("Marca"))
                            },
                            Categoria = new Categoria
                            {
                                Id = lector.GetInt32(lector.GetOrdinal("IdCategoria")),
                                Descripcion = lector.GetString(lector.GetOrdinal("Categoria"))
                            }
                        };

                        articulosEncontrados.Add(aux);
                    }
                    CargarImagenes(articulosEncontrados); // aqui le cargamos las imagenes
                    return articulosEncontrados;
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (lector != null && !lector.IsClosed)
                    {
                        lector.Close();
                    }
                }
            }
        }

        public int Guardar(Articulo nuevo)
        {
            using (AccesoDatos datos = new AccesoDatos())
            {
                int idArticulo = 0;

                try
                {
                    datos.DefinirConsulta(@"
                                INSERT INTO ARTICULOS (Codigo, Nombre, Descripcion, IdMarca, IdCategoria, Precio) 
                                OUTPUT inserted.Id 
                                VALUES (@Codigo, @Nombre, @Descripcion, @IdMarca, @IdCategoria, @Precio)
                             ");

                    datos.SetearParametro("@Codigo", nuevo.Codigo);
                    datos.SetearParametro("@Nombre", nuevo.Nombre);
                    datos.SetearParametro("@Descripcion", nuevo.Descripcion ?? (object)DBNull.Value);
                    datos.SetearParametro("@IdMarca", nuevo.Marca.Id);
                    datos.SetearParametro("@IdCategoria", nuevo.Categoria.Id);
                    datos.SetearParametro("@Precio", nuevo.Precio);

                    idArticulo = datos.EjecutarAccionEscalar();

                    if (idArticulo == 0)
                    {
                        throw new InvalidOperationException("No se pudo obtener el ID del artículo insertado.");
                    }

                    return idArticulo;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public void Modificar(Articulo articuloEditar)
        {
            using (AccesoDatos datos = new AccesoDatos())
            {
                try
                {
                    datos.IniciarTransaccion();

                    ActualizarArticulo(datos, articuloEditar);

                    List<string> urlsNuevas = new List<string>();
                    if (articuloEditar.Imagenes != null)
                    {
                        foreach (var imagen in articuloEditar.Imagenes)
                        {
                            urlsNuevas.Add(imagen.Url);
                        }
                    }
                    SincronizarImagenes(datos, articuloEditar.Id, urlsNuevas);

                    datos.ConfirmarTransaccion();
                }
                catch
                {
                    datos.RollbackTransaccion();
                    throw;
                }
            }
        }

        private void ActualizarArticulo(AccesoDatos datos, Articulo articulo)
        {
            datos.LimpiarParametros();
            datos.DefinirConsulta(@"
                        UPDATE ARTICULOS SET 
                            Codigo = @Codigo, 
                            Nombre = @Nombre, 
                            Descripcion = @Descripcion, 
                            IdMarca = @IdMarca, 
                            IdCategoria = @IdCategoria, 
                            Precio = @Precio 
                        WHERE Id = @Id
                    ");
            datos.SetearParametro("@Codigo", articulo.Codigo);
            datos.SetearParametro("@Nombre", articulo.Nombre);
            datos.SetearParametro("@Descripcion", articulo.Descripcion);
            datos.SetearParametro("@IdMarca", articulo.Marca.Id);
            datos.SetearParametro("@IdCategoria", articulo.Categoria.Id);
            datos.SetearParametro("@Precio", articulo.Precio);
            datos.SetearParametro("@Id", articulo.Id);

            datos.EjecutarAccion();
        }

        public List<string> ObtenerUrlsActuales(int idArticulo)
        {
            using (var datos = new AccesoDatos())
            {
                datos.LimpiarParametros();
                datos.DefinirConsulta("SELECT ImagenUrl FROM IMAGENES WHERE IdArticulo = @IdArticulo");
                datos.SetearParametro("@IdArticulo", idArticulo);

                List<string> urls = new List<string>();
                using (var lector = datos.EjecutarConsulta())
                {
                    while (lector.Read())
                    {
                        urls.Add(lector.GetString(lector.GetOrdinal("ImagenUrl")));
                    }
                }
                return urls;
            }
        }

        private void SincronizarImagenes(AccesoDatos datos, int idArticulo, List<string> urlsNuevas)
        {
            List<string> urlsActualesDB = ObtenerUrlsActuales(idArticulo);

            List<string> urlsAEliminar = new List<string>();
            foreach (string urlDB in urlsActualesDB)
            {
                bool encontrada = false;
                foreach (string urlNueva in urlsNuevas)
                {
                    if (urlDB == urlNueva)
                    {
                        encontrada = true;
                        break;
                    }
                }
                if (!encontrada)
                    urlsAEliminar.Add(urlDB);
            }


            List<string> urlsAInsertar = new List<string>();
            foreach (string urlNueva in urlsNuevas)
            {
                bool encontrada = false;
                foreach (string urlDB in urlsActualesDB)
                {
                    if (urlNueva == urlDB)
                    {
                        encontrada = true;
                        break;
                    }
                }
                if (!encontrada)
                    urlsAInsertar.Add(urlNueva);
            }


            if (urlsAEliminar.Count > 0)
            {
                List<string> placeholders = new List<string>();
                for (int i = 0; i < urlsAEliminar.Count; i++)
                {
                    placeholders.Add("@url" + i);
                }

                string listaPlaceholders = string.Join(",", placeholders);

                datos.LimpiarParametros();
                datos.DefinirConsulta($"DELETE FROM IMAGENES WHERE IdArticulo = @IdArticulo AND ImagenUrl IN ({listaPlaceholders})");
                datos.SetearParametro("@IdArticulo", idArticulo);

                for (int i = 0; i < urlsAEliminar.Count; i++)
                {
                    datos.SetearParametro("@url" + i, urlsAEliminar[i]);
                }

                datos.EjecutarAccion();
            }


            if (urlsAInsertar.Count > 0)
            {
                datos.DefinirConsulta("INSERT INTO IMAGENES (IdArticulo, ImagenUrl) VALUES (@IdArticulo, @ImagenUrl)");
                foreach (string url in urlsAInsertar)
                {
                    datos.LimpiarParametros();
                    datos.SetearParametro("@IdArticulo", idArticulo);
                    datos.SetearParametro("@ImagenUrl", url);
                    datos.EjecutarAccion();
                }
            }
        }

        public void Eliminar(int idArticulo)
        {
            using (AccesoDatos datos = new AccesoDatos())
            {
                try
                {
                    datos.IniciarTransaccion();

                    datos.LimpiarParametros();
                    datos.DefinirConsulta("DELETE FROM IMAGENES WHERE IdArticulo = @IdArticulo");
                    datos.SetearParametro("@IdArticulo", idArticulo);
                    datos.EjecutarAccion();

                    datos.LimpiarParametros();
                    datos.DefinirConsulta("DELETE FROM ARTICULOS WHERE Id = @IdArticulo");
                    datos.SetearParametro("@IdArticulo", idArticulo);
                    datos.EjecutarAccion();

                    datos.ConfirmarTransaccion();
                }
                catch (Exception)
                {
                    datos.RollbackTransaccion();
                    throw;
                }
            }
        }

        public void GuardarImagenes(int idArticulo, List<Imagen> nuevasImagenes)
        {
            using (AccesoDatos datos = new AccesoDatos())
            {
                try
                {
                    datos.IniciarTransaccion();
                    datos.DefinirConsulta("INSERT INTO IMAGENES (IdArticulo, ImagenUrl) VALUES (@IdArticulo, @ImagenUrl)");
                    datos.SetearParametro("@IdArticulo", idArticulo);

                    foreach (Imagen imagen in nuevasImagenes)
                    {
                        datos.LimpiarParametros();

                        datos.SetearParametro("@IdArticulo", idArticulo); // volvemos a setear el id 
                        datos.SetearParametro("@ImagenUrl", imagen.Url);

                        datos.EjecutarAccion();
                    }
                    datos.ConfirmarTransaccion();
                }
                catch (Exception)
                {
                    datos.RollbackTransaccion();
                    throw;
                }
            }
        }

        public bool ExisteCodigoArticulo(string codigo)
        {
            using (AccesoDatos datos = new AccesoDatos())
            {
                try
                {
                    datos.DefinirConsulta("SELECT COUNT(1) FROM ARTICULOS WHERE UPPER(Codigo) = UPPER(@Codigo)");
                    datos.SetearParametro("@Codigo", codigo.ToUpper());

                    int existe = datos.EjecutarAccionEscalar();

                    return existe != 0;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public bool ExisteIdArticulo(int idArticulo)
        {
            using (AccesoDatos datos = new AccesoDatos())
            {
                try
                {
                    datos.DefinirConsulta("SELECT COUNT(1) FROM ARTICULOS WHERE Id = @Id");
                    datos.SetearParametro("@Id", idArticulo);

                    int existe = datos.EjecutarAccionEscalar();

                    return existe != 0;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

    }
}
