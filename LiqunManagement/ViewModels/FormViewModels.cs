using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LiqunManagement.ViewModels
{
    public class FormViewModels
    {
        public IEnumerable<DDLViewModel> ddllist { get; set; }
        public IEnumerable<objectForm> objectformlist { get; set; }
    }

    public class DDLViewModel
    {
        public int? order { get; set; }
        public string id { get; set; }
        public string text { get; set; }
    }

    public class objectForm
    {
        public string FormId { get; set; }
        public DateTime CreateTime { get; set; }
        public string ProcessName { get; set; }
        public string Address { get; set; }
        public DateTime SignDate { get; set; }
        public string Landlord { get; set; }
        public string Tenant { get; set; }
    }
}