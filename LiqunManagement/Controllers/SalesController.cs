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
            #region 使用者資料
            var EmployeeData = (from db in memberdb.Members.Where(x => x.Account == User.Identity.Name)
                                join empdb in memberdb.EmployeeData on db.Account equals empdb.Account into temp
                                from empdb0 in temp.DefaultIfEmpty()
                                select new MembersViewModel
                                {
                                    Name = db.Name,
                                    Department = empdb0 != null ? empdb0.Department : null,
                                    Position = empdb0 != null ? empdb0.Position : null,
                                }).FirstOrDefault();
            if (EmployeeData != null)
            {
                ViewBag.UserName = EmployeeData.Name;                 //使用者名稱
                ViewBag.Department = EmployeeData.Department;   //使用者部門
                ViewBag.Position = EmployeeData.Position;       //使用者職位
            }
            //確認角色
            var role = User.IsInRole("Admin");
            //var role2 = User.IsInRole("User");
            #endregion
            var Formlist = from form in formdb.ObjectForm
                           join obj in formdb.HomeObject on form.FormID equals obj.FormID
                           join lan in formdb.LandLord on form.FormID equals lan.FormID into temp1
                           from land in temp1.DefaultIfEmpty()
                           join ten in formdb.Tenant on form.FormID equals ten.FormID into temp2
                           from tena in temp2.DefaultIfEmpty()
                           select new objectForm
                           {
                               FormId = (string)form.FormID,
                               CreateTime = (DateTime)form.CreateTime,
                               ProcessName = (string)form.ProcessName,
                               Address = (string)obj.fulladdress,
                               SignDate = (DateTime)obj.signdate,
                               Landlord = land != null ? land.Name : null,
                               Tenant = tena != null ? tena.Name : null,
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