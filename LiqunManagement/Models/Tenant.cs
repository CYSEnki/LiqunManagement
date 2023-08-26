namespace LiqunManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Form.Tenant")]
    public partial class Tenant
    {
        [Key]
        public int TenantNo { get; set; }

        public int? TenantType { get; set; }

        [StringLength(200)]
        public string vulnerablefile_Name { get; set; }

        [StringLength(300)]
        public string vulnerablefile_Alias { get; set; }

        [StringLength(200)]
        public string sheetfile_Name { get; set; }

        [StringLength(300)]
        public string sheetfile_Alias { get; set; }

        [StringLength(20)]
        public string Name { get; set; }

        public int? Gender { get; set; }

        public DateTime? Birthday { get; set; }

        [StringLength(20)]
        public string IDNumber { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(10)]
        public string RoadCode { get; set; }

        [StringLength(50)]
        public string detailaddress { get; set; }

        [StringLength(10)]
        public string RoadCodeContact { get; set; }

        [StringLength(50)]
        public string detailaddressContact { get; set; }

        [StringLength(20)]
        public string accountNo { get; set; }

        [StringLength(20)]
        public string BankNo { get; set; }

        [StringLength(20)]
        public string BrancheNo { get; set; }

        [StringLength(50)]
        public string MemberArray { get; set; }

        [StringLength(200)]
        public string Couple { get; set; }

        [StringLength(200)]
        public string Family1 { get; set; }

        [StringLength(200)]
        public string Family2 { get; set; }

        [StringLength(200)]
        public string Family3 { get; set; }

        [StringLength(200)]
        public string Family4 { get; set; }

        [StringLength(200)]
        public string Family5 { get; set; }

        [MaxLength(50)]
        public byte[] Family6 { get; set; }

        [StringLength(200)]
        public string Family7 { get; set; }

        [StringLength(200)]
        public string Family8 { get; set; }

        [StringLength(200)]
        public string Family9 { get; set; }

        [StringLength(200)]
        public string Family10 { get; set; }

        [StringLength(200)]
        public string Agent1 { get; set; }

        [StringLength(200)]
        public string Agent2 { get; set; }

        [StringLength(200)]
        public string Agent3 { get; set; }

        [StringLength(200)]
        public string Gurantor1 { get; set; }

        [StringLength(200)]
        public string Gurantor2 { get; set; }

        [StringLength(200)]
        public string Gurantor3 { get; set; }
    }
}
