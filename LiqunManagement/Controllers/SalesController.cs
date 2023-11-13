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
        /// <summary>
        /// 業務起單中，擬稿中
        /// </summary>
        /// <param name="formtype">-1(解約) 0(業務起單中) 1(秘書審查中) 2(結案，合約履行中) 3(續約中)</param>
        /// <param name="casetype">0(擬稿中) 1(完稿)</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult CaseManage(int formtype = 0, int casetype = 0)
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
            var admin = User.IsInRole("Admin");
            ViewBag.Role = User.IsInRole("Admin") ? "Admin" : User.IsInRole("Agent") ? "Agent" : User.IsInRole("Secretary") ? "Secretary" : "";
            #endregion
            var ObjectFormData = formdb.ObjectForm.Where(x => x.FormType == formtype).AsEnumerable();
            if (!admin)
            {
                //非系統管理員
                ObjectFormData = ObjectFormData.Where(x => x.AgentAccount == User.Identity.Name);
            }

            //因目前系統內casetype都等於1
            if (casetype == 0)
                casetype = 1;

            var Formlist = from form in ObjectFormData
                           join obj in formdb.HomeObject.Where(x => x.CaseType == casetype) on form.FormID equals obj.FormID
                           join lan in formdb.LandLord on obj.CaseID equals lan.CaseID into temp1
                           from land in temp1.DefaultIfEmpty()
                           join ten in formdb.Tenant on obj.CaseID equals ten.CaseID into temp2
                           from tena in temp2.DefaultIfEmpty()
                           select new objectFormViewModel
                           {
                               FormID = (string)form.FormID,
                               CaseID = (string)obj.CaseID,
                               CreateTime = (DateTime)form.CreateTime,
                               ProcessName = (string)form.ProcessName,
                               Address = (string)obj.fulladdress,
                               SignDate = (DateTime)obj.signdate,
                               Landlord = land != null ? land.Name : null,
                               Tenant = tena != null ? tena.Name : null,

                               FormType = form.FormType,
                               CaseType = obj.CaseType,
                           };
            //ViewBag.Formlist = Formlist;

            ViewBag.FormType = formtype;
            ViewBag.CaseType = casetype;
            var model = new FormViewModels
            {
                objectformlist = Formlist,
            };

            return View(model);
        }
        
        /// <summary>
        /// 送出簽核(提交至秘書端)
        /// </summary>
        /// <param name="CaseID">媒合編號</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CaseManage(string CaseID)
        {
            var IsAdmin = User.IsInRole("Admin");

            var FormID = formdb.HomeObject.Where(x => x.CaseID == CaseID).Select(x => x.FormID).FirstOrDefault();
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

                        var existHomeObject = context.HomeObject.Where(x => x.CaseID == CaseID).FirstOrDefault();
                        existHomeObject.CaseType = 1;

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
        /// <summary>
        /// 已完稿的所有表單
        /// </summary>
        /// <param name="formtype">-1(解約) 0(業務起單中) 1(秘書審查中) 2(結案，合約履行中) 3(續約中)</param>
        /// <param name="casetype">0(擬稿中) 1(完稿)</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult CaseProcess(int formtype = 1, int casetype = 1)
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

            var ObjectFormData = formdb.ObjectForm.Where(x => x.FormType == formtype).AsEnumerable();
                
            if (!role)
            {
                //非系統管理員
                ObjectFormData = ObjectFormData.Where(x => x.AgentAccount == User.Identity.Name);
            }

            var Formlist = (from form in ObjectFormData
                           join obj in formdb.HomeObject.Where(x => x.CaseType == casetype) on form.FormID equals obj.FormID
                           join lan in formdb.LandLord on obj.CaseID equals lan.CaseID into temp1
                           from land in temp1.DefaultIfEmpty()
                           join ten in formdb.Tenant on obj.CaseID equals ten.CaseID into temp2
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
                           }).ToList();
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