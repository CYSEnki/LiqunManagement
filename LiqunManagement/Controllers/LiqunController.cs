using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LiqunManagement.Controllers
{
    public class LiqunController : Controller
    {
        // GET: Liqun
        public ActionResult Index()
        {
            //判斷使用者是否已經過登入驗證
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Members");
            }
            return View();
        }
    }
}