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

        public DateTime? CreateTime { get; set; }

        [StringLength(30)]
        public string CreateAccount { get; set; }

        public DateTime? UpdateTime { get; set; }

        [StringLength(30)]
        public string UpdateAccount { get; set; }

        [StringLength(50)]
        public string Department { get; set; }

        [StringLength(20)]
        public string Position { get; set; }
    }
}
