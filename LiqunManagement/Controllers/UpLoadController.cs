using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LiqunManagement.Controllers
{
    public class UpLoadController : Controller
    {
        [HttpGet]
        // GET: UpLoad
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        #region 上傳Region資料
        public ActionResult UploadRegion()
        {

            return View();
        }
        #endregion


        [HttpPost]
        #region 上傳Region資料
        public ActionResult UploadRegion(HttpPostedFileBase file)
        {
            var ddd = 1;
            return Json("");
        }
        #endregion
    }
}