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
            var formmodel = ddlservices.GetRegionDDL("");
            ViewBag.citylist = JsonConvert.SerializeObject(formmodel.regionddl.ToList());
            List<int> payment_date = new List<int>();

            for(int i=1; i<32; i++)
            {
                payment_date.Add(i);
            }
            ViewBag.Payment_date = payment_date;
            return View();
        }
        [HttpPost]
        public ActionResult HomeObject(
            string notarizationRadio, 
            DateTime signdate, 
            string appraiserRadio, 
            string feature, 
            string selectcity,
            string selectdistrict,
            string selecctroad,
            string elseaddress,
            string useforRadio,
            string useforelse,
            HttpPostedFileBase taxFile,
            string rent,
            string deposit,
            string management_fee,
            DateTime startdate,
            DateTime enddate,
            int paydate,
            string buildtypeRadio,
            string roomtypeRadio,
            string roomamount,
            string hallamount,
            string bathamount,
            string carparkRadio,
            string parktypeRadio,
            string parkfloorRadio,
            string carpositionnumber,
            string carmonthrent,
            string scootermonthrent,
            string parkmanagementfee,
            string scootermanagementfee
            )
        {
            if (ModelState.IsValid)
            {
                var now = DateTime.Now;
                string newFormID = "LQ" + now.ToString("yy") + "000001";
                var lastformid = liqundb.AllForm.OrderByDescending(x => x.FormNo).Select(x=>x.FormId).FirstOrDefault();
                if (!String.IsNullOrEmpty(lastformid))
                {
                    var idIndex = Convert.ToInt32(lastformid.Substring(4));
                    newFormID = lastformid.Substring(0, 4) + (idIndex + 1).ToString("D6");
                }

                var userid = "enkisu";

                try
                {
                    //找到地址
                    var address = liqundb.Region.Where(x => x.RoadCode == selecctroad).FirstOrDefault();
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
                    using (var context = new LiqunModels())
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
                            elseaddress = elseaddress,
                            fulladdress = address.City + address.District + address.Road + elseaddress,
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
                            carpark = Convert.ToInt32(carparkRadio),
                            parktype = Convert.ToInt32(parktypeRadio),
                            parkfloor = Convert.ToInt32(parkfloorRadio),
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
                    using (var context = new LiqunModels())
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
                catch(Exception ex)
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
            var formmodel = ddlservices.GetRegionDDL("");
            ViewBag.citylist = JsonConvert.SerializeObject(formmodel.regionddl.ToList());
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
            string IDnumber_landlord, 
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

        #region 找到Region下拉選單
        public ActionResult DDLRegion(string regioncode)
        {
            DDLServices ddlservices = new DDLServices();
            var formmodel = ddlservices.GetRegionDDL(regioncode);

            var regionJson = JsonConvert.SerializeObject(formmodel.regionddl.ToList());
            return Json(regionJson);
        }
        #endregion

    }
}