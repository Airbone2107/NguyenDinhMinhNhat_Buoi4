using NguyenDinhMinhNhat_Buoi4.Models;

namespace NguyenDinhMinhNhat_Buoi4.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<Order> GetByIdAsync(int id);
        Task<Order> GetOrderWithDetailsAsync(int id);
        Task DeleteAsync(int id);
    }
} 