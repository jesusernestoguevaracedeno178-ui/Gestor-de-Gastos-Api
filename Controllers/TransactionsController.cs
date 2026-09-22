using GestorGastosApi.DTOs;
using GestorGastosApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SQLitePCL;

namespace GestorGastosApiControllers;

/// <summary>
/// Controlador HTTP para gestionar transacciones.
/// </summary>
[ApiController]
[Route("api/[controller]")]            //Ruta base: /api/transactions.
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _service;

    public TransactionsController(ITransactionService service)
    {
        _service = service;
    }

    // ============================================================
    // GET /api/transactions
    // Devuelve todas las transacciones (con su categoría).
    // ============================================================
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var transactions = await _service.GetAllAsync();
        return Ok(transactions);
    }
    
    // ============================================================
    // GET /api/transactions/{id}
    // ============================================================
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var transaction = await _service.GetByIdAsync(id);
        if(transaction is null)
            return NotFound();
        return Ok(transaction);
    }
    // ============================================================
    // GET /api/transactions/category/{categoryId}
    // Filtra transacciones por categoría.
    // ============================================================
    [HttpGet("Category/{categoryId:int}")]
    public async Task<IActionResult> GetByCategory(int categoryId)
    {
        var transactions = await _service.GetByCategoryIdAsync(categoryId);
        return Ok(transactions);
    }

    // ============================================================
    // GET /api/transactions/range?start=2024-01-01&end=2024-12-31
    // Filtra transacciones por rango de fechas.
    // ============================================================
    [HttpGet("range")]
    public async Task<IActionResult> GetByDateRange(
        [FromQuery] DateTime start,
        [FromQuery] DateTime end)
    {
        if(start > end)
            return BadRequest("La fecha inicial no puede ser mayor a la final.");

        var transactions =await _service.GetByDateRangeAsync(start, end);
        return Ok(transactions); 
    }

    // ============================================================
    // POST /api/transactions
    // ============================================================
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TransactionCreateDto dto)
    {
        try
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new{id = created.Id}, created);
        }
        catch(InvalidOperationException ex)
        {
             return BadRequest(ex.Message);    // Reglas de negocio: categoría no existe, monto inválido, etc.
        }
    }
    // ============================================================
    // PUT /api/transactions/{id}
    // ===================================================
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] TransactionUpdateDto dto)
    {
        if(id != dto.Id)
            return BadRequest("El Id de la URL no coincide con el del body.");

        try
        {
            var update = await _service.UpdateAsync(id, dto);
            if(update is null)
                return NotFound();

            return Ok(update);
        }
        catch(InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // ============================================================
    // DELETE /api/transactions/{id}
    // ============================================================
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if(!deleted)
            return NotFound();

        return NoContent();
    }


}