# Catálogo de Productos API

API RESTful para la gestión de productos.
Permite listar, crear, modificar, eliminar y buscar productos, incluyendo marca, categoría e imágenes.
Implementa DTOs, validaciones básicas y manejo de errores, asegurando una comunicación clara y confiable entre cliente y servidor.

---

## Funcionalidades principales

* **Listar productos:** devuelve todos los productos con sus marcas, categorías e imágenes.
* **Obtener producto por ID:** devuelve un producto específico con todos sus datos relacionados.
* **Alta de producto:** crea un nuevo producto mediante un DTO simplificado.
* **Agregar imágenes:** permite asociar imágenes a un producto existente mediante su ID.
* **Modificar producto:** actualiza datos existentes con validaciones básicas.
* **Eliminar producto:** eliminación física de registros.
* **Búsqueda de productos:** permite filtrar productos por distintos criterios.

---

## Arquitectura

El proyecto está organizado en capas, siguiendo buenas prácticas de separación de responsabilidades:

* **Dominio:** entidades y modelos del negocio.
* **Comun:** clases y utilidades compartidas entre capas.
* **Negocio:** lógica de negocio y validaciones.
* **Datos:** acceso a datos mediante **ADO.NET**.
* **API:** controladores y endpoints que exponen los servicios a clientes.

---

## Tecnologías utilizadas

* **.NET Framework 4.8 / ASP.NET Web API**
* **ADO.NET** para acceso a datos
* **SQL Server**
* **DTOs y validaciones propias**

---

## Ejecución del proyecto

1. Clonar el repositorio:

   ```bash
   git clone https://github.com/usuario/catalogo-productos-api.git
   ```
2. Ejecutar la API desde Visual Studio.
3. Acceder a los endpoints mediante Postman o navegador.

---

## Endpoints RESTful

| Método | Endpoint                       | Descripción                    |
| ------ | ------------------------------ | ------------------------------ |
| GET    | `/api/productos`               | Lista todos los productos      |
| GET    | `/api/productos/{id}`          | Obtiene un producto específico |
| POST   | `/api/productos`               | Crea un nuevo producto         |
| PUT    | `/api/productos/{id}`          | Modifica un producto existente |
| DELETE | `/api/productos/{id}`          | Elimina un producto            |
| POST   | `/api/productos/{id}/imagenes` | Agrega imágenes a un producto  |
