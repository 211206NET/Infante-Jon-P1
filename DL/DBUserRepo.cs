namespace DL;

public class DBUserRepo : IURepo {

    private string _connectionString;
    public DBUserRepo(string connectionString){
        _connectionString = connectionString;
    }

    /// <summary>
    /// Adds a user object to the database
    /// </summary>
    /// <param name="userToAdd"></param>
    public void AddUser(User userToAdd){
        //Establishing new connection
        using SqlConnection connection = new SqlConnection(_connectionString);
        connection.Open();
        //Our insert command to add a user
        string sqlCmd = "INSERT INTO Customer (ID, Username, Password) VALUES (@ID, @username, @pass)"; 
        using SqlCommand cmdAddUser= new SqlCommand(sqlCmd, connection);
        //Adding paramaters
        Random rnd = new Random();
        int id = rnd.Next(1000000);
        cmdAddUser.Parameters.AddWithValue("@ID", id);
        cmdAddUser.Parameters.AddWithValue("@username", userToAdd.Username);
        string hashedPassword = PasswordHash.GenerateHashedPassword(userToAdd.Password!);
        cmdAddUser.Parameters.AddWithValue("@pass", hashedPassword);
        //Executing command
        cmdAddUser.ExecuteNonQuery();
        connection.Close();
        //Log user has been added
        Log.Information("The user {user} has been added to the database with the ID {ID}", userToAdd.Username, userToAdd.ID);
    }
    /// <summary>
    /// Search for the user for matching username
    /// </summary>
    /// <param name="username">username we are searching for a duplicate</param>
    /// <returns>bool: true if there is duplicate, false if not</returns>
    public bool IsDuplicate(string username)
    {
        string searchQuery = $"SELECT * FROM Customer WHERE Username= @username";

        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand cmd = new SqlCommand(searchQuery, connection);
        cmd.Parameters.AddWithValue("@username", username);


        connection.Open();

        using SqlDataReader reader = cmd.ExecuteReader();

        if (reader.HasRows)
        {
            //Query returned something, there exists a record that shares the same username 
            return true;
        }
        //no record was returned. No duplicate record in the db
        return false;
    }

    /// <summary>
    /// Gets the entire list of users from the database[named customer inside the database due to special naming by SQL]
    /// </summary>
    /// <returns>Returns a list of customer(user) objects</returns>
    public List<User> GetAllUsers(){
        List<User> allUsers = new List<User>();

        using SqlConnection connection = new SqlConnection(_connectionString);
        string customerSelect = "Select * From Customer";
        string productOrderSelect = "Select * From ProductOrder";
        string storeOrderSelect = "Select * From StoreOrder";
        
        //A single dataSet to hold all our data
        DataSet usSet = new DataSet();

        //Three different adapters for different tables
        using SqlDataAdapter customerAdapter = new SqlDataAdapter(customerSelect, connection);
        using SqlDataAdapter productOrderAdapter = new SqlDataAdapter(productOrderSelect, connection);
        using SqlDataAdapter storeOrderAdapter = new SqlDataAdapter(storeOrderSelect, connection);

        //Filling the Dataset with each table
        customerAdapter.Fill(usSet, "Customer");
        productOrderAdapter.Fill(usSet, "ProductOrder");
        storeOrderAdapter.Fill(usSet, "StoreOrder");

        //Declaring each data table from the dataset
        DataTable? customerTable = usSet.Tables["Customer"];
        DataTable? productOrderTable = usSet.Tables["ProductOrder"];
        DataTable? storeOrderTable = usSet.Tables["StoreOrder"];

        if(customerTable != null){   
            foreach(DataRow row in customerTable.Rows){
                //Use customer constructor with DataRow object to quickly create user with parameters
                User user = new User(row);

                //Assigns each product order corresponding to the current user
                if (productOrderTable != null){
                    user.ShoppingCart = productOrderTable.AsEnumerable().Where(r => (int) r["userID"] == user.ID && (int)r["storeOrderID"] == 0).Select(
                        r => new ProductOrder(r)
                    ).ToList();
                }
                //Assigns each store order corresponding to the current user [uses reference id to match user id]
                if (storeOrderTable != null){
                    user.FinishedOrders = storeOrderTable.AsEnumerable().Where(r => (int) r["referenceID"] == user.ID).Select(
                        r => new StoreOrder(r)
                    ).ToList();
                }
                //Adds each product order to each store order in the list of users
                if(productOrderTable != null){
                    foreach(StoreOrder storeOrder in user.FinishedOrders!){
                        storeOrder.Orders = productOrderTable!.AsEnumerable().Where(r => (int) r["userOrderID"] == storeOrder.ID).Select(
                            r => new ProductOrder(r)
                        ).ToList();
                        }
                    }  
                //Add each store to the list of users
                allUsers.Add(user);
            }
        }
        return allUsers;
    }

    /// <summary>
    /// Gets the current user from the list of users by name
    /// </summary>
    /// <param name="username">current usename selected</param>
    /// <returns>User Object</returns>
    public User GetCurrentUserByUsername(string username){
        List<User> allUsers = GetAllUsers();
        foreach(User user in allUsers){
            if(user.Username == username){
                return user;
            }
        }
        //User not found
        return new User();
    }
    /// <summary>
    /// Checks if a user has entered a valid username and password
    /// </summary>
    /// <param name="username">username entered</param>
    /// <param name="password">password enetered</param>
    /// <returns>A boolean value true if logged in, false if invalid password</returns>
    public bool LoginUser(string username, string password)
    {
        User currUser = GetCurrentUserByUsername(username);
        //Hashes the entered password and checks if it matches the user's hashed password
        if (PasswordHash.VeryifyHashedPassword(password, currUser.Password!))
        {
            return true;
        }
        return false;
    }
    /// <summary>
    /// Returns a product by the ID given
    /// </summary>
    /// <param name="prodID">product object ID selected</param>
    /// <returns>Product Object</returns>
    public Product GetProductByID(int prodID)
    {
        string query = "SELECT * From Product WHERE ID = @ID";
        using SqlConnection connection = new SqlConnection(_connectionString);
        connection.Open();
        using SqlCommand cmd = new SqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@ID", prodID);

        using SqlDataReader reader = cmd.ExecuteReader();
        Product selectedProduct = new Product();
        if (reader.Read())
        {
            selectedProduct.ID = reader.GetInt32(0);
            selectedProduct.storeID = reader.GetInt32(1);
            selectedProduct.Name = reader.GetString(2);
            selectedProduct.Description = reader.GetString(3);
            selectedProduct.Price = reader.GetDecimal(4);
            selectedProduct.Quantity = reader.GetInt32(5);
        }
        connection.Close();
        return selectedProduct;
    }

    /// <summary>
    /// Adds a product order to the user's shopping cart inside the database
    /// </summary>
    /// <param name="userID">current user ID inputted</param>
    /// <param name="prodID">The selected product we are trying to order</param>
    /// <param name="quantity">The quantity we want to buy of that product</param>

    public void AddProductOrder(string username, int prodID, int quantity){
        Product currProd = GetProductByID(prodID);
        //If a user tried to order a quantity less than 0
        if (quantity <= 0)
        {
            throw new InputInvalidException("New quantity must be greater than 0");
        }
        //If the quantity goes over the amount the user can buy from the store
        int newStoreQuantity = (int)currProd.Quantity! - quantity;
        if (newStoreQuantity < 0)
        {
            throw new InputInvalidException($"The maximum number of {currProd.Name} you can order is {currProd.Quantity}");
        }
        //Establishing new connection
        using SqlConnection connection = new SqlConnection(_connectionString);
        connection.Open();
        //Our insert command to add a product order
        string sqlCmd = "INSERT INTO ProductOrder (ID, userID, storeID, storeOrderID, userOrderID, productID, ItemName, TotalPrice, Quantity) VALUES (@ID, @userID, @storeID, @storeOrderID, @userOrderID, @productID, @itemName, @totPrice, @qty)"; 
        using SqlCommand cmdAddProductOrder= new SqlCommand(sqlCmd, connection);
        //Adding paramaters
        //Getting the current product information by id
        Product currProduct = GetProductByID(prodID);
        //Getting a new random id for the product order
        Random rnd = new Random();
        int id = rnd.Next(1000000);
        cmdAddProductOrder.Parameters.AddWithValue("@ID", id);
        //Gets the current user id from the username
        User currUser = GetCurrentUserByUsername(username);
        int userID = (int)currUser.ID!;
        cmdAddProductOrder.Parameters.AddWithValue("@userID", userID);
        cmdAddProductOrder.Parameters.AddWithValue("@storeID", currProduct.storeID);
        cmdAddProductOrder.Parameters.AddWithValue("@storeOrderID", 0);
        cmdAddProductOrder.Parameters.AddWithValue("@userOrderID", 0);
        cmdAddProductOrder.Parameters.AddWithValue("@productID", currProduct.ID);
        cmdAddProductOrder.Parameters.AddWithValue("@itemName", currProduct.Name);
        //Updating total price
        decimal totPrice = currProduct.Price * quantity;
        cmdAddProductOrder.Parameters.AddWithValue("@totPrice", totPrice);
        cmdAddProductOrder.Parameters.AddWithValue("@qty", quantity);
        //Executing command
        cmdAddProductOrder.ExecuteNonQuery();
        connection.Close();
        Log.Information("A product order of {quantity} {itemName}s has been placed with the ID of {ID} to the user ID of {currUserID}'s shopping cart",quantity, currProduct.Name, id, userID);
        }
    /// <summary>
    /// Gets all the current product orders in a user's shopping cart
    /// </summary>
    /// <param name="username">username inputted to get shopping cart from</param>
    /// <returns>A List of product orders(line items)</returns>
    public List<ProductOrder> GetAllProductOrders(string username)
    {
        List<ProductOrder> shoppingCart = new List<ProductOrder>();
        string selectProdCmd = "SELECT * FROM ProductOrder WHERE userID = @userID AND storeOrderID = @0";
        SqlConnection connection = new SqlConnection(_connectionString);
        SqlCommand cmd = new SqlCommand(selectProdCmd, connection);
        //Gets the current user's id from the username
        User currUser = GetCurrentUserByUsername(username);
        int userID = (int)currUser.ID!;
        cmd.Parameters.AddWithValue("@userID", userID);
        cmd.Parameters.AddWithValue("@0", 0);

        DataSet productOrderSet = new DataSet();

        SqlDataAdapter adapter = new SqlDataAdapter(cmd);

        adapter.Fill(productOrderSet, "productOrder");

        DataTable productOrderTable = productOrderSet.Tables["productOrder"]!;
        if (productOrderTable != null)
        {
            //Adds each of the product orders we find to the cart to return
            foreach (DataRow row in productOrderTable.Rows)
            {
                ProductOrder pOrder = new ProductOrder(row);
                shoppingCart.Add(pOrder);
            }
        }

        return shoppingCart;
    }
    /// <summary>
    /// Gets the current selected product order by id
    /// </summary>
    /// <param name="prodOrderID"></param>
    /// <returns></returns>
    public ProductOrder GetProductOrder(int prodOrderID)
    {
        string query = "SELECT * From ProductOrder WHERE ID = @ID";
        using SqlConnection connection = new SqlConnection(_connectionString);
        connection.Open();
        using SqlCommand cmd = new SqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@ID", prodOrderID);

        using SqlDataReader reader = cmd.ExecuteReader();
        ProductOrder selectedProduct = new ProductOrder();
        if (reader.Read())
        {
            selectedProduct.ID = reader.GetInt32(0);
            selectedProduct.userID = reader.GetInt32(1);
            selectedProduct.storeID = reader.GetInt32(2);
            selectedProduct.storeOrderID = reader.GetInt32(3);
            selectedProduct.userOrderID = reader.GetInt32(4);
            selectedProduct.productID = reader.GetInt32(5);
            selectedProduct.ItemName = reader.GetString(6);
            selectedProduct.TotalPrice = reader.GetDecimal(7);
            selectedProduct.Quantity = reader.GetInt32(8);
        }
        connection.Close();
        return selectedProduct;
    }

    /// <summary>
    /// Edits a selected product order in the user's shopping cart, and saves it back to the database
    /// </summary>
    /// <param name="prodOrderID">ID of the product order selected to edit</param>
    /// <param name="quantity">New quantity to update</param>
    /// <param name="storeOrderID">Edit's the store order id when we checkout the cart</param>
    /// <param name="userOrderID">Edit's the user's order id when we checkout the cart</param>
    public void EditProductOrder(int prodOrderID, int quantity, int storeOrderID, int userOrderID){
        ProductOrder currPOrder = GetProductOrder(prodOrderID);
        Product currProd = GetProductByID((int)currPOrder.productID!);
        //If a user tried to order a quantity less than 0
        if (quantity <= 0)
        {
            throw new InputInvalidException("New quantity must be greater than 0");
        }
        //If the quantity goes over the amount the user can buy from the store
        int totalProductQuantity = (int)currProd.Quantity! + (int)currPOrder.Quantity!;
        int newStoreQuantity =  totalProductQuantity - quantity;
        if (newStoreQuantity < 0)
        {
            throw new InputInvalidException($"The maximum number of {currProd.Name} you can order is {totalProductQuantity}");
        }
        using SqlConnection connection = new SqlConnection(_connectionString);
        connection.Open();
        //Updates a single product by id, and passed in requirements
        string sqlEditCmd = $"UPDATE ProductOrder SET Quantity = @qty, TotalPrice = @tPrice, storeOrderID = @sOrderID, userOrderID = @uOrderID WHERE ID = @prodOrderID";
        using SqlCommand cmdEditProdOrder = new SqlCommand(sqlEditCmd, connection);
        //Adds the paramaters to the sql command
        cmdEditProdOrder.Parameters.AddWithValue("@qty", quantity);
        //Calculating new total price
        decimal totPrice = (((decimal)currPOrder.TotalPrice/(int)currPOrder.Quantity!)*quantity);
        cmdEditProdOrder.Parameters.AddWithValue("@tPrice", totPrice);
        cmdEditProdOrder.Parameters.AddWithValue("@prodOrderID", prodOrderID);
        cmdEditProdOrder.Parameters.AddWithValue("@sOrderID", storeOrderID);
        cmdEditProdOrder.Parameters.AddWithValue("@uOrderID", userOrderID);
        //Edits the current product selected
        cmdEditProdOrder.ExecuteNonQuery();
        connection.Close();
        //If the user order id is 0, that means the product order is still in the cart.
        //When the user checks out, the user order id is changed to match for the store orders and we don't want to call the edit product logger
        if (userOrderID == 0){
            Log.Information("The product order with ID {ID} has been edited with a new quantity of {quantity}",prodOrderID, quantity);
        }
    }

    /// <summary>
    /// Remove a Product Order from the user's shopping cart and database
    /// </summary>
    /// <param name="prodOrderID">the product order ID of the product order we have selected</param>
    public void DeleteProductOrder(int prodOrderID){
        using SqlConnection connection = new SqlConnection(_connectionString);
        connection.Open();
        //Deletes a single product by id
        string sqlDelCmd = $"DELETE FROM ProductOrder WHERE ID = @prodOrderID";
        using SqlCommand cmdDelProdOrder = new SqlCommand(sqlDelCmd, connection);
        cmdDelProdOrder.Parameters.AddWithValue("@prodOrderID", prodOrderID);
        //Deletes the current product selected
        cmdDelProdOrder.ExecuteNonQuery();
        connection.Close();
        Log.Information("The product order with ID {ID} has been deleted from the user's shopping cart", prodOrderID);

    }

    /// <summary>
    /// Adds a store order to the database
    /// </summary>
    /// <param name="username">current user selected by username</param>
    /// <param name="currStoreOrder">storeOrder object to add to previous orders</param>
    public void AddStoreOrder(string username, StoreOrder currStoreOrder){
        using SqlConnection connection = new SqlConnection(_connectionString);
        connection.Open();
        string sqlInsertCmd = "INSERT INTO StoreOrder (ID, userID, userName, referenceID, storeID, currDate, DateSeconds, TotalAmount) VALUES (@ID, @uID, @username, @refID, @stID, @date, @dateS, @tAmount)";
        //Creates the new sql command
        using SqlCommand cmd = new SqlCommand(sqlInsertCmd, connection);
        //Adds the paramaters to the insert command
        cmd.Parameters.AddWithValue("@ID", currStoreOrder.ID);
        cmd.Parameters.AddWithValue("@uID", currStoreOrder.userID);
        cmd.Parameters.AddWithValue("@username", currStoreOrder.userName);
        cmd.Parameters.AddWithValue("@refID", currStoreOrder.referenceID);
        cmd.Parameters.AddWithValue("@stID", currStoreOrder.storeID);
        cmd.Parameters.AddWithValue("@date", currStoreOrder.currDate);
        cmd.Parameters.AddWithValue("@dateS", currStoreOrder.DateSeconds);
        cmd.Parameters.AddWithValue("@tAmount", currStoreOrder.TotalAmount);
        //Executes the insert command
        cmd.ExecuteNonQuery();
        connection.Close();
        if (currStoreOrder.storeID != 0)
        {
            Log.Information("The user {currUsername} has checked out and created a new store order with ID {ID} with a total amount of ${totAmount}", username, currStoreOrder.ID, currStoreOrder.TotalAmount);
        }
        else
        {
            Log.Information("An order has been added to the store with an id of {storeid} from the user {username} with a total amount of ${totAmount}", currStoreOrder.storeID, username, currStoreOrder.TotalAmount);
        }
    }

    public void Checkout(string username)
    {
        //Get the current user's shopping cart with a list of product orders
        List<ProductOrder> shoppingCart = GetAllProductOrders(username);
        //Gets the current user's ID
        int userID = (int)GetCurrentUserByUsername(username).ID!;
        //Gets the current total amount of the entire shopping cart
        decimal cartTotal = 0;
        foreach(ProductOrder pOrder in shoppingCart)
        {
            cartTotal += pOrder.TotalPrice;
        }
        //Getting the current date for and random id for storeOrder paramaters
        Random rnd = new Random();
        int cartid = rnd.Next(1000000);
        string currTime = DateTime.Now.ToString();
        double currTimeSeconds = DateTime.Now.Subtract(DateTime.MinValue).TotalSeconds;
        StoreOrder userStoreOrder = new StoreOrder
        {
            ID = cartid!,
            userID = userID,
            userName = username,
            referenceID = userID,
            storeID = 0,
            TotalAmount = cartTotal,
            currDate = currTime!,
            DateSeconds = currTimeSeconds!,
        };
        //Adds to current user's store order list
        AddStoreOrder(username, userStoreOrder);
        //Get each corresponding store from each product's ID and add to a dictionary
        Dictionary<int, List<ProductOrder>> storeOrdersToPlace = new Dictionary<int, List<ProductOrder>>();
        foreach (ProductOrder pOrder in shoppingCart)
        {
            //Getting the ID of the current store from the product id's string id code
            int currStoreID = (int)pOrder.storeID!;
            if (storeOrdersToPlace.ContainsKey(currStoreID))
            {
                storeOrdersToPlace[currStoreID].Add(pOrder);
            }
            //If there is no key found
            else
            {
                List<ProductOrder> listP = new List<ProductOrder>();
                listP.Add(pOrder);
                //Assigns the initial first item to a new dictionary key (by store index, list of product orders)
                storeOrdersToPlace.Add(currStoreID, listP);
            }
        }
        //Iterate over dictionary with store ids and corresponding product orders
        foreach (KeyValuePair<int, List<ProductOrder>> kv in storeOrdersToPlace)
        {
            //get new store Order id between 1 and 1,000,000
            int sid = rnd.Next(1000000);
            //calcuate total order value for list of product orders
            decimal StoreOrderTotalValue = 0;
            foreach (ProductOrder pOrd in kv.Value)
            {
                StoreOrderTotalValue += pOrd.TotalPrice!;
                //Declare a store order id for the checked out product to be accessed later by database
                //edit product order store order id to save as a reference to which product order this store belongs to
                EditProductOrder((int)pOrd.ID!, (int)pOrd.Quantity!, sid, cartid);
            }
            StoreOrder storeOrderToAdd = new StoreOrder
            {
                ID = sid!,
                userID = userID!,
                userName = username,
                referenceID = kv.Key,
                storeID = kv.Key,
                TotalAmount = StoreOrderTotalValue!,
                currDate = currTime!,
                DateSeconds = currTimeSeconds!,
            };
            //Adds store order to current selected store
            AddStoreOrder(username, storeOrderToAdd);
        }
        Log.Information("The user {user} has checked out their cart!", username);
    }
    public List<StoreOrder> GetStoreOrders(string username, string selection)
    {
        //Get the current user's id
        int userID = (int)GetCurrentUserByUsername(username).ID!;
        string selectStoreOrderCmd = "";
        //Sort by highest price first
        if (selection == "high")
        {
            selectStoreOrderCmd = "SELECT * FROM StoreOrder WHERE referenceID = @userID ORDER BY TotalAmount DESC";
        }
        //Sort by lowest price first
        else if (selection == "low")
        {
            selectStoreOrderCmd = "SELECT * FROM StoreOrder WHERE referenceID = @userID ORDER BY TotalAmount ASC";
        }
        //Sort by most recently ordered
        else if (selection == "new")
        {
            selectStoreOrderCmd = "SELECT * FROM StoreOrder WHERE referenceID = @userID ORDER BY DateSeconds DESC";
        }
        //Sort by last ordered
        else if (selection == "old")
        {
            selectStoreOrderCmd = "SELECT * FROM StoreOrder WHERE referenceID = @userID ORDER BY DateSeconds ASC";
        }
        //Incorrect ordering found, use default by how SQL orders it (by ID)
        else
        {
            selectStoreOrderCmd = "SELECT * FROM StoreOrder WHERE referenceID = @userID";
        }

        //Get all the product orders to fill each store order correctly
        string selectProductOrderCmd = "SELECT * FROM ProductOrder WHERE userID = @userID2";
        SqlConnection connection = new SqlConnection(_connectionString);

        SqlCommand sCmd = new SqlCommand(selectStoreOrderCmd, connection);
        sCmd.Parameters.AddWithValue("@userID", userID);
        SqlCommand pCmd = new SqlCommand(selectProductOrderCmd, connection);
        pCmd.Parameters.AddWithValue("@userID2", userID);

        DataSet sOrderSet = new DataSet();

        SqlDataAdapter storeOrderAdapter = new SqlDataAdapter(sCmd);
        SqlDataAdapter prodOrderAdapter = new SqlDataAdapter(pCmd);

        storeOrderAdapter.Fill(sOrderSet, "storeOrder");
        prodOrderAdapter.Fill(sOrderSet, "productOrder");

        DataTable storeOrderTable = sOrderSet.Tables["storeOrder"]!;
        DataTable productOrderTable = sOrderSet.Tables["productOrder"]!;

        List<StoreOrder> allOrders = new List<StoreOrder>();
        foreach (DataRow row in storeOrderTable.Rows)
        {
            //Create a new store order object retrieved from the database
            StoreOrder storeOrder = new StoreOrder(row);
            //Add each product order to the corresponding store order
            if (productOrderTable != null)
            {
                storeOrder.Orders = productOrderTable!.AsEnumerable().Where(r => (int)r["userOrderID"] == storeOrder.ID).Select(
                    r => new ProductOrder(r)
                ).ToList();
            }
            allOrders.Add(storeOrder);
        }
        return allOrders;
    }

    //Unused with database implementation
    public void ClearShoppingCart(User currUser){}

    //Unused with database implementation
    public int GetCurrentUserIndexByID(int userID){
        return 0;
    }
}