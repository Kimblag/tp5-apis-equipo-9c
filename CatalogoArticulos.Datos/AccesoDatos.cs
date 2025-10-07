using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
namespace CatalogoArticulos.Datos
{
    public class AccesoDatos : IDisposable
    {

        public SqlConnection Conexion { get { return conexion; } }
        public SqlCommand Comando { get { return comando; } }

        private SqlConnection conexion;
        private SqlCommand comando;

        public AccesoDatos()
        {
            conexion = new SqlConnection("server=.\\SQLEXPRESS; database=CATALOGO_P3_DB; integrated security=true");
            comando = new SqlCommand();
        }

        public void DefinirConsulta(string consultaSql)
        {
            comando.CommandType = CommandType.Text;
            comando.CommandText = consultaSql;
        }

        public SqlDataReader EjecutarConsulta()
        {
            comando.Connection = conexion;

            try
            {
                if (conexion.State == ConnectionState.Closed) conexion.Open();

                // se devuelve el lector, así le quitamos la responsabilidad a la clase.
                return comando.ExecuteReader();
            }
            catch (Exception)
            {
                this.Dispose(); // se usa la interfaz Dispose para poder usar el método dispose que es mas seguro
                throw;
            }
        }

        public void EjecutarAccion()
        {
            comando.Connection = conexion;
            try
            {
                if (conexion.State == ConnectionState.Closed) conexion.Open();
                comando.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int EjecutarAccionEscalar()
        {
            comando.Connection = conexion;
            object id = null;

            try
            {
                if (conexion.State == ConnectionState.Closed) conexion.Open();
                id = comando.ExecuteScalar();

                if (id == null || id == DBNull.Value) return 0;

                return Convert.ToInt32(id);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SetearParametro(string nombre, object valor)
        {
            comando.Parameters.AddWithValue(nombre, valor);
        }

        public void LimpiarParametros()
        {
            comando.Parameters.Clear();
        }

        public void Dispose()
        {
            // se cierra la conexion solo si existe y está abierta
            if (conexion != null && conexion.State == ConnectionState.Open)
            {
                conexion.Close();
            }

            // se liberan los recursos
            if (conexion != null)
            {
                conexion.Dispose();
                conexion = null;
            }
            if (comando != null)
            {
                comando.Dispose();
                comando = null;
            }
        }
    }
}
