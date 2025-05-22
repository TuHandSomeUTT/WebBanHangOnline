using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;

namespace WebBanHangOnline.Controllers
{
    public class ArticleController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Article
        public ActionResult Index(string alias)
        {
            var item = db.Posts.FirstOrDefault(x => x.Alias == alias);
            // Truy vấn bài viết từ cơ sở dữ liệu theo alias được truyền từ route URL, tìm bài viết đầu tiên trong bảng Posts có Alias trùng với tham số URL
            return View(item);
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}