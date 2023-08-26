using LiqunManagement.Models;
using LiqunManagement.Services;
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
    public class FormController : BaseController
    {
        // GET: Form
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(string Inline1Radio1, DateTime signdate, string Inline1Radio2, string Specialtext)
        {
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
            var now = DateTime.Now;
            string newFormID = "LQ" + now.ToString("yy") + "000001";
            var lastformid = formdb.ObjectForm.OrderByDescending(x => x.FormNo).Select(x => x.FormId).FirstOrDefault();
            if (!String.IsNullOrEmpty(lastformid))
            {
                var idIndex = Convert.ToInt32(lastformid.Substring(4));
                newFormID = lastformid.Substring(0, 4) + (idIndex + 1).ToString("D6");
            }

            var userid = "enkisu";


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
            #endregion

            string fileNames = JsonConvert.SerializeObject(fileNamesArray);
            string taxfile_alias = JsonConvert.SerializeObject(taxfile_aliasArray);

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
                        FormId = newFormID,
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
                        carmonthrent = Convert.ToInt32(carmonthrent),
                        carparkmanagefee = Convert.ToInt32(parkmanagementfee),
                        scooterpositionnumber = morpositionnumber,
                        scootermonthrent = Convert.ToInt32(scootermonthrent),
                        scootermanagefee = Convert.ToInt32(scootermanagementfee),
                        Accessory = JsonHomeObjectAccessory,
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
                        FormId = newFormID,
                        CreateAccount = userid,
                        CreateTime = now,
                        UpdateAccount = userid,
                        UpdateTime = now,
                        ProcessAccount = userid,
                        ProcessName = "蘇家潁",
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
            var Form = formdb.LandLord.Where(x => x.FormId == FormID).FirstOrDefault();
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

            //共有人(Json格式)
            string CoOwnerRadio,
            string CoOwnerInput1,
            string CoOwnerInput2,
            string CoOwnerInput3,
            string CoOwnerInput4,
            string CoOwnerInput5,

            //代理人(Json格式)
            string AgentInput
            //bool? agentCheckbox,            //代理人Chcekbox
            //string Name_11,                 //代理人姓名
            //string genderRadio_11,          //代理人性別(男:1; 女:0)
            //string birthday_11,             //代理人生日
            //string IDNumber_11,             //代理人身分證
            //string Phone_11,                //代理人電話
            //string addressroad_11,          //代理人地址(路)
            //string detailaddress_11,        //代理人詳細地址  
            //bool? sameaddress_check_11,     //代理人通訊地址Checkbox
            //string contactroad_11,          //代理人通訊地址(路)
            //string detailcontact_11,        //代理人詳細通訊地址
            )
        {
            if(FormID == null)
            {
                return View();
            }
            if(sameaddress_check_0 != null)
            {
                contactroad_0 = addressroad_0;
                detailcontact_0 = detailaddress_0;
            }
            try
            {
                // 建立資料上下文（Data Context）
                using (var context = new FormModels())
                {
                    // 建立要插入的資料物件
                    var newData = new LandLord
                    {
                        FormId = FormID,
                        Name = Name_0,
                        Gender = Convert.ToInt32(genderRadio_0),
                        Birthday = Convert.ToDateTime(birthday_0),
                        IDNumber = IDNumber_0,
                        Phone = Phone_0,
                        BankNo = bank_0,
                        BrancheNo = bankbranche_0,
                        RoadCode = addressroad_0,
                        detailaddress = detailaddress_0,
                        RoadCodeContact = contactroad_0,
                        detailaddressContact = detailcontact_0,
                        CoOwner1 = CoOwnerInput1,
                        CoOwner2 = CoOwnerInput2,
                        CoOwner3 = CoOwnerInput3,
                        CoOwner4 = CoOwnerInput4,
                        CoOwner5 = CoOwnerInput5,
                        Agent = AgentInput,
                        CreateTime = DateTime.Now,
                        CreateUser = "Enkisu",
                        UpdateTime = DateTime.Now,
                        UpdateUser = "Enkisu",
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

            return View();
        }
        #endregion

        #region 房客資料
        public ActionResult Tenant()
        {
            DDLServices ddlservices = new DDLServices();
            ViewBag.citylist = JsonConvert.SerializeObject(ddlservices.GetRegionDDL("").ddllist.ToList());
            ViewBag.banklist = JsonConvert.SerializeObject(ddlservices.GetBankDDL("", "bank").ddllist.ToList());

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

            //共有人(Json格式)
            string CoOwnerRadio,
            string CoOwnerInput1,
            string CoOwnerInput2,
            string CoOwnerInput3,
            string CoOwnerInput4,
            string CoOwnerInput5
            )
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction("Tenant", "Form");
            }
            else
            {
                return RedirectToAction("Tenant", "Memebers");
            }
        }
        #endregion

        #region 秘書填寫
        [HttpGet]
        public ActionResult Secretary()
        {
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