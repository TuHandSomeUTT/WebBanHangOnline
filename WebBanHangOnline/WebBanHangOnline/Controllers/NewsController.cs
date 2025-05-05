using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;

namespace WebBanHangOnline.Controllers
{
    public class NewsController : Controller
    {
        public ApplicationDbContext db = new ApplicationDbContext();
        // GET: News
        public ActionResult Index()
        {
            var items = db.News.ToList();
            return View(items);
        }

        public ActionResult Detail(int id)
        {
            var item = db.News.Find(id);
            return View(item);
        }

        public ActionResult Partial_News_Home()
        {
            var items = db.News.Take(3).ToList(); //3 bản ghi đầu tiên
            return PartialView(items);
        }
    }
}