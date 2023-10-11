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
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Policy;
using System.Web;
using System.Web.ApplicationServices;
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
        [HttpGet]
        public ActionResult HomeObject(string FormID, string ControllerName)
        {
            DDLServices ddlservices = new DDLServices();
            var initmodel = InitialModel();
            ControllerName = ControllerName != null ? "Secretary" : "Sales";     //是否秘書端進入
            ViewBag.ControllerName = ControllerName;
            logger.Info("進入房屋物件資料頁面，ControllerName:" + ControllerName);
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
            var IsAdmin = User.IsInRole("Admin");
            ViewBag.Role = User.IsInRole("Admin") ? "Admin" : User.IsInRole("Agent") ? "Agent" : User.IsInRole("Secretary") ? "Secretary" : "";
            #endregion

            #region 下拉選單
            ViewBag.citylist = JsonConvert.SerializeObject(ddlservices.GetRegionDDL("").ddllist.ToList());
            List<int> payment_date = new List<int>();
            for (int i = 1; i <= 31; i++){payment_date.Add(i);}
            ViewBag.Payment_date = payment_date;
            #endregion

            if (!String.IsNullOrEmpty(FormID))
            {
                #region 存在表單資料
                try
                {
                    var CaseData = formdb.ObjectForm.Where(x => x.FormID == FormID).FirstOrDefault();
                    if (CaseData.ProcessAccount != User.Identity.Name && !IsAdmin)
                    {
                        ///無編輯權限: 
                        ///非處理職員 且 非系統管理員
                        return RedirectToAction("CaseManage", ControllerName);
                    }
                    var ExistHomeObject = formdb.HomeObject.Where(x => x.FormID == FormID).FirstOrDefault();
                    if (ExistHomeObject != null)
                    {
                        //取得物件地址DDL
                        var addressdata = formdb.Region.Where(x => x.City == ExistHomeObject.city && x.District == ExistHomeObject.district && x.Road == ExistHomeObject.road).FirstOrDefault();
                        var CityCode = addressdata.CityCode;
                        var DistrictCode = addressdata.DistrictCode;
                        var Address = addressdata.RoadCode;
                        var districtlist = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(CityCode).ddllist.ToList());
                        var roadlist = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(DistrictCode).ddllist.ToList());

                        var FormValue = new HomeObjectViewModel
                        {
                            objecttype = ExistHomeObject.objecttype,
                            notarization = ExistHomeObject.notarization,
                            signdateStr = Convert.ToDateTime(ExistHomeObject.signdate).ToString("yyyy-MM-dd"),
                            appraiser = ExistHomeObject.appraiser,
                            feature = ExistHomeObject.feature != null ? ExistHomeObject.feature : "",
                            citycode = CityCode,
                            districtcode = DistrictCode,
                            distirctJsonDDL = districtlist,     //鄉鎮市區地址
                            roadcode = Address,
                            roadJsonDDL = roadlist,             //道路地址
                            AddressDetail = ExistHomeObject.detailaddress,
                            usefor = ExistHomeObject.usefor,
                            useforelse = String.IsNullOrWhiteSpace(ExistHomeObject.useforelse) ? "" : ExistHomeObject.useforelse,
                            rent = ExistHomeObject.rent,
                            deposit = ExistHomeObject.deposit,
                            management_fee = ExistHomeObject.management_fee,
                            startdateStr = Convert.ToDateTime(ExistHomeObject.startdate).ToString("yyyy-MM-dd"),
                            enddateStr = Convert.ToDateTime(ExistHomeObject.enddate).ToString("yyyy-MM-dd"),
                            paydate = ExistHomeObject.paydate,
                            buildtype = ExistHomeObject.buildtype,
                            roomtype = ExistHomeObject.roomtype,
                            roomamount = ExistHomeObject.roomamount,
                            //roomamountlist = JsonConvert.DeserializeObject<List<int>>(ExistHomeObject.roomamount),
                            havepark = ExistHomeObject.havepark,
                            //haveparklist = JsonConvert.DeserializeObject<List<int>>(ExistHomeObject.havepark),
                            parktype = ExistHomeObject.parktype,
                            parkfloor = ExistHomeObject.parkfloor,
                            //parkfloorlist = JsonConvert.DeserializeObject<List<int>>(ExistHomeObject.parkfloor),
                            carpositionnumber = String.IsNullOrEmpty(ExistHomeObject.carpositionnumber) ? "" : ExistHomeObject.carpositionnumber,   //汽車位編號
                            carmonthrent = ExistHomeObject.carmonthrent,
                            carparkmanagefee = ExistHomeObject.carparkmanagefee,
                            scooterparkfloor = ExistHomeObject.scooterparkfloor,
                            //scooterparkfloorlist = JsonConvert.DeserializeObject<List<int>>(ExistHomeObject.scooterparkfloor),
                            scooterpositionnumber = String.IsNullOrEmpty(ExistHomeObject.scooterpositionnumber) ? "" : ExistHomeObject.scooterpositionnumber,   //機車位編號
                            scootermonthrent = ExistHomeObject.scootermonthrent,
                            scootermanagefee = ExistHomeObject.scootermanagefee,
                            Accessory = ExistHomeObject.Accessory,    //房屋附屬物件

                            Memo = String.IsNullOrEmpty(ExistHomeObject.Memo) ? "" : ExistHomeObject.Memo.Replace("\r\n", "\\r\\n"),
                        };

                        var model = new FormViewModels
                        {
                            FormID = FormID != null ? FormID : "",
                            homeobjectviewmodel = FormValue,
                        };

                        ViewBag.FileNameList = ExistHomeObject.taxfile_name != null ? JsonConvert.DeserializeObject<List<string>>(ExistHomeObject.taxfile_name) : null;
                        ViewBag.FileAliasList = ExistHomeObject.taxfile_alias != null ?JsonConvert.DeserializeObject<List<string>>(ExistHomeObject.taxfile_alias) : null;

                        return View(model);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("取得房屋物件資料錯誤 : " + ex.ToString());
                }
                #endregion

            }
            //若無案件編號; 無物件資料
            //返回空白表單
            return View(initmodel);
        }

        //[Insert]新增物件
        [HttpPost]
        public ActionResult HomeObject(HomeObjectInputViewModel inputmodel, string ControllerName)
        {
            FormService formService = new FormService();
            FileService fileService = new FileService();
            var initmodel = InitialModel();
            ControllerName = ControllerName != null ? ControllerName : "Sales";     //是否秘書端進入
            logger.Info("房屋物件資料頁面送出編輯，ControllerName:" + ControllerName);

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
            var IsAdmin = User.IsInRole("Admin");
            ViewBag.Role = User.IsInRole("Admin") ? "Admin" : User.IsInRole("Agent") ? "Agent" : User.IsInRole("Secretary") ? "Secretary" : "";
            #endregion

            #region 資料處理
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
            var Startdate = Convert.ToDateTime(startdateYear + "-" + startdateparts[1] + "-" + startdateparts[2]);
            var Enddate = Convert.ToDateTime(enddateYear + "-" + enddateparts[1] + "-" + enddateparts[2]);

            //找到地址
            var address = formdb.Region.Where(x => x.RoadCode == inputmodel.selectroad).FirstOrDefault();
            #endregion

            //找到新表單編號
            var newFormID = String.IsNullOrEmpty(inputmodel.FormID) ? formService.GetNewFormID() : inputmodel.FormID;

            FileViewMode filemodel = new FileViewMode();
            logger.Info("開始存檔");
            #region 存檔
            if (inputmodel.taxFile != null && inputmodel.taxFile.Any())
            {
                filemodel = fileService.SaveFile(newFormID, "房屋稅單", inputmodel.taxFile, "Form", User.Identity.Name);
                logger.Info("存檔成功 filemodel" + filemodel);
                if (filemodel == null)
                    return Content("上傳檔案錯誤，請重新選擇檔案，若問題未解決，請尋求系統管理員協助。");
            }
            #endregion

            #region 新增/編輯
            try
            {
                if (String.IsNullOrEmpty(inputmodel.FormID))
                {
                    #region 新增表單
                    using (var context = new FormModels())
                    {
                        //創建新物件
                        var newHomeData = new HomeObject
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
                            taxfile_name = filemodel.FileName,
                            taxfile_alias = filemodel.FileAlias,
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

                            CreateAccount = User.Identity.Name,
                            CreateTime = DateTime.Now,
                            UpdateAccount = User.Identity.Name,
                            UpdateTime = DateTime.Now,
                            Memo = inputmodel.memo,
                        };
                        context.HomeObject.Add(newHomeData);

                        //創建新案件
                        var newCaseData = new ObjectForm
                        {
                            FormID = newFormID,
                            CreateAccount = User.Identity.Name,
                            CreateTime = DateTime.Now,
                            UpdateAccount = User.Identity.Name,
                            UpdateTime = DateTime.Now,
                            ProcessAccount = User.Identity.Name,
                            ProcessName = EmployeeData.Name,
                            AgentAccount = User.Identity.Name,
                            FormType = 0,
                        };
                        // 使用資料上下文插入資料物件
                        context.ObjectForm.Add(newCaseData);
                        context.SaveChanges();
                    }
                    #endregion
                }
                else
                {
                    #region 編輯表單
                    var CaseData = formdb.ObjectForm.Where(x => x.FormID == inputmodel.FormID).FirstOrDefault();
                    if (CaseData.ProcessAccount != User.Identity.Name && !IsAdmin)
                    {
                        ///無編輯權限
                        ///非處理職員 且 非系統管理員
                        logger.Info("房屋物件編輯無權限，重新導向，ControllerName:" + ControllerName);
                        return RedirectToAction("CaseManage", ControllerName);
                    }

                    using (var context = new FormModels())
                    {
                        //更新物件資料
                        var existformdata = context.HomeObject.Where(x => x.FormID == inputmodel.FormID).FirstOrDefault();

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
                        existformdata.taxfile_name = filemodel.FileName;
                        existformdata.taxfile_alias = filemodel.FileAlias;
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

                        existformdata.UpdateAccount = User.Identity.Name;
                        existformdata.UpdateTime = DateTime.Now;
                        existformdata.Memo = inputmodel.memo;
                        // 儲存更改到資料庫
                        context.SaveChanges();
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                MailService mailService = new MailService();
                mailService.SendMail("【力群管理系統】物件資料(新增/編輯)錯誤", ex.ToString(), "cys.enki@gmail.com");
                ViewBag.ErrorMessage = ex.Message;
                logger.Info("房屋物件編輯錯誤，返回initial頁面:");
                return View(initmodel);
            }
            #endregion
            return RedirectToAction("CaseManage", ControllerName);
        }
        #endregion

        #region 房東資料
        [HttpGet]
        public ActionResult Landlord(string FormID, string ControllerName)
        {
            var initmodel = InitialModel();
            initmodel.FormID = FormID != null ? FormID : "";
            ControllerName = ControllerName != null ? "Secretary" : "Sales";     //是否秘書端進入
            ViewBag.ControllerName = ControllerName;
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
            var IsAdmin = User.IsInRole("Admin");
            ViewBag.Role = User.IsInRole("Admin") ? "Admin" : User.IsInRole("Agent") ? "Agent" : User.IsInRole("Secretary") ? "Secretary" : "";
            #endregion

            DDLServices ddlservices = new DDLServices();
            ViewBag.citylist = JsonConvert.SerializeObject(ddlservices.GetRegionDDL("").ddllist.ToList());
            ViewBag.banklist = JsonConvert.SerializeObject(ddlservices.GetBankDDL("", "bank").ddllist.ToList());

            #region 取得資料
            if (!String.IsNullOrEmpty(FormID))
            {
                var CaseData = formdb.ObjectForm.Where(x => x.FormID == FormID).FirstOrDefault();
                if (CaseData.ProcessAccount != User.Identity.Name && !IsAdmin)
                {
                    ///無編輯權限
                    ///非處理職員 且 非系統管理員
                    return RedirectToAction("CaseManage", ControllerName);
                }
                try
                {
                    var ExistLandLlordForm = formdb.LandLord.Where(x => x.FormID == FormID).FirstOrDefault();
                    ViewBag.Exist = ExistLandLlordForm != null ? "exist" : "noexist";
                    if (ExistLandLlordForm != null)
                    {
                        //傳回已存在房客資料
                        #region 取得房東資料(地址)
                        //取得房東戶籍地址
                        string[] Addressparts = ExistLandLlordForm.Address.Split('-');    //地址
                        var AddressSplit_Landlord = new { City = Addressparts[0].ToString(), District = Addressparts[1].ToString(), Road = Addressparts[2].ToString(), };
                        var addressCode_Landlord = formdb.Region.Where(x => x.City == AddressSplit_Landlord.City && x.District == AddressSplit_Landlord.District && x.Road == AddressSplit_Landlord.Road).Select(x => x.RoadCode).FirstOrDefault();
                        //取得房東通訊地址
                        string[] Contactparts = ExistLandLlordForm.ContactAddress.Split('-');    //通訊地址
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
                        var CoOwnerCount = Convert.ToInt32(JsonConvert.DeserializeObject<List<string>>(ExistLandLlordForm.MemberArray)[0]);
                        string[] coownerarray1 = new string[7];
                        string[] coownerarray2 = new string[7];
                        string[] coownerarray3 = new string[7];
                        string[] coownerarray4 = new string[7];
                        string[] coownerarray5 = new string[7];
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
                            coownerArrays[count] = JsonConvert.DeserializeObject<List<string>>(ExistLandLlordForm.GetType().GetProperty(coOwnerProperty).GetValue(ExistLandLlordForm).ToString()).ToArray();

                            //將地址轉換成地址代碼
                            for (int arrayscount = 0; arrayscount < coownerArrays[count].Count(); arrayscount++)
                            {
                                //4:地址 // 6:通訊地址
                                if (arrayscount == 3 || arrayscount == 5)
                                {
                                    string[] Parts = coownerArrays[count][arrayscount].Split('-');    //通訊地址
                                                                                                      //門牌地址 => 地址代碼
                                    var Split = new { City = Parts[0].ToString(), District = Parts[1].ToString(), Road = Parts[2].ToString(), };
                                    var co_addresscode = formdb.Region.Where(x => x.City == Split.City && x.District == Split.District && x.Road == Split.Road).Select(x => x.RoadCode).FirstOrDefault();
                                    coownerArrays[count][arrayscount] = co_addresscode;

                                    //找到DDL並存入
                                    if (arrayscount == 3)
                                    {
                                        var splitaddress = new { City = Parts[0].ToString(), District = Parts[1].ToString(), Road = Parts[2].ToString(), };
                                        CoOwner_AddressArraays[arraycount][0] = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(co_addresscode.Substring(0, 2)).ddllist.ToList());
                                        CoOwner_AddressArraays[arraycount][1] = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(co_addresscode.Substring(0, 4)).ddllist.ToList());
                                    }
                                    if (arrayscount == 5)
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
                        var AgentCount = Convert.ToInt32(JsonConvert.DeserializeObject<List<string>>(ExistLandLlordForm.MemberArray)[1]);
                        //取得代理人陣列
                        string[] agentarray = new string[7];
                        //取得代理人地址陣列
                        string[] Agent_Address_Input = new string[4];
                        if (ExistLandLlordForm.Agent != null)
                        {
                            if (AgentCount > 0)
                            {
                                agentarray = JsonConvert.DeserializeObject<List<string>>(ExistLandLlordForm.Agent).ToArray();

                                //將地址轉換成地址代碼
                                for (int arrayscount = 0; arrayscount < agentarray.Count(); arrayscount++)
                                {

                                    //4:地址 // 6:通訊地址
                                    if (arrayscount == 3 || arrayscount == 5)
                                    {
                                        string[] Parts = agentarray[arrayscount].Split('-');    //通訊地址
                                                                                                //門牌地址 => 地址代碼
                                        var Split = new { City = Parts[0].ToString(), District = Parts[1].ToString(), Road = Parts[2].ToString(), };
                                        var ag_addresscode = formdb.Region.Where(x => x.City == Split.City && x.District == Split.District && x.Road == Split.Road).Select(x => x.RoadCode).FirstOrDefault();
                                        agentarray[arrayscount] = ag_addresscode;

                                        //找到DDL並存入
                                        if (arrayscount == 3)
                                        {
                                            var splitaddress = new { City = Parts[0].ToString(), District = Parts[1].ToString(), Road = Parts[2].ToString(), };
                                            Agent_Address_Input[0] = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(ag_addresscode.Substring(0, 2)).ddllist.ToList());
                                            Agent_Address_Input[1] = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(ag_addresscode.Substring(0, 4)).ddllist.ToList());
                                        }
                                        if (arrayscount == 5)
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
                            Name = ExistLandLlordForm.Name,      //房東姓名(公司名稱)
                            Principal = ExistLandLlordForm.Gender != 2 ? "" : ExistLandLlordForm.Principal,       //負責人
                            Gender = ExistLandLlordForm.Gender,      //性別(0/1)，法人(2)
                            BirthdayStr = ExistLandLlordForm.Gender != 2 ? Convert.ToDateTime(ExistLandLlordForm.Birthday).ToString("yyyy-MM-dd") : null,     //房東生日
                            IDNumber = ExistLandLlordForm.IDNumber,      //身分證字號(統一編號)
                            Phone = ExistLandLlordForm.Phone,        //電話
                            BankNo = ExistLandLlordForm.BankNo,      //銀行代碼
                            BrancheNo = ExistLandLlordForm.BrancheNo,        //分行代碼
                            BankAccount = ExistLandLlordForm.BankAccount,        //銀行帳號
                            AddressCode_LandLord = addressCode_Landlord,    //地址代碼
                            AddressDetail = ExistLandLlordForm.AddressDetail,
                            ContactCode_Landlord = contactCode_Landlord,    //通訊地址代碼
                            ContactAddressDetail = ExistLandLlordForm.ContactAddressDetail,
                            CoOwnerCount = CoOwnerCount,
                            CoOwner1 = JsonConvert.SerializeObject(coownerArrays[0]),
                            CoOwner2 = JsonConvert.SerializeObject(coownerArrays[1]),
                            CoOwner3 = JsonConvert.SerializeObject(coownerArrays[2]),
                            CoOwner4 = JsonConvert.SerializeObject(coownerArrays[3]),
                            CoOwner5 = JsonConvert.SerializeObject(coownerArrays[4]),
                            AgentCount = AgentCount,
                            Agent = JsonConvert.SerializeObject(agentarray),
                            Memo = String.IsNullOrEmpty(ExistLandLlordForm.Memo) ? "" : ExistLandLlordForm.Memo.Replace("\r\n", "\\r\\n"),

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

                        initmodel.landlordviewmodel = FormValue;
                        return View(initmodel);
                    }
                    else
                    {
                        //無房客資料，創建新表單
                        return View(initmodel);
                    }
                }
                catch (Exception ex)
                {
                    MailService mailService = new MailService();
                    mailService.SendMail("【力群管理系統】房東(進入頁面)錯誤", ex.ToString(), "cys.enki@gmail.com");
                    TempData["ErrorMessage"] = ex.Message;
                    return RedirectToAction("CaseManage", ControllerName);
                }
            }
            #endregion

            //無案件編號，請先建立物件資料
            return RedirectToAction("CaseManage", ControllerName);
        }
        [HttpPost]
        public ActionResult Landlord(LandlordInputViewModel inputmodel, string ControllerName)
        {
            var initmodel = InitialModel();
            initmodel.FormID = inputmodel.FormID;
            ControllerName = ControllerName != null ? ControllerName : "Sales";     //是否秘書端進入
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
            var IsAdmin = User.IsInRole("Admin");
            ViewBag.Role = User.IsInRole("Admin") ? "Admin" : User.IsInRole("Agent") ? "Agent" : User.IsInRole("Secretary") ? "Secretary" : "";
            #endregion

            #region 轉換資料
            //轉換地址
            var addressdata = formdb.Region.Where(x => x.RoadCode == inputmodel.addressroad_0).FirstOrDefault();
            var Address = addressdata.City + "-" + addressdata.District + "-" + addressdata.Road;
            var ContactAddress = "";
            if (inputmodel.sameaddress_check_0 != null)
            {
                ContactAddress = Address;
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
            string[] coownerarray1 = new string[7];
            string[] coownerarray2 = new string[7];
            string[] coownerarray3 = new string[7];
            string[] coownerarray4 = new string[7];
            string[] coownerarray5 = new string[7];

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
                    if(i == 3 || i == 5)
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
            string[] agentarray = new string[7];
            if (inputmodel.agentCheckbox != null)
            {
                //找到要取得的共有人Json陣列資料
                List<string> agentlist = new List<string>();
                agentlist = JsonConvert.DeserializeObject<List<string>>(inputmodel.AgentInput);
                //遞迴取出資料
                for (int i = 0; i < agentlist.Count(); i++)
                //foreach(var item in coownerlist)
                {
                    if (i == 3 || i == 5)
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
            #endregion

            #region 新增/編輯
            if (!String.IsNullOrEmpty(inputmodel.FormID))
            {
                try
                {
                    var CaseData = formdb.ObjectForm.Where(x => x.FormID == inputmodel.FormID).FirstOrDefault();
                    if (CaseData.ProcessAccount != User.Identity.Name && !IsAdmin)
                    {
                        ///無編輯權限: 
                        ///非處理職員 且 非系統管理員
                        return RedirectToAction("CaseManage", ControllerName);
                    }
                    var existLandlordForm = formdb.LandLord.Where(x => x.FormID == inputmodel.FormID).FirstOrDefault();
                    if (existLandlordForm == null)
                    {
                        //初次建立房東資料
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

                                CreateAccount = User.Identity.Name,
                                CreateTime = DateTime.Now,
                                UpdateAccount = User.Identity.Name,
                                UpdateTime = DateTime.Now,
                                Memo = inputmodel.memo,
                            };

                            context.LandLord.Add(newData);
                            context.SaveChanges();
                        }
                    }
                    else
                    {
                        //編輯已存在房東資料
                        using (var context = new FormModels())
                        {
                            var existLandlordData = context.LandLord.Where(x => x.FormID == inputmodel.FormID).FirstOrDefault();

                            existLandlordData.Name = inputmodel.Name_0;
                            existLandlordData.Principal = inputmodel.persontypeRadio == "1" ? inputmodel.Principal : null;
                            existLandlordData.Gender = inputmodel.persontypeRadio == "1" ? 2 : Convert.ToInt32(inputmodel.genderRadio_0);   //男:1 女:0 法人:2
                            existLandlordData.Birthday = LandlordBirthday;
                            existLandlordData.IDNumber = inputmodel.IDNumber_0;
                            existLandlordData.Phone = inputmodel.Phone_0;
                            existLandlordData.BankNo = inputmodel.bank_0;
                            existLandlordData.BrancheNo = inputmodel.bankbranche_0;
                            existLandlordData.BankAccount = inputmodel.bankaccount_0;
                            existLandlordData.Address = Address;
                            existLandlordData.AddressDetail = inputmodel.AddressDetail_0;
                            existLandlordData.ContactAddress = ContactAddress;
                            existLandlordData.ContactAddressDetail = inputmodel.detailcontact_0;
                            existLandlordData.MemberArray = jsonmemberArray;
                            existLandlordData.CoOwner1 = CoOwnerCount >= 1 ? JsonConvert.SerializeObject(coownerArrays[0]) : null;
                            existLandlordData.CoOwner2 = CoOwnerCount >= 2 ? JsonConvert.SerializeObject(coownerArrays[1]) : null;
                            existLandlordData.CoOwner3 = CoOwnerCount >= 3 ? JsonConvert.SerializeObject(coownerArrays[2]) : null;
                            existLandlordData.CoOwner4 = CoOwnerCount >= 4 ? JsonConvert.SerializeObject(coownerArrays[3]) : null;
                            existLandlordData.CoOwner5 = CoOwnerCount >= 5 ? JsonConvert.SerializeObject(coownerArrays[4]) : null;
                            existLandlordData.Agent = inputmodel.agentCheckbox != null ? JsonConvert.SerializeObject(agentarray) : null;

                            existLandlordData.UpdateAccount = User.Identity.Name;
                            existLandlordData.UpdateTime = DateTime.Now;
                            existLandlordData.Memo = inputmodel.memo;

                            context.SaveChanges();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MailService mailService = new MailService();
                    mailService.SendMail("【力群管理系統】房客資料寫入DB錯誤", ex.ToString(), "cys.enki@gmail.com");
                    ViewBag.ErrorMessage = ex.Message;
                    return View(initmodel);
                }

            }
            #endregion

            //此案件編號不存在，請先新增物件資料
            return RedirectToAction("CaseManage", ControllerName);
        }
        #endregion

        #region 房客資料
        [HttpGet]
        public ActionResult Tenant(string FormID, string ControllerName)
        {
            var initmodel = InitialModel();
            initmodel.FormID = FormID != null ? FormID : "";
            ControllerName = ControllerName != null ? "Secretary" : "Sales";     //是否秘書端進入
            ViewBag.ControllerName = ControllerName;
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
            var IsAdmin = User.IsInRole("Admin");
            ViewBag.Role = User.IsInRole("Admin") ? "Admin" : User.IsInRole("Agent") ? "Agent" : User.IsInRole("Secretary") ? "Secretary" : "";
            #endregion
            DDLServices ddlservices = new DDLServices();
            ViewBag.citylist = JsonConvert.SerializeObject(ddlservices.GetRegionDDL("").ddllist.ToList());
            ViewBag.banklist = JsonConvert.SerializeObject(ddlservices.GetBankDDL("", "bank").ddllist.ToList());

            if (!String.IsNullOrEmpty(FormID))
            {
                #region 取得資料
                var CaseData = formdb.ObjectForm.Where(x => x.FormID == FormID).FirstOrDefault();
                if (CaseData.ProcessAccount != User.Identity.Name && !IsAdmin)
                {
                    ///無編輯權限: 
                    ///非處理職員 且 非系統管理員
                    return RedirectToAction("CaseManage", ControllerName);
                }
                try
                {
                    var ExistTenantForm = formdb.Tenant.Where(x => x.FormID == FormID).FirstOrDefault();
                    ViewBag.Exist = ExistTenantForm != null ? "exist" : "noexist";
                    if (ExistTenantForm != null)
                    {
                        //存在房客資料，取出房客資料
                        #region 取得房客資料(地址)
                        //取得房東戶籍地址
                        string[] Addressparts = ExistTenantForm.Address.Split('-');    //地址
                        var AddressSplit_Tenant = new { City = Addressparts[0].ToString(), District = Addressparts[1].ToString(), Road = Addressparts[2].ToString(), };
                        var addressCode_Tenant = formdb.Region.Where(x => x.City == AddressSplit_Tenant.City && x.District == AddressSplit_Tenant.District && x.Road == AddressSplit_Tenant.Road).Select(x => x.RoadCode).FirstOrDefault();
                        //取得房客通訊地址
                        string[] Contactparts = ExistTenantForm.ContactAddress.Split('-');    //通訊地址
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
                        var MemberList = JsonConvert.DeserializeObject<List<string>>(ExistTenantForm.MemberArray);
                        var CoupleCount = Convert.ToInt32(MemberList[0]);   //配偶人數
                        var DirectCount = Convert.ToInt32(MemberList[1]);   //直系親屬人數
                        var AgentCount = Convert.ToInt32(MemberList[2]);    //代理人人數
                        var GuarantorCount = Convert.ToInt32(MemberList[3]);//保證人人數

                        #region 取得代理人資料(地址)
                        string[] agentarray1 = new string[7];
                        string[] agentarray2 = new string[7];
                        string[] agentarray3 = new string[7];
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
                                agentArrays[count] = JsonConvert.DeserializeObject<List<string>>(ExistTenantForm.GetType().GetProperty(agentProperty).GetValue(ExistTenantForm).ToString()).ToArray();

                                //將地址轉換成地址代碼
                                for (int arrayscount = 0; arrayscount < agentArrays[count].Count(); arrayscount++)
                                {
                                    //5:地址 // 7:通訊地址
                                    if (arrayscount == 3 || arrayscount == 5)
                                    {
                                        string[] Parts = agentArrays[count][arrayscount].Split('-');    //通訊地址
                                                                                                        //門牌地址 => 地址代碼
                                        var Split = new { City = Parts[0].ToString(), District = Parts[1].ToString(), Road = Parts[2].ToString(), };
                                        var co_addresscode = formdb.Region.Where(x => x.City == Split.City && x.District == Split.District && x.Road == Split.Road).Select(x => x.RoadCode).FirstOrDefault();
                                        agentArrays[count][arrayscount] = co_addresscode;

                                        //找到DDL並存入
                                        if (arrayscount == 3)
                                        {
                                            var splitaddress = new { City = Parts[0].ToString(), District = Parts[1].ToString(), Road = Parts[2].ToString(), };
                                            agent_AddressArraays[arraycount][0] = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(co_addresscode.Substring(0, 2)).ddllist.ToList());
                                            agent_AddressArraays[arraycount][1] = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(co_addresscode.Substring(0, 4)).ddllist.ToList());
                                        }
                                        if (arrayscount == 5)
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
                        string[] Guentarray1 = new string[7];
                        string[] Guentarray2 = new string[7];
                        string[] Guentarray3 = new string[7];
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
                                GuentArrays[count] = JsonConvert.DeserializeObject<List<string>>(ExistTenantForm.GetType().GetProperty(GuentProperty).GetValue(ExistTenantForm).ToString()).ToArray();

                                //將地址轉換成地址代碼
                                for (int arrayscount = 0; arrayscount < GuentArrays[count].Count(); arrayscount++)
                                {
                                    //5:地址 // 7:通訊地址
                                    if (arrayscount == 3 || arrayscount == 5)
                                    {
                                        string[] Parts = GuentArrays[count][arrayscount].Split('-');    //通訊地址
                                                                                                        //門牌地址 => 地址代碼
                                        var Split = new { City = Parts[0].ToString(), District = Parts[1].ToString(), Road = Parts[2].ToString(), };
                                        var co_addresscode = formdb.Region.Where(x => x.City == Split.City && x.District == Split.District && x.Road == Split.Road).Select(x => x.RoadCode).FirstOrDefault();
                                        GuentArrays[count][arrayscount] = co_addresscode;

                                        //找到DDL並存入
                                        if (arrayscount == 3)
                                        {
                                            var splitaddress = new { City = Parts[0].ToString(), District = Parts[1].ToString(), Road = Parts[2].ToString(), };
                                            Guent_AddressArraays[arraycount][0] = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(co_addresscode.Substring(0, 2)).ddllist.ToList());
                                            Guent_AddressArraays[arraycount][1] = JsonConvert.SerializeObject(ddlservices.GetRegionDDL(co_addresscode.Substring(0, 4)).ddllist.ToList());
                                        }
                                        if (arrayscount == 5)
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
                            TenantType = ExistTenantForm.TenantType,
                            Name = ExistTenantForm.Name,                  //房客姓名
                            Gender = ExistTenantForm.Gender,              //性別(0/1)
                            BirthdayStr = Convert.ToDateTime(ExistTenantForm.Birthday).ToString("yyyy-MM-dd"),     //房客生日
                            IDNumber = ExistTenantForm.IDNumber,          //身分證字號(統一編號)
                            Phone = ExistTenantForm.Phone,                //電話
                            BankNo = ExistTenantForm.BankNo,              //銀行代碼
                            BrancheNo = ExistTenantForm.BrancheNo,        //分行代碼
                            BankAccount = ExistTenantForm.BankAccount,    //銀行帳號

                            //地址下拉選單
                            TeAddress_District = Tenant_Address_Input[0],
                            TeAddress_Road = Tenant_Address_Input[1],
                            TeContact_District = Tenant_Address_Input[2],
                            TeContact_Road = Tenant_Address_Input[3],
                            AddressCode = addressCode_Tenant,           //地址代碼
                            AddressDetail = ExistTenantForm.AddressDetail,
                            ContactAddressCode = contactCode_Tenant,    //通訊地址代碼
                            ContactAddressDetail = ExistTenantForm.ContactAddressDetail,
                            accountNo = ExistTenantForm.accountNo,            //戶號

                            //MemberArray = ExistTenantForm.MemberArray,        //人數陣列[配偶, 直系, 代理人, 保證人]
                            CoupleCount = CoupleCount.ToString(),
                            Couple = ExistTenantForm.Couple,

                            DirectCount = DirectCount.ToString(),
                            Family1 = DirectCount >= 1 ? ExistTenantForm.Family1 : "[]",
                            Family2 = DirectCount >= 2 ? ExistTenantForm.Family2 : "[]",
                            Family3 = DirectCount >= 3 ? ExistTenantForm.Family3 : "[]",
                            Family4 = DirectCount >= 4 ? ExistTenantForm.Family4 : "[]",
                            Family5 = DirectCount >= 5 ? ExistTenantForm.Family5 : "[]",
                            Family6 = DirectCount >= 6 ? ExistTenantForm.Family6 : "[]",
                            Family7 = DirectCount >= 7 ? ExistTenantForm.Family7 : "[]",
                            Family8 = DirectCount >= 8 ? ExistTenantForm.Family8 : "[]",
                            Family9 = DirectCount >= 9 ? ExistTenantForm.Family9 : "[]",
                            Family10 = DirectCount >= 10 ? ExistTenantForm.Family10 : "[]",

                            AgentCount = AgentCount.ToString(),
                            Agent1 = JsonConvert.SerializeObject(agentArrays[0]),
                            Agent2 = JsonConvert.SerializeObject(agentArrays[1]),
                            Agent3 = JsonConvert.SerializeObject(agentArrays[2]),

                            GuarantorCount = GuarantorCount.ToString(),
                            Guarantor1 = JsonConvert.SerializeObject(GuentArrays[0]),
                            Guarantor2 = JsonConvert.SerializeObject(GuentArrays[1]),
                            Guarantor3 = JsonConvert.SerializeObject(GuentArrays[2]),

                            Memo = String.IsNullOrEmpty(ExistTenantForm.Memo) ? "" : ExistTenantForm.Memo.Replace("\r\n", "\\r\\n"),

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


                        //檔案資料(弱勢戶佐證文件)
                        ViewBag.vulnerablefile_NameList = ExistTenantForm.vulnerablefile_Name != null ? JsonConvert.DeserializeObject<List<string>>(ExistTenantForm.vulnerablefile_Name) : null;
                        ViewBag.vulnerablefile_AliasList = ExistTenantForm.vulnerablefile_Alias != null ? JsonConvert.DeserializeObject<List<string>>(ExistTenantForm.vulnerablefile_Alias) : null;

                        //檔案資料(上傳300億試算表截圖)
                        ViewBag.sheetfile_NameList = ExistTenantForm.sheetfile_Name != null ? JsonConvert.DeserializeObject<List<string>>(ExistTenantForm.sheetfile_Name) : null;
                        ViewBag.sheetfile_AliasList = ExistTenantForm.sheetfile_Alias != null ? JsonConvert.DeserializeObject<List<string>>(ExistTenantForm.sheetfile_Alias) : null;



                        initmodel.tenantviewmodel = FormValue;
                        return View(initmodel);
                    }
                    else
                    {
                        //無房客資料，創建新房客空表單
                        return View(initmodel);
                    }
                }
                catch (Exception ex)
                {
                    MailService mailService = new MailService();
                    mailService.SendMail("【力群管理系統】房客(進入頁面)錯誤", ex.ToString(), "cys.enki@gmail.com");
                    ViewBag.ErrorMessage = ex.Message;
                    return View(initmodel);
                }
                #endregion
            }

            //此案件編號不存在，請先新增物件資料
            return RedirectToAction("CaseManage", ControllerName);
        }

        [HttpPost]
        public ActionResult Tenant(TenantInputViewModel inputmodel, string ControllerName)
        {
            var initmodel = InitialModel();
            initmodel.FormID = inputmodel.FormID;
            ControllerName = ControllerName != null ? ControllerName : "Sales";     //是否秘書端進入
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
            var IsAdmin = User.IsInRole("Admin");
            ViewBag.Role = User.IsInRole("Admin") ? "Admin" : User.IsInRole("Agent") ? "Agent" : User.IsInRole("Secretary") ? "Secretary" : "";
            #endregion

            FileService fileService = new FileService();
            FileViewMode filemodel_vulnerablefile = new FileViewMode()
            {
                FileName = "[]",
                FileAlias = "[]",
            };
            #region 存檔(弱勢戶佐證文件)
            if (inputmodel.FormID != null && inputmodel.vulnerablefile != null && inputmodel.vulnerablefile.Any())
            {
                filemodel_vulnerablefile = fileService.SaveFile(inputmodel.FormID, "弱勢戶佐證文件", inputmodel.vulnerablefile, "Form", User.Identity.Name);

                if (filemodel_vulnerablefile == null)
                    return Content("上傳檔案錯誤，請重新選擇檔案，若問題未解決，請尋求系統管理員協助。");
            }
            #endregion


            FileViewMode filemodel_sheetfile = new FileViewMode()
            {
                FileName = "[]",
                FileAlias = "[]",
            };
            #region 存檔(上傳300億試算表截圖)
            if (inputmodel.FormID != null && inputmodel.sheetfile != null && inputmodel.sheetfile.Any())
            {
                filemodel_sheetfile = fileService.SaveFile(inputmodel.FormID, "上傳300億試算表截圖", inputmodel.sheetfile, "Form", User.Identity.Name);

                if (filemodel_sheetfile == null)
                    return Content("上傳檔案錯誤，請重新選擇檔案，若問題未解決，請尋求系統管理員協助。");
            }
            #endregion

            #region 存檔(弱勢戶)(過時)
            ////取得檔名與檔案GUID
            //List<string> vulnerablefileNameArray = new List<string>();
            //List<string> vulnerablefileAliasArray = new List<string>();
            ////存檔
            //if (inputmodel.vulnerablefile != null && inputmodel.vulnerablefile.Any())
            //{
            //    try
            //    {
            //        foreach (var file in inputmodel.vulnerablefile)
            //        {
            //            if (file != null && file.ContentLength > 0)
            //            {
            //                string name = Path.GetFileName(file.FileName);
            //                vulnerablefileNameArray.Add(name);
            //                string alias = Guid.NewGuid().ToString() + Path.GetExtension(name);
            //                vulnerablefileAliasArray.Add(alias);

            //                string path = Path.Combine(Server.MapPath("~/Uploads/TaxFile"), alias);
            //                file.SaveAs(path);
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        MailService mailService = new MailService();
            //        mailService.SendMail("【力群管理系統】房客資料(弱勢戶)存檔錯誤", ex.ToString(), "cys.enki@gmail.com");
            //        ViewBag.ErrorMessage = ex.Message;
            //        return View(initmodel);
            //    }
            //}
            //string vulnerablefileNames = JsonConvert.SerializeObject(vulnerablefileNameArray);
            //string vulnerablefileAlias = JsonConvert.SerializeObject(vulnerablefileAliasArray);
            #endregion

            #region 存檔(300E)(過時)
            ////取得檔名與檔案GUID
            //List<string> sheetfileNameArray = new List<string>();
            //List<string> sheetfileAliasArray = new List<string>();
            ////存檔
            //if (inputmodel.sheetfile != null && inputmodel.sheetfile.Any())
            //{
            //    try
            //    {
            //        foreach (var file in inputmodel.sheetfile)
            //        {
            //            if (file != null && file.ContentLength > 0)
            //            {
            //                string name = Path.GetFileName(file.FileName);
            //                sheetfileNameArray.Add(name);
            //                string alias = Guid.NewGuid().ToString() + Path.GetExtension(name);
            //                sheetfileAliasArray.Add(alias);

            //                string path = Path.Combine(Server.MapPath("~/Uploads/TaxFile"), alias);
            //                file.SaveAs(path);
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        MailService mailService = new MailService();
            //        mailService.SendMail("【力群管理系統】房客資料(300E)存檔錯誤", ex.ToString(), "cys.enki@gmail.com");
            //        ViewBag.ErrorMessage = ex.Message;
            //        return View(initmodel);
            //    }
            //}
            //string sheetfileNames = JsonConvert.SerializeObject(sheetfileNameArray);
            //string sheetfileAlias = JsonConvert.SerializeObject(sheetfileAliasArray);
            #endregion

            #region 轉換資料
            //轉換地址
            var addressdata = formdb.Region.Where(x => x.RoadCode == inputmodel.addressroad_0).FirstOrDefault();
            var Address = addressdata.City + "-" + addressdata.District + "-" + addressdata.Road;
            var ContactAddress = "";
            if (inputmodel.sameaddress_check_0 != null)
            {
                ContactAddress = Address;
                inputmodel.detailcontact_0 = inputmodel.AddressDetail_0;
            }
            else
            {
                var contactdata = formdb.Region.Where(x => x.RoadCode == inputmodel.contactroad_0).FirstOrDefault();
                ContactAddress = contactdata.City + "-" + contactdata.District + "-" + contactdata.Road;
            }

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

            string[] agentarray1 = new string[7];
            string[] agentarray2 = new string[7];
            string[] agentarray3 = new string[7];

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
                    if (i == 3 || i == 5)
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

            #region 打包保證人資料
            string[] guarantorarray1 = new string[7];
            string[] guarantorarray2 = new string[7];
            string[] guarantorarray3 = new string[7];

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
                    if (i == 3 || i == 5)
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
            #endregion

            #region 新增/編輯
            if (!String.IsNullOrEmpty(inputmodel.FormID))
            {
                var CaseData = formdb.ObjectForm.Where(x => x.FormID == inputmodel.FormID).FirstOrDefault();
                if (CaseData.ProcessAccount != User.Identity.Name && !IsAdmin)
                {
                    ///無編輯權限: 
                    ///非處理職員 且 非系統管理員
                    return RedirectToAction("CaseManage", ControllerName);
                }
                try
                {
                    var existTenantForm = formdb.Tenant.Where(x => x.FormID == inputmodel.FormID).FirstOrDefault();
                    if (existTenantForm == null)
                    {
                        //房客資料不存在，創建新房客資料
                        using (var context = new FormModels())
                        {
                            var newTenantData = new Tenant
                            {
                                FormID = inputmodel.FormID,
                                TenantType = Convert.ToInt32(inputmodel.typeRadio),
                                vulnerablefile_Name = filemodel_vulnerablefile.FileName,
                                vulnerablefile_Alias = filemodel_vulnerablefile.FileAlias,
                                sheetfile_Name = filemodel_sheetfile.FileName,
                                sheetfile_Alias = filemodel_sheetfile.FileAlias,
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

                                CreateAccount = User.Identity.Name,
                                CreateTime = DateTime.Now,
                                UpdateAccount = User.Identity.Name,
                                UpdateTime = DateTime.Now,
                                Memo = inputmodel.memo,
                            };

                            context.Tenant.Add(newTenantData);
                            context.SaveChanges();
                        }
                    }
                    else
                    {
                        //已存在房客資料，編輯房客資料

                        using (var context = new FormModels())
                        {
                            var existformdata = context.Tenant.Where(x => x.FormID == inputmodel.FormID).FirstOrDefault();

                            existformdata.TenantType = Convert.ToInt32(inputmodel.typeRadio);
                            existformdata.vulnerablefile_Name = filemodel_vulnerablefile.FileName;
                            existformdata.vulnerablefile_Alias = filemodel_vulnerablefile.FileAlias;
                            existformdata.sheetfile_Name = filemodel_sheetfile.FileName;
                            existformdata.sheetfile_Alias = filemodel_sheetfile.FileAlias;
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

                            existformdata.UpdateAccount = User.Identity.Name;
                            existformdata.UpdateTime = DateTime.Now;
                            existformdata.Memo = inputmodel.memo;

                            context.SaveChanges();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MailService mailService = new MailService();
                    mailService.SendMail("【力群管理系統】房客資料(新增/編輯)錯誤", ex.ToString(), "cys.enki@gmail.com");
                    ViewBag.ErrorMessage = ex.Message;
                    return View(initmodel); //返回表單頁面
                }
            }
            #endregion

            //此案件編號不存在，請先新增物件資料
            return RedirectToAction("CaseManage", ControllerName);
        }
        #endregion

        #region 秘書填寫
        [HttpGet]
        public ActionResult Secretary(string FormID)
        {
            var initmodel = InitialModel();
            initmodel.FormID = FormID != null ? FormID : "";
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
            var IsAdmin = User.IsInRole("Admin");
            ViewBag.Role = User.IsInRole("Admin") ? "Admin" : User.IsInRole("Agent") ? "Agent" : User.IsInRole("Secretary") ? "Secretary" : "";
            #endregion

            #region 存在表單資料
            if (!String.IsNullOrEmpty(FormID))
            {
                var notarization = formdb.HomeObject.Where(x => x.FormID == FormID).FirstOrDefault().notarization;
                try
                {
                    var CaseData = formdb.ObjectForm.Where(x => x.FormID == FormID).FirstOrDefault();
                    if (CaseData.ProcessAccount != User.Identity.Name && !IsAdmin)
                    {
                        //無編輯權限
                        return RedirectToAction("CaseManage", "Secretary");
                    }
                    var existSecretaryForm = formdb.Secretary.Where(x => x.FormID == FormID).FirstOrDefault();
                    if(existSecretaryForm != null)
                    {
                        //已存在填寫資料，取出至頁面
                        var FormValue = new SecretaryViewModel
                        {
                            qualifyRadio = existSecretaryForm.qualifyRadio,
                            excerpt = existSecretaryForm.excerpt,
                            excerptShort = existSecretaryForm.excerptShort,
                            buildNo = existSecretaryForm.buildNo,
                            placeNo = existSecretaryForm.placeNo,
                            buildCreateDate = Convert.ToDateTime(existSecretaryForm.buildCreateDate).ToString("yyyy-MM-dd"),
                            floorAmount = existSecretaryForm.floorAmount,
                            floorNo = existSecretaryForm.floorNo,
                            squareAmount = existSecretaryForm.squareAmount,
                            pinAmount = existSecretaryForm.pinAmount,
                            notarizationFeeRadio = (int)(notarization != null ? notarization : 0),
                            rentMarket = existSecretaryForm.rentMarket,
                            rentAgent = existSecretaryForm.rentAgent,
                            depositAgent = existSecretaryForm.depositAgent,
                            Memo = existSecretaryForm.Memo,
                        };
                        initmodel.secretaryviewmodel = FormValue;
                        return View(initmodel);
                    }
                    else
                    {
                        //初次進入秘書填寫頁面，取得是否公證，並預設公證負擔人
                        var FormValue = new SecretaryViewModel
                        {
                            notarizationFeeRadio = (int)(notarization != null ? notarization : 0),
                        };
                        initmodel.secretaryviewmodel = FormValue;
                        return View(initmodel);
                    }
                }
                catch(Exception ex)
                {
                    MailService mailService = new MailService();
                    mailService.SendMail("【力群管理系統】秘書填寫(進入頁面)錯誤", ex.ToString(), "cys.enki@gmail.com");
                    TempData["ErrorMessage"] = ex.Message;
                    return RedirectToAction("CaseManage", "Secretary");
                }
            }
            #endregion
            //案件編號不存在，請先新增物件資料
            return RedirectToAction("CaseManage", "Secretary");
        }
        [HttpPost]
        public ActionResult Secretary(SecretaryInputViewModel inputmodel)
        {
            var initmodel = InitialModel();
            initmodel.FormID = inputmodel.FormID;
            #region 使用者資料
            ViewBag.Role = User.IsInRole("Admin") ? "Admin" : User.IsInRole("Agent") ? "Agent" : User.IsInRole("Secretary") ? "Secretary" : "";
            #endregion

            try
            {
                var existSecretaryForm = formdb.Secretary.Where(x => x.FormID == inputmodel.FormID).FirstOrDefault();
                //var builddatetime = Convert.ToDateTime(inputmodel.buildCreatedate);
                var buildDateTime = Convert.ToDateTime((Convert.ToInt32(inputmodel.buildCreatedate.Split('-')[0]) + 1911).ToString() + "-" + inputmodel.buildCreatedate.Split('-')[1] + "-" + inputmodel.buildCreatedate.Split('-')[2]);
                if (existSecretaryForm == null)
                {
                    //初次新增秘書填寫，創建新資料
                    using (var context = new FormModels())
                    {
                        var newData = new Secretary
                        {
                            FormID = inputmodel.FormID,
                            qualifyRadio = Convert.ToInt32(inputmodel.qualifyRadio),
                            excerpt = inputmodel.excerpt,
                            excerptShort = inputmodel.excerptShort,
                            buildNo = inputmodel.buildNo,
                            placeNo = inputmodel.placeNo,
                            buildCreateDate = buildDateTime,
                            floorAmount = inputmodel.floorAmount,
                            floorNo =  inputmodel.floorNo,
                            squareAmount = inputmodel.squareAmount,
                            pinAmount = inputmodel.pinAmount,
                            notarizationFeeRadio = inputmodel.notarizationFeeRadio,
                            rentMarket = inputmodel.rentMarket,
                            rentAgent = inputmodel.rentAgent,
                            depositAgent = inputmodel.depositAgent,

                            CreateAccount = User.Identity.Name,
                            CreateTime = DateTime.Now,
                            UpdateAccount = User.Identity.Name,
                            UpdateTime = DateTime.Now,
                            Memo = inputmodel.memo,
                        };

                        context.Secretary.Add(newData);
                        context.SaveChanges();
                    }
                }
                else
                {
                    //已存在秘書填寫資料，編輯表單
                    using (var context = new FormModels())
                    {
                        var existformdata = context.Secretary.Where(x => x.FormID == inputmodel.FormID).FirstOrDefault();

                        existformdata.qualifyRadio = Convert.ToInt32(inputmodel.qualifyRadio);
                        existformdata.excerpt = inputmodel.excerpt;
                        existformdata.excerptShort = inputmodel.excerptShort;
                        existformdata.buildNo = inputmodel.buildNo;
                        existformdata.placeNo = inputmodel.placeNo;
                        existformdata.buildCreateDate = buildDateTime;
                        existformdata.floorAmount = inputmodel.floorAmount;
                        existformdata.floorNo = inputmodel.floorNo;
                        existformdata.squareAmount = inputmodel.squareAmount;
                        existformdata.pinAmount = inputmodel.pinAmount;
                        existformdata.notarizationFeeRadio = inputmodel.notarizationFeeRadio;
                        existformdata.rentMarket = inputmodel.rentMarket;
                        existformdata.rentAgent = inputmodel.rentAgent;
                        existformdata.depositAgent = inputmodel.depositAgent;

                        existformdata.UpdateAccount = User.Identity.Name;
                        existformdata.UpdateTime = DateTime.Now;
                        existformdata.Memo = inputmodel.memo;

                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                MailService mailService = new MailService();
                mailService.SendMail("【力群管理系統】秘書填寫資料(新增/編輯)錯誤", ex.ToString(), "cys.enki@gmail.com");
                ViewBag.ErrorMessage = ex.Message;
                return View(initmodel);
            }
            return RedirectToAction("CaseManage", "Secretary");
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
            if (banklength != null)
            {
                mindata = banklength.minlength.Split(',');
                int datacount = mindata.Length;
                if (mindata.Length == 1)
                {
                    mindata = new string[0];
                }
            }


            var result = new
            {
                lengthset = mindata,
                minlength = banklength != null ? banklength.minlength : "0",
                maxlength = banklength != null ? banklength.maxlength : "0",
                bankJson = bankJson
            };


            return Json(result);
        }
        #endregion

        #region 上傳資料區
        [HttpGet]
        public ActionResult Uploads(string FormID, string ControllerName)
        {
            var initmodel = InitialModel();
            initmodel.FormID = FormID != null ? FormID : "";
            ControllerName = ControllerName != null ? "Secretary" : "Sales";     //是否秘書端進入
            ViewBag.ControllerName = ControllerName;

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
            var IsAdmin = User.IsInRole("Admin");
            ViewBag.Role = User.IsInRole("Admin") ? "Admin" : User.IsInRole("Agent") ? "Agent" : User.IsInRole("Secretary") ? "Secretary" : "";
            #endregion

            #region 取得資料
            if (!String.IsNullOrEmpty(FormID))
            {
                var CaseData = formdb.ObjectForm.Where(x => x.FormID == FormID).FirstOrDefault();
                if (CaseData.ProcessAccount != User.Identity.Name && !IsAdmin)
                {
                    ///無編輯權限
                    ///非處理職員 且 非系統管理員
                    return RedirectToAction("CaseManage", ControllerName);
                }
                try
                {
                    initmodel.Uploads = new Uploads();
                    var existFileData = formdb.FileLog.Where(x => x.FormID == FormID).FirstOrDefault();
                    if (existFileData != null)
                    {
                        var FileData = formdb.FileLog.Where(x => x.FormID == FormID).AsEnumerable();;
                        #region 證件資料區
                        //正面身分證(房東)
                        initmodel.Uploads.frontIdentityCardLandlord = from file in FileData.Where(x => x.Category == "房東正面身分證")
                                                                      select new FileViewMode
                                                                      {
                                                                          FileName = file?.FileNames,
                                                                          FileAlias = file?.FileAlias,
                                                                      };
                        //反面身分證(房東)
                        initmodel.Uploads.reverseIdentityCardLandlord = from file in FileData.Where(x => x.Category == "房東反面身分證")
                                                                        select new FileViewMode
                                                                        {
                                                                            FileName = file?.FileNames,
                                                                            FileAlias = file?.FileAlias,
                                                                        };
                        //存摺(房東)
                        initmodel.Uploads.passbookLandlord = from file in FileData.Where(x => x.Category == "房東存摺")
                                                             select new FileViewMode
                                                             {
                                                                 FileName = file?.FileNames,
                                                                 FileAlias = file?.FileAlias,
                                                             };

                        //正面身分證(房客)
                        initmodel.Uploads.frontIdentityCardTenant = from file in FileData.Where(x => x.Category == "房客正面身分證")
                                                                    select new FileViewMode
                                                                    {
                                                                        FileName = file?.FileNames,
                                                                        FileAlias = file?.FileAlias,
                                                                    };

                        //反面身分證(房客)
                        initmodel.Uploads.reverseIdentityCardTenant = from file in FileData.Where(x => x.Category == "房客反面身分證")
                                                                      select new FileViewMode
                                                                      {
                                                                          FileName = file?.FileNames,
                                                                          FileAlias = file?.FileAlias,
                                                                      };

                        //存摺(房客)
                        initmodel.Uploads.passbookTenant = from file in FileData.Where(x => x.Category == "房客存摺")
                                                           select new FileViewMode
                                                           {
                                                               FileName = file?.FileNames,
                                                               FileAlias = file?.FileAlias,
                                                           };
                        //戶籍謄本(房客)
                        initmodel.Uploads.householdTenant = from file in FileData.Where(x => x.Category == "房客戶籍謄本")
                                                            select new FileViewMode
                                                            {
                                                                FileName = file?.FileNames,
                                                                FileAlias = file?.FileAlias,
                                                            };
                        //財產清單(房客)
                        initmodel.Uploads.propertyTenant = from file in FileData.Where(x => x.Category == "房客財產清單")
                                                           select new FileViewMode
                                                           {
                                                               FileName = file?.FileNames,
                                                               FileAlias = file?.FileAlias,
                                                           };
                        //所得清單(房客)
                        initmodel.Uploads.incomeTenant = from file in FileData.Where(x => x.Category == "房客所得清單")
                                                         select new FileViewMode
                                                         {
                                                             FileName = file?.FileNames,
                                                             FileAlias = file?.FileAlias,
                                                         };
                        //證件補充資料
                        initmodel.Uploads.documentSupplement = from file in FileData.Where(x => x.Category == "證件補充資料")
                                                               select new FileViewMode
                                                               {
                                                                   FileName = file?.FileNames,
                                                                   FileAlias = file?.FileAlias,
                                                               };
                        #endregion

                        #region 屋況資料區
                        //門牌
                        initmodel.Uploads.housenumber = from file in FileData.Where(x => x.Category == "門牌")
                                                        select new FileViewMode
                                                        {
                                                            FileName = file?.FileNames,
                                                            FileAlias = file?.FileAlias,
                                                        };
                        //大門
                        initmodel.Uploads.door = from file in FileData.Where(x => x.Category == "大門")
                                                 select new FileViewMode
                                                 {
                                                     FileName = file?.FileNames,
                                                     FileAlias = file?.FileAlias,
                                                 };
                        //衛浴設備
                        initmodel.Uploads.bath = from file in FileData.Where(x => x.Category == "衛浴設備")
                                                 select new FileViewMode
                                                 {
                                                     FileName = file?.FileNames,
                                                     FileAlias = file?.FileAlias,
                                                 };
                        //物件大門
                        initmodel.Uploads.objectdoor = from file in FileData.Where(x => x.Category == "物件大門")
                                                       select new FileViewMode
                                                       {
                                                           FileName = file?.FileNames,
                                                           FileAlias = file?.FileAlias,
                                                       };

                        //樓梯照
                        initmodel.Uploads.stairs = from file in FileData.Where(x => x.Category == "樓梯照")
                                                   select new FileViewMode
                                                   {
                                                       FileName = file?.FileNames,
                                                       FileAlias = file?.FileAlias,
                                                   };

                        //滅火器
                        initmodel.Uploads.fire = from file in FileData.Where(x => x.Category == "滅火器")
                                                 select new FileViewMode
                                                 {
                                                     FileName = file?.FileNames,
                                                     FileAlias = file?.FileAlias,
                                                 };

                        //偵煙器
                        initmodel.Uploads.smoke = from file in FileData.Where(x => x.Category == "偵煙器")
                                                  select new FileViewMode
                                                  {
                                                      FileName = file?.FileNames,
                                                      FileAlias = file?.FileAlias,
                                                  };
                        //熱水器
                        initmodel.Uploads.water = from file in FileData.Where(x => x.Category == "熱水器")
                                                  select new FileViewMode
                                                  {
                                                      FileName = file?.FileNames,
                                                      FileAlias = file?.FileAlias,
                                                  };
                        //補充資料
                        initmodel.Uploads.additional = from file in FileData.Where(x => x.Category == "補充資料")
                                                       select new FileViewMode
                                                       {
                                                           FileName = file?.FileNames,
                                                           FileAlias = file?.FileAlias,
                                                       };

                        return View(initmodel);
                        #endregion
                    }
                    else
                    {
                        initmodel.Uploads.frontIdentityCardLandlord = null;
                        initmodel.Uploads.reverseIdentityCardLandlord = null;
                        initmodel.Uploads.passbookLandlord = null;
                        initmodel.Uploads.frontIdentityCardTenant = null;
                        initmodel.Uploads.reverseIdentityCardTenant = null;
                        initmodel.Uploads.passbookTenant = null;
                        initmodel.Uploads.householdTenant = null;
                        initmodel.Uploads.propertyTenant = null;
                        initmodel.Uploads.incomeTenant = null;
                        initmodel.Uploads.documentSupplement = null;

                        initmodel.Uploads.housenumber = null;
                        initmodel.Uploads.door = null;
                        initmodel.Uploads.bath = null;
                        initmodel.Uploads.objectdoor = null;
                        initmodel.Uploads.stairs = null;
                        initmodel.Uploads.fire = null;
                        initmodel.Uploads.smoke = null;
                        initmodel.Uploads.water = null;
                        initmodel.Uploads.additional = null;
                        return View(initmodel);
                    }
                }
                catch(Exception ex)
                {
                    MailService mailService = new MailService();
                    mailService.SendMail("【力群管理系統】資料上傳區(進入頁面)錯誤", ex.ToString(), "cys.enki@gmail.com");
                    TempData["ErrorMessage"] = ex.Message;
                    return RedirectToAction("CaseManage", ControllerName);
                }

            }
            #endregion

            //無案件編號，請先建立物件資料
            return RedirectToAction("CaseManage", ControllerName);
        }
        #endregion

        #region 檔案操作
        [HttpPost]
        public ActionResult Uploads(string FormID, IEnumerable<HttpPostedFileBase> File, string Category, string Path)
        {
            FileService fileService = new FileService();

            var upload = fileService.SaveFile(FormID, Category, File, Path, User.Identity.Name);
            if(upload == null)
            {
                return Content("檔案上傳失敗，請尋求系統管理員協助。");
            }
            return Json("Success");
        }


        [HttpPost]
        //檔案下載
        public ActionResult Download(string FileAlias, string Path)
        {
            //找到GUID名稱
            var FileName = formdb.FileLog.Where(x => x.FileAlias == FileAlias).Select(x=> x.FileNames).FirstOrDefault();

            if(FileAlias != null)
            {
                //選擇要下載之資料夾位址
                var Map = "~/Uploads/" + Path + "/" + FileAlias;

                //將相對路徑轉為絕對路徑
                var path = Server.MapPath(Map);

                var file = new FileInfo(path);
                //設定檔案下載的 ContentType
                if (file.Exists)
                {
                    //設定檔案下載的 ContentType
                    var ContentType = MimeMapping.GetMimeMapping(file.FullName);
                    var fileBytes = System.IO.File.ReadAllBytes(file.FullName);
                    return File(fileBytes, ContentType, FileName);
                }
            }
            //檔案不存在時的處理
            return Json("檔案不存在");
        }

        [HttpPost]
        //(Ajax)檔案刪除
        public JsonResult DeleteFile(string FormID, string FileAlias, string FileType)
        {
            FileService fileService = new FileService();
            var response = "nodata";
            switch (FileType)
            {
                case "taxfile":
                    var ExistFormData = formdb.HomeObject.Where(x => x.FormID == FormID).FirstOrDefault();
                    if (ExistFormData != null)
                    {
                        List<string> FileNameList = JsonConvert.DeserializeObject<List<string>>(ExistFormData.taxfile_name);
                        List<string> FileAliasList = JsonConvert.DeserializeObject<List<string>>(ExistFormData.taxfile_alias);

                        for (int i = 0; i < FileAliasList.Count; i++)
                        {
                            if (FileAliasList[i] == FileAlias)
                            {
                                #region 刪除檔案
                                //刪除檔案
                                var deletestatus = fileService.DeleteFile(FileAlias);
                                #endregion
                                FileNameList.Remove(FileNameList[i]);
                                FileAliasList.Remove(FileAliasList[i]);

                                var jsonnamelist = JsonConvert.SerializeObject(FileNameList);
                                var jsonaliaslist = JsonConvert.SerializeObject(FileAliasList);

                                ExistFormData.taxfile_name = jsonnamelist;
                                ExistFormData.taxfile_alias = jsonaliaslist;
                                formdb.SaveChanges();
                                if (FileNameList.Count != 0)
                                {
                                    var result = new
                                    {
                                        FileName = jsonnamelist,
                                        File_Name_Alias = jsonaliaslist
                                    };
                                    return Json(result);
                                }
                            }
                        }

                    }

                    break;

                //弱勢戶佐證文件
                case "filenameSpace1":
                    response = "nodata1";
                    var ExistFormData1 = formdb.Tenant.Where(x => x.FormID == FormID).FirstOrDefault();
                    if (ExistFormData1 != null)
                    {
                        List<string> FileNameList = JsonConvert.DeserializeObject<List<string>>(ExistFormData1.vulnerablefile_Name);
                        List<string> FileAliasList = JsonConvert.DeserializeObject<List<string>>(ExistFormData1.vulnerablefile_Alias);

                        for (int i = 0; i < FileAliasList.Count; i++)
                        {
                            if (FileAliasList[i] == FileAlias)
                            {
                                #region 刪除檔案
                                //刪除檔案
                                var deletestatus = fileService.DeleteFile(FileAlias);
                                #endregion
                                FileNameList.Remove(FileNameList[i]);
                                FileAliasList.Remove(FileAliasList[i]);

                                var jsonnamelist = JsonConvert.SerializeObject(FileNameList);
                                var jsonaliaslist = JsonConvert.SerializeObject(FileAliasList);

                                ExistFormData1.vulnerablefile_Name = jsonnamelist;
                                ExistFormData1.vulnerablefile_Alias = jsonaliaslist;
                                formdb.SaveChanges();
                                if (FileNameList.Count != 0)
                                {
                                    var result = new
                                    {
                                        FileName = jsonnamelist,
                                        File_Name_Alias = jsonaliaslist
                                    };
                                    return Json(result);
                                }
                            }
                        }

                    }

                    break;

                //上傳300億試算表截圖
                case "filenameSpace2":
                    response = "nodata2";
                    var ExistFormData2 = formdb.Tenant.Where(x => x.FormID == FormID).FirstOrDefault();
                    if (ExistFormData2 != null)
                    {
                        List<string> FileNameList = JsonConvert.DeserializeObject<List<string>>(ExistFormData2.sheetfile_Name);
                        List<string> FileAliasList = JsonConvert.DeserializeObject<List<string>>(ExistFormData2.sheetfile_Alias);

                        for (int i = 0; i < FileAliasList.Count; i++)
                        {
                            if (FileAliasList[i] == FileAlias)
                            {
                                #region 刪除檔案
                                //刪除檔案
                                var deletestatus = fileService.DeleteFile(FileAlias);
                                #endregion
                                FileNameList.Remove(FileNameList[i]);
                                FileAliasList.Remove(FileAliasList[i]);

                                var jsonnamelist = JsonConvert.SerializeObject(FileNameList);
                                var jsonaliaslist = JsonConvert.SerializeObject(FileAliasList);

                                ExistFormData2.sheetfile_Name = jsonnamelist;
                                ExistFormData2.sheetfile_Alias = jsonaliaslist;
                                formdb.SaveChanges();
                                if (FileNameList.Count != 0)
                                {
                                    var result = new
                                    {
                                        FileName = jsonnamelist,
                                        File_Name_Alias = jsonaliaslist
                                    };
                                    return Json(result);
                                }
                            }
                        }

                    }
                    break;

                default:
                    var trydeletefile = fileService.DeleteFile(FileAlias);
                    if(trydeletefile)
                        response = "success";

                    break;
            }
            return Json(response);
        }
        #endregion
    }
}