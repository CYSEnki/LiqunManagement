using LiqunManagement.Security;
using LiqunManagement.Services;
using LiqunManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace LiqunManagement.Controllers
{
    public class FormStuffController : Controller
    {
        [HttpGet]
        // GET: FormStuff
        public ActionResult Index()
        {
            //判斷使用者是否已經過登入驗證
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Members", "Login"); //已登入，重新導向

            return View();
        }


        [HttpPost]
        // Post: FormStuff
        public ActionResult Index(string Account, string Password)
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


        //宣告Members資料表的Service物件
        private readonly MembersDBService membersdbservice = new MembersDBService();
        //宣告寄信用的Service物件
        private readonly MailService mailservice = new MailService();

        #region 登入
        //登入一開始畫面
        public ActionResult test()
        {
            //判斷使用者是否已經過登入驗證
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Members"); //已登入，重新導向

            var model = new MembersLoginViewModel();
            return View(model);
        }

        //傳入登入資料的Action
        [HttpPost]
        public ActionResult test(string Account, string Password)
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
        #endregion

    }
}