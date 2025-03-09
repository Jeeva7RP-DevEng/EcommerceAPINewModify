using ECommerce.API.Models;
using Microsoft.Data.SqlClient;

namespace ECommerce.API.DataAccess
{
    public class DataAccess : IDataAccess
    {
        private readonly IConfiguration _configuration;



        //public DataAccess(IConfiguration configuration)
        //{
        //    _configuration = configuration;
        //    _dbConnection = _configuration.GetConnectionString("DefaultConnection");
        //}

        private readonly string _dbConnection;

        public DataAccess(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _dbConnection = _configuration.GetConnectionString("DefaultConnection")
                            ?? throw new InvalidOperationException("Database connection string is missing.");
        }


        // ✅ Product Categories
        public List<ProductCategory> GetProductCategories()
        {
            var categories = new List<ProductCategory>();
            using (SqlConnection connection = new(_dbConnection))
            {
                string query = "SELECT * FROM ProductCategories";
                using SqlCommand command = new(query, connection);
                connection.Open();
                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    categories.Add(new ProductCategory
                    {
                        Id = (int)reader["Id"],
                        Category = reader["Name"].ToString() ?? "NO-NAME"
                    });
                }
            }
            return categories;
        }

        public ProductCategory GetProductCategory(int id)
        {
            ProductCategory category = null;
            using (SqlConnection connection = new(_dbConnection))
            {
                string query = "SELECT * FROM ProductCategories WHERE Id = @Id";
                using SqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@Id", id);
                connection.Open();
                using SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    category = new ProductCategory
                    {
                        Id = (int)reader["Id"],
                        Category = reader["Name"].ToString() ?? "NO-NAME",
                    };
                }
            }
            return category;
        }

        public string ProductCategoryAdd(string category, string subCategory)
        {
            using (SqlConnection connection = new(_dbConnection))
            {
                string query = "INSERT INTO ProductCategories (Name, SubCategory) VALUES (@Category, @SubCategory)";
                using SqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@Category", category);
                command.Parameters.AddWithValue("@SubCategory", subCategory);
                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0 ? "Category added successfully" : "Error adding category";
            }
        }

        // ✅ Products
        public Offer GetOffer(int id)
        {
            Offer offer = null;
            using (SqlConnection connection = new(_dbConnection))
            {
                string query = "SELECT * FROM Offers WHERE Id = @Id";
                using SqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@Id", id);
                connection.Open();
                using SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    offer = new Offer
                    {
                        Id = (int)reader["Id"],
                        Title = reader["Title"].ToString() ?? "NO-NAME",
                        Discount = (int)reader["Discount"]
                    };
                }
            }
            return offer;
        }

        public List<Product> GetProducts(string category, string subcategory, int count)
        {
            var products = new List<Product>();
            using (SqlConnection connection = new(_dbConnection))
            {
                string query = @"
                    SELECT TOP (@Count) p.*, c.Id AS CategoryId, c.Name AS CategoryName 
                    FROM Products p
                    JOIN ProductCategories c ON p.CategoryId = c.Id
                    WHERE c.Name = @Category AND c.SubCategory = @SubCategory
                    ORDER BY NEWID();";

                using SqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@Count", count);
                command.Parameters.AddWithValue("@Category", category);
                command.Parameters.AddWithValue("@SubCategory", subcategory);

                connection.Open();
                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    products.Add(new Product
                    {
                        Id = (int)reader["Id"],
                        Title = reader["Title"].ToString() ?? "NO-NAME",
                        Description = reader["Description"].ToString() ?? "NO-NAME",
                        Price = reader["Price"] != DBNull.Value ? Convert.ToDouble(reader["Price"]) : 0.00,
                        Quantity = (int)reader["Quantity"],
                        ImageName = reader["ImageName"].ToString() ?? "NO-NAME",
                        ProductCategory = new ProductCategory
                        {
                            Id = (int)reader["CategoryId"],
                            Category = reader["CategoryName"].ToString() ?? "NO-NAME"
                        }
                    });
                }
            }
            return products;
        }

        public Product GetProduct(int id)
        {
            Product product = null;
            using (SqlConnection connection = new(_dbConnection))
            {
                string query = "SELECT * FROM Products WHERE Id = @Id";
                using SqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@Id", id);
                connection.Open();
                using SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    product = new Product
                    {
                        Id = (int)reader["Id"],
                        Title = reader["Title"].ToString() ?? "NO-NAME",
                        Description = reader["Description"].ToString() ?? "NO-NAME",
                        Price = Convert.ToDouble(reader["Price"]),
                        Quantity = (int)reader["Quantity"],
                        ImageName = reader["ImageName"].ToString() ?? "NO-NAME"
                    };
                }
            }
            return product;
        }

        public List<Product> PutProduct(int id, float price, int quantity)
        {
            using (SqlConnection connection = new(_dbConnection))
            {
                connection.Open();

                // ✅ Update the product price and quantity
                string updateQuery = "UPDATE Products SET Price = @Price, Quantity = @Quantity WHERE Id = @Id";
                using (SqlCommand updateCommand = new(updateQuery, connection))
                {
                    updateCommand.Parameters.AddWithValue("@Id", id);
                    updateCommand.Parameters.AddWithValue("@Price", price);
                    updateCommand.Parameters.AddWithValue("@Quantity", quantity);
                    updateCommand.ExecuteNonQuery();
                }

                // ✅ Fetch updated product list
                return GetProducts("", "", 10);
            }
        }

        // ✅ Users
        public bool InsertUser(User user)
        {
            using (SqlConnection connection = new(_dbConnection))
            {
                string query = @"
                    INSERT INTO Users (FirstName, LastName, Email, Password, Address, Mobile, CreatedAt)
                    VALUES (@FirstName, @LastName, @Email, @Password, @Address, @Mobile, @CreatedAt);";

                using SqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@FirstName", user.FirstName);
                command.Parameters.AddWithValue("@LastName", user.LastName);
                command.Parameters.AddWithValue("@Email", user.Email);
                command.Parameters.AddWithValue("@Password", user.Password); // Hash password
                command.Parameters.AddWithValue("@Address", user.Address);
                command.Parameters.AddWithValue("@Mobile", user.Mobile);
                command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

                connection.Open();
                return command.ExecuteNonQuery() > 0;
            }
        }

        public User GetUser(int id)
        {
            User user = null;
            using (SqlConnection connection = new(_dbConnection))
            {
                string query = "SELECT * FROM Users WHERE Id = @Id";
                using SqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@Id", id);
                connection.Open();
                using SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    user = new User
                    {
                        Id = (int)reader["Id"],
                        FirstName = reader["FirstName"].ToString() ?? "NO-NAME",
                        LastName = reader["LastName"].ToString() ?? "NO-NAME",
                        Email = reader["Email"].ToString() ?? "NO-NAME",
                        Address = reader["Address"].ToString() ?? "NO-NAME",
                        Mobile = reader["Mobile"].ToString() ?? "NO-NAME"
                    };
                }
            }
            return user;
        }



        public void InsertReview(Review review)
        {
            using (SqlConnection connection = new(_dbConnection))
            {
                string query = "INSERT INTO Reviews (ProductId, UserId, Rating, Comment) VALUES (@ProductId, @UserId, @Rating, @Comment)";
                SqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@ProductId", review.Product.Id);
                command.Parameters.AddWithValue("@UserId", review.User.Id);
                command.Parameters.AddWithValue("@Rating", review.Product.Description);
                command.Parameters.AddWithValue("@Comment", review.Value);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        //public List<Review> GetProductReviews(int productId)
        //{
        //    List<Review> reviews = new();
        //    using (SqlConnection connection = new(_dbConnection))
        //    {
        //        string query = "SELECT * FROM Reviews WHERE ProductId = @ProductId";
        //        SqlCommand command = new(query, connection);
        //        command.Parameters.AddWithValue("@ProductId", productId);

        //        connection.Open();
        //        SqlDataReader reader = command.ExecuteReader();
        //        while (reader.Read())
        //        {
        //            reviews.Add(new Review
        //            {
        //                Id = (int)reader["Id"],
        //                Product = reader["ProductID"],
        //                User = (int)reader["UserId"] ?? "",
        //                Value = (int)reader["Rating"]
        //            });
        //        }
        //    }
        //    return reviews;
        //}



        public bool InsertCartItem(int userId, int productId)
        {
            using (SqlConnection connection = new(_dbConnection))
            {
                string query = "INSERT INTO CartItems (UserId, ProductId) VALUES (@UserId, @ProductId)";
                SqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@ProductId", productId);

                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }

        public int InsertPayment(Payment payment)
        {
            using (SqlConnection connection = new(_dbConnection))
            {
                string query = "INSERT INTO Payments (UserId, Amount, PaymentMethod) VALUES (@UserId, @Amount, @PaymentMethod); SELECT SCOPE_IDENTITY();";
                SqlCommand command = new(query, connection);
                command.Parameters.AddWithValue("@UserId", payment.User.Id);
                command.Parameters.AddWithValue("@Amount", payment.AmountPaid);
                command.Parameters.AddWithValue("@PaymentMethod", payment.PaymentMethod);

                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }




        public Cart GetActiveCartOfUser(int userid)
        {
            var cart = new Cart();
            using (SqlConnection connection = new(_dbConnection))
            {
                SqlCommand command = new()
                {
                    Connection = connection
                };
                connection.Open();

                string query = "SELECT COUNT(*) From Carts WHERE UserId=" + userid + " AND Ordered='false';";
                command.CommandText = query;

                int count = (int)command.ExecuteScalar();
                if (count == 0)
                {
                    return cart;
                }

                query = "SELECT CartId From Carts WHERE UserId=" + userid + " AND Ordered='false';";
                command.CommandText = query;

                int cartid = (int)command.ExecuteScalar();

                query = "select * from CartItems where CartId=" + cartid + ";";
                command.CommandText = query;

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    CartItem item = new()
                    {
                        Id = (int)reader["CartItemId"],
                        Product = GetProduct((int)reader["ProductId"])
                    };
                    cart.CartItems.Add(item);
                }

                cart.Id = cartid;
                cart.User = GetUser(userid);
                cart.Ordered = false;
                cart.OrderedOn = "";
            }
            return cart;
        }

        public List<Cart> GetAllPreviousCartsOfUser(int userid)
        {
            var carts = new List<Cart>();
            using (SqlConnection connection = new(_dbConnection))
            {
                SqlCommand command = new()
                {
                    Connection = connection
                };
                string query = "SELECT CartId FROM Carts WHERE UserId=" + userid + " AND Ordered='true';";
                command.CommandText = query;
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var cartid = (int)reader["CartId"];
                    carts.Add(GetCart(cartid));
                }
            }
            return carts;
        }

        public Cart GetCart(int cartid)
        {
            var cart = new Cart();
            using (SqlConnection connection = new(_dbConnection))
            {
                SqlCommand command = new()
                {
                    Connection = connection
                };
                connection.Open();

                string query = "SELECT * FROM CartItems WHERE CartId=" + cartid + ";";
                command.CommandText = query;

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    CartItem item = new()
                    {
                        Id = (int)reader["CartItemId"],
                        Product = GetProduct((int)reader["ProductId"])
                    };
                    cart.CartItems.Add(item);
                }
                reader.Close();

                query = "SELECT * FROM Carts WHERE CartId=" + cartid + ";";
                command.CommandText = query;
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    cart.Id = cartid;
                    cart.User = GetUser((int)reader["UserId"]);
                    cart.Ordered = bool.Parse((string)reader["Ordered"]);
                    cart.OrderedOn = (string)reader["OrderedOn"];
                }
                reader.Close();
            }
            return cart;
        }


        public List<PaymentMethod> GetPaymentMethods()
        {
            var result = new List<PaymentMethod>();
            using (SqlConnection connection = new(_dbConnection))
            {
                string query = "SELECT * FROM PaymentMethods;";
                using (SqlCommand command = new(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PaymentMethod paymentMethod = new()
                            {
                                Id = (int)reader["PaymentMethodId"],
                                Type = reader["Type"].ToString() ?? "CASH",
                                Provider = reader["Provider"].ToString() ?? "Bank",
                                Available = (bool)reader["Available"], // Correcting bool parsing
                                Reason = reader["Reason"].ToString() ?? "BUSY"
                            };
                            result.Add(paymentMethod);
                        }
                    }
                }
            }
            return result;
        }


        public void UpdateProduct(int id, float price, int quantity)
        {
            using (SqlConnection connection = new(_dbConnection))
            {
                string query = "UPDATE Products SET Price=@price, Quantity=@quantity WHERE ProductId=@id;";
                using (SqlCommand command = new(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@price", price);
                    command.Parameters.AddWithValue("@quantity", quantity);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }


        public int InsertOrder(Order order)
        {
            int orderId = 0;
            using (SqlConnection connection = new(_dbConnection))
            {
                string query = @"
            INSERT INTO Orders (UserId, CartId, PaymentId, CreatedAt) 
            VALUES (@uid, @cid, @pid, @createdAt);
            SELECT SCOPE_IDENTITY();"; // Use SCOPE_IDENTITY() instead of a separate query.

                using (SqlCommand command = new(query, connection))
                {
                    command.Parameters.AddWithValue("@uid", order.User.Id);
                    command.Parameters.AddWithValue("@cid", order.Cart.Id);
                    command.Parameters.AddWithValue("@pid", order.Payment.Id);
                    command.Parameters.AddWithValue("@createdAt", order.CreatedAt);

                    connection.Open();
                    orderId = Convert.ToInt32(command.ExecuteScalar());

                    if (orderId > 0)
                    {
                        string updateQuery = "UPDATE Carts SET Ordered=1, OrderedOn=@orderedOn WHERE CartId=@cartId;";
                        using (SqlCommand updateCommand = new(updateQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@orderedOn", DateTime.Now.ToString());
                            updateCommand.Parameters.AddWithValue("@cartId", order.Cart.Id);
                            updateCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
            return orderId;
        }

        public string AddProductCategory(string category, string subCategory)
        {
            using (SqlConnection connection = new(_dbConnection))
            {
                string query = "INSERT INTO ProductCategories (Category, SubCategory) VALUES (@category, @subCategory);";
                using (SqlCommand command = new(query, connection))
                {
                    command.Parameters.AddWithValue("@category", category);
                    command.Parameters.AddWithValue("@subCategory", subCategory);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            return $"New Category: {category}, SubCategory: {subCategory}";
        }


        public User IsUserPresent(string email, string password)
        {
            User user = null;

            using (SqlConnection connection = new(_dbConnection))
            using (SqlCommand command = new SqlCommand())
            {
                command.Connection = connection;
                connection.Open();

                command.CommandText = "SELECT * FROM Users WHERE Email = @email;";
                command.Parameters.Add("@email", System.Data.SqlDbType.NVarChar).Value = email;

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new User
                        {
                            Id = (int)reader["UserId"],
                            FirstName = (string)reader["FirstName"],
                            LastName = (string)reader["LastName"],
                            Email = (string)reader["Email"],
                            Address = (string)reader["Address"],
                            Mobile = (string)reader["Mobile"],
                            Password = (string)reader["Password"], // Hashed Password
                            CreatedAt = (string)reader["CreatedAt"],
                            ModifiedAt = (string)reader["ModifiedAt"]
                        };
                    }
                }
            }

            return user;
        }

        public List<Review> GetProductReviews(int productId)
        {
            throw new NotImplementedException();
        }
    }
}




//        


//        public DataAccess(IConfiguration configuration)
//        {
//            this.configuration = configuration;
//            this._dbConnection = configuration.GetConnectionString("DefaultConnection");
//        }

//        // Implementing the missing methods


//        public void InsertReview(Review review)
//        {
//            using (SqlConnection connection = new(_dbConnection))
//            {
//                string query = "INSERT INTO Reviews (ProductId, UserId, Rating, Comment) VALUES (@ProductId, @UserId, @Rating, @Comment)";
//                SqlCommand command = new(query, connection);
//                command.Parameters.AddWithValue("@ProductId", review.Product);
//                command.Parameters.AddWithValue("@UserId", review.User.Id);
//                command.Parameters.AddWithValue("@Rating", review.Value);
//                command.Parameters.AddWithValue("@Comment", review.Value);

//                connection.Open();
//                command.ExecuteNonQuery();
//            }
//        }





//        public bool InsertCartItem(int userId, int productId)
//        {
//            using (SqlConnection connection = new(_dbConnection))
//            {
//                string query = "INSERT INTO CartItems (UserId, ProductId) VALUES (@UserId, @ProductId)";
//                SqlCommand command = new(query, connection);
//                command.Parameters.AddWithValue("@UserId", userId);
//                command.Parameters.AddWithValue("@ProductId", productId);

//                connection.Open();
//                int rowsAffected = command.ExecuteNonQuery();
//                return rowsAffected > 0;
//            }
//        }

//        public int InsertPayment(Payment payment)
//        {
//            using (SqlConnection connection = new(_dbConnection))
//            {
//                string query = "INSERT INTO Payments (UserId, Amount, PaymentMethod) VALUES (@UserId, @Amount, @PaymentMethod); SELECT SCOPE_IDENTITY();";
//                SqlCommand command = new(query, connection);
//                command.Parameters.AddWithValue("@UserId", payment.User);
//                command.Parameters.AddWithValue("@Amount", payment.AmountPaid);
//                command.Parameters.AddWithValue("@PaymentMethod", payment.PaymentMethod);

//                connection.Open();
//                return Convert.ToInt32(command.ExecuteScalar());
//            }
//        }

//        
//    }

//}






