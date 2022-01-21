using Xunit;
using Moq;
using WebAPI.Controllers;
using BL;
using DL;
using Models;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc;


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
}