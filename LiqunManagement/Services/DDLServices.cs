using LiqunManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LiqunManagement.Services
{
    public class DDLServices : BaseService
    {

        #region 找到縣市街道下拉選單
        public List<RegionDDLViewModel> GetRegionDDL(string value)
        {
            //var citycodelist = Liqundb.Region.Select(x => x.CityCode).Distinct();
            var ddlist = (from reg in Liqundb.Region
                          select new RegionDDLViewModel
                          {
                              text = reg.City,
                              id = reg.CityCode,
                          }).Distinct().OrderBy(x => x.id).ToList();

            return ddlist;
        }
        #endregion
    }
}