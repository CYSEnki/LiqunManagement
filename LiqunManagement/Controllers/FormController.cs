using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LiqunManagement.Controllers
{
    public class FormController : Controller
    {
        // GET: Form
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult HomeObject()
        {
            return View();
        }

        #region 找到Region下拉選單
        public ActionResult DDLRegion()
        {

            return Json("");
        }
        #endregion

    }
}