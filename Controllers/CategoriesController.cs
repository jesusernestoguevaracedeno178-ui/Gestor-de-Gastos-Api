using GestorGastosApi.DTOs;
using GestorGastosApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SQLitePCL;

namespace GestorGastosApi.Controllers;

// <summary>
// Controlador HTTP para gestionar categorías.
// Solo recibe peticiones, llama al Service y devuelve respuestas HTTP.
// Toda la lógica vive en CategoryService.
// </summary>
[ApiController]                 // Activa validaciones automáticas y binding de JSON
[Route("api/[controller]")]     // Ruta base: /api/categories
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _service;

    // El Service viene inyectado por DI.
    public CategoriesController(ICategoryService service)
    {
        _service = service;
    }

     // ============================================================
    // GET /api/categories
    // Devuelve todas las categorías.
    // ============================================================
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _service.GetAllAsync();
        return Ok(categories); // HTTP 200 con la lista
    }

    // ============================================================
    // GET /api/categories/{id}
    // Devuelve una categoría por Id.
    // ============================================================
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _service.GetByIdAsync(id);
        if(category is null)
            return NotFound();    // HTTP 404
        return Ok(category);      // HTTP 200
    }
    
    // ============================================================
    // POST /api/categories
    // Crea una nueva categoría.
    // ============================================================
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto)
    {
        var created = await _service.CreateAsync(dto);

        // CreatedAtAction devuelve HTTP 201 con la cabecera Location
        // apuntando al endpoint GetById con el nuevo Id.
        return CreatedAtAction(nameof(GetById), new{id = created.Id}, created);
    }

    // ============================================================
    // PUT /api/categories/{id}
    // Actualiza una categoría existente.
    // ============================================================
    [HttpPut("id:int")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryUpdateDto dto)
    {
        if (id != dto.Id)
            return BadRequest("El Id de la URL no coincide con el del body.");

        var updated =await _service.UpdateAsync(id, dto);
        if(updated is null)
            return NotFound();  //HTTP 404

        return Ok(updated);     //HTTP 200 con el recurso actualizado.
    }

    
    // ============================================================
    // DELETE /api/categories/{id}
    // Elimina una categoría.
    // ============================================================ 
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted =await _service.DeleteAsync(id);
            if(!deleted)
                return NotFound();

            return NoContent();   //HTTP 204(éxito sin cuerpo)
        }
        catch (InvalidOperationException ex)
        {
            // La regla de negocio dice que no se puede borrar si tiene transacciones.
            return BadRequest(ex.Message);      //HTTP 400
        }
    }
}