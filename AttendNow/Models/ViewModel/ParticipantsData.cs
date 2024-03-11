using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AttendNow.Models.ViewModel
{
   

    public class ParticipantData
    {
        // Assuming ParticipantReport is a list of SP_GetParticipants_Result
        public List<SP_Participant_Report> ParticipantReport { get; set; }
        public List<SP_Participant_Report> LocationReport { get; set; }
        public List<SP_Participant_Report> ParticipantStatusReport { get; set; }
        public List<SP_Participant_Report> MeetingReport { get; set; }
        public List<SP_GetParticipants_Result> ParticipantList { get; set; }
       
        // Other properties or methods
    }


}