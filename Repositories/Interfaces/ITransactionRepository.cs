using GestorGastosApi.Entities;

namespace GestorGastosApi.Repositories.Interfaces;

// Contrato del repositorio de transacciones.
public interface ITransactionRepository
{
    Task<IEnumerable<Transaction>> GetAllAsync();// Obtiene todas las transacciones, incluyendo los datos de su categoría.
    Task<Transaction?> GetByIdAsync(int id);// Obtiene una transacción por su Id, incluyendo su categoría.
    Task<IEnumerable<Transaction>> GetByCategoryIdAsync(int categoryId);// Obtiene las transacciones de una categoría específica.
    Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);// Obtiene las transacciones dentro de un rango de fechas.
    Task<Transaction> UpdateAsync(Transaction transaction);// Actualiza una transacción existente.
    Task<bool> DeleteAsync(int id);// Elimina una transacción por su Id.
    Task<Transaction> CreateAsync(Transaction transaction);
}