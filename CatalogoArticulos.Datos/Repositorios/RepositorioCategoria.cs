using CatalogoArticulos.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogoArticulos.Datos.Repositorios
{
    public class RepositorioCategoria
    {
        public bool Existe(int idCategoria)
        {
            using (AccesoDatos datos = new AccesoDatos())
            {
                try
                {
                    datos.DefinirConsulta("SELECT COUNT(1) FROM CATEGORIAS WHERE Id = @Id");
                    datos.SetearParametro("@Id", idCategoria);

                    int existe = datos.EjecutarAccionEscalar();

                    return existe != 0;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }


        public Categoria ObtenerPorID(int idCategoria)
        {
            using (AccesoDatos datos = new AccesoDatos())
            {
                try
                {
                    datos.DefinirConsulta("SELECT Descripcion FROM CATEGORIAS WHERE Id = @Id");
                    datos.SetearParametro("@Id", idCategoria);

                    Categoria categoria = null;
                    using (SqlDataReader lector = datos.EjecutarConsulta())
                    {
                        if (lector.Read())
                        {
                            if (!lector.IsDBNull(lector.GetOrdinal("Descripcion")))
                            {
                                categoria = new Categoria
                                {
                                    Id = idCategoria,
                                    Descripcion = lector.GetString(lector.GetOrdinal("Descripcion"))
                                };
                            }
                        }
                    }

                    return categoria;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }
}
