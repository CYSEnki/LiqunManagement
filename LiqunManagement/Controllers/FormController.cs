using LiqunManagement.Services;
using LiqunManagement.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Management;
using System.Web.Mvc;

namespace LiqunManagement.Controllers
{
    public class FormController : BaseController
    {
        // GET: Form
        public new ActionResult Index()
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
            DateTime signdate,          //(datetime)簽約日
            string appraiserRadio,      //(radio)簽估價師:1; 非簽估價師:0
            string feature,             //(text)特色
            string selecctroad,         //(ddl)物件地址
            string detailaddress,       //(text)地址細節
            string useforRadio,         //(radio)主要用途 住家用:0; 商業用:1; 辦公室:2; 一般事務所:3; 其他:4
            string useforelse,          //(text)主要用途 其他
            HttpPostedFileBase taxFile, //(file)上傳稅單
            string rent,                //(number)租金
            string deposit,             //(number)押金
            string management_fee,      //(number)管理費
            DateTime startdate,         //(datetime)起租日
            DateTime enddate,           //(datetime)結束日
            int paydate,                //(ddl)繳租日
            string buildtypeRadio,      //(radio)建物型態 透天厝:0; 公寓:1; 華夏:2; 電梯大樓:3
            string roomtypeRadio,       //(radio)房型 整層出租:0; 獨立套房:1;
            string roomamount,          //(text)房數
            string hallamount,          //(text)廳數
            string bathamount,          //(text)衛數


            string noparkcheck,         //(checkbox)車位 無車位:0
            string carparkcheck,        //(checkbox)車位 汽車車位:0
            string morparkcheck,        //(checkbox)車位 機車車位:0
            string parktypeRadio,       //(radio)汽車車位樣式 坡道平面:0; 坡道機械:1; 機械平面:2; 機械機械:3
            string carparkfloorRadio,   //(radio)汽車位於 地上:1; 地下:0
            string parkfloornumber,     //(number)汽車位於幾樓
            string carpositionnumber,   //(text)汽車位編號
            string carmonthrent,        //(text)汽車月租金
            string parkmanagementfee,   //(number)汽車管理費
            string morpositionnumber,   //(text)機車位編號
            string scootermonthrent,    //(number)機車月租金
            string scootermanagementfee //(number)機車管理費
            )
        {
            if (ModelState.IsValid)
            {
                var now = DateTime.Now;
                string newFormID = "LQ" + now.ToString("yy") + "000001";
                var lastformid = formdb.AllForm.OrderByDescending(x => x.FormNo).Select(x => x.FormId).FirstOrDefault();
                if (!String.IsNullOrEmpty(lastformid))
                {
                    var idIndex = Convert.ToInt32(lastformid.Substring(4));
                    newFormID = lastformid.Substring(0, 4) + (idIndex + 1).ToString("D6");
                }

                var userid = "enkisu";

                try
                {
                    //找到地址
                    var address = formdb.Region.Where(x => x.RoadCode == selecctroad).FirstOrDefault();
                    //將金額型別轉換為int
                    rent = new string(rent.Where(char.IsDigit).ToArray());
                    int rent_Integer = int.Parse(rent);
                    deposit = new string(deposit.Where(char.IsDigit).ToArray());
                    int deposit_Integer = int.Parse(deposit);
                    management_fee = new string(management_fee.Where(char.IsDigit).ToArray());
                    int management_fee_Integer = int.Parse(management_fee);
                    carmonthrent = new string(carmonthrent.Where(char.IsDigit).ToArray());
                    int carmonthrent_Integer = int.Parse(carmonthrent);
                    scootermonthrent = new string(scootermonthrent.Where(char.IsDigit).ToArray());
                    int scootermonthrent_Integer = int.Parse(scootermonthrent);
                    parkmanagementfee = new string(parkmanagementfee.Where(char.IsDigit).ToArray());
                    int parkmanagementfee_Integer = int.Parse(parkmanagementfee);
                    scootermanagementfee = new string(scootermanagementfee.Where(char.IsDigit).ToArray());
                    int scootermanagementfee_Integer = int.Parse(scootermanagementfee);

                    // 建立資料上下文（Data Context）
                    using (var context = new FormModels())
                    {
                        // 建立要插入的資料物件
                        var newData = new HomeObject
                        {
                            FormId = newFormID,
                            notarization = Convert.ToInt32(notarizationRadio),
                            signdate = signdate,
                            appraiser = Convert.ToInt32(appraiserRadio),
                            feature = feature,
                            city = address.City,
                            district = address.District,
                            road = address.Road,
                            elseaddress = detailaddress,
                            fulladdress = address.City + address.District + address.Road + detailaddress,
                            usefor = Convert.ToInt32(useforRadio),
                            useforelse = useforelse,
                            taxfile_name = taxFile.FileName,
                            taxfile_guid = Guid.NewGuid().ToString(),
                            rent = rent_Integer,
                            deposit = deposit_Integer,
                            management_fee = management_fee_Integer,
                            startdate = startdate,
                            enddate = enddate,
                            paydate = paydate,
                            buildtype = Convert.ToInt32(buildtypeRadio),
                            roomtype = Convert.ToInt32(roomtypeRadio),
                            roomamount = roomtypeRadio == "1" ? "套" : roomamount,
                            hallamount = Convert.ToInt32(hallamount),
                            bathamount = Convert.ToInt32(bathamount),
                            carpark = Convert.ToInt32(detailaddress),
                            parktype = Convert.ToInt32(parktypeRadio),
                            parkfloor = Convert.ToInt32(parkfloornumber),
                            carpositionnumber = carpositionnumber,
                            carmonthrent = carmonthrent_Integer,
                            scootermonthrent = scootermonthrent_Integer,
                            parkmanagementfee = parkmanagementfee_Integer,
                            scootermanagementfee = scootermanagementfee_Integer,
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
                        var newData = new AllForm
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
                        context.AllForm.Add(newData);
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
            else
            {
                return RedirectToAction("Login", "Memebers");
            }
        }
        #endregion

        #region 房東資料
        public ActionResult Landlord()
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
        public ActionResult Landlord(
            string name_landlord,
            string genderRadio_landlord,
            DateTime birthday_landlord,
            string inputIDnumber_landlord,
            string phonenumber_landlord,
            string road_address_landlord,
            string elseaddress_landlord,
            bool sameaddress_landlord,
            string road_contact_landlord,
            string elseaddress_contact_landlord

            )
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction("HomeObject", "Form");
            }
            else
            {
                return RedirectToAction("Login", "Memebers");
            }
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
        public ActionResult Tenant(string FormID)
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