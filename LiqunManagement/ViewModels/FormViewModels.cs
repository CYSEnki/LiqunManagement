using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LiqunManagement.ViewModels
{
    public class FormViewModels
    {
        public IEnumerable<RegionDDLViewModel> regionddl { get; set; }
    }

    public class RegionDDLViewModel
    {
        public int? order { get; set; }
        public string id { get; set; }
        public string text { get; set; }
    }
}