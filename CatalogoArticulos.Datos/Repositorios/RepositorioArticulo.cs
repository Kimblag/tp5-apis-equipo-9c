using CatalogoProductos.Dominio.Entidades;
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
                    datos.DefinirConsulta(@"
                        SELECT AR.Id, AR.Codigo, AR.Nombre, AR.Descripcion, AR.Precio,
                               IM.Id AS IdImagen, IM.ImagenUrl, 
                               CAT.Descripcion AS Categoria, MARC.Descripcion AS Marca
                        FROM ARTICULOS AR
                        -- Se recomienda LEFT JOIN para asegurar que el artículo se liste aunque no tenga imágenes
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

                                    Categoria = new Categoria { Descripcion = lector.GetString(lector.GetOrdinal("Categoria")) },
                                    Marca = new Marca { Descripcion = lector.GetString(lector.GetOrdinal("Marca")) },
                                    Imagenes = new List<Imagen>()
                                };

                                articulos.Add(articuloActual);
                            }

                            if (!lector.IsDBNull(lector.GetOrdinal("ImageUrl")))
                            {
                                articuloActual.Imagenes.Add(
                                    new Imagen
                                    {
                                        Id = lector.GetInt32(lector.GetOrdinal("IdImagen")),
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

            string idsArticulos = string.Join(",", ids);

            using (AccesoDatos datos = new AccesoDatos())
            {
                SqlDataReader lector = null;
                try
                {
                    datos.DefinirConsulta($"SELECT Id, IdArticulo, ImagenUrl FROM IMAGENES WHERE IdArticulo IN ({idsArticulos})");

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
                // usamos transaccion para evitar inconsistencias; todo o nada (como lo visto en BBDDII)
                SqlTransaction transaccion = null;
                int idArticulo = 0;

                try
                {
                    datos.Conexion.Open();
                    transaccion = datos.Conexion.BeginTransaction();
                    datos.Comando.Transaction = transaccion;

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

                    if (nuevo.Imagenes != null && nuevo.Imagenes.Count > 0)
                    {
                        datos.LimpiarParametros();
                        datos.DefinirConsulta("INSERT INTO IMAGENES (IdArticulo, ImagenUrl) VALUES (@IdArticulo, @ImagenUrl)");
                        datos.SetearParametro("@IdArticulo", idArticulo);

                        foreach (var imagen in nuevo.Imagenes)
                        {
                            datos.LimpiarParametros();
                            datos.SetearParametro("@IdArticulo", idArticulo);
                            datos.SetearParametro("@ImagenUrl", imagen.Url);

                            datos.EjecutarAccion();
                        }
                    }

                    transaccion.Commit(); // confirmar el commit
                    return idArticulo;
                }
                catch (Exception)
                {
                    try
                    {
                        // hacemos rollback en caso de fallo
                        transaccion?.Rollback(); // deshacemos TODO (artículo e imágenes).
                    }
                    catch (Exception exRollback)
                    {
                        throw new Exception("Error al intentar deshacer la transacción de alta.", exRollback);
                    }
                    throw;
                }
            }
        }

        public void Modificar(Articulo articuloEditar)
        {
            using (AccesoDatos datos = new AccesoDatos())
            {
                SqlTransaction transaccion = null;

                try
                {
                    datos.Conexion.Open();
                    transaccion = datos.Conexion.BeginTransaction();
                    datos.Comando.Transaction = transaccion;

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
                    datos.EjecutarAccion();


                    List<string> urlsActualesDB = ObtenerUrlsActuales(datos, articuloEditar.Id);

                    List<string> urlsNuevasEntidad = new List<string>();
                    if (articuloEditar.Imagenes != null)
                    {
                        foreach (var imagen in articuloEditar.Imagenes)
                        {
                            urlsNuevasEntidad.Add(imagen.Url);
                        }
                    }

                    List<string> urlsAEliminar = new List<string>();
                    foreach (string urlDB in urlsActualesDB)
                    {
                        // Si la url que está en la DB NO está en la nueva lista, se agrega para eliminarla
                        if (!urlsNuevasEntidad.Contains(urlDB))
                        {
                            urlsAEliminar.Add(urlDB);
                        }
                    }

                    List<string> urlsAInsertar = new List<string>();
                    foreach (string urlNueva in urlsNuevasEntidad)
                    {
                        // Si la URL nueva NO está en la DB, la guardamos
                        if (!urlsActualesDB.Contains(urlNueva))
                        {
                            urlsAInsertar.Add(urlNueva);
                        }
                    }

                    if (urlsAEliminar.Count > 0)
                    {
                        // generamos una lista de urls a eliminar para interpolar en la query de eliminar
                        string listaUrls = string.Join("','", urlsAEliminar);

                        datos.LimpiarParametros();
                        datos.DefinirConsulta($"DELETE FROM IMAGENES WHERE IdArticulo = @IdArticulo AND ImagenUrl IN ('{listaUrls}')");
                        datos.SetearParametro("@IdArticulo", articuloEditar.Id);
                        datos.EjecutarAccion();
                    }

                    if (urlsAInsertar.Count > 0)
                    {
                        datos.DefinirConsulta("INSERT INTO IMAGENES (IdArticulo, ImagenUrl) VALUES (@IdArticulo, @ImagenUrl)");

                        foreach (string url in urlsAInsertar)
                        {
                            datos.LimpiarParametros();
                            datos.SetearParametro("@IdArticulo", articuloEditar.Id);
                            datos.SetearParametro("@ImagenUrl", url);
                            datos.EjecutarAccion();
                        }
                    }

                    transaccion.Commit();
                }
                catch (Exception)
                {
                    try
                    {
                        transaccion?.Rollback();
                    }
                    catch (Exception exRollback)
                    {
                        throw new Exception("Error al intentar deshacer la transacción de modificación.", exRollback);
                    }
                    throw;
                }
            }
        }

        private List<string> ObtenerUrlsActuales(AccesoDatos datos, int idArticulo)
        {
            List<string> urls = new List<string>();

            datos.LimpiarParametros();
            datos.DefinirConsulta("SELECT ImagenUrl FROM IMAGENES WHERE IdArticulo = @IdArticulo");
            datos.SetearParametro("@IdArticulo", idArticulo);

            SqlDataReader lector = null;
            try
            {
                lector = datos.EjecutarConsulta();
                while (lector.Read())
                {
                    urls.Add(lector.GetString(0));
                }
            }
            finally
            {
                if (lector != null && !lector.IsClosed)
                {
                    lector.Close();
                }
            }
            return urls;
        }

        public void Eliminar(int idArticulo)
        {
            using (AccesoDatos datos = new AccesoDatos())
            {
                SqlTransaction transaccion = null;

                try
                {
                    datos.Conexion.Open();
                    transaccion = datos.Conexion.BeginTransaction();
                    datos.Comando.Transaction = transaccion;

                    datos.LimpiarParametros();
                    datos.DefinirConsulta("DELETE FROM IMAGENES WHERE IdArticulo = @IdArticulo");
                    datos.SetearParametro("@IdArticulo", idArticulo);
                    datos.EjecutarAccion();

                    datos.LimpiarParametros();
                    datos.DefinirConsulta("DELETE FROM ARTICULOS WHERE Id = @IdArticulo");
                    datos.SetearParametro("@IdArticulo", idArticulo);
                    datos.EjecutarAccion();

                    transaccion.Commit();
                }
                catch (Exception)
                {
                    try
                    {
                        transaccion?.Rollback();
                    }
                    catch (Exception exRollback)
                    {
                        throw new Exception("Error al intentar deshacer la transacción de eliminación.", exRollback);
                    }
                    throw;
                }
            }
        }

        public void GuardarImagenes(int idArticulo, List<string> urls)
        {
            using (AccesoDatos datos = new AccesoDatos())
            {
                try
                {
                    datos.DefinirConsulta("INSERT INTO IMAGENES (IdArticulo, ImagenUrl) VALUES (@IdArticulo, @ImagenUrl)");
                    datos.SetearParametro("@IdArticulo", idArticulo);

                    foreach (string url in urls)
                    {
                        datos.LimpiarParametros();


                        datos.SetearParametro("@IdArticulo", idArticulo); // volvemos a setear el id 
                        datos.SetearParametro("@ImagenUrl", url);

                        datos.EjecutarAccion();
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

    }
}
