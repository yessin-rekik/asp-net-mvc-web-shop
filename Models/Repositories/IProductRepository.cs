namespace tp2.Models.Repositories
{
    public interface IProductRepository
    {
        Product? GetById(int Id);
        IList<Product> GetAll();
        IQueryable<Product> GetAllProducts();
        void Add(Product t);
        Product? Update(Product t);
        void Delete(int Id);
        IList<Product> GetProductsByCategID(int? CategId);
        IList<Product> FindByName(string name);
    }
}
