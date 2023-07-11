using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LiqunManagement.ViewModels
{
    public class UploadViewModels
    {
        public IEnumerable<RegionViewModel> RegionIEnumerable { get; set; }
    }

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
}