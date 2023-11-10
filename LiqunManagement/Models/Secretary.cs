namespace LiqunManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Form.Secretary")]
    public partial class Secretary
    {
        [Key]
        public int secretaryNo { get; set; }

        [Required]
        [StringLength(50)]
        public string FormID { get; set; }

        [StringLength(100)]
        public string CaseID { get; set; }

        [StringLength(50)]
        public string LandlordID { get; set; }

        [StringLength(50)]
        public string TenantID { get; set; }

        public int? qualifyRadio { get; set; }

        [StringLength(20)]
        public string excerpt { get; set; }

        [StringLength(20)]
        public string excerptShort { get; set; }

        [StringLength(20)]
        public string buildNo { get; set; }

        [StringLength(100)]
        public string placeNo { get; set; }

        public DateTime? buildCreateDate { get; set; }

        public int? floorAmount { get; set; }

        public int? floorNo { get; set; }

        public double? squareAmount { get; set; }

        public double? pinAmount { get; set; }

        public int? notarizationFeeRadio { get; set; }

        public int? rentMarket { get; set; }

        public int? rentAgent { get; set; }

        public int? depositAgent { get; set; }

        [StringLength(300)]
        public string Memo { get; set; }

        public DateTime CreateTime { get; set; }

        [Required]
        [StringLength(30)]
        public string CreateAccount { get; set; }

        public DateTime UpdateTime { get; set; }

        [Required]
        [StringLength(30)]
        public string UpdateAccount { get; set; }
    }
}
