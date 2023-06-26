using LiqunManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LiqunManagement.Controllers
{
    public class LiqunController : Controller
    {
        //宣告Members資料表的Service物件
        private readonly MembersDBService membersdbservice = new MembersDBService();
        // GET: Liqun
        public ActionResult Index()
        {
            //判斷使用者是否已經過登入驗證
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Members");

            //var name = User.Identity.Name;
            //string RoleData = membersdbservice.GetRole(LoginMember.Account);
            //ViewBag.UserName = 
            return View();
        }

        [HttpGet]
        public ActionResult Form()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Members");



            return View();
        }
    }
}