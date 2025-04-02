using Microsoft.EntityFrameworkCore;
using NguyenDinhMinhNhat_Buoi4.Models;

namespace NguyenDinhMinhNhat_Buoi4.Repositories
{
    public class EFProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;
        
        /// <summary>
        /// Khởi tạo một instance mới của EFProductRepository
        /// </summary>
        /// <param name="context">ApplicationDbContext được tiêm vào (Dependency Injection)</param>
        public EFProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        
        /// <summary>
        /// Lấy danh sách tất cả các Product từ cơ sở dữ liệu, bao gồm thông tin Category liên quan
        /// </summary>
        /// <returns>Tập hợp các đối tượng Product kèm theo thông tin Category</returns>
        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            // return await _context.Products.ToListAsync();
            return await _context.Products
            .Include(p => p.Category) // Include thông tin về category
            .ToListAsync();
        }
        
        /// <summary>
        /// Lấy Product theo Id chỉ định, bao gồm thông tin Category liên quan
        /// </summary>
        /// <param name="id">Id của Product cần lấy</param>
        /// <returns>Đối tượng Product kèm theo thông tin Category nếu tìm thấy, null nếu không tìm thấy</returns>
        public async Task<Product> GetByIdAsync(int id)
        {
            // return await _context.Products.FindAsync(id);
            // lấy thông tin kèm theo category
            return await _context.Products.Include(p =>
           p.Category).FirstOrDefaultAsync(p => p.Id == id);
        }
        
        /// <summary>
        /// Thêm mới một Product vào cơ sở dữ liệu
        /// </summary>
        /// <param name="product">Đối tượng Product cần thêm mới</param>
        /// <returns>Task hoàn thành khi thêm xong</returns>
        public async Task AddAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }
        
        /// <summary>
        /// Cập nhật thông tin của một Product trong cơ sở dữ liệu
        /// </summary>
        /// <param name="product">Đối tượng Product đã được cập nhật thông tin</param>
        /// <returns>Task hoàn thành khi cập nhật xong</returns>
        public async Task UpdateAsync(Product product)
        {
            try
            {
                _context.Entry(product).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết
                Console.WriteLine($"Lỗi trong Repository.UpdateAsync: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                throw; // Ném lại ngoại lệ để controller có thể xử lý
            }
        }
        
        /// <summary>
        /// Xóa Product theo Id chỉ định
        /// </summary>
        /// <param name="id">Id của Product cần xóa</param>
        /// <returns>Task hoàn thành khi xóa xong</returns>
        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }
}
