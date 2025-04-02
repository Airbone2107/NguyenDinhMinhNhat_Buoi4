using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NguyenDinhMinhNhat_Buoi4.Models
{
    public class Order
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; }
        
        public DateTime OrderDate { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal TotalPrice { get; set; }

        [Required]
        public string ShippingAddress { get; set; }
        
        // Trường Notes không bắt buộc, có thể để trống
        [Display(Name = "Ghi chú")]
        public string Notes { get; set; }

        // Các thuộc tính bổ sung theo yêu cầu
        [Required]
        [Display(Name = "Họ và tên")]
        public string CustomerName { get; set; }
        
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
        
        [Required]
        [Phone]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; }
        
        [Required]
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }
        
        [NotMapped]
        public decimal TotalAmount { get => TotalPrice; }

        [ForeignKey("UserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }
        
        [ValidateNever]
        public List<OrderDetail> OrderDetails { get; set; }
    }
}
