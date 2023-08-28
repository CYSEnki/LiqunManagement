using LiqunManagement.Models;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using LiqunManagement.ViewModels;

namespace LiqunManagement.Services
{
    public class MembersDBService : BaseService
    {
        #region 註冊
        //註冊新會員方法
        public void Register(MembersViewModel newMember)
        {
            //將密碼Hash過
            newMember.Password = HashPassword(newMember.Password);

            //新增語法
            Members user = new Members
            {
                Account = newMember.Account,
                Password = newMember.Password,
                Name = newMember.Name,
                //Email = newMember.Email,          //不須驗證
                //AuthCode = newMember.AuthCode,    //不須驗證
                IsAdmin = newMember.IsAdmin,
                Status = true,
            };

            Memberdb.Members.Add(user);
            Memberdb.SaveChanges();
        }
        #endregion

        #region Hash密碼
        //Hash密碼用的方法
        public string HashPassword(string Password)
        {
            //宣告Hash時所添加的無意義亂數值
            string saltkey = "1no23rn0vn8nacnf23nr1985fndafvq4114";
            //將剛剛宣告的字串與密碼結合
            string saltAndPassword = String.Concat(Password, saltkey);
            //定義SHA256的HASH物件
            SHA256CryptoServiceProvider sha256Hasher = new SHA256CryptoServiceProvider();
            //取得密碼轉換成byte資料
            byte[] PasswordData = Encoding.Default.GetBytes(saltAndPassword);
            //取得Hash後byte資料
            byte[] HashDate = sha256Hasher.ComputeHash(PasswordData);
            //將Hash後byte資料轉換成string
            string Hashresult = Convert.ToBase64String(HashDate);

            //回傳Hash結果
            return Hashresult;
        }
        #endregion

        #region 查詢一筆資料
        private MembersViewModel GetDataByAccount(string Account)
        {
            var AccountData = Memberdb.Members.Where(x => x.Account == Account).FirstOrDefault();

            MembersViewModel Data = new MembersViewModel();
            if (AccountData != null)
            {
                Data.Account = AccountData.Account;
                Data.Password = AccountData.Password;
                Data.Name = AccountData.Name;
                Data.Email = AccountData.Email;
                Data.AuthCode = AccountData.AuthCode;
                Data.IsAdmin = AccountData.IsAdmin;
            }
            else
            {
                Data = null;
            }
            return Data;
        }
        #endregion

        #region 帳號註冊重複確認
        public bool AccountCheck(string Account)
        {
            //藉由傳入帳號取得會員資料
            MembersViewModel Data = GetDataByAccount(Account);
            //判斷是否有查詢到會員
            bool result = (Data == null);
            //回傳結果
            return result;
        }
        #endregion

        #region 信箱驗證
        //信箱驗證碼驗證方法
        public string EmailValidate(string Account, string AuthCode)
        {
            //取得傳入帳號的會員資料
            MembersViewModel ValidateMember = GetDataByAccount(Account);
            //宣告驗證後訊息字串
            string ValidateStr = string.Empty;
            if(ValidateMember != null)
            {
                //判斷傳入驗證碼與資料庫中是否相同
                if(ValidateMember.AuthCode == AuthCode)
                {
                    //將資料庫的驗證碼設為空
                    try
                    {
                        using(var context = new MembersModel())
                        {
                            var memberdata = context.Members.Where(x => x.Account == Account).FirstOrDefault();
                            memberdata.AuthCode = "";
                            context.SaveChanges();
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.Write(ex.ToString());
                    }
                    ValidateStr = "帳號信箱驗證成功，現在可以登入了";
                }
                else
                {
                    ValidateStr = "驗證碼錯誤，請重新確認或再註冊";
                }
            }
            else
            {
                ValidateStr = "傳送資料錯誤，請重新確認或再註冊";
            }
            return ValidateStr;
        }
        #endregion

        #region 登入確認
        //登入帳密確認方法，並回傳驗證後訊息
        public string LoginCheck(string Account, string Password)
        {
            //取得傳入帳號的會員資料
            MembersViewModel LoginMember = GetDataByAccount(Account);
            //判斷是否有此會員
            if(LoginMember != null)
            {
                //判斷是否有經過信箱驗證，有經驗證驗證碼欄位會被清空
                if (String.IsNullOrWhiteSpace(LoginMember.AuthCode))
                {
                    //進行帳號密碼確認
                    if(PasswordCheck(LoginMember, Password))
                    {
                        return "";
                    }
                    else
                    {
                        return "密碼輸入錯誤";
                    }
                }
                else
                {
                    return "此帳號尚未經過Email驗證，請去收信";
                }
            }
            else
            {
                return "無此會員帳號，請去註冊";
            }
        }
        #endregion

        #region 密碼確認
        //進行密碼確認方法
        public bool PasswordCheck(MembersViewModel CheckMember, string Password)
        {
            //判斷資料庫裡的密碼資料與傳入密碼資料Hash後是否一樣
            bool result = CheckMember.Password.Equals(HashPassword(Password));
            //回傳結果
            return result;
        }
        #endregion

        #region 取得角色
        //取得會員的權限角色資料
        public string GetRole(string Account)
        {
            //宣告初始角色字串
            string Role = "User";
            //取得傳入帳號的會員資料
            MembersViewModel LoginMember = GetDataByAccount(Account);
            //判斷資料庫欄位，用以確認是否為Admin
            if (LoginMember.IsAdmin)
            {
                Role += ",Admin";
            }
            //回傳最後結果
            return Role;
        }
        #endregion
    }
}