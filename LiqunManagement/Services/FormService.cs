using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LiqunManagement.Services
{
    public class FormService : BaseService
    {
        #region 取得新表單編號
        public string GetNewFormID()
        {
            //取得新表單編號
            string newFormID = "LQ" + DateTime.Now.ToString("yy") + "000001";
            var lastformid = formdb.ObjectForm.OrderByDescending(x => x.FormNo).Select(x => x.FormID).FirstOrDefault();
            if (!String.IsNullOrEmpty(lastformid))
            {
                var idIndex = Convert.ToInt32(lastformid.Substring(4));
                newFormID = lastformid.Substring(0, 4) + (idIndex + 1).ToString("D6");
            }
            return newFormID;
        }
        #endregion
    }
}