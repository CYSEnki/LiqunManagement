using LiqunManagement.Attributes;
using LiqunManagement.Models;
using LiqunManagement.Services;
using LiqunManagement.ViewModels;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using NPOI.SS.Util;
using NPOI.Util;
using Org.BouncyCastle.Bcpg.Sig;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;

namespace LiqunManagement.Controllers
{
    [AdminAuthorize]
    public class FormController : BaseController
    {
        // Initial Model
        public FormViewModels InitialModel()
        {
            var model = new FormViewModels
            {
                FormID = null,
                homeobjectviewmodel = null,
            };

            return model;
        }

        #region 房屋物件資料
        //頁面讀取
        public ActionResult HomeObject(string FormID)
        {
            logger.Info("進入房屋物件資料 FormID : " + (FormID != null ? FormID : ""));
            #region 使用者資料
            var EmployeeData = (from db in memberdb.Members.Where(x => x.Account == User.Identity.Name)
                                join empdb in memberdb.EmployeeData on db.Account equals empdb.Account into temp
                                from empdb0 in temp.DefaultIfEmpty()
                                join deptdb in memberdb.Department on empdb0.DivCode equals deptdb.DivCode into temp2
                                from deptdb0 in temp2.DefaultIfEmpty()
                                select new MembersViewModel
                                {
                                    Name = db.Name,
                                    Department = empdb0 != null ? deptdb0.DivFullName : null,
                                    Position = empdb0 != null ? empdb0.JobTitle : null,
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

            #region 存在表單資料
            try
            {
                var ExistForm = formdb.HomeObject.Where(x => x.FormID == FormID).FirstOrDefault();
                if (ExistForm != null)
                {
                    //取得物件地址DDL
                    var addressdata = formdb.Region.Where(x => x.City == ExistForm.city && x.District == ExistForm.district && x.Road == ExistForm.road).FirstOrDefault();
                    var CityCode = addressdata.CityCode;
                    var DistrictCode = addressdata.DistrictCode;
                    var Address = addressdata.RoadCode;
                    var districtlist = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(CityCode).ddllist.ToList());
                    var roadlist = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(DistrictCode).ddllist.ToList());

                    var FormValue = new HomeObjectViewModel
                    {
                        objecttype = ExistForm.objecttype,
                        notarization = ExistForm.notarization,
                        signdateStr = Convert.ToDateTime(ExistForm.signdate).ToString("yyyy-MM-dd"),
                        appraiser = ExistForm.appraiser,
                        feature = ExistForm.feature != null ? ExistForm.feature : "",
                        citycode = CityCode,
                        districtcode = DistrictCode,
                        distirctJsonDDL = districtlist,     //鄉鎮市區地址
                        roadcode = Address,
                        roadJsonDDL = roadlist,             //道路地址
                        AddressDetail = ExistForm.detailaddress,
                        usefor = ExistForm.usefor,
                        useforelse = String.IsNullOrWhiteSpace(ExistForm.useforelse) ? "" : ExistForm.useforelse,
                        rent = ExistForm.rent,
                        deposit = ExistForm.deposit,
                        management_fee = ExistForm.management_fee,
                        startdateStr = Convert.ToDateTime(ExistForm.startdate).ToString("yyyy-MM-dd"),
                        enddateStr = Convert.ToDateTime(ExistForm.enddate).ToString("yyyy-MM-dd"),
                        paydate = ExistForm.paydate,
                        buildtype = ExistForm.buildtype,
                        roomtype = ExistForm.roomtype,
                        roomamount = ExistForm.roomamount,
                        //roomamountlist = JsonConvert.DeserializeObject<List<int>>(ExistForm.roomamount),
                        havepark = ExistForm.havepark,
                        //haveparklist = JsonConvert.DeserializeObject<List<int>>(ExistForm.havepark),
                        parktype = ExistForm.parktype,
                        parkfloor = ExistForm.parkfloor,
                        //parkfloorlist = JsonConvert.DeserializeObject<List<int>>(ExistForm.parkfloor),
                        carpositionnumber = String.IsNullOrEmpty(ExistForm.carpositionnumber) ? "" : ExistForm.carpositionnumber,   //汽車位編號
                        carmonthrent = ExistForm.carmonthrent,
                        carparkmanagefee = ExistForm.carparkmanagefee,
                        scooterparkfloor = ExistForm.scooterparkfloor,
                        //scooterparkfloorlist = JsonConvert.DeserializeObject<List<int>>(ExistForm.scooterparkfloor),
                        scooterpositionnumber = String.IsNullOrEmpty(ExistForm.scooterpositionnumber) ? "" : ExistForm.scooterpositionnumber,   //機車位編號
                        scootermonthrent = ExistForm.scootermonthrent,
                        scootermanagefee = ExistForm.scootermanagefee,
                        Accessory = ExistForm.Accessory,    //房屋附屬物件

                        Memo = String.IsNullOrEmpty(ExistForm.Memo) ? "" : ExistForm.Memo.Replace("\r\n", "\\r\\n"),
                    };


                    var model = new FormViewModels
                    {
                        FormID = FormID != null ? FormID : "",
                        homeobjectviewmodel = FormValue,

                    };

                    //Type type = FormValue.GetType();
                    //PropertyInfo[] properties = type.GetProperties();

                    //foreach (PropertyInfo property in properties)
                    //{
                    //    string propertyName = property.Name;
                    //    object propertyValue = property.GetValue(FormValue);

                    //    string logMessage = $"{propertyName}: {propertyValue}";
                    //    logger.Error(logMessage);
                    //}

                    //logger.Error("取得房屋物件資料成功" + FormValue + "\n" + FormValue.ToString());
                    return View(model);
                }
            }
            catch(Exception ex)
            {
                logger.Error("取得房屋物件資料錯誤 : " + ex.ToString());
            }
            #endregion

            var model0 = InitialModel();


            logger.Info("返還初始Model");
            return View(model0);
        }

        //[Insert]新增物件
        [HttpPost]
        #region 過時參數
        ///改由ViewModel
        //public ActionResult HomeObject(
        //    string FormID,

        //    string objecttypeRadio,     //(radio)包租:1; 代管:0
        //    string notarizationRadio,   //(radio)公證:1; 非公證:0
        //    string signdate,            //(datetime)簽約日
        //    string appraiserRadio,      //(radio)簽估價師:1; 非簽估價師:0
        //    string feature,             //(text)特色
        //    string selectroad,         //(ddl)物件地址
        //    string AddressDetail,       //(text)地址細節
        //    string useforRadio,         //(radio)主要用途 住家用:0; 商業用:1; 辦公室:2; 一般事務所:3; 其他:4
        //    string useforelse,          //(text)主要用途 其他
        //    IEnumerable<HttpPostedFileBase> taxFile, //(file)上傳稅單
        //    string rent,                //(number)租金
        //    string deposit,             //(number)押金
        //    string management_fee,      //(number)管理費
        //    string startdate,           //(datetime)起租日
        //    string enddate,             //(datetime)結束日
        //    int paydate,                //(ddl)繳租日
        //    string buildtypeRadio,      //(radio)建物型態 透天厝:0; 公寓:1; 華夏:2; 電梯大樓:3
        //    string roomtypeRadio,       //(radio)房型 整層出租:0; 獨立套房:1;
        //    string roomamount,          //(text)房數
        //    string hallamount,          //(text)廳數
        //    string bathamount,          //(text)衛數
        //    bool? noparkcheck,         //(checkbox)車位 無車位:0
        //    bool? carparkcheck,        //(checkbox)車位 汽車車位:0
        //    bool? morparkcheck,        //(checkbox)車位 機車車位:0
        //    string parktypeRadio,       //(radio)汽車車位樣式 坡道平面:0; 坡道機械:1; 機械平面:2; 機械機械:3
        //    string carparkfloorRadio,   //(radio)汽車位於 地上:1; 地下:0
        //    string parkfloornumber,     //(number)汽車位於幾樓
        //    string carpositionnumber,   //(text)汽車位編號
        //    string carmonthrent,        //(text)汽車月租金
        //    string carparkmanagefee,   //(number)汽車管理費
        //    string scooterparkfloorRadio,       //(radio)機車位於 地上:1; 地下:0
        //    string scooterparkfloornumber,     //(number)機車位於幾樓
        //    string morpositionnumber,   //(text)機車位編號
        //    string scootermonthrent,    //(number)機車月租金
        //    string scootermanagefee,//(number)機車管理費

        //    string JsonHomeObjectAccessory, //房屋附屬家具
        //    string memo                 //備註
        //    )
        #endregion
        public ActionResult HomeObject(HomeObjectInputViewModel inputmodel)
        {
            #region 使用者資料
            var EmployeeData = (from db in memberdb.Members.Where(x => x.Account == User.Identity.Name)
                                join empdb in memberdb.EmployeeData on db.Account equals empdb.Account into temp
                                from empdb0 in temp.DefaultIfEmpty()
                                join deptdb in memberdb.Department on empdb0.DivCode equals deptdb.DivCode into temp2
                                from deptdb0 in temp2.DefaultIfEmpty()
                                select new MembersViewModel
                                {
                                    Name = db.Name,
                                    Department = empdb0 != null ? deptdb0.DivFullName : null,
                                    Position = empdb0 != null ? empdb0.JobTitle : null,
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
            roomamountArray[0] = Convert.ToInt32(inputmodel.roomamount);
            roomamountArray[1] = Convert.ToInt32(inputmodel.hallamount);
            roomamountArray[2] = Convert.ToInt32(inputmodel.bathamount);
            string jsonroomamountArray = JsonConvert.SerializeObject(roomamountArray);

            int[] haveparkArray = new int[3];
            haveparkArray[0] = inputmodel.noparkcheck != null ? 1 : 0;
            haveparkArray[1] = inputmodel.carparkcheck != null ? 1 : 0;
            haveparkArray[2] = inputmodel.morparkcheck != null ? 1 : 0;
            string jsonhaveparkArray = JsonConvert.SerializeObject(haveparkArray);

            int[] parkfloorArray = new int[2];
            parkfloorArray[0] = Convert.ToInt32(inputmodel.carparkfloorRadio);
            parkfloorArray[1] = Convert.ToInt32(inputmodel.parkfloornumber);
            string jsonparkfloorArray = JsonConvert.SerializeObject(parkfloorArray);


            int[] scooterparkfloorArray = new int[2];
            scooterparkfloorArray[0] = Convert.ToInt32(inputmodel.scooterparkfloorRadio);
            scooterparkfloorArray[1] = Convert.ToInt32(inputmodel.scooterparkfloornumber);
            string jsonscooterparkfloorArray = JsonConvert.SerializeObject(scooterparkfloorArray);

            //民國 -> 西元
            string[] signdateparts = inputmodel.signdate.Split('-');
            string[] startdateparts = inputmodel.startdate.Split('-');
            string[] enddateparts = inputmodel.enddate.Split('-');
            var signdateYear = Convert.ToInt32(signdateparts[0]) + 1911;
            var startdateYear = Convert.ToInt32(startdateparts[0]) + 1911;
            var enddateYear = Convert.ToInt32(enddateparts[0]) + 1911;

            var Signdate = Convert.ToDateTime(signdateYear + "-" + signdateparts[1] + "-" + signdateparts[2]);
            var Startdate = Convert.ToDateTime(signdateYear + "-" + startdateparts[1] + "-" + startdateparts[2]);
            var Enddate = Convert.ToDateTime(signdateYear + "-" + enddateparts[1] + "-" + enddateparts[2]);


            #region 存檔
            //取得檔名與檔案GUID
            List<string> fileNamesArray = new List<string>();
            List<string> taxfile_aliasArray = new List<string>();
            //存檔
            if (inputmodel.taxFile != null && inputmodel.taxFile.Any())
            {
                try
                {
                    foreach (var file in inputmodel.taxFile)
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
                    MailService mailService = new MailService();
                    mailService.SendMail("【力群管理系統】房客資料寫入DB錯誤", ex.ToString(), "cys.enki@gmail.com");
                    return View();
                }
            }
            string fileNames = JsonConvert.SerializeObject(fileNamesArray);
            string taxfile_alias = JsonConvert.SerializeObject(taxfile_aliasArray);
            #endregion


            try
            {
                //找到地址
                var address = formdb.Region.Where(x => x.RoadCode == inputmodel.selectroad).FirstOrDefault();
                if(String.IsNullOrEmpty(inputmodel.FormID)) {
                    // 建立資料上下文（Data Context）
                    using (var context = new FormModels())
                    {
                        // 建立要插入的資料物件
                        var newData = new HomeObject
                        {
                            FormID = newFormID,
                            objecttype = Convert.ToInt32(inputmodel.objecttypeRadio),
                            notarization = Convert.ToInt32(inputmodel.notarizationRadio),
                            signdate = Signdate,
                            appraiser = Convert.ToInt32(inputmodel.appraiserRadio),
                            feature = inputmodel.feature,
                            city = address.City,
                            district = address.District,
                            road = address.Road,
                            detailaddress = inputmodel.AddressDetail,
                            fulladdress = address.City + address.District + address.Road + inputmodel.AddressDetail,
                            usefor = Convert.ToInt32(inputmodel.useforRadio),
                            useforelse = inputmodel.useforelse,
                            taxfile_name = fileNames,
                            taxfile_alias = taxfile_alias,
                            rent = Convert.ToInt32(inputmodel.rent),
                            deposit = Convert.ToInt32(inputmodel.deposit),
                            management_fee = Convert.ToInt32(inputmodel.management_fee),
                            startdate = Startdate,
                            enddate = Enddate,
                            paydate = inputmodel.paydate,
                            buildtype = Convert.ToInt32(inputmodel.buildtypeRadio),
                            roomtype = Convert.ToInt32(inputmodel.roomtypeRadio),
                            roomamount = jsonroomamountArray,
                            havepark = jsonhaveparkArray,
                            parktype = Convert.ToInt32(inputmodel.parktypeRadio),
                            parkfloor = jsonparkfloorArray,
                            carpositionnumber = inputmodel.carpositionnumber,
                            carmonthrent = String.IsNullOrEmpty(inputmodel.carmonthrent) ? 0 : Convert.ToInt32(inputmodel.carmonthrent),
                            carparkmanagefee = String.IsNullOrEmpty(inputmodel.carparkmanagefee) ? 0 : Convert.ToInt32(inputmodel.carparkmanagefee),
                            scooterparkfloor = jsonscooterparkfloorArray,
                            scooterpositionnumber = inputmodel.morpositionnumber,
                            scootermonthrent = String.IsNullOrEmpty(inputmodel.scootermonthrent) ? 0 : Convert.ToInt32(inputmodel.scootermonthrent),
                            scootermanagefee = String.IsNullOrEmpty(inputmodel.scootermanagefee) ? 0 : Convert.ToInt32(inputmodel.scootermanagefee),
                            Accessory = inputmodel.JsonHomeObjectAccessory,

                            CreateUser = userid,
                            CreateTime = now,
                            UpdateUser = userid,
                            UpdateTime = now,
                            Memo = inputmodel.memo,
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
                else
                {
                    // 建立資料上下文（Data Context）
                    using (var context = new FormModels())
                    {
                        var existformdata = context.HomeObject.Where(x => x.FormID == inputmodel.FormID).FirstOrDefault();

                        //更新物件內資料
                        existformdata.objecttype = Convert.ToInt32(inputmodel.objecttypeRadio);
                        existformdata.notarization = Convert.ToInt32(inputmodel.notarizationRadio);
                        existformdata.signdate = Signdate;
                        existformdata.appraiser = Convert.ToInt32(inputmodel.appraiserRadio);
                        existformdata.feature = inputmodel.feature;
                        existformdata.city = address.City;
                        existformdata.district = address.District;
                        existformdata.road = address.Road;
                        existformdata.detailaddress = inputmodel.AddressDetail;
                        existformdata.fulladdress = address.City + address.District + address.Road + inputmodel.AddressDetail;
                        existformdata.usefor = Convert.ToInt32(inputmodel.useforRadio);
                        existformdata.useforelse = inputmodel.useforelse;
                        existformdata.taxfile_name = fileNames;
                        existformdata.taxfile_alias = taxfile_alias;
                        existformdata.rent = Convert.ToInt32(inputmodel.rent);
                        existformdata.deposit = Convert.ToInt32(inputmodel.deposit);
                        existformdata.management_fee = Convert.ToInt32(inputmodel.management_fee);
                        existformdata.startdate = Startdate;
                        existformdata.enddate = Enddate;
                        existformdata.paydate = inputmodel.paydate;
                        existformdata.buildtype = Convert.ToInt32(inputmodel.buildtypeRadio);
                        existformdata.roomtype = Convert.ToInt32(inputmodel.roomtypeRadio);
                        existformdata.roomamount = jsonroomamountArray;
                        existformdata.havepark = jsonhaveparkArray;
                        existformdata.parktype = Convert.ToInt32(inputmodel.parktypeRadio);
                        existformdata.parkfloor = jsonparkfloorArray;
                        existformdata.carpositionnumber = inputmodel.carpositionnumber;
                        existformdata.carmonthrent = String.IsNullOrEmpty(inputmodel.carmonthrent) ? 0 : Convert.ToInt32(inputmodel.carmonthrent);
                        existformdata.carparkmanagefee = String.IsNullOrEmpty(inputmodel.carparkmanagefee) ? 0 : Convert.ToInt32(inputmodel.carparkmanagefee);
                        existformdata.scooterparkfloor = jsonscooterparkfloorArray;
                        existformdata.scooterpositionnumber = inputmodel.morpositionnumber;
                        existformdata.scootermonthrent = String.IsNullOrEmpty(inputmodel.scootermonthrent) ? 0 : Convert.ToInt32(inputmodel.scootermonthrent);
                        existformdata.scootermanagefee = String.IsNullOrEmpty(inputmodel.scootermanagefee) ? 0 : Convert.ToInt32(inputmodel.scootermanagefee);
                        existformdata.Accessory = inputmodel.JsonHomeObjectAccessory;

                        existformdata.UpdateUser = userid;
                        existformdata.UpdateTime = now;
                        existformdata.Memo = inputmodel.memo;
                        // 儲存更改到資料庫
                        context.SaveChanges();
                    }
                }

            }
            catch (Exception ex)
            {
                MailService mailService = new MailService();
                mailService.SendMail("【力群管理系統】房客資料寫入DB錯誤", ex.ToString(), "cys.enki@gmail.com");
                return View();
            }
            return RedirectToAction("CaseManage", "Sales");
        }
        #endregion

        #region 房東資料
        [HttpGet]
        public ActionResult Landlord(string FormID)
        {
            logger.Info("進入房東資料 FormID : " + (FormID != null ? FormID : ""));
            #region 使用者資料
            var EmployeeData = (from db in memberdb.Members.Where(x => x.Account == User.Identity.Name)
                                join empdb in memberdb.EmployeeData on db.Account equals empdb.Account into temp
                                from empdb0 in temp.DefaultIfEmpty()
                                join deptdb in memberdb.Department on empdb0.DivCode equals deptdb.DivCode into temp2
                                from deptdb0 in temp2.DefaultIfEmpty()
                                select new MembersViewModel
                                {
                                    Name = db.Name,
                                    Department = empdb0 != null ? deptdb0.DivFullName : null,
                                    Position = empdb0 != null ? empdb0.JobTitle : null,
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
            //if(Form != null)
            //{
            //    return RedirectToAction("CaseManage", "Sales");
            //}

            #region 存在表單資料
            try
            {
                var ExistForm = formdb.LandLord.Where(x => x.FormID == FormID).FirstOrDefault();
                if (ExistForm != null)
                {
                    #region 取得房東資料(地址)
                    //取得房東戶籍地址
                    string[] Addressparts = ExistForm.Address.Split('-');    //地址
                    var AddressSplit_Landlord = new { City = Addressparts[0].ToString(), District = Addressparts[1].ToString(), Road = Addressparts[2].ToString(), };
                    var addressCode_Landlord = formdb.Region.Where(x => x.City == AddressSplit_Landlord.City && x.District == AddressSplit_Landlord.District && x.Road == AddressSplit_Landlord.Road).Select(x => x.RoadCode).FirstOrDefault();
                    //取得房東通訊地址
                    string[] Contactparts = ExistForm.ContactAddress.Split('-');    //通訊地址
                    var ContactSplit_Landlord = new { City = Contactparts[0].ToString(), District = Contactparts[1].ToString(), Road = Contactparts[2].ToString(), };
                    var contactCode_Landlord = formdb.Region.Where(x => x.City == ContactSplit_Landlord.City && x.District == ContactSplit_Landlord.District && x.Road == ContactSplit_Landlord.Road).Select(x => x.RoadCode).FirstOrDefault();

                    //取得房東地址陣列下拉選單
                    string[] Landlord_Address_Input = new string[4];
                    Landlord_Address_Input[0] = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(addressCode_Landlord.Substring(0, 2)).ddllist.ToList());
                    Landlord_Address_Input[1] = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(addressCode_Landlord.Substring(0, 4)).ddllist.ToList());
                    Landlord_Address_Input[2] = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(contactCode_Landlord.Substring(0, 2)).ddllist.ToList());
                    Landlord_Address_Input[3] = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(contactCode_Landlord.Substring(0, 4)).ddllist.ToList());

                    #endregion

                    #region 取得共有人資料(地址)
                    //取得共有人人數
                    var CoOwnerCount = Convert.ToInt32(JsonConvert.DeserializeObject<List<string>>(ExistForm.MemberArray)[0]);
                    string[] coownerarray1 = new string[8];
                    string[] coownerarray2 = new string[8];
                    string[] coownerarray3 = new string[8];
                    string[] coownerarray4 = new string[8];
                    string[] coownerarray5 = new string[8];
                    string[][] coownerArrays = new string[][] { coownerarray1, coownerarray2, coownerarray3, coownerarray4, coownerarray5 };    //輸出

                    //取得共有人戶籍地址DDL[district, road, districtContact, roadContact]
                    string[] CoOwner_Address_Input1 = new string[4];
                    string[] CoOwner_Address_Input2 = new string[4];
                    string[] CoOwner_Address_Input3 = new string[4];
                    string[] CoOwner_Address_Input4 = new string[4];
                    string[] CoOwner_Address_Input5 = new string[4];
                    string[][] CoOwner_AddressArraays = new string[][] { CoOwner_Address_Input1, CoOwner_Address_Input2, CoOwner_Address_Input3, CoOwner_Address_Input4, CoOwner_Address_Input5 };    //輸出
                    int arraycount = 0;


                    for (int count = 0; count < CoOwnerCount; count++)
                    {
                        //找到要取得的共有人Json陣列資料
                        string coOwnerProperty = $"CoOwner{count + 1}";
                        coownerArrays[count] = JsonConvert.DeserializeObject<List<string>>(ExistForm.GetType().GetProperty(coOwnerProperty).GetValue(ExistForm).ToString()).ToArray();

                        //將地址轉換成地址代碼
                        for (int arrayscount = 0; arrayscount < coownerArrays[count].Count(); arrayscount++)
                        {
                            //4:地址 // 6:通訊地址
                            if (arrayscount == 4 || arrayscount == 6)
                            {
                                string[] Parts = coownerArrays[count][arrayscount].Split('-');    //通訊地址
                                //門牌地址 => 地址代碼
                                var Split = new { City = Parts[0].ToString(), District = Parts[1].ToString(), Road = Parts[2].ToString(), };
                                var co_addresscode = formdb.Region.Where(x => x.City == Split.City && x.District == Split.District && x.Road == Split.Road).Select(x => x.RoadCode).FirstOrDefault();
                                coownerArrays[count][arrayscount] = co_addresscode;

                                //找到DDL並存入
                                if (arrayscount == 4)
                                {
                                    var splitaddress = new { City = Parts[0].ToString(), District = Parts[1].ToString(), Road = Parts[2].ToString(), };
                                    CoOwner_AddressArraays[arraycount][0] = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(co_addresscode.Substring(0, 2)).ddllist.ToList());
                                    CoOwner_AddressArraays[arraycount][1] = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(co_addresscode.Substring(0, 4)).ddllist.ToList());
                                }
                                if (arrayscount == 6)
                                {
                                    CoOwner_AddressArraays[arraycount][2] = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(co_addresscode.Substring(0, 2)).ddllist.ToList());
                                    CoOwner_AddressArraays[arraycount][3] = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(co_addresscode.Substring(0, 4)).ddllist.ToList());
                                    arraycount++;
                                }
                            }
                            else
                            {
                                coownerArrays[count][arrayscount] = coownerArrays[count][arrayscount];
                            }
                        }
                    }
                    #endregion

                    #region 取得代理人資料(地址)
                    //取得代理人人數
                    var AgentCount = Convert.ToInt32(JsonConvert.DeserializeObject<List<string>>(ExistForm.MemberArray)[1]);
                    //取得代理人陣列
                    string[] agentarray = new string[9];
                    //取得代理人地址陣列
                    string[] Agent_Address_Input = new string[4];
                    if (ExistForm.Agent != null)
                    {
                        if (AgentCount > 0)
                        {
                            agentarray = JsonConvert.DeserializeObject<List<string>>(ExistForm.Agent).ToArray();

                            //將地址轉換成地址代碼
                            for (int arrayscount = 0; arrayscount < agentarray.Count(); arrayscount++)
                            {

                                //4:地址 // 6:通訊地址
                                if (arrayscount == 5 || arrayscount == 7)
                                {
                                    string[] Parts = agentarray[arrayscount].Split('-');    //通訊地址
                                                                                            //門牌地址 => 地址代碼
                                    var Split = new { City = Parts[0].ToString(), District = Parts[1].ToString(), Road = Parts[2].ToString(), };
                                    var ag_addresscode = formdb.Region.Where(x => x.City == Split.City && x.District == Split.District && x.Road == Split.Road).Select(x => x.RoadCode).FirstOrDefault();
                                    agentarray[arrayscount] = ag_addresscode;

                                    //找到DDL並存入
                                    if (arrayscount == 5)
                                    {
                                        var splitaddress = new { City = Parts[0].ToString(), District = Parts[1].ToString(), Road = Parts[2].ToString(), };
                                        Agent_Address_Input[0] = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(ag_addresscode.Substring(0, 2)).ddllist.ToList());
                                        Agent_Address_Input[1] = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(ag_addresscode.Substring(0, 4)).ddllist.ToList());
                                    }
                                    if (arrayscount == 7)
                                    {
                                        Agent_Address_Input[2] = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(ag_addresscode.Substring(0, 2)).ddllist.ToList());
                                        Agent_Address_Input[3] = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(ag_addresscode.Substring(0, 4)).ddllist.ToList());
                                        arraycount++;
                                    }
                                }
                                else
                                {
                                    agentarray[arrayscount] = agentarray[arrayscount];
                                }
                            }
                        }
                    }
                    #endregion

                    var FormValue = new LandLordViewModel
                    {
                        FormID = FormID,
                        Name = ExistForm.Name,      //房東姓名(公司名稱)
                        Principal = ExistForm.Gender != 2 ? "" : ExistForm.Principal,       //負責人
                        Gender = ExistForm.Gender,      //性別(0/1)，法人(2)
                        BirthdayStr = ExistForm.Gender != 2 ? Convert.ToDateTime(ExistForm.Birthday).ToString("yyyy-MM-dd") : null,     //房東生日
                        IDNumber = ExistForm.IDNumber,      //身分證字號(統一編號)
                        Phone = ExistForm.Phone,        //電話
                        BankNo = ExistForm.BankNo,      //銀行代碼
                        BrancheNo = ExistForm.BrancheNo,        //分行代碼
                        BankAccount = ExistForm.BankAccount,        //銀行帳號
                        AddressCode_LandLord = addressCode_Landlord,    //地址代碼
                        AddressDetail = ExistForm.AddressDetail,
                        ContactCode_Landlord = contactCode_Landlord,    //通訊地址代碼
                        ContactAddressDetail = ExistForm.ContactAddressDetail,
                        CoOwnerCount = CoOwnerCount,
                        CoOwner1 = JsonConvert.SerializeObject(coownerArrays[0]),
                        CoOwner2 = JsonConvert.SerializeObject(coownerArrays[1]),
                        CoOwner3 = JsonConvert.SerializeObject(coownerArrays[2]),
                        CoOwner4 = JsonConvert.SerializeObject(coownerArrays[3]),
                        CoOwner5 = JsonConvert.SerializeObject(coownerArrays[4]),
                        AgentCount = AgentCount,
                        Agent = JsonConvert.SerializeObject(agentarray),
                        Memo = String.IsNullOrEmpty(ExistForm.Memo) ? "" : ExistForm.Memo.Replace("\r\n", "\\r\\n"),

                        //下拉選單
                        LaAddress_District = Landlord_Address_Input[0],
                        LaAddress_Road = Landlord_Address_Input[1],
                        LaContact_District = Landlord_Address_Input[2],
                        LaContact_Road = Landlord_Address_Input[3],

                        CoAddress1_District = CoOwner_AddressArraays[0][0],
                        CoAddress1_Road = CoOwner_AddressArraays[0][1],
                        CoContact1_District = CoOwner_AddressArraays[0][2],
                        CoContact1_Road = CoOwner_AddressArraays[0][3],

                        CoAddress2_District = CoOwner_AddressArraays[1][0],
                        CoAddress2_Road = CoOwner_AddressArraays[1][1],
                        CoContact2_District = CoOwner_AddressArraays[1][2],
                        CoContact2_Road = CoOwner_AddressArraays[1][3],

                        CoAddress3_District = CoOwner_AddressArraays[2][0],
                        CoAddress3_Road = CoOwner_AddressArraays[2][1],
                        CoContact3_District = CoOwner_AddressArraays[2][2],
                        CoContact3_Road = CoOwner_AddressArraays[2][3],

                        CoAddress4_District = CoOwner_AddressArraays[3][0],
                        CoAddress4_Road = CoOwner_AddressArraays[3][1],
                        CoContact4_District = CoOwner_AddressArraays[3][2],
                        CoContact4_Road = CoOwner_AddressArraays[3][3],

                        CoAddress5_District = CoOwner_AddressArraays[4][0],
                        CoAddress5_Road = CoOwner_AddressArraays[4][1],
                        CoContact5_District = CoOwner_AddressArraays[4][2],
                        CoContact5_Road = CoOwner_AddressArraays[4][3],

                        AgAddress_District = Agent_Address_Input[0],
                        AgAddress_Road = Agent_Address_Input[1],
                        AgContact_District = Agent_Address_Input[2],
                        AgContact_Road = Agent_Address_Input[3],
                    };

                    var model = new FormViewModels
                    {
                        FormID = FormID != null ? FormID : "",
                        landlordviewmodel = FormValue,
                    };
                    #region Nlog
                    //取得目前model資料
                    //Type type = FormValue.GetType();
                    //PropertyInfo[] properties = type.GetProperties();

                    //foreach (PropertyInfo property in properties)
                    //{
                    //    string propertyName = property.Name;
                    //    object propertyValue = property.GetValue(FormValue);

                    //    string logMessage = $"{propertyName}: {propertyValue}";
                    //    logger.Error(logMessage);
                    //}
                    #endregion
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                logger.Error("取得房屋物件資料錯誤 : " + ex.ToString());
            }
            #endregion

            var initmodel = InitialModel();


            return View(initmodel);
        }
        [HttpPost]
        #region 過時
        //public ActionResult Landlord(
        //    string FormID,                  //表單編號

        //    string persontypeRadio,         //自然人:0; 法人:1
        //    string Name_0,                  //房東姓名
        //    string Principal,               //負責人姓名(法人才需要填寫)
        //    string genderRadio_0,           //房東性別(女:0; 男:1; 法人:2)
        //    string birthday_0,              //房東生日
        //    string IDNumber_0,              //房東身分證
        //    string Phone_0,                 //房東電話
        //    string addressroad_0,           //房東地址(路)
        //    string AddressDetail_0,         //房東詳細地址  
        //    bool? sameaddress_check_0,       //房東通訊地址Checkbox
        //    string contactroad_0,           //房東通訊地址(路)
        //    string detailcontact_0,         //房東詳細通訊地址
        //    string bank_0,                  //房東銀行
        //    string bankbranche_0,           //房東銀行支部
        //    string bankaccount_0,           //房東銀行帳號

        //    //共有人(Json格式)
        //    string CoOwnerRadio,
        //    string CoOwnerInput1,
        //    string CoOwnerInput2,
        //    string CoOwnerInput3,
        //    string CoOwnerInput4,
        //    string CoOwnerInput5,

        //    //代理人(Json格式)
        //    bool? agentCheckbox,
        //    string AgentInput,

        //    string memo
        //    )
        #endregion
        public ActionResult Landlord(LandlordInputViewModel inputmodel)
        {
            #region 使用者資料
            var EmployeeData = (from db in memberdb.Members.Where(x => x.Account == User.Identity.Name)
                                join empdb in memberdb.EmployeeData on db.Account equals empdb.Account into temp
                                from empdb0 in temp.DefaultIfEmpty()
                                join deptdb in memberdb.Department on empdb0.DivCode equals deptdb.DivCode into temp2
                                from deptdb0 in temp2.DefaultIfEmpty()
                                select new MembersViewModel
                                {
                                    Name = db.Name,
                                    Department = empdb0 != null ? deptdb0.DivFullName : null,
                                    Position = empdb0 != null ? empdb0.JobTitle : null,
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
            if (String.IsNullOrEmpty(inputmodel.FormID))
            {
                return View();
            }

            //轉換地址
            var addressdata = formdb.Region.Where(x => x.RoadCode == inputmodel.addressroad_0).FirstOrDefault();
            var Address = addressdata.City + "-" + addressdata.District + "-" + addressdata.Road;
            var ContactAddress = "";
            if (inputmodel.sameaddress_check_0 != null)
            {
                inputmodel.contactroad_0 = Address;
                inputmodel.detailcontact_0 = inputmodel.AddressDetail_0;
            }
            else
            {
                addressdata = formdb.Region.Where(x => x.RoadCode == inputmodel.contactroad_0).FirstOrDefault();
                ContactAddress = addressdata.City + "-" + addressdata.District + "-" + addressdata.Road;
            }

            //成員陣列[共有人, 代理人]
            int[] memberArray = new int[2];
            int CoOwnerCount = Convert.ToInt32(inputmodel.CoOwnerRadio);
            memberArray[0] = CoOwnerCount;
            memberArray[1] = inputmodel.agentCheckbox != null ? 1: 0;
            string jsonmemberArray = JsonConvert.SerializeObject(memberArray);

            #region 打包共有人資料
            string[] coownerarray1 = new string[8];
            string[] coownerarray2 = new string[8];
            string[] coownerarray3 = new string[8];
            string[] coownerarray4 = new string[8];
            string[] coownerarray5 = new string[8];

            //創建複數陣列存取資料型態
            string[] coownerInputs = new string[] { inputmodel.CoOwnerInput1, inputmodel.CoOwnerInput2, inputmodel.CoOwnerInput3, inputmodel.CoOwnerInput4, inputmodel.CoOwnerInput5 };    //輸入
            string[][] coownerArrays = new string[][] { coownerarray1, coownerarray2, coownerarray3, coownerarray4, coownerarray5 };    //輸出

            for (int count = 0; count < CoOwnerCount; count++)
            {
                //找到要取得的共有人Json陣列資料
                List<string> coownerlist = new List<string>();
                coownerlist = JsonConvert.DeserializeObject<List<string>>(coownerInputs[count]);
                //遞迴取出資料
                for(int i=0; i < coownerlist.Count(); i++) 
                //foreach(var item in coownerlist)
                {
                    if(i == 4 || i == 6)
                    {
                        //由Address找到地址資料
                        var roadcode = coownerlist[i].ToString();
                        addressdata = formdb.Region.Where(x => x.RoadCode == roadcode).FirstOrDefault();
                        coownerArrays[count][i] = addressdata.City + "-" + addressdata.District + "-" + addressdata.Road;
                    }
                    else
                    {
                        coownerArrays[count][i] = coownerlist[i];
                    }
                }
            }





            #endregion

            #region 打包代理人資料
            string[] agentarray = new string[9];
            if (inputmodel.agentCheckbox != null)
            {
                //找到要取得的共有人Json陣列資料
                List<string> agentlist = new List<string>();
                agentlist = JsonConvert.DeserializeObject<List<string>>(inputmodel.AgentInput);
                //遞迴取出資料
                for (int i = 0; i < agentlist.Count(); i++)
                //foreach(var item in coownerlist)
                {
                    if (i == 5 || i == 7)
                    {
                        //由Address找到地址資料
                        var roadcode = agentlist[i].ToString();
                        addressdata = formdb.Region.Where(x => x.RoadCode == roadcode).FirstOrDefault();
                        agentarray[i] = addressdata.City + "-" + addressdata.District + "-" + addressdata.Road;
                    }
                    else
                    {
                        agentarray[i] = agentlist[i];
                    }
                }

            }

            #endregion
            DateTime? LandlordBirthday = null;
            if (inputmodel.persontypeRadio != "1")
            {
                LandlordBirthday = Convert.ToDateTime(inputmodel.birthday_0);
            }
            try
            {
                var exist = formdb.LandLord.Where(x => x.FormID == inputmodel.FormID).FirstOrDefault();
                if (exist == null)
                {
                    //新增
                    // 建立資料上下文（Data Context）
                    using (var context = new FormModels())
                    {
                        // 建立要插入的資料物件
                        var newData = new LandLord
                        {
                            FormID = inputmodel.FormID,
                            Name = inputmodel.Name_0,
                            Principal = inputmodel.persontypeRadio == "1" ? inputmodel.Principal : null,
                            Gender = inputmodel.persontypeRadio == "1" ? 2 : Convert.ToInt32(inputmodel.genderRadio_0),   //男:1 女:0 法人:2
                            Birthday = LandlordBirthday,
                            IDNumber = inputmodel.IDNumber_0,
                            Phone = inputmodel.Phone_0,
                            BankNo = inputmodel.bank_0,
                            BrancheNo = inputmodel.bankbranche_0,
                            BankAccount = inputmodel.bankaccount_0,
                            Address = Address,
                            AddressDetail = inputmodel.AddressDetail_0,
                            ContactAddress = ContactAddress,
                            ContactAddressDetail = inputmodel.detailcontact_0,
                            MemberArray = jsonmemberArray,
                            CoOwner1 = CoOwnerCount >= 1 ? JsonConvert.SerializeObject(coownerArrays[0]) : null,
                            CoOwner2 = CoOwnerCount >= 2 ? JsonConvert.SerializeObject(coownerArrays[1]) : null,
                            CoOwner3 = CoOwnerCount >= 3 ? JsonConvert.SerializeObject(coownerArrays[2]) : null,
                            CoOwner4 = CoOwnerCount >= 4 ? JsonConvert.SerializeObject(coownerArrays[3]) : null,
                            CoOwner5 = CoOwnerCount >= 5 ? JsonConvert.SerializeObject(coownerArrays[4]) : null,
                            Agent = inputmodel.agentCheckbox != null ? JsonConvert.SerializeObject(agentarray) : null,

                            CreateUser = userid,
                            CreateTime = now,
                            UpdateUser = userid,
                            UpdateTime = now,
                            Memo = inputmodel.memo,
                        };
                        // 使用資料上下文插入資料物件
                        context.LandLord.Add(newData);
                        // 儲存更改到資料庫
                        context.SaveChanges();
                    }
                }
                else
                {
                    //編輯
                    // 建立資料上下文（Data Context）
                    using (var context = new FormModels())
                    {
                        var existformdata = context.LandLord.Where(x => x.FormID == inputmodel.FormID).FirstOrDefault();

                        //更新物件內資料
                        existformdata.Name = inputmodel.Name_0;
                        existformdata.Principal = inputmodel.persontypeRadio == "1" ? inputmodel.Principal : null;
                        existformdata.Gender = inputmodel.persontypeRadio == "1" ? 2 : Convert.ToInt32(inputmodel.genderRadio_0);   //男:1 女:0 法人:2
                        existformdata.Birthday = LandlordBirthday;
                        existformdata.IDNumber = inputmodel.IDNumber_0;
                        existformdata.Phone = inputmodel.Phone_0;
                        existformdata.BankNo = inputmodel.bank_0;
                        existformdata.BrancheNo = inputmodel.bankbranche_0;
                        existformdata.BankAccount = inputmodel.bankaccount_0;
                        existformdata.Address = Address;
                        existformdata.AddressDetail = inputmodel.AddressDetail_0;
                        existformdata.ContactAddress = ContactAddress;
                        existformdata.ContactAddressDetail = inputmodel.detailcontact_0;
                        existformdata.MemberArray = jsonmemberArray;
                        existformdata.CoOwner1 = CoOwnerCount >= 1 ? JsonConvert.SerializeObject(coownerArrays[0]) : null;
                        existformdata.CoOwner2 = CoOwnerCount >= 2 ? JsonConvert.SerializeObject(coownerArrays[1]) : null;
                        existformdata.CoOwner3 = CoOwnerCount >= 3 ? JsonConvert.SerializeObject(coownerArrays[2]) : null;
                        existformdata.CoOwner4 = CoOwnerCount >= 4 ? JsonConvert.SerializeObject(coownerArrays[3]) : null;
                        existformdata.CoOwner5 = CoOwnerCount >= 5 ? JsonConvert.SerializeObject(coownerArrays[4]) : null;
                        existformdata.Agent = inputmodel.agentCheckbox != null ? JsonConvert.SerializeObject(agentarray) : null;

                        existformdata.UpdateUser = userid;
                        existformdata.UpdateTime = now;
                        existformdata.Memo = inputmodel.memo;
                        // 儲存更改到資料庫
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                MailService mailService = new MailService();
                mailService.SendMail("【力群管理系統】房客資料寫入DB錯誤", ex.ToString(), "cys.enki@gmail.com");
                return RedirectToAction("Landlord", "Form", new {FormID = inputmodel.FormID });
            }

            return RedirectToAction("CaseManage", "Sales");
        }
        #endregion

        #region 房客資料
        [HttpGet]
        public ActionResult Tenant(string FormID)
        {
            logger.Info("進入房客資料 FormID : " + (FormID != null ? FormID : ""));
            #region 使用者資料
            var EmployeeData = (from db in memberdb.Members.Where(x => x.Account == User.Identity.Name)
                                join empdb in memberdb.EmployeeData on db.Account equals empdb.Account into temp
                                from empdb0 in temp.DefaultIfEmpty()
                                join deptdb in memberdb.Department on empdb0.DivCode equals deptdb.DivCode into temp2
                                from deptdb0 in temp2.DefaultIfEmpty()
                                select new MembersViewModel
                                {
                                    Name = db.Name,
                                    Department = empdb0 != null ? deptdb0.DivFullName : null,
                                    Position = empdb0 != null ? empdb0.JobTitle : null,
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
            //var Form = formdb.Tenant.Where(x => x.FormID == FormID).FirstOrDefault();
            //if (Form != null)
            //{
            //    return RedirectToAction("CaseManage", "Sales");
            //}

            #region 存在表單資料
            try
            {
                var ExistForm = formdb.Tenant.Where(x => x.FormID == FormID).FirstOrDefault();
                if (ExistForm != null)
                {
                    #region 取得房客資料(地址)
                    //取得房東戶籍地址
                    string[] Addressparts = ExistForm.Address.Split('-');    //地址
                    var AddressSplit_Tenant = new { City = Addressparts[0].ToString(), District = Addressparts[1].ToString(), Road = Addressparts[2].ToString(), };
                    var addressCode_Tenant = formdb.Region.Where(x => x.City == AddressSplit_Tenant.City && x.District == AddressSplit_Tenant.District && x.Road == AddressSplit_Tenant.Road).Select(x => x.RoadCode).FirstOrDefault();
                    //取得房客通訊地址
                    string[] Contactparts = ExistForm.ContactAddress.Split('-');    //通訊地址
                    var ContactSplit_Tenant = new { City = Contactparts[0].ToString(), District = Contactparts[1].ToString(), Road = Contactparts[2].ToString(), };
                    var contactCode_Tenant = formdb.Region.Where(x => x.City == ContactSplit_Tenant.City && x.District == ContactSplit_Tenant.District && x.Road == ContactSplit_Tenant.Road).Select(x => x.RoadCode).FirstOrDefault();

                    //取得房客地址陣列下拉選單
                    string[] Tenant_Address_Input = new string[4];
                    Tenant_Address_Input[0] = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(addressCode_Tenant.Substring(0, 2)).ddllist.ToList());
                    Tenant_Address_Input[1] = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(addressCode_Tenant.Substring(0, 4)).ddllist.ToList());
                    Tenant_Address_Input[2] = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(contactCode_Tenant.Substring(0, 2)).ddllist.ToList());
                    Tenant_Address_Input[3] = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(contactCode_Tenant.Substring(0, 4)).ddllist.ToList());
                    #endregion

                    //人數陣列
                    var MemberList = JsonConvert.DeserializeObject<List<string>>(ExistForm.MemberArray);
                    var CoupleCount = Convert.ToInt32(MemberList[0]);   //配偶人數
                    var DirectCount = Convert.ToInt32(MemberList[1]);   //直系親屬人數
                    var AgentCount = Convert.ToInt32(MemberList[2]);    //代理人人數
                    var GuarantorCount = Convert.ToInt32(MemberList[3]);//保證人人數

                    #region 取得代理人資料(地址)
                    string[] agentarray1 = new string[9];
                    string[] agentarray2 = new string[9];
                    string[] agentarray3 = new string[9];
                    string[][] agentArrays = new string[][] { agentarray1, agentarray2, agentarray3 };    //輸出

                    //取得代理人戶籍地址DDL[district, road, districtContact, roadContact]
                    string[] agent_Address_Input1 = new string[4];
                    string[] agent_Address_Input2 = new string[4];
                    string[] agent_Address_Input3 = new string[4];
                    string[][] agent_AddressArraays = new string[][] { agent_Address_Input1, agent_Address_Input2, agent_Address_Input3 };    //輸出
                    if (AgentCount > 0)
                    {
                        int arraycount = 0;

                        for (int count = 0; count < AgentCount; count++)
                        {
                            //找到要取得的共有人Json陣列資料
                            string agentProperty = $"Agent{count + 1}";
                            agentArrays[count] = JsonConvert.DeserializeObject<List<string>>(ExistForm.GetType().GetProperty(agentProperty).GetValue(ExistForm).ToString()).ToArray();

                            //將地址轉換成地址代碼
                            for (int arrayscount = 0; arrayscount < agentArrays[count].Count(); arrayscount++)
                            {
                                //5:地址 // 7:通訊地址
                                if (arrayscount == 5 || arrayscount == 7)
                                {
                                    string[] Parts = agentArrays[count][arrayscount].Split('-');    //通訊地址
                                                                                                    //門牌地址 => 地址代碼
                                    var Split = new { City = Parts[0].ToString(), District = Parts[1].ToString(), Road = Parts[2].ToString(), };
                                    var co_addresscode = formdb.Region.Where(x => x.City == Split.City && x.District == Split.District && x.Road == Split.Road).Select(x => x.RoadCode).FirstOrDefault();
                                    agentArrays[count][arrayscount] = co_addresscode;

                                    //找到DDL並存入
                                    if (arrayscount == 5)
                                    {
                                        var splitaddress = new { City = Parts[0].ToString(), District = Parts[1].ToString(), Road = Parts[2].ToString(), };
                                        agent_AddressArraays[arraycount][0] = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(co_addresscode.Substring(0, 2)).ddllist.ToList());
                                        agent_AddressArraays[arraycount][1] = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(co_addresscode.Substring(0, 4)).ddllist.ToList());
                                    }
                                    if (arrayscount == 7)
                                    {
                                        agent_AddressArraays[arraycount][2] = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(co_addresscode.Substring(0, 2)).ddllist.ToList());
                                        agent_AddressArraays[arraycount][3] = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(co_addresscode.Substring(0, 4)).ddllist.ToList());
                                        arraycount++;
                                    }
                                }
                                else
                                {
                                    agentArrays[count][arrayscount] = agentArrays[count][arrayscount];
                                }
                            }
                        }
                    }
                    #endregion


                    #region 取得保證人資料(地址)
                    string[] Guentarray1 = new string[9];
                    string[] Guentarray2 = new string[9];
                    string[] Guentarray3 = new string[9];
                    string[][] GuentArrays = new string[][] { Guentarray1, Guentarray2, Guentarray3 };    //輸出

                    //取得保證人戶籍地址DDL[district, road, districtContact, roadContact]
                    string[] Guent_Address_Input1 = new string[4];
                    string[] Guent_Address_Input2 = new string[4];
                    string[] Guent_Address_Input3 = new string[4];
                    string[][] Guent_AddressArraays = new string[][] { Guent_Address_Input1, Guent_Address_Input2, Guent_Address_Input3 };    //輸出
                    if (GuarantorCount > 0)
                    {
                        int arraycount = 0;
                        for (int count = 0; count < GuarantorCount; count++)
                        {
                            //找到要取得的共有人Json陣列資料
                            string GuentProperty = $"Guarantor{count + 1}";
                            GuentArrays[count] = JsonConvert.DeserializeObject<List<string>>(ExistForm.GetType().GetProperty(GuentProperty).GetValue(ExistForm).ToString()).ToArray();

                            //將地址轉換成地址代碼
                            for (int arrayscount = 0; arrayscount < GuentArrays[count].Count(); arrayscount++)
                            {
                                //5:地址 // 7:通訊地址
                                if (arrayscount == 5 || arrayscount == 7)
                                {
                                    string[] Parts = GuentArrays[count][arrayscount].Split('-');    //通訊地址
                                                                                                    //門牌地址 => 地址代碼
                                    var Split = new { City = Parts[0].ToString(), District = Parts[1].ToString(), Road = Parts[2].ToString(), };
                                    var co_addresscode = formdb.Region.Where(x => x.City == Split.City && x.District == Split.District && x.Road == Split.Road).Select(x => x.RoadCode).FirstOrDefault();
                                    GuentArrays[count][arrayscount] = co_addresscode;

                                    //找到DDL並存入
                                    if (arrayscount == 5)
                                    {
                                        var splitaddress = new { City = Parts[0].ToString(), District = Parts[1].ToString(), Road = Parts[2].ToString(), };
                                        Guent_AddressArraays[arraycount][0] = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(co_addresscode.Substring(0, 2)).ddllist.ToList());
                                        Guent_AddressArraays[arraycount][1] = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(co_addresscode.Substring(0, 4)).ddllist.ToList());
                                    }
                                    if (arrayscount == 7)
                                    {
                                        Guent_AddressArraays[arraycount][2] = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(co_addresscode.Substring(0, 2)).ddllist.ToList());
                                        Guent_AddressArraays[arraycount][3] = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(co_addresscode.Substring(0, 4)).ddllist.ToList());
                                        arraycount++;
                                    }
                                }
                                else
                                {
                                    GuentArrays[count][arrayscount] = GuentArrays[count][arrayscount];
                                }
                            }
                        }
                    }
                    #endregion

                    var FormValue = new TenantViewModel
                    {
                        FormID = FormID,
                        TenantType = ExistForm.TenantType,
                        Name = ExistForm.Name,                  //房客姓名
                        Gender = ExistForm.Gender,              //性別(0/1)
                        BirthdayStr = Convert.ToDateTime(ExistForm.Birthday).ToString("yyyy-MM-dd"),     //房客生日
                        IDNumber = ExistForm.IDNumber,          //身分證字號(統一編號)
                        Phone = ExistForm.Phone,                //電話
                        BankNo = ExistForm.BankNo,              //銀行代碼
                        BrancheNo = ExistForm.BrancheNo,        //分行代碼
                        BankAccount = ExistForm.BankAccount,    //銀行帳號

                        //地址下拉選單
                        TeAddress_District = Tenant_Address_Input[0],
                        TeAddress_Road = Tenant_Address_Input[1],
                        TeContact_District = Tenant_Address_Input[2],
                        TeContact_Road = Tenant_Address_Input[3],
                        AddressCode = addressCode_Tenant,           //地址代碼
                        AddressDetail = ExistForm.AddressDetail,
                        ContactAddressCode = contactCode_Tenant,    //通訊地址代碼
                        ContactAddressDetail = ExistForm.ContactAddressDetail,
                        accountNo = ExistForm.accountNo,            //戶號

                        //MemberArray = ExistForm.MemberArray,        //人數陣列[配偶, 直系, 代理人, 保證人]
                        CoupleCount = CoupleCount.ToString(),
                        Couple = ExistForm.Couple,

                        DirectCount = DirectCount.ToString(),
                        Family1 = DirectCount >= 1 ? ExistForm.Family1 : "[]",
                        Family2 = DirectCount >= 2 ? ExistForm.Family2 : "[]",
                        Family3 = DirectCount >= 3 ? ExistForm.Family3 : "[]",
                        Family4 = DirectCount >= 4 ? ExistForm.Family4 : "[]",
                        Family5 = DirectCount >= 5 ? ExistForm.Family5 : "[]",
                        Family6 = DirectCount >= 6 ? ExistForm.Family6 : "[]",
                        Family7 = DirectCount >= 7 ? ExistForm.Family7 : "[]",
                        Family8 = DirectCount >= 8 ? ExistForm.Family8 : "[]",
                        Family9 = DirectCount >= 9 ? ExistForm.Family9 : "[]",
                        Family10 = DirectCount >= 10 ? ExistForm.Family10 : "[]",

                        AgentCount = AgentCount.ToString(),
                        Agent1 = JsonConvert.SerializeObject(agentArrays[0]),
                        Agent2 = JsonConvert.SerializeObject(agentArrays[1]),
                        Agent3 = JsonConvert.SerializeObject(agentArrays[2]),

                        GuarantorCount = GuarantorCount.ToString(),
                        Guarantor1 = JsonConvert.SerializeObject(GuentArrays[0]),
                        Guarantor2 = JsonConvert.SerializeObject(GuentArrays[1]),
                        Guarantor3 = JsonConvert.SerializeObject(GuentArrays[2]),

                        Memo = String.IsNullOrEmpty(ExistForm.Memo) ? "" : ExistForm.Memo.Replace("\r\n", "\\r\\n"),

                        //下拉選單
                        //代理人一
                        AgAddress1_District = agent_AddressArraays[0][0],
                        AgAddress1_Road = agent_AddressArraays[0][1],
                        AgContact1_District = agent_AddressArraays[0][2],
                        AgContact1_Road = agent_AddressArraays[0][3],
                        //代理人二
                        AgAddress2_District = agent_AddressArraays[1][0],
                        AgAddress2_Road = agent_AddressArraays[1][1],
                        AgContact2_District = agent_AddressArraays[1][2],
                        AgContact2_Road = agent_AddressArraays[1][3],
                        //代理人三
                        AgAddress3_District = agent_AddressArraays[2][0],
                        AgAddress3_Road = agent_AddressArraays[2][1],
                        AgContact3_District = agent_AddressArraays[2][2],
                        AgContact3_Road = agent_AddressArraays[2][3],
                        //保證人一
                        GuAddress1_District = Guent_AddressArraays[0][0],
                        GuAddress1_Road = Guent_AddressArraays[0][1],
                        GuContact1_District = Guent_AddressArraays[0][2],
                        GuContact1_Road = Guent_AddressArraays[0][3],
                        //保證人二
                        GuAddress2_District = Guent_AddressArraays[1][0],
                        GuAddress2_Road = Guent_AddressArraays[1][1],
                        GuContact2_District = Guent_AddressArraays[1][2],
                        GuContact2_Road = Guent_AddressArraays[1][3],
                        //保證人三
                        GuAddress3_District = Guent_AddressArraays[2][0],
                        GuAddress3_Road = Guent_AddressArraays[2][1],
                        GuContact3_District = Guent_AddressArraays[2][2],
                        GuContact3_Road = Guent_AddressArraays[2][3],
                    };

                    var model = new FormViewModels
                    {
                        FormID = FormID != null ? FormID : "",
                        tenantviewmodel = FormValue,
                    };
                    #region Nlog
                    //取得目前model資料
                    //Type type = FormValue.GetType();
                    //PropertyInfo[] properties = type.GetProperties();

                    //foreach (PropertyInfo property in properties)
                    //{
                    //    string propertyName = property.Name;
                    //    object propertyValue = property.GetValue(FormValue);

                    //    string logMessage = $"{propertyName}: {propertyValue}";
                    //    logger.Error(logMessage);
                    //}
                    #endregion
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                logger.Error("取得房屋物件資料錯誤 : " + ex.ToString());
            }
            #endregion

            List<int> payment_date = new List<int>();
            for (int i = 1; i < 32; i++)
            {
                payment_date.Add(i);
            }
            ViewBag.Payment_date = payment_date;
            var initmodel = InitialModel();
            return View(initmodel);
        }

        [HttpPost]
        #region 過時
        //public ActionResult Tenant(
        //    string FormID,                  //表單編號

        //    string typeRadio,               //房客分類(一般戶; 一類戶; 二類戶)
        //    IEnumerable<HttpPostedFileBase> vulnerablefile,     //(file)上傳弱勢戶佐證文件
        //    IEnumerable<HttpPostedFileBase> sheetfile,          //(file)上傳300億試算表截圖

        //    string Name_0,                  //房客姓名
        //    string genderRadio_0,           //房客性別(男:1; 女:0)
        //    string birthday_0,              //房客生日
        //    string IDNumber_0,              //房客身分證
        //    string Phone_0,                 //房客電話
        //    string addressroad_0,           //房客地址(路)
        //    string AddressDetail_0,         //房客詳細地址  
        //    bool? sameaddress_check_0,      //房客通訊地址Checkbox
        //    string contactroad_0,           //房客通訊地址(路)
        //    string detailcontact_0,         //房客詳細通訊地址
        //    string accountnumber,           //房客戶號
        //    string bank_0,                  //房客銀行
        //    string bankbranche_0,           //房客銀行支部
        //    string bankaccount_0,           //房東銀行帳號

        //    //配偶
        //    bool? coupleInput,            //丈夫

        //    //直系親屬(Json格式)
        //    string directCount,             //親屬人數
        //    string directInput1,
        //    string directInput2,
        //    string directInput3,
        //    string directInput4,
        //    string directInput5,
        //    string directInput6,
        //    string directInput7,
        //    string directInput8,
        //    string directInput9,
        //    string directInput10,

        //    //代理人(Json格式)
        //    string AgentRadio,
        //    string AgentInput11,
        //    string AgentInput12,
        //    string AgentInput13,

        //    //保證人(Json格式)
        //    string GuarantorRadio,
        //    string GuarantorInput21,
        //    string GuarantorInput22,
        //    string GuarantorInput23,

        //    string memo
        //    )
        #endregion
        public ActionResult Tenant(TenantInputViewModel inputmodel)
        {
            #region 使用者資料
            var EmployeeData = (from db in memberdb.Members.Where(x => x.Account == User.Identity.Name)
                                join empdb in memberdb.EmployeeData on db.Account equals empdb.Account into temp
                                from empdb0 in temp.DefaultIfEmpty()
                                join deptdb in memberdb.Department on empdb0.DivCode equals deptdb.DivCode into temp2
                                from deptdb0 in temp2.DefaultIfEmpty()
                                select new MembersViewModel
                                {
                                    Name = db.Name,
                                    Department = empdb0 != null ? deptdb0.DivFullName : null,
                                    Position = empdb0 != null ? empdb0.JobTitle : null,
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

            if (String.IsNullOrEmpty(inputmodel.FormID))
            {
                return View();
            }

            //轉換地址
            var addressdata = formdb.Region.Where(x => x.RoadCode == inputmodel.addressroad_0).FirstOrDefault();
            var Address = addressdata.City + "-" + addressdata.District + "-" + addressdata.Road;
            var ContactAddress = "";
            if (inputmodel.sameaddress_check_0 != null)
            {
                inputmodel.contactroad_0 = inputmodel.addressroad_0;
                inputmodel.detailcontact_0 = inputmodel.AddressDetail_0;
            }
            else
            {
                addressdata = formdb.Region.Where(x => x.RoadCode == inputmodel.contactroad_0).FirstOrDefault();
                ContactAddress = addressdata.City + "-" + addressdata.District + "-" + addressdata.Road;
            }

            #region 存檔(弱勢戶)
            //取得檔名與檔案GUID
            List<string> vulnerablefileNameArray = new List<string>();
            List<string> vulnerablefileAliasArray = new List<string>();
            //存檔
            if (inputmodel.vulnerablefile != null && inputmodel.vulnerablefile.Any())
            {
                try
                {
                    foreach (var file in inputmodel.vulnerablefile)
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
                    MailService mailService = new MailService();
                    mailService.SendMail("【力群管理系統】房客資料寫入DB錯誤", ex.ToString(), "cys.enki@gmail.com");
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
            if (inputmodel.sheetfile != null && inputmodel.sheetfile.Any())
            {
                try
                {
                    foreach (var file in inputmodel.sheetfile)
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
                    MailService mailService = new MailService();
                    mailService.SendMail("【力群管理系統】房客資料寫入DB錯誤", ex.ToString(), "cys.enki@gmail.com");
                }
            }
            string sheetfileNames = JsonConvert.SerializeObject(sheetfileNameArray);
            string sheetfileAlias = JsonConvert.SerializeObject(sheetfileAliasArray);
            #endregion


            //成員陣列[配偶, 直系, 代理人, 保證人]
            var coupleType = -1;
            if (!String.IsNullOrEmpty(inputmodel.coupleInput))
            {
                var coupleArray = JsonConvert.DeserializeObject<string[]>(inputmodel.coupleInput);
                coupleType = Convert.ToInt32(coupleArray[0]);    //-1:無; 1:丈夫; 0:妻子;
            }
            int[] memberArray = new int[4];
            var agentCount = Convert.ToInt32(inputmodel.AgentRadio);
            var guarantorCount = Convert.ToInt32(inputmodel.GuarantorRadio);
            memberArray[0] = coupleType != -1 ? 1 : 0;
            memberArray[1] = Convert.ToInt32(inputmodel.directCount);
            memberArray[2] = agentCount;
            memberArray[3] = guarantorCount;
            string jsonmemberArray = JsonConvert.SerializeObject(memberArray);

            #region 打包代理人資料

            string[] agentarray1 = new string[9];
            string[] agentarray2 = new string[9];
            string[] agentarray3 = new string[9];

            //創建複數陣列存取資料型態
            string[] agentInputs = new string[] { inputmodel.AgentInput11, inputmodel.AgentInput12, inputmodel.AgentInput13};   //輸入
            string[][] agentArrays = new string[][] { agentarray1, agentarray2, agentarray3};        //輸出

            for (int count = 0; count < agentCount; count++)
            {
                //找到要取得的共有人Json陣列資料
                List<string> agentlist = new List<string>();
                agentlist = JsonConvert.DeserializeObject<List<string>>(agentInputs[count]);
                //遞迴取出資料
                for (int i = 0; i < agentlist.Count(); i++)
                //foreach(var item in agentlist)
                {
                    if (i == 5 || i == 7)
                    {
                        //由Address找到地址資料
                        var roadcode = agentlist[i].ToString();
                        addressdata = formdb.Region.Where(x => x.RoadCode == roadcode).FirstOrDefault();
                        agentArrays[count][i] = addressdata.City + "-" + addressdata.District + "-" + addressdata.Road;
                    }
                    else
                    {
                        agentArrays[count][i] = agentlist[i];
                    }
                }
            }
            #endregion

            
            #region 打包代理人資料

            string[] guarantorarray1 = new string[9];
            string[] guarantorarray2 = new string[9];
            string[] guarantorarray3 = new string[9];

            //創建複數陣列存取資料型態
            string[] guarantorInputs = new string[] { inputmodel.GuarantorInput21, inputmodel.GuarantorInput22, inputmodel.GuarantorInput23 };   //輸入
            string[][] guarantorArrays = new string[][] { guarantorarray1, guarantorarray2, guarantorarray3};        //輸出

            for (int count = 0; count < guarantorCount; count++)
            {
                //找到要取得的共有人Json陣列資料
                List<string> guarantorlist = new List<string>();
                guarantorlist = JsonConvert.DeserializeObject<List<string>>(guarantorInputs[count]);
                //遞迴取出資料
                for (int i = 0; i < guarantorlist.Count(); i++)
                //foreach(var item in guarantorlist)
                {
                    if (i == 5 || i == 7)
                    {
                        //由Address找到地址資料
                        var roadcode = guarantorlist[i].ToString();
                        addressdata = formdb.Region.Where(x => x.RoadCode == roadcode).FirstOrDefault();
                        guarantorArrays[count][i] = addressdata.City + "-" + addressdata.District + "-" + addressdata.Road;
                    }
                    else
                    {
                        guarantorArrays[count][i] = guarantorlist[i];
                    }
                }
            }
            #endregion


            try
            {
                var exist = formdb.Tenant.Where(x => x.FormID == inputmodel.FormID).FirstOrDefault();
                if (exist == null)
                {
                    // 建立資料上下文（Data Context）
                    using (var context = new FormModels())
                    {
                        // 建立要插入的資料物件
                        var newData = new Tenant
                        {
                            FormID = inputmodel.FormID,
                            TenantType = Convert.ToInt32(inputmodel.typeRadio),
                            vulnerablefile_Name = vulnerablefileNames,
                            vulnerablefile_Alias = vulnerablefileAlias,
                            sheetfile_Name = sheetfileNames,
                            sheetfile_Alias = sheetfileAlias,
                            Name = inputmodel.Name_0,
                            Gender = Convert.ToInt32(inputmodel.genderRadio_0),
                            Birthday = Convert.ToDateTime(inputmodel.birthday_0),
                            IDNumber = inputmodel.IDNumber_0,
                            Phone = inputmodel.Phone_0,
                            BankNo = inputmodel.bank_0,
                            BrancheNo = inputmodel.bankbranche_0,
                            BankAccount = inputmodel.bankaccount_0,
                            Address = Address,
                            accountNo = inputmodel.accountnumber,
                            AddressDetail = inputmodel.AddressDetail_0,
                            ContactAddress = ContactAddress,
                            ContactAddressDetail = inputmodel.detailcontact_0,
                            MemberArray = jsonmemberArray,
                            //配偶
                            Couple = inputmodel.coupleInput,
                            //直系親屬
                            Family1 = inputmodel.directInput1,
                            Family2 = inputmodel.directInput2,
                            Family3 = inputmodel.directInput3,
                            Family4 = inputmodel.directInput4,
                            Family5 = inputmodel.directInput5,
                            Family6 = inputmodel.directInput6,
                            Family7 = inputmodel.directInput7,
                            Family8 = inputmodel.directInput8,
                            Family9 = inputmodel.directInput9,
                            Family10 = inputmodel.directInput10,
                            Agent1 = agentCount >= 1 ? JsonConvert.SerializeObject(agentArrays[0]) : null,
                            Agent2 = agentCount >= 2 ? JsonConvert.SerializeObject(agentArrays[1]) : null,
                            Agent3 = agentCount >= 3 ? JsonConvert.SerializeObject(agentArrays[2]) : null,
                            Guarantor1 = guarantorCount >= 1 ? JsonConvert.SerializeObject(guarantorArrays[0]) : null,
                            Guarantor2 = guarantorCount >= 2 ? JsonConvert.SerializeObject(guarantorArrays[1]) : null,
                            Guarantor3 = guarantorCount >= 3 ? JsonConvert.SerializeObject(guarantorArrays[2]) : null,

                            CreateUser = userid,
                            CreateTime = now,
                            UpdateUser = userid,
                            UpdateTime = now,
                            Memo = inputmodel.memo,
                        };
                        // 使用資料上下文插入資料物件
                        context.Tenant.Add(newData);
                        // 儲存更改到資料庫
                        context.SaveChanges();
                    }
                }
                else
                {
                    // 建立資料上下文（Data Context）
                    using (var context = new FormModels())
                    {
                        var existformdata = context.Tenant.Where(x => x.FormID == inputmodel.FormID).FirstOrDefault();
                        //更新物件內資料
                        existformdata.TenantType = Convert.ToInt32(inputmodel.typeRadio);
                        existformdata.vulnerablefile_Name = vulnerablefileNames;
                        existformdata.vulnerablefile_Alias = vulnerablefileAlias;
                        existformdata.sheetfile_Name = sheetfileNames;
                        existformdata.sheetfile_Alias = sheetfileAlias;
                        existformdata.Name = inputmodel.Name_0;
                        existformdata.Gender = Convert.ToInt32(inputmodel.genderRadio_0);
                        existformdata.Birthday = Convert.ToDateTime(inputmodel.birthday_0);
                        existformdata.IDNumber = inputmodel.IDNumber_0;
                        existformdata.Phone = inputmodel.Phone_0;
                        existformdata.BankNo = inputmodel.bank_0;
                        existformdata.BrancheNo = inputmodel.bankbranche_0;
                        existformdata.BankAccount = inputmodel.bankaccount_0;
                        existformdata.Address = Address;
                        existformdata.accountNo = inputmodel.accountnumber;
                        existformdata.AddressDetail = inputmodel.AddressDetail_0;
                        existformdata.ContactAddress = ContactAddress;
                        existformdata.ContactAddressDetail = inputmodel.detailcontact_0;
                        existformdata.MemberArray = jsonmemberArray;
                        //配偶
                        existformdata.Couple = inputmodel.coupleInput;
                        //直系親屬
                        existformdata.Family1 = inputmodel.directInput1;
                        existformdata.Family2 = inputmodel.directInput2;
                        existformdata.Family3 = inputmodel.directInput3;
                        existformdata.Family4 = inputmodel.directInput4;
                        existformdata.Family5 = inputmodel.directInput5;
                        existformdata.Family6 = inputmodel.directInput6;
                        existformdata.Family7 = inputmodel.directInput7;
                        existformdata.Family8 = inputmodel.directInput8;
                        existformdata.Family9 = inputmodel.directInput9;
                        existformdata.Family10 = inputmodel.directInput10;
                        existformdata.Agent1 = agentCount >= 1 ? JsonConvert.SerializeObject(agentArrays[0]) : null;
                        existformdata.Agent2 = agentCount >= 2 ? JsonConvert.SerializeObject(agentArrays[1]) : null;
                        existformdata.Agent3 = agentCount >= 3 ? JsonConvert.SerializeObject(agentArrays[2]) : null;
                        existformdata.Guarantor1 = guarantorCount >= 1 ? JsonConvert.SerializeObject(guarantorArrays[0]) : null;
                        existformdata.Guarantor2 = guarantorCount >= 2 ? JsonConvert.SerializeObject(guarantorArrays[1]) : null;
                        existformdata.Guarantor3 = guarantorCount >= 3 ? JsonConvert.SerializeObject(guarantorArrays[2]) : null;

                        existformdata.UpdateUser = userid;
                        existformdata.UpdateTime = now;
                        existformdata.Memo = inputmodel.memo;


                        // 儲存更改到資料庫
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                MailService mailService = new MailService();
                mailService.SendMail("【力群管理系統】房客資料寫入DB錯誤", ex.ToString(), "cys.enki@gmail.com");
                return RedirectToAction("Tenant", "Form", new { FormID = inputmodel.FormID });
            }

            return RedirectToAction("CaseManage", "Sales");
        }
        #endregion

        #region 秘書填寫
        [HttpGet]
        public ActionResult Secretary()
        {
            logger.Info("進入秘書填寫");
            #region 使用者資料
            var EmployeeData = (from db in memberdb.Members.Where(x => x.Account == User.Identity.Name)
                                join empdb in memberdb.EmployeeData on db.Account equals empdb.Account into temp
                                from empdb0 in temp.DefaultIfEmpty()
                                join deptdb in memberdb.Department on empdb0.DivCode equals deptdb.DivCode into temp2
                                from deptdb0 in temp2.DefaultIfEmpty()
                                select new MembersViewModel
                                {
                                    Name = db.Name,
                                    Department = empdb0 != null ? deptdb0.DivFullName : null,
                                    Position = empdb0 != null ? empdb0.JobTitle : null,
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
                                join deptdb in memberdb.Department on empdb0.DivCode equals deptdb.DivCode into temp2
                                from deptdb0 in temp2.DefaultIfEmpty()
                                select new MembersViewModel
                               {
                                   Name = db.Name,
                                   Department = empdb0 != null ? deptdb0.DivFullName : null,
                                   Position = empdb0 != null ? empdb0.JobTitle : null,
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