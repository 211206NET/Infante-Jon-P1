using CustomExceptions;
namespace Models;

public class Product {
    

    public Product(){}

    public Product(DataRow r){
        ID = (int) r["ID"];
        storeID = (int) r["storeID"];
        Name = r["Name"].ToString() ?? "";
        Description = r["Description"].ToString() ?? "";
        Price = (decimal) r["Price"];
        Quantity = (int) r["Quantity"];
}

    public int? ID { get; set; }

    public int? storeID {get; set; }
    
    public string? Name { get; set;}

    public string? Description {get; set;}

    private decimal _price;
    public decimal Price{ 
        
        get => _price;
        
        set {
            //checks if value is less than or equal to 0
            if (value <= 0){
                throw new InputInvalidException("Price must be greater than 0. Please enter a valid amount:");
            }   
            this._price = value;
            }
        }

    private int? _quantity;
    public int? Quantity{ 
        
        get => _quantity;
        
        set {
            //checks if value is less than 0
            if (value < 0){
                throw new InputInvalidException("Quantity must be 0 or higher. Please enter a valid amount:");
                }       
            this._quantity = value;
        }
    }
}