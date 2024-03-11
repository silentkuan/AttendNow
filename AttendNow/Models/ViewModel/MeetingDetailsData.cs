using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AttendNow.Models.ViewModel
{


    public class MeetingDetailsData
    {
        public int id { get; set; }
        public string meeting_code { get; set; }
        public string meeting_type { get; set; }
        public string title { get; set; }
        [AllowHtml]
        public string detail { get; set; }
        public Nullable<System.DateTime> startDate { get; set; }
        public Nullable<System.DateTime> endDate { get; set; }
        public string organizer { get; set; }
        [AllowHtml]
        public string remark { get; set; }
        public string place { get; set; }
        [AllowHtml]
        public string link { get; set; }
        public string department { get; set; }
        public string location { get; set; }
        public string createBy { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public string editBy { get; set; }
        public Nullable<System.DateTime> editDate { get; set; }
        public Nullable<bool> type { get; set; }
        public string password { get; set; }
        public string status { get; set; }
        public string condition { get; set; }
        public Nullable<bool> certificate { get; set; }
        public string factory_id { get; set; }
        public string mode { get; set; }

        public Nullable<bool> limit_status { get; set; }

        public int limit { get; set; }
    }


}