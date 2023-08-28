using Jose;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Text;

namespace LiqunManagement.Security
{
    public class JwtService
    {
        #region 製作Token
        public string GenerateToken(string Account, string Role)
        {
            JwtObject jwtobject = new JwtObject
            {
                Account = Account,  //帳號
                Role = Role,        //帳號角色
                Expire = DateTime.Now.AddMinutes(Convert.ToInt32(WebConfigurationManager.AppSettings["ExpireMinutes"])).ToString(),     //取得有限時間
            };
            //從Web.Config取得密鑰
            string SecretKey = WebConfigurationManager.AppSettings["SecretKey"].ToString();
            //JWT的內容
            var payload = jwtobject;
            ///將資料加密為Token
            ///JwsAlgorithm.HS512為加密方式，意思為使用HS512進行Base64編碼
            var token = JWT.Encode(payload, Encoding.UTF8.GetBytes(SecretKey), JwsAlgorithm.HS512);
            return token;
        }
        #endregion
    }
}