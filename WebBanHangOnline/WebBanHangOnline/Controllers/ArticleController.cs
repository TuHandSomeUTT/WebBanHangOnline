using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Controllers
{
    public class ArticleController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Article
        //public ActionResult Index(string alias)
        //{
        //    var item = db.Posts.FirstOrDefault(x => x.Alias == alias);
        //    // Truy vấn bài viết từ cơ sở dữ liệu theo alias được truyền từ route URL, tìm bài viết đầu tiên trong bảng Posts có Alias trùng với tham số URL
        //    return View(item);
        //}

        public ActionResult Index(int? page)
        {
            var pageSize = 5; // 5 record per page
            if (page == null)
            {
                page = 1;
            }
            IEnumerable<Posts> items = db.Posts.OrderByDescending(x => x.CreatedDate);
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            items = items.ToPagedList(pageIndex, pageSize);//bản ghi mới nhất sẽ lên đầu
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;
            return View(items);
        }

        public ActionResult Detail(string alias, int id)
        {
            var item = db.Posts.Find(id);
            if (item == null) return HttpNotFound();
            return View(item);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}