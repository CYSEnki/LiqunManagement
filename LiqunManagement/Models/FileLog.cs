namespace LiqunManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Form.FileLog")]
    public partial class FileLog
    {
        [Key]
        public int FileNo { get; set; }

        [StringLength(10)]
        public string FormID { get; set; }

        [StringLength(20)]
        public string Category { get; set; }

        [StringLength(1000)]
        public string FileNames { get; set; }

        [StringLength(300)]
        public string FileAlias { get; set; }

        [StringLength(10)]
        public string FilePath { get; set; }

        [StringLength(10)]
        public string FileExtension { get; set; }

        public DateTime? CreateTime { get; set; }

        [StringLength(30)]
        public string CreateAccount { get; set; }

        public DateTime? UpdateTime { get; set; }

        [StringLength(30)]
        public string UpdateAccount { get; set; }
    }
}
