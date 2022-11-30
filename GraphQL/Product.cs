

namespace WebApiAutores.GraphQL
{
    public record Product(int Id, string Name, int Quantity);

    // model types for graphql
    public class ProductType : ObjectType<Product>
    {
        public ProductType()
        {
            
        }
    }

    public interface IProductProvider
    {
        List<Product> GetProducts();
    }

    public class ProductProvider : IProductProvider
    {
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public List<Product> GetProducts()
        {
            return new List<Product>()
            {
                new Product(1,"Laptop",20),
                new Product(2,"Mouse", 40),
                new Product(3,"Keyboard",60),
                new Product(4,"Monitor",80),

            };
        }
    }


    // query for the graphql models
    [ExtendObjectType("Queries")]
    public class ProductQuery
    {
        private readonly IProductProvider productProvider;

        public ProductQuery(IProductProvider productProvider)
        {
            this.productProvider = productProvider;
        }

        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public List<Product> Products() 
        {
            return productProvider.GetProducts();
        } 
    }



}
