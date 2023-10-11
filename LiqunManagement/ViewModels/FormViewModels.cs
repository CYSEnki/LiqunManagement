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
        public IEnumerable<objectFormViewModel> objectformlist { get; set; }

        //表單編號
        public string FormID { get; set; }

        //房屋物件
        public HomeObjectViewModel homeobjectviewmodel { get; set; }
        public LandLordViewModel landlordviewmodel { get; set; }
        public TenantViewModel tenantviewmodel { get; set; }
        public SecretaryViewModel secretaryviewmodel { get; set; }

        //上傳區資料
        public Uploads Uploads { get; set; }
    }

    public class DDLViewModel
    {
        public int? order { get; set; }
        public string id { get; set; }
        public string text { get; set; }
    }

    public class objectFormViewModel
    {
        public string FormID { get; set; }
        public DateTime CreateTime { get; set; }
        public string ProcessAccount { get; set; }
        public string ProcessName { get; set; }
        public string Address { get; set; }
        public DateTime SignDate { get; set; }
        public string Landlord { get; set; }
        public string Tenant { get; set; }
        public string AgentAccount { get; set; }
<<<<<<< HEAD
        public string AgentName { get; set; }
=======
>>>>>>> 18655414c38c45def05cddf5cf82eb31f1c682b9
        public string AssistantAccount { get; set; }
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

        public string CreateAccount { get; set; }

        public DateTime? UpdateTime { get; set; }

        public string UpdateAccount { get; set; }

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
        public string CreateAccount { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string UpdateAccount { get; set; }
        public string Memo { get; set; }

        //下拉選單
        public string LaAddress_District { get; set; }
        public string LaAddress_Road { get; set; }
        public string LaContact_District { get; set; }
        public string LaContact_Road { get; set; }

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
    //房客資料
    public class TenantViewModel
    {
        public string FormID { get; set; }
        public int? TenantType { get; set; }
        public string vulnerablefile_Name { get; set; }
        public string vulnerablefile_Alias { get; set; }
        public string sheetfile_Name { get; set; }
        public string sheetfile_Alias { get; set; }
        public string Name { get; set; }
        public int? Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public string BirthdayStr { get; set; }
        public string IDNumber { get; set; }
        public string Phone { get; set; }
        public string BankNo { get; set; }
        public string BrancheNo { get; set; }
        public string BankAccount { get; set; }
        public string AddressCode { get; set; }         //戶籍地址代碼
        public string Address { get; set; }             //戶籍地址
        public string AddressDetail { get; set; }       //戶籍地址細節
        public string ContactAddressCode { get; set; }  //通訊地址代碼
        public string ContactAddress { get; set; }      //通訊地址
        public string ContactAddressDetail { get; set; }//通訊地址細節
        public string accountNo { get; set; }           //戶號
        public string MemberArray { get; set; }         //人數陣列[配偶, 直系, 代理人, 保證人]
        public string CoupleCount { get; set; }
        public string Couple { get; set; }
        public string DirectCount { get; set; }
        public string Family1 { get; set; }
        public string Family2 { get; set; }
        public string Family3 { get; set; }
        public string Family4 { get; set; }
        public string Family5 { get; set; }
        public string Family6 { get; set; }
        public string Family7 { get; set; }
        public string Family8 { get; set; }
        public string Family9 { get; set; }
        public string Family10 { get; set; }
        public string AgentCount { get; set; }
        public string Agent1 { get; set; }
        public string Agent2 { get; set; }
        public string Agent3 { get; set; }
        public string GuarantorCount { get; set; }
        public string Guarantor1 { get; set; }
        public string Guarantor2 { get; set; }
        public string Guarantor3 { get; set; }
        public DateTime? CreateTime { get; set; }
        public string CreateAccount { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string UpdateAccount { get; set; }
        public string Memo { get; set; }


        //下拉選單
        public string TeAddress_District { get; set; }
        public string TeAddress_Road { get; set; }
        public string TeContact_District { get; set; }
        public string TeContact_Road { get; set; }


        //代理人一
        public string AgAddress1_District { get; set; }
        public string AgAddress1_Road { get; set; }
        public string AgContact1_District { get; set; }
        public string AgContact1_Road { get; set; }
        //代理人二
        public string AgAddress2_District { get; set; }
        public string AgAddress2_Road { get; set; }
        public string AgContact2_District { get; set; }
        public string AgContact2_Road { get; set; }
        //代理人三
        public string AgAddress3_District { get; set; }
        public string AgAddress3_Road { get; set; }
        public string AgContact3_District { get; set; }
        public string AgContact3_Road { get; set; }

        //保證人一
        public string GuAddress1_District { get; set; }
        public string GuAddress1_Road { get; set; }
        public string GuContact1_District { get; set; }
        public string GuContact1_Road { get; set; }
        //保證人二
        public string GuAddress2_District { get; set; }
        public string GuAddress2_Road { get; set; }
        public string GuContact2_District { get; set; }
        public string GuContact2_Road { get; set; }
        //保證人三
        public string GuAddress3_District { get; set; }
        public string GuAddress3_Road { get; set; }
        public string GuContact3_District { get; set; }
        public string GuContact3_Road { get; set; }
    }
    //秘書填寫
    public class SecretaryViewModel
    {
        [Required]
        [StringLength(50)]
        public string FormID { get; set; }

        public int qualifyRadio { get; set; }

        [Required]
        [StringLength(20)]
        public string excerpt { get; set; }

        [StringLength(20)]
        public string excerptShort { get; set; }

        [Required]
        [StringLength(20)]
        public string buildNo { get; set; }

        [Required]
        [StringLength(100)]
        public string placeNo { get; set; }

        public string buildCreateDate { get; set; }

        public int floorAmount { get; set; }

        public int floorNo { get; set; }

        public double squareAmount { get; set; }

        public double pinAmount { get; set; }

        public int notarizationFeeRadio { get; set; }

        public int rentMarket { get; set; }

        public int rentAgent { get; set; }

        public int depositAgent { get; set; }
        public string Memo { get; set; }
    }
    #endregion

    #region FormPost資料
    //物件資料
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
    //房東資料
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
    //房客資料
    public class TenantInputViewModel
    {
        public string FormID { get; set; }
        public string typeRadio { get; set; }
        public IEnumerable<HttpPostedFileBase> vulnerablefile { get; set; }
        public IEnumerable<HttpPostedFileBase> sheetfile { get; set; }
        public string Name_0 { get; set; }
        public string genderRadio_0 { get; set; }
        public string birthday_0 { get; set; }
        public string IDNumber_0 { get; set; }
        public string Phone_0 { get; set; }
        public string addressroad_0 { get; set; }
        public string AddressDetail_0 { get; set; }
        public bool? sameaddress_check_0 { get; set; }
        public string contactroad_0 { get; set; }
        public string detailcontact_0 { get; set; }
        public string accountnumber { get; set; }
        public string bank_0 { get; set; }
        public string bankbranche_0 { get; set; }
        public string bankaccount_0 { get; set; }

        //配偶
        public string coupleInput { get; set; }

        //直系親屬
        public string directCount { get; set; }
        public string directInput1 { get; set; }
        public string directInput2 { get; set; }
        public string directInput3 { get; set; }
        public string directInput4 { get; set; }
        public string directInput5 { get; set; }
        public string directInput6 { get; set; }
        public string directInput7 { get; set; }
        public string directInput8 { get; set; }
        public string directInput9 { get; set; }
        public string directInput10 { get; set; }

        //代理人
        public string AgentRadio { get; set; }
        public string AgentInput11 { get; set; }
        public string AgentInput12 { get; set; }
        public string AgentInput13 { get; set; }

        //保證人
        public string GuarantorRadio { get; set; }
        public string GuarantorInput21 { get; set; }
        public string GuarantorInput22 { get; set; }
        public string GuarantorInput23 { get; set; }

        public string memo { get; set; }
    }
    //秘書填寫
    public class SecretaryInputViewModel
    {
        [Required]
        [StringLength(50)]
        public string FormID { get; set; }

        public int qualifyRadio { get; set; }

        [Required]
        [StringLength(20)]
        public string excerpt { get; set; }

        [StringLength(20)]
        public string excerptShort { get; set; }

        [Required]
        [StringLength(20)]
        public string buildNo { get; set; }

        [Required]
        [StringLength(100)]
        public string placeNo { get; set; }

        public string buildCreatedate { get; set; }

        public int floorAmount { get; set; }

        public int floorNo { get; set; }

        public double squareAmount { get; set; }

        public double pinAmount { get; set; }

        public int notarizationFeeRadio { get; set; }

        public int rentMarket { get; set; }

        public int rentAgent { get; set; }

        public int depositAgent { get; set; }

        public string memo { get; set; }
    }
    #endregion

    #region 存取檔案命名
    public class FileViewMode
    {
        public string FileName { get; set; }
        public string FileAlias { get; set; }
        public string Path { get; set; }
    }
    #endregion

    #region 附件上傳(上傳區資料)
    //證件上船區
    //房東身分證正面
    public class Uploads
    {
        #region 證件上傳區
        //正面身分證(房東)
        public IEnumerable<FileViewMode> frontIdentityCardLandlord { get; set; }
        //反面身分證(房東)
        public IEnumerable<FileViewMode> reverseIdentityCardLandlord { get; set; }
        //存摺(房東)
        public IEnumerable<FileViewMode> passbookLandlord { get; set; }
        
        //正面身分證(房客)
        public IEnumerable<FileViewMode> frontIdentityCardTenant { get; set; }
        //反面身分證(房客)
        public IEnumerable<FileViewMode> reverseIdentityCardTenant { get; set; }
        //存摺(房客)
        public IEnumerable<FileViewMode> passbookTenant { get; set; }
        //戶籍謄本(房客)
        public IEnumerable<FileViewMode> householdTenant { get; set; }
        //財產清單(房客)
        public IEnumerable<FileViewMode> propertyTenant { get; set; }
        //所得清單(房客)
        public IEnumerable<FileViewMode> incomeTenant { get; set; }
        
        //證件補充資料
        public IEnumerable<FileViewMode> documentSupplement { get; set; }
        #endregion

        #region 屋況上傳區
        //門牌
        public IEnumerable<FileViewMode> housenumber { get; set; }
        //大門
        public IEnumerable<FileViewMode> door { get; set; }
        //衛浴設備
        public IEnumerable<FileViewMode> bath { get; set; }
        //物件大門
        public IEnumerable<FileViewMode> objectdoor { get; set; }
        //樓梯照
        public IEnumerable<FileViewMode> stairs { get; set; }
        //滅火器
        public IEnumerable<FileViewMode> fire { get; set; }
        //偵煙器
        public IEnumerable<FileViewMode> smoke { get; set; }
        //熱水器
        public IEnumerable<FileViewMode> water { get; set; }
        //補充資料
        public IEnumerable<FileViewMode> additional { get; set; }

        #endregion
    }

    #endregion
}