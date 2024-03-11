using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AttendNow.Models.ViewModel
{
    public class RoleData
    {
        public tbl_role Role { get; set; }
        public sys_setting_role_permission UserModule { get; set; }
        public sys_setting_role_permission DepartmentModule { get; set; }
        public sys_setting_role_permission RoleModule { get; set; }
        public sys_setting_role_permission LocationModule { get; set; }
        public sys_setting_role_permission FactoryModule { get; set; }
        public sys_setting_role_permission ParticipantModule { get; set; }
        public sys_setting_role_permission MeetingModule { get; set; }

    }
}