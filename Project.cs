using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq.Mapping;

namespace Application
{
    [Table(Name = "Projects")]
    public class Project
    {
        [Column(Name = "GUID", IsPrimaryKey = true, CanBeNull = false)]
        public Guid GUID { get; set; }

        [Column(Name = "Name")]
        public string Name { get; set; }

        [Column(Name = "OwnerGuid", CanBeNull = false)]
        public Guid OwnerGUID { get; set; }

        [Column(Name = "StartDate")]
        public DateTime StartDate { get; set; }

        [Column(Name = "FinishDate")]
        public DateTime FinishDate { get; set; }

    }
}
