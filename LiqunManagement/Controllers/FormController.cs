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
            List<int> payment_date = new List<int>();

            for(int i=1; i<32; i++)
            {
                payment_date.Add(i);
            }
            ViewBag.Payment_date = payment_date;
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
            string Inline1Radio3else,
            HttpPostedFileBase taxfile,
            string rent,
            string deposit,
            string management_fee,
            DateTime startdate,
            DateTime enddate,
            int paydate,
            string Inline1Radio4,
            string Inline1Radio5,
            string housenumber,
            string hallnumber,
            string bathnumber,
            string Inline1Radio6,
            string Inline1Radio7,
            string Inline1Radio8,
            string carpositionnumber,
            string carmonthrent,
            string scootermonthrent,
            string parkmanagementfee,
            string scootermanagementfee
            )
        {
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