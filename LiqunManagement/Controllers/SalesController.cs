using LiqunManagement.Models;
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

        #region 管理案件(業務)
        [HttpGet]
        public ActionResult CaseManage()
        {
            var ErrorMessage = TempData["ErrorMessage"];
            ViewBag.ErrorMessage = ErrorMessage != null ? ErrorMessage.ToString() : "";

            #region 使用者資料
            var EmployeeData = (from db in memberdb.Members.Where(x => x.Account == User.Identity.Name)
                                join empdb in memberdb.EmployeeData on db.Account equals empdb.Account into temp
                                from empdb0 in temp.DefaultIfEmpty()
                                join deptdb in memberdb.Department on empdb0.DivCode equals deptdb.DivCode into temp2
                                from deptdb0 in temp2.DefaultIfEmpty()
                                select new MembersViewModel
                                {
                                    Name = db.Name,
                                    Department = empdb0 != null ? deptdb0.DivFullName : null,
                                    Position = empdb0 != null ? empdb0.JobTitle : null,
                                }).FirstOrDefault();
            if (EmployeeData != null)
            {
                ViewBag.UserName = EmployeeData.Name;                 //使用者名稱
                ViewBag.Department = EmployeeData.Department;   //使用者部門
                ViewBag.Position = EmployeeData.Position;       //使用者職位
            }
            //確認角色
            var role = User.IsInRole("Admin");
            ViewBag.Role = User.IsInRole("Admin") ? "Admin" : User.IsInRole("Agent") ? "Agent" : User.IsInRole("Secretary") ? "Secretary" : "";
            #endregion
            var ObjectFormData = formdb.ObjectForm.Where(x => x.FormType == 0).AsEnumerable();
            if (!role)
            {
                //非系統管理員
                ObjectFormData = ObjectFormData.Where(x => x.AgentAccount == User.Identity.Name);
            }

            var Formlist = from form in ObjectFormData
                           join obj in formdb.HomeObject on form.FormID equals obj.FormID
                           join lan in formdb.LandLord on form.FormID equals lan.FormID into temp1
                           from land in temp1.DefaultIfEmpty()
                           join ten in formdb.Tenant on form.FormID equals ten.FormID into temp2
                           from tena in temp2.DefaultIfEmpty()
                           select new objectFormViewModel
                           {
                               FormID = (string)form.FormID,
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
        [HttpPost]
        public ActionResult CaseManage(string FormID)
        {
            var IsAdmin = User.IsInRole("Admin");

            var AgentAccount = formdb.ObjectForm.Where(x => x.FormID == FormID).Select(x => x.AgentAccount).FirstOrDefault();
            var AssistantAccount = memberdb.EmployeeData.Where(x => x.Account == AgentAccount).Select(x => x.AssistantAccount).FirstOrDefault();
            var AssistantData = memberdb.Members.Where(x => x.Account == AssistantAccount).FirstOrDefault();
            
            var userName = memberdb.Members.Where(x => x.Account == User.Identity.Name).Select(x => x.Name).FirstOrDefault();

            try
            {
                if(!String.IsNullOrEmpty(AssistantAccount))
                {
                    //變更表單狀態
                    using (var context = new FormModels())
                    {
                        var existform = context.ObjectForm.Where(x => x.FormID == FormID).FirstOrDefault();
                        existform.ProcessAccount = AssistantAccount;
                        existform.ProcessName = AssistantData.Name;
                        existform.FormType = 1;

                        context.SaveChanges();
                    }
                }
                else if (IsAdmin)
                {
                    //變更表單狀態
                    using (var context = new FormModels())
                    {
                        var existform = context.ObjectForm.Where(x => x.FormID == FormID).FirstOrDefault();
                        existform.ProcessAccount = User.Identity.Name;
                        existform.ProcessName = userName;
                        existform.FormType = 1;

                        context.SaveChanges();
                    }
                }
            }
            catch(Exception ex)
            {
                var error = ex.ToString();
            }
            return Json("successed");
        }
        #endregion

        #region 待結案區
        [HttpGet]
        public ActionResult CaseProcess()
        {
            #region 使用者資料
            var EmployeeData = (from db in memberdb.Members.Where(x => x.Account == User.Identity.Name)
                                join empdb in memberdb.EmployeeData on db.Account equals empdb.Account into temp
                                from empdb0 in temp.DefaultIfEmpty()
                                join deptdb in memberdb.Department on empdb0.DivCode equals deptdb.DivCode into temp2
                                from deptdb0 in temp2.DefaultIfEmpty()
                                select new MembersViewModel
                                {
                                    Name = db.Name,
                                    Department = empdb0 != null ? deptdb0.DivFullName : null,
                                    Position = empdb0 != null ? empdb0.JobTitle : null,
                                }).FirstOrDefault();
            if (EmployeeData != null)
            {
                ViewBag.UserName = EmployeeData.Name;                 //使用者名稱
                ViewBag.Department = EmployeeData.Department;   //使用者部門
                ViewBag.Position = EmployeeData.Position;       //使用者職位
            }
            //確認角色
            var role = User.IsInRole("Admin");
            ViewBag.Role = User.IsInRole("Admin") ? "Admin" : User.IsInRole("Agent") ? "Agent" : User.IsInRole("Secretary") ? "Secretary" : "";
            #endregion
            var ObjectFormData = formdb.ObjectForm.Where(x => x.FormType == 1 || x.FormType == 2).AsEnumerable();
            if (!role)
            {
                //非系統管理員
                ObjectFormData = ObjectFormData.Where(x => x.AgentAccount == User.Identity.Name);
            }

            var Formlist = from form in ObjectFormData
                           join obj in formdb.HomeObject on form.FormID equals obj.FormID
                           join lan in formdb.LandLord on form.FormID equals lan.FormID into temp1
                           from land in temp1.DefaultIfEmpty()
                           join ten in formdb.Tenant on form.FormID equals ten.FormID into temp2
                           from tena in temp2.DefaultIfEmpty()
                           select new objectFormViewModel
                           {
                               FormID = (string)form.FormID,
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
        #endregion
    }
}