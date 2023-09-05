using LiqunManagement.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Helpers;

namespace LiqunManagement.ViewModels
{
    public class FormViewModels
    {
        public IEnumerable<DDLViewModel> ddllist { get; set; }
        public IEnumerable<objectForm> objectformlist { get; set; }

        //表單編號
        public string FormID { get; set; }
        //房屋物件
        public HomeObjectViewModel homeobjectviewmodel { get; set; }
    }

    public class DDLViewModel
    {
        public int? order { get; set; }
        public string id { get; set; }
        public string text { get; set; }
    }

    public class objectForm
    {
        public string FormId { get; set; }
        public DateTime CreateTime { get; set; }
        public string ProcessName { get; set; }
        public string Address { get; set; }
        public DateTime SignDate { get; set; }
        public string Landlord { get; set; }
        public string Tenant { get; set; }
    }

    //房屋物件

    public class HomeObjectViewModel
    {
        public string FormID { get; set; }

        public string CaseId { get; set; }

        public int? objecttype { get; set; }

        public int? notarization { get; set; }

        public DateTime? signdate { get; set; }
        public string signdateStr { get; set; }

        public int? appraiser { get; set; }

        public string feature { get; set; }

        public string city { get; set; }
        public string citycode { get; set; }

        public string district { get; set; }
        public string districtcode { get; set; }

        public string road { get; set; }
        public string roadcode { get; set; }

        public string detailaddress { get; set; }

        public string fulladdress { get; set; }

        public int? usefor { get; set; }

        public string useforelse { get; set; }

        public string taxfile_name { get; set; }

        public string taxfile_alias { get; set; }

        public int? rent { get; set; }

        public int? deposit { get; set; }

        public int? management_fee { get; set; }

        public DateTime? startdate { get; set; }
        public string startdateStr { get; set; }

        public DateTime? enddate { get; set; }
        public string enddateStr { get; set; }

        public int? paydate { get; set; }

        public int? buildtype { get; set; }

        public int? roomtype { get; set; }

        public string roomamount { get; set; }
        public List<int> roomamountlist { get; set; }

        public string havepark { get; set; }
        public List<int> haveparklist { get; set; }

        public int? parktype { get; set; }

        public string parkfloor { get; set; }
        public List<int> parkfloorlist { get; set; }

        public string carpositionnumber { get; set; }

        public int? carmonthrent { get; set; }

        public int? carparkmanagefee { get; set; }
        public string scooterparkfloor { get; set; }
        public List<int> scooterparkfloorlist { get; set; }

        public string scooterpositionnumber { get; set; }

        public int? scootermonthrent { get; set; }

        public int? scootermanagefee { get; set; }

        public string Accessory { get; set; }
        public List<int> Accessorylist { get; set; }

        public DateTime? CreateTime { get; set; }

        public string CreateUser { get; set; }

        public DateTime? UpdateTime { get; set; }

        public string UpdateUser { get; set; }

        public string Memo { get; set; }


        //物件地址(鄉鎮市區)
        public string distirctJsonDDL { get; set; }
        public string roadJsonDDL { get; set; }
    }
}