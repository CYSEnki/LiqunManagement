namespace LiqunManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("LOG.EmployeeData")]
    public partial class EmployeeData
    {
        [Key]
        [StringLength(30)]
        public string Account { get; set; }

        [StringLength(10)]
        public string DivCode { get; set; }

        public DateTime? CreateTime { get; set; }

        [StringLength(30)]
        public string CreateAccount { get; set; }

        public DateTime? UpdateTime { get; set; }

        [StringLength(30)]
        public string UpdateAccount { get; set; }

        [StringLength(10)]
        public string JobTitle { get; set; }
    }
}
