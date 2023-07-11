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
        public FormViewModels GetRegionDDL(string regioncode)
        {
            //var citycodelist = Liqundb.Region.Select(x => x.CityCode).Distinct();
            IEnumerable<RegionDDLViewModel> regionDDL = new List<RegionDDLViewModel>();
            var codelength = regioncode.Length;
            switch (codelength)
            {
                case 0:
                    regionDDL = (from reg in Liqundb.Region
                                 select new RegionDDLViewModel
                                 {
                                     order = reg.CityOrder,
                                     text = reg.City,
                                     id = reg.CityCode,
                                 }).Distinct().OrderBy(x => x.order);
                    break;
                case 2:
                    regionDDL = (from reg in Liqundb.Region
                                 where reg.CityCode == regioncode
                                 select new RegionDDLViewModel
                                 {
                                     text = reg.District,
                                     id = reg.DistrictCode,
                                 }).Distinct().OrderBy(x => x.id);
                    break;
                case 4:
                    regionDDL = (from reg in Liqundb.Region
                                 where reg.DistrictCode == regioncode
                                 select new RegionDDLViewModel
                                 {
                                     text = reg.Road,
                                     id = reg.RoadCode,
                                 }).Distinct().OrderBy(x => x.id);
                    break;
            }
            var formmodel = new FormViewModels();
            formmodel.regionddl = regionDDL;
            return formmodel;
        }
        #endregion
    }
}