using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AttendNow.Models.ViewModel
{


    public class SP_Meeting_Report
    {
        public string createDate { get; set; }
        public string factory { get; set; }
        public string department { get; set; }
        public string location { get; set; }
        public string type { get; set; }

        public string meeting_type { get; set; }
        public string mode { get; set; }
        public string certificate { get; set; }
        public string condition { get; set; }
        public string place { get; set; }
        public Nullable<int> total { get; set; }
        public long row_num { get; set; }
    }


}