using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Models;
using BL;
using DL;
using CustomExceptions;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductOrderController : Controller
    {
        private ISBL _sbl;
        private IUBL _iubl;
        public ProductOrderController(IUBL iubl, ISBL sbl)
        {
            _iubl = iubl;
            _sbl = sbl;
        }
        // GET api/<ProductOrderController>/username
        // Gets a user's entire shopping cart
        [HttpGet("{username}")]
        public ActionResult<List<ProductOrder>> GetShoppingCart(string username)
        {
            List<ProductOrder> shoppingCart = _iubl.GetAllProductOrders(username);
            //The shopping cart is not empty
            if (shoppingCart.Count != 0)
            {
                return Ok(shoppingCart);
            }
            //No product orders found in the user's cart
            else
            {
                return NoContent();
            }
        }
        // GET api/<ProductOrderController>/id
        // Gets an individual product order
        [HttpGet("{id:int}")]
        public ActionResult<ProductOrder> GetProductOrder(int id)
        {
            ProductOrder pOrder = _iubl.GetProductOrder(id);
            if (pOrder.ID != null)
            {
                return Ok(pOrder);
            }
            else
            {
                return NoContent();
            }
        }
        // POST api/<ProductOrderController>
        // Adds a product order to the selected store
        [HttpPost]
        public ActionResult Post(string username, int prodID, int quantity)
        {
            Product currProdSelected = _iubl.GetProductByID(prodID);
            //Valid product found
            if(currProdSelected.ID != null)
            {
                try
                {
                    _iubl.AddProductOrder(username, prodID, quantity);
                    //Checking if we can edit the current product's quantity from the product order
                    Product currProduct = _sbl.GetProductByID(prodID);
                    //Updates the current proucts quantity
                    currProduct.Quantity = currProduct.Quantity - quantity;
                    _sbl.EditProduct(prodID, currProduct);
                }
                catch (InputInvalidException ex)
                {
                    return Conflict(ex.Message);
                }
            return Ok("Successfully added a product order to your cart");
            }
            //No product found with inputted id
            else
            {
                return NoContent();
            }

        }
        // PUT api/<ProductOrderController>/id
        // Edits a product selected by id with a new quantity
        [HttpPut("{id}")]
        public ActionResult Put(int id, int quantity)
        {
            try
            {
                ProductOrder pOrder = _iubl.GetProductOrder(id);
                //Product Order not found
                if (pOrder.ID == null)
                {
                    return NoContent();
                }
                _iubl.EditProductOrder(id, quantity, 0, 0);
                //Checking if we can edit the current product's quantity from the product order
                Product currProduct = _sbl.GetProductByID((int)pOrder.productID!);
                //Updates the current proucts quantity
                currProduct.Quantity = currProduct.Quantity + pOrder.Quantity - quantity;
                _sbl.EditProduct((int)pOrder.productID, currProduct);
            }
            //We couldn't update the product order due to invalid quantity amount
            catch (InputInvalidException ex)
            {
                return Conflict(ex.Message);
            }
            //The 0s in place are for storeOrderId and userOrderId with are both 0 because we have not checked out
            return Ok();

        }
        // DELETE api/<ProductOrderController>/id
        // Deletes a product order from the store
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            ProductOrder pOrder = _iubl.GetProductOrder(id);
            if (pOrder.ID != null)
            {
                //Updates the product's quantity with the amount that was deleted
                Product currProduct = _sbl.GetProductByID((int)pOrder.productID!);
                currProduct.Quantity = currProduct.Quantity + pOrder.Quantity;
                _sbl.EditProduct((int)pOrder.productID, currProduct);
                //Deletes the product order from the user's cart
                _iubl.DeleteProductOrder(id);
                return Ok("Product order has been deleted");
            }
            //We couldn't find any product orders with that id
            else
            {
                return NoContent();
            }

        }
    }
}