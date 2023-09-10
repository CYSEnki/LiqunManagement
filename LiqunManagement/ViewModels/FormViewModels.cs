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
        public LandLordViewModel landlordviewmodel { get; set; }
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

    #region FormGet資料
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

        public string AddressDetail { get; set; }

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
    //房東資料
    public partial class LandLordViewModel
    {
        public string FormID { get; set; }
        //房東資料
        public string Name { get; set; }
        public string Principal { get; set; }
        public int? Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public string BirthdayStr { get; set; }
        public string IDNumber { get; set; }
        public string Phone { get; set; }
        public string BankNo { get; set; }
        public string BrancheNo { get; set; }
        public string BankAccount { get; set; }
        //房東地址
        public string Address { get; set; }
        public string AddressCode_LandLord { get; set; }
        public string AddressDetail { get; set; }
        public string ContactAddress { get; set; }
        public string ContactCode_Landlord { get; set; }
        public string ContactAddressDetail { get; set; }
        //成員人數陣列[共有人, 代理人]
        public string MemberArray { get; set; }
        //共有人人數
        public int CoOwnerCount { get; set; }
        //是否有代理人
        public int AgentCount { get; set; }
        public string CoOwner1 { get; set; }
        public string CoOwner2 { get; set; }
        public string CoOwner3 { get; set; }
        public string CoOwner4 { get; set; }
        public string CoOwner5 { get; set; }
        public string Agent { get; set; }
        public DateTime? CreateTime { get; set; }
        public string CreateUser { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string UpdateUser { get; set; }
        public string Memo { get; set; }

        //下拉選單
        public string Address_DistrictDDL_Landlord { get; set; }
        public string Address_RoadDDL_Landlord { get; set; }
        public string Contact_DistrictDDL_Landlord { get; set; }
        public string Contact_RoadDDL_Landlord { get; set; }

        //共有人一
        public string CoAddress1_District { get; set; }
        public string CoAddress1_Road { get; set; }
        public string CoContact1_District { get; set; }
        public string CoContact1_Road { get; set; }
        //共有人二
        public string CoAddress2_District { get; set; }
        public string CoAddress2_Road { get; set; }
        public string CoContact2_District { get; set; }
        public string CoContact2_Road { get; set; }
        //共有人三
        public string CoAddress3_District { get; set; }
        public string CoAddress3_Road { get; set; }
        public string CoContact3_District { get; set; }
        public string CoContact3_Road { get; set; }
        //共有人四
        public string CoAddress4_District { get; set; }
        public string CoAddress4_Road { get; set; }
        public string CoContact4_District { get; set; }
        public string CoContact4_Road { get; set; }
        //共有人五
        public string CoAddress5_District { get; set; }
        public string CoAddress5_Road { get; set; }
        public string CoContact5_District { get; set; }
        public string CoContact5_Road { get; set; }
        //代理人
        public string AgAddress_District { get; set; }
        public string AgAddress_Road { get; set; }
        public string AgContact_District { get; set; }
        public string AgContact_Road { get; set; }
    }
    #endregion

    #region FormPost資料
    public class HomeObjectInputViewModel
    {
        // 表單編號
        public string FormID { get; set; }

        // 物件類型 (radio)包租:1; 代管:0
        public string objecttypeRadio { get; set; }

        // 公證 (radio)公證:1; 非公證:0
        public string notarizationRadio { get; set; }

        // 簽約日 (datetime)簽約日
        public string signdate { get; set; }

        // 簽估價師 (radio)簽估價師:1; 非簽估價師:0
        public string appraiserRadio { get; set; }

        // 特色 (text)特色
        public string feature { get; set; }

        // 物件地址 (ddl)物件地址
        public string selectroad { get; set; }

        // 地址細節 (text)地址細節
        public string AddressDetail { get; set; }

        // 主要用途 (radio)主要用途 住家用:0; 商業用:1; 辦公室:2; 一般事務所:3; 其他:4
        public string useforRadio { get; set; }

        // 主要用途 (text)主要用途 其他
        public string useforelse { get; set; }

        // 上傳稅單 (file)上傳稅單
        public IEnumerable<HttpPostedFileBase> taxFile { get; set; }

        // 租金 (number)租金
        public string rent { get; set; }

        // 押金 (number)押金
        public string deposit { get; set; }

        // 管理費 (number)管理費
        public string management_fee { get; set; }

        // 起租日 (datetime)起租日
        public string startdate { get; set; }

        // 結束日 (datetime)結束日
        public string enddate { get; set; }

        // 繳租日 (ddl)繳租日
        public int paydate { get; set; }

        // 建物型態 (radio)建物型態 透天厝:0; 公寓:1; 華夏:2; 電梯大樓:3
        public string buildtypeRadio { get; set; }

        // 房型 (radio)房型 整層出租:0; 獨立套房:1;
        public string roomtypeRadio { get; set; }

        // 房數 (text)房數
        public string roomamount { get; set; }

        // 廳數 (text)廳數
        public string hallamount { get; set; }

        // 衛數 (text)衛數
        public string bathamount { get; set; }

        // 車位 無車位 (checkbox)車位 無車位:0
        public bool? noparkcheck { get; set; }

        // 車位 汽車車位 (checkbox)車位 汽車車位:0
        public bool? carparkcheck { get; set; }

        // 車位 機車車位 (checkbox)車位 機車車位:0
        public bool? morparkcheck { get; set; }

        // 汽車車位樣式 (radio)汽車車位樣式 坡道平面:0; 坡道機械:1; 機械平面:2; 機械機械:3
        public string parktypeRadio { get; set; }

        // 汽車位於 地上 (radio)汽車位於 地上:1; 地下:0
        public string carparkfloorRadio { get; set; }

        // 汽車位於幾樓 (number)汽車位於幾樓
        public string parkfloornumber { get; set; }

        // 汽車位編號 (text)汽車位編號
        public string carpositionnumber { get; set; }

        // 汽車月租金 (text)汽車月租金
        public string carmonthrent { get; set; }

        // 汽車管理費 (number)汽車管理費
        public string carparkmanagefee { get; set; }

        // 機車位於 地上 (radio)機車位於 地上:1; 地下:0
        public string scooterparkfloorRadio { get; set; }

        // 機車位於幾樓 (number)機車位於幾樓
        public string scooterparkfloornumber { get; set; }

        // 機車位編號 (text)機車位編號
        public string morpositionnumber { get; set; }

        // 機車月租金 (number)機車月租金
        public string scootermonthrent { get; set; }

        // 機車管理費 (number)機車管理費
        public string scootermanagefee { get; set; }

        // 房屋附屬家具
        public string JsonHomeObjectAccessory { get; set; }

        // 備註
        public string memo { get; set; }
    }

    public class LandlordInputViewModel
    {
        public string FormID { get; set; }
        public string persontypeRadio { get; set; }
        public string Name_0 { get; set; }
        public string Principal { get; set; }
        public string genderRadio_0 { get; set; }
        public string birthday_0 { get; set; }
        public string IDNumber_0 { get; set; }
        public string Phone_0 { get; set; }
        public string addressroad_0 { get; set; }
        public string AddressDetail_0 { get; set; }
        public bool? sameaddress_check_0 { get; set; }
        public string contactroad_0 { get; set; }
        public string detailcontact_0 { get; set; }
        public string bank_0 { get; set; }
        public string bankbranche_0 { get; set; }
        public string bankaccount_0 { get; set; }
        public string CoOwnerRadio { get; set; }
        public string CoOwnerInput1 { get; set; }
        public string CoOwnerInput2 { get; set; }
        public string CoOwnerInput3 { get; set; }
        public string CoOwnerInput4 { get; set; }
        public string CoOwnerInput5 { get; set; }
        public bool? agentCheckbox { get; set; }
        public string AgentInput { get; set; }
        public string memo { get; set; }
    }
    #endregion
}