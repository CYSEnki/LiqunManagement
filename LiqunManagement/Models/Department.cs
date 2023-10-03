namespace LiqunManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("LOG.Department")]
    public partial class Department
    {
        [Key]
        public int DeptNo { get; set; }

        [StringLength(10)]
        public string DivCode { get; set; }

        [StringLength(20)]
        public string DivName { get; set; }

        [StringLength(50)]
        public string DivFullName { get; set; }

        public int? DivLevel { get; set; }

        [StringLength(10)]
        public string ParentDivCode { get; set; }

        [StringLength(10)]
        public string ManagerTitle { get; set; }

        [StringLength(10)]
        public string GeneralTitle { get; set; }

        [StringLength(30)]
        public string ManageAccount { get; set; }

        public DateTime? Createdate { get; set; }

        public DateTime? Enddate { get; set; }

        [StringLength(10)]
        public string Role { get; set; }
    }
}
