using Xunit;
using Moq;
using WebAPI.Controllers;
using BL;
using DL;
using Models;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc;
using System;


 namespace Tests;

 public class ControllerTest
 {
    [Fact]
    public void ProductControllerGetShouldGetAllProdctsByStoreId()
    {
        //Arrange, Act, Assert pattern
        //Gets called, we instead return a stubbed data
        var mockBL = new Mock<ISBL>();
        int i = 1;
        mockBL.Setup(x => x.GetAllProducts(i)).Returns(
                new List<Product>
                {
                     new Product
                     {
                         ID = 1,
                         storeID = i,
                         Name = "IPhone 13 Pro",
                         Description = "Unused",
                         Price = 999.99M,
                         Quantity = 8
                     },
                     new Product
                     {
                         ID = 2,
                         storeID = i,
                         Name = "Pixel 3",
                         Description = "Partially used",
                         Price = 899.99M,
                         Quantity = 4
                     },
                }
            );
        var productCtrllr = new ProductController(mockBL.Object);

        //Act
        var result = productCtrllr.Get(i);

        //Assert
        Assert.NotNull(result);
        Assert.IsType<ActionResult<List<Product>>>(result);
        //Assert.Equal(200, result.StatusCode);
    }
    [Fact] public void ProductControllerShouldGetProductById()
    {
        //Arrange
        var mockBL = new Mock<ISBL>();
        int i = 1;
        mockBL.Setup(x => x.GetProductByID(i)).Returns(
            new Product
            {                        
                ID = 1,
                storeID = i,
                Name = "IPhone 13 Pro",
                Description = "Unused",
                Price = 999.99M,
                Quantity = 8
            });
        var productCtrllr = new ProductController(mockBL.Object);
        //Act
        var result = productCtrllr.GetProduct(i);

        //Assert
        Assert.NotNull(result);
        Assert.IsType<ActionResult<Product>>(result);
    }
    public interface ICanTestIUandIS : IUBL, ISBL { };

    [Fact]
    public void ProductOrderControllerShouldGetShoppingCartByUsername()
    {
        //Arrange, Act, Assert pattern
        //Gets called, we instead return a stubbed data
        var mockBL = new Mock<ICanTestIUandIS>();
        string username = "Jon";
        mockBL.Setup(x => x.GetAllProductOrders(username)).Returns(
                new List<ProductOrder>
                {
                     new ProductOrder
                     {
                         ID = 1,
                         userID = 2,
                         storeID = 4,
                         storeOrderID = 0,
                         userOrderID = 2,
                         productID = 10,
                         ItemName = "IPad Pro 4",
                         TotalPrice = 2098.98M,
                         Quantity = 4
                     },
                     new ProductOrder
                     {
                         ID = 1,
                         userID = 2,
                         storeID = 4,
                         storeOrderID = 0,
                         userOrderID = 2,
                         productID = 12,
                         ItemName = "MacBook Pro 2016",
                         TotalPrice = 1699.99M,
                         Quantity = 1
                     }
                }
            ) ;
        var productOrderCtrllr = new ProductOrderController(mockBL.Object, mockBL.Object);

        //Act
        var result = productOrderCtrllr.GetShoppingCart(username);

        //Assert
        Assert.NotNull(result);
        Assert.IsType<ActionResult<List<ProductOrder>>>(result);
    }
    [Fact]
    public void ProductOrderControllerShouldGetProducOrdertById()
    {
        //Arrange
        var mockBL = new Mock<ICanTestIUandIS>();
        int i = 1;
        mockBL.Setup(x => x.GetProductOrder(i)).Returns(
                     new ProductOrder
                     {
                         ID = 1,
                         userID = 2,
                         storeID = 4,
                         storeOrderID = 0,
                         userOrderID = 2,
                         productID = 20,
                         ItemName = "Samsung Galaxy S10",
                         TotalPrice = 1099.99M,
                         Quantity = 1
                     });
        var productOrderCtrllr = new ProductOrderController(mockBL.Object, mockBL.Object);
        //Act
        var result = productOrderCtrllr.GetProductOrder(i);

        //Assert
        Assert.NotNull(result);
        Assert.IsType<ActionResult<ProductOrder>>(result);
    }
    [Fact]
    public void StoreControllerShouldGetAllStores()
    {
        //Arrange
        var mockBL = new Mock<ISBL>();
        mockBL.Setup(x => x.GetAllStores()).Returns(
            new List<Store>
                {
                     new Store
                     {
                         ID = 1,
                         Name = "Pacific Branch",
                         Address = "611 Jones St",
                         City = "SF",
                         State = "CA"
                     },
                     new Store
                     {
                         ID = 2,
                         Name = "Atlantic Branch",
                         Address = "500 Seven Farms Dr",
                         City = "CHS",
                         State = "SC"
                     },

                }
            );
        var storeCtrllr = new StoreController(mockBL.Object);
        //Act
        var result = storeCtrllr.GetStore();

        //Assert
        Assert.NotNull(result);
        Assert.IsType<ActionResult<List<Store>>>(result);
    }
    [Fact]
    public void StoreControllerShouldGetStoreById()
    {
        //Arrange
        var mockBL = new Mock<ISBL>();
        int i = 1;
        mockBL.Setup(x => x.GetStoreByID(i)).Returns(
                     new Store
                     {
                         ID = 5,
                         Name = "Midwest Branch",
                         Address = "101 Lake Dr",
                         City = "Cincinatti",
                         State = "OH"
                     }
            );
        var storeCtrllr = new StoreController(mockBL.Object);
        //Act
        var result = storeCtrllr.GetStoreById(i);

        //Assert
        Assert.NotNull(result);
        Assert.IsType<ActionResult<Store>>(result);
    }
    [Fact]
    public void UserControllerShouldGetAllUsers()
    {
        //Arrange
        var mockBL = new Mock<IUBL>();
        mockBL.Setup(x => x.GetAllUsers()).Returns(
            new List<User>
                {
                     new User
                     {
                         ID = 1,
                         Username = "Jon",
                         Password = "@#JKW$&Y^GSER#Q@$#QG"
                     },
                     new User
                     {
                         ID = 2,
                         Username = "Joey",
                         Password = "@#BHY#$%^@#$%HHAAHAHD/@#$%@#"
                     },

                }
            );
        var userCtrllr = new UserController(mockBL.Object);
        //Act
        var result = userCtrllr.GetUsers();

        //Assert
        Assert.NotNull(result);
        Assert.IsType<ActionResult<List<User>>>(result);
    }
    [Fact]
    public void UserControllerShouldGetUserByUsername()
    {
        //Arrange
        var mockBL = new Mock<IUBL>();
        string username = "Jon";
        mockBL.Setup(x => x.GetCurrentUserByUsername(username)).Returns(
                     new User
                     {
                         ID = 1,
                         Username = "Jon",
                         Password = "@AHDS&*#ASGSASAGG"
                     }                   
            );
        var userCtrllr = new UserController(mockBL.Object);
        //Act
        var result = userCtrllr.GetUserByUsername(username);

        //Assert
        Assert.NotNull(result);
        Assert.IsType<ActionResult<User>>(result);
    }

}
