﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    public class ProductImageController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/ProductImages
        public ActionResult Index(int id)
        {
            ViewBag.ProductId = id;
            var items = db.ProductImages.Where(x => x.ProductId == id).ToList();
            return View(items);
        }

        [HttpPost]
        public ActionResult AddImage(int productId, string url)
        {
            db.ProductImages.Add(new ProductImage {
                ProductId = productId,
                Image = url,
                IsDefault = false
            });
            db.SaveChanges();
            return Json(new { Success = true });
        }
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = db.ProductImages.Find(id);
            db.ProductImages.Remove(item);
            db.SaveChanges();
            return Json(new { success = true});
        }

        [HttpPost]
        public ActionResult SetDefault(int id)
        {
            var image = db.ProductImages.Find(id);
            if (image != null)
            {
                // Reset tất cả ảnh của sản phẩm về IsDefault = false
                var allImages = db.ProductImages.Where(x => x.ProductId == image.ProductId).ToList();
                foreach (var img in allImages)
                {
                    img.IsDefault = false;
                }

                // Đặt ảnh được chọn thành default
                image.IsDefault = true;
                db.SaveChanges();

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

    }
}