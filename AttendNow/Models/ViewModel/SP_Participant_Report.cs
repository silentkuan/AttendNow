using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AttendNow.Models.ViewModel
{


    public class SP_Participant_Report
    {
        public string staff_no { get; set; }
        public string name { get; set; }
        public string department { get; set; }
        public string location { get; set; }
        public Nullable<int> total { get; set; }
        public string factory { get; set; }
        public string meeting_code { get; set; }
        public string meeting { get; set; }
        public string joinedDate { get; set; }
        public long row_num { get; set; }

        public string status { get; set; }
    }


}