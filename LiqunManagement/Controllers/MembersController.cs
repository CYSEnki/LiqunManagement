﻿using LiqunManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using LiqunManagement.Services;
using LiqunManagement.Security;
using System.Web.Configuration;
using System.Data;
using LiqunManagement.Models;
using System.Linq.Expressions;

namespace LiqunManagement.Controllers
{
    [Authorize]
    public class MembersController : BaseController
    {
        //宣告Members資料表的Service物件
        private readonly MembersDBService membersdbservice = new MembersDBService();
        //宣告寄信用的Service物件
        private readonly MailService mailservice = new MailService();

        [Authorize]
        // GET: Members
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        #region 帳號設定(註冊)
        public ActionResult Register()
        {
            //判斷使用者是否已經過登入驗證
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Members");

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

            if (!role)
                return RedirectToAction("Index", "Liqun");
            #endregion

            var members = from db in memberdb.Members.Where(x => String.IsNullOrEmpty(x.AuthCode))
                          join empdb in memberdb.EmployeeData on db.Account equals empdb.Account into temp
                          from empdb0 in temp.DefaultIfEmpty()
                          join deptdb in memberdb.Department on empdb0.DivCode equals deptdb.DivCode into temp2
                          from deptdb0 in temp2.DefaultIfEmpty()
                          select new MembersViewModel
                          {
                              Account = db.Account,
                              Name = db.Name,
                              Status = db.Status,
                              Department = empdb0 != null ? deptdb0.DivFullName : null,
                              Position = empdb0 != null ? empdb0.JobTitle : null,
                          };

            DDLServices ddlServices = new DDLServices();
            var DDLJson_Dept = JsonConvert.SerializeObject(ddlServices.GetDeptDDL("").ddllist.ToList());
            var DDLJson_Secretary = JsonConvert.SerializeObject(ddlServices.GetSecretaryDDL("").ddllist.ToList());

            MemberRegisterViewModel model = new MemberRegisterViewModel()
            {
                MemberList = members,
                ddldept = DDLJson_Dept,
                ddlsecretary = DDLJson_Secretary,
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        //設定此Action只接受頁面POST資料傳入
        public ActionResult Register(MemberRegisterViewModel registermember)
        {
            MailService mailservice = new MailService();
            //判斷頁面資料是否都經過驗證
            if (ModelState.IsValid)
            {
                #region 信箱驗證信方法(暫不使用)
                ////將頁面資料中的密碼欄位填入
                //registermember.newMember.Password = registermember.Password;
                ////取得信箱驗證碼
                //string AuthCode = mailservice.GetValidateCode();
                ////將信箱驗證碼填入
                //registermember.newMember.AuthCode = AuthCode;
                ////呼叫Service註冊新會員
                //membersdbservice.Register(registermember.newMember);
                ////取得寫好的驗證信範本內容
                //string TempMail = System.IO.File.ReadAllText(Server.MapPath("~/Views/Shared/RegisterEmailTemplate.html"));
                ////宣告Email驗證用的Url
                //UriBuilder ValidateUrl = new UriBuilder(Request.Url)
                //{
                //    Path = Url.Action("EmailValidate", "Members"
                //    , new
                //    {
                //        Account = registermember.newMember.Account,
                //        AuthCode = AuthCode
                //    })
                //};

                ////藉由Service將使用者資料填入驗證信範本中
                //string MailBody = mailservice.GetRegisterMailBody(TempMail, registermember.newMember.Name, ValidateUrl.ToString().Replace("%3F", "?"));
                ////呼叫Service寄出驗證信
                //mailservice.SendMail(MailBody, registermember.newMember.Email);
                ////用TempData儲存註冊訊息
                //TempData["RegisterState"] = "註冊成功，請去收信以驗證Email";
                ////重新導向頁面
                //return RedirectToAction("RegisterResult");
                #endregion

                #region 管理員設定帳號方法
                //提醒系統管理員已新增帳號密碼
                mailservice.SendMail("新增帳密", "人員: " + registermember.newMember.Account, "cys.Enki@gmail.com");


                registermember.newMember.Password = registermember.Password;
                membersdbservice.Register(registermember.newMember);
                return RedirectToAction("Register");
                #endregion
            }
            //未經驗證清空密碼相關欄位
            registermember.Password = null;
            registermember.PasswordCheck = null;
            //將資料回填至View中
            return View(registermember);
        }

        //註冊結果顯示頁面
        [Authorize]
        public ActionResult RegisterResult()
        {
            return View();
        }

        //判斷註冊帳號是否已經被註冊過Action
        [Authorize]
        public JsonResult AccountCheck(MemberRegisterViewModel registermember)
        {
            //呼叫Service來判斷，並回傳結果
            return Json(membersdbservice.AccountCheck(registermember.newMember.Account), JsonRequestBehavior.AllowGet);
        }

        //接收驗證信連結傳進來的Action
        [AllowAnonymous]
        public ActionResult EmailValidate(string Account, string AuthCode)
        {
            //用ViewData儲存，使用Service進行信箱驗證後的結果訊息
            ViewData["EmailValidate"] = membersdbservice.EmailValidate(Account, AuthCode);
            return View();
        }


        //(AJAX)取得職員資料
        [Authorize]
        public JsonResult GetEmployeeData(string Account)
        {
            var result = "nodata";
            DDLServices ddlServices = new DDLServices();
            var ddl = JsonConvert.SerializeObject(ddlServices.GetDeptDDL("").ddllist.ToList());

            var divcode = memberdb.EmployeeData.Where(x => x.Account == Account).Select(x => x.DivCode).FirstOrDefault();
            var divcodeIsnull = String.IsNullOrEmpty(divcode);

            if(divcodeIsnull)
            {
                return Json(result);
            }
            else
            {
                var EmployeeData = from db in memberdb.EmployeeData.Where(x => x.Account == Account)
                                   join deptdb in memberdb.Department on db.DivCode equals deptdb.DivCode
                                   select new
                                   {
                                       //Department = deptdb0.DivFullName,
                                       JobTitle = db.JobTitle,
                                       divcode = divcodeIsnull ? "" : deptdb.DivCode,
                                       AssistAccount = String.IsNullOrEmpty(db.AssistantAccount) ? "" : db.AssistantAccount,
                                   };
                var ddd = EmployeeData.FirstOrDefault();
                return Json(EmployeeData);
            }

        }

        //編輯職員資料
        [Authorize]
        [HttpPost]
        public ActionResult UpdateMemberData(string Account, string Name, string DivCode, string SecretaryAccount)
        {
            var empdata = memberdb.EmployeeData.Where(x => x.Account == Account).FirstOrDefault();

            var deptdata = memberdb.Department.Where(x => x.DivCode == DivCode).FirstOrDefault();
            try
            {
                if (empdata != null)
                {
                    // 更新員工的部門和職位
                    //empdata.Department = Department;
                    empdata.DivCode = DivCode;
                    empdata.JobTitle = deptdata.GeneralTitle;
                    empdata.UpdateTime = DateTime.Now;
                    empdata.UpdateAccount = User.Identity.Name;
                    empdata.AssistantAccount = SecretaryAccount;

                    // 提交更改到資料庫
                    memberdb.SaveChanges();
                }
                else
                {
                    // 創建一個新的員工實例
                    var newEmployee = new EmployeeData
                    {
                        Account = Account,
                        DivCode = DivCode,
                        JobTitle = deptdata.GeneralTitle,
                        CreateTime = DateTime.Now,
                        CreateAccount = User.Identity.Name,
                        UpdateTime = DateTime.Now,
                        UpdateAccount = User.Identity.Name,
                        AssistantAccount = SecretaryAccount,
                    };

                    // 使用 Entity Framework 將新員工新增到資料庫
                    memberdb.EmployeeData.Add(newEmployee);
                    memberdb.SaveChanges();
                }

                //將角色存入職員資料中
                var membersData = memberdb.Members.Where(x => x.Account == Account).FirstOrDefault();
                if(membersData != null)
                {
                    membersData.Role = deptdata.Role;
                    memberdb.SaveChanges();
                }
            }
            catch(Exception ex)
            {
                var error = ex.ToString();

            }
            return RedirectToAction("Register");
        }

        //停用/啟用職員帳號狀態
        public ActionResult ChangeEmpStatus(string Account, bool ToStatus)
        {
            var empdata = memberdb.Members.Where(x => x.Account == Account).FirstOrDefault();
            try
            {
                if (empdata != null)
                {
                    // 更新員工的部門和職位
                    empdata.Status = ToStatus;
                    // 提交更改到資料庫
                    memberdb.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                var error = ex.ToString();

            }
            return RedirectToAction("Register");
        }

        //重設密碼
        public ActionResult ResetPassword(string Account, string Password)
        {
            #region 管理員設定帳號方法
            var hashpassword = membersdbservice.HashPassword(Password);

            var empdata = memberdb.Members.Where(x => x.Account == Account).FirstOrDefault();
            try
            {
                if (empdata != null)
                {
                    // 更新員工的部門和職位
                    empdata.Password = hashpassword;
                    // 提交更改到資料庫
                    memberdb.SaveChanges();
                }
            }
            catch(Exception ex)
            {
                var error = ex.ToString();
            }

            #endregion
            return RedirectToAction("Register");
        }
        #endregion

        #region 登入
        //登入一開始畫面
        [AllowAnonymous]
        public ActionResult Login()
        {
            //判斷使用者是否已經過登入驗證
            if (User.Identity.IsAuthenticated)
            {
                var member = memberdb.Members.Where(x => x.Account == User.Identity.Name).FirstOrDefault();
                if (member != null)
                {
                    if (member.Status)
                    {
                        return RedirectToAction("Index", "Liqun"); //已登入，並且非停權，重新導向
                    }
                }
            }

            var model = new MembersLoginViewModel();
            return View(model);
        }

        //傳入登入資料的Action
        /// <summary>
        /// 
        /// </summary>
        /// <param name="LoginMember">MembersLoginViiewmModel儲存:帳號(Account)/密碼(Password)</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(MembersLoginViewModel LoginMember)
        {
            if (ModelState.IsValid)
            {
                //使用Service裡的方法來驗證登入的帳號密碼
                string ValidateStr = membersdbservice.LoginCheck(LoginMember.Account, LoginMember.Password);
                //判斷驗證後結果是否有錯誤訊息
                if (String.IsNullOrEmpty(ValidateStr))
                {
                    //無錯誤訊息，則登入

                    //先藉由Service取得登入者角色資料
                    string RoleData = membersdbservice.GetRole(LoginMember.Account);
                    //設定JWT
                    JwtService jwtservice = new JwtService();
                    //從Web.Config撈出資料
                    //Cookie名稱
                    string cookieName = WebConfigurationManager.AppSettings["CookieName"].ToString();
                    //取得Token(加密)
                    string Token = jwtservice.GenerateToken(LoginMember.Account, RoleData);

                    ////產生一個Cookie
                    HttpCookie cookie = new HttpCookie(cookieName);
                    //設定單值
                    cookie.Value = Server.UrlEncode(Token);
                    //寫到用戶端
                    Response.Cookies.Add(cookie);
                    //設定Cookie期限
                    Response.Cookies[cookieName].Expires = DateTime.Now.AddMinutes(Convert.ToInt32(WebConfigurationManager.AppSettings["ExpireMinutes"]));

                    //重新導向頁面
                    return RedirectToAction("Index", "Liqun");

                }
                else
                {
                    LoginMember.Password = null;
                    //有驗證錯誤訊息，加入頁面模型中
                    ModelState.AddModelError("", ValidateStr);
                    //將資料回填至View中
                    return View(LoginMember);
                }
            }
            else
            {
                LoginMember.Password = null;
                //將資料回填至View中
                return View(LoginMember);
            }
        }
        #endregion

        #region 登出
        [AllowAnonymous]
        public ActionResult Logout()
        {
            //使用者登出
            //Cookie名稱
            string cookieName = WebConfigurationManager.AppSettings["CookieName"].ToString();
            //清除Cookie
            HttpCookie cookie = new HttpCookie(cookieName);
            cookie.Expires = DateTime.Now.AddDays(-1);
            cookie.Values.Clear();
            Response.Cookies.Set(cookie);
            //重新導向至登入Action
            return RedirectToAction("Login");

        }
        #endregion

        #region 部門管理
        public ActionResult DeptManage()
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

            if (!role)
                return RedirectToAction("Index", "Liqun");
            #endregion

            var deptlist = from db in memberdb.Department
                           select new DepartmentViewModel
                           {
                               DivCode = db.DivCode,
                               DivName = db.DivName,
                               DivFullName = db.DivFullName,
                               ParentDivCode = db.ParentDivCode,
                               ParentDivName = memberdb.Department.Where(x => x.DivCode == db.ParentDivCode).FirstOrDefault().DivFullName,
                               ManageName = memberdb.Members.Where(x => x.Account == db.ManageAccount).FirstOrDefault().Name,

                           };
            var model = new MemberRegisterViewModel
            {
                Deptlist = deptlist,
            };

            return View(model);
        }

        //取得部門主管資料
        public JsonResult GetManager(string divcode)
        {
            var result = "nodata";
            DDLServices ddlServices = new DDLServices();
            var ddl = ddlServices.GetManager(divcode).ddllist;
            var jsonddl = JsonConvert.SerializeObject(ddlServices.GetManager(divcode).ddllist.ToList());

            if (ddl.Count() == 0)
            {
                return Json(result);
            }
            else
            {
                return Json(jsonddl);
            }

        }

        //更動部門主管
        public ActionResult UpdateManager(string DivCode, string Account)
        {
            var empdata = memberdb.EmployeeData.Where(x => x.Account == Account).FirstOrDefault();
            var deptdata = memberdb.Department.Where(x => x.DivCode == DivCode).FirstOrDefault();
            var canclempdata = memberdb.EmployeeData.Where(x => x.JobTitle == deptdata.ManagerTitle).FirstOrDefault();
            try
            {
                if (deptdata != null)
                {
                    // 更新員工的職稱
                    empdata.JobTitle = deptdata.ManagerTitle;
                    //更換部門主管
                    deptdata.ManageAccount = Account;

                    // 提交更改到資料庫
                    memberdb.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                var error = ex.ToString();
            }

            try
            {
                if (deptdata != null)
                {
                    canclempdata.JobTitle = deptdata.GeneralTitle;
                    memberdb.SaveChanges();

                }
            }
            catch (Exception ex)
            {
                var error = ex.ToString();

            }
            return RedirectToAction("DeptManage");
        }
        #endregion

    }
}