using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebBanHangOnline.Models
{
    public class OrderViewModel
    {
        [Required(ErrorMessage = "Tên Khách Hàng Là Trường Bắt Buộc")]
        public string CustomerName { get; set; }
        [Required(ErrorMessage = "Số Điện Thoại Khách Hàng Là Trường Bắt Buộc")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Địa Chỉ Khách Hàng Là Trường Bắt Buộc")]
        public string Address { get; set; }
        public int TypePayment { get; set; }
    }
}