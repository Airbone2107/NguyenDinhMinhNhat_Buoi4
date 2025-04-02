using Microsoft.AspNetCore.Mvc;
using NguyenDinhMinhNhat_Buoi4.Models;
using NguyenDinhMinhNhat_Buoi4.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using NguyenDinhMinhNhat_Buoi4.Areas.Admin.Models;

namespace NguyenDinhMinhNhat_Buoi4.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{SD.Role_Admin},{SD.Role_Company},{SD.Role_Employee}")]
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
        
        // Hiển thị danh sách sản phẩm cho quản lý
        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }
        
        // Hiển thị form thêm sản phẩm mới
        public async Task<IActionResult> Add()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }
        
        // Xử lý thêm sản phẩm mới
        [HttpPost]
        public async Task<IActionResult> Add(Product product, IFormFile imageUrl)
        {
            if (ModelState.IsValid)
            {
                if (imageUrl != null)
                {
                    product.ImageUrl = await SaveImage(imageUrl);
                }
                
                await _productRepository.AddAsync(product);
                return RedirectToAction("Index");
            }
            
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(product);
        }
        
        private async Task<string> SaveImage(IFormFile image)
        {
            var fileName = Path.GetFileName(image.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }
            
            return "/images/" + fileName;
        }
        
        // Hiển thị form cập nhật sản phẩm
        public async Task<IActionResult> Update(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }
        
        // Xử lý cập nhật sản phẩm
        [HttpPost]
        public async Task<IActionResult> Update(int id, Product product, IFormFile imageUrl)
        {
            if (id != product.Id)
            {
                return NotFound();
            }
            
            // Ghi lại trạng thái ModelState
            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"Lỗi ở {state.Key}: {error.ErrorMessage}");
                    }
                }
                
                // Bỏ qua lỗi Description và ImageUrl nếu có
                if (ModelState.ContainsKey("Description"))
                {
                    ModelState.Remove("Description");
                }
                
                if (ModelState.ContainsKey("ImageUrl"))
                {
                    ModelState.Remove("ImageUrl");
                }
            }
            
            try
            {
                // Lấy thông tin sản phẩm hiện tại từ database
                var currentProduct = await _productRepository.GetByIdAsync(id);
                if (currentProduct == null)
                {
                    return NotFound();
                }
                
                // Áp dụng dữ liệu mới từ form vào sản phẩm hiện tại
                currentProduct.Name = product.Name;
                currentProduct.Price = product.Price;
                currentProduct.CategoryId = product.CategoryId;
                
                // Chỉ cập nhật Description nếu có giá trị
                if (!string.IsNullOrEmpty(product.Description))
                {
                    currentProduct.Description = product.Description;
                }
                
                // Xử lý hình ảnh (nếu có)
                if (imageUrl != null)
                {
                    // Xóa ảnh cũ nếu cần
                    if (!string.IsNullOrEmpty(currentProduct.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", currentProduct.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    
                    // Lưu ảnh mới
                    currentProduct.ImageUrl = await SaveImage(imageUrl);
                }
                // Không cần else vì chúng ta đang sử dụng đối tượng currentProduct đã có ImageUrl
                
                // Cập nhật sản phẩm trong database
                await _productRepository.UpdateAsync(currentProduct);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Ghi log lỗi để có thông tin chi tiết
                Console.WriteLine($"Lỗi cập nhật sản phẩm: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                
                ModelState.AddModelError("", $"Không thể cập nhật sản phẩm: {ex.Message}");
            }
            
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }
        
        // Hiển thị form xóa sản phẩm
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            
            return View(product);
        }
        
        // Xử lý xóa sản phẩm
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productRepository.DeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
} 