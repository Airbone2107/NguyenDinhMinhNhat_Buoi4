// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Định dạng tiền tệ
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })
        .format(amount)
        .replace('₫', 'đ');
}

// Cập nhật số lượng sản phẩm trong giỏ hàng
function updateCartItemCount() {
    fetch('/ShoppingCart/GetCartItemCount')
        .then(response => response.json())
        .then(data => {
            const cartBadge = document.getElementById('cartItemCount');
            if (cartBadge) {
                cartBadge.textContent = data.count;
                
                if (data.count > 0) {
                    cartBadge.style.display = 'flex';
                } else {
                    cartBadge.style.display = 'none';
                }
            }
        })
        .catch(error => console.error('Lỗi khi lấy số lượng sản phẩm:', error));
}

// Hiển thị thông tin giỏ hàng khi hover
function updateCartPreview() {
    $.ajax({
        url: '/ShoppingCart/GetCartPreview',
        type: 'GET',
        success: function (response) {
            let items = response.items;
            let cartPreviewContent = `
                <div class="dropdown-header">
                    Giỏ hàng của bạn
                </div>
                <div class="dropdown-items">`;
            
            if (items && items.length > 0) {
                items.forEach(function (item) {
                    cartPreviewContent += `
                        <a href="/Product/Display/${item.productId}" class="dropdown-item cart-item-link">
                            <img src="${item.imageUrl}" alt="${item.name}" class="cart-item-image" />
                            <div class="cart-item-details">
                                <div class="cart-item-name">${item.name}</div>
                                <div class="cart-item-price">${formatCurrency(item.price)} x ${item.quantity}</div>
                            </div>
                        </a>`;
                });
                
                cartPreviewContent += `</div>
                    <div class="dropdown-footer">
                        <div class="cart-preview-total">
                            <span>Tổng cộng: ${formatCurrency(response.totalPrice)}</span>
                        </div>
                        <a href="/ShoppingCart/Index" class="view-cart-btn">Xem giỏ hàng</a>
                    </div>`;
            } else {
                cartPreviewContent += `
                    <div class="empty-cart-message">
                        <i class="fas fa-shopping-cart"></i>
                        <p>Giỏ hàng trống</p>
                    </div>
                </div>
                <div class="dropdown-footer">
                    <a href="/ShoppingCart/Index" class="view-cart-btn">Xem giỏ hàng</a>
                </div>`;
            }
            
            $('#cartPreview').html(cartPreviewContent);
        },
        error: function(error) {
            console.error('Lỗi khi lấy thông tin giỏ hàng:', error);
        }
    });
}

// Quản lý dropdown giỏ hàng khi hover
function setupCartDropdown() {
    let cartIcon = document.querySelector(".dropdown-hover");
    let dropdownMenu = document.getElementById("cartPreview");
    let hideTimeout; // Biến để lưu timeout khi rời khỏi icon

    function showDropdown() {
        clearTimeout(hideTimeout); // Xóa timeout nếu có
        dropdownMenu.classList.add("show");
    }

    function hideDropdown() {
        hideTimeout = setTimeout(function () {
            if (!cartIcon.matches(":hover") && !dropdownMenu.matches(":hover")) {
                dropdownMenu.classList.remove("show");
            }
        }, 200); // Delay 200ms để tránh dropdown bị mất khi di chuyển nhanh
    }

    cartIcon.addEventListener("mouseenter", showDropdown);
    dropdownMenu.addEventListener("mouseenter", showDropdown);

    cartIcon.addEventListener("mouseleave", hideDropdown);
    dropdownMenu.addEventListener("mouseleave", hideDropdown);
}

// Khởi tạo các hàm khi trang được tải
document.addEventListener('DOMContentLoaded', function() {
    updateCartItemCount();
    updateCartPreview();
    setupCartDropdown();
});
