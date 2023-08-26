using LiqunManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LiqunManagement.Controllers
{
    public class SalesController : BaseController
    {
        // GET: Sales
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult CaseManage()
        {
            var Formlist = from form in formdb.ObjectForm
                           join obj in formdb.HomeObject on form.FormId equals obj.FormId
                           select new objectForm
                           {
                               FormId = (string)form.FormId,
                               CreateTime = (DateTime)form.CreateTime,
                               ProcessName = (string)form.ProcessName,
                               Address = (string)obj.fulladdress,
                               SignDate = (DateTime)obj.signdate,
                               Landlord = "陳霸天",
                               Tenant = "小蘋果",
                           };
            //ViewBag.Formlist = Formlist;

            var model = new FormViewModels
            {
                objectformlist = Formlist,
            };

            return View(model);
        }
    }
}