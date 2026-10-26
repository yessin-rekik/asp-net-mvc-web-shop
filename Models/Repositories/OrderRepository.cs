using Microsoft.EntityFrameworkCore;

namespace tp2.Models.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        readonly AppDbContext context;

        public OrderRepository(AppDbContext context)
        {
            this.context = context;
        }

        public void Add(Order o)
        {
            context.Orders.Add(o);
            context.SaveChanges();
        }

        public Order? GetById(int id)
        {
            return context.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .FirstOrDefault(o => o.Id == id);
        }

        public IList<Order> GetByUserId(string userId)
        {
            return context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId)
                .Include(o => o.Items)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        public IList<Order> GetAll()
        {
            return context.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        public bool UpdateStatus(int id, string status)
        {
            var order = context.Orders.Find(id);
            if (order == null)
            {
                return false;
            }

            order.Status = status;
            context.SaveChanges();
            return true;
        }
    }
}
