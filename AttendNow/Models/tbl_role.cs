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
    
    public partial class tbl_role
    {
        public int id { get; set; }
        public string role_code { get; set; }
        public string name { get; set; }
        public string status { get; set; }
        public string editBy { get; set; }
        public Nullable<System.DateTime> editDate { get; set; }
        public string createBy { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
    }
}
