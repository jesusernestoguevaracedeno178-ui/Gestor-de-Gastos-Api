using GestorGastosApi.Data;
using GestorGastosApi.Entities;
using GestorGastosApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GestorGastosApi.Repositories;

// <summary>
// Implementación del repositorio de transacciones usando EF Core.
// </summary>
public class TransactionRepository : ITransactionRepository
{
    private readonly GestorGastosDbContext _context;

    public TransactionRepository(GestorGastosDbContext context)
    {
        _context = context;
    }

    
    public async Task<IEnumerable<Transaction>> GetAllAsync()
    {
        // .Include() carga la categoría relacionada (equivale a un JOIN).
        return await _context.Transactions
            .Include(t => t.Category)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Transaction?> GetByIdAsync(int id)
    {
        return await _context.Transactions
            .Include(t => t.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Transaction>> GetByCategoryIdAsync(int categoryId)
    {
        return await _context.Transactions
            .Include(t => t.Category)
            .AsNoTracking()
            .Where(t => t.CategoryId == categoryId)   // Equivale a WHERE CategoryId = @id
            .ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Transactions
            .Include(t => t.Category)
            .AsNoTracking()
            .Where(t => t.Date >= startDate && t.Date <= endDate)
            .OrderByDescending(t => t.Date)   // Más recientes primero
            .ToListAsync();
    }

    public async Task<Transaction> CreateAsync(Transaction transaction)
    {
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<Transaction> UpdateAsync(Transaction transaction)
    {
        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction is null)
            return false;

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();
        return true;
    }

}
