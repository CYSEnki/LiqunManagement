//註冊用ViewModel
using LiqunManagement.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace LiqunManagement.ViewModels
{
    public partial class MembersViewModel
    {
        //帳號
        [DisplayName("帳號")]
        [Required(ErrorMessage = "請輸入帳號")]
        [StringLength(30, MinimumLength = 6, ErrorMessage = "帳號長度須介於6-30字元")]
        [Remote("AccountCheck", "Members", ErrorMessage = "此帳號已被註冊過")]
        public string Account { get; set; }

        //密碼
        public string Password { get; set; }

        //姓名
        [DisplayName("姓名")]
        [Required(ErrorMessage = "請輸入姓名")]
        [StringLength(20, ErrorMessage = "姓名長度最多20字元")]
        public string Name { get; set; }

        //電子信箱
        [DisplayName("Email")]
        //[Required(ErrorMessage = "請輸入Email")]
        [StringLength(200, ErrorMessage = "Email長度最多200字元")]
        [EmailAddress(ErrorMessage = "這不是Email格式")]
        public string Email { get; set; }

        //信箱驗證碼
        public string AuthCode { get; set; }

        //管理者
        public bool IsAdmin { get; set; }

        //帳號狀態
        public bool Status { get; set; }

        //部門
        public string Department { get; set; }

        //職位
        public string Position { get; set; }
    }
}