using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CatalogoArticulo.Comun
{
    public static class ValidadorCampos
    {
        public static bool EsTextoObligatorio(string texto)
        {
            return !string.IsNullOrWhiteSpace(texto);
        }

        public static bool EsAlfanumerico(string texto)
        {
            foreach (char c in texto)
            {
                if (!char.IsLetterOrDigit(c))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool EsNumerico(string texto)
        {
            foreach (char c in texto)
            {
                if (!char.IsDigit(c))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool TieneLongitudMinima(string texto, int minimo)
        {
            return texto?.Trim().Length >= minimo;
        }

        public static bool TieneLongitudMaxima(string texto, int maximo)
        {
            return texto?.Trim().Length <= maximo;
        }

        public static string NormalizarTexto(string texto)
        {
            return texto?.Trim().ToUpper();
        }

        public static bool EsTextoValido(string texto, int minimo, int maximo)
        {
            return EsTextoObligatorio(texto)
                && TieneLongitudMinima(texto, minimo)
                && TieneLongitudMaxima(texto, maximo);
        }

        public static bool EsEmailValido(string texto)
        {
            string patron = "^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$";
            return Regex.IsMatch(texto, patron);
        }

        public static bool EsSoloLetrasYEspacios(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return false;

            return texto.All(c => char.IsLetter(c) || char.IsWhiteSpace(c));
        }


        public static bool EsPrecioValido(decimal? valor)
        {
            bool esValido = true;
            if (!valor.HasValue) esValido = false;
            if (valor < 0) esValido = false;
            return esValido;
        }

        public static bool EsUrlValida(string texto)
        {
            string patron = @"^(http|https)://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?$";
            return Regex.IsMatch(texto, patron);
        }

    }
}
