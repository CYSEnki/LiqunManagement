﻿using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using LiqunManagement.Models;
using LiqunManagement.Services;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using ICell = NPOI.SS.UserModel.ICell;
using LiqunManagement.ViewModels;
using NPOI.SS.Formula.Functions;
using System.Data.SqlClient;
using System.Configuration;
using LiqunManagement.Attributes;
using NPOI.Util;

namespace LiqunManagement.Controllers
{
    [AdminAuthorize]
    public class UpLoadController : BaseController
    {
        //建立與資料庫的連線字串
        private readonly static string cnstr = ConfigurationManager.ConnectionStrings["FormModels"].ConnectionString;
        //建立與資料庫的連線
        private readonly SqlConnection conn = new SqlConnection(cnstr);

        //將錯誤訊息字串存取至List
        List<string> errorlist = new List<string>();
        [HttpGet]
        // GET: UpLoad
        public ActionResult Index()
        {
            return View();
        }

        #region 上傳臺灣地址(Region)資料
        [HttpGet]
        public ActionResult UploadRegion()
        {
            logger.Info("UploadRegion Get");
            logger.Info("取得使用者資料");
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
                logger.Info("姓名:" + EmployeeData.Name + "部門:" + EmployeeData.Department);
            }
            //確認角色
            var IsAdmin = User.IsInRole("Admin");
            ViewBag.Role = User.IsInRole("Admin") ? "Admin" : User.IsInRole("Agent") ? "Agent" : User.IsInRole("Secretary") ? "Secretary" : "";
            #endregion

            ViewBag.Message = "Initial";

            //UploadService uploadservice = new UploadService();
            //var model = uploadservice.GetRegionData();

            logger.Info("回傳Views");
            return View();
        }
        [HttpPost]
        public ActionResult UploadRegion(HttpPostedFileBase file)
        {
            string errorstring = "";
            List<string> errorstringlist = new List<string>();
            ViewBag.Message = "匯入成功";
            if (file != null)
            {
                //若上傳資料內有資料則執行此動作
                Stream stream = file.InputStream; //使用Stream(流)對檔案進行操作
                DataTable dataTable = new DataTable();
                DataTable datarow = new DataTable();
                IWorkbook wb;   //存取XLSM或XLS版本
                ISheet sheet;   //存取頁籤
                IRow headerRow; //存取第一列
                int cellCount;  //紀錄共有幾欄
                try
                {
                    //依excel版本，NPOI載入檔案
                    if (file.FileName.ToUpper().EndsWith("XLSX"))
                        wb = new XSSFWorkbook(stream); // excel版本(.xlsx)
                    else
                        wb = new HSSFWorkbook(stream); // excel版本(.xls)

                    //取第一個頁籤   
                    sheet = wb.GetSheetAt(0);
                    //取第一個頁籤的第一列
                    headerRow = sheet.GetRow(0);
                    //計算出第一列共有多少欄位
                    cellCount = headerRow.LastCellNum;

                    //迴圈執行第一列的第一個欄位到最後一個欄位，將抓到的值塞進DataTable做完欄位名稱
                    for (int i = headerRow.FirstCellNum; i < cellCount; i++)
                    {
                        if (headerRow.GetCell(i) != null)
                            dataTable.Columns.Add(new DataColumn(headerRow.GetCell(i).StringCellValue));
                        else//null 則放空白
                            dataTable.Columns.Add(new DataColumn(""));
                    }

                    int column = 0; //計算每一列讀到第幾個欄位

                    // 略過第零列(標題列)，一直處理至最後一列
                    for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
                    {
                        //取目前的列(row)
                        IRow row = sheet.GetRow(i);
                        if (row == null)
                        {
                            break;
                        }
                        string first = " ";
                        ICell firstcell = row.GetCell(0, MissingCellPolicy.RETURN_NULL_AND_BLANK);
                        if (firstcell != null)
                        {
                            first = firstcell.ToString();
                        }
                        bool check = firstcolumncheck(i, first, cellCount, row);   //確認該列資料第一欄未輸入資料

                        if (check == true) break;

                        //宣告DataRow
                        DataRow dataRow = dataTable.NewRow();
                        //宣告ICell
                        ICell cell;
                        try
                        {
                            //依先前取得，依每一列的欄位數，逐一設定欄位內容
                            for (int j = row.FirstCellNum; j < cellCount; j++)
                            {
                                //計算每一列讀到第幾個欄位(秀在錯誤訊息上)
                                column = j;

                                //設定cell為目前第j欄位
                                cell = row.GetCell(j, MissingCellPolicy.RETURN_NULL_AND_BLANK);

                                checkcell_region(i, j, cell);
                                if (cell != null) //若cell有值
                                {
                                    //用cell.CellType判斷資料的型別
                                    //再依照欄位屬性，用StringCellValue、DateCellValue、NumericCellValue、DateCellValue取值
                                    switch (cell.CellType)
                                    {
                                        //字串型態欄位
                                        case NPOI.SS.UserModel.CellType.String:
                                            //設定dataRow第j欄位的值，cell以字串型態取值
                                            dataRow[j] = cell.StringCellValue;
                                            break;

                                        //數字型態欄位
                                        case NPOI.SS.UserModel.CellType.Numeric:

                                            if (HSSFDateUtil.IsCellDateFormatted(cell)) //日期格式
                                            {
                                                //設定dataRow第j欄位的值，cell以日期格式取值
                                                dataRow[j] = DateTime.FromOADate(cell.NumericCellValue).ToString("yyyy/MM/dd HH:mm:ss");
                                            }
                                            else //非日期格式
                                            {
                                                //設定dataRow第j欄位的值，cell以數字型態取值
                                                dataRow[j] = cell.NumericCellValue;
                                            }
                                            break;

                                        //布林值
                                        case NPOI.SS.UserModel.CellType.Boolean:

                                            //設定dataRow第j欄位的值，cell以布林型態取值
                                            dataRow[j] = cell.BooleanCellValue;
                                            break;

                                        //空值
                                        case NPOI.SS.UserModel.CellType.Blank:

                                            dataRow[j] = "";
                                            break;

                                        // 預設
                                        default:

                                            dataRow[j] = cell.StringCellValue;
                                            break;
                                    }
                                }
                            }
                            //DataTable加入dataRow
                            dataTable.Rows.Add(dataRow);
                        }
                        catch (Exception ex)
                        {
                            //錯誤訊息
                            throw new Exception("第 " + i + "列第" + column + "欄，資料格式有誤:\r\r" + ex.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "匯入失敗";
                    ViewBag.ErrorMessage = ex.ToString();
                    errorstring = "匯入失敗，請檢查匯入格式";
                    var error = ex.ToString();
                    errorlist.Add(errorstring);
                    ViewBag.Errorlist = errorlist;
                }
                finally
                {
                    //釋放資源
                    sheet = null;
                    wb = null;
                    stream.Dispose();
                    stream.Close();
                }

                if (errorlist.Count >= 1)
                {
                    ViewBag.Message = "匯入失敗";
                    ViewBag.Errorlist = errorlist;
                    return View();
                }
                var CodeA = 'A';
                var Codea = 'a';
                var Code1 = 1;
                var Code2 = 1;

                var CityName = "";
                var DistrictName = "";
                var RoadName = "";
                try
                {
                    //using (var context = new FormModels())
                    //{
                    //    // 取得所有資料
                    //    var allData = context.Region.AsEnumerable();

                    //    // 移除所有資料
                    //    context.Region.RemoveRange(allData);
                    //    // 儲存更改到資料庫
                    //    context.SaveChanges();
                    //}
                    //Sql語法
                    string sql = $@"TRUNCATE TABLE [Liqun].[Form].[Region]; ";

                    //確保程式不會因執行錯誤而整個中斷
                    try
                    {
                        //開啟資料庫連線
                        conn.Open();
                        //執行Sql指令
                        SqlCommand cmd = new SqlCommand(sql, conn);
                        SqlDataReader dr = cmd.ExecuteReader();
                    }
                    catch (Exception e)
                    {
                        //丟出錯誤
                        throw new Exception(e.Message.ToString());
                    }
                    finally
                    {
                        //關閉資料庫連線
                        conn.Close();
                    }


                }
                catch(Exception ex)
                {
                    ViewBag.Message = "匯入失敗";
                    ViewBag.ErrorMessage = ex.ToString();
                }
                RegionViewModel regionviewmodel = new RegionViewModel();
                //dataTable跑回圈，insert資料至DB
                foreach (DataRow dr in dataTable.Rows)
                {
                    try
                    {
                        if(CityName == "")
                        {
                            CityName = dr["縣市名稱"].ToString();
                            DistrictName = dr["行政區域名稱"].ToString();
                            RoadName = dr["路名"].ToString();
                        }
                        else
                        {
                            if (CityName != dr["縣市名稱"].ToString())
                            {
                                if (Codea == 'z')
                                {
                                    CodeA++;
                                }
                                else
                                {
                                    Codea++;
                                }
                                Code1 = Code2 = 1;
                                CityName = dr["縣市名稱"].ToString();
                            }
                            if (DistrictName != dr["行政區域名稱"].ToString())
                            {
                                Code1++;
                                Code2 = 1;
                                DistrictName = dr["行政區域名稱"].ToString();
                            }
                            if (RoadName != dr["路名"].ToString())
                            {
                                Code2++;
                                RoadName = dr["路名"].ToString();
                            }
                        }


                        var CityCode = CodeA.ToString() + Codea.ToString();
                        var datedata = new RegionViewModel()
                        {
                            CityOrder = Convert.ToInt32(dr["排序"]),
                            City = dr["縣市名稱"].ToString(),
                            CityCode = CityCode,
                            District = dr["行政區域名稱"].ToString(),
                            DistrictCode = CityCode + Code1.ToString("D2"),
                            Road = dr["路名"].ToString(),
                            RoadCode = CityCode + Code1.ToString("D2") + Code2.ToString("D3"),
                        };
                        UploadService uploadservice = new UploadService();
                        uploadservice.InsertRegion(datedata);
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Message = "匯入失敗，請檢查格式";
                        ViewBag.ErrorMessage = ex.ToString();
                        errorstring = "匯入失敗，請重新確認資料內容。";
                        errorlist.Add(errorstring);
                        ViewBag.Errorlist = errorlist;
                        return View();
                    }
                }
            }
            else
            {
                ViewBag.Message = "請選擇檔案";
                return View();
            }
            string correctstring = "";
            List<string> correctlist = new List<string>();
            correctlist.Add(correctstring);
            return View();
        }

        #region 驗證Excel匯入資料正確性
        //將列數(rowcell)、欄數(columncell)、單元格內容(cell)參數傳入後，驗證資料
        public void checkcell_region(int rowcell, int columncell, ICell cell)
        {
            //存取錯誤訊息字串
            string errorstring = "";
            //計算每一列讀到第幾個欄位(秀在錯誤訊息上)，欄位數字加一因為參數(columncell)從0開始數
            int columnnumber = columncell + 1;
            //讀取參數(rowcell)設strrow為第幾列
            string strrow = rowcell.ToString();
            //讀取欄位數(columnnumber)設strcolumn為第幾列
            string strcolumn = columnnumber.ToString();

            #region 取得單元格型別
            var Categorycelltype = "Spacetype";
            if (cell != null)
            {
                switch (cell.CellType)
                {
                    //字串型態欄位
                    case NPOI.SS.UserModel.CellType.String:
                        //設定dataRow第j欄位的值，cell以字串型態取值
                        Categorycelltype = "Stringtype";
                        break;

                    //數字型態欄位
                    case NPOI.SS.UserModel.CellType.Numeric:

                        if (HSSFDateUtil.IsCellDateFormatted(cell)) //日期格式
                        {
                            //設定dataRow第j欄位的值，cell以日期格式取值
                            Categorycelltype = "Datetype";
                        }
                        else //非日期格式
                        {
                            //設定dataRow第j欄位的值，cell以數字型態取值
                            Categorycelltype = "Numerictype";
                        }
                        break;

                    //布林值
                    case NPOI.SS.UserModel.CellType.Boolean:
                        Categorycelltype = "Booltype";
                        break;

                    //空值
                    case NPOI.SS.UserModel.CellType.Blank:
                        Categorycelltype = "Spacetype";
                        break;

                    // 預設
                    default:
                        //content = cell.StringCellValue;
                        Categorycelltype = "Stringtype";
                        break;
                }
            }
            #endregion

            //是否為空單元格
            if (Categorycelltype == "Spacetype")
            {
                errorstring = "第" + strrow + "列主項目(第" + strcolumn + "欄)，資料格式不可為空";
                errorlist.Add(errorstring);
            }
            else
            {
                switch (columnnumber)
                {
                    case 1:
                        //是否為字串型別或數字型別
                        if (Categorycelltype == "Stringtype" || Categorycelltype == "Numerictype")
                        {
                            string ItemNo = cell.ToString();
                            if (ItemNo != null)
                            {
                                if (ItemNo.Length > 50)
                                {
                                    errorstring = "第" + strrow + "列產品編號(第" + strcolumn + "欄)，資料長度過長";
                                    errorlist.Add(errorstring);
                                }
                            }
                        }
                        else
                        {
                            errorstring = "第" + strrow + "列產品編號(第" + strcolumn + "欄)，格式不正確，請再次確認是否為產品編號格式";
                            errorlist.Add(errorstring);
                        }
                        break;

                    default:
                        //是否為字串型別
                        if (Categorycelltype != "Stringtype")
                        {
                            errorstring = "第" + strrow + "列主項目(第" + strcolumn + "欄)，格式不正確，請再次確認是否為字串型別";
                            errorlist.Add(errorstring);
                        }
                        else
                        {
                            string Category = cell.ToString();
                            if (Category != null)
                            {
                                if (Category.Length > 20)
                                {
                                    errorstring = "第" + strrow + "列主項目(第" + strcolumn + "欄)，資料長度過長";
                                    errorlist.Add(errorstring);
                                }
                            }
                        }
                        break;
                }
            }
        }
        #endregion
        #endregion

        #region 上傳銀行(Bank)資料
        [HttpGet]
        public ActionResult UploadBank()
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
            var IsAdmin = User.IsInRole("Admin");
            ViewBag.Role = User.IsInRole("Admin") ? "Admin" : User.IsInRole("Agent") ? "Agent" : User.IsInRole("Secretary") ? "Secretary" : "";
            #endregion
            var UserName = User.Identity.Name;
            ViewBag.Message = "Initial";
            //UploadService uploadservice = new UploadService();
            //var model = uploadservice.GetRegionData();

            return View();
        }
        [HttpPost]
        public ActionResult UploadBank(HttpPostedFileBase file)
        {
            string errorstring = "";
            List<string> errorstringlist = new List<string>();
            ViewBag.Message = "匯入成功";
            if (file != null)
            {
                //若上傳資料內有資料則執行此動作
                Stream stream = file.InputStream; //使用Stream(流)對檔案進行操作
                DataTable dataTable = new DataTable();
                DataTable datarow = new DataTable();
                IWorkbook wb;   //存取XLSM或XLS版本
                ISheet sheet;   //存取頁籤
                IRow headerRow; //存取第一列
                int cellCount;  //紀錄共有幾欄
                try
                {
                    //依excel版本，NPOI載入檔案
                    if (file.FileName.ToUpper().EndsWith("XLSX"))
                        wb = new XSSFWorkbook(stream); // excel版本(.xlsx)
                    else
                        wb = new HSSFWorkbook(stream); // excel版本(.xls)

                    //取第一個頁籤   
                    sheet = wb.GetSheetAt(0);
                    //取第一個頁籤的第一列
                    headerRow = sheet.GetRow(0);
                    //計算出第一列共有多少欄位
                    cellCount = headerRow.LastCellNum;

                    //迴圈執行第一列的第一個欄位到最後一個欄位，將抓到的值塞進DataTable做完欄位名稱
                    for (int i = headerRow.FirstCellNum; i < cellCount; i++)
                    {
                        if (headerRow.GetCell(i) != null)
                            dataTable.Columns.Add(new DataColumn(headerRow.GetCell(i).StringCellValue));
                        else//null 則放空白
                            dataTable.Columns.Add(new DataColumn(""));
                    }

                    int column = 0; //計算每一列讀到第幾個欄位

                    // 略過第零列(標題列)，一直處理至最後一列
                    for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
                    {
                        //取目前的列(row)
                        IRow row = sheet.GetRow(i);
                        if (row == null)
                        {
                            break;
                        }
                        string first = " ";
                        ICell firstcell = row.GetCell(0, MissingCellPolicy.RETURN_NULL_AND_BLANK);
                        if (firstcell != null)
                        {
                            first = firstcell.ToString();
                        }
                        bool check = firstcolumncheck(i, first, cellCount, row);   //確認該列資料第一欄未輸入資料

                        if (check == true) break;

                        //宣告DataRow
                        DataRow dataRow = dataTable.NewRow();
                        //宣告ICell
                        ICell cell;
                        try
                        {
                            //依先前取得，依每一列的欄位數，逐一設定欄位內容
                            for (int j = row.FirstCellNum; j < cellCount; j++)
                            {
                                //計算每一列讀到第幾個欄位(秀在錯誤訊息上)
                                column = j;

                                //設定cell為目前第j欄位
                                cell = row.GetCell(j, MissingCellPolicy.RETURN_NULL_AND_BLANK);

                                checkcell_bank(i, j, cell);
                                if (cell != null) //若cell有值
                                {
                                    //用cell.CellType判斷資料的型別
                                    //再依照欄位屬性，用StringCellValue、DateCellValue、NumericCellValue、DateCellValue取值
                                    switch (cell.CellType)
                                    {
                                        //字串型態欄位
                                        case NPOI.SS.UserModel.CellType.String:
                                            //設定dataRow第j欄位的值，cell以字串型態取值
                                            dataRow[j] = cell.StringCellValue;
                                            break;

                                        //數字型態欄位
                                        case NPOI.SS.UserModel.CellType.Numeric:

                                            if (HSSFDateUtil.IsCellDateFormatted(cell)) //日期格式
                                            {
                                                //設定dataRow第j欄位的值，cell以日期格式取值
                                                dataRow[j] = DateTime.FromOADate(cell.NumericCellValue).ToString("yyyy/MM/dd HH:mm:ss");
                                            }
                                            else //非日期格式
                                            {
                                                //設定dataRow第j欄位的值，cell以數字型態取值
                                                dataRow[j] = cell.NumericCellValue;
                                            }
                                            break;

                                        //布林值
                                        case NPOI.SS.UserModel.CellType.Boolean:

                                            //設定dataRow第j欄位的值，cell以布林型態取值
                                            dataRow[j] = cell.BooleanCellValue;
                                            break;

                                        //空值
                                        case NPOI.SS.UserModel.CellType.Blank:

                                            dataRow[j] = "";
                                            break;

                                        // 預設
                                        default:

                                            dataRow[j] = cell.StringCellValue;
                                            break;
                                    }
                                }
                            }
                            //DataTable加入dataRow
                            dataTable.Rows.Add(dataRow);
                        }
                        catch (Exception ex)
                        {
                            //錯誤訊息
                            throw new Exception("第 " + i + "列第" + column + "欄，資料格式有誤:\r\r" + ex.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "匯入失敗";
                    ViewBag.ErrorMessage = ex.ToString();
                    errorstring = "匯入失敗，請檢查匯入格式";
                    var error = ex.ToString();
                    errorlist.Add(errorstring);
                    ViewBag.Errorlist = errorlist;
                }
                finally
                {
                    //釋放資源
                    sheet = null;
                    wb = null;
                    stream.Dispose();
                    stream.Close();
                }

                if (errorlist.Count >= 1)
                {
                    ViewBag.Message = "匯入失敗";
                    ViewBag.Errorlist = errorlist;
                    return View();
                }
                try
                {
                    //using (var context = new FormModels())
                    //{
                    //    // 取得所有資料
                    //    var allData = context.Region.AsEnumerable();

                    //    // 移除所有資料
                    //    context.Region.RemoveRange(allData);
                    //    // 儲存更改到資料庫
                    //    context.SaveChanges();
                    //}
                    //Sql語法
                    string sql = $@"TRUNCATE TABLE [Liqun].[Form].[Bank]; ";

                    //確保程式不會因執行錯誤而整個中斷
                    try
                    {
                        //開啟資料庫連線
                        conn.Open();
                        //執行Sql指令
                        SqlCommand cmd = new SqlCommand(sql, conn);
                        SqlDataReader dr = cmd.ExecuteReader();
                    }
                    catch (Exception e)
                    {
                        //丟出錯誤
                        throw new Exception(e.Message.ToString());
                    }
                    finally
                    {
                        //關閉資料庫連線
                        conn.Close();
                    }


                }
                catch(Exception ex)
                {
                    ViewBag.Message = "匯入失敗";
                    ViewBag.ErrorMessage = ex.ToString();
                }
                //dataTable跑回圈，insert資料至DB
                foreach (DataRow dr in dataTable.Rows)
                {
                    try
                    {
                        var datedata = new BankViewModel()
                        {
                            BankRegion = dr["分區"].ToString(),
                            RootCheck = dr["根目錄"].ToString() == "0" ? false : true,
                            BankCode = dr["總機構代號"].ToString(),
                            BankName = String.IsNullOrEmpty(dr["總機構名稱"].ToString()) ? null : dr["總機構名稱"].ToString(),
                            BranchCode = String.IsNullOrEmpty(dr["分支機構代號"].ToString()) ? null : dr["分支機構代號"].ToString(),
                            BranchName = String.IsNullOrEmpty(dr["分行機構名稱"].ToString()) ? null : dr["分行機構名稱"].ToString(),
                            BranchFullName = String.IsNullOrEmpty(dr["機構名稱"].ToString()) ? null : dr["機構名稱"].ToString(),
                            CodeMinlength = String.IsNullOrEmpty(dr["最小帳號長度"].ToString()) ? null : dr["最小帳號長度"].ToString(),
                            CodeMaxlength = String.IsNullOrEmpty(dr["最大帳號長度"].ToString()) ? null : dr["最大帳號長度"].ToString(),
                        };
                        UploadService uploadservice = new UploadService();
                        uploadservice.InsertBank(datedata);
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Message = "匯入失敗，請檢查格式";
                        ViewBag.ErrorMessage = ex.ToString();
                        errorstring = "匯入失敗，請重新確認資料內容。";
                        errorlist.Add(errorstring);
                        ViewBag.Errorlist = errorlist;
                        return View();
                    }
                }
            }
            else
            {
                ViewBag.Message = "請選擇檔案";
                return View();
            }
            string correctstring = "";
            List<string> correctlist = new List<string>();
            correctlist.Add(correctstring);
            return View();
        }

        #region 驗證Excel匯入資料正確性
        //將列數(rowcell)、欄數(columncell)、單元格內容(cell)參數傳入後，驗證資料
        public void checkcell_bank(int rowcell, int columncell, ICell cell)
        {
            //存取錯誤訊息字串
            string errorstring = "";
            //計算每一列讀到第幾個欄位(秀在錯誤訊息上)，欄位數字加一因為參數(columncell)從0開始數
            int columnnumber = columncell + 1;
            //讀取參數(rowcell)設strrow為第幾列
            string strrow = rowcell.ToString();
            //讀取欄位數(columnnumber)設strcolumn為第幾列
            string strcolumn = columnnumber.ToString();

            #region 取得單元格型別
            var Categorycelltype = "Spacetype";
            if (cell != null)
            {
                switch (cell.CellType)
                {
                    //字串型態欄位
                    case NPOI.SS.UserModel.CellType.String:
                        //設定dataRow第j欄位的值，cell以字串型態取值
                        Categorycelltype = "Stringtype";
                        break;

                    //數字型態欄位
                    case NPOI.SS.UserModel.CellType.Numeric:

                        if (HSSFDateUtil.IsCellDateFormatted(cell)) //日期格式
                        {
                            //設定dataRow第j欄位的值，cell以日期格式取值
                            Categorycelltype = "Datetype";
                        }
                        else //非日期格式
                        {
                            //設定dataRow第j欄位的值，cell以數字型態取值
                            Categorycelltype = "Numerictype";
                        }
                        break;

                    //布林值
                    case NPOI.SS.UserModel.CellType.Boolean:
                        Categorycelltype = "Booltype";
                        break;

                    //空值
                    case NPOI.SS.UserModel.CellType.Blank:
                        Categorycelltype = "Spacetype";
                        break;

                    // 預設
                    default:
                        //content = cell.StringCellValue;
                        Categorycelltype = "Stringtype";
                        break;
                }
            }
            #endregion

            else
            {
                switch (columnnumber)
                {
                    case 1:
                        //是否為字串型別或數字型別
                        if (Categorycelltype == "Stringtype")
                        {
                            string ItemNo = cell.ToString();
                            if (ItemNo != null)
                            {
                                if (ItemNo.Length > 50)
                                {
                                    errorstring = "第" + strrow + "列分區(第" + strcolumn + "欄)，資料長度過長";
                                    errorlist.Add(errorstring);
                                }
                            }
                        }
                        else
                        {
                            errorstring = "第" + strrow + "列分區(第" + strcolumn + "欄)，格式不正確，請再次確認是否為銀行分區";
                            errorlist.Add(errorstring);
                        }
                        break;
                }
            }
        }
        #endregion
        #endregion

        #region 上傳段/小段/事務所(Excerpt)資料
        [HttpGet]
        public ActionResult UploadExcerpt()
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
            var IsAdmin = User.IsInRole("Admin");
            ViewBag.Role = User.IsInRole("Admin") ? "Admin" : User.IsInRole("Agent") ? "Agent" : User.IsInRole("Secretary") ? "Secretary" : "";
            #endregion
            var UserName = User.Identity.Name;
            ViewBag.Message = "Initial";
            //UploadService uploadservice = new UploadService();
            //var model = uploadservice.GetRegionData();

            return View();
        }
        [HttpPost]
        public ActionResult UploadExcerpt(HttpPostedFileBase file)
        {
            string errorstring = "";
            List<string> errorstringlist = new List<string>();
            ViewBag.Message = "匯入成功";
            if (file != null)
            {
                //若上傳資料內有資料則執行此動作
                Stream stream = file.InputStream; //使用Stream(流)對檔案進行操作
                DataTable dataTable = new DataTable();
                DataTable datarow = new DataTable();
                IWorkbook wb;   //存取XLSM或XLS版本
                ISheet sheet;   //存取頁籤
                IRow headerRow; //存取第一列
                int cellCount;  //紀錄共有幾欄
                try
                {
                    //依excel版本，NPOI載入檔案
                    if (file.FileName.ToUpper().EndsWith("XLSX"))
                        wb = new XSSFWorkbook(stream); // excel版本(.xlsx)
                    else
                        wb = new HSSFWorkbook(stream); // excel版本(.xls)

                    //取第一個頁籤   
                    sheet = wb.GetSheetAt(0);
                    //取第一個頁籤的第一列
                    headerRow = sheet.GetRow(0);
                    //計算出第一列共有多少欄位
                    cellCount = headerRow.LastCellNum;

                    //迴圈執行第一列的第一個欄位到最後一個欄位，將抓到的值塞進DataTable做完欄位名稱
                    for (int i = headerRow.FirstCellNum; i < cellCount; i++)
                    {
                        if (headerRow.GetCell(i) != null)
                            dataTable.Columns.Add(new DataColumn(headerRow.GetCell(i).StringCellValue));
                        else//null 則放空白
                            dataTable.Columns.Add(new DataColumn(""));
                    }

                    int column = 0; //計算每一列讀到第幾個欄位

                    // 略過第零列(標題列)，一直處理至最後一列
                    for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
                    {
                        //取目前的列(row)
                        IRow row = sheet.GetRow(i);
                        if (row == null)
                        {
                            break;
                        }
                        string first = " ";
                        ICell firstcell = row.GetCell(0, MissingCellPolicy.RETURN_NULL_AND_BLANK);
                        if (firstcell != null)
                        {
                            first = firstcell.ToString();
                        }
                        bool check = firstcolumncheck(i, first, cellCount, row);   //確認該列資料第一欄未輸入資料

                        if (check == true) break;

                        //宣告DataRow
                        DataRow dataRow = dataTable.NewRow();
                        //宣告ICell
                        ICell cell;
                        try
                        {
                            //依先前取得，依每一列的欄位數，逐一設定欄位內容
                            for (int j = row.FirstCellNum; j < cellCount; j++)
                            {
                                //計算每一列讀到第幾個欄位(秀在錯誤訊息上)
                                column = j;

                                //設定cell為目前第j欄位
                                cell = row.GetCell(j, MissingCellPolicy.RETURN_NULL_AND_BLANK);

                                checkcell_excerpt(i, j, cell);
                                if (cell != null) //若cell有值
                                {
                                    //用cell.CellType判斷資料的型別
                                    //再依照欄位屬性，用StringCellValue、DateCellValue、NumericCellValue、DateCellValue取值
                                    switch (cell.CellType)
                                    {
                                        //字串型態欄位
                                        case NPOI.SS.UserModel.CellType.String:
                                            //設定dataRow第j欄位的值，cell以字串型態取值
                                            dataRow[j] = cell.StringCellValue;
                                            break;

                                        //數字型態欄位
                                        case NPOI.SS.UserModel.CellType.Numeric:

                                            if (HSSFDateUtil.IsCellDateFormatted(cell)) //日期格式
                                            {
                                                //設定dataRow第j欄位的值，cell以日期格式取值
                                                dataRow[j] = DateTime.FromOADate(cell.NumericCellValue).ToString("yyyy/MM/dd HH:mm:ss");
                                            }
                                            else //非日期格式
                                            {
                                                //設定dataRow第j欄位的值，cell以數字型態取值
                                                dataRow[j] = cell.NumericCellValue;
                                            }
                                            break;

                                        //布林值
                                        case NPOI.SS.UserModel.CellType.Boolean:

                                            //設定dataRow第j欄位的值，cell以布林型態取值
                                            dataRow[j] = cell.BooleanCellValue;
                                            break;

                                        //空值
                                        case NPOI.SS.UserModel.CellType.Blank:

                                            dataRow[j] = "";
                                            break;

                                        // 預設
                                        default:

                                            dataRow[j] = cell.StringCellValue;
                                            break;
                                    }
                                }
                            }
                            //DataTable加入dataRow
                            dataTable.Rows.Add(dataRow);
                        }
                        catch (Exception ex)
                        {
                            //錯誤訊息
                            throw new Exception("第 " + i + "列第" + column + "欄，資料格式有誤:\r\r" + ex.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "匯入失敗";
                    ViewBag.ErrorMessage = ex.ToString();
                    errorstring = "匯入失敗，請檢查匯入格式";
                    var error = ex.ToString();
                    errorlist.Add(errorstring);
                    ViewBag.Errorlist = errorlist;
                }
                finally
                {
                    //釋放資源
                    sheet = null;
                    wb = null;
                    stream.Dispose();
                    stream.Close();
                }

                if (errorlist.Count >= 1)
                {
                    ViewBag.Message = "匯入失敗";
                    ViewBag.Errorlist = errorlist;
                    return View();
                }
                try
                {
                    //using (var context = new FormModels())
                    //{
                    //    // 取得所有資料
                    //    var allData = context.Region.AsEnumerable();

                    //    // 移除所有資料
                    //    context.Region.RemoveRange(allData);
                    //    // 儲存更改到資料庫
                    //    context.SaveChanges();
                    //}
                    //Sql語法
                    string sql = $@"TRUNCATE TABLE [Liqun].[Form].[Excerpt]; ";

                    //確保程式不會因執行錯誤而整個中斷
                    try
                    {
                        //開啟資料庫連線
                        conn.Open();
                        //執行Sql指令
                        SqlCommand cmd = new SqlCommand(sql, conn);
                        SqlDataReader dr = cmd.ExecuteReader();
                    }
                    catch (Exception e)
                    {
                        //丟出錯誤
                        throw new Exception(e.Message.ToString());
                    }
                    finally
                    {
                        //關閉資料庫連線
                        conn.Close();
                    }


                }
                catch(Exception ex)
                {
                    ViewBag.Message = "匯入失敗";
                    ViewBag.ErrorMessage = ex.ToString();
                }
                //dataTable跑回圈，insert資料至DB
                foreach (DataRow dr in dataTable.Rows)
                {
                    try
                    {
                        var excerptdata = new ExcerptViewModel()
                        {
                            CityCode = dr["縣市代碼"].ToString(),
                            CityName = dr["縣市名稱"].ToString(),
                            DistrictCode = dr["鄉鎮市區代碼"].ToString(),
                            DistrictName = String.IsNullOrEmpty(dr["鄉鎮市區名稱"].ToString()) ? null : dr["鄉鎮市區名稱"].ToString(),
                            OfficeCode = String.IsNullOrEmpty(dr["事務所代碼"].ToString()) ? null : dr["事務所代碼"].ToString(),
                            OfficeName = String.IsNullOrEmpty(dr["事務所名稱"].ToString()) ? null : dr["事務所名稱"].ToString(),
                            Excerpt1 = String.IsNullOrEmpty(dr["段"].ToString()) ? null : dr["段"].ToString(),
                            ExcerptShort = String.IsNullOrEmpty(dr["小段"].ToString()) ? null : dr["小段"].ToString(),
                            ExcerptCode = String.IsNullOrEmpty(dr["段名代碼"].ToString()) ? null : dr["段名代碼"].ToString(),
                        };
                        UploadService uploadservice = new UploadService();
                        uploadservice.InsertExcerpt(excerptdata);
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Message = "匯入失敗，請檢查格式";
                        ViewBag.ErrorMessage = ex.ToString();
                        errorstring = "匯入失敗，請重新確認資料內容。";
                        errorlist.Add(errorstring);
                        ViewBag.Errorlist = errorlist;
                        return View();
                    }
                }
            }
            else
            {
                ViewBag.Message = "請選擇檔案";
                return View();
            }
            string correctstring = "";
            List<string> correctlist = new List<string>();
            correctlist.Add(correctstring);
            return View();
        }

        #region 驗證Excel匯入資料正確性
        //將列數(rowcell)、欄數(columncell)、單元格內容(cell)參數傳入後，驗證資料
        public void checkcell_excerpt(int rowcell, int columncell, ICell cell)
        {
            //存取錯誤訊息字串
            string errorstring = "";
            //計算每一列讀到第幾個欄位(秀在錯誤訊息上)，欄位數字加一因為參數(columncell)從0開始數
            int columnnumber = columncell + 1;
            //讀取參數(rowcell)設strrow為第幾列
            string strrow = rowcell.ToString();
            //讀取欄位數(columnnumber)設strcolumn為第幾列
            string strcolumn = columnnumber.ToString();

            #region 取得單元格型別
            var Categorycelltype = "Spacetype";
            if (cell != null)
            {
                switch (cell.CellType)
                {
                    //字串型態欄位
                    case NPOI.SS.UserModel.CellType.String:
                        //設定dataRow第j欄位的值，cell以字串型態取值
                        Categorycelltype = "Stringtype";
                        break;

                    //數字型態欄位
                    case NPOI.SS.UserModel.CellType.Numeric:

                        if (HSSFDateUtil.IsCellDateFormatted(cell)) //日期格式
                        {
                            //設定dataRow第j欄位的值，cell以日期格式取值
                            Categorycelltype = "Datetype";
                        }
                        else //非日期格式
                        {
                            //設定dataRow第j欄位的值，cell以數字型態取值
                            Categorycelltype = "Numerictype";
                        }
                        break;

                    //布林值
                    case NPOI.SS.UserModel.CellType.Boolean:
                        Categorycelltype = "Booltype";
                        break;

                    //空值
                    case NPOI.SS.UserModel.CellType.Blank:
                        Categorycelltype = "Spacetype";
                        break;

                    // 預設
                    default:
                        //content = cell.StringCellValue;
                        Categorycelltype = "Stringtype";
                        break;
                }
            }
            #endregion

            else
            {
                switch (columnnumber)
                {
                    case 1:
                        //是否為字串型別或數字型別
                        if (Categorycelltype == "Stringtype")
                        {
                            string ItemNo = cell.ToString();
                            if (ItemNo != null)
                            {
                                if (ItemNo.Length > 10)
                                {
                                    errorstring = "第" + strrow + "列分區(第" + strcolumn + "欄)，資料長度過長";
                                    errorlist.Add(errorstring);
                                }
                            }
                        }
                        else
                        {
                            errorstring = "第" + strrow + "列分區(第" + strcolumn + "欄)，格式不正確，請再次確認是否為銀行分區";
                            errorlist.Add(errorstring);
                        }
                        break;
                }
            }
        }
        #endregion
        #endregion

        #region 判斷此列是否第一欄未輸入資料
        //將列數(rowcell)、欄位內容(checkcolumn)、單元格總數(cellCount)、此列內容(row)參數傳入後，判斷此列是否只有第一欄未輸入資料，若整列無資料則匯入此列以上的Excel表格，若僅第一欄未輸入資料則顯示錯誤訊息
        public bool firstcolumncheck(int rowcell, string checkcolumn, int cellCount, IRow row)
        {
            string errorstring = "";
            List<string> errorstringlist = new List<string>();
            //預設錯誤訊息為0筆資料
            int errorstringnumber = 0;
            //預設檢查完成的欄位數為0筆資料
            int checkcolumnisnull = 0;
            //因列數(rowcell)參數從0開始算，略過標題列，顯示的列數為(rowcell+1)
            int rownumber = rowcell + 1;
            if (string.IsNullOrEmpty(checkcolumn.Trim()))   //如果此列首筆資料為空
            {
                //測試每一個欄位(總欄位數為cellCount)
                for (int N = 0; N < cellCount; N++)
                {
                    //若此列(Irow row)的單元格內容為Null，則給予一個空值
                    ICell Ncell = row.GetCell(N, MissingCellPolicy.RETURN_NULL_AND_BLANK);
                    //當Ncell為Null
                    if (Ncell == null)
                    {
                        if (errorstringnumber == 0)
                        {
                            //此欄位檢查完成，單元格無資料
                            checkcolumnisnull++;
                        }
                        continue;
                    }
                    //當Ncell為空值
                    else if (Ncell.ToString() == "")
                    {
                        if (errorstringnumber == 0)
                        {
                            //此欄位檢查完成，單元格無資料
                            checkcolumnisnull++;
                        }
                        continue;
                    }
                    else
                    {
                        checkcolumnisnull++;
                        errorstring = "第" + rownumber + "列第1欄，資料格式不可為空。因為此列第" + checkcolumnisnull + "欄有值";
                        errorlist.Add(errorstring);
                        errorstringnumber++;
                        break;
                    }
                }
            }
            bool check = errorstringnumber > 0 || (checkcolumnisnull == 9) || (checkcolumnisnull == 13) ? true : false;      //true:為空，false不為空
            return check;
        }
        #endregion

        #region 上傳表單資料(HomeObject/LandLord/Tenant/Secretary)資料
        [HttpGet]
        public ActionResult UploadObjectForm()
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
                logger.Info("姓名:" + EmployeeData.Name + "部門:" + EmployeeData.Department);
            }
            //確認角色
            var IsAdmin = User.IsInRole("Admin");
            ViewBag.Role = User.IsInRole("Admin") ? "Admin" : User.IsInRole("Agent") ? "Agent" : User.IsInRole("Secretary") ? "Secretary" : "";
            #endregion

            ViewBag.Message = "Initial";
            return View();
        }

        [HttpPost]
        #region 物件資料上傳
        public ActionResult UploadHombObject(HttpPostedFileBase file)
        {
            string errorstring = "";
            List<string> errorstringlist = new List<string>();
            ViewBag.Message = "匯入成功";
            if (file != null)
            {
                //若上傳資料內有資料則執行此動作
                Stream stream = file.InputStream; //使用Stream(流)對檔案進行操作
                DataTable dataTable = new DataTable();
                DataTable datarow = new DataTable();
                IWorkbook wb;   //存取XLSM或XLS版本
                ISheet sheet;   //存取頁籤
                IRow headerRow; //存取第一列
                int cellCount;  //紀錄共有幾欄
                try
                {
                    //依excel版本，NPOI載入檔案
                    if (file.FileName.ToUpper().EndsWith("XLSX"))
                        wb = new XSSFWorkbook(stream); // excel版本(.xlsx)
                    else
                        wb = new HSSFWorkbook(stream); // excel版本(.xls)

                    //取第一個頁籤   
                    sheet = wb.GetSheetAt(0);
                    //取第一個頁籤的第一列
                    headerRow = sheet.GetRow(0);
                    //計算出第一列共有多少欄位
                    cellCount = headerRow.LastCellNum;

                    //迴圈執行第一列的第一個欄位到最後一個欄位，將抓到的值塞進DataTable做完欄位名稱
                    for (int i = headerRow.FirstCellNum; i < cellCount; i++)
                    {
                        if (headerRow.GetCell(i) != null)
                            dataTable.Columns.Add(new DataColumn(headerRow.GetCell(i).StringCellValue));
                        else//null 則放空白
                            dataTable.Columns.Add(new DataColumn(""));
                    }

                    int column = 0; //計算每一列讀到第幾個欄位

                    // 略過第零列(標題列)，一直處理至最後一列
                    for (int i = (sheet.FirstRowNum + 4); i <= sheet.LastRowNum; i++)
                    {
                        //取目前的列(row)
                        IRow row = sheet.GetRow(i);
                        if (row == null)
                        {
                            break;
                        }
                        string first = " ";
                        ICell firstcell = row.GetCell(0, MissingCellPolicy.RETURN_NULL_AND_BLANK);
                        if (firstcell != null)
                        {
                            first = firstcell.ToString();
                        }
                        bool check = firstcolumncheck_ObjectForm(i, first, cellCount, row);   //確認該列資料第一欄未輸入資料

                        if (check == true) break;

                        //宣告DataRow
                        DataRow dataRow = dataTable.NewRow();
                        //宣告ICell
                        ICell cell;
                        try
                        {
                            //依先前取得，依每一列的欄位數，逐一設定欄位內容
                            for (int j = row.FirstCellNum; j < cellCount; j++)
                            {
                                //計算每一列讀到第幾個欄位(秀在錯誤訊息上)
                                column = j;

                                //設定cell為目前第j欄位
                                cell = row.GetCell(j, MissingCellPolicy.RETURN_NULL_AND_BLANK);

                                //checkcell_objectForm(i, j, cell);
                                if (cell != null) //若cell有值
                                {
                                    //用cell.CellType判斷資料的型別
                                    //再依照欄位屬性，用StringCellValue、DateCellValue、NumericCellValue、DateCellValue取值
                                    switch (cell.CellType)
                                    {
                                        //字串型態欄位
                                        case NPOI.SS.UserModel.CellType.String:
                                            //設定dataRow第j欄位的值，cell以字串型態取值
                                            dataRow[j] = cell.StringCellValue;
                                            break;

                                        //數字型態欄位
                                        case NPOI.SS.UserModel.CellType.Numeric:

                                            if (HSSFDateUtil.IsCellDateFormatted(cell)) //日期格式
                                            {
                                                //設定dataRow第j欄位的值，cell以日期格式取值
                                                dataRow[j] = DateTime.FromOADate(cell.NumericCellValue).ToString("yyyy/MM/dd HH:mm:ss");
                                            }
                                            else //非日期格式
                                            {
                                                //設定dataRow第j欄位的值，cell以數字型態取值
                                                dataRow[j] = cell.NumericCellValue;
                                            }
                                            break;

                                        //布林值
                                        case NPOI.SS.UserModel.CellType.Boolean:

                                            //設定dataRow第j欄位的值，cell以布林型態取值
                                            dataRow[j] = cell.BooleanCellValue;
                                            break;

                                        //空值
                                        case NPOI.SS.UserModel.CellType.Blank:

                                            dataRow[j] = "";
                                            break;

                                        // 預設
                                        default:

                                            dataRow[j] = cell.StringCellValue;
                                            break;
                                    }
                                }
                            }
                            //DataTable加入dataRow
                            dataTable.Rows.Add(dataRow);
                        }
                        catch (Exception ex)
                        {
                            //錯誤訊息
                            throw new Exception("第 " + i + "列第" + column + "欄，資料格式有誤:\r\r" + ex.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "匯入失敗";
                    ViewBag.ErrorMessage = ex.ToString();
                    errorstring = "匯入失敗，請檢查匯入格式";
                    var error = ex.ToString();
                    errorlist.Add(errorstring);
                    ViewBag.Errorlist = errorlist;
                }
                finally
                {
                    //釋放資源
                    sheet = null;
                    wb = null;
                    stream.Dispose();
                    stream.Close();
                }

                if (errorlist.Count >= 1)
                {
                    ViewBag.Message = "匯入失敗";
                    ViewBag.Errorlist = errorlist;
                    return RedirectToAction("UploadObjectForm");
                }

                //DataTable跑回圈，Insert資料至DB
                HomeObject homeobject = new HomeObject();
                foreach (DataRow dr in dataTable.Rows)
                {
                    UploadService uploadservice = new UploadService();
                    var formid = dr["FormID"].ToString();
                    var caseid = dr["CaseID"].ToString();
                    var existObjectForm = formdb.ObjectForm.Where(x => x.FormID == formid).FirstOrDefault();
                    var existHomeobject = formdb.HomeObject.Where(x => x.CaseID == caseid).FirstOrDefault();
                    var existSameForm_CaseID = formdb.HomeObject.Where(x => x.FormID == formid).Select(x => x.CaseID).FirstOrDefault();  //找到是否此匯入資料為系統正常寫入之資料，若已存在之Form之CaseID為LQ開頭則跳過

                    try
                    {
                        if (existObjectForm == null)
                        {
                            var AgentAccount = dr["AgentAccount"].ToString();
                            var AssistantAccount = memberdb.EmployeeData.Where(x => x.Account == AgentAccount).Select(x => x.AssistantAccount).FirstOrDefault();
                            var AssistantData = memberdb.Members.Where(x => x.Account == AssistantAccount).FirstOrDefault();

                            var formdata = new ObjectForm()
                            {
                                FormID = formid,
                                CreateAccount = AgentAccount,
                                CreateTime = DateTime.Now,
                                UpdateAccount = AgentAccount,
                                UpdateTime = DateTime.Now,
                                ProcessAccount = AssistantAccount,
                                ProcessName = AssistantData.Name,
                                FormType = dr["FormActive"].ToString() == "0" ? -1 : 2,   //結案
                                AgentAccount = AgentAccount,
                            };
                            //uploadservice.InsertForm(formdata);

                            using (var context = new FormModels())
                            {
                                // 使用資料上下文插入資料物件
                                context.ObjectForm.Add(formdata);
                                // 儲存更改到資料庫
                                context.SaveChanges();
                            }
                        }
                        else
                        {
                            //表單已存在
                            using (var context = new FormModels())
                            {
                                var exist = context.ObjectForm.Where(x => x.FormID == formid).FirstOrDefault();
                                bool formactive = dr["FormActive"].ToString() == "1" ? true : false;    //是否為履約中
                                var period = dr["ContractPeriod"].ToString();

                                exist.FormType = (exist.FormType == 3 || period == "1" || period == "2") && formactive ? 3 : exist.FormType == 2 && formactive ? 2 : formactive ? 2 : -1;   //結案

                                // 儲存更改到資料庫
                                context.SaveChanges();
                            }
                        }

                        if (existHomeobject == null)
                        {
                            bool DoInsert = true;
                            //存在相同之表單編號
                            if (existSameForm_CaseID != null)
                            {
                                //此表單編號之媒合編號為LQ開頭，則不執行輸入
                                DoInsert = existSameForm_CaseID.StartsWith("LQ") ? false : true;
                            }
                            if (DoInsert)
                            {
                                //起租日
                                var startdateArray = dr["startdate"].ToString().Split('-');
                                var startdateDateTime = Convert.ToDateTime((Convert.ToInt32(startdateArray[0]) + 1911).ToString() + "-" + startdateArray[1] + "-" + startdateArray[2]);
                                //結束日
                                var enddateArray = dr["enddate"].ToString().Split('-');
                                var enddateDateTime = Convert.ToDateTime((Convert.ToInt32(enddateArray[0]) + 1911).ToString() + "-" + enddateArray[1] + "-" + enddateArray[2]);


                                var insertdata = new HomeObject()
                                {
                                    FormID = formid,
                                    Renewals = Convert.ToInt32(dr["ContractPeriod"]),
                                    Phase = Convert.ToInt32(dr["Phase"]),
                                    CaseID = dr["CaseID"].ToString(),
                                    CaseType = 1,
                                    objecttype = Convert.ToInt32(dr["objecttype"]),
                                    notarization = null,
                                    signdate = startdateDateTime,
                                    appraiser = null,
                                    feature = null,
                                    city = null,
                                    district = null,
                                    road = null,
                                    detailaddress = null,
                                    fulladdress = dr["fulladdress"].ToString(),
                                    usefor = Convert.ToInt32(dr["usefor"]),
                                    useforelse = null,
                                    rent = Convert.ToInt32(dr["rent"]),
                                    deposit = null,
                                    management_fee = null,
                                    startdate = startdateDateTime,
                                    enddate = enddateDateTime,
                                    paydate = Convert.ToInt32(dr["paydate"]),
                                    buildtype = null,
                                    roomtype = null,
                                    roomamount = null,
                                    havepark = null,
                                    parktype = null,
                                    parkfloor = null,
                                    carpositionnumber = null,
                                    carmonthrent = null,
                                    carparkmanagefee = null,
                                    scooterparkfloor = null,
                                    scooterpositionnumber = null,
                                    scootermonthrent = null,
                                    scootermanagefee = null,
                                    Accessory = null,
                                    CreateTime = DateTime.Now,
                                    CreateAccount = User.Identity.Name,
                                    UpdateTime = DateTime.Now,
                                    UpdateAccount = User.Identity.Name,
                                    Memo = dr["Memo"].ToString(),
                                };
                                //uploadservice.InsertHombObject(insertdata);

                                // 建立資料上下文（Data Context）
                                using (var context = new FormModels())
                                {
                                    // 使用資料上下文插入資料物件
                                    context.HomeObject.Add(insertdata);
                                    // 儲存更改到資料庫
                                    context.SaveChanges();
                                }

                            }
                        }
                        else
                        {
                            //房屋物件已存在
                            using (var context = new FormModels())
                            {
                                var exist = context.HomeObject.Where(x => x.CaseID == caseid).FirstOrDefault();
                                exist.Renewals = Convert.ToInt32(dr["ContractPeriod"]);
                                exist.CaseType = dr["FormActive"].ToString() == "0" ? -1 : 2;   //結案
                                // 儲存更改到資料庫
                                context.SaveChanges();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Message = "匯入失敗，請檢查格式";
                        ViewBag.ErrorMessage = ex.ToString();
                        errorstring = dr["FormID"].ToString() + dr["CaseID"].ToString() + "匯入失敗，請重新確認資料內容。";
                        errorlist.Add(errorstring);
                        ViewBag.Errorlist = errorlist;
                        return RedirectToAction("UploadObjectForm");
                    }
                }
            }
            else
            {
                ViewBag.Message = "請選擇檔案";
                return RedirectToAction("UploadObjectForm");
            }
            string correctstring = "";
            List<string> correctlist = new List<string>();
            correctlist.Add(correctstring);
            return RedirectToAction("UploadObjectForm");
        }
        #endregion
        
        [HttpPost]
        #region 房東資料上傳
        public ActionResult UploadLandlord(HttpPostedFileBase file)
        {
            string errorstring = "";
            List<string> errorstringlist = new List<string>();
            ViewBag.Message = "匯入成功";
            if (file != null)
            {
                //若上傳資料內有資料則執行此動作
                Stream stream = file.InputStream; //使用Stream(流)對檔案進行操作
                DataTable dataTable = new DataTable();
                DataTable datarow = new DataTable();
                IWorkbook wb;   //存取XLSM或XLS版本
                ISheet sheet;   //存取頁籤
                IRow headerRow; //存取第一列
                int cellCount;  //紀錄共有幾欄
                try
                {
                    //依excel版本，NPOI載入檔案
                    if (file.FileName.ToUpper().EndsWith("XLSX"))
                        wb = new XSSFWorkbook(stream); // excel版本(.xlsx)
                    else
                        wb = new HSSFWorkbook(stream); // excel版本(.xls)

                    //取第一個頁籤   
                    sheet = wb.GetSheetAt(1);
                    //取第一個頁籤的第一列
                    headerRow = sheet.GetRow(0);
                    //計算出第一列共有多少欄位
                    cellCount = headerRow.LastCellNum;

                    //迴圈執行第一列的第一個欄位到最後一個欄位，將抓到的值塞進DataTable做完欄位名稱
                    for (int i = headerRow.FirstCellNum; i < cellCount; i++)
                    {
                        if (headerRow.GetCell(i) != null)
                            dataTable.Columns.Add(new DataColumn(headerRow.GetCell(i).StringCellValue));
                        else//null 則放空白
                            dataTable.Columns.Add(new DataColumn(""));
                    }

                    int column = 0; //計算每一列讀到第幾個欄位

                    // 略過第零列(標題列)，一直處理至最後一列
                    for (int i = (sheet.FirstRowNum + 4); i <= sheet.LastRowNum; i++)
                    {
                        //取目前的列(row)
                        IRow row = sheet.GetRow(i);
                        if (row == null)
                        {
                            break;
                        }
                        string first = " ";
                        ICell firstcell = row.GetCell(0, MissingCellPolicy.RETURN_NULL_AND_BLANK);
                        if (firstcell != null)
                        {
                            first = firstcell.ToString();
                        }
                        bool check = firstcolumncheck_ObjectForm(i, first, cellCount, row);   //確認該列資料第一欄未輸入資料

                        if (check == true) break;

                        //宣告DataRow
                        DataRow dataRow = dataTable.NewRow();
                        //宣告ICell
                        ICell cell;
                        try
                        {
                            //依先前取得，依每一列的欄位數，逐一設定欄位內容
                            for (int j = row.FirstCellNum; j < cellCount; j++)
                            {
                                //計算每一列讀到第幾個欄位(秀在錯誤訊息上)
                                column = j;

                                //設定cell為目前第j欄位
                                cell = row.GetCell(j, MissingCellPolicy.RETURN_NULL_AND_BLANK);

                                //checkcell_objectForm(i, j, cell);
                                if (cell != null) //若cell有值
                                {
                                    //用cell.CellType判斷資料的型別
                                    //再依照欄位屬性，用StringCellValue、DateCellValue、NumericCellValue、DateCellValue取值
                                    switch (cell.CellType)
                                    {
                                        //字串型態欄位
                                        case NPOI.SS.UserModel.CellType.String:
                                            //設定dataRow第j欄位的值，cell以字串型態取值
                                            dataRow[j] = cell.StringCellValue;
                                            break;

                                        //數字型態欄位
                                        case NPOI.SS.UserModel.CellType.Numeric:

                                            if (HSSFDateUtil.IsCellDateFormatted(cell)) //日期格式
                                            {
                                                //設定dataRow第j欄位的值，cell以日期格式取值
                                                dataRow[j] = DateTime.FromOADate(cell.NumericCellValue).ToString("yyyy/MM/dd HH:mm:ss");
                                            }
                                            else //非日期格式
                                            {
                                                //設定dataRow第j欄位的值，cell以數字型態取值
                                                dataRow[j] = cell.NumericCellValue;
                                            }
                                            break;

                                        //布林值
                                        case NPOI.SS.UserModel.CellType.Boolean:

                                            //設定dataRow第j欄位的值，cell以布林型態取值
                                            dataRow[j] = cell.BooleanCellValue;
                                            break;

                                        //空值
                                        case NPOI.SS.UserModel.CellType.Blank:

                                            dataRow[j] = "";
                                            break;

                                        // 預設
                                        default:

                                            dataRow[j] = cell.StringCellValue;
                                            break;
                                    }
                                }
                            }
                            //DataTable加入dataRow
                            dataTable.Rows.Add(dataRow);
                        }
                        catch (Exception ex)
                        {
                            //錯誤訊息
                            throw new Exception("第 " + i + "列第" + column + "欄，資料格式有誤:\r\r" + ex.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "匯入失敗";
                    ViewBag.ErrorMessage = ex.ToString();
                    errorstring = "匯入失敗，請檢查匯入格式";
                    var error = ex.ToString();
                    errorlist.Add(errorstring);
                    ViewBag.Errorlist = errorlist;
                }
                finally
                {
                    //釋放資源
                    sheet = null;
                    wb = null;
                    stream.Dispose();
                    stream.Close();
                }

                if (errorlist.Count >= 1)
                {
                    ViewBag.Message = "匯入失敗";
                    ViewBag.Errorlist = errorlist;
                    return RedirectToAction("UploadObjectForm");
                }
                //dataTable跑回圈，insert資料至DB
                foreach (DataRow dr in dataTable.Rows)
                {
                    try
                    {
                        UploadService uploadservice = new UploadService();
                        var formid = dr["FormID"].ToString();
                        var caseid = dr["CaseID"].ToString();
                        var existlandlord = formdb.LandLord.Where(x => x.CaseID == caseid).FirstOrDefault();
                        var existSameForm_CaseID = formdb.LandLord.Where(x => x.FormID == formid).Select(x => x.CaseID).FirstOrDefault();
                        if (existlandlord == null)
                        {
                            bool DoInsert = true;
                            //存在相同之表單編號
                            if (existSameForm_CaseID != null)
                            {
                                //此表單編號之媒合編號為LQ開頭，則不執行輸入
                                DoInsert = existSameForm_CaseID.StartsWith("LQ") ? false : true;
                            }
                            if (DoInsert)
                            {
                                var insertdata = new LandLord()
                                {
                                    FormID = dr["FormID"].ToString(),
                                    CaseID = dr["CaseID"].ToString(),
                                    Name = dr["Name"].ToString(),
                                    Principal = dr["Principal"].ToString(),
                                    Gender = Convert.ToInt32(dr["Gender"]),
                                    Birthday = null,
                                    IDNumber = dr["IDNumber"].ToString(),
                                    Phone = dr["Phone"].ToString(),
                                    BankNo = dr["BankNo"].ToString(),
                                    BrancheNo = dr["BrancheNo"].ToString(),
                                    BankAccount = dr["BankAccount"].ToString(),
                                    Address = null,
                                    AddressDetail = null,
                                    ContactAddress = null,
                                    ContactAddressDetail = null,
                                    MemberArray = null,
                                    CoOwner1 = null,
                                    CoOwner2 = null,
                                    CoOwner3 = null,
                                    CoOwner4 = null,
                                    CoOwner5 = null,
                                    Agent = null,
                                    CreateTime = DateTime.Now,
                                    CreateAccount = User.Identity.Name,
                                    UpdateTime = DateTime.Now,
                                    UpdateAccount = User.Identity.Name,
                                    Memo = dr["Memo"].ToString(),
                                };
                                uploadservice.InsertLandlord(insertdata);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Message = "匯入失敗，請檢查格式";
                        ViewBag.ErrorMessage = ex.ToString();
                        errorstring = "匯入失敗，請重新確認資料內容。";
                        errorlist.Add(errorstring);
                        ViewBag.Errorlist = errorlist;
                        return RedirectToAction("UploadObjectForm");
                    }
                }
            }
            else
            {
                ViewBag.Message = "請選擇檔案";
                return RedirectToAction("UploadObjectForm");
            }
            string correctstring = "";
            List<string> correctlist = new List<string>();
            correctlist.Add(correctstring);
            return RedirectToAction("UploadObjectForm");
        }
        #endregion
        
        [HttpPost]
        #region 房客資料上傳
        public ActionResult UploadTenant(HttpPostedFileBase file)
        {
            string errorstring = "";
            List<string> errorstringlist = new List<string>();
            ViewBag.Message = "匯入成功";
            if (file != null)
            {
                //若上傳資料內有資料則執行此動作
                Stream stream = file.InputStream; //使用Stream(流)對檔案進行操作
                DataTable dataTable = new DataTable();
                DataTable datarow = new DataTable();
                IWorkbook wb;   //存取XLSM或XLS版本
                ISheet sheet;   //存取頁籤
                IRow headerRow; //存取第一列
                int cellCount;  //紀錄共有幾欄
                try
                {
                    //依excel版本，NPOI載入檔案
                    if (file.FileName.ToUpper().EndsWith("XLSX"))
                        wb = new XSSFWorkbook(stream); // excel版本(.xlsx)
                    else
                        wb = new HSSFWorkbook(stream); // excel版本(.xls)

                    //取第一個頁籤   
                    sheet = wb.GetSheetAt(2);
                    //取第一個頁籤的第一列
                    headerRow = sheet.GetRow(0);
                    //計算出第一列共有多少欄位
                    cellCount = headerRow.LastCellNum;

                    //迴圈執行第一列的第一個欄位到最後一個欄位，將抓到的值塞進DataTable做完欄位名稱
                    for (int i = headerRow.FirstCellNum; i < cellCount; i++)
                    {
                        if (headerRow.GetCell(i) != null)
                            dataTable.Columns.Add(new DataColumn(headerRow.GetCell(i).StringCellValue));
                        else//null 則放空白
                            dataTable.Columns.Add(new DataColumn(""));
                    }

                    int column = 0; //計算每一列讀到第幾個欄位

                    // 略過第零列(標題列)，一直處理至最後一列
                    for (int i = (sheet.FirstRowNum + 4); i <= sheet.LastRowNum; i++)
                    {
                        //取目前的列(row)
                        IRow row = sheet.GetRow(i);
                        if (row == null)
                        {
                            break;
                        }
                        string first = " ";
                        ICell firstcell = row.GetCell(0, MissingCellPolicy.RETURN_NULL_AND_BLANK);
                        if (firstcell != null)
                        {
                            first = firstcell.ToString();
                        }
                        bool check = firstcolumncheck_ObjectForm(i, first, cellCount, row);   //確認該列資料第一欄未輸入資料
                        if (check == true) break;
                        //宣告DataRow
                        DataRow dataRow = dataTable.NewRow();
                        //宣告ICell
                        ICell cell;
                        try
                        {
                            //依先前取得，依每一列的欄位數，逐一設定欄位內容
                            for (int j = row.FirstCellNum; j < cellCount; j++)
                            {
                                //計算每一列讀到第幾個欄位(秀在錯誤訊息上)
                                column = j;

                                //設定cell為目前第j欄位
                                cell = row.GetCell(j, MissingCellPolicy.RETURN_NULL_AND_BLANK);

                                //checkcell_objectForm(i, j, cell);
                                if (cell != null) //若cell有值
                                {
                                    //用cell.CellType判斷資料的型別
                                    //再依照欄位屬性，用StringCellValue、DateCellValue、NumericCellValue、DateCellValue取值
                                    switch (cell.CellType)
                                    {
                                        //字串型態欄位
                                        case NPOI.SS.UserModel.CellType.String:
                                            //設定dataRow第j欄位的值，cell以字串型態取值
                                            dataRow[j] = cell.StringCellValue;
                                            break;

                                        //數字型態欄位
                                        case NPOI.SS.UserModel.CellType.Numeric:

                                            if (HSSFDateUtil.IsCellDateFormatted(cell)) //日期格式
                                            {
                                                //設定dataRow第j欄位的值，cell以日期格式取值
                                                dataRow[j] = DateTime.FromOADate(cell.NumericCellValue).ToString("yyyy/MM/dd HH:mm:ss");
                                            }
                                            else //非日期格式
                                            {
                                                //設定dataRow第j欄位的值，cell以數字型態取值
                                                dataRow[j] = cell.NumericCellValue;
                                            }
                                            break;

                                        //布林值
                                        case NPOI.SS.UserModel.CellType.Boolean:

                                            //設定dataRow第j欄位的值，cell以布林型態取值
                                            dataRow[j] = cell.BooleanCellValue;
                                            break;

                                        //空值
                                        case NPOI.SS.UserModel.CellType.Blank:

                                            dataRow[j] = "";
                                            break;

                                        // 預設
                                        default:

                                            dataRow[j] = cell.StringCellValue;
                                            break;
                                    }
                                }
                            }
                            //DataTable加入dataRow
                            dataTable.Rows.Add(dataRow);
                        }
                        catch (Exception ex)
                        {
                            //錯誤訊息
                            throw new Exception("第 " + i + "列第" + column + "欄，資料格式有誤:\r\r" + ex.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "匯入失敗";
                    ViewBag.ErrorMessage = ex.ToString();
                    errorstring = "匯入失敗，請檢查匯入格式";
                    var error = ex.ToString();
                    errorlist.Add(errorstring);
                    ViewBag.Errorlist = errorlist;
                }
                finally
                {
                    //釋放資源
                    sheet = null;
                    wb = null;
                    stream.Dispose();
                    stream.Close();
                }

                if (errorlist.Count >= 1)
                {
                    ViewBag.Message = "匯入失敗";
                    ViewBag.Errorlist = errorlist;
                    return RedirectToAction("UploadObjectForm");
                }
                //dataTable跑回圈，insert資料至DB
                foreach (DataRow dr in dataTable.Rows)
                {
                    UploadService uploadservice = new UploadService();
                    var formid = dr["FormID"].ToString();
                    var caseid = dr["CaseID"].ToString();
                    var existtenant = formdb.Tenant.Where(x => x.CaseID == caseid).FirstOrDefault();
                    var existSameForm_CaseID = formdb.Tenant.Where(x => x.FormID == formid).Select(x=>x.CaseID).FirstOrDefault();
                    try
                    {
                        if (existtenant == null)
                        {
                            bool DoInsert = true;
                            //存在相同之表單編號
                            if (existSameForm_CaseID != null)
                            {
                                //此表單編號之媒合編號為LQ開頭，則不執行輸入
                                DoInsert = existSameForm_CaseID.StartsWith("LQ") ? false : true;
                            }
                            if (DoInsert)
                            {
                                var insertdata = new Tenant()
                                {
                                    FormID = dr["FormID"].ToString(),
                                    CaseID = dr["CaseID"].ToString(),
                                    TenantType = null,
                                    Name = dr["Name"].ToString(),
                                    Gender = Convert.ToInt32(dr["Gender"]),
                                    Birthday = null,
                                    IDNumber = dr["IDNumber"].ToString(),
                                    Phone = dr["Phone"].ToString(),
                                    Address = null,
                                    AddressDetail = null,
                                    ContactAddress = null,
                                    ContactAddressDetail = null,
                                    accountNo = null,
                                    BankNo = dr["BankNo"].ToString(),
                                    BrancheNo = dr["BrancheNo"].ToString(),
                                    BankAccount = dr["BankAccount"].ToString(),
                                    MemberArray = null,
                                    Couple = null,
                                    Family1 = null,
                                    Family2 = null,
                                    Family3 = null,
                                    Family4 = null,
                                    Family5 = null,
                                    Family6 = null,
                                    Family7 = null,
                                    Family8 = null,
                                    Family9 = null,
                                    Family10 = null,
                                    Agent1 = null,
                                    Agent2 = null,
                                    Agent3 = null,
                                    Guarantor1 = null,
                                    Guarantor2 = null,
                                    Guarantor3 = null,
                                    CreateTime = DateTime.Now,
                                    CreateAccount = User.Identity.Name,
                                    UpdateTime = DateTime.Now,
                                    UpdateAccount = User.Identity.Name,
                                    Memo = dr["Memo"].ToString(),
                                };
                                uploadservice.InsertTenant(insertdata);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Message = "匯入失敗，請檢查格式";
                        ViewBag.ErrorMessage = ex.ToString();
                        errorstring = formid + "匯入失敗，請重新確認資料內容。";
                        errorlist.Add(errorstring);
                        ViewBag.Errorlist = errorlist;
                        return RedirectToAction("UploadObjectForm");
                    }
                }
            }
            else
            {
                ViewBag.Message = "請選擇檔案";
                return RedirectToAction("UploadObjectForm");
            }
            string correctstring = "";
            List<string> correctlist = new List<string>();
            correctlist.Add(correctstring);
            return RedirectToAction("UploadObjectForm");
        }
        #endregion
        
        [HttpPost]
        #region 秘書填寫上傳
        public ActionResult UploadSecretary(HttpPostedFileBase file)
        {
            string errorstring = "";
            List<string> errorstringlist = new List<string>();
            ViewBag.Message = "匯入成功";
            if (file != null)
            {
                //若上傳資料內有資料則執行此動作
                Stream stream = file.InputStream; //使用Stream(流)對檔案進行操作
                DataTable dataTable = new DataTable();
                DataTable datarow = new DataTable();
                IWorkbook wb;   //存取XLSM或XLS版本
                ISheet sheet;   //存取頁籤
                IRow headerRow; //存取第一列
                int cellCount;  //紀錄共有幾欄
                try
                {
                    //依excel版本，NPOI載入檔案
                    if (file.FileName.ToUpper().EndsWith("XLSX"))
                        wb = new XSSFWorkbook(stream); // excel版本(.xlsx)
                    else
                        wb = new HSSFWorkbook(stream); // excel版本(.xls)

                    //取第一個頁籤   
                    sheet = wb.GetSheetAt(3);
                    //取第一個頁籤的第一列
                    headerRow = sheet.GetRow(0);
                    //計算出第一列共有多少欄位
                    cellCount = headerRow.LastCellNum;

                    //迴圈執行第一列的第一個欄位到最後一個欄位，將抓到的值塞進DataTable做完欄位名稱
                    for (int i = headerRow.FirstCellNum; i < cellCount; i++)
                    {
                        if (headerRow.GetCell(i) != null)
                            dataTable.Columns.Add(new DataColumn(headerRow.GetCell(i).StringCellValue));
                        else//null 則放空白
                            dataTable.Columns.Add(new DataColumn(""));
                    }

                    int column = 0; //計算每一列讀到第幾個欄位

                    // 略過第零列(標題列)，一直處理至最後一列
                    for (int i = (sheet.FirstRowNum + 4); i <= sheet.LastRowNum; i++)
                    {
                        //取目前的列(row)
                        IRow row = sheet.GetRow(i);
                        if (row == null)
                        {
                            break;
                        }
                        string first = " ";
                        ICell firstcell = row.GetCell(0, MissingCellPolicy.RETURN_NULL_AND_BLANK);
                        if (firstcell != null)
                        {
                            first = firstcell.ToString();
                        }
                        bool check = firstcolumncheck_ObjectForm(i, first, cellCount, row);   //確認該列資料第一欄未輸入資料

                        if (check == true) break;

                        //宣告DataRow
                        DataRow dataRow = dataTable.NewRow();
                        //宣告ICell
                        ICell cell;
                        try
                        {
                            //依先前取得，依每一列的欄位數，逐一設定欄位內容
                            for (int j = row.FirstCellNum; j < cellCount; j++)
                            {
                                //計算每一列讀到第幾個欄位(秀在錯誤訊息上)
                                column = j;

                                //設定cell為目前第j欄位
                                cell = row.GetCell(j, MissingCellPolicy.RETURN_NULL_AND_BLANK);

                                //checkcell_objectForm(i, j, cell);
                                if (cell != null) //若cell有值
                                {
                                    //用cell.CellType判斷資料的型別
                                    //再依照欄位屬性，用StringCellValue、DateCellValue、NumericCellValue、DateCellValue取值
                                    switch (cell.CellType)
                                    {
                                        //字串型態欄位
                                        case NPOI.SS.UserModel.CellType.String:
                                            //設定dataRow第j欄位的值，cell以字串型態取值
                                            dataRow[j] = cell.StringCellValue;
                                            break;

                                        //數字型態欄位
                                        case NPOI.SS.UserModel.CellType.Numeric:

                                            if (HSSFDateUtil.IsCellDateFormatted(cell)) //日期格式
                                            {
                                                //設定dataRow第j欄位的值，cell以日期格式取值
                                                dataRow[j] = DateTime.FromOADate(cell.NumericCellValue).ToString("yyyy/MM/dd HH:mm:ss");
                                            }
                                            else //非日期格式
                                            {
                                                //設定dataRow第j欄位的值，cell以數字型態取值
                                                dataRow[j] = cell.NumericCellValue;
                                            }
                                            break;

                                        //布林值
                                        case NPOI.SS.UserModel.CellType.Boolean:

                                            //設定dataRow第j欄位的值，cell以布林型態取值
                                            dataRow[j] = cell.BooleanCellValue;
                                            break;

                                        //空值
                                        case NPOI.SS.UserModel.CellType.Blank:

                                            dataRow[j] = "";
                                            break;

                                        // 預設
                                        default:

                                            dataRow[j] = cell.StringCellValue;
                                            break;
                                    }
                                }
                            }
                            //DataTable加入dataRow
                            dataTable.Rows.Add(dataRow);
                        }
                        catch (Exception ex)
                        {
                            //錯誤訊息
                            throw new Exception("第 " + i + "列第" + column + "欄，資料格式有誤:\r\r" + ex.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "匯入失敗";
                    ViewBag.ErrorMessage = ex.ToString();
                    errorstring = "匯入失敗，請檢查匯入格式";
                    var error = ex.ToString();
                    errorlist.Add(errorstring);
                    ViewBag.Errorlist = errorlist;
                }
                finally
                {
                    //釋放資源
                    sheet = null;
                    wb = null;
                    stream.Dispose();
                    stream.Close();
                }

                if (errorlist.Count >= 1)
                {
                    ViewBag.Message = "匯入失敗";
                    ViewBag.Errorlist = errorlist;
                    return RedirectToAction("UploadObjectForm");
                }

                //dataTable跑回圈，insert資料至DB
                foreach (DataRow dr in dataTable.Rows)
                {
                    try
                    {

                        UploadService uploadservice = new UploadService();
                        var formid = dr["FormID"].ToString();
                        var caseid = dr["CaseID"].ToString();
                        var existsecretary = formdb.Secretary.Where(x => x.CaseID == caseid).FirstOrDefault();
                        var existSameForm_CaseID = formdb.Secretary.Where(x => x.FormID == formid).Select(x => x.CaseID).FirstOrDefault();
                      
                        if (existsecretary == null)
                        {
                            bool DoInsert = true;
                            //存在相同之表單編號
                            if (existSameForm_CaseID != null)
                            {
                                //此表單編號之媒合編號為LQ開頭，則不執行輸入
                                DoInsert = existSameForm_CaseID.StartsWith("LQ") ? false : true;
                            }
                            if (DoInsert)
                            {
                                var insertdata = new Secretary()
                                {
                                    FormID = dr["FormID"].ToString(),
                                    CaseID = dr["CaseID"].ToString(),
                                    LandlordID = dr["LandlordID"].ToString(),
                                    TenantID = dr["TenantID"].ToString(),
                                    qualifyRadio = null,
                                    excerpt = dr["excerpt"].ToString(),
                                    excerptShort = dr["excerptShort"].ToString(),
                                    buildNo = null,
                                    placeNo = dr["placeNo"].ToString(),
                                    buildCreateDate = null,
                                    floorAmount = null,
                                    floorNo = null,
                                    squareAmount = !String.IsNullOrEmpty(dr["squareAmount"].ToString()) ? Convert.ToDouble(dr["squareAmount"]) : 0,
                                    pinAmount = !String.IsNullOrEmpty(dr["pinAmount"].ToString()) ? Convert.ToDouble(dr["pinAmount"]) : 0,
                                    notarizationFeeRadio = null,
                                    rentMarket = null,
                                    rentAgent = null,
                                    depositAgent = null,
                                    CreateTime = DateTime.Now,
                                    CreateAccount = User.Identity.Name,
                                    UpdateTime = DateTime.Now,
                                    UpdateAccount = User.Identity.Name,
                                    Memo = dr["Memo"].ToString(),
                                };
                                uploadservice.InsertSecretary(insertdata);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Message = "匯入失敗，請檢查格式";
                        ViewBag.ErrorMessage = ex.ToString();
                        errorstring = "匯入失敗，請重新確認資料內容。";
                        errorlist.Add(errorstring);
                        ViewBag.Errorlist = errorlist;
                        return RedirectToAction("UploadObjectForm");
                    }
                }
            }
            else
            {
                ViewBag.Message = "請選擇檔案";
                return RedirectToAction("UploadObjectForm");
            }
            string correctstring = "";
            List<string> correctlist = new List<string>();
            correctlist.Add(correctstring);
            return RedirectToAction("UploadObjectForm");
        }
        #endregion












        #region 驗證Excel匯入資料正確性(HomeObject/LandLord/Tenant/Secretary)
        ////將列數(rowcell)、欄數(columncell)、單元格內容(cell)參數傳入後，驗證資料
        //public void //checkcell_objectForm(int rowcell, int columncell, ICell cell)
        //{
        //    //存取錯誤訊息字串
        //    string errorstring = "";
        //    //計算每一列讀到第幾個欄位(秀在錯誤訊息上)，欄位數字加一因為參數(columncell)從0開始數
        //    int columnnumber = columncell + 1;
        //    //讀取參數(rowcell)設strrow為第幾列
        //    string strrow = rowcell.ToString();
        //    //讀取欄位數(columnnumber)設strcolumn為第幾列
        //    string strcolumn = columnnumber.ToString();

        //    #region 取得單元格型別
        //    var Categorycelltype = "Spacetype";
        //    if (cell != null)
        //    {
        //        switch (cell.CellType)
        //        {
        //            //字串型態欄位
        //            case NPOI.SS.UserModel.CellType.String:
        //                //設定dataRow第j欄位的值，cell以字串型態取值
        //                Categorycelltype = "Stringtype";
        //                break;

        //            //數字型態欄位
        //            case NPOI.SS.UserModel.CellType.Numeric:

        //                if (HSSFDateUtil.IsCellDateFormatted(cell)) //日期格式
        //                {
        //                    //設定dataRow第j欄位的值，cell以日期格式取值
        //                    Categorycelltype = "Datetype";
        //                }
        //                else //非日期格式
        //                {
        //                    //設定dataRow第j欄位的值，cell以數字型態取值
        //                    Categorycelltype = "Numerictype";
        //                }
        //                break;

        //            //布林值
        //            case NPOI.SS.UserModel.CellType.Boolean:
        //                Categorycelltype = "Booltype";
        //                break;

        //            //空值
        //            case NPOI.SS.UserModel.CellType.Blank:
        //                Categorycelltype = "Spacetype";
        //                break;

        //            // 預設
        //            default:
        //                //content = cell.StringCellValue;
        //                Categorycelltype = "Stringtype";
        //                break;
        //        }
        //    }
        //    #endregion

        //    //是否為空單元格
        //    if (Categorycelltype == "Spacetype")
        //    {
        //        errorstring = "第" + strrow + "列主項目(第" + strcolumn + "欄)，資料格式不可為空";
        //        errorlist.Add(errorstring);
        //    }
        //    else
        //    {
        //        switch (columnnumber)
        //        {
        //            case 1:
        //                //是否為字串型別或數字型別
        //                if (Categorycelltype == "Stringtype" || Categorycelltype == "Numerictype")
        //                {
        //                    string ItemNo = cell.ToString();
        //                    if (ItemNo != null)
        //                    {
        //                        if (ItemNo.Length > 50)
        //                        {
        //                            errorstring = "第" + strrow + "列產品編號(第" + strcolumn + "欄)，資料長度過長";
        //                            errorlist.Add(errorstring);
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    errorstring = "第" + strrow + "列產品編號(第" + strcolumn + "欄)，格式不正確，請再次確認是否為產品編號格式";
        //                    errorlist.Add(errorstring);
        //                }
        //                break;

        //            default:
        //                //是否為字串型別
        //                if (Categorycelltype != "Stringtype")
        //                {
        //                    errorstring = "第" + strrow + "列主項目(第" + strcolumn + "欄)，格式不正確，請再次確認是否為字串型別";
        //                    errorlist.Add(errorstring);
        //                }
        //                else
        //                {
        //                    string Category = cell.ToString();
        //                    if (Category != null)
        //                    {
        //                        if (Category.Length > 20)
        //                        {
        //                            errorstring = "第" + strrow + "列主項目(第" + strcolumn + "欄)，資料長度過長";
        //                            errorlist.Add(errorstring);
        //                        }
        //                    }
        //                }
        //                break;
        //        }
        //    }
        //}
        #endregion

        #region 判斷此列是否第一欄未輸入資料(HomeObject/LandLord/Tenant/Secretary)
        //將列數(rowcell)、欄位內容(checkcolumn)、單元格總數(cellCount)、此列內容(row)參數傳入後，判斷此列是否只有第一欄未輸入資料，若整列無資料則匯入此列以上的Excel表格，若僅第一欄未輸入資料則顯示錯誤訊息
        public bool firstcolumncheck_ObjectForm(int rowcell, string checkcolumn, int cellCount, IRow row)
        {
            string errorstring = "";
            List<string> errorstringlist = new List<string>();
            //預設錯誤訊息為0筆資料
            int errorstringnumber = 0;
            //預設檢查完成的欄位數為0筆資料
            int checkcolumnisnull = 0;
            //因列數(rowcell)參數從0開始算，略過標題列，顯示的列數為(rowcell+1)
            int rownumber = rowcell + 1;
            if (string.IsNullOrEmpty(checkcolumn.Trim()))   //如果此列首筆資料為空
            {
                //測試每一個欄位(總欄位數為cellCount)
                for (int N = 0; N < cellCount; N++)
                {
                    //若此列(Irow row)的單元格內容為Null，則給予一個空值
                    ICell Ncell = row.GetCell(N, MissingCellPolicy.RETURN_NULL_AND_BLANK);
                    //當Ncell為Null
                    if (Ncell == null)
                    {
                        if (errorstringnumber == 0)
                        {
                            //此欄位檢查完成，單元格無資料
                            checkcolumnisnull++;
                        }
                        continue;
                    }
                    //當Ncell為空值
                    else if (Ncell.ToString() == "")
                    {
                        if (errorstringnumber == 0)
                        {
                            //此欄位檢查完成，單元格無資料
                            checkcolumnisnull++;
                        }
                        continue;
                    }
                    else
                    {
                        checkcolumnisnull++;
                        errorstring = "第" + rownumber + "列第1欄，資料格式不可為空。因為此列第" + checkcolumnisnull + "欄有值";
                        errorlist.Add(errorstring);
                        errorstringnumber++;
                        break;
                    }
                }
            }
            bool check = errorstringnumber > 0 || (checkcolumnisnull == 9) || (checkcolumnisnull == 13) ? true : false;      //true:為空，false不為空
            return check;
        }
        #endregion

        #endregion

    }
}