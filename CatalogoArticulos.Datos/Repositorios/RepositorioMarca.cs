using CatalogoArticulos.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogoArticulos.Datos.Repositorios
{
    public class RepositorioMarca
    {
        public bool Existe(int idMarca)
        {
            using (AccesoDatos datos = new AccesoDatos())
            {
                try
                {
                    datos.DefinirConsulta("SELECT COUNT(1) FROM MARCAS WHERE Id = @Id");
                    datos.SetearParametro("@Id", idMarca);

                    int existe = datos.EjecutarAccionEscalar();

                    return existe != 0;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }


        public Marca ObtenerPorID(int idMarca)
        {
            using (AccesoDatos datos = new AccesoDatos())
            {
                try
                {
                    datos.DefinirConsulta("SELECT Descripcion FROM MARCAS WHERE Id = @Id");
                    datos.SetearParametro("@Id", idMarca);

                    Marca marca = null;
                    using (SqlDataReader lector = datos.EjecutarConsulta())
                    {
                        if (lector.Read())
                        {
                            if (!lector.IsDBNull(lector.GetOrdinal("Descripcion")))
                            {
                                marca = new Marca
                                {
                                    Id = idMarca,
                                    Descripcion = lector.GetString(lector.GetOrdinal("Descripcion"))
                                };
                            }
                        }
                    }

                    return marca;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }


    }
}
