using LiqunManagement.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Management;
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
        [HttpPost]
        public ActionResult Index(string Inline1Radio1, DateTime signdate, string Inline1Radio2, string Specialtext)
        {
            if (ModelState.IsValid)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Memebers");
            }
        }

        #region 房屋物件資料
        public ActionResult HomeObject()
        {
            DDLServices ddlservices = new DDLServices();
            var formmodel = ddlservices.GetRegionDDL("");
            ViewBag.citylist = JsonConvert.SerializeObject(formmodel.regionddl.ToList());

            return View();
        }

        [HttpPost]
        public ActionResult HomeObject(
            string Inline1Radio1, 
            DateTime signdate, 
            string Inline1Radio2, 
            string Specialtext, 
            string cityselect,
            string district,
            string road,
            string Inline1Radio3,
            string Inline1Radio3else
            )
        {
            var taiwan = Specialtext;
            if (ModelState.IsValid)
            {
                return RedirectToAction("HomeObject", "Form");
            }
            else
            {
                return RedirectToAction("Login", "Memebers");
            }
        }
        #endregion

        #region 找到Region下拉選單
        public ActionResult DDLRegion(string regioncode)
        {
            DDLServices ddlservices = new DDLServices();
            var formmodel = ddlservices.GetRegionDDL(regioncode);

            var regionJson = JsonConvert.SerializeObject(formmodel.regionddl.ToList());
            return Json(regionJson);
        }
        #endregion

    }
}