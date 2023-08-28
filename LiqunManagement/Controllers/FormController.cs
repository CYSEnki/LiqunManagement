using LiqunManagement.Attributes;
using LiqunManagement.Models;
using LiqunManagement.Services;
using LiqunManagement.ViewModels;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Bcpg.Sig;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LiqunManagement.Controllers
{
    [AdminAuthorize]
    public class FormController : BaseController
    {
        // GET: Form
        public ActionResult Index()
        {
            #region 使用者資料
            var EmployeeData = (from db in memberdb.Members.Where(x => x.Account == User.Identity.Name)
                                join empdb in memberdb.EmployeeData on db.Account equals empdb.Account into temp
                                from empdb0 in temp.DefaultIfEmpty()
                                select new MembersViewModel
                                {
                                    Name = db.Name,
                                    Department = empdb0 != null ? empdb0.Department : null,
                                    Position = empdb0 != null ? empdb0.Position : null,
                                }).FirstOrDefault();
            if (EmployeeData != null)
            {
                ViewBag.UserName = EmployeeData.Name;                 //使用者名稱
                ViewBag.Department = EmployeeData.Department;   //使用者部門
                ViewBag.Position = EmployeeData.Position;       //使用者職位
            }
            //確認角色
            var role = User.IsInRole("Admin");
            //var role2 = User.IsInRole("User");
            #endregion
            return View();
        }
        [HttpPost]
        public ActionResult Index(string Inline1Radio1, DateTime signdate, string Inline1Radio2, string Specialtext)
        {
            #region 使用者資料
            var EmployeeData = (from db in memberdb.Members.Where(x => x.Account == User.Identity.Name)
                                join empdb in memberdb.EmployeeData on db.Account equals empdb.Account into temp
                                from empdb0 in temp.DefaultIfEmpty()
                                select new MembersViewModel
                                {
                                    Name = db.Name,
                                    Department = empdb0 != null ? empdb0.Department : null,
                                    Position = empdb0 != null ? empdb0.Position : null,
                                }).FirstOrDefault();
            if (EmployeeData != null)
            {
                ViewBag.UserName = EmployeeData.Name;                 //使用者名稱
                ViewBag.Department = EmployeeData.Department;   //使用者部門
                ViewBag.Position = EmployeeData.Position;       //使用者職位
            }
            //確認角色
            var role = User.IsInRole("Admin");
            //var role2 = User.IsInRole("User");
            #endregion
            if (ModelState.IsValid)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Memebers");
            }
        }

        #region 房屋物件資料
        public ActionResult HomeObject()
        {
            #region 使用者資料
            var EmployeeData = (from db in memberdb.Members.Where(x => x.Account == User.Identity.Name)
                                join empdb in memberdb.EmployeeData on db.Account equals empdb.Account into temp
                                from empdb0 in temp.DefaultIfEmpty()
                                select new MembersViewModel
                                {
                                    Name = db.Name,
                                    Department = empdb0 != null ? empdb0.Department : null,
                                    Position = empdb0 != null ? empdb0.Position : null,
                                }).FirstOrDefault();
            if (EmployeeData != null)
            {
                ViewBag.UserName = EmployeeData.Name;                 //使用者名稱
                ViewBag.Department = EmployeeData.Department;   //使用者部門
                ViewBag.Position = EmployeeData.Position;       //使用者職位
            }
            //確認角色
            var role = User.IsInRole("Admin");
            //var role2 = User.IsInRole("User");
            #endregion
            DDLServices ddlservices = new DDLServices();
            ViewBag.citylist = JsonConvert.SerializeObject(ddlservices.GetRegionDDL("").ddllist.ToList());
            List<int> payment_date = new List<int>();

            for (int i = 1; i < 32; i++)
            {
                payment_date.Add(i);
            }
            ViewBag.Payment_date = payment_date;
            return View();
        }
        [HttpPost]
        public ActionResult HomeObject(
            string objecttypeRadio,     //(radio)包租:1; 代管:0
            string notarizationRadio,   //(radio)公證:1; 非公證:0
            string signdate,            //(datetime)簽約日
            string appraiserRadio,      //(radio)簽估價師:1; 非簽估價師:0
            string feature,             //(text)特色
            string selecctroad,         //(ddl)物件地址
            string detailaddress,       //(text)地址細節
            string useforRadio,         //(radio)主要用途 住家用:0; 商業用:1; 辦公室:2; 一般事務所:3; 其他:4
            string useforelse,          //(text)主要用途 其他
            IEnumerable<HttpPostedFileBase> taxFile, //(file)上傳稅單
            string rent,                //(number)租金
            string deposit,             //(number)押金
            string management_fee,      //(number)管理費
            string startdate,           //(datetime)起租日
            string enddate,             //(datetime)結束日
            int paydate,                //(ddl)繳租日
            string buildtypeRadio,      //(radio)建物型態 透天厝:0; 公寓:1; 華夏:2; 電梯大樓:3
            string roomtypeRadio,       //(radio)房型 整層出租:0; 獨立套房:1;
            string roomamount,          //(text)房數
            string hallamount,          //(text)廳數
            string bathamount,          //(text)衛數
            bool? noparkcheck,         //(checkbox)車位 無車位:0
            bool? carparkcheck,        //(checkbox)車位 汽車車位:0
            bool? morparkcheck,        //(checkbox)車位 機車車位:0
            string parktypeRadio,       //(radio)汽車車位樣式 坡道平面:0; 坡道機械:1; 機械平面:2; 機械機械:3
            string carparkfloorRadio,   //(radio)汽車位於 地上:1; 地下:0
            string parkfloornumber,     //(number)汽車位於幾樓
            string carpositionnumber,   //(text)汽車位編號
            string carmonthrent,        //(text)汽車月租金
            string parkmanagementfee,   //(number)汽車管理費
            string morpositionnumber,   //(text)機車位編號
            string scootermonthrent,    //(number)機車月租金
            string scootermanagementfee,//(number)機車管理費

            string JsonHomeObjectAccessory, //房屋附屬家具
            string memo                 //備註
            )
        {
            #region 使用者資料
            var EmployeeData = (from db in memberdb.Members.Where(x => x.Account == User.Identity.Name)
                                join empdb in memberdb.EmployeeData on db.Account equals empdb.Account into temp
                                from empdb0 in temp.DefaultIfEmpty()
                                select new MembersViewModel
                                {
                                    Name = db.Name,
                                    Department = empdb0 != null ? empdb0.Department : null,
                                    Position = empdb0 != null ? empdb0.Position : null,
                                }).FirstOrDefault();
            if (EmployeeData != null)
            {
                ViewBag.UserName = EmployeeData.Name;                 //使用者名稱
                ViewBag.Department = EmployeeData.Department;   //使用者部門
                ViewBag.Position = EmployeeData.Position;       //使用者職位
            }
            //確認角色
            var role = User.IsInRole("Admin");
            //var role2 = User.IsInRole("User");
            #endregion
            var now = DateTime.Now;

            var userid = User.Identity.Name;

            string newFormID = "LQ" + now.ToString("yy") + "000001";
            var lastformid = formdb.ObjectForm.OrderByDescending(x => x.FormNo).Select(x => x.FormID).FirstOrDefault();
            if (!String.IsNullOrEmpty(lastformid))
            {
                var idIndex = Convert.ToInt32(lastformid.Substring(4));
                newFormID = lastformid.Substring(0, 4) + (idIndex + 1).ToString("D6");
            }



            int[] roomamountArray = new int[3];
            roomamountArray[0] = Convert.ToInt32(roomamount);
            roomamountArray[1] = Convert.ToInt32(hallamount);
            roomamountArray[2] = Convert.ToInt32(bathamount);
            string jsonroomamountArray = JsonConvert.SerializeObject(roomamountArray);

            int[] haveparkArray = new int[3];
            haveparkArray[0] = noparkcheck != null ? 1 : 0;
            haveparkArray[1] = carparkcheck != null ? 1 : 0;
            haveparkArray[2] = morparkcheck != null ? 1 : 0;
            string jsonhaveparkArray = JsonConvert.SerializeObject(haveparkArray);

            int[] parkfloorArray = new int[2];
            parkfloorArray[0] = Convert.ToInt32(carparkfloorRadio);
            parkfloorArray[1] = Convert.ToInt32(parkfloornumber);
            string jsonparkfloorArray = JsonConvert.SerializeObject(parkfloorArray);

            //民國 -> 西元
            var Signdate = Convert.ToDateTime(signdate).AddYears(1911);
            var Startdate = Convert.ToDateTime(startdate).AddYears(1911);
            var Enddate = Convert.ToDateTime(enddate).AddYears(1911);


            #region 存檔
            //取得檔名與檔案GUID
            List<string> fileNamesArray = new List<string>();
            List<string> taxfile_aliasArray = new List<string>();
            //存檔
            if (taxFile != null && taxFile.Any())
            {
                try
                {
                    foreach (var file in taxFile)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            string name = Path.GetFileName(file.FileName);
                            fileNamesArray.Add(name);
                            string alias = Guid.NewGuid().ToString() + Path.GetExtension(name);
                            taxfile_aliasArray.Add(alias);


                            string path = Path.Combine(Server.MapPath("~/Uploads/TaxFile"), alias);
                            file.SaveAs(path);
                        }
                    }
                }
                catch (Exception ex)
                {
                    var error = ex.ToString();
                }
            }
            string fileNames = JsonConvert.SerializeObject(fileNamesArray);
            string taxfile_alias = JsonConvert.SerializeObject(taxfile_aliasArray);
            #endregion


            try
            {
                //找到地址
                var address = formdb.Region.Where(x => x.RoadCode == selecctroad).FirstOrDefault();

                // 建立資料上下文（Data Context）
                using (var context = new FormModels())
                {
                    // 建立要插入的資料物件
                    var newData = new HomeObject
                    {
                        FormID = newFormID,
                        objecttype = Convert.ToInt32(objecttypeRadio),
                        notarization = Convert.ToInt32(notarizationRadio),
                        signdate = Signdate,
                        appraiser = Convert.ToInt32(appraiserRadio),
                        feature = feature,
                        city = address.City,
                        district = address.District,
                        road = address.Road,
                        detailaddress = detailaddress,
                        fulladdress = address.City + address.District + address.Road + detailaddress,
                        usefor = Convert.ToInt32(useforRadio),
                        useforelse = useforelse,
                        taxfile_name = fileNames,
                        taxfile_alias = taxfile_alias,
                        rent = Convert.ToInt32(rent),
                        deposit = Convert.ToInt32(deposit),
                        management_fee = Convert.ToInt32(management_fee),
                        startdate = Startdate,
                        enddate = Enddate,
                        paydate = paydate,
                        buildtype = Convert.ToInt32(buildtypeRadio),
                        roomtype = Convert.ToInt32(roomtypeRadio),
                        roomamount = jsonroomamountArray,
                        havepark = jsonhaveparkArray,
                        parktype = Convert.ToInt32(parktypeRadio),
                        parkfloor = jsonparkfloorArray,
                        carpositionnumber = carpositionnumber,
                        carmonthrent = String.IsNullOrEmpty(carmonthrent) ? 0 : Convert.ToInt32(carmonthrent),
                        carparkmanagefee = String.IsNullOrEmpty(parkmanagementfee) ? 0 : Convert.ToInt32(parkmanagementfee),
                        scooterpositionnumber = morpositionnumber,
                        scootermonthrent = String.IsNullOrEmpty(scootermonthrent) ? 0 : Convert.ToInt32(scootermonthrent),
                        scootermanagefee = String.IsNullOrEmpty(scootermanagementfee) ? 0 : Convert.ToInt32(scootermanagementfee),
                        Accessory = JsonHomeObjectAccessory,

                        CreateUser = userid,
                        CreateTime = now,
                        UpdateUser = userid,
                        UpdateTime = now,
                        Memo = memo,
                    };
                    // 使用資料上下文插入資料物件
                    context.HomeObject.Add(newData);
                    // 儲存更改到資料庫
                    context.SaveChanges();
                }

                // 建立資料上下文（Data Context）
                using (var context = new FormModels())
                {
                    // 建立要插入的資料物件
                    var newData = new ObjectForm
                    {
                        FormID = newFormID,
                        CreateAccount = userid,
                        CreateTime = now,
                        UpdateAccount = userid,
                        UpdateTime = now,
                        ProcessAccount = userid,
                        ProcessName = EmployeeData.Name,
                        FormType = 0,
                    };
                    // 使用資料上下文插入資料物件
                    context.ObjectForm.Add(newData);
                    // 儲存更改到資料庫
                    context.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                var error = ex.ToString();
            }
            return RedirectToAction("HomeObject", "Form");
        }
        #endregion

        #region 房東資料
        [HttpGet]
        public ActionResult Landlord(string FormID)
        {
            #region 使用者資料
            var EmployeeData = (from db in memberdb.Members.Where(x => x.Account == User.Identity.Name)
                                join empdb in memberdb.EmployeeData on db.Account equals empdb.Account into temp
                                from empdb0 in temp.DefaultIfEmpty()
                                select new MembersViewModel
                                {
                                    Name = db.Name,
                                    Department = empdb0 != null ? empdb0.Department : null,
                                    Position = empdb0 != null ? empdb0.Position : null,
                                }).FirstOrDefault();
            if (EmployeeData != null)
            {
                ViewBag.UserName = EmployeeData.Name;                 //使用者名稱
                ViewBag.Department = EmployeeData.Department;   //使用者部門
                ViewBag.Position = EmployeeData.Position;       //使用者職位
            }
            //確認角色
            var role = User.IsInRole("Admin");
            //var role2 = User.IsInRole("User");
            #endregion
            ViewBag.FormID = FormID != null ? FormID : "";
            DDLServices ddlservices = new DDLServices();
            ViewBag.citylist = JsonConvert.SerializeObject(ddlservices.GetRegionDDL("").ddllist.ToList());
            ViewBag.banklist = JsonConvert.SerializeObject(ddlservices.GetBankDDL("", "bank").ddllist.ToList());
            List<int> payment_date = new List<int>();

            for (int i = 1; i < 32; i++)
            {
                payment_date.Add(i);
            }
            ViewBag.Payment_date = payment_date;
            var Form = formdb.LandLord.Where(x => x.FormID == FormID).FirstOrDefault();
            if(Form != null)
            {
                return RedirectToAction("CaseManage", "Sales");
            }



            return View();
        }
        [HttpPost]
        public ActionResult Landlord(
            string FormID,                  //表單編號

            string Name_0,                  //房東姓名
            string genderRadio_0,           //房東性別(男:1; 女:0)
            string birthday_0,              //房東生日
            string IDNumber_0,              //房東身分證
            string Phone_0,                 //房東電話
            string addressroad_0,           //房東地址(路)
            string detailaddress_0,         //房東詳細地址  
            bool? sameaddress_check_0,       //房東通訊地址Checkbox
            string contactroad_0,           //房東通訊地址(路)
            string detailcontact_0,         //房東詳細通訊地址
            string bank_0,                  //房東銀行
            string bankbranche_0,           //房東銀行支部
            string bankaccount_0,           //房東銀行帳號

            //共有人(Json格式)
            string CoOwnerRadio,
            string CoOwnerInput1,
            string CoOwnerInput2,
            string CoOwnerInput3,
            string CoOwnerInput4,
            string CoOwnerInput5,

            //代理人(Json格式)
            bool? agentCheckbox,
            string AgentInput,

            string memo
            )
        {
            #region 使用者資料
            var EmployeeData = (from db in memberdb.Members.Where(x => x.Account == User.Identity.Name)
                                join empdb in memberdb.EmployeeData on db.Account equals empdb.Account into temp
                                from empdb0 in temp.DefaultIfEmpty()
                                select new MembersViewModel
                                {
                                    Name = db.Name,
                                    Department = empdb0 != null ? empdb0.Department : null,
                                    Position = empdb0 != null ? empdb0.Position : null,
                                }).FirstOrDefault();
            if (EmployeeData != null)
            {
                ViewBag.UserName = EmployeeData.Name;                 //使用者名稱
                ViewBag.Department = EmployeeData.Department;   //使用者部門
                ViewBag.Position = EmployeeData.Position;       //使用者職位
            }
            //確認角色
            var role = User.IsInRole("Admin");
            //var role2 = User.IsInRole("User");
            #endregion
            var now = DateTime.Now;
            var userid = "enkisu";
            if (String.IsNullOrEmpty(FormID))
            {
                return View();
            }
            if(sameaddress_check_0 != null)
            {
                contactroad_0 = addressroad_0;
                detailcontact_0 = detailaddress_0;
            }

            //成員陣列[共有人, 代理人]
            int[] memberArray = new int[2];
            memberArray[0] = Convert.ToInt32(CoOwnerRadio);
            memberArray[1] = agentCheckbox != null ? 1: 0;
            string jsonmemberArray = JsonConvert.SerializeObject(memberArray);

            try
            {
                // 建立資料上下文（Data Context）
                using (var context = new FormModels())
                {
                    // 建立要插入的資料物件
                    var newData = new LandLord
                    {
                        FormID = FormID,
                        Name = Name_0,
                        Gender = Convert.ToInt32(genderRadio_0),
                        Birthday = Convert.ToDateTime(birthday_0),
                        IDNumber = IDNumber_0,
                        Phone = Phone_0,
                        BankNo = bank_0,
                        BrancheNo = bankbranche_0,
                        BankAccount = bankaccount_0,
                        RoadCode = addressroad_0,
                        detailaddress = detailaddress_0,
                        RoadCodeContact = contactroad_0,
                        detailaddressContact = detailcontact_0,
                        MemberArray = jsonmemberArray,
                        CoOwner1 = CoOwnerInput1,
                        CoOwner2 = CoOwnerInput2,
                        CoOwner3 = CoOwnerInput3,
                        CoOwner4 = CoOwnerInput4,
                        CoOwner5 = CoOwnerInput5,
                        Agent = AgentInput,

                        CreateUser = userid,
                        CreateTime = now,
                        UpdateUser = userid,
                        UpdateTime = now,
                        Memo = memo,
                    };
                    // 使用資料上下文插入資料物件
                    context.LandLord.Add(newData);
                    // 儲存更改到資料庫
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                var error = ex.ToString();
                return RedirectToAction("Landlord", "Form", new {FormID = FormID});
            }

            return RedirectToAction("CaseManage", "Sales");
        }
        #endregion

        #region 房客資料
        public ActionResult Tenant(string FormID)
        {
            #region 使用者資料
            var EmployeeData = (from db in memberdb.Members.Where(x => x.Account == User.Identity.Name)
                                join empdb in memberdb.EmployeeData on db.Account equals empdb.Account into temp
                                from empdb0 in temp.DefaultIfEmpty()
                                select new MembersViewModel
                                {
                                    Name = db.Name,
                                    Department = empdb0 != null ? empdb0.Department : null,
                                    Position = empdb0 != null ? empdb0.Position : null,
                                }).FirstOrDefault();
            if (EmployeeData != null)
            {
                ViewBag.UserName = EmployeeData.Name;                 //使用者名稱
                ViewBag.Department = EmployeeData.Department;   //使用者部門
                ViewBag.Position = EmployeeData.Position;       //使用者職位
            }
            //確認角色
            var role = User.IsInRole("Admin");
            //var role2 = User.IsInRole("User");
            #endregion
            ViewBag.FormID = FormID != null ? FormID : "";
            DDLServices ddlservices = new DDLServices();
            ViewBag.citylist = JsonConvert.SerializeObject(ddlservices.GetRegionDDL("").ddllist.ToList());
            ViewBag.banklist = JsonConvert.SerializeObject(ddlservices.GetBankDDL("", "bank").ddllist.ToList());

            //房客資料是否已填寫過
            var Form = formdb.Tenant.Where(x => x.FormID == FormID).FirstOrDefault();
            if (Form != null)
            {
                return RedirectToAction("CaseManage", "Sales");
            }
            List<int> payment_date = new List<int>();
            for (int i = 1; i < 32; i++)
            {
                payment_date.Add(i);
            }
            ViewBag.Payment_date = payment_date;
            return View();
        }
        [HttpPost]
        public ActionResult Tenant(
            string FormID,                  //表單編號

            string typeRadio,               //房客分類(一般戶; 一類戶; 二類戶)
            IEnumerable<HttpPostedFileBase> vulnerablefile,     //(file)上傳弱勢戶佐證文件
            IEnumerable<HttpPostedFileBase> sheetfile,          //(file)上傳300億試算表截圖

            string Name_0,                  //房客姓名
            string genderRadio_0,           //房客性別(男:1; 女:0)
            string birthday_0,              //房客生日
            string IDNumber_0,              //房客身分證
            string Phone_0,                 //房客電話
            string addressroad_0,           //房客地址(路)
            string detailaddress_0,         //房客詳細地址  
            bool? sameaddress_check_0,      //房客通訊地址Checkbox
            string contactroad_0,           //房客通訊地址(路)
            string detailcontact_0,         //房客詳細通訊地址
            string accountnumber,           //房客戶號
            string bank_0,                  //房客銀行
            string bankbranche_0,           //房客銀行支部
            string bankaccount_0,           //房東銀行帳號

            //配偶
            bool? couplecheck_1,            //丈夫
            bool? couplecheck_0,            //妻子
            string coupleName,              //配偶姓名
            string birthday_couple,         //配偶生日
            string separatenumber,          //分戶戶號(非必填)

            //直系親屬(Json格式)
            string directCount,             //親屬人數
            string DirectInput1,
            string DirectInput2,
            string DirectInput3,
            string DirectInput4,
            string DirectInput5,
            string DirectInput6,
            string DirectInput7,
            string DirectInput8,
            string DirectInput9,
            string DirectInput10,

            //代理人(Json格式)
            string AgentRadio,
            string AgentInput11,
            string AgentInput12,
            string AgentInput13,

            //保證人(Json格式)
            string GuarantorRadio,
            string GuarantorInput21,
            string GuarantorInput22,
            string GuarantorInput23,

            string memo
            )
        {
            #region 使用者資料
            var EmployeeData = (from db in memberdb.Members.Where(x => x.Account == User.Identity.Name)
                                join empdb in memberdb.EmployeeData on db.Account equals empdb.Account into temp
                                from empdb0 in temp.DefaultIfEmpty()
                                select new MembersViewModel
                                {
                                    Name = db.Name,
                                    Department = empdb0 != null ? empdb0.Department : null,
                                    Position = empdb0 != null ? empdb0.Position : null,
                                }).FirstOrDefault();
            if (EmployeeData != null)
            {
                ViewBag.UserName = EmployeeData.Name;                 //使用者名稱
                ViewBag.Department = EmployeeData.Department;   //使用者部門
                ViewBag.Position = EmployeeData.Position;       //使用者職位
            }
            //確認角色
            var role = User.IsInRole("Admin");
            //var role2 = User.IsInRole("User");
            #endregion
            var now = DateTime.Now;
            var userid = "enkisu";

            if (String.IsNullOrEmpty(FormID))
            {
                return View();
            }
            if (sameaddress_check_0 != null)
            {
                contactroad_0 = addressroad_0;
                detailcontact_0 = detailaddress_0;
            }

            #region 存檔(弱勢戶)
            //取得檔名與檔案GUID
            List<string> vulnerablefileNameArray = new List<string>();
            List<string> vulnerablefileAliasArray = new List<string>();
            //存檔
            if (vulnerablefile != null && vulnerablefile.Any())
            {
                try
                {
                    foreach (var file in vulnerablefile)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            string name = Path.GetFileName(file.FileName);
                            vulnerablefileNameArray.Add(name);
                            string alias = Guid.NewGuid().ToString() + Path.GetExtension(name);
                            vulnerablefileAliasArray.Add(alias);

                            string path = Path.Combine(Server.MapPath("~/Uploads/TaxFile"), alias);
                            file.SaveAs(path);
                        }
                    }
                }
                catch (Exception ex)
                {
                    var error = ex.ToString();
                }
            }
            string vulnerablefileNames = JsonConvert.SerializeObject(vulnerablefileNameArray);
            string vulnerablefileAlias = JsonConvert.SerializeObject(vulnerablefileAliasArray);
            #endregion
            
            #region 存檔(300E)
            //取得檔名與檔案GUID
            List<string> sheetfileNameArray = new List<string>();
            List<string> sheetfileAliasArray = new List<string>();
            //存檔
            if (vulnerablefile != null && vulnerablefile.Any())
            {
                try
                {
                    foreach (var file in vulnerablefile)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            string name = Path.GetFileName(file.FileName);
                            sheetfileNameArray.Add(name);
                            string alias = Guid.NewGuid().ToString() + Path.GetExtension(name);
                            sheetfileAliasArray.Add(alias);

                            string path = Path.Combine(Server.MapPath("~/Uploads/TaxFile"), alias);
                            file.SaveAs(path);
                        }
                    }
                }
                catch (Exception ex)
                {
                    var error = ex.ToString();
                }
            }
            string sheetfileNames = JsonConvert.SerializeObject(sheetfileNameArray);
            string sheetfileAlias = JsonConvert.SerializeObject(sheetfileAliasArray);
            #endregion


            //成員陣列[配偶, 直系, 代理人, 保證人]
            var coupletype = couplecheck_1 != null ? 1 : couplecheck_0 == null ? 0 : -1;    //-1:無; 1:丈夫; 0:妻子;
            int[] memberArray = new int[4];
            memberArray[0] = coupletype;
            memberArray[1] = Convert.ToInt32(directCount);
            memberArray[2] = Convert.ToInt32(AgentRadio);
            memberArray[3] = Convert.ToInt32(GuarantorRadio);
            string jsonmemberArray = JsonConvert.SerializeObject(memberArray);

            //打包配偶資料陣列[配偶類別, 配偶姓名, 配偶生日, 分戶戶號(非必填)]
            List<string> coupleArray = new List<string>();

            if (coupletype != -1)
            {
                coupleArray.Add(coupletype.ToString());
                coupleArray.Add(coupleName);
                coupleArray.Add(birthday_couple);
                coupleArray.Add(String.IsNullOrEmpty(separatenumber) ? "" : separatenumber);
            }
            string jsoncoupleArray = JsonConvert.SerializeObject(coupleArray);


            try
            {
                // 建立資料上下文（Data Context）
                using (var context = new FormModels())
                {
                    // 建立要插入的資料物件
                    var newData = new Tenant
                    {
                        FormID = FormID,
                        TenantType = Convert.ToInt32(typeRadio),
                        vulnerablefile_Name = vulnerablefileNames,
                        vulnerablefile_Alias = vulnerablefileAlias,
                        sheetfile_Name = sheetfileNames,
                        sheetfile_Alias = sheetfileAlias,
                        Name = Name_0,
                        Gender = Convert.ToInt32(genderRadio_0),
                        Birthday = Convert.ToDateTime(birthday_0),
                        IDNumber = IDNumber_0,
                        Phone = Phone_0,
                        BankNo = bank_0,
                        BrancheNo = bankbranche_0,
                        BankAccount = bankaccount_0,
                        RoadCode = addressroad_0,
                        accountNo = accountnumber,
                        detailaddress = detailaddress_0,
                        RoadCodeContact = contactroad_0,
                        detailaddressContact = detailcontact_0,
                        MemberArray = jsonmemberArray,
                        Couple = jsoncoupleArray,
                        Family1 = DirectInput1,
                        Family2 = DirectInput2,
                        Family3 = DirectInput3,
                        Family4 = DirectInput4,
                        Family5 = DirectInput5,
                        Family6 = DirectInput6,
                        Family7 = DirectInput7,
                        Family8 = DirectInput8,
                        Family9 = DirectInput9,
                        Family10 = DirectInput10,
                        Agent1 = AgentInput11,
                        Agent2 = AgentInput12,
                        Agent3 = AgentInput13,
                        Guarantor1 = GuarantorInput21,
                        Guarantor2 = GuarantorInput22,
                        Guarantor3 = GuarantorInput23,

                        CreateUser = userid,
                        CreateTime = now,
                        UpdateUser = userid,
                        UpdateTime = now,
                        Memo = memo,
                    };
                    // 使用資料上下文插入資料物件
                    context.Tenant.Add(newData);
                    // 儲存更改到資料庫
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                var error = ex.ToString();
                return RedirectToAction("Tenant", "Form", new { FormID = FormID });
            }

            return RedirectToAction("CaseManage", "Sales");
        }
        #endregion

        #region 秘書填寫
        [HttpGet]
        public ActionResult Secretary()
        {
            #region 使用者資料
            var EmployeeData = (from db in memberdb.Members.Where(x => x.Account == User.Identity.Name)
                                join empdb in memberdb.EmployeeData on db.Account equals empdb.Account into temp
                                from empdb0 in temp.DefaultIfEmpty()
                                select new MembersViewModel
                                {
                                    Name = db.Name,
                                    Department = empdb0 != null ? empdb0.Department : null,
                                    Position = empdb0 != null ? empdb0.Position : null,
                                }).FirstOrDefault();
            if (EmployeeData != null)
            {
                ViewBag.UserName = EmployeeData.Name;                 //使用者名稱
                ViewBag.Department = EmployeeData.Department;   //使用者部門
                ViewBag.Position = EmployeeData.Position;       //使用者職位
            }
            //確認角色
            var role = User.IsInRole("Admin");
            //var role2 = User.IsInRole("User");
            #endregion
            return View();
        }
        [HttpPost]
        public ActionResult Secretary(
            string LandString,
            string LandNumber,
            string BuildString,
            string BuildNumber,
            string PingNumber,
            string Squaremeter,
            bool? ParkingSpace,
            bool? ParkingSpace2,
            string ManageFee,
            string ParkManageFee,
            HttpPostedFileBase PowerFile
            )
        {
            #region 使用者資料
            var EmployeeData = (from db in memberdb.Members.Where(x => x.Account == User.Identity.Name)
                               join empdb in memberdb.EmployeeData on db.Account equals empdb.Account into temp
                               from empdb0 in temp.DefaultIfEmpty()
                               select new MembersViewModel
                               {
                                   Name = db.Name,
                                   Department = empdb0 != null ? empdb0.Department : null,
                                   Position = empdb0 != null ? empdb0.Position : null,
                               }).FirstOrDefault();
            if (EmployeeData != null)
            {
                ViewBag.UserName = EmployeeData.Name;                 //使用者名稱
                ViewBag.Department = EmployeeData.Department;   //使用者部門
                ViewBag.Position = EmployeeData.Position;       //使用者職位
            }
            //確認角色
            var role = User.IsInRole("Admin");
            //var role2 = User.IsInRole("User");
            #endregion
            DDLServices ddlservices = new DDLServices();
            ViewBag.citylist = JsonConvert.SerializeObject(ddlservices.GetRegionDDL("").ddllist.ToList());
            List<int> payment_date = new List<int>();

            for (int i = 1; i < 32; i++)
            {
                payment_date.Add(i);
            }
            ViewBag.Payment_date = payment_date;
            return View();
        }
        #endregion

        #region 下拉選單變更事件
        //地址
        public ActionResult DDLRegion(string regioncode)
        {
            DDLServices ddlservices = new DDLServices();
            var regionJson = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(regioncode).ddllist.ToList());
            return Json(regionJson);
        }
        //銀行
        public ActionResult DDLBank(string bankcode)
        {
            DDLServices ddlservices = new DDLServices();
            var bankJson = JsonConvert.SerializeObject(ddlservices.GetBankDDL(bankcode, "branches").ddllist.ToList());

            //字元大小
            var banklength = (from db in formdb.Bank.Where(x => x.RootCheck == true && x.BankCode == bankcode)
                             select new
                             {
                                 minlength = db.CodeMinlength,
                                 maxlength = db.CodeMaxlength,
                             }).FirstOrDefault();
            string[] mindata = new string[0];
            if(banklength != null)
            {
                mindata = banklength.minlength.Split(',');
                int datacount = mindata.Length;
                if(mindata.Length == 1)
                {
                    mindata = new string[0];
                }
            }


            var result = new
            {
                lengthset = mindata,
                minlength = banklength.minlength,
                maxlength = banklength.maxlength,
                bankJson = bankJson
            };


            return Json(result);
        }
        #endregion

    }
}