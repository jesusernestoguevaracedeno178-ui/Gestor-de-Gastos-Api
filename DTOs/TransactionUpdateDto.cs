using GestorGastosApi.Entities;

namespace GestorGastosApi.DTOs;

// <summary>
// DTO de entrada para actualizar una transacción existente.
// </summary>
public class TransactionUpdateDto
{
    public int Id{get; set;}
    public decimal Amount{get; set;}
    public DateTime Date{get; set; }
    public string Description {get; set;} = string.Empty;
    public TransactionType Type{get; set;}
    public int CategoryId{get; set;}
}