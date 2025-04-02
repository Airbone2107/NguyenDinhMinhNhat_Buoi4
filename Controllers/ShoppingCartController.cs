using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NguyenDinhMinhNhat_Buoi4.Extensions;
using NguyenDinhMinhNhat_Buoi4.Models;
using NguyenDinhMinhNhat_Buoi4.Repositories;

namespace NguyenDinhMinhNhat_Buoi4.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShoppingCartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IProductRepository productRepository)
        {
            _productRepository = productRepository;
            _context = context;
            _userManager = userManager;
        }
        
        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn trống, không thể đặt hàng.";
                return RedirectToAction("Index", "Home");
            }
            
            // Truyền dữ liệu giỏ hàng vào ViewBag để hiển thị trong view
            ViewBag.CartItems = cart.Items;
            ViewBag.SubTotal = cart.Items.Sum(i => i.Price * i.Quantity);
            ViewBag.ShippingFee = 0; // Hoặc tính phí vận chuyển nếu có
            ViewBag.Total = ViewBag.SubTotal + ViewBag.ShippingFee;
            
            var order = new Order();
            
            // Tạm thời gán giá trị để tránh lỗi validation khi chạy view
            order.UserId = "temp";
            order.ShippingAddress = "temp";
            
            // Tự động điền thông tin người dùng nếu đã đăng nhập
            if (User.Identity.IsAuthenticated)
            {
                var user = _userManager.GetUserAsync(User).Result;
                if (user != null)
                {
                    order.CustomerName = user.FullName;
                    order.Email = user.Email;
                    order.Phone = user.PhoneNumber;
                    order.Address = user.Address;
                }
            }
            
            return View(order);
        }
        
        [HttpPost]
        public async Task<IActionResult> Checkout(Order order)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                // Xử lý giỏ hàng trống...
                return RedirectToAction("Index", "Home");
            }
            
            if (!ModelState.IsValid)
            {
                // Truyền lại thông tin giỏ hàng khi có lỗi validation
                ViewBag.CartItems = cart.Items;
                ViewBag.SubTotal = cart.Items.Sum(i => i.Price * i.Quantity);
                ViewBag.ShippingFee = 0; // Hoặc tính phí vận chuyển nếu có
                ViewBag.Total = ViewBag.SubTotal + ViewBag.ShippingFee;
                return View(order);
            }
            
            try
            {
                // Nếu người dùng đã đăng nhập, lưu thông tin UserId
                if (User.Identity.IsAuthenticated)
                {
                    var user = await _userManager.GetUserAsync(User);
                    order.UserId = user.Id;
                }
                else
                {
                    // Nếu không đăng nhập, tạo một ID ngẫu nhiên để theo dõi
                    order.UserId = "guest-" + Guid.NewGuid().ToString();
                }
                
                order.OrderDate = DateTime.Now;
                order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
                order.ShippingAddress = order.Address; // Đảm bảo địa chỉ giao hàng được cập nhật
                
                // Đảm bảo Notes không null
                if (order.Notes == null)
                {
                    order.Notes = string.Empty;
                }
                
                order.OrderDetails = cart.Items.Select(i => new OrderDetail
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList();
                
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                
                // Chuẩn bị dữ liệu cho trang OrderCompleted
                var orderDetails = order.OrderDetails.Select(od => new {
                    ProductId = od.ProductId,
                    ProductName = cart.Items.FirstOrDefault(i => i.ProductId == od.ProductId)?.Name,
                    Quantity = od.Quantity,
                    Price = od.Price,
                    Total = od.Price * od.Quantity
                }).ToList();
                
                ViewBag.OrderDetails = orderDetails;
                ViewBag.SubTotal = order.TotalPrice;
                ViewBag.ShippingFee = 0; // Hoặc tính phí vận chuyển nếu có
                
                // Xóa giỏ hàng sau khi đặt hàng thành công
                HttpContext.Session.Remove("Cart");
                
                return View("OrderCompleted", order); // Truyền đối tượng order thay vì chỉ Id
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi đặt hàng: " + ex.Message);
                
                // Truyền lại thông tin giỏ hàng khi có lỗi
                ViewBag.CartItems = cart.Items;
                ViewBag.SubTotal = cart.Items.Sum(i => i.Price * i.Quantity);
                ViewBag.ShippingFee = 0; // Hoặc tính phí vận chuyển nếu có
                ViewBag.Total = ViewBag.SubTotal + ViewBag.ShippingFee;
                return View(order);
            }
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(Order order)
        {
            // Lấy thông tin giỏ hàng
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn trống, không thể đặt hàng.";
                return RedirectToAction("Index", "Home");
            }
            
            // Xóa các lỗi validation không cần thiết trước khi kiểm tra ModelState
            ModelState.Remove("UserId");
            ModelState.Remove("ShippingAddress");
            ModelState.Remove("OrderDetails");
            ModelState.Remove("Notes");
            
            if (!ModelState.IsValid)
            {
                // Truyền lại thông tin giỏ hàng khi có lỗi validation
                ViewBag.CartItems = cart.Items;
                ViewBag.SubTotal = cart.Items.Sum(i => i.Price * i.Quantity);
                ViewBag.ShippingFee = 0; // Hoặc tính phí vận chuyển nếu có
                ViewBag.Total = ViewBag.SubTotal + ViewBag.ShippingFee;
                
                return View("Checkout", order);
            }
            
            try
            {
                // Nếu người dùng đã đăng nhập, lưu thông tin UserId
                if (User.Identity.IsAuthenticated)
                {
                    var user = await _userManager.GetUserAsync(User);
                    order.UserId = user.Id;
                }
                else
                {
                    // Nếu không đăng nhập, tạo một ID ngẫu nhiên để theo dõi
                    order.UserId = "guest-" + Guid.NewGuid().ToString();
                }
                
                // Thiết lập các thông tin cần thiết
                order.OrderDate = DateTime.Now;
                order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
                order.ShippingAddress = order.Address; // Sao chép địa chỉ
                
                // Đảm bảo Notes không null
                if (order.Notes == null)
                {
                    order.Notes = string.Empty;
                }
                
                // Khởi tạo OrderDetails nếu chưa có
                if (order.OrderDetails == null)
                {
                    order.OrderDetails = new List<OrderDetail>();
                }
                
                // Tạo chi tiết đơn hàng
                order.OrderDetails = cart.Items.Select(i => new OrderDetail
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList();
                
                // Lưu đơn hàng vào cơ sở dữ liệu
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                
                // Chuẩn bị dữ liệu cho trang OrderCompleted
                var orderDetails = order.OrderDetails.Select(od => new {
                    ProductId = od.ProductId,
                    ProductName = cart.Items.FirstOrDefault(i => i.ProductId == od.ProductId)?.Name,
                    Quantity = od.Quantity,
                    Price = od.Price,
                    Total = od.Price * od.Quantity,
                    ImageUrl = cart.Items.FirstOrDefault(i => i.ProductId == od.ProductId)?.ImageUrl
                }).ToList();
                
                ViewBag.OrderDetails = orderDetails;
                ViewBag.SubTotal = order.TotalPrice;
                ViewBag.ShippingFee = 0; // Hoặc tính phí vận chuyển nếu có
                ViewBag.Total = order.TotalPrice;
                
                // Xóa giỏ hàng sau khi đặt hàng thành công
                HttpContext.Session.Remove("Cart");
                
                // Hiển thị trang xác nhận đơn hàng đã hoàn tất
                return View("OrderCompleted", order);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi đặt hàng: " + ex.Message);
                
                // Truyền lại thông tin giỏ hàng khi có lỗi
                ViewBag.CartItems = cart.Items;
                ViewBag.SubTotal = cart.Items.Sum(i => i.Price * i.Quantity);
                ViewBag.ShippingFee = 0; // Hoặc tính phí vận chuyển nếu có
                ViewBag.Total = ViewBag.SubTotal + ViewBag.ShippingFee;
                
                return View("Checkout", order);
            }
        }

        public async Task<IActionResult> AddToCart(int productId, int quantity, string redirect = "")
        {
            // Giả sử bạn có phương thức lấy thông tin sản phẩm từ productId
            var product = await GetProductFromDatabase(productId);
            if (product == null)
            {
                return NotFound();
            }
            
            var cartItem = new CartItem
            {
                ProductId = productId,
                Name = product.Name,
                Price = product.Price,
                Quantity = quantity,
                ImageUrl = product.ImageUrl
            };
            
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            cart.AddItem(cartItem);
            HttpContext.Session.SetObjectAsJson("Cart", cart);
            
            // Kiểm tra nếu yêu cầu là Ajax
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "Sản phẩm đã được thêm vào giỏ hàng" });
            }
            
            // Nếu có tham số redirect=checkout, chuyển đến trang thanh toán
            if (redirect.ToLower() == "checkout")
            {
                return RedirectToAction("Checkout");
            }
            
            return RedirectToAction("Index");
        }
        
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            return View(cart);
        }
        
        [HttpGet]
        public IActionResult GetCartItemCount()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            int count = cart?.Items.Sum(i => i.Quantity) ?? 0;
            return Json(new { count });
        }
        
        [HttpGet]
        public IActionResult GetCartPreview()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            var items = cart?.Items ?? new List<CartItem>();
            
            return Json(new { 
                items = items.Select(i => new {
                    i.ProductId,
                    i.Name,
                    i.Price,
                    i.Quantity,
                    ImageUrl = i.ImageUrl ?? "/images/no-image.png"
                }).Take(5).ToList(),
                totalItems = items.Sum(i => i.Quantity),
                totalPrice = items.Sum(i => i.Price * i.Quantity)
            });
        }
        
        private async Task<Product> GetProductFromDatabase(int productId)
        {
            // Truy vấn cơ sở dữ liệu để lấy thông tin sản phẩm
            var product = await _productRepository.GetByIdAsync(productId);
            return product;
        }
        
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart is not null)
            {
                cart.RemoveItem(productId);
                // Lưu lại giỏ hàng vào Session sau khi đã xóa mục
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }
            return RedirectToAction("Index");
        }
        
        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            if (quantity <= 0)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Số lượng không hợp lệ" });
                }
                return RedirectToAction("RemoveFromCart", new { productId });
            }
            
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart != null)
            {
                var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
                if (item != null)
                {
                    item.Quantity = quantity;
                    HttpContext.Session.SetObjectAsJson("Cart", cart);
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        decimal totalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
                        int totalItems = cart.Items.Sum(i => i.Quantity);
                        
                        return Json(new { 
                            success = true, 
                            totalPrice = totalPrice,
                            totalItems = totalItems,
                            itemTotal = item.Price * item.Quantity,
                            message = "Số lượng đã được cập nhật" 
                        });
                    }
                }
            }
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = "Không thể cập nhật số lượng" });
            }
            
            return RedirectToAction("Index");
        }
    }
}
