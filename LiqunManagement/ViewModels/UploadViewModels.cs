using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using NLog.LayoutRenderers;

namespace LiqunManagement.ViewModels
{
    public class UploadViewModels
    {
        public IEnumerable<RegionViewModel> RegionIEnumerable { get; set; }
    }
    //地址
    public class RegionViewModel
    {
        public int CityOrder { get; set; }
        public string City { get; set; }

        public string CityCode { get; set; }

        public string District { get; set; }

        public string DistrictCode { get; set; }

        public string Road { get; set; }

        public string RoadCode { get; set; }
    }
    //銀行
    public class BankViewModel
    {
        public string BankRegion { get; set; }
        public bool? RootCheck { get; set; }
        public string BankCode { get; set; }
        public string BankName { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public string BranchFullName { get; set; }
        public string CodeMinlength { get; set; }
        public string CodeMaxlength { get; set; }
    }

    //事務所 & 段
    public class ExcerptViewModel
    {
        public string CityCode { get; set; }
        public string CityName { get; set; }
        public string DistrictCode { get; set; }
        public string DistrictName { get; set; }
        public string OfficeCode { get; set; }
        public string OfficeName { get; set; }
        public string Excerpt1 { get; set; }
        public string ExcerptShort { get; set; }
        public string ExcerptCode { get; set; }
    }
}