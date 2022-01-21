using Xunit;
using Models;
using CustomExceptions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tests;

public class ModelsTest{
    [Fact]
    public void UserShouldCreate(){
        //Arrange
        //To test this case I make sure im using the models namespace
        
        //Act: Create user object
        User testUser = new User();

        //Assert: Assert that the user was created
        Assert.NotNull(testUser);
    }

    [Fact]
    public void UserShouldSetValue(){
        //Arrange
        User testUser = new User();
        int ID = 42;
        string username = "Jon";
        string password = "banana";
        List<ProductOrder> shoppingCart = new List<ProductOrder>();
        List<StoreOrder> finishedOrders = new List<StoreOrder>();

        //Act
        testUser.ID = ID;
        testUser.Username = username;
        testUser.Password = password;
        testUser.ShoppingCart = shoppingCart;
        testUser.FinishedOrders = finishedOrders;

        //Assert
        Assert.Equal(ID, testUser.ID);
        Assert.Equal(username, testUser.Username);
        Assert.Equal(password, testUser.Password);
        Assert.Equal(shoppingCart, testUser.ShoppingCart);
        Assert.Equal(finishedOrders, testUser.FinishedOrders);
    }

    [Fact]
    public void ProductShouldCreateAndSetValues(){
        //Arrange
        Product testProduct = new Product();
        int ID = 99;
        int storeID = 9994;
        string name = "Iphone 12";
        string description = "Brand new";
        decimal price = 99;
        int quantity = 4;

        //Act
        testProduct.ID = ID;
        testProduct.storeID = storeID;
        testProduct.Name = name;
        testProduct.Description = description;
        testProduct.Price = price;
        testProduct.Quantity = quantity;

        //Assert
        Assert.Equal(ID, testProduct.ID);
        Assert.Equal(storeID, testProduct.storeID);
        Assert.Equal(name, testProduct.Name);
        Assert.Equal(description, testProduct.Description);
        Assert.Equal(price, testProduct.Price);
        Assert.Equal(quantity, testProduct.Quantity);
    }

    // [Theory]
    // [InlineData(-50.9, -4)]
    // [InlineData(-30, -1)]
    // [InlineData(0, -6)]
    // public void ProductShouldNotHaveInvalidQuantityorPrice(decimal price, int quantity){
    //     //Arrange: Testing if the product will have an invalid price or quantity. 
    //     //Price should be a decimal greater than 0
    //     //Quantity should be an integer greater than or equal to 0
    //     Product testProduct = new Product();

    //     //Act: Using inline data as paramaters
    //     //Assert
    //     Assert.Throws<InputInvalidException>(() => testProduct.Price = price);
    //     Assert.Throws<InputInvalidException>(() => testProduct.Quantity = quantity);
    // }

    [Fact]
    public void StoreShouldCreate(){
        //Arrange
        //To test this case I make sure im using the models namespace
        
        //Act: Create store object
        Store testStore = new Store();

        //Assert: Assert that the store was created
        Assert.NotNull(testStore);
    }

    [Fact]
    public void StoreShouldSetValue(){
        //Arrange
        Store testStore = new Store();
        string name = "Atlantic Branch";
        string address = "500 Seven Farms Dr";
        string city = "Charleston";
        string state = "SC";
        List<Product> products = new List<Product>();
        List<StoreOrder> allOrders = new List<StoreOrder>();

        //Act
        testStore.Name = name;
        testStore.Address = address;
        testStore.City = city;
        testStore.State = state;
        testStore.Products = products;
        testStore.AllOrders = allOrders;

        //Assert
        Assert.Equal(name, testStore.Name);
        Assert.Equal(address, testStore.Address);
        Assert.Equal(city, testStore.City);
        Assert.Equal(state, testStore.State);
        Assert.Equal(products, testStore.Products);
        Assert.Equal(allOrders, testStore.AllOrders);
    }

    [Fact]
    public void ProductOrderShouldSetValue(){
        //Arrange
        ProductOrder testPOrder = new ProductOrder();
        int ID = 4242;
        int userID = 99333;
        int storeID = 9994;
        int productID = 32332423;
        string itemName = "Pixel 4";
        decimal TotalPrice = 999.99M;
        int Quantity = 87;

        //Act
        testPOrder.ID = ID;
        testPOrder.userID = userID;
        testPOrder.storeID = storeID;
        testPOrder.productID = productID;
        testPOrder.ItemName = itemName;
        testPOrder.TotalPrice = TotalPrice;
        testPOrder.Quantity = Quantity;

        //Assert
        Assert.Equal(ID, testPOrder.ID);
        Assert.Equal(userID, testPOrder.userID);
        Assert.Equal(storeID, testPOrder.storeID);
        Assert.Equal(productID, testPOrder.productID);
        Assert.Equal(itemName, testPOrder.ItemName);
        Assert.Equal(TotalPrice, testPOrder.TotalPrice);
        Assert.Equal(Quantity, testPOrder.Quantity);   
    }

    // [Theory]
    // [InlineData(0)]
    // [InlineData(-1)]
    // [InlineData(-15)]
    // public void ProductOrderShouldNotHaveInvalidQuantity(int quantity){
    //     //Arrange: Testing if the product will have an invalid quantity. 
    //     //Quantity should greater than 0
    //     ProductOrder testPOrder = new ProductOrder();

    //     //Act: Using inline data as paramaters

    //     //Assert
    //     Assert.Throws<InputInvalidException>(() => testPOrder.Quantity = quantity);
    // }

    [Fact]
    public void StoreOrderShouldSetValue(){
        //Arrange
        StoreOrder testStoreOrder = new StoreOrder();
        int ID = 9032;
        int userID = 222935;
        int referenceID = 222935;
        string currDate = "12/30/21 5:43PM";
        double DateSeconds = 999432.3377;
        decimal TotalAmount = 3829.98M;
        List<ProductOrder> Orders = new List<ProductOrder>();
        
        //Act
        testStoreOrder.ID = ID;
        testStoreOrder.userID = userID;
        testStoreOrder.referenceID = referenceID;
        testStoreOrder.currDate = currDate;
        testStoreOrder.DateSeconds = DateSeconds;
        testStoreOrder.TotalAmount = TotalAmount;
        testStoreOrder.Orders = Orders;

        //Assert
        Assert.Equal(ID, testStoreOrder.ID);
        Assert.Equal(userID, testStoreOrder.userID);
        Assert.Equal(currDate, testStoreOrder.currDate);
        Assert.Equal(DateSeconds, testStoreOrder.DateSeconds);
        Assert.Equal(TotalAmount, testStoreOrder.TotalAmount);
        Assert.Equal(Orders, testStoreOrder.Orders);
    }
    [Fact]
    public void UserShouldSetShoppingCart(){
        //Arrange
        User testUser = new User();
        List<ProductOrder> shoppingCart = new List<ProductOrder>();
        ProductOrder testPOrder1 = new ProductOrder();
        ProductOrder testPOrder2 = new ProductOrder();
        List<ProductOrder> testCart = new List<ProductOrder>();

        //Act
        int PID1 = 12121;
        int PID2 = 895673;
        int userID = 9933;
        int storeID = 322;
        int productID1 = 563223523;
        int productID2 = 9324243;
        string PName1 = "Galaxy S10";
        string PName2 = "IPhone 12 Pro";
        decimal PTotalPrice1 = 1998.99M;
        decimal PTotalPrice2 = 992.25M;
        int PQuantity1 = 2;
        int PQuantity2 = 3;
        //Product Order 1
        testPOrder1.ID = PID1;
        testPOrder1.userID = userID;
        testPOrder1.storeID = storeID;
        testPOrder1.productID = productID1;
        testPOrder1.ItemName = PName1;
        testPOrder1.TotalPrice = PTotalPrice1;
        testPOrder1.Quantity = PQuantity1;
        //Product Order 2
        testPOrder2.ID = PID2;
        testPOrder2.userID = userID;
        testPOrder2.storeID = storeID;
        testPOrder2.productID = productID2;
        testPOrder2.ItemName = PName2;
        testPOrder2.TotalPrice = PTotalPrice2;
        testPOrder2.Quantity = PQuantity2;
        //Sets empty shopping cart
        testUser.ShoppingCart = shoppingCart;
        //Adding items to cart
        testUser.ShoppingCart.Add(testPOrder1);
        testUser.ShoppingCart.Add(testPOrder2);
        testCart.Add(testPOrder1);
        testCart.Add(testPOrder2);

        //Assert the two list of product orders equals each other
        Assert.Equal(testCart, testUser.ShoppingCart);
    }
}