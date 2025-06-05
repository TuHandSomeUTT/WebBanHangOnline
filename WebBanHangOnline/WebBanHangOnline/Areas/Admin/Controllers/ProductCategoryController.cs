using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;
using PagedList;
namespace WebBanHangOnline.Areas.Admin.Controllers
{
    public class ProductCategoryController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/ProductCategory
        //public ActionResult Index()
        //{
        //    var items = db.ProductCategories;
        //    return View(items);
        //}

        public ActionResult Index(string searchText, int? page)
        {
            var items = db.ProductCategories.AsQueryable();

            if (!string.IsNullOrEmpty(searchText))
            {
                searchText = searchText.ToLower();
                items = items.Where(x => x.Title.ToLower().Contains(searchText));
            }

            int pageSize = 5;
            int pageNumber = page ?? 1;

            ViewBag.SearchText = searchText;
            ViewBag.PageSize = pageSize;
            ViewBag.Page = pageNumber;

            return View(items.OrderByDescending(x => x.Id).ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(ProductCategory model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedDate = DateTime.Now;
                model.ModifiedDate = DateTime.Now;
                model.Alias = WebBanHangOnline.Models.Common.Filter.FilterChar(model.Title);
                db.ProductCategories.Add(model);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }
    }
}