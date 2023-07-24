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
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ObjectNo { get; set; }

        [Required]
        [StringLength(10)]
        public string FormId { get; set; }

        [StringLength(10)]
        public string CaseId { get; set; }

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
        public string elseaddress { get; set; }

        [StringLength(200)]
        public string fulladdress { get; set; }

        public int? usefor { get; set; }

        [StringLength(100)]
        public string useforelse { get; set; }

        [StringLength(50)]
        public string taxfile_name { get; set; }

        [StringLength(100)]
        public string taxfile_guid { get; set; }

        public int? rent { get; set; }

        public int? deposit { get; set; }

        public int? management_fee { get; set; }

        public DateTime? startdate { get; set; }

        public DateTime? enddate { get; set; }

        public int? paydate { get; set; }

        public int? buildtype { get; set; }

        public int? roomtype { get; set; }

        [StringLength(10)]
        public string roomamount { get; set; }

        public int? hallamount { get; set; }

        public int? bathamount { get; set; }

        public int? carpark { get; set; }

        public int? parktype { get; set; }

        public int? parkfloor { get; set; }

        [StringLength(10)]
        public string carpositionnumber { get; set; }

        public int? carmonthrent { get; set; }

        public int? scootermonthrent { get; set; }

        public int? parkmanagementfee { get; set; }

        public int? scootermanagementfee { get; set; }

        public int? equipGas { get; set; }

        public int? equipTV { get; set; }

        public int? equipAir { get; set; }

        public int? equipDesk { get; set; }

        public int? equipWash { get; set; }

        public int? equipChair { get; set; }

        public int? equipStove { get; set; }

        public int? equipSofa { get; set; }

        public int? equipIce { get; set; }

        public int? equipCabinet { get; set; }

        public int? equipBed { get; set; }
    }
}
