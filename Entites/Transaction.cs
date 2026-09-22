using GestorGastosApi.Entities;

namespace GestorGastosApi.Entities;

//<sumary>
//Reprecenta un movimiento de dinero(un ingreso o un gasto).
//Se convierte en la tabla "Transaction" en SQL Server.
//</sumary>
public class Transaction
{

    public int Id{get; set; }//Identificador unico de la transaccion. Clave primaria.

    public decimal Amount{get; set;}//Monto de transacciones. Usamos decimales(no double ni float).
                                    //es el tipo correcto para evitar errores de redondeo.
    public DateTime Date{get; set; }//Fecha en la que ocurrio la transaccion.
    
    public string Description{get; set; } = string.Empty;//Descripcion breve de la transaccion(Ej: Almuerzo en el trabajo)

    public TransactionType Type {get; set;}// Tipo de transacción: Income (ingreso) o Expense (gasto).
                                           // Se guardará como número (1 o 2) en la base de dato:
    public int CategoryId{get; set; }// Clave foránea (Foreign Key) que apunta a la categoría a la que pertenece.
                                     // Esta es la parte "muchos" de la relación 1 a N.
    public Category? Category{get; set;}// Propiedad de navegación hacia la categoría.
                                        // Propiedad de navegación hacia la categoría.(ej: transaction.Category.Name). NO se convierte en columna; solo existe en C#.

}