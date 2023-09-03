//註冊用ViewModel
using LiqunManagement.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Collections.Generic;

namespace LiqunManagement.ViewModels
{
    public class MemberRegisterViewModel
    {
        //public IE<MembersViewModel> MembersList { get; set; }

        public IEnumerable<MembersViewModel> MemberList { get; set; }
        public IEnumerable<DepartmentViewModel> Deptlist { get; set; }
        public MembersViewModel newMember { get; set; }
        public EmployeeData  employeeDatamodel { get; set; }

        //下拉選單
        public IEnumerable<DDLViewModel> ddllist { get; set; }

        [DisplayName("密碼")]
        [Required(ErrorMessage = "請輸入密碼")]
        public string Password { get; set; }

        [DisplayName("確認密碼")]
        [Compare("Password", ErrorMessage = "兩次密碼輸入不一致")]
        [Required(ErrorMessage = "請輸入確認密碼")]
        public string PasswordCheck { get; set; }
    }

}