using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;
using WebBanHangOnline.Models.Payments;

namespace WebBanHangOnline.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ShoppingCartController()
        {
        }

        public ShoppingCartController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        // GET: ShoppingCart
        [AllowAnonymous] //cho phép vào web và thực hiện các hành động khi chưa login(method nào cho phép thì người chưa login dùng được)
        public ActionResult Index()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null && cart.Items.Any())
            {
                ViewBag.CheckCart = cart;
            }
            return View();
        }

        [AllowAnonymous]
        public ActionResult VnpayReturn()
        {
            if (Request.QueryString.Count > 0)
            {
                string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"]; //Chuoi bi mat
                var vnpayData = Request.QueryString;
                VnPayLibrary vnpay = new VnPayLibrary();

                foreach (string s in vnpayData)
                {
                    //get all querystring data
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s, vnpayData[s]);
                    }
                }
                string orderCode = Convert.ToString(vnpay.GetResponseData("vnp_TxnRef"));
                long vnpayTranId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                String vnp_SecureHash = Request.QueryString["vnp_SecureHash"];
                String TerminalID = Request.QueryString["vnp_TmnCode"];
                long vnp_Amount = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount")) / 100;
                String bankCode = Request.QueryString["vnp_BankCode"];

                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                    {
                        var itemOrder = db.Orders.FirstOrDefault(x => x.Code == orderCode);
                        if (itemOrder != null)
                        {
                            itemOrder.Status = 2;//đã thanh toán
                            db.Orders.Attach(itemOrder);
                            db.Entry(itemOrder).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        //Thanh toan thanh cong
                        ViewBag.InnerText = "Giao dịch được thực hiện thành công. Cảm ơn quý khách đã sử dụng dịch vụ";
                        //log.InfoFormat("Thanh toan thanh cong, OrderId={0}, VNPAY TranId={1}", orderId, vnpayTranId);
                    }
                    else
                    {
                        //Thanh toan khong thanh cong. Ma loi: vnp_ResponseCode
                        ViewBag.InnerText = "Có lỗi xảy ra trong quá trình xử lý.Mã lỗi: " + vnp_ResponseCode;
                        //log.InfoFormat("Thanh toan loi, OrderId={0}, VNPAY TranId={1},ResponseCode={2}", orderId, vnpayTranId, vnp_ResponseCode);
                    }
                    //displayTmnCode.InnerText = "Mã Website (Terminal ID):" + TerminalID;
                    //displayTxnRef.InnerText = "Mã giao dịch thanh toán:" + orderId.ToString();
                    //displayVnpayTranNo.InnerText = "Mã giao dịch tại VNPAY:" + vnpayTranId.ToString();
                    ViewBag.ThanhToanThanhCong = "Số tiền thanh toán (VND): " + vnp_Amount.ToString(); //truyền sang view VnpayReturn
                    //displayBankCode.InnerText = "Ngân hàng thanh toán:" + bankCode;
                }
            }
            //var a = UrlPayment(0, "DH3574");
            return View();
        }

        [AllowAnonymous]
        public ActionResult CheckOut()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null && cart.Items.Any())
            {
                ViewBag.CheckCart = cart;
            }
            return View();
        }
        [AllowAnonymous]
        public ActionResult CheckOutSuccess()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult Partial_Item_ThanhToan()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null && cart.Items.Any())
            {
                return PartialView(cart.Items);
            }
            return PartialView();
        }
        [AllowAnonymous]
        public ActionResult Partial_Item_Cart()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null && cart.Items.Any())
            {
                return PartialView(cart.Items);
            }
            return PartialView();
        }
        [AllowAnonymous]
        public ActionResult ShowCount()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null)
            {
                return Json(new { Count = cart.Items.Count }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Count = 0 }, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public ActionResult Partial_CheckOut()
        {
            var user = UserManager.FindByNameAsync(User.Identity.Name).Result;
            if (user != null)
            {
                ViewBag.User = user;
            }
            return PartialView();
        }

        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public ActionResult CheckOut(OrderViewModel req)
        //{
        //    var code = new { Success = false, Code = -1, Url = "" };
        //    if (ModelState.IsValid)
        //    {
        //        ShoppingCart cart = (ShoppingCart)Session["Cart"];
        //        if (cart != null)
        //        {
        //            Order order = new Order();
        //            order.CustomerName = req.CustomerName;
        //            order.Phone = req.Phone;
        //            order.Address = req.Address;
        //            order.Email = req.Email;
        //            order.Status = 1; //1 là trạng thái chưa thanh toán, 2 là đã thanh toán, 3 là hoàn thành đơn (đã giao OK), 4 là hủy :)
        //            cart.Items.ForEach(x => order.OrderDetails.Add(new OrderDetail
        //            {
        //                ProductId = x.ProductId,
        //                Quantity = x.Quantity,
        //                Price = x.Price,

        //            }));
        //            order.TotalAmount = cart.Items.Sum(x => (x.Price * x.Quantity));
        //            order.Quantity = cart.Items.Sum(x => x.Quantity); // ✅ Gán tổng số lượng sản phẩm (Bài học cột sống: không gán và order bị 0 sản phẩm :)))
        //            order.TypePayment = req.TypePayment;
        //            order.CreatedDate = DateTime.Now;
        //            order.ModifiedDate = DateTime.Now;
        //            order.CreatedBy = req.Phone;
        //            if (User.Identity.IsAuthenticated)
        //            order.CustomerId = User.Identity.GetUserId();
        //            Random rd = new Random();
        //            order.Code = "DH" + rd.Next(0,9) + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9);
        //            //order.Email = req.Email;
        //            db.Orders.Add(order);
        //            db.SaveChanges();

        //            // code send mail cho KH
        //            var strSanPham = "";
        //            var thanhTien = decimal.Zero;
        //            var tongTien = decimal.Zero;
        //            foreach(var sp in cart.Items)
        //            {
        //                strSanPham += "<tr>";
        //                strSanPham += "<td>"+sp.ProductName+"</td>";
        //                strSanPham += "<td>"+sp.Quantity+"</td>";
        //                strSanPham += "<td>"+WebBanHangOnline.Common.Common.FormatNumber(sp.TotalPrice)+"</td>";
        //                strSanPham += "</tr>";
        //                thanhTien += sp.Price * sp.Quantity;
        //            }
        //            tongTien = thanhTien;
        //            string contentCustomer = System.IO.File.ReadAllText(Server.MapPath("~/Content/templates/send2.html"));
        //            contentCustomer = contentCustomer.Replace("{{MaDon}}", order.Code);
        //            contentCustomer = contentCustomer.Replace("{{SanPham}}", strSanPham);
        //            contentCustomer = contentCustomer.Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"));
        //            contentCustomer = contentCustomer.Replace("{{TenKhachHang}}", order.CustomerName);
        //            contentCustomer = contentCustomer.Replace("{{Phone}}", order.Phone);
        //            contentCustomer = contentCustomer.Replace("{{Email}}", req.Email);
        //            contentCustomer = contentCustomer.Replace("{{DiaChiNhanHang}}", order.Address);
        //            contentCustomer = contentCustomer.Replace("{{ThanhTien}}", WebBanHangOnline.Common.Common.FormatNumber(thanhTien, 0));
        //            contentCustomer = contentCustomer.Replace("{{TongTien}}", WebBanHangOnline.Common.Common.FormatNumber(tongTien, 0));
        //            WebBanHangOnline.Common.Common.SendMail("Gốm Sứ Bình Xuyên", "Đơn hàng #"+order.Code, contentCustomer.ToString(), req.Email);

        //            string contentAdmin = System.IO.File.ReadAllText(Server.MapPath("~/Content/templates/send1.html"));
        //            contentAdmin = contentAdmin.Replace("{{MaDon}}", order.Code);
        //            contentAdmin = contentAdmin.Replace("{{SanPham}}", strSanPham);
        //            contentAdmin = contentAdmin.Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"));
        //            contentAdmin = contentAdmin.Replace("{{TenKhachHang}}", order.CustomerName);
        //            contentAdmin = contentAdmin.Replace("{{Phone}}", order.Phone);
        //            contentAdmin = contentAdmin.Replace("{{Email}}", req.Email);
        //            contentAdmin = contentAdmin.Replace("{{DiaChiNhanHang}}", order.Address);
        //            contentAdmin = contentAdmin.Replace("{{ThanhTien}}", WebBanHangOnline.Common.Common.FormatNumber(thanhTien, 0));
        //            contentAdmin = contentAdmin.Replace("{{TongTien}}", WebBanHangOnline.Common.Common.FormatNumber(tongTien, 0));
        //            WebBanHangOnline.Common.Common.SendMail("Gốm Sứ Bình Xuyên", "Đơn hàng mới #" + order.Code, contentAdmin.ToString(), ConfigurationManager.AppSettings["EmailAdmin"]);
        //            cart.ClearCart();   
        //            code = new { Success = true, Code = req.TypePayment, Url = "" };
        //            //var url = "";
        //            if (req.TypePayment == 2)
        //            {
        //                var url = UrlPayment(req.TypePaymentVN, order.Code);
        //                code = new { Success = true, Code = req.TypePayment, Url = url };
        //            }

        //            //code = new { Success = true, code = 1, Url = url };
        //            //return RedirectToAction("CheckOutSuccess");
        //        }
        //    }
        //    return Json(code);
        //}

        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public ActionResult CheckOut(OrderViewModel req)
        //{
        //    var result = new { Success = false, Code = -1, Url = "" };

        //    if (!ModelState.IsValid)
        //        return Json(result);

        //    var cart = Session["Cart"] as ShoppingCart;
        //    if (cart == null || cart.Items.Count == 0)
        //        return Json(result);

        //    var order = new Order
        //    {
        //        CustomerName = req.CustomerName,
        //        Phone = req.Phone,
        //        Address = req.Address,
        //        Email = req.Email,
        //        Status = 1, //1 là trạng thái chưa thanh toán, 2 là đã thanh toán, 3 là hoàn thành đơn (đã giao OK), 4 là hủy :)
        //        CreatedDate = DateTime.Now,
        //        ModifiedDate = DateTime.Now,
        //        CreatedBy = req.Phone,
        //        TypePayment = req.TypePayment,
        //        Code = "DH" + new Random().Next(1000, 9999).ToString()
        //    };

        //    if (User.Identity.IsAuthenticated)
        //        order.CustomerId = User.Identity.GetUserId(); // lấy id của khách hàng (người đang đăng nhập). Trong database (bảng AspNetUsers nếu ta xài ASP.NET Identity mặc định), thì mỗi user có một Id (kiểu string), ví dụ: d5aa4e21-3d90-4cdd-9bbf-3f83a32e490d
        //    foreach (var item in cart.Items)
        //    {
        //        order.OrderDetails.Add(new OrderDetail
        //        {
        //            ProductId = item.ProductId,
        //            Quantity = item.Quantity,
        //            Price = item.Price
        //        });
        //    }

        //    order.TotalAmount = cart.Items.Sum(x => x.Price * x.Quantity);
        //    order.Quantity = cart.Items.Sum(x => x.Quantity);

        //    db.Orders.Add(order);
        //    db.SaveChanges();

        //    // Email nội dung sản phẩm
        //    var strSanPham = new StringBuilder();
        //    decimal thanhTien = 0;

        //    foreach (var item in cart.Items)
        //    {
        //        strSanPham.Append("<tr>");
        //        strSanPham.AppendFormat("<td>{0}</td>", item.ProductName);
        //        strSanPham.AppendFormat("<td>{0}</td>", item.Quantity);
        //        strSanPham.AppendFormat("<td>{0}</td>", WebBanHangOnline.Common.Common.FormatNumber(item.TotalPrice));
        //        strSanPham.Append("</tr>");
        //        thanhTien += item.Price * item.Quantity;
        //    }

        //    var tongTien = thanhTien;

        //    // Gửi mail cho KH
        //    string contentCustomer = System.IO.File.ReadAllText(Server.MapPath("~/Content/templates/send2.html"));
        //    contentCustomer = contentCustomer
        //        .Replace("{{MaDon}}", order.Code)
        //        .Replace("{{SanPham}}", strSanPham.ToString())
        //        .Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"))
        //        .Replace("{{TenKhachHang}}", order.CustomerName)
        //        .Replace("{{Phone}}", order.Phone)
        //        .Replace("{{Email}}", req.Email)
        //        .Replace("{{DiaChiNhanHang}}", order.Address)
        //        .Replace("{{ThanhTien}}", WebBanHangOnline.Common.Common.FormatNumber(thanhTien, 0))
        //        .Replace("{{TongTien}}", WebBanHangOnline.Common.Common.FormatNumber(tongTien, 0));

        //    WebBanHangOnline.Common.Common.SendMail("Gốm Sứ Bình Xuyên", $"Đơn hàng #{order.Code}", contentCustomer, req.Email);

        //    // Gửi mail cho Admin
        //    string contentAdmin = System.IO.File.ReadAllText(Server.MapPath("~/Content/templates/send1.html"));
        //    contentAdmin = contentAdmin
        //        .Replace("{{MaDon}}", order.Code)
        //        .Replace("{{SanPham}}", strSanPham.ToString())
        //        .Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"))
        //        .Replace("{{TenKhachHang}}", order.CustomerName)
        //        .Replace("{{Phone}}", order.Phone)
        //        .Replace("{{Email}}", req.Email)
        //        .Replace("{{DiaChiNhanHang}}", order.Address)
        //        .Replace("{{ThanhTien}}", WebBanHangOnline.Common.Common.FormatNumber(thanhTien, 0))
        //        .Replace("{{TongTien}}", WebBanHangOnline.Common.Common.FormatNumber(tongTien, 0));

        //    WebBanHangOnline.Common.Common.SendMail("Gốm Sứ Bình Xuyên", $"Đơn hàng mới #{order.Code}", contentAdmin, ConfigurationManager.AppSettings["EmailAdmin"]);

        //    // Xóa giỏ hàng
        //    cart.ClearCart();

        //    // Nếu thanh toán online
        //    if (req.TypePayment == 2)
        //    {
        //        var url = UrlPayment(req.TypePaymentVN, order.Code);
        //        result = new { Success = true, Code = req.TypePayment, Url = url };
        //    }
        //    else
        //    {
        //        result = new { Success = true, Code = req.TypePayment, Url = "" };
        //    }

        //    return Json(result);
        //}

        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public ActionResult CheckOut(OrderViewModel req)
        //{
        //    var result = new { Success = false, Code = -1, Url = "" };

        //    if (!ModelState.IsValid)
        //        return Json(result);

        //    var cart = Session["Cart"] as ShoppingCart;
        //    if (cart == null || cart.Items.Count == 0)
        //        return Json(result);

        //    // Bắt đầu một transaction
        //    using (var transaction = db.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            // ✅ BƯỚC 1: Kiểm tra tồn kho trước khi tạo đơn hàng
        //            // Lặp lại kiểm tra tồn kho để tránh trường hợp race condition
        //            // Đảm bảo rằng việc truy vấn sản phẩm được thực hiện trong transaction
        //            foreach (var item in cart.Items)
        //            {
        //                var product = db.Products.FirstOrDefault(p => p.Id == item.ProductId);
        //                if (product == null || product.Quantity < item.Quantity)
        //                {
        //                    // Nếu tồn kho không đủ, rollback transaction
        //                    transaction.Rollback();
        //                    return Json(new
        //                    {
        //                        Success = false,
        //                        Code = -2,
        //                        Url = "",
        //                        Message = $"Sản phẩm \"{item.ProductName}\" không đủ tồn kho. Chỉ còn lại {product?.Quantity ?? 0} sản phẩm."
        //                    });
        //                }
        //            }

        //            // Tạo đơn hàng
        //            var order = new Order
        //            {
        //                CustomerName = req.CustomerName,
        //                Phone = req.Phone,
        //                Address = req.Address,
        //                Email = req.Email,
        //                Status = 1,
        //                CreatedDate = DateTime.Now,
        //                ModifiedDate = DateTime.Now,
        //                CreatedBy = req.Phone,
        //                TypePayment = req.TypePayment,
        //                Code = "DH" + new Random().Next(1000, 9999).ToString()
        //            };

        //            if (User.Identity.IsAuthenticated)
        //                order.CustomerId = User.Identity.GetUserId();

        //            foreach (var item in cart.Items)
        //            {
        //                order.OrderDetails.Add(new OrderDetail
        //                {
        //                    ProductId = item.ProductId,
        //                    Quantity = item.Quantity,
        //                    Price = item.Price
        //                });
        //            }

        //            order.TotalAmount = cart.Items.Sum(x => x.Price * x.Quantity);
        //            order.Quantity = cart.Items.Sum(x => x.Quantity);

        //            db.Orders.Add(order);
        //            db.SaveChanges(); // Lưu đơn hàng để có OrderDetails

        //            // ✅ BƯỚC 2: Sau khi lưu đơn hàng thành công, trừ số lượng tồn kho
        //            foreach (var item in order.OrderDetails)
        //            {
        //                // Lấy lại sản phẩm để đảm bảo cập nhật trạng thái mới nhất trong transaction
        //                var product = db.Products.FirstOrDefault(p => p.Id == item.ProductId);
        //                if (product != null)
        //                {
        //                    product.Quantity -= item.Quantity;

        //                    // Nếu không muốn để tồn kho âm, thì ép về 0
        //                    if (product.Quantity < 0)
        //                    {
        //                        product.Quantity = 0;
        //                    }
        //                    db.Entry(product).State = EntityState.Modified; // Đánh dấu là đã thay đổi
        //                }
        //            }
        //            db.SaveChanges(); // Lưu các thay đổi về tồn kho

        //            // Commit transaction nếu mọi thứ đều thành công
        //            transaction.Commit();

        //            // Các phần gửi mail và xử lý thanh toán khác giữ nguyên
        //            var strSanPham = new StringBuilder();
        //            decimal thanhTien = 0;

        //            foreach (var item in cart.Items)
        //            {
        //                strSanPham.Append("<tr>");
        //                strSanPham.AppendFormat("<td>{0}</td>", item.ProductName);
        //                strSanPham.AppendFormat("<td>{0}</td>", item.Quantity);
        //                strSanPham.AppendFormat("<td>{0}</td>", WebBanHangOnline.Common.Common.FormatNumber(item.TotalPrice));
        //                strSanPham.Append("</tr>");
        //                thanhTien += item.Price * item.Quantity;
        //            }

        //            var tongTien = thanhTien;

        //            string contentCustomer = System.IO.File.ReadAllText(Server.MapPath("~/Content/templates/send2.html"));
        //            contentCustomer = contentCustomer
        //                .Replace("{{MaDon}}", order.Code)
        //                .Replace("{{SanPham}}", strSanPham.ToString())
        //                .Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"))
        //                .Replace("{{TenKhachHang}}", order.CustomerName)
        //                .Replace("{{Phone}}", order.Phone)
        //                .Replace("{{Email}}", req.Email)
        //                .Replace("{{DiaChiNhanHang}}", order.Address)
        //                .Replace("{{ThanhTien}}", WebBanHangOnline.Common.Common.FormatNumber(thanhTien, 0))
        //                .Replace("{{TongTien}}", WebBanHangOnline.Common.Common.FormatNumber(tongTien, 0));

        //            WebBanHangOnline.Common.Common.SendMail("Gốm Sứ Bình Xuyên", $"Đơn hàng #{order.Code}", contentCustomer, req.Email);

        //            string contentAdmin = System.IO.File.ReadAllText(Server.MapPath("~/Content/templates/send1.html"));
        //            contentAdmin = contentAdmin
        //                .Replace("{{MaDon}}", order.Code)
        //                .Replace("{{SanPham}}", strSanPham.ToString())
        //                .Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"))
        //                .Replace("{{TenKhachHang}}", order.CustomerName)
        //                .Replace("{{Phone}}", order.Phone)
        //                .Replace("{{Email}}", req.Email)
        //                .Replace("{{DiaChiNhanHang}}", order.Address)
        //                .Replace("{{ThanhTien}}", WebBanHangOnline.Common.Common.FormatNumber(thanhTien, 0))
        //                .Replace("{{TongTien}}", WebBanHangOnline.Common.Common.FormatNumber(tongTien, 0));

        //            WebBanHangOnline.Common.Common.SendMail("Gốm Sứ Bình Xuyên", $"Đơn hàng mới #{order.Code}", contentAdmin, ConfigurationManager.AppSettings["EmailAdmin"]);

        //            cart.ClearCart();

        //            if (req.TypePayment == 2)
        //            {
        //                var url = UrlPayment(req.TypePaymentVN, order.Code);
        //                result = new { Success = true, Code = req.TypePayment, Url = url };
        //            }
        //            else
        //            {
        //                result = new { Success = true, Code = req.TypePayment, Url = "" };
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            // Nếu có lỗi, rollback transaction
        //            transaction.Rollback();
        //            // Log lỗi để dễ dàng debug
        //            System.Diagnostics.Debug.WriteLine($"Lỗi khi tạo đơn hàng và trừ tồn kho: {ex.Message} - StackTrace: {ex.StackTrace}");
        //            return Json(new { Success = false, Code = -3, Url = "", Message = "Đã xảy ra lỗi trong quá trình đặt hàng. Vui lòng thử lại sau." });
        //        }
        //    } // `using` statement sẽ tự động Dispose transaction

        //    return Json(result);
        //}

        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public ActionResult CheckOut(OrderViewModel req)
        //{
        //    var result = new { Success = false, Code = -1, Url = "" };

        //    if (!ModelState.IsValid)
        //        return Json(result);

        //    var cart = Session["Cart"] as ShoppingCart;
        //    if (cart == null || cart.Items.Count == 0)
        //        return Json(result);

        //    // ✅ Đồng bộ lại AvailableQuantity từ database
        //    foreach (var item in cart.Items)
        //    {
        //        var product = db.Products.FirstOrDefault(p => p.Id == item.ProductId);
        //        if (product != null)
        //        {
        //            item.AvailableQuantity = product.Quantity;
        //        }
        //    }

        //    // Bắt đầu một transaction
        //    using (var transaction = db.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            // ✅ BƯỚC 1: Kiểm tra tồn kho trước khi tạo đơn hàng
        //            // Lặp lại kiểm tra tồn kho để tránh trường hợp race condition
        //            // Đảm bảo rằng việc truy vấn sản phẩm được thực hiện trong transaction
        //            foreach (var item in cart.Items)
        //            {
        //                var product = db.Products.FirstOrDefault(p => p.Id == item.ProductId);
        //                if (product == null || product.Quantity < item.Quantity)
        //                {
        //                    // Nếu tồn kho không đủ, rollback transaction
        //                    transaction.Rollback();
        //                    return Json(new
        //                    {
        //                        Success = false,
        //                        Code = -2,
        //                        Url = "",
        //                        Message = $"Sản phẩm \"{item.ProductName}\" không đủ tồn kho. Chỉ còn lại {product?.Quantity ?? 0} sản phẩm."
        //                    });
        //                }
        //            }

        //            // Tạo đơn hàng
        //            var order = new Order
        //            {
        //                CustomerName = req.CustomerName,
        //                Phone = req.Phone,
        //                Address = req.Address,
        //                Email = req.Email,
        //                Status = 1,
        //                CreatedDate = DateTime.Now,
        //                ModifiedDate = DateTime.Now,
        //                CreatedBy = req.Phone,
        //                TypePayment = req.TypePayment,
        //                Code = "DH" + new Random().Next(1000, 9999).ToString()
        //            };

        //            if (User.Identity.IsAuthenticated)
        //                order.CustomerId = User.Identity.GetUserId();

        //            foreach (var item in cart.Items)
        //            {
        //                order.OrderDetails.Add(new OrderDetail
        //                {
        //                    ProductId = item.ProductId,
        //                    Quantity = item.Quantity,
        //                    Price = item.Price
        //                });
        //            }

        //            order.TotalAmount = cart.Items.Sum(x => x.Price * x.Quantity);
        //            order.Quantity = cart.Items.Sum(x => x.Quantity);

        //            db.Orders.Add(order);
        //            db.SaveChanges(); // Lưu đơn hàng để có OrderDetails

        //            // ✅ BƯỚC 2: Sau khi lưu đơn hàng thành công, trừ số lượng tồn kho
        //            foreach (var item in order.OrderDetails)
        //            {
        //                // Lấy lại sản phẩm để đảm bảo cập nhật trạng thái mới nhất trong transaction
        //                var product = db.Products.FirstOrDefault(p => p.Id == item.ProductId);
        //                if (product != null)
        //                {
        //                    product.Quantity -= item.Quantity;

        //                    // Nếu không muốn để tồn kho âm, thì ép về 0
        //                    if (product.Quantity < 0)
        //                    {
        //                        product.Quantity = 0;
        //                    }
        //                    db.Entry(product).State = EntityState.Modified; // Đánh dấu là đã thay đổi
        //                }
        //            }
        //            db.SaveChanges(); // Lưu các thay đổi về tồn kho

        //            // Commit transaction nếu mọi thứ đều thành công
        //            transaction.Commit();

        //            // Các phần gửi mail và xử lý thanh toán khác giữ nguyên
        //            var strSanPham = new StringBuilder();
        //            decimal thanhTien = 0;

        //            foreach (var item in cart.Items)
        //            {
        //                strSanPham.Append("<tr>");
        //                strSanPham.AppendFormat("<td>{0}</td>", item.ProductName);
        //                strSanPham.AppendFormat("<td>{0}</td>", item.Quantity);
        //                strSanPham.AppendFormat("<td>{0}</td>", WebBanHangOnline.Common.Common.FormatNumber(item.TotalPrice));
        //                strSanPham.Append("</tr>");
        //                thanhTien += item.Price * item.Quantity;
        //            }

        //            var tongTien = thanhTien;

        //            string contentCustomer = System.IO.File.ReadAllText(Server.MapPath("~/Content/templates/send2.html"));
        //            contentCustomer = contentCustomer
        //                .Replace("{{MaDon}}", order.Code)
        //                .Replace("{{SanPham}}", strSanPham.ToString())
        //                .Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"))
        //                .Replace("{{TenKhachHang}}", order.CustomerName)
        //                .Replace("{{Phone}}", order.Phone)
        //                .Replace("{{Email}}", req.Email)
        //                .Replace("{{DiaChiNhanHang}}", order.Address)
        //                .Replace("{{ThanhTien}}", WebBanHangOnline.Common.Common.FormatNumber(thanhTien, 0))
        //                .Replace("{{TongTien}}", WebBanHangOnline.Common.Common.FormatNumber(tongTien, 0));

        //            WebBanHangOnline.Common.Common.SendMail("Gốm Sứ Bình Xuyên", $"Đơn hàng #{order.Code}", contentCustomer, req.Email);

        //            string contentAdmin = System.IO.File.ReadAllText(Server.MapPath("~/Content/templates/send1.html"));
        //            contentAdmin = contentAdmin
        //                .Replace("{{MaDon}}", order.Code)
        //                .Replace("{{SanPham}}", strSanPham.ToString())
        //                .Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"))
        //                .Replace("{{TenKhachHang}}", order.CustomerName)
        //                .Replace("{{Phone}}", order.Phone)
        //                .Replace("{{Email}}", req.Email)
        //                .Replace("{{DiaChiNhanHang}}", order.Address)
        //                .Replace("{{ThanhTien}}", WebBanHangOnline.Common.Common.FormatNumber(thanhTien, 0))
        //                .Replace("{{TongTien}}", WebBanHangOnline.Common.Common.FormatNumber(tongTien, 0));

        //            WebBanHangOnline.Common.Common.SendMail("Gốm Sứ Bình Xuyên", $"Đơn hàng mới #{order.Code}", contentAdmin, ConfigurationManager.AppSettings["EmailAdmin"]);

        //            cart.ClearCart();

        //            if (req.TypePayment == 2)
        //            {
        //                var url = UrlPayment(req.TypePaymentVN, order.Code);
        //                result = new { Success = true, Code = req.TypePayment, Url = url };
        //            }
        //            else
        //            {
        //                result = new { Success = true, Code = req.TypePayment, Url = "" };
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            // Nếu có lỗi, rollback transaction
        //            transaction.Rollback();
        //            // Log lỗi để dễ dàng debug
        //            System.Diagnostics.Debug.WriteLine($"Lỗi khi tạo đơn hàng và trừ tồn kho: {ex.Message} - StackTrace: {ex.StackTrace}");
        //            return Json(new { Success = false, Code = -3, Url = "", Message = "Đã xảy ra lỗi trong quá trình đặt hàng. Vui lòng thử lại sau." });
        //        }
        //    } // `using` statement sẽ tự động Dispose transaction

        //    return Json(result);
        //}

        // Sửa send mail
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult CheckOut(OrderViewModel req)
        {
            var result = new { Success = false, Code = -1, Url = "" };

            if (!ModelState.IsValid)
                return Json(result);

            var cart = Session["Cart"] as ShoppingCart;
            if (cart == null || cart.Items.Count == 0)
                return Json(result);

            foreach (var item in cart.Items)
            {
                var product = db.Products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    item.AvailableQuantity = product.Quantity;
                }
            }

            Order order = null;
            var strSanPham = new StringBuilder();
            decimal thanhTien = 0;
            decimal tongTien = 0;

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    foreach (var item in cart.Items)
                    {
                        var product = db.Products.FirstOrDefault(p => p.Id == item.ProductId);
                        if (product == null || product.Quantity < item.Quantity)
                        {
                            transaction.Rollback();
                            return Json(new
                            {
                                Success = false,
                                Code = -2,
                                Url = "",
                                Message = $"Sản phẩm \"{item.ProductName}\" không đủ tồn kho. Chỉ còn lại {product?.Quantity ?? 0} sản phẩm."
                            });
                        }
                    }

                    order = new Order
                    {
                        CustomerName = req.CustomerName,
                        Phone = req.Phone,
                        Address = req.Address,
                        Email = req.Email,
                        Status = 1,
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now,
                        CreatedBy = req.Phone,
                        TypePayment = req.TypePayment,
                        Code = "DH" + new Random().Next(1000, 9999).ToString()
                    };

                    if (User.Identity.IsAuthenticated)
                        order.CustomerId = User.Identity.GetUserId();

                    foreach (var item in cart.Items)
                    {
                        order.OrderDetails.Add(new OrderDetail
                        {
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            Price = item.Price
                        });
                    }

                    order.TotalAmount = cart.Items.Sum(x => x.Price * x.Quantity);
                    order.Quantity = cart.Items.Sum(x => x.Quantity);

                    db.Orders.Add(order);
                    db.SaveChanges();

                    foreach (var item in order.OrderDetails)
                    {
                        var product = db.Products.FirstOrDefault(p => p.Id == item.ProductId);
                        if (product != null)
                        {
                            product.Quantity -= item.Quantity;
                            if (product.Quantity < 0)
                            {
                                product.Quantity = 0;
                            }
                            db.Entry(product).State = EntityState.Modified;
                        }
                    }
                    db.SaveChanges();

                    transaction.Commit();

                    foreach (var item in cart.Items)
                    {
                        strSanPham.Append("<tr>");
                        strSanPham.AppendFormat("<td>{0}</td>", item.ProductName);
                        strSanPham.AppendFormat("<td>{0}</td>", item.Quantity);
                        strSanPham.AppendFormat("<td>{0}</td>", WebBanHangOnline.Common.Common.FormatNumber(item.TotalPrice));
                        strSanPham.Append("</tr>");
                        thanhTien += item.Price * item.Quantity;
                    }

                    tongTien = thanhTien;

                    cart.ClearCart();

                    if (req.TypePayment == 2)
                    {
                        var url = UrlPayment(req.TypePaymentVN, order.Code);
                        result = new { Success = true, Code = req.TypePayment, Url = url };
                    }
                    else
                    {
                        result = new { Success = true, Code = req.TypePayment, Url = "" };
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    System.Diagnostics.Debug.WriteLine($"Lỗi khi tạo đơn hàng và trừ tồn kho: {ex.Message} - StackTrace: {ex.StackTrace}");
                    return Json(new { Success = false, Code = -3, Url = "", Message = "Đã xảy ra lỗi trong quá trình đặt hàng. Vui lòng thử lại sau." });
                }
            }

            // ✅ Tách phần gửi mail RA NGOÀI transaction để tránh rollback khi lỗi mail
            try
            {
                string contentCustomer = System.IO.File.ReadAllText(Server.MapPath("~/Content/templates/send2.html"));
                contentCustomer = contentCustomer
                    .Replace("{{MaDon}}", order.Code)
                    .Replace("{{SanPham}}", strSanPham.ToString())
                    .Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"))
                    .Replace("{{TenKhachHang}}", order.CustomerName)
                    .Replace("{{Phone}}", order.Phone)
                    .Replace("{{Email}}", req.Email)
                    .Replace("{{DiaChiNhanHang}}", order.Address)
                    .Replace("{{ThanhTien}}", WebBanHangOnline.Common.Common.FormatNumber(thanhTien, 0))
                    .Replace("{{TongTien}}", WebBanHangOnline.Common.Common.FormatNumber(tongTien, 0));

                WebBanHangOnline.Common.Common.SendMail("Gốm Sứ Bình Xuyên", $"Đơn hàng #{order.Code}", contentCustomer, req.Email);

                string contentAdmin = System.IO.File.ReadAllText(Server.MapPath("~/Content/templates/send1.html"));
                contentAdmin = contentAdmin
                    .Replace("{{MaDon}}", order.Code)
                    .Replace("{{SanPham}}", strSanPham.ToString())
                    .Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"))
                    .Replace("{{TenKhachHang}}", order.CustomerName)
                    .Replace("{{Phone}}", order.Phone)
                    .Replace("{{Email}}", req.Email)
                    .Replace("{{DiaChiNhanHang}}", order.Address)
                    .Replace("{{ThanhTien}}", WebBanHangOnline.Common.Common.FormatNumber(thanhTien, 0))
                    .Replace("{{TongTien}}", WebBanHangOnline.Common.Common.FormatNumber(tongTien, 0));

                WebBanHangOnline.Common.Common.SendMail("Gốm Sứ Bình Xuyên", $"Đơn hàng mới #{order.Code}", contentAdmin, ConfigurationManager.AppSettings["EmailAdmin"]);
            }
            catch (Exception emailEx)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi gửi email: " + emailEx.Message);
            }

            return Json(result);
        }

        //[AllowAnonymous] NGUYÊN BẢN
        //[HttpPost]
        //public ActionResult AddToCart(int id, int quantity)
        //{
        //    var code = new { Success = false, msg = "", code = -1, Count = 0 };
        //    var db = new ApplicationDbContext();
        //    var checkProduct = db.Products.FirstOrDefault(x => x.Id == id);
        //    if (checkProduct != null)
        //    {
        //        ShoppingCart cart = (ShoppingCart)Session["Cart"];
        //        if (cart == null)
        //        {
        //            cart = new ShoppingCart();
        //        }
        //        ShoppingCartItem item = new ShoppingCartItem
        //        {
        //            ProductId = checkProduct.Id,
        //            ProductName = checkProduct.Title,
        //            CategoryName = checkProduct.ProductCategory.Title,
        //            Alias = checkProduct.Alias,
        //            Quantity = quantity
        //        };
        //        if (checkProduct.ProductImage.FirstOrDefault(x => x.IsDefault) != null)
        //        {
        //            item.ProductImg = checkProduct.ProductImage.FirstOrDefault(x => x.IsDefault).Image;
        //        }
        //        item.Price = checkProduct.Price;
        //        if (checkProduct.PriceSale > 0)
        //        {
        //            item.Price = (decimal)checkProduct.PriceSale;
        //        }
        //        item.TotalPrice = item.Quantity * item.Price;
        //        cart.AddToCart(item, quantity);
        //        Session["Cart"] = cart;
        //        code = new { Success = true, msg = "Thêm Sản Phẩm Vào Giỏ Hàng Thành Công", code = 1, Count = cart.Items.Count };
        //    }
        //    return Json(code);
        //}

        [AllowAnonymous]
        [HttpPost]
        public ActionResult AddToCart(int id, int quantity)
        {
            var code = new { Success = false, msg = "", code = -1, Count = 0 };
            var db = new ApplicationDbContext();
            var checkProduct = db.Products.FirstOrDefault(x => x.Id == id);
            if (checkProduct != null)
            {
                ShoppingCart cart = Session["Cart"] as ShoppingCart;
                if (cart == null)
                {
                    cart = new ShoppingCart();
                }

                // Kiểm tra tồn kho trước khi thêm
                if (quantity > checkProduct.Quantity)
                {
                    return Json(new
                    {
                        Success = false,
                        msg = $"Không thể thêm quá số lượng tồn kho. Hiện còn {checkProduct.Quantity} sản phẩm.",
                        code = -2,
                        Count = cart.Items.Count
                    });
                }

                ShoppingCartItem item = new ShoppingCartItem
                {
                    ProductId = checkProduct.Id,
                    ProductName = checkProduct.Title,
                    CategoryName = checkProduct.ProductCategory.Title,
                    Alias = checkProduct.Alias,
                    Quantity = quantity,
                    AvailableQuantity = checkProduct.Quantity // ✅ Gán tồn kho vào đây
                };

                if (checkProduct.ProductImage.FirstOrDefault(x => x.IsDefault) != null)
                {
                    item.ProductImg = checkProduct.ProductImage.FirstOrDefault(x => x.IsDefault).Image;
                }

                item.Price = checkProduct.Price;
                if (checkProduct.PriceSale > 0)
                {
                    item.Price = (decimal)checkProduct.PriceSale;
                }

                item.TotalPrice = item.Quantity * item.Price;

                cart.AddToCart(item, quantity); // Thêm vào hoặc tăng số lượng

                Session["Cart"] = cart;

                code = new
                {
                    Success = true,
                    msg = "Thêm Sản Phẩm Vào Giỏ Hàng Thành Công",
                    code = 1,
                    Count = cart.Items.Count
                };
            }
            return Json(code);
        }

        //Test thử check cart ngoài giao diện, chống người dùng thêm sản phẩm nhiều hơn số lượng hiện có trong kho
        [HttpPost]
        [AllowAnonymous]
        public JsonResult CheckQuantity(int id)
        {
            var db = new ApplicationDbContext();
            var product = db.Products.FirstOrDefault(x => x.Id == id);
            if (product == null) return Json(new { Quantity = 0 });

            int inCartQty = 0;
            var cart = Session["Cart"] as ShoppingCart;
            if (cart != null)
            {
                var existing = cart.Items.FirstOrDefault(x => x.ProductId == id);
                if (existing != null)
                {
                    inCartQty = existing.Quantity;
                }
            }

            return Json(new { Quantity = inCartQty });
        }

        //[AllowAnonymous]
        //[HttpPost]
        //public ActionResult Update(int id, int quantity)
        //{
        //    ShoppingCart cart = (ShoppingCart)Session["Cart"];
        //    if (cart != null)
        //    {
        //        cart.UpdateQuantity(id, quantity);
        //        return Json(new { Success = true });
        //    }
        //    return Json(new { Success = false });
        //}
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Update(int id, int quantity)
        {
            var db = new ApplicationDbContext();
            var product = db.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return Json(new { Success = false, Message = "Sản phẩm không tồn tại." });
            }

            if (quantity > product.Quantity)
            {
                return Json(new
                {
                    Success = false,
                    Message = $"Số lượng yêu cầu vượt quá tồn kho. Hiện chỉ còn {product.Quantity} sản phẩm."
                });
            }

            ShoppingCart cart = Session["Cart"] as ShoppingCart;
            if (cart != null)
            {
                cart.UpdateQuantity(id, quantity);
                Session["Cart"] = cart;
                return Json(new { Success = true });
            }

            return Json(new { Success = false, Message = "Không tìm thấy giỏ hàng." });
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var code = new { Success = false, msg = "", code = -1, Count = 0 };
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null)
            {
                var checkProduct = cart.Items.FirstOrDefault(x => x.ProductId == id);
                if (checkProduct != null)
                {
                    cart.Remove(id);
                    code = new { Success = true, msg = "", code = 1, Count = cart.Items.Count };
                }
            }
            return Json(code);
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult DeleteAll()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null)
            {
                cart.ClearCart();
                return Json(new { Success = true });
            }
            return Json(new { Success = false });
        }

        #region Thanh toán VN Pay
        public string UrlPayment(int TypePaymentVN, string orderCode)
        {
            var urlPayment = "";
            var order = db.Orders.FirstOrDefault(x => x.Code == orderCode);
            //Get Config Info
            string vnp_Returnurl = ConfigurationManager.AppSettings["vnp_Returnurl"]; //URL nhan ket qua tra ve 
            string vnp_Url = ConfigurationManager.AppSettings["vnp_Url"]; //URL thanh toan cua VNPAY 
            string vnp_TmnCode = ConfigurationManager.AppSettings["vnp_TmnCode"]; //Ma định danh merchant kết nối (Terminal Id)
            string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"]; //Secret Key

            //Build URL for VNPAY
            VnPayLibrary vnpay = new VnPayLibrary();
            var Price = (long)order.TotalAmount * 100;
            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", Price.ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000
            if (TypePaymentVN == 1)
            {
                vnpay.AddRequestData("vnp_BankCode", "VNPAYQR");
            }
            else if (TypePaymentVN == 2)
            {
                vnpay.AddRequestData("vnp_BankCode", "VNBANK");
            }
            else if (TypePaymentVN == 3)
            {
                vnpay.AddRequestData("vnp_BankCode", "INTCARD");
            }

            vnpay.AddRequestData("vnp_CreateDate", order.CreatedDate.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress());
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toán đơn hàng :" + order.Code);
            vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other

            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", order.Code); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày

            //Add Params of 2.1.0 Version
            //Billing

            urlPayment = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            //log.InfoFormat("VNPAY URL: {0}", paymentUrl);
            return urlPayment;
        }
        #endregion
    }
}
