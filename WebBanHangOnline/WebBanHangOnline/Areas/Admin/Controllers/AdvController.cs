using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;
using PagedList;
using System.Net; // Đảm bảo bạn có using này

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class AdvController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/Posts
        //public ActionResult Index(int? page)
        //{
        //    int pageSize = 10; // Số lượng item mỗi trang
        //    int pageNumber = page ?? 1; // Nếu page = null thì mặc định là 1

        //    var items = db.Advs.OrderByDescending(x => x.CreatedDate).ToPagedList(pageNumber, pageSize);// Sắp xếp nếu muốn

        //    ViewBag.Page = pageNumber;
        //    ViewBag.PageSize = pageSize;

        //    return View(items);
        //}
        public ActionResult Index(string SearchText, int? page)
        {
            int pageSize = 10; // Số lượng item mỗi trang
            int pageNumber = page ?? 1; // Nếu page = null thì mặc định là 1

            var items = db.Advs.AsQueryable();

            if (!string.IsNullOrEmpty(SearchText))
            {
                items = items.Where(x => x.Title.Contains(SearchText));
            }

            var pagedItems = items.OrderByDescending(x => x.CreatedDate).ToPagedList(pageNumber, pageSize);// Sắp xếp nếu muốn

            ViewBag.Page = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.SearchText = SearchText; // để giữ lại giá trị tìm kiếm khi reload view

            return View(pagedItems);
        }

        //public ActionResult Add()
        //{
        //    return View();
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Add(Adv model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        model.CreatedDate = DateTime.Now;
        //        model.ModifiedDate = DateTime.Now;
        //        db.Advs.Add(model);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    return View(model);
        //}
        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(Adv model)
        {
            if (model == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                if (ModelState.IsValid)
                {
                    model.CreatedDate = DateTime.Now;
                    model.ModifiedDate = DateTime.Now;
                    db.Advs.Add(model);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
            }

            return View(model);
        }

        //public ActionResult Edit(int id)
        //{
        //    var item = db.Advs.Find(id);
        //    return View(item);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(Adv model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        model.ModifiedDate = DateTime.Now;
        //        db.Advs.Attach(model);
        //        db.Entry(model).State = System.Data.Entity.EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    return View(model);
        //}
        public ActionResult Edit(int id)
        {
            var item = db.Advs.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Adv model)
        {
            if (ModelState.IsValid)
            {
                var existing = db.Advs.Find(model.Id);
                if (existing == null)
                {
                    return HttpNotFound();
                }

                // Cập nhật thủ công từng trường để tránh ghi đè dữ liệu không mong muốn
                existing.Title = model.Title;
                existing.Description = model.Description;
                existing.Image = model.Image;
                existing.Link = model.Link;
                existing.Type = model.Type;
                existing.IsActive = model.IsActive;
                existing.ModifiedDate = DateTime.Now;

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = db.Advs.Find(id);
            if (item != null)
            {
                db.Advs.Remove(item);
                db.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }


        [HttpPost]
        public ActionResult DeleteAll(string ids)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                var items = ids.Split(',');
                if (items != null && items.Any())
                {
                    foreach (var item in items)
                    {
                        var obj = db.Advs.Find(Convert.ToInt32(item));
                        db.Advs.Remove(obj);
                        db.SaveChanges();
                    }
                }
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}