using DocumentFormat.OpenXml.Wordprocessing;
using LiqunManagement.Models;
using LiqunManagement.ViewModels;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
using System.Web;



namespace LiqunManagement.Services
{
    public class FileService : BaseService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="FormID"></param>
        /// <param name="category">上傳資料類別</param>
        /// <param name="File">檔案</param>
        /// <param name="FilePath">檔案上傳路徑</param>
        /// <param name="UserAccount">使用者帳號</param>
        /// <returns></returns>
        #region 存檔
        public FileViewMode SaveFile(string FormID, string category, IEnumerable<HttpPostedFileBase> File, string FilePath, string UserAccount)
        {
            var existHomeObject = formdb.HomeObject.Where(x => x.FormID == FormID).FirstOrDefault();
            var existTenant = formdb.Tenant.Where(x => x.FormID == FormID).FirstOrDefault();
            //若已有資料存在
            // 使用 JsonConvert.DeserializeObject 将 JSON 字符串转换为 List<string>
            //取得檔名與檔案GUID
            List<string> fileNamesArray = new List<string>();
            List<string> taxfile_aliasArray = new List<string>();
            var type = "Form";
            switch (category)
            {
                case "房屋稅單":
                    if(existHomeObject != null)
                    {
                        if (existHomeObject.taxfile_name != null)
                        {
                            fileNamesArray = JsonConvert.DeserializeObject<List<string>>(existHomeObject.taxfile_name);
                            taxfile_aliasArray = JsonConvert.DeserializeObject<List<string>>(existHomeObject.taxfile_alias);
                        }
                    }
                    break;

                case "弱勢戶佐證文件":
                    if (existTenant != null)
                    {
                        if (existTenant.vulnerablefile_Name != null)
                        {
                            fileNamesArray = JsonConvert.DeserializeObject<List<string>>(existTenant.vulnerablefile_Name);
                            taxfile_aliasArray = JsonConvert.DeserializeObject<List<string>>(existTenant.vulnerablefile_Alias);
                        }
                    }
                    break;

                case "上傳300億試算表截圖":
                    if (existTenant != null)
                    {
                        if (existTenant.sheetfile_Name != null)
                        {
                            fileNamesArray = JsonConvert.DeserializeObject<List<string>>(existTenant.sheetfile_Name);
                            taxfile_aliasArray = JsonConvert.DeserializeObject<List<string>>(existTenant.sheetfile_Alias);
                        }
                    }
                    break;
                default:
                    type = "else";
                    break;
            }

            //存檔
            try
            {
                if (File != null && File.Any())
                {
                    foreach (var file in File)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            logger.Info("找到檔案資料 fileName : " + file.FileName);
                            string name = Path.GetFileName(file.FileName);
                            fileNamesArray.Add(name);
                            string alias = Guid.NewGuid().ToString() + Path.GetExtension(name);
                            taxfile_aliasArray.Add(alias);
                            logger.Info("合成路徑 FilePath : " + FilePath);
                            string path = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/" + FilePath), alias);
                            file.SaveAs(path);

                            logger.Info("存檔完成，執行變更FileLog");

                            //儲存資料
                            using (var context = new FormModels())
                            {
                                var FileData = new FileLog
                                {
                                    FormID = FormID,
                                    Category = category,
                                    FileNames = name,
                                    FileAlias = alias,
                                    FileExtension = Path.GetExtension(name),
                                    FilePath = FilePath,
                                    CreateTime = DateTime.Now,
                                    CreateAccount = UserAccount,
                                    UpdateTime = DateTime.Now,
                                    UpdateAccount = UserAccount,
                                };

                                context.FileLog.Add(FileData);
                                context.SaveChanges();
                            }
                        }


                    }
                }
            }
            catch(Exception ex)
            {
                logger.Info("存檔錯誤 : " + ex.ToString());
                MailService mailService = new MailService();
                mailService.SendMail("【力群管理系統】物件資料(taxFile)存檔錯誤", ex.ToString(), "cys.enki@gmail.com");
                return null;
            }

            var filemodel = new FileViewMode()
            {
                FileName = JsonConvert.SerializeObject(fileNamesArray),
                FileAlias = JsonConvert.SerializeObject(taxfile_aliasArray),
            };
            return filemodel;
        }
        #endregion

        #region 刪檔
        public bool DeleteFile(string FileAlias)
        {
            //確認資料庫內是否有資料
            var ExistData = formdb.FileLog.Where(x => x.FileAlias == FileAlias).FirstOrDefault();
            try
            {
                if (ExistData != null)
                {
                    string filePath = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/Uploads/Reason"), FileAlias);

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                    else
                    {
                        // 文件不存在，可能需要处理错误或提供相关消息
                    }

                    formdb.FileLog.Remove(ExistData);
                    formdb.SaveChanges();
                }
                return true;
            }
            catch(Exception ex)
            {
                MailService mailService = new MailService();
                mailService.SendMail("【力群管理系統】物件資料(taxFile)存檔錯誤", ex.ToString(), "cys.enki@gmail.com");
                return false;
            }
        }
        #endregion
    }
}