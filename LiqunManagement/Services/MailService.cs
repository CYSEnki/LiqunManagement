using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using LiqunManagement.Models;
using LiqunManagement.ViewModels;
using LiqunManagement.Services;

namespace LiqunManagement.Services
{
    public class MailService : BaseService
    {
        //private string gmail_account = "cys.enki@gmail.com";  //Gmail帳號
        //private string gmail_password = "kigyfioavdbtzunl"; //Gmail密碼
        //private string gmail_mail = "cys.enki@gmail.com";     //Gmail信箱

        #region 寄會員驗證信
        //產生驗證碼方法
        public string GetValidateCode()
        {
            //設定驗證碼字元的陣列
            string[] Code =
            {
                "A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z",
                "1","2","3","4","5","6","7","8","9","0",
                "a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z"
            };
            //宣告初始為空的驗證碼字串
            string ValidateCode = string.Empty;
            //宣告可產生隨機數值的物件
            Random rd = new Random();
            //使用迴圈產生出驗證碼
            for(int i = 0; i<10; i++)
            {
                ValidateCode += Code[rd.Next(Code.Count())];
            }
            return ValidateCode;
        }

        //將使用者資料填入驗證信範本中
        public string GetRegisterMailBody(string TempString, string UserName, string ValidateUrl)
        {
            //將使用者資料填入
            TempString = TempString.Replace("{{UserName}}", UserName);
            TempString = TempString.Replace("{{ValidateUrl}}", ValidateUrl);
            //回傳最後結果
            return TempString;
        }

        #endregion

        private string liqun_account = "liqunmanagement@liqun.company";     //力群信箱
        private string liqun_password = "XU870517xu";                       //力群密碼
        //private string liqun_password = "Ne6-tp8fud";                       //力群密碼


        #region 寄信方法
        //寄驗證信的方法
        public void SendMail(string Subject, string MailBody, string ToEmail)
        {
            #region gmaill
            ////建立寄信用Smtp物件,這裡使用Gmail為例
            //SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
            ////設定使用的Port，這裡設定Gmail所使用的
            //SmtpServer.Port = 587;
            ////建立使用者憑據，這裡要設定您的Gmail帳戶
            //SmtpServer.Credentials = new System.Net.NetworkCredential(gmail_account, gmail_password);
            ////開啟SSL
            //SmtpServer.EnableSsl = true;
            #endregion

            #region godaddy
            //SmtpClient smtpClient = new SmtpClient("smtpout.asia.secureserver.net")
            SmtpClient smtpClient = new SmtpClient("smtpout.secureserver.net")
            {
                Port = 587,
                Credentials = new System.Net.NetworkCredential(liqun_account, liqun_password),
                EnableSsl = true,
            };
            #endregion

            //宣告信件內容物件
            MailMessage mail = new MailMessage();
            //設定來源信箱
            mail.From = new MailAddress(liqun_account);
            //設定收信者信箱
            mail.To.Add(ToEmail);
            //設定信件主旨
            mail.Subject = String.IsNullOrEmpty(Subject) ? "會員註冊確認信" : Subject;
            //設定信件內容
            mail.Body = MailBody;
            //設定信件內容為HTML格式
            mail.IsBodyHtml = true;
            try
            {
                //送出信件
                smtpClient.Send(mail);
            }
            catch(Exception ex)
            {
                var error = ex.ToString();
            }
        }
        #endregion
    }
}