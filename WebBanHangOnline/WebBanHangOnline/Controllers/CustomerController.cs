using System.Linq;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // [PartialView] – Hiển thị danh sách đơn hàng trong trang Profile
        public PartialViewResult OrderHistory()
        {
            var username = User.Identity.Name;
            var user = db.Users.FirstOrDefault(x => x.UserName == username);

            var orders = db.Orders
                .Where(x => x.CustomerId == user.Id)
                .OrderByDescending(x => x.CreatedDate)
                .ToList();

            return PartialView(orders); // Dùng Html.Partial hoặc Html.Action ở View Profile
        }

        // [View] Trang chi tiết đơn hàng
        [Authorize]
        public ActionResult Details(int id)
        {
            var username = User.Identity.Name;
            var user = db.Users.FirstOrDefault(x => x.UserName == username);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = db.Orders
                .Include("OrderDetails.Product")
                .FirstOrDefault(x => x.Id == id && x.CustomerId == user.Id);

            if (order == null)
            {
                return HttpNotFound("Đơn hàng không tồn tại hoặc không thuộc về bạn.");
            }

            return View(order);
        }

    }
}
