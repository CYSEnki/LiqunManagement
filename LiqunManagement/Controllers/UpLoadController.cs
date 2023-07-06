using NPOI.HSSF.UserModel;
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

namespace LiqunManagement.Controllers
{
    public class UpLoadController : Controller
    {
        //將錯誤訊息字串存取至List
        List<string> errorlist = new List<string>();
        [HttpGet]
        // GET: UpLoad
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        #region 上傳Region資料
        public ActionResult UploadRegion()
        {

            var UserName = User.Identity.Name;
            return View();
        }
        #endregion


        [HttpPost]
        #region 上傳Region資料
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

                                checkcell(i, j, cell);
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
                    errorstring = "匯入失敗，請檢查匯入格式";
                    var error = ex.ToString();
                    errorlist.Add(errorstring);
                }
                finally
                {
                    //釋放資源
                    sheet = null;
                    wb = null;
                    stream.Dispose();
                    stream.Close();
                }

                if (errorlist.Count > 1)
                {
                    //存取頁面錯誤資訊至errormodel中
                    //var errormodel = new EquipInfoViewModel
                    //{
                    //    //回傳資料庫內容
                    //    EquipContent = model2.EquipContent,
                    //    errorMessage = errorlist.ToList(),
                    //    correctMessage = model2.correctMessage,
                    //    EquipInfoAndName = model2.EquipInfoAndName,
                    //    ChtItemID = model2.ChtItemID,
                    //};

                    ViewBag.Message = errorlist;

                    //return View();
                    return RedirectToAction("UploadRegion", "Upload");
                }
                var CodeA = 'A';
                var Codea = 'a';
                var Code1 = 1;
                var Code2 = 1;

                var CityName = "";
                var DistrictName = "";
                var RoadName = "";
                using (var context = new LiqunModels())
                {
                    // 取得所有資料
                    var allData = context.Region.AsEnumerable();

                    // 移除所有資料
                    context.Region.RemoveRange(allData);

                    // 儲存更改到資料庫
                    context.SaveChanges();
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
                        var CityCode = CodeA.ToString() + Codea.ToString();
                        var datedata = new RegionViewModel()
                        {
                            City = dr["縣市名稱"].ToString(),
                            CityCode = CityCode,
                            District = dr["行政區域名稱"].ToString(),
                            DistrictCode = CityCode + Code1.ToString("D2"),
                            Road = dr["路名"].ToString(),
                            RoadCode = CityCode + Code1.ToString("D2") + Code2.ToString("D3"),
                        };

                        UploadService uploadservice = new UploadService();
                        uploadservice.InsertRegion(datedata);

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
                    catch (Exception ex)
                    {
                        ViewBag.Message = "匯入失敗，請檢查格式";
                        errorstring = "匯入失敗，請重新確認資料內容。";
                        errorlist.Add(errorstring);
                        var error = ex.ToString();

                        ////找到初始化頁面資料內容
                        //var model2 = InitialFrontPage();

                        //存取頁面錯誤資訊至errormodel中
                        //var errormodel = new EquipInfoViewModel
                        //{
                        //    //回傳資料庫內容
                        //    EquipContent = model.EquipContent,
                        //    errorMessage = errorlist.ToList(),
                        //    correctMessage = model.correctMessage,
                        //    EquipInfoAndName = model.EquipInfoAndName,
                        //    ChtItemID = model.ChtItemID,
                        //};


                        //return View(errormodel);
                        return View();
                    }
                }
            }
            else
            {
                ViewBag.Message = "匯入失敗";
                errorstring = "請選擇檔案";
                errorlist.Add(errorstring);
                //存取頁面錯誤資訊至errormodel中
                //var errormodel = new EquipInfoViewModel
                //{
                //    //回傳資料庫內容
                //    EquipContent = model.EquipContent,
                //    errorMessage = errorlist.ToList(),
                //    correctMessage = model.correctMessage,
                //    EquipInfoAndName = model.EquipInfoAndName,
                //    ChtItemID = model.ChtItemID,
                //};

                //return View(errormodel);
                return View();
            }

            string correctstring = "";
            List<string> correctlist = new List<string>();
            correctlist.Add(correctstring);
            //var correctmodel = new EquipInfoViewModel
            //{
            //    //回傳資料庫內容
            //    EquipContent = okmodel.EquipContent,
            //    errorMessage = okmodel.errorMessage,
            //    correctMessage = correctlist,
            //    EquipInfoAndName = okmodel.EquipInfoAndName,
            //    ChtItemID = okmodel.ChtItemID,
            //};
            //return View(correctmodel);
            return View();
        }
        #endregion

        #region 驗證Excel匯入資料正確性
        //將列數(rowcell)、欄數(columncell)、單元格內容(cell)參數傳入後，驗證資料
        public void checkcell(int rowcell, int columncell, ICell cell)
        {
            //存取錯誤訊息字串
            string errorstring = "";
            //計算每一列讀到第幾個欄位(秀在錯誤訊息上)，欄位數字加一因為參數(columncell)從0開始數
            int columnnumber = columncell + 1;
            //讀取參數(rowcell)設strrow為第幾列
            string strrow = rowcell.ToString();
            //讀取欄位數(columnnumber)設strcolumn為第幾列
            string strcolumn = columnnumber.ToString();
            //取得單元格型別
            string Categorycelltype = "Spacetype";
            if(cell != null)
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

            //是否為空單元格
            if (Categorycelltype == "Spacetype")
            {
                errorstring = "第" + strrow + "列主項目(第" + strcolumn + "欄)，資料格式不可為空";
                errorlist.Add(errorstring);
            }
            else
            {
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
            }
        }
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
            bool check = errorstringnumber > 0 || (checkcolumnisnull == 13) ? true : false;      //true:為空，false不為空
            return check;
        }
        #endregion

    }
}