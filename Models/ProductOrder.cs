using CustomExceptions;
namespace Models;

public class ProductOrder {
    

    public ProductOrder(){}

    public ProductOrder(DataRow r){
        ID = (int) r["ID"];
        userID = (int) r["userID"];
        storeID = (int) r["storeID"];
        storeOrderID = (int) r["storeOrderID"];
        userOrderID = (int) r["userOrderID"];
        productID = (int)r["productID"];
        ItemName = r["ItemName"].ToString() ?? "";
        TotalPrice = (decimal)r["TotalPrice"];
        Quantity = (int) r["Quantity"];
    }

    public int? ID { get; set; }

    public int? userID { get; set; }

    public int? storeID { get; set; }
    
    public int? storeOrderID { get; set;}

    public int? userOrderID { get; set; }
    
    public int? productID { get; set; }

    public string? ItemName { get; set; }

    public decimal TotalPrice { get; set; }

    private int? _quantity;
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0. Please enter a valid amount.")]
    public int? Quantity{ 
        
        get => _quantity;
        
        set {
/*            if (value <= 0){
                throw new InputInvalidException("\nQuantity must be greater than 0. Please enter a valid amount:");
            }   */                
            
            this._quantity = value;
        }
    }


}