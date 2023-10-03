namespace LiqunManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Form.HomeObject")]
    public partial class HomeObject
    {
        [Key]
        public int ObjectNo { get; set; }

        [Required]
        [StringLength(10)]
        public string FormID { get; set; }

        [StringLength(10)]
        public string CaseId { get; set; }

        public int? objecttype { get; set; }

        public int? notarization { get; set; }

        public DateTime? signdate { get; set; }

        public int? appraiser { get; set; }

        [StringLength(200)]
        public string feature { get; set; }

        [StringLength(10)]
        public string city { get; set; }

        [StringLength(10)]
        public string district { get; set; }

        [StringLength(20)]
        public string road { get; set; }

        [StringLength(50)]
        public string detailaddress { get; set; }

        [StringLength(200)]
        public string fulladdress { get; set; }

        public int? usefor { get; set; }

        [StringLength(100)]
        public string useforelse { get; set; }

        [StringLength(500)]
        public string taxfile_name { get; set; }

        [StringLength(1000)]
        public string taxfile_alias { get; set; }

        public int? rent { get; set; }

        public int? deposit { get; set; }

        public int? management_fee { get; set; }

        public DateTime? startdate { get; set; }

        public DateTime? enddate { get; set; }

        public int? paydate { get; set; }

        public int? buildtype { get; set; }

        public int? roomtype { get; set; }

        [StringLength(20)]
        public string roomamount { get; set; }

        [StringLength(20)]
        public string havepark { get; set; }

        public int? parktype { get; set; }

        [StringLength(20)]
        public string parkfloor { get; set; }

        [StringLength(20)]
        public string carpositionnumber { get; set; }

        public int? carmonthrent { get; set; }

        public int? carparkmanagefee { get; set; }

        [StringLength(20)]
        public string scooterparkfloor { get; set; }

        [StringLength(20)]
        public string scooterpositionnumber { get; set; }

        public int? scootermonthrent { get; set; }

        public int? scootermanagefee { get; set; }

        [StringLength(100)]
        public string Accessory { get; set; }

        public DateTime? CreateTime { get; set; }

        [StringLength(30)]
        public string CreateAccount { get; set; }

        public DateTime? UpdateTime { get; set; }

        [StringLength(30)]
        public string UpdateAccount { get; set; }

        [StringLength(300)]
        public string Memo { get; set; }
    }
}
