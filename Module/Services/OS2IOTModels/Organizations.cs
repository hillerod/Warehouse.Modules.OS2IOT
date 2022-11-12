using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Module.Services.OS2IOTModels
{

    public class Organizations
    {
        public int count { get; set; }
        public OrganizationsDatum[] data { get; set; }
    }

    public class OrganizationsDatum
    {
        public int id { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public string name { get; set; }
        public OrganizationsApplication[] applications { get; set; }
    }

    public class OrganizationsApplication
    {
        public int id { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }

}
