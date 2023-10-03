using LiqunManagement.Models;
using LiqunManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
//using DocumentFormat.OpenXml.Packaging;
//using DocumentFormat.OpenXml.Wordprocessing;
using System.Threading;
using Xceed; // 引用DocX庫的命名空間
using Xceed.Words.NET;
using NPOI.Util;
using System.Reflection;
using System.Text.RegularExpressions;
using static System.Net.WebRequestMethods;

namespace LiqunManagement.Controllers
{
    public class SecretaryController : BaseController
    {
        // GET: Secretary
        public ActionResult Index()
        {
            return View();
        }

        #region 管理案件(秘書)
        [HttpGet]
        public ActionResult CaseManage()
        {
            var ErrorMessage = TempData["ErrorMessage"];
            ViewBag.ErrorMessage = ErrorMessage != null ? ErrorMessage.ToString() : "";
            ViewBag.DownLoadMessage = TempData["DownLoadMessage"] != null ? TempData["DownLoadMessage"] : "";
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
            ViewBag.Role = User.IsInRole("Admin") ? "Admin" : User.IsInRole("Agent") ? "Agent" : User.IsInRole("Secretary") ? "Secretary" : "";
            #endregion

            var ObjectFormData = (from objform in formdb.ObjectForm.Where(x => x.FormType == 1)
                                  select new objectFormViewModel
                                  {
                                      FormID = objform.FormID,
                                      CreateTime = (DateTime)objform.CreateTime,
                                      ProcessName = objform.ProcessName,
                                      ProcessAccount = objform.ProcessAccount,
                                      AgentAccount = objform.AgentAccount,
                                  }).AsEnumerable();
            if (!role)
            {
                //非系統管理員，找出該業務員的專門秘書(助理)為業務員
                ObjectFormData = (from objform in ObjectFormData.Where(x => x.ProcessAccount == User.Identity.Name)
                                  join emp in memberdb.EmployeeData on objform.AgentAccount equals emp.Account into emptemp
                                  from emp0 in emptemp.DefaultIfEmpty()
                                  select new objectFormViewModel
                                  {
                                      FormID = (string)objform.FormID,
                                      AgentAccount = objform.AgentAccount,
                                      CreateTime = (DateTime)objform.CreateTime,
                                      ProcessName = (string)objform.ProcessName,
                                      AssistantAccount = emp0 != null ? emp0.AssistantAccount != null ? emp0.AssistantAccount : objform.ProcessAccount : objform.ProcessAccount,
                                  }).Where(x => x.AssistantAccount == User.Identity.Name).AsEnumerable();
            }
            
            var Formlist = from form in ObjectFormData
                           join obj in formdb.HomeObject on form.FormID equals obj.FormID
                           join lan in formdb.LandLord on form.FormID equals lan.FormID into temp1
                           from land in temp1.DefaultIfEmpty()
                           join ten in formdb.Tenant on form.FormID equals ten.FormID into temp2
                           from tena in temp2.DefaultIfEmpty()
                           select new objectFormViewModel
                           {
                               FormID = (string)form.FormID,
                               CreateTime = (DateTime)form.CreateTime,
                               ProcessName = (string)form.ProcessName,
                               Address = (string)obj.fulladdress,
                               SignDate = (DateTime)obj.signdate,
                               Landlord = land != null ? land.Name : null,
                               Tenant = tena != null ? tena.Name : null,
                           };
            //ViewBag.Formlist = Formlist;
            var model = new FormViewModels
            {
                objectformlist = Formlist,
            };

            return View(model);
        }
        [HttpPost]
        public ActionResult CaseManage(string FormID)
        {
            try
            {
                //變更表單狀態
                using (var context = new FormModels())
                {
                    //秘書領取表單
                    var existform = context.ObjectForm.Where(x => x.FormID == FormID).FirstOrDefault();
                    existform.AgentAccount = User.Identity.Name;
                    existform.FormType = 2;

                    context.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                var error = ex.ToString();
            }
            return Json("successed");
        }
        #endregion

        //[HttpPost]
        #region 匯出(過時/因雲端無法使用Window office套件)
        //public ActionResult ExportWord(string FormID)
        //{
        //    logger.Info("匯出Word表單");

        //    try
        //    {
        //        logger.Info("創建一個 Word.Application 對象");
        //        // 創建一個 Word.Application 對象
        //        Application wordApp = new Application();

        //        logger.Info("找到是否都有填寫完成");
        //        var HomeObject = formdb.HomeObject.Where(x => x.FormID == FormID).FirstOrDefault();
        //        var Landlord = formdb.LandLord.Where(x => x.FormID == FormID).FirstOrDefault();
        //        var Tenant = formdb.Tenant.Where(x => x.FormID == FormID).FirstOrDefault();
        //        var Secretary = formdb.Secretary.Where(x => x.FormID == FormID).FirstOrDefault();
        //        if (HomeObject == null || Landlord == null || Tenant == null || Secretary == null)
        //        {
        //            logger.Info("未填寫完成");
        //            TempData["DownLoadMessage"] = "檔案未填寫完成，請填寫完成後再執行匯出。";
        //            return RedirectToAction("CaseManage", "Secretary");
        //        }

        //        //劃選
        //        string unicodeSquare = Char.ConvertFromUtf32(0x25A1); // □
        //        string unicodeBlackSquare = Char.ConvertFromUtf32(0x25A0); // ■

        //        // 要處理的Word文件列表
        //        List<string> wordFilesToProcess = new List<string>
        //        {
        //            "表單1.出租人出租住宅申請書.docx",
        //            "表單2.屋況及租屋安全檢核表(租賃標的現況確認書)-代租.docx",
        //            "表單3.出租人補助費用申請書.docx",
        //            "表單5.民眾(房客)承租住宅申請書.docx"
        //        };
        //        #region 處理資料
        //        //將需要替換的文字全部寫入
        //        //物件資料
        //        var signdatetime = Convert.ToDateTime(HomeObject.signdate);

        //        logger.Info("處理資料");
        //        //房東資料
        //        var naturepeople = Landlord.Gender != 2 ? true : false;     //自然人/法人
        //        var birthLandlord = Convert.ToDateTime(Landlord.Birthday);
        //        var sameaddressLandlord = (Landlord.Address + Landlord.AddressDetail) == (Landlord.ContactAddress + Landlord.ContactAddressDetail) ? true : false;
        //        var MemberArray = JsonConvert.DeserializeObject<List<int>>(Landlord.MemberArray);
        //        var CoOwnerCount = MemberArray[0];
        //        var AgentCount = MemberArray[1];
        //        var AgentArray = AgentCount > 0 ? JsonConvert.DeserializeObject<List<string>>(Landlord.Agent) : new List<string>();
        //        var BankName = formdb.Bank.Where(x => x.BankCode == Landlord.BankNo).Select(x => x.BankName).FirstOrDefault();
        //        var BranchName = formdb.Bank.Where(x => x.BranchCode == Landlord.BrancheNo).Select(x => x.BranchName).FirstOrDefault();
        //        var FormData = new
        //        {
        //            //【物件資料】
        //            addressObject = HomeObject.fulladdress,
        //            objecttypeA = HomeObject.objecttype == 0 ? unicodeBlackSquare : unicodeSquare,      //包租
        //            objecttypeB = HomeObject.objecttype == 1 ? unicodeBlackSquare : unicodeSquare,      //代管
        //            notarizationA = HomeObject.objecttype == 1 && HomeObject.notarization == 0 ? unicodeBlackSquare : unicodeSquare,  //公證(只有代租才需要)
        //            notarizationB = HomeObject.objecttype == 1 && HomeObject.notarization == 1 ? unicodeBlackSquare : unicodeSquare,  //無公證(只有代租才需要)
        //            signdate = (signdatetime.Year - 1911) + signdatetime.ToString("-MM-dd"),                //簽約日
        //            appraiser = HomeObject.appraiser == 1 ? unicodeBlackSquare : unicodeSquare,       //有簽估價師
        //            existagentLandlord = AgentCount > 0 ? unicodeBlackSquare : unicodeSquare,                  //有共有人
        //            existcoownerLandlord = CoOwnerCount > 0 ? unicodeBlackSquare : unicodeSquare,                  //有共有人
        //            usefor_Home = HomeObject.usefor == 0 ? unicodeBlackSquare : unicodeSquare,
        //            usefor_Commercial = HomeObject.usefor == 1 ? unicodeBlackSquare : unicodeSquare,





        //            //【房東資料】
        //            //自然人
        //            nameLandlord = naturepeople ? Landlord.Name : "",          //房東姓名
        //            genderLandlord = naturepeople ? Landlord.Gender == 0 ? "女性" : "男性" : "",      //房東性別
        //            idLandlord = naturepeople ? Landlord.IDNumber : "",  //房東身分證
        //            birthLandlord = naturepeople ? (birthLandlord.Year - 1911) + birthLandlord.ToString("年MM月dd日") : "",     //房東生日
        //            phoneLandlord = naturepeople ? Landlord.Phone : "",        //房東手機
        //            addressLandlord = naturepeople ? Landlord.Address + Landlord.AddressDetail : "",                //戶籍地址
        //            sameaddressLandlord = naturepeople && sameaddressLandlord ? unicodeBlackSquare : unicodeSquare,
        //            contactLandlord = naturepeople ? sameaddressLandlord ? "" : Landlord.ContactAddress + Landlord.ContactAddressDetail : "",  //通訊地址

        //            BankCode = Landlord.BankNo,         //銀行代碼
        //            BankName = BankName,                //銀行名稱
        //            BranchCode = Landlord.BrancheNo,      //分行代碼
        //            BranchName = BranchName,            //分行明成
        //            BankAccount = Landlord.BankAccount, //帳戶號碼
        //                                                //私法人
        //            nameCompany = !naturepeople ? Landlord.Name : "",          //法人姓名
        //            namePrincipal = !naturepeople ? Landlord.Principal : "",   //負責人姓名
        //            idCompany = !naturepeople ? Landlord.IDNumber : "",        //統一編號
        //            phoneCompany = !naturepeople ? Landlord.Phone : "",        //法人電話號碼
        //            addressCompany = !naturepeople ? Landlord.Address + Landlord.AddressDetail : "",                //戶籍地址
        //            nameCompanyAgent = !naturepeople && AgentCount > 0 ? AgentArray[0] : "",    //法人代理人姓名
        //            idCompanyAgent = !naturepeople && AgentCount > 0 ? AgentArray[1] : "",      //法人代理人身份證字號
        //            phoneCompanyAgent = !naturepeople && AgentCount > 0 ? AgentArray[2] : "",   //法人代理人電話號碼
        //            addressCompanyAgent = !naturepeople && AgentCount > 0 ? AgentArray[3] + AgentArray[4] : "",    //法人代理人戶籍地址

        //            //房客資料
        //            tenantType2 = Tenant.TenantType == 2 ? unicodeBlackSquare : unicodeSquare,     //是否為一般戶最後一項

        //            //秘書填寫
        //            transition2 = Secretary.qualifyRadio == 2 ? unicodeBlackSquare : unicodeSquare,     //是否為轉軌2
        //            excerpt = Secretary.excerpt,                //段
        //            excerptShort = Secretary.excerptShort,      //小段
        //            buildNo = Secretary.buildNo,                //建號
        //            floorAmount = Secretary.floorAmount,        //層數
        //            floorNo = Secretary.floorNo,                //層次
        //            buildCreateDate = (Secretary.buildCreateDate.Year - 1911) + Secretary.buildCreateDate.ToString("年MM月dd日"),    //建築完成日
        //            squareAmount = Secretary.squareAmount,
        //            pinAmount = Secretary.pinAmount,
        //            rentMarket = Secretary.rentMarket,
        //            rentAgentA = HomeObject.objecttype == 0 ? Secretary.rentAgent.ToString() : "",
        //            rentAgentB = HomeObject.objecttype == 1 ? Secretary.rentAgent.ToString() : "",
        //            depositAgent = Secretary.depositAgent,


        //        };
        //        #endregion
        //        logger.Info("完成處理資料，開始匯出表單");


        //        logger.Info("創建一個唯一的臨時文件夾");
        //        // 創建一個唯一的臨時文件夾
        //        string tempFolderPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        //        Directory.CreateDirectory(tempFolderPath);
        //        logger.Info("轉換文字");

        //        foreach (string fileName in wordFilesToProcess)
        //        {
        //            // 組合Word文件的完整路徑
        //            string filePath = Path.Combine(Server.MapPath("~/WordSample"), fileName);

        //            // 打開指定的Word文件
        //            Document doc = wordApp.Documents.Open(filePath);

        //            ///上面的代碼中，
        //            ///Find 方法用於在 Range 對象中查找指定的文字，
        //            ///Execute 方法用於執行查找和替換操作。
        //            ///其中的 FindText 參數用於指定要查找的文字，
        //            ///ReplaceWith 參數用於指定要替換的新文字，
        //            ///Replace 參數用於指定替換的類型（例如，是否匹配大小寫等）。
        //            switch (fileName)
        //            {
        //                case "表單1.出租人出租住宅申請書.docx":

        //                    // 遍歷 Range 對象，進行文字替換
        //                    foreach (Range range in doc.StoryRanges)
        //                    {
        //                        range.Find.Execute(FindText: "«房東姓名»", ReplaceWith: FormData.nameLandlord, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«房東身分證字號»", ReplaceWith: FormData.idLandlord, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«房東性別»", ReplaceWith: FormData.genderLandlord, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«房東生日»", ReplaceWith: FormData.birthLandlord, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«房東電話»", ReplaceWith: FormData.phoneLandlord, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«房東戶籍地址»", ReplaceWith: FormData.addressLandlord, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«房東同戶籍地址»", ReplaceWith: FormData.sameaddressLandlord, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«房東通訊地址»", ReplaceWith: FormData.contactLandlord, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«房東金融機構»", ReplaceWith: FormData.BankName, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«房東銀行代碼»", ReplaceWith: FormData.BankCode, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«房東分行戶名»", ReplaceWith: FormData.BranchName, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«房東分行代碼»", ReplaceWith: FormData.BranchCode, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«房東帳號»", ReplaceWith: FormData.BankAccount, Replace: WdReplace.wdReplaceAll);

        //                        range.Find.Execute(FindText: "«法人姓名»", ReplaceWith: FormData.nameCompany, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«法人負責人»", ReplaceWith: FormData.namePrincipal, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«法人統一編號»", ReplaceWith: FormData.idCompany, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«法人電話»", ReplaceWith: FormData.phoneCompany, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«法人戶籍地址»", ReplaceWith: FormData.addressCompany, Replace: WdReplace.wdReplaceAll);

        //                        range.Find.Execute(FindText: "«法人代理人姓名»", ReplaceWith: FormData.nameCompanyAgent, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«法人代理人身分證»", ReplaceWith: FormData.idCompanyAgent, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«法人代理人戶籍地址»", ReplaceWith: FormData.addressCompanyAgent, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«法人代理人電話»", ReplaceWith: FormData.phoneCompanyAgent, Replace: WdReplace.wdReplaceAll);

        //                        range.Find.Execute(FindText: "«建物門牌»", ReplaceWith: FormData.addressObject, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«段»", ReplaceWith: FormData.excerpt, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«小段»", ReplaceWith: FormData.excerptShort, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«建號»", ReplaceWith: FormData.buildNo, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«層數»", ReplaceWith: FormData.floorAmount, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«層次»", ReplaceWith: FormData.floorNo, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«建築完成日»", ReplaceWith: FormData.buildCreateDate, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«平方»", ReplaceWith: FormData.squareAmount, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«坪»", ReplaceWith: FormData.pinAmount, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«市場租金»", ReplaceWith: FormData.rentMarket, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«待租押金»", ReplaceWith: FormData.depositAgent, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«待租租金A»", ReplaceWith: FormData.rentAgentA, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«待租租金B»", ReplaceWith: FormData.rentAgentB, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«包租»", ReplaceWith: FormData.objecttypeA, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«代租»", ReplaceWith: FormData.objecttypeB, Replace: WdReplace.wdReplaceAll); range.Find.Execute(FindText: "«A»", ReplaceWith: FormData.objecttypeA, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«公證»", ReplaceWith: FormData.notarizationA, Replace: WdReplace.wdReplaceAll); range.Find.Execute(FindText: "«A»", ReplaceWith: FormData.objecttypeA, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«非公證»", ReplaceWith: FormData.notarizationB, Replace: WdReplace.wdReplaceAll);

        //                        range.Find.Execute(FindText: "«appraiser»", ReplaceWith: FormData.appraiser, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«existagentLandlord»", ReplaceWith: FormData.existagentLandlord, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«existcoownerLandlord»", ReplaceWith: FormData.existcoownerLandlord, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«transition2»", ReplaceWith: FormData.transition2, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«tenantType2»", ReplaceWith: FormData.tenantType2, Replace: WdReplace.wdReplaceAll);


        //                        range.Find.Execute(FindText: "«住家用»", ReplaceWith: FormData.usefor_Home, Replace: WdReplace.wdReplaceAll);
        //                        range.Find.Execute(FindText: "«商業用»", ReplaceWith: FormData.usefor_Commercial, Replace: WdReplace.wdReplaceAll);
        //                    }
        //                    break;
        //                case "表單2.屋況及租屋安全檢核表(租賃標的現況確認書)-代租.docx":
        //                    break;
        //                case "表單3.出租人補助費用申請書.docx":
        //                    break;
        //                case "表單5.民眾(房客)承租住宅申請書.docx":
        //                    break;
        //            }

        //            // 決定新的文件名，可以根據需要進行命名
        //            string newFileName = $"Processed_{fileName}";

        //            // 保存修改後的Word文件到tempFolderPath
        //            string tempFilePath = Path.Combine(tempFolderPath, newFileName);
        //            doc.SaveAs2(tempFilePath);

        //            // 關閉Word文件
        //            doc.Close();
        //        }

        //        // 關閉 Word.Application 對象
        //        wordApp.Quit();
        //        logger.Info("轉換文字成功");


        //        logger.Info("開始壓縮檔案");
        //        string zipFileName = "WordFiles" + DateTime.Now.ToString("yyMMddHHmm") + ".zip";
        //        // 壓縮暫存文件夾中的所有文件到一個ZIP文件
        //        string zipFilePath = Path.Combine(Path.GetTempPath(), zipFileName);
        //        ZipFile.CreateFromDirectory(tempFolderPath, zipFilePath);

        //        logger.Info("壓縮成功");
        //        // 刪除暫存文件夾
        //        Directory.Delete(tempFolderPath, true);

        //        logger.Info("輸出資料");
        //        // 將ZIP文件發送給用戶進行下載
        //        byte[] zipFileBytes = System.IO.File.ReadAllBytes(zipFilePath);
        //        return File(zipFileBytes, "application/zip", zipFileName);
        //    }
        //    catch(Exception ex)
        //    {
        //        logger.Error("匯出失敗 : " + ex.ToString());
        //        return Content(ex.ToString());
        //    }
        //}
        #endregion

        [HttpPost]
        [Obsolete]
        #region 匯出
        public ActionResult ExportWord(string FormID)
        {
            logger.Info("匯出Word表單");

            try
            {
                //特殊符號
                string unicodeSquare = Char.ConvertFromUtf32(0x25A1);       // □
                string unicodeBlackSquare = Char.ConvertFromUtf32(0x25A0);  // ■
                string unicodeCheck = Char.ConvertFromUtf32(0x2713);        // ✓

                var HomeObject = formdb.HomeObject.Where(x => x.FormID == FormID).FirstOrDefault();
                var Landlord = formdb.LandLord.Where(x => x.FormID == FormID).FirstOrDefault();
                var Tenant = formdb.Tenant.Where(x => x.FormID == FormID).FirstOrDefault();
                var Secretary = formdb.Secretary.Where(x => x.FormID == FormID).FirstOrDefault();
                if (HomeObject == null || Landlord == null || Tenant == null || Secretary == null)
                {
                    TempData["DownLoadMessage"] = "檔案未填寫完成，請填寫完成後再執行匯出。";
                    return RedirectToAction("CaseManage", "Secretary");
                }

                //表單業務員
                var agentaccount = formdb.ObjectForm.Where(x => x.FormID == FormID).Select(x => x.AgentAccount).FirstOrDefault();
                var formagentName = memberdb.Members.Where(x => x.Account == agentaccount).Select(x => x.Name).FirstOrDefault();
               
                #region 處理資料
                //將需要替換的文字全部寫入

                //物件資料
                var Accessory = JsonConvert.DeserializeObject<List<int>>(HomeObject.Accessory);         //房屋附屬物件
                var ParkArray = JsonConvert.DeserializeObject<List<int>>(HomeObject.havepark);          //停車位
                var ParkPosition = JsonConvert.DeserializeObject<List<int>>(HomeObject.parkfloor);      //停車位
                var MorParkPosition = JsonConvert.DeserializeObject<List<int>>(HomeObject.scooterparkfloor);      //停車位
                var roomAmountArray = JsonConvert.DeserializeObject<List<int>>(HomeObject.roomamount);      //房型

                //房東資料
                var naturepeople = Landlord.Gender != 2 ? true : false;     //自然人/法人
                var birthLandlord = Convert.ToDateTime(Landlord.Birthday);   //房東生日
                var sameaddressLandlord = (Landlord.Address + Landlord.AddressDetail) == (Landlord.ContactAddress + Landlord.ContactAddressDetail) ? true : false;  //是否同戶籍地(房東)
                var MemberLandlordArray = JsonConvert.DeserializeObject<List<int>>(Landlord.MemberArray);   //成員人數資料(陣列)
                var CoOwnerCount = MemberLandlordArray[0];  //共有人人數
                var AgentCount = MemberLandlordArray[1];    //代理人人數
                var AgentArray = AgentCount > 0 ? JsonConvert.DeserializeObject<List<string>>(Landlord.Agent) : new List<string>();     //代理人資料(陣列)
                var banknameLandlord = formdb.Bank.Where(x => x.BankCode == Landlord.BankNo).Select(x => x.BankName).FirstOrDefault();          //銀行名稱
                var branchLandlord = formdb.Bank.Where(x => x.BranchCode == Landlord.BrancheNo).Select(x => x.BranchName).FirstOrDefault(); //分行名稱
                var addressLandlord = Landlord.Address.Split('-')[0] + Landlord.Address.Split('-')[1] + Landlord.Address.Split('-')[2] + Landlord.AddressDetail; //戶籍地址(自然人/法人)
                var contactLandlord = Landlord.ContactAddress.Split('-')[0] + Landlord.ContactAddress.Split('-')[1] + Landlord.ContactAddress.Split('-')[2] + Landlord.ContactAddressDetail; //通訊地址(自然人)
                var addressCompanyAgent = AgentCount > 0 ? AgentArray[3].Split('-')[0] + AgentArray[3].Split('-')[1] + AgentArray[3].Split('-')[2] + AgentArray[4] : "";  //地址(法人代理人)
                var contactCompanyAgent = AgentCount > 0 ? AgentArray[5].Split('-')[0] + AgentArray[5].Split('-')[1] + AgentArray[5].Split('-')[2] + AgentArray[6] : "";  //地址(法人代理人)
                //房客資料
                var birthTenant = Convert.ToDateTime(Tenant.Birthday);  //房客生日
                var addressTenant = Tenant.Address.Split('-')[0] + Tenant.Address.Split('-')[1] + Tenant.Address.Split('-')[2] + Tenant.AddressDetail; //戶籍地址(房客)
                var contactTenant = Tenant.ContactAddress.Split('-')[0] + Tenant.ContactAddress.Split('-')[1] + Tenant.ContactAddress.Split('-')[2] + Tenant.ContactAddressDetail; //通訊地址(房客)
                var sameaddressTenant = (Tenant.Address + Tenant.AddressDetail) == (Tenant.ContactAddress + Tenant.ContactAddressDetail) ? true : false;  //是否同戶籍地(房客)
                var banknameTenant = formdb.Bank.Where(x => x.BankCode == Tenant.BankNo).Select(x => x.BankName).FirstOrDefault();          //銀行名稱(房客)
                var branchTenant = formdb.Bank.Where(x => x.BranchCode == Tenant.BrancheNo).Select(x => x.BranchName).FirstOrDefault();     //分行名稱(房客)

                var memberTenantArray = JsonConvert.DeserializeObject<List<int>>(Tenant.MemberArray);   //成員人數資料(陣列)[配偶,直系,代理人,共有人]
                var coupleTenantCount = memberTenantArray[0];
                var directTenantCount = memberTenantArray[1];
                var agentTenantCount = memberTenantArray[2];
                var gaurantorTenantCount = memberTenantArray[3];
                var coupleTenantArray = coupleTenantCount > 0 ? JsonConvert.DeserializeObject<List<string>>(Tenant.Couple) : new List<string>();    //配偶(陣列)[性別,姓名,身分證,生日,分戶戶號]

                var FormData = new
                {
                    #region 物件資料
                    //【物件資料】
                    addressObject = HomeObject.fulladdress,
                    objecttypeA = HomeObject.objecttype == 0 ? unicodeBlackSquare : unicodeSquare,      //包租(劃選)
                    objecttypeB = HomeObject.objecttype == 1 ? unicodeBlackSquare : unicodeSquare,      //代管(劃選)
                    notarizationA = HomeObject.objecttype == 1 && HomeObject.notarization == 1 ? unicodeBlackSquare : unicodeSquare,  //公證(只有代租才需要)(劃選)
                    notarizationB = HomeObject.objecttype == 1 && HomeObject.notarization == 0 ? unicodeBlackSquare : unicodeSquare,  //無公證(只有代租才需要)(劃選)
                    //signdate = (signdatetime.Year - 1911) + signdatetime.ToString("-MM-dd"),            //簽約日
                    rent = string.Format("{0:N0}", HomeObject.rent),        //租金
                    deposit = string.Format("{0:N0}", HomeObject.deposit), //押金
                    depositPerMonth = (HomeObject.deposit / HomeObject.rent)?.ToString("F2"),  //押金為幾個月 的租金 => 租金/押金
                    paydate = HomeObject.paydate.ToString(),    //繳租日
                    //建物型態
                    buildtype0 = HomeObject.buildtype == 0 ? unicodeBlackSquare : unicodeSquare,    //透天厝
                    buildtype1 = HomeObject.buildtype == 1 ? unicodeBlackSquare : unicodeSquare,    //公寓
                    buildtype2 = HomeObject.buildtype == 2 ? unicodeBlackSquare : unicodeSquare,    //華夏
                    buildtype3 = HomeObject.buildtype == 3 ? unicodeBlackSquare : unicodeSquare,    //電梯大樓
                    //建物現有格局
                    roomtype = HomeObject.roomtype == 0 ? unicodeBlackSquare : unicodeSquare,     //為整層出租，有隔間
                    roomAmount = roomAmountArray[0] == 0 && roomAmountArray[1] == 0 && roomAmountArray[2] == 0 ? "套" : roomAmountArray[0].ToString(), //房
                    rubyAmount = roomAmountArray[0] == 0 && roomAmountArray[1] == 0 && roomAmountArray[2] == 0 ? "X" : roomAmountArray[1].ToString(), //房
                    bathAmount = roomAmountArray[0] == 0 && roomAmountArray[1] == 0 && roomAmountArray[2] == 0 ? "X" : roomAmountArray[2].ToString(), //房
                    //管理費
                    existManageGroup = HomeObject.buildtype == 3 || HomeObject.management_fee > 0 ? true : false,      //有無管委會(有管理費 or 電梯大樓 => 一定有)
                    existManageFee = HomeObject.management_fee > 0 ? "有" : HomeObject.buildtype == 3 ? "無" : "空",        //管理費是否要選擇(有管理費:有管理費， 無管理費: 電梯大樓選無， 其他不選擇(選空)
                    //劃選
                    existappraiser = HomeObject.appraiser == 1 ? true : false,      //是否有估價師
                    appraiser = HomeObject.appraiser == 1 ? unicodeBlackSquare : unicodeSquare,         //有簽估價師(劃選)
                    usefor_Home = HomeObject.usefor == 0 ? unicodeBlackSquare : unicodeSquare,          //是否住家用途(劃選)
                    usefor_Homeexcept = HomeObject.usefor != 0 ? unicodeBlackSquare : unicodeSquare,    //是否商業用途(劃選)
                    managecommitteeA = HomeObject.management_fee > 0 ? unicodeBlackSquare : unicodeSquare,    //有管理委員會(用管理費是否為0來判斷)
                    managecommitteeB = HomeObject.management_fee == 0 ? unicodeBlackSquare : unicodeSquare,    //無管理委員會(用管理費是否為0來判斷)
                    management_fee_PerPin = HomeObject.management_fee > 0 ? Convert.ToDouble(HomeObject.management_fee / Secretary.pinAmount).ToString("0.00") : "",
                    management_fee = HomeObject.management_fee > 0 ? string.Format("{0:N0}", HomeObject.management_fee) : "", //管理費

                    //房屋附屬物件(設備)
                    GAS = Accessory[0] > 0 ? unicodeBlackSquare : unicodeSquare,        //天然瓦斯
                    TVFOUR = Accessory[1] > 0 ? unicodeBlackSquare : unicodeSquare,     //第四台
                    NonLineNet = Accessory[2] > 0 ? unicodeBlackSquare : unicodeSquare, //無線網路

                    electricwater = Accessory[3] > 0 ? unicodeBlackSquare : unicodeSquare,  //電熱水器(劃選)
                    gaswater = Accessory[3] == 0 ? unicodeBlackSquare : unicodeSquare,  //瓦斯熱水器(劃選)
                    indoorgas = Accessory[3] == 0 && Accessory[4] > 0 ? unicodeBlackSquare : unicodeSquare,  //室內瓦斯熱水器(劃選)
                    outdoorgas = Accessory[3] == 0 && Accessory[5] > 0 ? unicodeBlackSquare : unicodeSquare,  //室外瓦斯熱水器(劃選)
                    //房屋附屬物件(家電)
                    B1 = new { Square = Accessory[6] != 0 ? unicodeBlackSquare : unicodeSquare, Amount =  Accessory[6] == 0 ? " " : Accessory[6].ToString() },       //電視
                    B2 = new { Square = Accessory[7] != 0 ? unicodeBlackSquare : unicodeSquare, Amount =  Accessory[7] == 0 ? " " : Accessory[7].ToString() },       //冷氣
                    B3 = new { Square = Accessory[8] != 0 ? unicodeBlackSquare : unicodeSquare, Amount =  Accessory[8] == 0 ? " " : Accessory[8].ToString() },       //冰箱
                    B4 = new { Square = Accessory[9] != 0 ? unicodeBlackSquare : unicodeSquare, Amount =  Accessory[9] == 0 ? " " : Accessory[9].ToString() },       //熱水器
                    B5 = new { Square = Accessory[10] != 0 ? unicodeBlackSquare : unicodeSquare, Amount = Accessory[10] == 0 ? " " :  Accessory[10].ToString() },    //洗衣機
                    B6 = new { Square = Accessory[11] != 0 ? unicodeBlackSquare : unicodeSquare, Amount = Accessory[11] == 0 ? " " :  Accessory[11].ToString() },    //微波爐
                    B7 = new { Square = Accessory[12] != 0 ? unicodeBlackSquare : unicodeSquare, Amount = Accessory[12] == 0 ? " " :  Accessory[12].ToString() },    //洗碗機
                    B8 = new { Square = Accessory[13] != 0 ? unicodeBlackSquare : unicodeSquare, Amount = Accessory[13] == 0 ? " " :  Accessory[13].ToString() },    //排油煙機
                    B9 = new { Square = Accessory[14] != 0 ? unicodeBlackSquare : unicodeSquare, Amount = Accessory[14] == 0 ? " " :  Accessory[14].ToString() },    //瓦斯爐
                    //房屋附屬物件(家具)
                    C1 = new { Square = Accessory[15] != 0 ? unicodeBlackSquare : unicodeSquare, Amount = Accessory[15] == 0 ? " " :  Accessory[15].ToString() },    //鞋櫃
                    C2 = new { Square = Accessory[16] != 0 ? unicodeBlackSquare : unicodeSquare, Amount = Accessory[16] == 0 ? " " :  Accessory[16].ToString() },    //餐桌
                    C3 = new { Square = Accessory[17] != 0 ? unicodeBlackSquare : unicodeSquare, Amount = Accessory[17] == 0 ? " " :  Accessory[17].ToString() },    //餐桌椅
                    C4 = new { Square = Accessory[18] != 0 ? unicodeBlackSquare : unicodeSquare, Amount = Accessory[18] == 0 ? " " :  Accessory[18].ToString() },    //沙發
                    C5 = new { Square = Accessory[19] != 0 ? unicodeBlackSquare : unicodeSquare, Amount = Accessory[19] == 0 ? " " :  Accessory[19].ToString() },    //電視櫃
                    C6 = new { Square = Accessory[20] != 0 ? unicodeBlackSquare : unicodeSquare, Amount = Accessory[20] == 0 ? " " :  Accessory[20].ToString() },    //茶几
                    C7 = new { Square = Accessory[21] != 0 ? unicodeBlackSquare : unicodeSquare, Amount = Accessory[21] == 0 ? " " :  Accessory[21].ToString() },    //床組(頭)
                    C8 = new { Square = Accessory[22] != 0 ? unicodeBlackSquare : unicodeSquare, Amount = Accessory[22] == 0 ? " " :  Accessory[22].ToString() },    //衣櫃
                    C9 = new { Square = Accessory[23] != 0 ? unicodeBlackSquare : unicodeSquare, Amount = Accessory[23] == 0 ? " " :  Accessory[23].ToString() },    //置物櫃
                    C10 = new { Square = Accessory[24] != 0 ? unicodeBlackSquare : unicodeSquare, Amount = Accessory[24] == 0 ? " " : Accessory[24].ToString() },    //梳妝台
                    C11 = new { Square = Accessory[25] != 0 ? unicodeBlackSquare : unicodeSquare, Amount = Accessory[25] == 0 ? " " : Accessory[25].ToString() },    //書櫃
                    C12 = new { Square = Accessory[26] != 0 ? unicodeBlackSquare : unicodeSquare, Amount = Accessory[26] == 0 ? " " : Accessory[26].ToString() },    //書桌
                    C13 = new { Square = Accessory[27] != 0 ? unicodeBlackSquare : unicodeSquare, Amount = Accessory[27] == 0 ? " " : Accessory[27].ToString() },    //書桌椅

                    //汽車車位
                    havecarpark = ParkArray[1] > 0 ? unicodeBlackSquare : unicodeSquare,   //有停車位
                    nocarpark = ParkArray[1] == 0 ? unicodeBlackSquare : unicodeSquare,   //無停車位
                    parktypeA = ParkArray[1] > 0 && HomeObject.parktype == 0 ? unicodeBlackSquare : unicodeSquare,   //坡道平面
                    parktypeB = ParkArray[1] > 0 && HomeObject.parktype == 1 ? unicodeBlackSquare : unicodeSquare,   //坡道機械
                    parktypeC = ParkArray[1] > 0 && HomeObject.parktype == 2 ? unicodeBlackSquare : unicodeSquare,   //機械平面
                    parktypeD = ParkArray[1] > 0 && HomeObject.parktype == 3 ? unicodeBlackSquare : unicodeSquare,   //機械機械
                    parkposition = ParkArray[1] > 0 ? ParkPosition[0] == 1 ? "上" : "下" : "",        //位於地(上/下)
                    parkfloor = ParkArray[1] > 0 ? ParkPosition[1].ToString() : "",     //車位於幾層
                    carpositionnumber = ParkArray[1] > 0 ? HomeObject.carpositionnumber : "",     //車位編號
                    carparkmanagefee = ParkArray[1] > 0 ? string.Format("{0:N0}", HomeObject.carparkmanagefee) : "", //停車管理費
                    //機車車位
                    havemorpark = ParkArray[2] > 0 ? unicodeBlackSquare : unicodeSquare,    //有停車位
                    nomorpark = ParkArray[2] == 0 ? unicodeBlackSquare : unicodeSquare,     //有停車位
                    morparkposition = ParkArray[2] > 0 ? MorParkPosition[0] == 1 ? "上" : "下" : "",  //位於地(上/下)
                    morparkfloor = ParkArray[2] > 0 ? MorParkPosition[1].ToString() : "",  //位於地(上/下)
                    scooterpositionnumber = ParkArray[2] > 0 ? HomeObject.scooterpositionnumber : "", //機車位編號
                    scootermanagefee = ParkArray[2] > 0 ? string.Format("{0:N0}", HomeObject.scootermanagefee) : "", //停車管理費

                    totalparkManagefee = (HomeObject.carparkmanagefee + HomeObject.scootermanagefee) > 0 ? string.Format("{0:N0}", HomeObject.carparkmanagefee + HomeObject.scootermanagefee) : "",
                    totalparkRent = string.Format("{0:N0}", HomeObject.carmonthrent + HomeObject.scootermonthrent),

                    totalRent = string.Format("{0:N0}", HomeObject.rent + HomeObject.carmonthrent + HomeObject.scootermonthrent),
                    #endregion

                    #region 房東資料
                    //【房東資料】
                    //自然人
                    nameLandlord = naturepeople ? Landlord.Name : "",                                   //房東姓名
                    genderLandlord = naturepeople ? Landlord.Gender == 1 ? unicodeBlackSquare + "男" + unicodeSquare + "女" : unicodeSquare + "男" + unicodeBlackSquare + "女" : unicodeSquare + "男" + unicodeSquare + "女",         //房東性別
                    idLandlord = naturepeople ? Landlord.IDNumber : "",                                 //房東身分證
                    birthLandlord = naturepeople ? (birthLandlord.Year - 1911) + birthLandlord.ToString("年MM月dd日") : "",     //房東生日
                    phoneLandlord = naturepeople ? Regex.Replace(Landlord.Phone, @"(\d{4})(\d{3})(\d{3})", "$1-$2-$3") : "",                                 //房東手機
                    addressLandlord = naturepeople ? addressLandlord : "",                              //戶籍地址
                    sameaddressLandlord = naturepeople && sameaddressLandlord ? unicodeBlackSquare : unicodeSquare, //是否同戶籍地址(房東)(劃選)
                    contactLandlord = naturepeople && !sameaddressLandlord ? contactLandlord : "",  //通訊地址
                    nameLandlordAgent = AgentCount > 0 ? AgentArray[0] : "",            //房東代理人姓名
                    idLandlordAgent = AgentCount > 0 ? AgentArray[1] : "",              //房東代理人身份證字號
                    phoneLandlordAgent = AgentCount > 0 ? Regex.Replace(AgentArray[2], @"(\d{4})(\d{3})(\d{3})", "$1-$2-$3") : "",           //房東代理人電話號碼
                    addressLandlordAgent = AgentCount > 0 ? addressCompanyAgent : "",   //房東代理人戶籍地址
                    contactLandlordAgent = AgentCount > 0 ? contactCompanyAgent : "",   //房東代理人通訊地址

                    bankcodeLandlord = Landlord.BankNo,             //房東銀行代碼
                    banknameLandlord = banknameLandlord,            //房東銀行名稱
                    branchcodeLandlord = Landlord.BrancheNo,        //房東分行代碼
                    branchLandlord = branchLandlord,                //房東分行明成
                    BankAccount = Landlord.BankAccount,     //帳戶號碼

                    //私法人
                    nameCompany = !naturepeople ? Landlord.Name : "",               //法人姓名
                    namePrincipal = !naturepeople ? Landlord.Principal : "",        //負責人姓名
                    idCompany = !naturepeople ? Landlord.IDNumber : "",             //統一編號
                    phoneCompany = !naturepeople ? Regex.Replace(Landlord.Phone, @"(\d{4})(\d{3})(\d{3})", "$1-$2-$3") : "",             //法人電話號碼
                    addressCompany = !naturepeople ? addressLandlord : "",     //戶籍地址
                    nameCompanyAgent = !naturepeople && AgentCount > 0 ? AgentArray[0] : "",            //法人代理人姓名
                    idCompanyAgent = !naturepeople && AgentCount > 0 ? AgentArray[1] : "",              //法人代理人身份證字號
                    phoneCompanyAgent = !naturepeople && AgentCount > 0 ? Regex.Replace(AgentArray[2], @"(\d{4})(\d{3})(\d{3})", "$1-$2-$3") : "",           //法人代理人電話號碼
                    addressCompanyAgent = !naturepeople && AgentCount > 0 ? addressCompanyAgent : "",   //法人代理人戶籍地址
                    contactCompanyAgent = !naturepeople && AgentCount > 0 ? contactCompanyAgent : "",   //法人代理人通訊地址
                    iscompany = !naturepeople ? "V" : "",                                               //是否為法人(勾選)
                    existagentLandlord = AgentCount > 0 ? unicodeBlackSquare : unicodeSquare,                        //有房東代理人(劃選)
                    existcoownerLandlord = CoOwnerCount > 0 ? unicodeBlackSquare : unicodeSquare,                    //有共有人(劃選)

                    existagentLandlordorTenant = AgentCount > 0 || agentTenantCount >= 1 ? unicodeBlackSquare : unicodeSquare,  //有房東代理人或房客代理人
                    //保證人
                    CoOwner1 = CoOwnerCount >= 1 ? JsonConvert.DeserializeObject<List<string>>(Landlord.CoOwner1) : new List<string>(),
                    CoOwner2 = CoOwnerCount >= 2 ? JsonConvert.DeserializeObject<List<string>>(Landlord.CoOwner2) : new List<string>(),
                    CoOwner3 = CoOwnerCount >= 3 ? JsonConvert.DeserializeObject<List<string>>(Landlord.CoOwner3) : new List<string>(),
                    CoOwner4 = CoOwnerCount >= 4 ? JsonConvert.DeserializeObject<List<string>>(Landlord.CoOwner4) : new List<string>(),

                    #endregion

                    #region 房客資料
                    //【房客資料】
                    nameTenant = Tenant.Name,                                                       //房客姓名
                    genderTenantA = Tenant.Gender == 1 ? unicodeBlackSquare : unicodeSquare,      //房客性別(男)
                    genderTenantB = Tenant.Gender == 0 ? unicodeBlackSquare : unicodeSquare,      //房客性別(女)
                    idTenant = Tenant.IDNumber,                                                     //房客身分證
                    birthTenant = (birthTenant.Year - 1911) + birthTenant.ToString("年MM月dd日"),     //房客生日
                    phoneTenant = Regex.Replace(Tenant.Phone, @"(\d{4})(\d{3})(\d{3})", "$1-$2-$3"),                                                     //房客手機
                    addressTenant = addressTenant,                                                  //戶籍地址
                    sameaddressTenant = sameaddressTenant ? unicodeBlackSquare : unicodeSquare,     //是否同戶籍地址(房東)(劃選)
                    contactTenant = !sameaddressTenant ? contactTenant : "",                         //通訊地址
                    accountNo = Tenant.accountNo,
                    accountNoArray = Tenant.accountNo.ToCharArray(),                                     //戶口名簿戶號(戶號)
                    bankcodeTenant = Tenant.BankNo,             //房客銀行代碼
                    banknameTenant = banknameTenant,            //房客銀行名稱
                    branchcodeTenant = Tenant.BrancheNo,        //房客分行代碼
                    branchTenant = branchTenant,                //房客分行明成
                    coupleaccountNoArray = coupleTenantCount > 0 ? !String.IsNullOrEmpty(coupleTenantArray[4]) ? coupleTenantArray[4].ToCharArray() : null : null,
                    //直系
                    Family1 = directTenantCount >= 1 ? JsonConvert.DeserializeObject<List<string>>(Tenant.Family1) : new List<string>(),
                    Family2 = directTenantCount >= 2 ? JsonConvert.DeserializeObject<List<string>>(Tenant.Family2) : new List<string>(),
                    Family3 = directTenantCount >= 3 ? JsonConvert.DeserializeObject<List<string>>(Tenant.Family3) : new List<string>(),
                    Family4 = directTenantCount >= 4 ? JsonConvert.DeserializeObject<List<string>>(Tenant.Family4) : new List<string>(),
                    Family5 = directTenantCount >= 5 ? JsonConvert.DeserializeObject<List<string>>(Tenant.Family5) : new List<string>(),
                    Family6 = directTenantCount >= 6 ? JsonConvert.DeserializeObject<List<string>>(Tenant.Family6) : new List<string>(),
                    Family7 = directTenantCount >= 7 ? JsonConvert.DeserializeObject<List<string>>(Tenant.Family7) : new List<string>(),
                    Family8 = directTenantCount >= 8 ? JsonConvert.DeserializeObject<List<string>>(Tenant.Family8) : new List<string>(),
                    Family9 = directTenantCount >= 9 ? JsonConvert.DeserializeObject<List<string>>(Tenant.Family9) : new List<string>(),
                    //代理人
                    existagentTenant = agentTenantCount >= 1 ? unicodeBlackSquare : unicodeSquare,         //有房客代理人(劃選)
                    agentTenant1 = agentTenantCount >= 1 ? JsonConvert.DeserializeObject<List<string>>(Tenant.Agent1) : new List<string>(),
                    agentTenant2 = agentTenantCount >= 2 ? JsonConvert.DeserializeObject<List<string>>(Tenant.Agent2) : new List<string>(),
                    agentTenant3 = agentTenantCount >= 3 ? JsonConvert.DeserializeObject<List<string>>(Tenant.Agent3) : new List<string>(),
                    //保證人
                    guarantorTenant1 = gaurantorTenantCount >= 1 ? JsonConvert.DeserializeObject<List<string>>(Tenant.Guarantor1) : new List<string>(),
                    guarantorTenant2 = gaurantorTenantCount >= 2 ? JsonConvert.DeserializeObject<List<string>>(Tenant.Guarantor2) : new List<string>(),
                    guarantorTenant3 = gaurantorTenantCount >= 3 ? JsonConvert.DeserializeObject<List<string>>(Tenant.Guarantor3) : new List<string>(),

                    //劃選
                    tenantType = Tenant.TenantType,
                    tenantType2 = Tenant.TenantType == 2 ? unicodeBlackSquare : unicodeSquare,      //是否為一般戶最後一項(申請人為身心障礙者或65歲以上老人，申請換居並將自有住宅出租。)
                    #endregion

                    #region 秘書填寫
                    //【秘書填寫】
                    excerpt = Secretary.excerpt,                            //段
                    excerptShort = Secretary.excerptShort != null ? Secretary.excerptShort : "",                  //小段
                    buildNo = Secretary.buildNo,                            //建號
                    placeNo = Secretary.placeNo,                            //地號
                    floorAmount = Secretary.floorAmount.ToString(),         //層數
                    floorNo = Secretary.floorNo.ToString(),                 //層次
                    buildCreateDate = (Secretary.buildCreateDate.Year - 1911) + Secretary.buildCreateDate.ToString("年MM月dd日"),    //建築完成日
                    squareAmount = Secretary.squareAmount.ToString(),       //平方公尺
                    pinAmount = Secretary.pinAmount.ToString(),             //坪數
                    rentMarket = string.Format("{0:N0}", Secretary.rentMarket),           //市場租金
                    rentAgentA = HomeObject.objecttype == 0 ? string.Format("{0:N0}", Secretary.rentAgent) : "",  //待租租金A(是否包租?true)
                    rentAgentB = HomeObject.objecttype == 1 ? string.Format("{0:N0}", Secretary.rentAgent) : "",  //待租租金B(是否代租?true)
                    depositAgent = string.Format("{0:N0}", Secretary.depositAgent),       //待租押金
                    qualifyType = Secretary.qualifyRadio, //是否為轉軌2(劃選)(原租賃契約影本〔由租金補貼轉入本計畫時適用〕)
                    #endregion

                    //其他文件
                    elsefile = HomeObject.appraiser == 1 || CoOwnerCount > 0 || Tenant.TenantType == 2 || Secretary.qualifyRadio == 2 ? "V" : "", //是否有其他文件(勾選)
                };
                #endregion
                
                // 要處理的Word文件列表
                List<string> wordFilesToProcess = new List<string>
                {
                    "表單1.出租人出租住宅申請書.docx",
                    "表單2.屋況及租屋安全檢核表(租賃標的現況確認書)-代租-1.docx",
                    "表單2.屋況及租屋安全檢核表(租賃標的現況確認書)-代租-2.docx",
                    "表單3.出租人補助費用申請書.docx",
                    directTenantCount <= 2 ? "表單5.民眾(房客)承租住宅申請書.docx" : directTenantCount <= 7 ? "表單5.民眾(房客)承租住宅申請書-8人.docx" :"表單5.民眾(房客)承租住宅申請書-10人.docx"
                };
                if (FormData.existappraiser)    //有估價師
                    wordFilesToProcess.Add("附表單1.估價師租金簽註意見書.docx");
                if(AgentCount > 0)              //有房東代理人
                    wordFilesToProcess.Add("附表單2.代理人授權書-東.docx");
                if(CoOwnerCount > 0)            //有房東共有人
                    wordFilesToProcess.Add("附表單3.共有住宅代表人授權書.docx");
                if (agentTenantCount > 0)       //有房客代理人
                    wordFilesToProcess.Add("附表單2.代理人授權書-客.docx");
                if (HomeObject.objecttype == 1)
                {
                    //代租代管
                    List<string> additionalFilesToAdd = new List<string>
                    {
                        "1.社會住宅代租代管計畫-委託租賃-契約書範本.docx",
                        "2.社會住宅代租代管計畫-委託管理-契約書範本-任.docx",
                        "3.社會住宅-租賃契約書範本.docx",
                    };
                    wordFilesToProcess.AddRange(additionalFilesToAdd);
                }
                else
                {
                    //包租包管
                    List<string> additionalFilesToAdd = new List<string>
                    {
                        "4.社會住宅-包租契約書範本-任.docx",
                        "5.社會住宅-轉租契約書範本-任.docx",
                    };
                    wordFilesToProcess.AddRange(additionalFilesToAdd);
                }

                // 創建一個唯一的臨時文件夾
                string tempFolderPath = Path.Combine(Server.MapPath("~/Temp"), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempFolderPath);

                #region 替換WordSample參數
                foreach (var file in wordFilesToProcess)
                {
                    // 組合Word文件的完整路徑
                    string filePath = Path.Combine(Server.MapPath("~/WordSample"), file);
                    // 打開現有的Word文檔
                    using (DocX doc = DocX.Load(filePath))
                    {
                        #region 日期
                        bool Isnotarization = HomeObject.notarization == 1 ? true : false;  //是否公證

                        var signdatetime = Convert.ToDateTime(HomeObject.signdate);         //簽約日
                        var startdatetime = Convert.ToDateTime(HomeObject.startdate);       //起租日
                        var enddatetime = Convert.ToDateTime(HomeObject.enddate);           //結束日

                        ///規則
                        ///當無公證時，《簽約日》去找到《起租日》並將簽約日更動為起租日
                        ///
                        signdatetime = !Isnotarization ? startdatetime : signdatetime;

                        //起租日
                        doc.ReplaceText("«起租日»", (startdatetime.Year - 1911).ToString() + startdatetime.ToString("年MM月dd日"));
                        doc.ReplaceText("«yyy0»", (startdatetime.Year - 1911).ToString());
                        doc.ReplaceText("«MM0»", startdatetime.Month.ToString());
                        doc.ReplaceText("«dd0»", startdatetime.Day.ToString());
                        //簽約日
                        var newsigndatetime = signdatetime;
                        doc.ReplaceText("«簽約日»", (newsigndatetime.Year - 1911).ToString() + newsigndatetime.ToString("年MM月dd日"));
                        doc.ReplaceText("«yyy1»", (newsigndatetime.Year - 1911).ToString());
                        doc.ReplaceText("«MM1»", newsigndatetime.Month.ToString());
                        doc.ReplaceText("«dd1»", newsigndatetime.Day.ToString());
                        //結束日
                        doc.ReplaceText("«結束日»", (enddatetime.Year - 1911).ToString() + enddatetime.ToString("年MM月dd日"));
                        //簽約日(-2d)
                        newsigndatetime = signdatetime.AddDays(-2);
                        doc.ReplaceText("«yyy2»", (newsigndatetime.Year - 1911).ToString());
                        doc.ReplaceText("«MM2»", newsigndatetime.Month.ToString());
                        doc.ReplaceText("«dd2»", newsigndatetime.Day.ToString());
                        //簽約日(-2d)(-10d) => (-12d)
                        newsigndatetime = signdatetime.AddDays(-12);
                        doc.ReplaceText("«yyy3»", (newsigndatetime.Year - 1911).ToString());
                        doc.ReplaceText("«MM3»", newsigndatetime.Month.ToString());
                        doc.ReplaceText("«dd3»", newsigndatetime.Day.ToString());
                        //簽約日(-2d)(-10d)(-10d) => (-22d)
                        newsigndatetime = signdatetime.AddDays(-22);
                        doc.ReplaceText("«yyy4»", (newsigndatetime.Year - 1911).ToString());
                        doc.ReplaceText("«MM4»", newsigndatetime.Month.ToString());
                        doc.ReplaceText("«dd4»", newsigndatetime.Day.ToString());
                        //簽約日(-2d)(-10d)(-10d)(+2d) => (-20d)
                        newsigndatetime = signdatetime.AddDays(-20);
                        doc.ReplaceText("«yyy5»", (newsigndatetime.Year - 1911).ToString());
                        doc.ReplaceText("«MM5»", newsigndatetime.Month.ToString());
                        doc.ReplaceText("«dd5»", newsigndatetime.Day.ToString());
                        //簽約日(-2d)(-10d)(-10d)(+2d)(+6M)(-1d) => (+6M-21d)
                        newsigndatetime = signdatetime.AddMonths(6).AddDays(-21);
                        doc.ReplaceText("«yyy6»", (newsigndatetime.Year - 1911).ToString());
                        doc.ReplaceText("«MM6»", newsigndatetime.Month.ToString());
                        doc.ReplaceText("«dd6»", newsigndatetime.Day.ToString());
                        //簽約日(-2d)(-10d)(-10d)(+2d)(+5d) => (-15d)
                        newsigndatetime = signdatetime.AddDays(-15);
                        doc.ReplaceText("«yyy7»", (newsigndatetime.Year - 1911).ToString());
                        doc.ReplaceText("«MM7»", newsigndatetime.Month.ToString());
                        doc.ReplaceText("«dd7»", newsigndatetime.Day.ToString());
                        //簽約日(-2d)(-10d)(-10d)(+2d)(+5d)(+1M)(-1d) => (+1M-16d)
                        newsigndatetime = signdatetime.AddMonths(1).AddDays(-16);
                        doc.ReplaceText("«yyy8»", (newsigndatetime.Year - 1911).ToString());
                        doc.ReplaceText("«MM8»", newsigndatetime.Month.ToString());
                        doc.ReplaceText("«dd8»", newsigndatetime.Day.ToString());
                        #endregion

                        switch (file)
                        {
                            case "表單1.出租人出租住宅申請書.docx":

                                doc.ReplaceText("«房東姓名»", FormData.nameLandlord);
                                doc.ReplaceText("«房東身分證»", FormData.idLandlord);
                                doc.ReplaceText("«房東性別»", FormData.genderLandlord);
                                doc.ReplaceText("«房東生日»", FormData.birthLandlord);
                                doc.ReplaceText("«房東電話»", FormData.phoneLandlord);
                                doc.ReplaceText("«房東戶籍地址»", FormData.addressLandlord);
                                doc.ReplaceText("«房東同戶籍地址»", FormData.sameaddressLandlord);
                                doc.ReplaceText("«房東通訊地址»", FormData.contactLandlord);
                                doc.ReplaceText("«房東金融機構»", FormData.banknameLandlord);
                                doc.ReplaceText("«房東銀行代碼»", FormData.bankcodeLandlord);
                                doc.ReplaceText("«房東分行戶名»", FormData.branchLandlord);
                                doc.ReplaceText("«房東分行代碼»", FormData.branchcodeLandlord);
                                doc.ReplaceText("«房東帳號»", FormData.BankAccount);

                                doc.ReplaceText("«法人姓名»", FormData.nameCompany);
                                doc.ReplaceText("«法人負責人»", FormData.namePrincipal);
                                doc.ReplaceText("«法人統一編號»", FormData.idCompany);
                                doc.ReplaceText("«法人電話»", FormData.phoneCompany);
                                doc.ReplaceText("«法人戶籍地址»", FormData.addressCompany);

                                doc.ReplaceText("«法人代理人姓名»", FormData.nameCompanyAgent);
                                doc.ReplaceText("«法人代理人身分證»", FormData.idCompanyAgent);
                                doc.ReplaceText("«法人代理人戶籍地址»", FormData.addressCompanyAgent);
                                doc.ReplaceText("«法人代理人電話»", FormData.phoneCompanyAgent);

                                doc.ReplaceText("«建物門牌»", FormData.addressObject);          //建物門牌
                                doc.ReplaceText("«段»", FormData.excerpt);                   //段
                                doc.ReplaceText("«小段»", FormData.excerptShort);             //小段(可Null)
                                doc.ReplaceText("«建號»", FormData.buildNo);                  //建號
                                doc.ReplaceText("«層數»", FormData.floorAmount);              //層數
                                doc.ReplaceText("«層次»", FormData.floorNo);                  //層次
                                doc.ReplaceText("«建築完成日»", FormData.buildCreateDate);   //建築完成日
                                doc.ReplaceText("«平方»", FormData.squareAmount);            //平方公尺
                                doc.ReplaceText("«坪»", FormData.pinAmount);                 //坪數
                                doc.ReplaceText("«市場租金»", FormData.rentMarket);         //市場租金
                                doc.ReplaceText("«待租押金»", FormData.depositAgent);       //待租押金
                                doc.ReplaceText("«待租租金A»", FormData.rentAgentA);        //待租租金(包租包管)
                                doc.ReplaceText("«待租租金B»", FormData.rentAgentB);        //待租租金(代租代管)
                                doc.ReplaceText("«包租»", FormData.objecttypeA);          //包租(■/□)
                                doc.ReplaceText("«代租»", FormData.objecttypeB);          //代租(■/□)
                                doc.ReplaceText("«公證»", FormData.notarizationA);        //公證(■/□)
                                doc.ReplaceText("«非公證»", FormData.notarizationB);      //非公證(■/□)

                                doc.ReplaceText("«iscompany»", FormData.iscompany);
                                doc.ReplaceText("«appraiser»", FormData.appraiser);
                                doc.ReplaceText("«existagentLandlord»", FormData.existagentLandlord);
                                doc.ReplaceText("«existcoownerLandlord»", FormData.existcoownerLandlord);
                                doc.ReplaceText("«transition2»", FormData.qualifyType == 2 ? unicodeBlackSquare : unicodeSquare);   //轉軌2
                                doc.ReplaceText("«tenantType2»", FormData.tenantType == 2 ? unicodeBlackSquare : unicodeSquare);    //一般戶(申請人或家庭成員為身心障礙者或65歲以上老人，申請換居並將自有住宅出租。)
                                doc.ReplaceText("«elsefile»", FormData.elsefile);

                                doc.ReplaceText("«住家用»", FormData.usefor_Home);
                                doc.ReplaceText("«商業用»", FormData.usefor_Homeexcept);

                                break;

                            case "表單2.屋況及租屋安全檢核表(租賃標的現況確認書)-代租-1.docx":
                            case "表單2.屋況及租屋安全檢核表(租賃標的現況確認書)-代租-2.docx":
                                doc.ReplaceText("«建物門牌»", FormData.addressObject);      //物件地址
                                doc.ReplaceText("«A0»", FormData.electricwater);        //電熱水器
                                doc.ReplaceText("«A1»", FormData.gaswater);             //瓦斯熱水器
                                doc.ReplaceText("«A2»", FormData.outdoorgas);           //瓦斯(戶外)
                                doc.ReplaceText("«A3»", FormData.indoorgas);            //瓦斯(戶內)
                                //管委會
                                doc.ReplaceText("«有管»", FormData.existManageGroup ? unicodeBlackSquare : unicodeSquare);     //【有管委會】
                                doc.ReplaceText("«無管»", !FormData.existManageGroup ? unicodeBlackSquare : unicodeSquare);     //【無管委會】
                                doc.ReplaceText("«有管費»", FormData.existManageFee == "有" ? unicodeBlackSquare : unicodeSquare);     //【有管理費】
                                doc.ReplaceText("«無管費»", FormData.existManageFee == "無" ? unicodeBlackSquare : unicodeSquare);     //【無管理費】若為電梯大樓:選無，若為其他:不選擇

                                doc.ReplaceText("«管理坪»", FormData.management_fee_PerPin);    //每坪管理費
                                doc.ReplaceText("«管理月»", FormData.management_fee);    //每月管理費
                                doc.ReplaceText("«A6»", FormData.havecarpark);          //有停車位
                                doc.ReplaceText("«A7»", FormData.nocarpark);            //無停車位
                                doc.ReplaceText("«A8»", FormData.parktypeA);            //坡道平面
                                doc.ReplaceText("«A9»", FormData.parktypeB);            //坡道機械
                                doc.ReplaceText("«A10»", FormData.parktypeC);           //機械平面
                                doc.ReplaceText("«A11»", FormData.parktypeD);           //機械機械
                                doc.ReplaceText("«A12»", FormData.parkposition);        //上/下
                                doc.ReplaceText("«A13»", FormData.parkfloor);           //幾層
                                doc.ReplaceText("«A14»", FormData.carpositionnumber);   //車位編號
                                doc.ReplaceText("«A15»", FormData.carparkmanagefee);    //幾層
                                doc.ReplaceText("«A16»", FormData.havemorpark);         //有機車停車位
                                doc.ReplaceText("«A17»", FormData.nomorpark);           //無機車停車位
                                doc.ReplaceText("«A18»", FormData.morparkposition);     //上下
                                doc.ReplaceText("«A19»", FormData.morparkfloor);        //幾層
                                doc.ReplaceText("«A20»", FormData.scooterpositionnumber);    //機車位編號
                                doc.ReplaceText("«A21»", FormData.scootermanagefee);    //幾層
                                doc.ReplaceText("«A22»", FormData.buildtype0);          //透天厝(■/□)
                                doc.ReplaceText("«A23»", FormData.buildtype1);          //公寓(■/□)
                                doc.ReplaceText("«A24»", FormData.buildtype2);          //華廈(■/□)
                                doc.ReplaceText("«A25»", FormData.buildtype3);          //電梯大樓(■/□)

                                doc.ReplaceText("«A26»", FormData.roomAmount);          //房
                                doc.ReplaceText("«A27»", FormData.rubyAmount);          //廳
                                doc.ReplaceText("«A28»", FormData.bathAmount);          //衛
                                doc.ReplaceText("«A29»", FormData.roomtype);            //有無隔間(■/□)

                                //房屋附屬物建(設備)
                                doc.ReplaceText("«D1»", FormData.GAS);        //電視
                                doc.ReplaceText("«D2»", FormData.TVFOUR);        //冷氣
                                doc.ReplaceText("«D3»", FormData.NonLineNet);        //冰箱
                                //房屋附屬物件(家電)
                                doc.ReplaceText("«B1»", FormData.B1.Square);        //電視
                                doc.ReplaceText("«B2»", FormData.B2.Square);        //冷氣
                                doc.ReplaceText("«B3»", FormData.B3.Square);        //冰箱
                                doc.ReplaceText("«B4»", FormData.B4.Square);        //熱水器
                                doc.ReplaceText("«B5»", FormData.B5.Square);        //洗衣機
                                doc.ReplaceText("«B6»", FormData.B6.Square);        //微波爐
                                doc.ReplaceText("«B7»", FormData.B7.Square);        //洗碗機
                                doc.ReplaceText("«B8»", FormData.B8.Square);        //排油煙機
                                doc.ReplaceText("«B9»", FormData.B9.Square);        //瓦斯爐
                                doc.ReplaceText("«BB1»", FormData.B1.Amount);
                                doc.ReplaceText("«BB2»", FormData.B2.Amount);
                                doc.ReplaceText("«BB3»", FormData.B3.Amount);
                                doc.ReplaceText("«BB4»", FormData.B4.Amount);
                                doc.ReplaceText("«BB5»", FormData.B5.Amount);
                                doc.ReplaceText("«BB6»", FormData.B6.Amount);
                                doc.ReplaceText("«BB7»", FormData.B7.Amount);
                                doc.ReplaceText("«BB8»", FormData.B8.Amount);
                                doc.ReplaceText("«BB9»", FormData.B9.Amount);
                                //房屋附屬物件(家具)               
                                doc.ReplaceText("«C1»", FormData.C1.Square);        //鞋櫃
                                doc.ReplaceText("«C2»", FormData.C2.Square);        //餐桌
                                doc.ReplaceText("«C3»", FormData.C3.Square);        //餐桌椅
                                doc.ReplaceText("«C4»", FormData.C4.Square);        //沙發
                                doc.ReplaceText("«C5»", FormData.C5.Square);        //電視櫃
                                doc.ReplaceText("«C6»", FormData.C6.Square);        //茶几
                                doc.ReplaceText("«C7»", FormData.C7.Square);        //床組(頭)
                                doc.ReplaceText("«C8»", FormData.C8.Square);        //衣櫃
                                doc.ReplaceText("«C9»", FormData.C9.Square);        //置物櫃          
                                doc.ReplaceText("«C10»", FormData.C10.Square);      //梳妝台          
                                doc.ReplaceText("«C11»", FormData.C11.Square);      //書櫃          
                                doc.ReplaceText("«C12»", FormData.C12.Square);      //書桌          
                                doc.ReplaceText("«C13»", FormData.C13.Square);      //書桌椅         
                                doc.ReplaceText("«CC1»", FormData.C1.Amount);    
                                doc.ReplaceText("«CC2»", FormData.C2.Amount);    
                                doc.ReplaceText("«CC3»", FormData.C3.Amount);    
                                doc.ReplaceText("«CC4»", FormData.C4.Amount);    
                                doc.ReplaceText("«CC5»", FormData.C5.Amount);    
                                doc.ReplaceText("«CC6»", FormData.C6.Amount);    
                                doc.ReplaceText("«CC7»", FormData.C7.Amount);    
                                doc.ReplaceText("«CC8»", FormData.C8.Amount);    
                                doc.ReplaceText("«CC9»", FormData.C9.Amount);      
                                doc.ReplaceText("«CC10»", FormData.C10.Amount);        
                                doc.ReplaceText("«CC11»", FormData.C11.Amount);        
                                doc.ReplaceText("«CC12»", FormData.C12.Amount);        
                                doc.ReplaceText("«CC13»", FormData.C13.Amount);   

                                break;

                            case "表單3.出租人補助費用申請書.docx":
                                //不須帶入
                                break;

                            case "表單5.民眾(房客)承租住宅申請書.docx":
                            case "表單5.民眾(房客)承租住宅申請書-8人.docx":
                            case "表單5.民眾(房客)承租住宅申請書-10人.docx":

                                doc.ReplaceText("«包租»", FormData.objecttypeA);              //包租
                                doc.ReplaceText("«代租»", FormData.objecttypeB);              //代租

                                doc.ReplaceText("«房客姓名»", FormData.nameTenant);           //房客姓名
                                doc.ReplaceText("«男»", FormData.genderTenantA);             //性別男(■/□)
                                doc.ReplaceText("«女»", FormData.genderTenantB);             //性別女(■/□)
                                doc.ReplaceText("«房客生日»", FormData.birthTenant);            //房客生日
                                doc.ReplaceText("«房客身分證»", FormData.idTenant);          //房客身分證
                                doc.ReplaceText("«戶號»", FormData.accountNo);          //房客戶號
                                doc.ReplaceText("«房客電話»", FormData.phoneTenant);            //房客電話
                                doc.ReplaceText("«房客戶籍地址»", FormData.addressTenant);    //房客戶籍地址
                                doc.ReplaceText("«房客同地址»", FormData.sameaddressTenant);   //房客同戶籍地(■/□)
                                doc.ReplaceText("«房客通訊地址»", FormData.contactTenant);      //房客通訊地址(同戶籍地為空)
                                doc.ReplaceText("«房客通訊地址二»", contactTenant);            //房客通訊地址

                                //房客資格(申請人資格)
                                doc.ReplaceText("«A0»", FormData.tenantType != 1 && FormData.tenantType != 2 ? unicodeBlackSquare : unicodeSquare);      //(一般戶)設籍於該市居民、有居住事實、或有就學就業需求者。(■/□)
                                doc.ReplaceText("«A1»", FormData.tenantType == 1 ? unicodeBlackSquare : unicodeSquare);      //(一般戶)現任職務之最高職務列等在警正四階以下或相當職務列等之基層警察及消防人員。(■/□)
                                doc.ReplaceText("«A2»", FormData.tenantType == 2 ? unicodeBlackSquare : unicodeSquare);      //(一般戶)申請人或家庭成員為身心障礙者或65歲以上老人，申請換居並將自有住宅出租。(■/□)
                                doc.ReplaceText("«A3»", FormData.tenantType == 3 ? unicodeBlackSquare : unicodeSquare);      //(一類)六十五歲以上之老人。(限申請人本人)(■/□)
                                doc.ReplaceText("«A4»", FormData.tenantType == 4 ? unicodeBlackSquare : unicodeSquare);      //(一類)原住民。(■/□)
                                doc.ReplaceText("«A5»", FormData.tenantType == 5 ? unicodeBlackSquare : unicodeSquare);      //(一類)育有未成年子女三人以上。(限申請人本人)(■/□)
                                doc.ReplaceText("«A6»", FormData.tenantType == 6 ? unicodeBlackSquare : unicodeSquare);      //(一類)特殊境遇家庭。(■/□)
                                doc.ReplaceText("«A7»", FormData.tenantType == 7 ? unicodeBlackSquare : unicodeSquare);      //(一類)於安置教養機構或寄養家庭結束安置無法返家，未滿二十五歲。(限申請人本人)(■/□)
                                doc.ReplaceText("«A8»", FormData.tenantType == 8 ? unicodeBlackSquare : unicodeSquare);      //(一類)受家庭暴力或性侵害之受害者及其子女。(■/□)
                                doc.ReplaceText("«A9»", FormData.tenantType == 9 ? unicodeBlackSquare : unicodeSquare);      //(一類)身心障礙者。(■/□)
                                doc.ReplaceText("«A10»", FormData.tenantType == 10 ? unicodeBlackSquare : unicodeSquare);    //(一類)感染人類免疫缺乏病毒者或罹患後天免疫缺乏症候群者。(■/□)
                                doc.ReplaceText("«A11»", FormData.tenantType == 11 ? unicodeBlackSquare : unicodeSquare);    //(一類)因懷孕或生育而遭遇困境之未成年人。(限申請人本人)(■/□)
                                doc.ReplaceText("«A12»", FormData.tenantType == 12 ? unicodeBlackSquare : unicodeSquare);    //(一類)災民。(■/□)
                                doc.ReplaceText("«A13»", FormData.tenantType == 13 ? unicodeBlackSquare : unicodeSquare);    //(一類)遊民。(■/□)
                                doc.ReplaceText("«A14»", FormData.tenantType == 14 ? unicodeBlackSquare : unicodeSquare);    //(二類)低收入戶。(■/□)
                                doc.ReplaceText("«A15»", FormData.tenantType == 15 ? unicodeBlackSquare : unicodeSquare);    //(二類)中低收入戶。(■/□)
                                //是否為現有住宅政策補貼戶
                                doc.ReplaceText("«B0»", FormData.qualifyType == 2 || FormData.qualifyType == 3 ? unicodeBlackSquare : unicodeSquare);    //是轉軌二或轉軌四(■/□)
                                doc.ReplaceText("«B1»", FormData.qualifyType != 2 && FormData.qualifyType != 3 ? unicodeBlackSquare : unicodeSquare);    //非轉軌二且非轉軌四(■/□)

                                //申請人與家庭成員
                                //申請人戶口名簿戶號(戶號)
                                doc.ReplaceText("«C0»", FormData.accountNoArray[0].ToString());    //戶號
                                doc.ReplaceText("«C1»", FormData.accountNoArray[1].ToString());    //戶號
                                doc.ReplaceText("«C2»", FormData.accountNoArray[2].ToString());    //戶號
                                doc.ReplaceText("«C3»", FormData.accountNoArray[3].ToString());    //戶號
                                doc.ReplaceText("«C4»", FormData.accountNoArray[4].ToString());    //戶號
                                doc.ReplaceText("«C5»", FormData.accountNoArray[5].ToString());    //戶號
                                doc.ReplaceText("«C6»", FormData.accountNoArray[6].ToString());    //戶號
                                doc.ReplaceText("«C7»", FormData.accountNoArray[7].ToString());    //戶號

                                //分戶戶號
                                doc.ReplaceText("«C10»", coupleTenantCount > 0 && FormData.coupleaccountNoArray != null ? FormData.coupleaccountNoArray.Length == 8 ? FormData.coupleaccountNoArray[0].ToString() : "" : "");    //分戶戶號
                                doc.ReplaceText("«C11»", coupleTenantCount > 0 && FormData.coupleaccountNoArray != null ? FormData.coupleaccountNoArray.Length == 8 ? FormData.coupleaccountNoArray[1].ToString() : "" : "");    //分戶戶號
                                doc.ReplaceText("«C12»", coupleTenantCount > 0 && FormData.coupleaccountNoArray != null ? FormData.coupleaccountNoArray.Length == 8 ? FormData.coupleaccountNoArray[2].ToString() : "" : "");    //分戶戶號
                                doc.ReplaceText("«C13»", coupleTenantCount > 0 && FormData.coupleaccountNoArray != null ? FormData.coupleaccountNoArray.Length == 8 ? FormData.coupleaccountNoArray[3].ToString() : "" : "");    //分戶戶號
                                doc.ReplaceText("«C14»", coupleTenantCount > 0 && FormData.coupleaccountNoArray != null ? FormData.coupleaccountNoArray.Length == 8 ? FormData.coupleaccountNoArray[4].ToString() : "" : "");    //分戶戶號
                                doc.ReplaceText("«C15»", coupleTenantCount > 0 && FormData.coupleaccountNoArray != null ? FormData.coupleaccountNoArray.Length == 8 ? FormData.coupleaccountNoArray[5].ToString() : "" : "");    //分戶戶號
                                doc.ReplaceText("«C16»", coupleTenantCount > 0 && FormData.coupleaccountNoArray != null ? FormData.coupleaccountNoArray.Length == 8 ? FormData.coupleaccountNoArray[6].ToString() : "" : "");    //分戶戶號
                                doc.ReplaceText("«C17»", coupleTenantCount > 0 && FormData.coupleaccountNoArray != null ? FormData.coupleaccountNoArray.Length == 8 ? FormData.coupleaccountNoArray[7].ToString() : "" : "");    //分戶戶號

                                //直系親屬
                                //(房客)
                                doc.ReplaceText("«D0»", FormData.nameTenant);                               //房客姓名
                                doc.ReplaceText("«D1»", FormData.idTenant.Length >= 1 ? FormData.idTenant.ToCharArray()[0].ToString() : "");    //房客身分證
                                doc.ReplaceText("«D2»", FormData.idTenant.Length >= 2 ? FormData.idTenant.ToCharArray()[1].ToString() : "");    //房客身分證
                                doc.ReplaceText("«D3»", FormData.idTenant.Length >= 3 ? FormData.idTenant.ToCharArray()[2].ToString() : "");    //房客身分證
                                doc.ReplaceText("«D4»", FormData.idTenant.Length >= 4 ? FormData.idTenant.ToCharArray()[3].ToString() : "");    //房客身分證
                                doc.ReplaceText("«D5»", FormData.idTenant.Length >= 5 ? FormData.idTenant.ToCharArray()[4].ToString() : "");    //房客身分證
                                doc.ReplaceText("«D6»", FormData.idTenant.Length >= 6 ? FormData.idTenant.ToCharArray()[5].ToString() : "");    //房客身分證
                                doc.ReplaceText("«D7»", FormData.idTenant.Length >= 7 ? FormData.idTenant.ToCharArray()[6].ToString() : "");    //房客身分證
                                doc.ReplaceText("«D8»", FormData.idTenant.Length >= 8 ? FormData.idTenant.ToCharArray()[7].ToString() : "");    //房客身分證
                                doc.ReplaceText("«D9»", FormData.idTenant.Length >= 9 ? FormData.idTenant.ToCharArray()[8].ToString() : "");    //房客身分證
                                doc.ReplaceText("«D10»", FormData.idTenant.Length >= 10 ? FormData.idTenant.ToCharArray()[9].ToString() : "");    //房客身分證
                                doc.ReplaceText("«D11»", (birthTenant.Year - 1911).ToString() + birthTenant.ToString(".MM.dd"));    //房客生日
                                //(直系)
                                var initialchar = 'D';
                                var ChildCount = 0;
                                for (int i = 1; i <= 9; i++)
                                {
                                    initialchar++;
                                    string familyPropertyName = $"Family{i}";
                                    PropertyInfo familyProperty = FormData.GetType().GetProperty(familyPropertyName);

                                    if (familyProperty != null)
                                    {
                                        List<string> familyData = (List<string>)familyProperty.GetValue(FormData);
                                        if (directTenantCount >= i)
                                        {
                                            var birthdate = directTenantCount >= i ? Convert.ToDateTime(familyData[3]) : DateTime.Now;
                                            TimeSpan difference = DateTime.Now - birthdate;
                                            bool IsChild = difference.TotalDays < (365.25 * 18); // 使用365.25以考虑闰年
                                            if (IsChild) ChildCount++;
                                        }
                                        doc.ReplaceText($"«{initialchar}0»", directTenantCount >= i ? familyData[1] : "");
                                        doc.ReplaceText($"«{initialchar}1»", directTenantCount >= i ? familyData[2].Length >= 1 ? familyData[2].ToCharArray()[0].ToString() : "" : "");    //房客身分證
                                        doc.ReplaceText($"«{initialchar}2»", directTenantCount >= i ? familyData[2].Length >= 2 ? familyData[2].ToCharArray()[1].ToString() : "" : "");    //房客身分證
                                        doc.ReplaceText($"«{initialchar}3»", directTenantCount >= i ? familyData[2].Length >= 3 ? familyData[2].ToCharArray()[2].ToString() : "" : "");    //房客身分證
                                        doc.ReplaceText($"«{initialchar}4»", directTenantCount >= i ? familyData[2].Length >= 4 ? familyData[2].ToCharArray()[3].ToString() : "" : "");    //房客身分證
                                        doc.ReplaceText($"«{initialchar}5»", directTenantCount >= i ? familyData[2].Length >= 5 ? familyData[2].ToCharArray()[4].ToString() : "" : "");    //房客身分證
                                        doc.ReplaceText($"«{initialchar}6»", directTenantCount >= i ? familyData[2].Length >= 6 ? familyData[2].ToCharArray()[5].ToString() : "" : "");    //房客身分證
                                        doc.ReplaceText($"«{initialchar}7»", directTenantCount >= i ? familyData[2].Length >= 7 ? familyData[2].ToCharArray()[6].ToString() : "" : "");    //房客身分證
                                        doc.ReplaceText($"«{initialchar}8»", directTenantCount >= i ? familyData[2].Length >= 8 ? familyData[2].ToCharArray()[7].ToString() : "" : "");    //房客身分證
                                        doc.ReplaceText($"«{initialchar}9»", directTenantCount >= i ? familyData[2].Length >= 9 ? familyData[2].ToCharArray()[8].ToString() : "" : "");    //房客身分證
                                        doc.ReplaceText($"«{initialchar}10»", directTenantCount >= i ? familyData[2].Length >= 10 ? familyData[2].ToCharArray()[9].ToString() : "" : "");    //房客身分證
                                        doc.ReplaceText($"«{initialchar}11»", directTenantCount >= i ? (Convert.ToInt32(familyData[3].Split('-')[0]) - 1911).ToString() + "." + familyData[3].Split('-')[1] + "." + familyData[3].Split('-')[2] : "");    //房客生日
                                        doc.ReplaceText($"«{initialchar}12»", directTenantCount >= i ? familyData[0] : "");    //房客關係
                                    }
                                }
                                doc.ReplaceText("«未成年人數»", ChildCount.ToString());       //未成年人數

                                doc.ReplaceText("«房客金融機構»", FormData.banknameTenant);       //房客金融機構
                                doc.ReplaceText("«房客銀行代碼»", FormData.bankcodeTenant);       //房客銀行代碼
                                doc.ReplaceText("«房客分行戶名»", FormData.branchTenant);         //房客分行戶名
                                doc.ReplaceText("«房客分行代碼»", FormData.branchcodeTenant);     //房客分行代碼
                                doc.ReplaceText("«房客帳號»", FormData.BankAccount);              //房客帳號

                                doc.ReplaceText("«一般戶二»", FormData.tenantType == 1 ? "V" : "");      //(一般戶)現任職務之最高職務列等在警正四階以下或相當職務列等之基層警察及消防人員。(■/□)
                                doc.ReplaceText("«一般戶三»", FormData.tenantType == 2 ? "V" : "");      //(一般戶)申請人或家庭成員為身心障礙者或65歲以上老人，申請換居並將自有住宅出租。(■/□)

                                break;

                            case "附表單1.估價師租金簽註意見書.docx":
                                doc.ReplaceText("«建物門牌»", FormData.addressObject);          //建物門牌
                                doc.ReplaceText("«段»", FormData.excerpt);                   //段
                                doc.ReplaceText("«小段»", FormData.excerptShort);             //小段(可Null)
                                doc.ReplaceText("«建號»", FormData.buildNo);                  //建號
                                doc.ReplaceText("«層數»", FormData.floorAmount);              //層數
                                doc.ReplaceText("«層次»", FormData.floorNo);                  //層次
                                doc.ReplaceText("«建築完成日»", FormData.buildCreateDate);   //建築完成日
                                doc.ReplaceText("«平方»", FormData.squareAmount);            //平方公尺
                                doc.ReplaceText("«坪»", FormData.pinAmount);                 //坪數
                                doc.ReplaceText("«市場租金»", FormData.rentMarket);         //市場租金
                                doc.ReplaceText("«待租押金»", FormData.depositAgent);       //待租押金
                                doc.ReplaceText("«待租租金A»", FormData.rentAgentA);        //待租租金(包租包管)
                                doc.ReplaceText("«待租租金B»", FormData.rentAgentB);        //待租租金(代租代管)
                                doc.ReplaceText("«包租»", FormData.objecttypeA);          //包租(■/□)
                                doc.ReplaceText("«代租»", FormData.objecttypeB);          //代租(■/□)
                                break;

                            case "附表單2.代理人授權書-東.docx":
                                doc.ReplaceText("«房東姓名»", FormData.nameLandlord);
                                doc.ReplaceText("«房東身分證»", FormData.idLandlord);
                                doc.ReplaceText("«房東電話»", FormData.phoneLandlord);
                                doc.ReplaceText("«房東戶籍地址»", FormData.addressLandlord);
                                doc.ReplaceText("«房東代理人姓名»", FormData.nameLandlordAgent);
                                doc.ReplaceText("«房東代理人身分證»", FormData.idLandlordAgent);
                                doc.ReplaceText("«房東代理人戶籍地址»", FormData.addressLandlordAgent);
                                doc.ReplaceText("«房東代理人電話»", FormData.phoneLandlordAgent);

                                doc.ReplaceText("«法人姓名»", FormData.nameCompany);
                                doc.ReplaceText("«法人負責人»", FormData.namePrincipal);
                                doc.ReplaceText("«法人統一編號»", FormData.idCompany);
                                doc.ReplaceText("«法人電話»", FormData.phoneCompany);
                                doc.ReplaceText("«法人戶籍地址»", FormData.addressCompany);

                                doc.ReplaceText("«建物門牌»", FormData.addressObject);      //物件地址
                                doc.ReplaceText("«段»", FormData.excerpt);                   //段
                                doc.ReplaceText("«小段»", FormData.excerptShort);             //小段(可Null)
                                doc.ReplaceText("«建號»", FormData.buildNo);                  //建號

                                break;

                            case "附表單2.代理人授權書-客.docx":
                                //根據代理人人數存相應表單數量
                                for (var i = 1; i <= agentTenantCount; i++)
                                {
                                    string agentPropertyName = $"agentTenant{i}";
                                    PropertyInfo agentProperty = FormData.GetType().GetProperty(agentPropertyName);

                                    if (agentProperty != null)
                                    {
                                        List<string> AgentData = (List<string>)agentProperty.GetValue(FormData);
                                        var agentAddress = AgentData[3].Split('-')[0] + AgentData[3].Split('-')[1] + AgentData[3].Split('-')[2] + AgentData[4];

                                        using (DocX agentDoc = DocX.Load(filePath))
                                        {
                                            agentDoc.ReplaceText("«房客姓名»", FormData.nameTenant);           //房客姓名
                                            agentDoc.ReplaceText("«房客身分證»", FormData.idTenant);          //房客身分證
                                            agentDoc.ReplaceText("«房客電話»", FormData.phoneTenant);            //房客電話
                                            agentDoc.ReplaceText("«房客戶籍地址»", FormData.addressTenant);    //房客戶籍地址

                                            agentDoc.ReplaceText("«建物門牌»", FormData.addressObject);          //建物門牌
                                            agentDoc.ReplaceText("«段»", FormData.excerpt);                   //段
                                            agentDoc.ReplaceText("«小段»", FormData.excerptShort);             //小段(可Null)
                                            agentDoc.ReplaceText("«建號»", FormData.buildNo);                  //建號

                                            agentDoc.ReplaceText($"«房客代理人»", AgentData[0]);                //代理人姓名
                                            agentDoc.ReplaceText($"«房客代理人身分證»", AgentData[1]);                //房客代理人身分證
                                            agentDoc.ReplaceText($"«房客代理人戶籍地址»", agentAddress);                //房客代理人戶籍地址
                                            agentDoc.ReplaceText($"«房客代理人電話»", Regex.Replace(AgentData[2], @"(\d{4})(\d{3})(\d{3})", "$1-$2-$3") );                //房客代理人電話
                                            #region 存至暫存檔
                                            string agentFileName = $"附表單2.代理人授權書-客_{AgentData[0]}.docx";
                                            string agentFilePath = Path.Combine(tempFolderPath, agentFileName);
                                            // 保存修改後的文檔
                                            agentDoc.SaveAs(agentFilePath);
                                            // 關閉Word文件
                                            agentDoc.Dispose();
                                            #endregion
                                        }
                                    }
                                }
                                break;

                            case "附表單3.共有住宅代表人授權書.docx":
                                doc.ReplaceText("«房東姓名»", FormData.nameLandlord);
                                doc.ReplaceText("«房東身分證»", FormData.idLandlord);
                                doc.ReplaceText("«房東電話»", FormData.phoneLandlord);
                                doc.ReplaceText("«房東戶籍地址»", FormData.addressLandlord);

                                doc.ReplaceText("«法人姓名»", FormData.nameCompany);
                                doc.ReplaceText("«法人負責人»", FormData.namePrincipal);
                                doc.ReplaceText("«法人統一編號»", FormData.idCompany);
                                doc.ReplaceText("«法人電話»", FormData.phoneCompany);
                                doc.ReplaceText("«法人戶籍地址»", FormData.addressCompany);

                                doc.ReplaceText("«建物門牌»", FormData.addressObject);      //物件地址
                                doc.ReplaceText("«段»", FormData.excerpt);                   //段
                                doc.ReplaceText("«小段»", FormData.excerptShort);             //小段(可Null)
                                doc.ReplaceText("«建號»", FormData.buildNo);                  //建號

                                //共有人資料
                                for (int i = 1; i <= 4; i++)
                                {
                                    string coownerPropertyName = $"CoOwner{i}";
                                    PropertyInfo CoOwnerProperty = FormData.GetType().GetProperty(coownerPropertyName);

                                    if (CoOwnerProperty != null)
                                    {
                                        List<string> CoownerData = (List<string>)CoOwnerProperty.GetValue(FormData);
                                      
                                        var coaddress = CoOwnerCount >= i ? CoownerData[3].Split('-')[0] + CoownerData[3].Split('-')[1] + CoownerData[3].Split('-')[2] + CoownerData[4] : "";

                                        doc.ReplaceText($"«共有人姓名{i}»", CoOwnerCount >= i ? CoownerData[0] : "");    //共有人姓名
                                        doc.ReplaceText($"«共有人身分證{i}»", CoOwnerCount >= i ? CoownerData[1] : "");    //共有人身分證
                                        doc.ReplaceText($"«共有人戶籍地址{i}»", coaddress);    //共有人姓名
                                        doc.ReplaceText($"«共有人電話{i}»", CoOwnerCount >= i ? Regex.Replace(CoownerData[2], @"(\d{4})(\d{3})(\d{3})", "$1-$2-$3") : "");    //共有人電話
                                    }
                                }
                                break;

                            ///契約(代租代管需填)
                            case "1.社會住宅代租代管計畫-委託租賃-契約書範本.docx":
                            case "2.社會住宅代租代管計畫-委託管理-契約書範本-任.docx":
                            case "3.社會住宅-租賃契約書範本.docx":
                            case "4.社會住宅-包租契約書範本-任.docx":
                            case "5.社會住宅-轉租契約書範本-任.docx":
                                //物件資料
                                doc.ReplaceText("«公證»", HomeObject.notarization == 1 ? "V" : "");
                                doc.ReplaceText("«無公證»", HomeObject.notarization == 0 ? "雙方時間無法配合" : "");
                                doc.ReplaceText("«租金»", FormData.rent);
                                doc.ReplaceText("«押金»", FormData.deposit);
                                doc.ReplaceText("«押金/租金»", FormData.depositPerMonth);       //押金為 *** 個月的租金
                                doc.ReplaceText("«繳租日»", FormData.paydate);
                                doc.ReplaceText("«繳租日»", FormData.managecommitteeA);
                                doc.ReplaceText("«Y0»", HomeObject.management_fee > 0 ? "V" : "");  //有無管理費
                                doc.ReplaceText("«Y1»", (HomeObject.carparkmanagefee + HomeObject.scootermanagefee) > 0 ? "V" : "");  //有無管理費
                                doc.ReplaceText("«管理費»", FormData.management_fee);  //管理費金額
                                doc.ReplaceText("«車管理費»", FormData.totalparkManagefee);     //車位管理費金額(汽車+機車)
                                doc.ReplaceText("«車租金»", FormData.totalparkRent);           //車位租金(汽車+機車)
                                doc.ReplaceText("«合計租金»", FormData.totalRent);              //合計租金(房租金 + 車租金)


                                doc.ReplaceText("«建物門牌»", FormData.addressObject);          //建物門牌
                                doc.ReplaceText("«段»", FormData.excerpt);                   //段
                                doc.ReplaceText("«小段»", FormData.excerptShort);             //小段(可Null)
                                doc.ReplaceText("«建號»", FormData.buildNo);                  //建號
                                doc.ReplaceText("«地號»", FormData.placeNo);                  //地號
                                doc.ReplaceText("«平方»", FormData.squareAmount);            //平方公尺

                                doc.ReplaceText("«A0»", FormData.havecarpark);          //有停車位
                                doc.ReplaceText("«A1»", FormData.nocarpark);            //無停車位
                                
                                doc.ReplaceText("«B0»", FormData.existagentLandlord);          //有房東代理人
                                doc.ReplaceText("«BB0»", FormData.existagentLandlordorTenant); //有房東代理人
                                doc.ReplaceText("«B1»", FormData.existcoownerLandlord);        //有房東共有人
                                doc.ReplaceText("«B3»", FormData.existagentTenant);            //有房客共有人

                                doc.ReplaceText("«B2»",  CoOwnerCount > 0  ? "共有住宅代表人授權書" : "");            //有共有人，帶入文字

                                doc.ReplaceText("«房客姓名»", FormData.nameTenant);           //房客姓名
                                doc.ReplaceText("«房客身分證»", FormData.idTenant);          //房客身分證
                                doc.ReplaceText("«房客電話»", FormData.phoneTenant);            //房客電話
                                doc.ReplaceText("«房客戶籍地址»", FormData.addressTenant);    //房客戶籍地址
                                doc.ReplaceText("«房客通訊地址»", FormData.contactTenant);    //房客通訊地址

                                doc.ReplaceText("«房東姓名»", FormData.nameLandlord);
                                doc.ReplaceText("«房東身分證»", FormData.idLandlord);
                                doc.ReplaceText("«房東電話»", FormData.phoneLandlord);
                                doc.ReplaceText("«房東戶籍地址»", FormData.addressLandlord);
                                doc.ReplaceText("«房東通訊地址»", contactLandlord);
                                doc.ReplaceText("«房東金融機構»", FormData.banknameLandlord);
                                doc.ReplaceText("«房東帳號»", FormData.BankAccount);

                                doc.ReplaceText("«法人姓名»", FormData.nameCompany);
                                doc.ReplaceText("«法人負責人»", FormData.namePrincipal);
                                doc.ReplaceText("«法人統一編號»", FormData.idCompany);
                                doc.ReplaceText("«法人電話»", FormData.phoneCompany);
                                doc.ReplaceText("«法人戶籍地址»", FormData.addressCompany);
                                doc.ReplaceText("«法人代理人姓名»", FormData.nameCompanyAgent);
                                doc.ReplaceText("«法人代理人身分證»", FormData.idCompanyAgent);
                                doc.ReplaceText("«法人代理人戶籍地址»", FormData.addressCompanyAgent);
                                doc.ReplaceText("«法人代理人電話»", FormData.phoneCompanyAgent);

                                //【房東】共有人資料(4人)
                                var nameClient = FormData.nameLandlord + FormData.nameCompany;
                                for (int i = 1; i <= 4; i++)
                                {
                                    string coownerPropertyName = $"CoOwner{i}";
                                    PropertyInfo CoOwnerProperty = FormData.GetType().GetProperty(coownerPropertyName);

                                    if (CoOwnerProperty != null)
                                    {
                                        List<string> CoownerData = (List<string>)CoOwnerProperty.GetValue(FormData);
                                        if(CoOwnerCount >= i)
                                        {
                                            var coaddress =CoownerData[3].Split('-')[0] + CoownerData[3].Split('-')[1] + CoownerData[3].Split('-')[2] + CoownerData[4];
                                            var coaddressContact =CoownerData[5].Split('-')[0] + CoownerData[5].Split('-')[1] + CoownerData[5].Split('-')[2] + CoownerData[6];
                                            doc.ReplaceText($"«房東共有人標題{i}»", "共有人：");    //共有人姓名
                                            doc.ReplaceText($"«房東共有人姓名{i}»", "姓名：" + CoownerData[0] + "　　　　　　　　　　　　　　　　　　　　(簽章)");    //共有人姓名
                                            doc.ReplaceText($"«房東共有人身分證{i}»", "身分證字號：" + CoownerData[1]);    //共有人身分證
                                            doc.ReplaceText($"«房東共有人戶籍地址{i}»", "戶籍地址：" + coaddress);    //共有人姓名
                                            doc.ReplaceText($"«房東共有人通訊地址{i}»", "通訊地址：" + coaddressContact);    //共有人通訊地址
                                            doc.ReplaceText($"«房東共有人電話{i}»", "連絡電話：" + Regex.Replace(CoownerData[2], @"(\d{4})(\d{3})(\d{3})", "$1-$2-$3"));    //共有人電話

                                            nameClient = nameClient + "," + CoownerData[0];
                                        }
                                        else
                                        {
                                            var paragraph0 = doc.Paragraphs.Where(p => p.Text.Contains($"«房東共有人標題{i}»")).FirstOrDefault();
                                            var paragraph1 = doc.Paragraphs.Where(p => p.Text.Contains($"«房東共有人姓名{i}»")).FirstOrDefault();
                                            var paragraph2 = doc.Paragraphs.Where(p => p.Text.Contains($"«房東共有人身分證{i}»")).FirstOrDefault();
                                            var paragraph3 = doc.Paragraphs.Where(p => p.Text.Contains($"«房東共有人戶籍地址{i}»")).FirstOrDefault();
                                            var paragraph4 = doc.Paragraphs.Where(p => p.Text.Contains($"«房東共有人通訊地址{i}»")).FirstOrDefault();
                                            var paragraph5 = doc.Paragraphs.Where(p => p.Text.Contains($"«房東共有人電話{i}»")).FirstOrDefault();
                                            if (paragraph0 != null)
                                            {
                                                doc.RemoveParagraph(paragraph0);
                                                doc.RemoveParagraph(paragraph1);
                                                doc.RemoveParagraph(paragraph2);
                                                doc.RemoveParagraph(paragraph3);
                                                doc.RemoveParagraph(paragraph4);
                                                doc.RemoveParagraph(paragraph5);
                                            }
                                        }
                                    }
                                }
                                //【房東】代理人資料(1人)
                                if(AgentCount > 0)
                                {
                                    doc.ReplaceText("«房東代理人標題»", "代理人：");    //共有人姓名
                                    doc.ReplaceText("«房東代理人姓名»", "姓名：" + FormData.nameLandlordAgent + "　　　　　　　　　　　　　　　　　　　　(簽章)");
                                    doc.ReplaceText("«房東代理人身分證»", "身分證字號：" + FormData.idLandlordAgent);
                                    doc.ReplaceText("«房東代理人戶籍地址»", "戶籍地址：" + FormData.addressLandlordAgent);
                                    doc.ReplaceText("«房東代理人通訊地址»", "通訊地址：" + FormData.contactLandlordAgent);
                                    //doc.ReplaceText("«房東代理人電話»", "連絡電話：" + FormData.phoneCompanyAgent);
                                    doc.ReplaceText("«房東代理人電話»", "連絡電話：" + Regex.Replace(FormData.phoneLandlordAgent, @"(\d{4})(\d{3})(\d{3})", "$1-$2-$3"));
                                }
                                else
                                {
                                    var paragraph0 = doc.Paragraphs.Where(p => p.Text.Contains("«房東代理人標題»")).FirstOrDefault();
                                    var paragraph1 = doc.Paragraphs.Where(p => p.Text.Contains("«房東代理人姓名»")).FirstOrDefault();
                                    var paragraph2 = doc.Paragraphs.Where(p => p.Text.Contains("«房東代理人身分證»")).FirstOrDefault();
                                    var paragraph3 = doc.Paragraphs.Where(p => p.Text.Contains("«房東代理人戶籍地址»")).FirstOrDefault();
                                    var paragraph4 = doc.Paragraphs.Where(p => p.Text.Contains("«房東代理人通訊地址»")).FirstOrDefault();
                                    var paragraph5 = doc.Paragraphs.Where(p => p.Text.Contains("«房東代理人電話»")).FirstOrDefault();
                                    if (paragraph0 != null)
                                    {
                                        doc.RemoveParagraph(paragraph0);
                                        doc.RemoveParagraph(paragraph1);
                                        doc.RemoveParagraph(paragraph2);
                                        doc.RemoveParagraph(paragraph3);
                                        doc.RemoveParagraph(paragraph4);
                                        doc.RemoveParagraph(paragraph5);
                                    }
                                }

                                //【房客】代理人資料(3人)
                                for (int i = 1; i <= 3; i++)
                                {
                                    string agentPropertyName = $"agentTenant{i}";
                                    PropertyInfo agentProperty = FormData.GetType().GetProperty(agentPropertyName);

                                    if (agentProperty != null)
                                    {
                                        List<string> AgentData = (List<string>)agentProperty.GetValue(FormData);
                                        if (agentTenantCount >= i)
                                        {
                                            var agaddress = AgentData[3].Split('-')[0] + AgentData[3].Split('-')[1] + AgentData[3].Split('-')[2] + AgentData[4];
                                            var agaddressContact = AgentData[5].Split('-')[0] + AgentData[5].Split('-')[1] + AgentData[5].Split('-')[2] + AgentData[6];
                                            doc.ReplaceText($"«房客代理人標題{i}»", "代理人：");                           //代理人標題
                                            doc.ReplaceText($"«房客代理人姓名{i}»", "姓名：" + AgentData[0] + "　　　　　　　　　　　　　　　　　　　　(簽章)");    //代理人姓名
                                            doc.ReplaceText($"«房客代理人身分證{i}»", "身分證字號：" + AgentData[1]);         //代理人身分證
                                            doc.ReplaceText($"«房客代理人戶籍地址{i}»", "戶籍地址：" + agaddress);            //代理人姓名
                                            doc.ReplaceText($"«房客代理人通訊地址{i}»", "通訊地址：" + agaddressContact);    //代理人通訊地址
                                            doc.ReplaceText($"«房客代理人電話{i}»", "連絡電話：" + Regex.Replace(AgentData[2], @"(\d{4})(\d{3})(\d{3})", "$1-$2-$3"));           //代理人電話
                                        }
                                        else
                                        {
                                            var paragraph0 = doc.Paragraphs.Where(p => p.Text.Contains($"«房客代理人標題{i}»")).FirstOrDefault();
                                            var paragraph1 = doc.Paragraphs.Where(p => p.Text.Contains($"«房客代理人姓名{i}»")).FirstOrDefault();
                                            var paragraph2 = doc.Paragraphs.Where(p => p.Text.Contains($"«房客代理人身分證{i}»")).FirstOrDefault();
                                            var paragraph3 = doc.Paragraphs.Where(p => p.Text.Contains($"«房客代理人戶籍地址{i}»")).FirstOrDefault();
                                            var paragraph4 = doc.Paragraphs.Where(p => p.Text.Contains($"«房客代理人通訊地址{i}»")).FirstOrDefault();
                                            var paragraph5 = doc.Paragraphs.Where(p => p.Text.Contains($"«房客代理人電話{i}»")).FirstOrDefault();
                                            if(paragraph0 != null)
                                            {
                                                doc.RemoveParagraph(paragraph0);
                                                doc.RemoveParagraph(paragraph1);
                                                doc.RemoveParagraph(paragraph2);
                                                doc.RemoveParagraph(paragraph3);
                                                doc.RemoveParagraph(paragraph4);
                                                doc.RemoveParagraph(paragraph5);
                                            }  
                                        }
                                    }
                                }

                                //【房客】保證人資料(3人)
                                for (int i = 1; i <= 3; i++)
                                {
                                    string guarantorPropertyName = $"guarantorTenant{i}";
                                    PropertyInfo guarantorProperty = FormData.GetType().GetProperty(guarantorPropertyName);

                                    if (guarantorProperty != null)
                                    {
                                        List<string> GuarantorData = (List<string>)guarantorProperty.GetValue(FormData);
                                        if (gaurantorTenantCount >= i)
                                        {
                                            var guaddress = GuarantorData[3].Split('-')[0] + GuarantorData[3].Split('-')[1] + GuarantorData[3].Split('-')[2] + GuarantorData[4];
                                            var guaddressContact = GuarantorData[5].Split('-')[0] + GuarantorData[5].Split('-')[1] + GuarantorData[5].Split('-')[2] + GuarantorData[6];
                                            doc.ReplaceText($"«房客保證人標題{i}»", "保證人：");                           //代理人標題
                                            doc.ReplaceText($"«房客保證人姓名{i}»", "姓名：" + GuarantorData[0] + "　　　　　　　　　　　　　　　　　　　　(簽章)");    //代理人姓名
                                            doc.ReplaceText($"«房客保證人身分證{i}»", "身分證字號：" + GuarantorData[1]);         //代理人身分證
                                            doc.ReplaceText($"«房客保證人戶籍地址{i}»", "戶籍地址：" + guaddress);            //代理人姓名
                                            doc.ReplaceText($"«房客保證人通訊地址{i}»", "通訊地址：" + guaddressContact);    //代理人通訊地址
                                            doc.ReplaceText($"«房客保證人電話{i}»", "連絡電話：" + Regex.Replace(GuarantorData[2], @"(\d{4})(\d{3})(\d{3})", "$1-$2-$3"));           //代理人電話
                                        }
                                        else
                                        {
                                            var paragraph0 = doc.Paragraphs.Where(p => p.Text.Contains($"«房客保證人標題{i}»")).FirstOrDefault();
                                            var paragraph1 = doc.Paragraphs.Where(p => p.Text.Contains($"«房客保證人姓名{i}»")).FirstOrDefault();
                                            var paragraph2 = doc.Paragraphs.Where(p => p.Text.Contains($"«房客保證人身分證{i}»")).FirstOrDefault();
                                            var paragraph3 = doc.Paragraphs.Where(p => p.Text.Contains($"«房客保證人戶籍地址{i}»")).FirstOrDefault();
                                            var paragraph4 = doc.Paragraphs.Where(p => p.Text.Contains($"«房客保證人通訊地址{i}»")).FirstOrDefault();
                                            var paragraph5 = doc.Paragraphs.Where(p => p.Text.Contains($"«房客保證人電話{i}»")).FirstOrDefault();
                                            if (paragraph0 != null)
                                            {
                                                doc.RemoveParagraph(paragraph0);
                                                doc.RemoveParagraph(paragraph1);
                                                doc.RemoveParagraph(paragraph2);
                                                doc.RemoveParagraph(paragraph3);
                                                doc.RemoveParagraph(paragraph4);
                                                doc.RemoveParagraph(paragraph5);
                                            }
                                        }
                                    }
                                }

                                doc.ReplaceText("«委託人»", nameClient);
                                break;
                            default:
                                break;
                        }

                        #region 存至暫存檔
                        string newFileName = $"{file}";
                        string tempFilePath = Path.Combine(tempFolderPath, newFileName);
                        // 保存修改後的文檔
                        if(file != "附表單2.代理人授權書-客.docx")
                            doc.SaveAs(tempFilePath);
                        // 關閉Word文件
                        doc.Dispose();
                        #endregion

                        #region 存至本檔案
                        //doc.Save();
                        //byte[] zipFileBytes = System.IO.File.ReadAllBytes(filePath);
                        #endregion

                        #region 檔案直接下載
                        //byte[] zipFileBytes = System.IO.File.ReadAllBytes(tempFilePath);
                        //return File(zipFileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "WordFiles");
                        #endregion
                    }
                }
                #endregion

                #region 附件下載檔
                var existfilelist = formdb.FileLog.Where(x => x.FormID == FormID).OrderBy(x => x.Category).AsEnumerable();

                // 創建名為"證件資料"的子文件夾
                string documentsFolderPath = Path.Combine(tempFolderPath, "證件資料");
                Directory.CreateDirectory(documentsFolderPath);
                // 創建名為"證件資料"的子文件夾
                string houseFolderPath = Path.Combine(tempFolderPath, "屋況資料");
                Directory.CreateDirectory(houseFolderPath);

                var categoryIndex = 0;
                var categorytemp = "";      //存取檔案類別，並再遇到相同檔案時，做名稱的轉換
                foreach (var fileitem in existfilelist)
                {
                    categoryIndex = categorytemp == fileitem.Category ? (categoryIndex + 1) : 0;
                    //選擇要下載之資料夾位址
                    var Map = "~/Uploads/" + fileitem.FilePath.Trim() + "/" + fileitem.FileAlias;

                    //將相對路徑轉為絕對路徑
                    var path = Server.MapPath(Map);

                    var file = new FileInfo(path);

                    //設定檔案下載的 ContentType
                    if (file.Exists)
                    {
                        #region 存至暫存檔
                        string newFileName =  categoryIndex == 0 ? $"{fileitem.Category}{Path.GetExtension(fileitem.FileNames)}" : $"{fileitem.Category}_{categoryIndex}{Path.GetExtension(fileitem.FileNames)}";
                        
                        
                        string tempFilePath = fileitem.FilePath.Trim() == "House" ? Path.Combine(houseFolderPath, newFileName) : Path.Combine(documentsFolderPath, newFileName);

                        // 複製原始檔案到臨時檔案名稱
                        file.CopyTo(tempFilePath);
                        // 保存修改後的文檔
                        //doc.SaveAs(tempFilePath);
                        #endregion

                    }

                    categorytemp = fileitem.Category;
                }

                #endregion



                #region 檔案壓縮下載
                string zipFileName = HomeObject.fulladdress + "_" + formagentName + "_" + DateTime.Now.ToString("_yyMMddHHmmsss") + ".zip";
                // 壓縮暫存文件夾中的所有文件到一個ZIP文件
                string zipFilePath = Path.Combine(Path.GetTempPath(), zipFileName);
                ZipFile.CreateFromDirectory(tempFolderPath, zipFilePath);
                Directory.Delete(tempFolderPath, true);

                // 将处理后的 Word 文档转换为字节数组
                //Directory.Delete(filePath, true);
                byte[] wordFileBytes = System.IO.File.ReadAllBytes(zipFilePath);

                byte[] zipFileBytes = System.IO.File.ReadAllBytes(zipFilePath);
                return File(zipFileBytes, "application/zip", zipFileName);
                #endregion

                #region 檔案封包下載
                //byte[] zipFileBytes = System.IO.File.ReadAllBytes(tempFolderPath);
                //return File(zipFileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "WordFiles");
                #endregion
               
            }
            catch (Exception ex)
            {
                logger.Error("匯出失敗 : " + ex.ToString());
                return Content(ex.ToString());
            }
        }
        #endregion
    }
}