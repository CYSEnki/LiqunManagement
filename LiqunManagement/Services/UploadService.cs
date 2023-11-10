using LiqunManagement.Models;
using LiqunManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace LiqunManagement.Services
{
    public class UploadService : BaseService
    {
        #region 取得Region資料
        public UploadViewModels GetRegionData()
        {
            var list = from Rlist in formdb.Region
                        select new RegionViewModel
                        {
                            City = Rlist.City,
                            District = Rlist.District,
                            Road = Rlist.Road,
                        };
            UploadViewModels uploadviewmodels = new UploadViewModels();
            uploadviewmodels.RegionIEnumerable = list;
            return uploadviewmodels;
        }
        #endregion

        #region 匯入陣列資料_臺灣地址
        public void InsertRegion(RegionViewModel RegionData)
        {
            // 建立資料上下文（Data Context）
            using (var context = new FormModels())
            {
                // 建立要插入的資料物件
                var newData = new Region
                {
                    CityOrder = RegionData.CityOrder,
                    City = RegionData.City,
                    CityCode = RegionData.CityCode,
                    District = RegionData.District,
                    DistrictCode = RegionData.DistrictCode,
                    Road = RegionData.Road,
                    RoadCode = RegionData.RoadCode,
                };

                // 使用資料上下文插入資料物件
                context.Region.Add(newData);

                // 儲存更改到資料庫
                context.SaveChanges();
            }

        }
        #endregion

        #region 匯入陣列資料_銀行資料
        public void InsertBank(BankViewModel BaankData)
        {
            // 建立資料上下文（Data Context）
            using (var context = new FormModels())
            {
                // 建立要插入的資料物件
                var newData = new Bank
                {
                    BankRegion = BaankData.BankRegion,
                    RootCheck = BaankData.RootCheck,
                    BankCode = BaankData.BankCode,
                    BankName = BaankData.BankName,
                    BranchCode = BaankData.BranchCode,
                    BranchName = BaankData.BranchName,
                    BranchFullName = BaankData.BranchFullName,
                    CodeMinlength = BaankData.CodeMinlength,
                    CodeMaxlength = BaankData.CodeMaxlength,
                };
                // 使用資料上下文插入資料物件
                context.Bank.Add(newData);
                // 儲存更改到資料庫
                context.SaveChanges();
            }

        }
        #endregion

        #region 匯入陣列資料_事務所 & 段
        public void InsertExcerpt(ExcerptViewModel ExcerptData)
        {
            // 建立資料上下文（Data Context）
            using (var context = new FormModels())
            {
                // 建立要插入的資料物件
                var newData = new Excerpt
                {
                    CityCode = ExcerptData.CityCode,
                    CityName = ExcerptData.CityName.Trim(),
                    DistrictCode = ExcerptData.DistrictCode,
                    DistrictName = ExcerptData.DistrictName.Trim(),
                    OfficeCode = ExcerptData.OfficeCode,
                    OfficeName = ExcerptData.OfficeName.Trim(),
                    Excerpt1 = ExcerptData.Excerpt1.Trim(),
                    ExcerptShort = ExcerptData.ExcerptShort.Trim(),
                    ExcerptCode = ExcerptData.ExcerptCode,
                };
                // 使用資料上下文插入資料物件
                context.Excerpt.Add(newData);
                // 儲存更改到資料庫
                context.SaveChanges();
            }
        }
        #endregion

        #region 匯入陣列資料_表單資料
        //物件資料
        public void InsertForm(ObjectForm InsertData)
        {
            // 建立資料上下文（Data Context）
            using (var context = new FormModels())
            {
                // 建立要插入的資料物件
                var newData = InsertData;
                // 使用資料上下文插入資料物件
                context.ObjectForm.Add(newData);
                // 儲存更改到資料庫
                context.SaveChanges();
            }

        }
        //物件資料
        public void InsertHombObject(HomeObject InsertData)
        {
            // 建立資料上下文（Data Context）
            using (var context = new FormModels())
            {
                // 建立要插入的資料物件
                var newData = InsertData;
                // 使用資料上下文插入資料物件
                context.HomeObject.Add(newData);
                // 儲存更改到資料庫
                context.SaveChanges();
            }

        }
        //房東資料
        public void InsertLandlord(LandLord InsertData)
        {
            // 建立資料上下文（Data Context）
            using (var context = new FormModels())
            {
                // 建立要插入的資料物件
                var newData = InsertData;
                // 使用資料上下文插入資料物件
                context.LandLord.Add(newData);
                // 儲存更改到資料庫
                context.SaveChanges();
            }

        }
        //房客資料
        public void InsertTenant(Tenant InsertData)
        {
            // 建立資料上下文（Data Context）
            using (var context = new FormModels())
            {
                // 建立要插入的資料物件
                var newData = InsertData;
                // 使用資料上下文插入資料物件
                context.Tenant.Add(newData);
                // 儲存更改到資料庫
                context.SaveChanges();
            }

        }
        //秘書填寫
        public void InsertSecretary(Secretary InsertData)
        {
            // 建立資料上下文（Data Context）
            using (var context = new FormModels())
            {
                // 建立要插入的資料物件
                var newData = InsertData;
                // 使用資料上下文插入資料物件
                context.Secretary.Add(newData);
                // 儲存更改到資料庫
                context.SaveChanges();
            }

        }
        #endregion
    }
}