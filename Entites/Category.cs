namespace GestorGastosApi.Entities;

//<sumary>
//Reprecenta una categori de gastos o de ingresos(Ej: Comida, Transporte, Sueldo).
//Esta clase se convierte en la tabla "Categorias" en el SQL Server
//</sumary>
public class Category
{  
    
    
    public int Id {get; set;}  //Identificador unico de la categoria. Es la clave primaria(Primary Key)
                               //EF Core reconoce una propiedad llamada  "Id" como clave primaria,
    public string Name{get; set;} = string.Empty;  //Nombre visible de la categoria (Ej: "Alimentacion , Transporte").

    public string Description{get; set;} = string.Empty;   //Descripcion opcional para datr mas detalles sobre la categoria.

    public List<Transaction> Transactions{get; set;} = new();    //Lissta de transacciones que pertenecen a esta categoria.
                                                                 //Esta relacion pertenece de 1 a N(Una categoria tiene muchas traansacciones).
                                                                 //No se convierte en una columna en la tabla Categorias. 



}
  
