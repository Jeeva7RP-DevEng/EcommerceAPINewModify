using ECommerce.API.Models;

namespace ECommerce.API.DataAccess
{
    public interface IDataAccess
    {
        // Product Categories
        List<ProductCategory> GetProductCategories();
        ProductCategory GetProductCategory(int id);

        // Product Categories Management
        string ProductCategoryAdd(string category, string subCategory);

        // Products
        Offer GetOffer(int id);
        List<Product> GetProducts(string category, string subcategory, int count);
        Product GetProduct(int id);
        List<Product> PutProduct(int id, float price, int quantity);

        // Users
        bool InsertUser(User user);
        User IsUserPresent(string email, string password);
        User GetUser(int id);

        void InsertReview(Review review);
        List<Review> GetProductReviews(int productId);

        bool InsertCartItem(int userId, int productId);
        Cart GetActiveCartOfUser(int userid);
        Cart GetCart(int cartid);
        List<Cart> GetAllPreviousCartsOfUser(int userid);
        List<PaymentMethod> GetPaymentMethods();
        int InsertPayment(Payment payment);
        int InsertOrder(Order order);


    }
}
