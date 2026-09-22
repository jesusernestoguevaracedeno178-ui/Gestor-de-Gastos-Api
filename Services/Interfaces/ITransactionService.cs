
using GestorGastosApi.DTOs;
using GestorGastosApi.Entities;

namespace GestorGastosApi.Services.Interfaces;

/// <summary>
/// Contrato del servicio de transacciones.
/// </summary>
public interface ITransactionService
{
    Task<IEnumerable<TransactionDto>> GetAllAsync();
    Task<TransactionDto?> GetByIdAsync(int id);
    Task<IEnumerable<TransactionDto>> GetByCategoryIdAsync(int categoryId);
    Task<IEnumerable<TransactionDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<TransactionDto> CreateAsync(TransactionCreateDto dto);
    Task<TransactionDto?> UpdateAsync(int id, TransactionUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}

