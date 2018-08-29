using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace Application
{
    public class AppContext:DataContext
    {
        public Table<Project> Projects { get { return this.GetTable<Project>(); } }
        public Table<ProjectOwner> Owners { get { return this.GetTable<ProjectOwner>(); } }
        public AppContext(string conString) : base(conString) { }
        public AppContext(IDbConnection con) : base(con) { }
    }
}
