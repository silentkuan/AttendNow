using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AttendNow.Models.ViewModel
{


    public class MeetingData
    {
        // Assuming ParticipantReport is a list of SP_GetParticipants_Result
        public List<SP_Meeting_Report> LocationReport { get; set; }
        public List<SP_Meeting_Report> TypeReport { get; set; }
        public List<SP_Participant_Report> MeetingReport { get; set; }
        public List<SP_Meeting_Report> ModeReport { get; set; }
        public List<SP_Meeting_Report> PlaceReport { get; set; }
        public List<SP_Meeting_Report> MeetingTypeReport { get; set; }
        public List<SP_Meeting_Report> CertificateReport { get; set; }
        public List<SP_Meeting_Report> ConditionReport { get; set; }
        public List<SP_GetMeetings_Result> MeetingList { get; set; }
        // Other properties or methods
    }


}