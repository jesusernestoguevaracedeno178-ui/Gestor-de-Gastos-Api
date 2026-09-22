using GestorGastosApi.DTOs;
using GestorGastosApi.Entities;
using GestorGastosApi.Repositories.Interfaces;
using GestorGastosApi.Services.Interfaces;

namespace GestorGastosApi.Services;

/// <summary>
/// Servicio de transacciones.
/// Aplica las reglas de negocio relacionadas con ingresos y gastos.
/// </summary>
public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;

    // El Service puede depender de VARIOS repositorios si los necesita.
    public TransactionService(
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<TransactionDto>> GetAllAsync()
    {
        var transactions = await _transactionRepository.GetAllAsync();
        return transactions.Select(t => ToDto(t));
    }

    public async Task<TransactionDto?> GetByIdAsync(int id)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id);
        return transaction is null ? null : ToDto(transaction);
    }

    public async Task<IEnumerable<TransactionDto>> GetByCategoryIdAsync(int categoryId)
    {
        var transactions = await _transactionRepository.GetByCategoryIdAsync(categoryId);
        return transactions.Select(t => ToDto(t));
    }

    public async Task<IEnumerable<TransactionDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var transactions = await _transactionRepository.GetByDateRangeAsync(startDate, endDate);
        return transactions.Select(t => ToDto(t));
    }

    public async Task<TransactionDto> CreateAsync(TransactionCreateDto dto)
    {
        // REGLA DE NEGOCIO: la categoría debe existir.
        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
        if (category is null)
            throw new InvalidOperationException(
                $"La categoría con Id {dto.CategoryId} no existe.");

        // REGLA DE NEGOCIO: el monto debe ser positivo.
        if (dto.Amount <= 0)
            throw new InvalidOperationException("El monto debe ser mayor que cero.");

        var transaction = new Transaction
        {
            Amount = dto.Amount,
            Date = dto.Date,
            Description = dto.Description,
            Type = dto.Type,
            CategoryId = dto.CategoryId
        };

        var created = await _transactionRepository.CreateAsync(transaction);

        // Volvemos a leer para traer la navegación (Category) cargada.
        var withCategory = await _transactionRepository.GetByIdAsync(created.Id);
        return ToDto(withCategory!);
    }

    public async Task<TransactionDto?> UpdateAsync(int id, TransactionUpdateDto dto)
    {
        var existing = await _transactionRepository.GetByIdAsync(id);
        if (existing is null)
            return null;

        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
        if (category is null)
            throw new InvalidOperationException(
                $"La categoría con Id {dto.CategoryId} no existe.");

        existing.Amount = dto.Amount;
        existing.Date = dto.Date;
        existing.Description = dto.Description;
        existing.Type = dto.Type;
        existing.CategoryId = dto.CategoryId;

        var updated = await _transactionRepository.UpdateAsync(existing);

        // Recargamos para traer la categoría actualizada.
        var withCategory = await _transactionRepository.GetByIdAsync(updated.Id);
        return ToDto(withCategory!);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _transactionRepository.DeleteAsync(id);
    }

    /// <summary>
    /// Conversión Entity → DTO. 
    /// Nota: usamos t.Category?.Name porque la navegación puede ser null.
    /// </summary>
    private static TransactionDto ToDto(Transaction transaction)
    {
        return new TransactionDto
        {
            Id = transaction.Id,
            Amount = transaction.Amount,
            Date = transaction.Date,
            Description = transaction.Description,
            Type = transaction.Type,
            CategoryId = transaction.CategoryId,
            CategoryName = transaction.Category?.Name ?? string.Empty
        };
    }


}