//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AttendNow.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class issue_db
    {
        public int id { get; set; }
        public string issue_id { get; set; }
        public string category { get; set; }
        public Nullable<int> quantity { get; set; }
        public string issueBy { get; set; }
        public string receiveBy { get; set; }
        public string warehouse_id { get; set; }
        public string store_id { get; set; }
        public Nullable<System.DateTime> issueAt { get; set; }
    }
}
