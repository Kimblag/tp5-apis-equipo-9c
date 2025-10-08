using CatalogoArticulos.DTOs;
using System;
using System.Collections.Generic;

namespace CatalogoArticulo.Comun.Helpers
{
    public static class ValidadorDTO
    {
        public static void ValidarArticuloGuardarDTO(ArticuloGuardarDTO dto)
        {
            if (dto == null)
                throw new InvalidOperationException("No se recibió ningún dato del artículo.");

            if (!ValidadorCampos.EsTextoValido(dto.Codigo, 1, 50))
                throw new InvalidOperationException("Código obligatorio y debe tener entre 1 y 50 caracteres.");

            if (!ValidadorCampos.EsTextoValido(dto.Nombre, 3, 50))
                throw new InvalidOperationException("Nombre obligatorio y debe tener entre 3 y 50 caracteres.");

            if (!string.IsNullOrEmpty(dto.Descripcion) && !ValidadorCampos.EsTextoValido(dto.Descripcion, 4, 150))
                throw new InvalidOperationException("Descripción debe tener entre 4 y 150 caracteres.");

            if (dto.IdMarca <= 0)
                throw new InvalidOperationException("Marca inválida.");

            if (dto.IdCategoria <= 0)
                throw new InvalidOperationException("Categoría inválida.");

            if (!ValidadorCampos.EsPrecioValido(dto.Precio))
                throw new InvalidOperationException("Precio inválido. Debe ser numérico positivo.");
        }

        public static void ValidarArticuloAgregarImagenesDTO(ArticuloAgregarImagenesDTO dto)
        {
            if (dto == null || dto.UrlImagenes == null || dto.UrlImagenes.Count == 0)
                throw new InvalidOperationException("No se proporcionaron imágenes.");

            foreach (var url in dto.UrlImagenes)
            {
                if (string.IsNullOrWhiteSpace(url))
                    throw new InvalidOperationException("LA URL no es valida, no debe tener espacios en blanco.");
                if (!ValidadorCampos.EsUrlValida(url))
                    throw new InvalidOperationException($"La URL {url} no es válida.");
            }
        }
    }
}
