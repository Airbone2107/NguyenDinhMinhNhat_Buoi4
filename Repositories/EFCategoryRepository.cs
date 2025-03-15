using Microsoft.EntityFrameworkCore;
using NguyenDinhMinhNhat_Buoi4.Models;

namespace NguyenDinhMinhNhat_Buoi4.Repositories
{
    public class EFCategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Khởi tạo một instance mới của EFCategoryRepository
        /// </summary>
        /// <param name="context">ApplicationDbContext được tiêm vào (Dependency Injection)</param>
        public EFCategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách tất cả các Category từ cơ sở dữ liệu
        /// </summary>
        /// <returns>Tập hợp các đối tượng Category</returns>
        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        /// <summary>
        /// Lấy Category theo Id chỉ định
        /// </summary>
        /// <param name="id">Id của Category cần lấy</param>
        /// <returns>Đối tượng Category nếu tìm thấy, null nếu không tìm thấy</returns>
        public async Task<Category> GetByIdAsync(int id)
        {
            return await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        }

        /// <summary>
        /// Thêm mới một Category vào cơ sở dữ liệu
        /// </summary>
        /// <param name="category">Đối tượng Category cần thêm mới</param>
        /// <returns>Task hoàn thành khi thêm xong</returns>
        public async Task AddAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Cập nhật thông tin của một Category trong cơ sở dữ liệu
        /// </summary>
        /// <param name="category">Đối tượng Category đã được cập nhật thông tin</param>
        /// <returns>Task hoàn thành khi cập nhật xong</returns>
        public async Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Xóa Category theo Id chỉ định
        /// </summary>
        /// <param name="id">Id của Category cần xóa</param>
        /// <returns>Task hoàn thành khi xóa xong</returns>
        public async Task DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }
    }
}
