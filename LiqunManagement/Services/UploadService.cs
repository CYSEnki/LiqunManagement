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
            var list = from Rlist in Liqundb.Region
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
            using (var context = new LiqunModels())
            {
                // 建立要插入的資料物件
                var newData = new Region
                {
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


        #region 匯入陣列資料_臺灣地址
        public void InsertBank(BankViewModel BaankData)
        {
            // 建立資料上下文（Data Context）
            using (var context = new LiqunModels())
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
                };

                // 使用資料上下文插入資料物件
                context.Bank.Add(newData);

                // 儲存更改到資料庫
                context.SaveChanges();
            }

        }
        #endregion
    }
}