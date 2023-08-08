using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LiqunManagement.ViewModels
{
    public class FormViewModels
    {
        public IEnumerable<DDLViewModel> ddllist { get; set; }
    }

    public class DDLViewModel
    {
        public int? order { get; set; }
        public string id { get; set; }
        public string text { get; set; }
    }
}