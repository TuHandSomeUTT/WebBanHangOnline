using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/Order
        //public ActionResult Index(int? page)
        //{
        //    var items = db.Orders.OrderByDescending(x=>x.CreatedDate).ToList();

        //    if (page == null)
        //    {
        //        page = 1;
        //    }
        //    var pageNumber = page ?? 1;
        //    var pageSize = 10;
        //    ViewBag.PageSize = pageSize;
        //    ViewBag.Page = pageNumber;
        //    return View(items.ToPagedList(pageNumber, pageSize));
        //}
        public ActionResult Index(string searchText, int? page)
        {
            var items = db.Orders.AsQueryable();

            if (!string.IsNullOrEmpty(searchText))
            {
                searchText = searchText.ToLower();
                items = items.Where(x =>
                    x.Code.ToLower().Contains(searchText) ||
                    x.CustomerName.ToLower().Contains(searchText) ||
                    x.Phone.ToLower().Contains(searchText) ||
                    x.Email.ToLower().Contains(searchText) ||
                    x.Address.ToLower().Contains(searchText)
                );
            }

            items = items.OrderByDescending(x => x.CreatedDate);

            var pageSize = 10;
            var pageNumber = page ?? 1;

            ViewBag.PageSize = pageSize;
            ViewBag.Page = pageNumber;
            ViewBag.SearchText = searchText;

            return View(items.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult View(int id)
        {
            var item = db.Orders.Find(id);
            return View(item);
        }

        public ActionResult Partial_SanPham(int id)
        {
            var items = db.OrderDetails.Where(x => x.OrderId == id).ToList();
            return PartialView(items);
        }

        [HttpPost]
        public ActionResult UpdateTT(int id, int trangthai)
        {
            var item = db.Orders.Find(id);
            if (item != null)
            {
                db.Orders.Attach(item);
                item.Status = trangthai;
                db.Entry(item).Property(x => x.Status).IsModified = true;
                db.SaveChanges();
                return Json(new { message = "Success", Success = true });
            }
            return Json(new { message = "Unsuccess", Success = false });
        }
    }
}