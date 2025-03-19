using Microsoft.AspNetCore.Mvc;
using NguyenDinhMinhNhat_Buoi4.Models;
using NguyenDinhMinhNhat_Buoi4.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NguyenDinhMinhNhat_Buoi4.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        
        public ProductController(IProductRepository productRepository,
       ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }
        
        // Hiển thị danh sách sản phẩm cho trang chủ
        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }
        
        // Hiển thị chi tiết sản phẩm
        public async Task<IActionResult> Display(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
    }
} 