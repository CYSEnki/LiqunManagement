using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using LiqunManagement.Models;
using System.Security.Principal;


namespace LiqunManagement.Attributes
{
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public MembersModel memberdb = new MembersModel();

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            bool authorized = false;
            string userId = "";
            //if (filterContext.HttpContext.Session["AdminUserID"] != null)
            //{
            //    userId = Convert.ToString(filterContext.HttpContext.Session["AdminUserID"]);
            //}


            // 檢查是否已驗證
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                userId = filterContext.HttpContext.User.Identity.Name;
            }

            //自訂驗證邏輯：
            //這邊是到db檢查是否存在此user，並且為啟用
            if (!string.IsNullOrEmpty(userId))
            {
                var member = memberdb.Members.Where(x => x.Account == userId).FirstOrDefault();
                if (member != null)
                {
                    if (member.Status)
                    {
                        authorized = true;
                    }
                }
            }
            if (!authorized)
                //驗證失敗則自動導向到指定的controller
                filterContext.Result = new RedirectToRouteResult(
               new RouteValueDictionary
               {
                    { "controller", "Members" },
                    { "action", "Login" }
               });

        }

    }
}