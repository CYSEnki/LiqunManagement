namespace LiqunManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Form.ObjectForm")]
    public partial class ObjectForm
    {
        [Key]
        public int FormNo { get; set; }

        [StringLength(10)]
        public string FormID { get; set; }

        [StringLength(30)]
        public string CreateAccount { get; set; }

        public DateTime? CreateTime { get; set; }

        [StringLength(30)]
        public string UpdateAccount { get; set; }

        public DateTime? UpdateTime { get; set; }

        [StringLength(30)]
        public string ProcessAccount { get; set; }

        [StringLength(50)]
        public string ProcessName { get; set; }

        public int? FormType { get; set; }

        [StringLength(30)]
        public string AgentAccount { get; set; }
    }
}
