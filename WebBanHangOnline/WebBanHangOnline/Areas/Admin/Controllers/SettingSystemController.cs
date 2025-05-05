using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    public class SettingSystemController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/SettingSystem
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Partial_Setting()
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult AddSetting(SettingSystemViewModel req)
        {
            SystemSetting set = null;
            //title
            var checkTite = db.SystemSettings.FirstOrDefault(x => x.SettingKey == "SettingTitle");
            if (checkTite == null)
            {
                set = new SystemSetting();
                set.SettingKey = "SettingTitle";
                set.SettingValue = req.SettingTitle;
                db.SystemSettings.Add(set);
            }
            else
            {
                checkTite.SettingValue = req.SettingTitle;
                db.Entry(checkTite).State = System.Data.Entity.EntityState.Modified;
            }
            //logo
            var checkLogo = db.SystemSettings.FirstOrDefault(x => x.SettingKey == "SettingLogo");
            if (checkLogo == null)
            {
                set = new SystemSetting();
                set.SettingKey = "SettingLogo";
                set.SettingValue = req.SettingLogo;
                db.SystemSettings.Add(set);
            }
            else
            {
                checkLogo.SettingValue = req.SettingLogo;
                db.Entry(checkLogo).State = System.Data.Entity.EntityState.Modified;
            }
            //email
            var email = db.SystemSettings.FirstOrDefault(x => x.SettingKey == "SettingEmail");
            if (email == null)
            {
                set = new SystemSetting();
                set.SettingKey = "SettingEmail";
                set.SettingValue = req.SettingEmail;
                db.SystemSettings.Add(set);
            }
            else
            {
                email.SettingValue = req.SettingEmail;
                db.Entry(email).State = System.Data.Entity.EntityState.Modified;
            }
            //hotline
            var hotline = db.SystemSettings.FirstOrDefault(x => x.SettingKey == "SettingHotline");
            if (hotline == null)
            {
                set = new SystemSetting();
                set.SettingKey = "SettingHotline";
                set.SettingValue = req.SettingHotline;
                db.SystemSettings.Add(set);
            }
            else
            {
                hotline.SettingValue = req.SettingHotline;
                db.Entry(hotline).State = System.Data.Entity.EntityState.Modified;
            }
            //titleSEO
            var titleSEO = db.SystemSettings.FirstOrDefault(x => x.SettingKey == "SettingTitleSeo");
            if (titleSEO == null)
            {
                set = new SystemSetting();
                set.SettingKey = "SettingTitleSeo";
                set.SettingValue = req.SettingTitleSeo;
                db.SystemSettings.Add(set);
            }
            else
            {
                titleSEO.SettingValue = req.SettingTitleSeo;
                db.Entry(titleSEO).State = System.Data.Entity.EntityState.Modified;
            }
            //DesSEO
            var DesSEO = db.SystemSettings.FirstOrDefault(x => x.SettingKey == "SettingDesSeo");
            if (DesSEO == null)
            {
                set = new SystemSetting();
                set.SettingKey = "SettingDesSeo";
                set.SettingValue = req.SettingDesSeo;
                db.SystemSettings.Add(set);
            }
            else
            {
                DesSEO.SettingValue = req.SettingDesSeo;
                db.Entry(DesSEO).State = System.Data.Entity.EntityState.Modified;
            }
            //KeySEO
            var KeySEO = db.SystemSettings.FirstOrDefault(x => x.SettingKey == "SettingKeySeo");
            if (KeySEO == null)
            {
                set = new SystemSetting();
                set.SettingKey = "SettingKeySeo";
                set.SettingValue = req.SettingKeySeo;
                db.SystemSettings.Add(set);
            }
            else
            {
                KeySEO.SettingValue = req.SettingKeySeo;
                db.Entry(KeySEO).State = System.Data.Entity.EntityState.Modified;
            }
            db.SaveChanges();
            return View("Partial_Setting");
        }
    }
}