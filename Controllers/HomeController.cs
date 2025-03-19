using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using NguyenDinhMinhNhat_Buoi4.Models;
using NguyenDinhMinhNhat_Buoi4.Repositories;

namespace NguyenDinhMinhNhat_Buoi4.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductRepository _productRepository;

        public HomeController(ILogger<HomeController> logger, IProductRepository productRepository)
        {
            _logger = logger;
            _productRepository = productRepository;
        }

        public async Task<IActionResult> Index()
        {
            // Hiển thị trang chủ với danh sách sản phẩm cho tất cả người dùng
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
} 