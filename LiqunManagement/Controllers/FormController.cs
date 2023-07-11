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

        public ActionResult HomeObject()
        {
            DDLServices ddlservices = new DDLServices();
            var formmodel = ddlservices.GetRegionDDL("");
            ViewBag.citylist = JsonConvert.SerializeObject(formmodel.regionddl.ToList());

            return View();
        }

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