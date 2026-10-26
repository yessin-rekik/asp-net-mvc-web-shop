namespace tp2.Models.Repositories
{
    public interface IOrderRepository
    {
        Order? GetById(int Id);
        IList<Order> GetByUserId(string userId);
        IList<Order> GetAll();
        void Add(Order o);
        bool UpdateStatus(int id, string status);
    }
}
