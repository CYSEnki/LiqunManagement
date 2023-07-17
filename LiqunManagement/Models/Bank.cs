namespace LiqunManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Form.Bank")]
    public partial class Bank
    {
        [Key]
        public int BankNo { get; set; }

        [StringLength(10)]
        public string BankRegion { get; set; }

        public bool? RootCheck { get; set; }

        [StringLength(10)]
        public string BankCode { get; set; }

        [StringLength(30)]
        public string BankName { get; set; }

        [StringLength(20)]
        public string BranchCode { get; set; }

        public string BranchName { get; set; }

        public string BranchFullName { get; set; }
    }
}
