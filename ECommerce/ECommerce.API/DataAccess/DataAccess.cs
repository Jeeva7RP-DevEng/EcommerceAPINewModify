using System;
using System.Collections.Generic;
using System.Data;
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

        public string IsUserPresent(string email, string password)
        {
            throw new NotImplementedException();
        }
    }
}


//using ECommerce.API.Models;
//using Microsoft.IdentityModel.Tokens;
//using System.Collections.Generic;
//using System.Data.Common;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;
//using Microsoft.Data.SqlClient; // ✅ Updated to use the recommended package
//using System.Text.RegularExpressions;

//namespace ECommerce.API.DataAccess

//{
//    public class DataAccess : IDataAccess
//    {
//        private readonly IConfiguration configuration;
//        private readonly string dbconnection;

//        public List<Product> PutProduct(int id, float price, int quantity)
//        {
//            List<Product> products = new();

//            using (SqlConnection connection = new(dbconnection))
//            {
//                connection.Open();

//                // ✅ Update the product price and quantity
//                string updateQuery = "UPDATE Products SET Price = @Price, Quantity = @Quantity WHERE Id = @Id";
//                using (SqlCommand updateCommand = new(updateQuery, connection))
//                {
//                    updateCommand.Parameters.AddWithValue("@Id", id);
//                    updateCommand.Parameters.AddWithValue("@Price", price);
//                    updateCommand.Parameters.AddWithValue("@Quantity", quantity);
//                    updateCommand.ExecuteNonQuery();
//                }

//                // ✅ Fetch updated product list with proper ProductCategory mapping
//                string selectQuery = @"
//            SELECT p.Id, p.Name, p.Price, p.Quantity, p.Description, p.ImageName, 
//                   c.Id AS CategoryId, c.Name AS CategoryName 
//            FROM Products p
//            JOIN ProductCategories c ON p.CategoryId = c.Id";  // Ensure this join is correct

//                using (SqlCommand selectCommand = new(selectQuery, connection))
//                using (SqlDataReader reader = selectCommand.ExecuteReader())
//                {
//                    while (reader.Read())
//                    {
//                        products.Add(new Product
//                        {
//                            Id = (int)reader["Id"],
//                            Title = reader["Name"].ToString() ?? "NO-NAME",
//                            Price = Convert.ToDouble(reader["Price"]),
//                            Quantity = (int)reader["Quantity"],
//                            Description = reader["Description"]?.ToString() ?? "N/A",
//                            ImageName = reader["ImageName"]?.ToString() ?? "",

//                            // ✅ Correctly map ProductCategory
//                            ProductCategory = new ProductCategory
//                            {
//                                Id = (int)reader["CategoryId"],
//                                Category = reader["CategoryName"].ToString() ?? "NO-NAME",
//                                SubCategory = reader["SubCategory"].ToString() ?? "N/A"   
//                            }
//                        });
//                    }
//                }
//            }

//            return products;
//        }


//        public DataAccess(IConfiguration configuration)
//        {
//            this.configuration = configuration;
//            this.dbconnection = configuration.GetConnectionString("DefaultConnection");
//        }

//        // Implementing the missing methods


//        public void InsertReview(Review review)
//        {
//            using (SqlConnection connection = new(dbconnection))
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
//            using (SqlConnection connection = new(dbconnection))
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
//            using (SqlConnection connection = new(dbconnection))
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

//        public string ProductCategoryAdd(string category, string subCategory)
//        {
//            using (SqlConnection connection = new(dbconnection))
//            {
//                string query = "INSERT INTO ProductCategories (Category, SubCategory) VALUES (@Category, @SubCategory)";
//                SqlCommand command = new(query, connection);
//                command.Parameters.AddWithValue("@Category", category);
//                command.Parameters.AddWithValue("@SubCategory", subCategory);

//                connection.Open();
//                int rowsAffected = command.ExecuteNonQuery();
//                return rowsAffected > 0 ? "Category added successfully" : "Error adding category";
//            }
//        }
//    }

//}



////{
////    public class DataAccess : IDataAccess
////    {
////        public class DataAccess : IDataAccess
////{
////    private readonly IConfiguration configuration;
////    private readonly string dbconnection;

////    public DataAccess(IConfiguration configuration)
////    {
////        this.configuration = configuration;
////        this.dbconnection = configuration.GetConnectionString("DefaultConnection");
////    }

////    // Implementing the missing methods

////    public List<Product> PutProduct(int id, float price, int quantity)
////    {
////        using (SqlConnection connection = new(dbconnection))
////        {
////            string query = "UPDATE Products SET Price = @Price, Quantity = @Quantity WHERE Id = @Id";
////            SqlCommand command = new(query, connection);
////            command.Parameters.AddWithValue("@Price", price);
////            command.Parameters.AddWithValue("@Quantity", quantity);
////            command.Parameters.AddWithValue("@Id", id);

////            connection.Open();
////            command.ExecuteNonQuery();

////            return GetProducts("", "", 10); // Example: Returning updated products
////        }
////    }

////    public void InsertReview(Review review)
////    {
////        using (SqlConnection connection = new(dbconnection))
////        {
////            string query = "INSERT INTO Reviews (ProductId, UserId, Rating, Comment) VALUES (@ProductId, @UserId, @Rating, @Comment)";
////            SqlCommand command = new(query, connection);
////            command.Parameters.AddWithValue("@ProductId", review.ProductId);
////            command.Parameters.AddWithValue("@UserId", review.UserId);
////            command.Parameters.AddWithValue("@Rating", review.Rating);
////            command.Parameters.AddWithValue("@Comment", review.Comment);

////            connection.Open();
////            command.ExecuteNonQuery();
////        }
////    }

////    public List<Review> GetProductReviews(int productId)
////    {
////        List<Review> reviews = new();
////        using (SqlConnection connection = new(dbconnection))
////        {
////            string query = "SELECT * FROM Reviews WHERE ProductId = @ProductId";
////            SqlCommand command = new(query, connection);
////            command.Parameters.AddWithValue("@ProductId", productId);

////            connection.Open();
////            SqlDataReader reader = command.ExecuteReader();
////            while (reader.Read())
////            {
////                reviews.Add(new Review
////                {
////                    Id = (int)reader["Id"],
////                    ProductId = (int)reader["ProductId"],
////                    UserId = (int)reader["UserId"],
////                    Rating = (int)reader["Rating"],
////                    Comment = reader["Comment"].ToString()
////                });
////            }
////        }
////        return reviews;
////    }

////    public User GetUser(int id)
////    {
////        User user = null;
////        using (SqlConnection connection = new(dbconnection))
////        {
////            string query = "SELECT * FROM Users WHERE UserId = @UserId";
////            SqlCommand command = new(query, connection);
////            command.Parameters.AddWithValue("@UserId", id);

////            connection.Open();
////            SqlDataReader reader = command.ExecuteReader();
////            if (reader.Read())
////            {
////                user = new User
////                {
////                    Id = (int)reader["UserId"],
////                    FirstName = reader["FirstName"].ToString(),
////                    LastName = reader["LastName"].ToString(),
////                    Email = reader["Email"].ToString(),
////                    Address = reader["Address"].ToString(),
////                    Mobile = reader["Mobile"].ToString()
////                };
////            }
////        }
////        return user;
////    }

////    public bool InsertCartItem(int userId, int productId)
////    {
////        using (SqlConnection connection = new(dbconnection))
////        {
////            string query = "INSERT INTO CartItems (UserId, ProductId) VALUES (@UserId, @ProductId)";
////            SqlCommand command = new(query, connection);
////            command.Parameters.AddWithValue("@UserId", userId);
////            command.Parameters.AddWithValue("@ProductId", productId);

////            connection.Open();
////            int rowsAffected = command.ExecuteNonQuery();
////            return rowsAffected > 0;
////        }
////    }

////    public int InsertPayment(Payment payment)
////    {
////        using (SqlConnection connection = new(dbconnection))
////        {
////            string query = "INSERT INTO Payments (UserId, Amount, PaymentMethod) VALUES (@UserId, @Amount, @PaymentMethod); SELECT SCOPE_IDENTITY();";
////            SqlCommand command = new(query, connection);
////            command.Parameters.AddWithValue("@UserId", payment.UserId);
////            command.Parameters.AddWithValue("@Amount", payment.Amount);
////            command.Parameters.AddWithValue("@PaymentMethod", payment.Method);

////            connection.Open();
////            return Convert.ToInt32(command.ExecuteScalar());
////        }
////    }

////    public string ProductCategoryAdd(string category, string subCategory)
////    {
////        using (SqlConnection connection = new(dbconnection))
////        {
////            string query = "INSERT INTO ProductCategories (Category, SubCategory) VALUES (@Category, @SubCategory)";
////            SqlCommand command = new(query, connection);
////            command.Parameters.AddWithValue("@Category", category);
////            command.Parameters.AddWithValue("@SubCategory", subCategory);

////            connection.Open();
////            int rowsAffected = command.ExecuteNonQuery();
////            return rowsAffected > 0 ? "Category added successfully" : "Error adding category";
////        }
////    }
////}

////        private readonly IConfiguration configuration;
////        private readonly string dbconnection;
////        private readonly string dateformat;
////        public DataAccess(IConfiguration configuration)
////        {
////            this.configuration = configuration;
////            dbconnection = this.configuration["ConnectionStrings:DB"];
////            dateformat = this.configuration["Constants:DateFormat"];
////        }

////        public Cart GetActiveCartOfUser(int userid)
////        {
////            var cart = new Cart();
////            using (SqlConnection connection = new(dbconnection))
////            {
////                SqlCommand command = new()
////                {
////                    Connection = connection
////                };
////                connection.Open();

////                string query = "SELECT COUNT(*) From Carts WHERE UserId=" + userid + " AND Ordered='false';";
////                command.CommandText = query;

////                int count = (int)command.ExecuteScalar();
////                if (count == 0)
////                {
////                    return cart;
////                }

////                query = "SELECT CartId From Carts WHERE UserId=" + userid + " AND Ordered='false';";
////                command.CommandText = query;

////                int cartid = (int)command.ExecuteScalar();

////                query = "select * from CartItems where CartId=" + cartid + ";";
////                command.CommandText = query;

////                SqlDataReader reader = command.ExecuteReader();
////                while (reader.Read())
////                {
////                    CartItem item = new()
////                    {
////                        Id = (int)reader["CartItemId"],
////                        Product = GetProduct((int)reader["ProductId"])
////                    };
////                    cart.CartItems.Add(item);
////                }

////                cart.Id = cartid;
////                cart.User = GetUser(userid);
////                cart.Ordered = false;
////                cart.OrderedOn = "";
////            }
////            return cart;
////        }

////        public List<Cart> GetAllPreviousCartsOfUser(int userid)
////        {
////            var carts = new List<Cart>();
////            using (SqlConnection connection = new(dbconnection))
////            {
////                SqlCommand command = new()
////                {
////                    Connection = connection
////                };
////                string query = "SELECT CartId FROM Carts WHERE UserId=" + userid + " AND Ordered='true';";
////                command.CommandText = query;
////                connection.Open();
////                SqlDataReader reader = command.ExecuteReader();
////                while (reader.Read())
////                {
////                    var cartid = (int)reader["CartId"];
////                    carts.Add(GetCart(cartid));
////                }
////            }
////            return carts;
////        }

////        public Cart GetCart(int cartid)
////        {
////            var cart = new Cart();
////            using (SqlConnection connection = new(dbconnection))
////            {
////                SqlCommand command = new()
////                {
////                    Connection = connection
////                };
////                connection.Open();

////                string query = "SELECT * FROM CartItems WHERE CartId=" + cartid + ";";
////                command.CommandText = query;

////                SqlDataReader reader = command.ExecuteReader();
////                while (reader.Read())
////                {
////                    CartItem item = new()
////                    {
////                        Id = (int)reader["CartItemId"],
////                        Product = GetProduct((int)reader["ProductId"])
////                    };
////                    cart.CartItems.Add(item);
////                }
////                reader.Close();

////                query = "SELECT * FROM Carts WHERE CartId=" + cartid + ";";
////                command.CommandText = query;
////                reader = command.ExecuteReader();
////                while (reader.Read())
////                {
////                    cart.Id = cartid;
////                    cart.User = GetUser((int)reader["UserId"]);
////                    cart.Ordered = bool.Parse((string)reader["Ordered"]);
////                    cart.OrderedOn = (string)reader["OrderedOn"];
////                }
////                reader.Close();
////            }
////            return cart;
////        }

////        public Offer GetOffer(int id)
////        {
////            var offer = new Offer();
////            using (SqlConnection connection = new(dbconnection))
////            {
////                SqlCommand command = new()
////                {
////                    Connection = connection
////                };

////                string query = "SELECT * FROM Offers WHERE OfferId=" + id + ";";
////                command.CommandText = query;

////                connection.Open();
////                SqlDataReader r = command.ExecuteReader();
////                while (r.Read())
////                {
////                    offer.Id = (int)r["OfferId"];
////                    offer.Title = (string)r["Title"];
////                    offer.Discount = (int)r["Discount"];
////                }
////            }
////            return offer;
////        }

////        public List<PaymentMethod> GetPaymentMethods()
////        {
////            var result = new List<PaymentMethod>();
////            using (SqlConnection connection = new(dbconnection))
////            {
////                string query = "SELECT * FROM PaymentMethods;";
////                using (SqlCommand command = new(query, connection))
////                {
////                    connection.Open();
////                    using (SqlDataReader reader = command.ExecuteReader())
////                    {
////                        while (reader.Read())
////                        {
////                            PaymentMethod paymentMethod = new()
////                            {
////                                Id = (int)reader["PaymentMethodId"],
////                                Type = reader["Type"].ToString(),
////                                Provider = reader["Provider"].ToString(),
////                                Available = (bool)reader["Available"], // Correcting bool parsing
////                                Reason = reader["Reason"].ToString()
////                            };
////                            result.Add(paymentMethod);
////                        }
////                    }
////                }
////            }
////            return result;
////        }

////        public Product GetProduct(int id)
////        {
////            var product = new Product();
////            using (SqlConnection connection = new(dbconnection))
////            {
////                string query = "SELECT * FROM Products WHERE ProductId=@id;";
////                using (SqlCommand command = new(query, connection))
////                {
////                    command.Parameters.AddWithValue("@id", id);
////                    connection.Open();
////                    using (SqlDataReader reader = command.ExecuteReader())
////                    {
////                        if (reader.Read())
////                        {
////                            product.Id = (int)reader["ProductId"];
////                            product.Title = reader["Title"].ToString();
////                            product.Description = reader["Description"].ToString();
////                            product.Price = Convert.ToDouble(reader["Price"]);
////                            product.Quantity = (int)reader["Quantity"];
////                            product.ImageName = reader["ImageName"].ToString();

////                            int categoryid = (int)reader["CategoryId"];
////                            product.ProductCategory = GetProductCategory(categoryid);

////                            int offerid = reader["OfferId"] != DBNull.Value ? (int)reader["OfferId"] : 0;
////                            product.Offer = GetOffer(offerid);
////                        }
////                    }
////                }
////            }
////            return product;
////        }

////        public void UpdateProduct(int id, float price, int quantity)
////        {
////            using (SqlConnection connection = new(dbconnection))
////            {
////                string query = "UPDATE Products SET Price=@price, Quantity=@quantity WHERE ProductId=@id;";
////                using (SqlCommand command = new(query, connection))
////                {
////                    command.Parameters.AddWithValue("@id", id);
////                    command.Parameters.AddWithValue("@price", price);
////                    command.Parameters.AddWithValue("@quantity", quantity);

////                    connection.Open();
////                    command.ExecuteNonQuery();
////                }
////            }
////        }

////        public List<ProductCategory> GetProductCategories()
////        {
////            var productCategories = new List<ProductCategory>();
////            using (SqlConnection connection = new(dbconnection))
////            {
////                string query = "SELECT * FROM ProductCategories;";
////                using (SqlCommand command = new(query, connection))
////                {
////                    connection.Open();
////                    using (SqlDataReader reader = command.ExecuteReader())
////                    {
////                        while (reader.Read())
////                        {
////                            var category = new ProductCategory()
////                            {
////                                Id = (int)reader["CategoryId"],
////                                Category = reader["Category"].ToString(),
////                                SubCategory = reader["SubCategory"].ToString()
////                            };
////                            productCategories.Add(category);
////                        }
////                    }
////                }
////            }
////            return productCategories;
////        }

////        public ProductCategory GetProductCategory(int id)
////        {
////            var productCategory = new ProductCategory();
////            using (SqlConnection connection = new(dbconnection))
////            {
////                string query = "SELECT * FROM ProductCategories WHERE CategoryId=@id;";
////                using (SqlCommand command = new(query, connection))
////                {
////                    command.Parameters.AddWithValue("@id", id);
////                    connection.Open();
////                    using (SqlDataReader reader = command.ExecuteReader())
////                    {
////                        if (reader.Read())
////                        {
////                            productCategory.Id = (int)reader["CategoryId"];
////                            productCategory.Category = reader["Category"].ToString();
////                            productCategory.SubCategory = reader["SubCategory"].ToString();
////                        }
////                    }
////                }
////            }
////            return productCategory;
////        }

////        public List<Product> GetProducts(string category, string subcategory, int count)
////        {
////            var products = new List<Product>();
////            using (SqlConnection connection = new(dbconnection))
////            {
////                string query = @"
////            SELECT TOP (@count) * FROM Products 
////            WHERE CategoryId=(SELECT CategoryId FROM ProductCategories WHERE Category=@category AND SubCategory=@subcategory) 
////            ORDER BY NEWID();";

////                using (SqlCommand command = new(query, connection))
////                {
////                    command.Parameters.AddWithValue("@count", count);
////                    command.Parameters.AddWithValue("@category", category);
////                    command.Parameters.AddWithValue("@subcategory", subcategory);

////                    connection.Open();
////                    using (SqlDataReader reader = command.ExecuteReader())
////                    {
////                        while (reader.Read())
////                        {
////                            var product = new Product()
////                            {
////                                Id = (int)reader["ProductId"],
////                                Title = reader["Title"].ToString(),
////                                Description = reader["Description"].ToString(),
////                                Price = Convert.ToDouble(reader["Price"]),
////                                Quantity = (int)reader["Quantity"],
////                                ImageName = reader["ImageName"].ToString(),
////                                ProductCategory = GetProductCategory((int)reader["CategoryId"]),
////                                Offer = GetOffer(reader["OfferId"] != DBNull.Value ? (int)reader["OfferId"] : 0)
////                            };
////                            products.Add(product);
////                        }
////                    }
////                }
////            }
////            return products;
////        }

////        public int InsertOrder(Order order)
////        {
////            int orderId = 0;
////            using (SqlConnection connection = new(dbconnection))
////            {
////                string query = @"
////            INSERT INTO Orders (UserId, CartId, PaymentId, CreatedAt) 
////            VALUES (@uid, @cid, @pid, @createdAt);
////            SELECT SCOPE_IDENTITY();"; // Use SCOPE_IDENTITY() instead of a separate query.

////                using (SqlCommand command = new(query, connection))
////                {
////                    command.Parameters.AddWithValue("@uid", order.User.Id);
////                    command.Parameters.AddWithValue("@cid", order.Cart.Id);
////                    command.Parameters.AddWithValue("@pid", order.Payment.Id);
////                    command.Parameters.AddWithValue("@createdAt", order.CreatedAt);

////                    connection.Open();
////                    orderId = Convert.ToInt32(command.ExecuteScalar());

////                    if (orderId > 0)
////                    {
////                        string updateQuery = "UPDATE Carts SET Ordered=1, OrderedOn=@orderedOn WHERE CartId=@cartId;";
////                        using (SqlCommand updateCommand = new(updateQuery, connection))
////                        {
////                            updateCommand.Parameters.AddWithValue("@orderedOn", DateTime.Now.ToString(dateformat));
////                            updateCommand.Parameters.AddWithValue("@cartId", order.Cart.Id);
////                            updateCommand.ExecuteNonQuery();
////                        }
////                    }
////                }
////            }
////            return orderId;
////        }

////        public string AddProductCategory(string category, string subCategory)
////        {
////            using (SqlConnection connection = new(dbconnection))
////            {
////                string query = "INSERT INTO ProductCategories (Category, SubCategory) VALUES (@category, @subCategory);";
////                using (SqlCommand command = new(query, connection))
////                {
////                    command.Parameters.AddWithValue("@category", category);
////                    command.Parameters.AddWithValue("@subCategory", subCategory);

////                    connection.Open();
////                    command.ExecuteNonQuery();
////                }
////            }
////            return $"New Category: {category}, SubCategory: {subCategory}";
////        }

////        public bool InsertUser(User user)
////        {
////            using (SqlConnection connection = new(dbconnection))
////            using (SqlCommand command = new SqlCommand())
////            {
////                command.Connection = connection;
////                connection.Open();

////                // Check if the email already exists
////                command.CommandText = "SELECT COUNT(*) FROM Users WHERE Email = @email;";
////                command.Parameters.Add("@email", System.Data.SqlDbType.NVarChar).Value = user.Email;

////                int count = (int)command.ExecuteScalar();
////                if (count > 0)
////                {
////                    return false;
////                }

////                // Hash the password before inserting
////                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);

////                command.CommandText = @"INSERT INTO Users 
////                                (FirstName, LastName, Address, Mobile, Email, Password, CreatedAt, ModifiedAt) 
////                                VALUES (@fn, @ln, @add, @mb, @em, @pwd, @cat, @mat);";

////                command.Parameters.Add("@fn", System.Data.SqlDbType.NVarChar).Value = user.FirstName;
////                command.Parameters.Add("@ln", System.Data.SqlDbType.NVarChar).Value = user.LastName;
////                command.Parameters.Add("@add", System.Data.SqlDbType.NVarChar).Value = user.Address;
////                command.Parameters.Add("@mb", System.Data.SqlDbType.NVarChar).Value = user.Mobile;
////                command.Parameters.Add("@pwd", System.Data.SqlDbType.NVarChar).Value = hashedPassword;
////                command.Parameters.Add("@cat", System.Data.SqlDbType.DateTime).Value = DateTime.UtcNow;
////                command.Parameters.Add("@mat", System.Data.SqlDbType.DateTime).Value = DateTime.UtcNow;

////                int rowsAffected = command.ExecuteNonQuery();
////                return rowsAffected > 0;
////            }
////        }

////        public string IsUserPresent(string email, string password)
////        {
////            User user = null;

////            using (SqlConnection connection = new(dbconnection))
////            using (SqlCommand command = new SqlCommand())
////            {
////                command.Connection = connection;
////                connection.Open();

////                command.CommandText = "SELECT * FROM Users WHERE Email = @email;";
////                command.Parameters.Add("@email", System.Data.SqlDbType.NVarChar).Value = email;

////                using (SqlDataReader reader = command.ExecuteReader())
////                {
////                    if (reader.Read())
////                    {
////                        user = new User
////                        {
////                            Id = (int)reader["UserId"],
////                            FirstName = (string)reader["FirstName"],
////                            LastName = (string)reader["LastName"],
////                            Email = (string)reader["Email"],
////                            Address = (string)reader["Address"],
////                            Mobile = (string)reader["Mobile"],
////                            Password = (string)reader["Password"], // Hashed Password
////                            CreatedAt = (string)reader["CreatedAt"],
////                            ModifiedAt = (string)reader["ModifiedAt"]
////                        };
////                    }
////                }
////            }

////            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
////            {
////                return "";
////            }

////            // Generate JWT token
////            string key = "MNU66iBl3T5rh6H52i69";
////            string duration = "60";
////            var symmetrickey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
////            var credentials = new SigningCredentials(symmetrickey, SecurityAlgorithms.HmacSha256);

////            var claims = new[]
////            {
////        new Claim("id", user.Id.ToString()),
////        new Claim("firstName", user.FirstName),
////        new Claim("lastName", user.LastName),
////        new Claim("email", user.Email),
////    };

////            var jwtToken = new JwtSecurityToken(
////                issuer: "localhost",
////                audience: "localhost",
////                claims: claims,
////                expires: DateTime.UtcNow.AddMinutes(Int32.Parse(duration)),
////                signingCredentials: credentials);

////            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
////        }
////    }
////}
