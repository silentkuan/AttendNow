using Newtonsoft.Json;
using AttendNow.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AttendNow.Models.ViewModel;
using System.Globalization;
using System.Data.Entity;
using System.Net.Mail;
using System.Configuration;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using ZXing;
using ZXing.QrCode;

namespace AttendNow.Controllers
{
    public class HomeController : Controller
    {
        //Use for forget password which is temporary stored place
        private static readonly object lockObject = new object();
        private static readonly ConcurrentDictionary<string, Tuple<string, DateTime>> storedValues = new ConcurrentDictionary<string, Tuple<string, DateTime>>();

        public ActionResult Index()
        {
            try
            {
                if (Session["user_id"] == null)
                {
                    return RedirectToAction("Logout", "User");
                }
                using (var db = new AttendNow_DBEntities())
                {
                   
                    var startDate = "";
                    var endDate = "";
                    var lastMonthStartDate = "";
                    var lastMonthEndDate = "";
                    TimeZoneInfo serverTimeZone = null; 
                    TimeZoneInfo convertedTimeZone = null; 
                    DateTime serverStartDateTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1); // Set to the first day of the current month
                    DateTime serverEndDateTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month))
                        .AddHours(23).AddMinutes(59).AddSeconds(59); /// Set to the end of the day
                    

                    //Vietnam , Thailand Timezone
                    if ((string)Session["timezone"] == "V")
                    {
                        //Set Vietnam , Thailand Timezone
                        convertedTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

                        // Set the Kind property to Local
                        serverStartDateTime = DateTime.SpecifyKind(serverStartDateTime, DateTimeKind.Unspecified);
                        serverEndDateTime = DateTime.SpecifyKind(serverEndDateTime, DateTimeKind.Unspecified);

                        // Convert to UTC (get thailand timezone)
                        DateTime utcStartDate = TimeZoneInfo.ConvertTimeToUtc(serverStartDateTime, convertedTimeZone);
                        DateTime utcEndDate = TimeZoneInfo.ConvertTimeToUtc(serverEndDateTime, convertedTimeZone);

                        serverTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");

                        //Get Server timezone from thailand timezone
                        startDate = TimeZoneInfo.ConvertTime(utcStartDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");
                        endDate = TimeZoneInfo.ConvertTime(utcEndDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");

                        lastMonthStartDate = TimeZoneInfo.ConvertTime(utcStartDate, serverTimeZone).AddMonths(-1).ToString("yyyy-MM-dd HH:mm:ss.fff");
                        lastMonthEndDate = TimeZoneInfo.ConvertTime(utcEndDate, serverTimeZone).AddMonths(-1).ToString("yyyy-MM-dd HH:mm:ss.fff");

                    }
                    else if ((string)Session["timezone"] == "J")
                    {
                        //Set Jordan Timezone
                        convertedTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Jordan Standard Time");

                        // Set the Kind property to Local
                        serverStartDateTime = DateTime.SpecifyKind(serverStartDateTime, DateTimeKind.Unspecified);
                        serverEndDateTime = DateTime.SpecifyKind(serverEndDateTime, DateTimeKind.Unspecified);

                        // Convert to UTC (get jordan timezone)
                        DateTime utcStartDate = TimeZoneInfo.ConvertTimeToUtc(serverStartDateTime, convertedTimeZone);
                        DateTime utcEndDate = TimeZoneInfo.ConvertTimeToUtc(serverEndDateTime, convertedTimeZone);


                        serverTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");

                        //Get Server timezone from jordan timezone
                        startDate = TimeZoneInfo.ConvertTime(utcStartDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");
                        endDate = TimeZoneInfo.ConvertTime(utcEndDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");
                        lastMonthStartDate = TimeZoneInfo.ConvertTime(utcStartDate, serverTimeZone).AddMonths(-1).ToString("yyyy-MM-dd HH:mm:ss.fff");
                        lastMonthEndDate = TimeZoneInfo.ConvertTime(utcEndDate, serverTimeZone).AddMonths(-1).ToString("yyyy-MM-dd HH:mm:ss.fff");

                    }
                    else if ((string)Session["timezone"] == "M")
                    {
                        //Set Server Timezone
                        convertedTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");

                        // Set the Kind property to Local
                        serverStartDateTime = DateTime.SpecifyKind(serverStartDateTime, DateTimeKind.Unspecified);
                        serverEndDateTime = DateTime.SpecifyKind(serverEndDateTime, DateTimeKind.Unspecified);

                        // Convert to UTC (get server timezone)
                        DateTime utcStartDate = TimeZoneInfo.ConvertTimeToUtc(serverStartDateTime, convertedTimeZone);
                        DateTime utcEndDate = TimeZoneInfo.ConvertTimeToUtc(serverEndDateTime, convertedTimeZone);

                        serverTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");

                        //Get Server timezone from server timezone
                        startDate = TimeZoneInfo.ConvertTime(utcStartDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");
                        endDate = TimeZoneInfo.ConvertTime(utcEndDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");
                        lastMonthStartDate = TimeZoneInfo.ConvertTime(utcStartDate, serverTimeZone).AddMonths(-1).ToString("yyyy-MM-dd HH:mm:ss.fff");
                        lastMonthEndDate = TimeZoneInfo.ConvertTime(utcEndDate, serverTimeZone).AddMonths(-1).ToString("yyyy-MM-dd HH:mm:ss.fff");
                    }

                    //Get meeting report data
                    var meetingReport = db.Database.SqlQuery<SP_Participant_Report>(
                              "SP_GetParticipantsReport @MeetingCode,@ControlBy,@StatusOptions, @Criteria, @Text, @DateType, @StartDate, @EndDate ,@Type",

                                         new SqlParameter("MeetingCode", ""),
                                         new SqlParameter("ControlBy", (string)Session["user_id"]?.ToString()),

                                          new SqlParameter("StatusOptions", "A"),
                                          new SqlParameter("Criteria", ""),
                                          new SqlParameter("Text", ""),
                                          new SqlParameter("DateType", "createDate"),
                                          new SqlParameter("StartDate", startDate),
                                          new SqlParameter("EndDate", endDate),
                                          new SqlParameter("Type", "meeting")

                                       ).ToList();

                    //get meeting report data (chart)
                            var meetingReportBasedDate = db.Database.SqlQuery<SP_Participant_Report>(
                         "SP_GetParticipantsReport @MeetingCode,@ControlBy,@StatusOptions, @Criteria, @Text, @DateType, @StartDate, @EndDate ,@Type,@BasedDate",

                                  new SqlParameter("MeetingCode", ""),
                                  new SqlParameter("ControlBy", (string)Session["user_id"]?.ToString()),

                                   new SqlParameter("StatusOptions", "A"),
                                   new SqlParameter("Criteria", ""),
                                   new SqlParameter("Text", ""),
                                   new SqlParameter("DateType", "createDate"),
                                   new SqlParameter("StartDate", startDate),
                                   new SqlParameter("EndDate", endDate),
                                   new SqlParameter("Type", "meeting"),
                                    new SqlParameter("BasedDate", "Y")

                                ).ToList();
            //GRAF MEETING PART


            var meetingTop3 = meetingReport
            .GroupBy(item => item.total)
            .Select(group => new { Meeting = string.Join(", ", group.Select(item => item.meeting_code + " (" + item.meeting + ")")), Total = group.Key })
            
            .ToList();
            //pie data
            ViewBag.meetingCodeTop3 = JsonConvert.SerializeObject(meetingTop3.Select(item => item.Meeting).ToArray());
            ViewBag.meetingTotalTop3 = JsonConvert.SerializeObject(meetingTop3.Select(item => item.Total).ToArray());
            

            //chart data based on date
            ViewBag.meetingTop3BasedDate = JsonConvert.SerializeObject(meetingReportBasedDate
                            .GroupBy(item => item.meeting_code)

                            .Select(group => group

                                .Select(item => new List<object> { item.meeting_code + " (" + item.meeting + ")", item.joinedDate, item.total })
                            )
                            .ToList());
                  

                    var participantReport = db.Database.SqlQuery<SP_Participant_Report>(
                               "SP_GetParticipantsReport @MeetingCode,@ControlBy,@StatusOptions, @Criteria, @Text, @DateType, @StartDate, @EndDate ,@Type",

                                          new SqlParameter("MeetingCode", ""),
                                          new SqlParameter("ControlBy", (string)Session["user_id"]?.ToString()),

                                           new SqlParameter("StatusOptions", "A"),
                                           new SqlParameter("Criteria", ""),
                                           new SqlParameter("Text", ""),
                                           new SqlParameter("DateType", "joinDate"),
                                           new SqlParameter("StartDate", startDate),
                                           new SqlParameter("EndDate", endDate),
                                           new SqlParameter("Type", "participant")

                                        ).ToList();

                            var participantTop3 = participantReport
                                    .GroupBy(item => item.total)
                                    .Select(group => new { Participant = string.Join(", ", group.Select(item => item.staff_no + " (" + item.name + ")")), Total = group.Key })
                    
                                    .ToList();

                    //pie data
                    ViewBag.participantNameTop3 = JsonConvert.SerializeObject(participantTop3.Select(item => item.Participant).ToArray());
                    ViewBag.participantTotalTop3 = JsonConvert.SerializeObject(participantTop3.Select(item => item.Total).ToArray());

                            ViewBag.Notification = "Welcome to AttendNow";
                            var roleID = Session["role_id"]?.ToString();
                            var meetingAccess = db.sys_setting_role_permission.SingleOrDefault(rp => rp.role == roleID && rp.module == "meeting");
                            var participantAccess = db.sys_setting_role_permission.SingleOrDefault(rp => rp.role == roleID && rp.module == "participant");

                    //get total report for this month's meeting
                    var thisMonthMeeting = db.Database.SqlQuery<SP_Meeting_Sum_Report>(
                                      "SP_GetMeetingsReport @StaffNo,@MeetingCode,@StatusOptions,@ConditionOptions,@TypeOptions,MeetingType,@Criteria,@Text,@DateType," +
                                      "@StartDate, @EndDate, @MeetingStartDate, @MeetingEndDate, @Type",
                                      new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                                      new SqlParameter("MeetingCode", ""),
                                      new SqlParameter("StatusOptions", "A"),
                                      new SqlParameter("ConditionOptions", ""),
                                      new SqlParameter("TypeOptions", ""),
                                      new SqlParameter("MeetingType", ""),
                                      new SqlParameter("Criteria", ""),
                                      new SqlParameter("Text", ""),
                                      new SqlParameter("DateType", "month"),
                                      new SqlParameter("StartDate", startDate),
                                      new SqlParameter("EndDate", endDate),
                                      new SqlParameter("MeetingStartDate", ""),
                                      new SqlParameter("MeetingEndDate", ""),
                                      new SqlParameter("Type", "sum")).ToList();

                    //get total report for last month's meeting
                    var lastMonthMeeting = db.Database.SqlQuery<SP_Meeting_Sum_Report>(
                                      "SP_GetMeetingsReport @StaffNo,@MeetingCode,@StatusOptions,@ConditionOptions,@TypeOptions,@MeetingType,@Criteria,@Text,@DateType," +
                                      "@StartDate, @EndDate, @MeetingStartDate, @MeetingEndDate, @Type",
                                      new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                                      new SqlParameter("MeetingCode", ""),
                                      new SqlParameter("StatusOptions", "A"),
                                      new SqlParameter("ConditionOptions", ""),
                                      new SqlParameter("TypeOptions", ""),
                                       new SqlParameter("MeetingType", ""),
                                      new SqlParameter("Criteria", ""),
                                      new SqlParameter("Text", ""),
                                      new SqlParameter("DateType", "month"),
                                      new SqlParameter("StartDate", lastMonthStartDate),
                                      new SqlParameter("EndDate", lastMonthEndDate),
                                      new SqlParameter("MeetingStartDate", ""),
                                      new SqlParameter("MeetingEndDate", ""),
                                      new SqlParameter("Type", "sum")).ToList();
                    ViewBag.totalMeeting = thisMonthMeeting.FirstOrDefault().total;
                    ViewBag.totalLastMonthMeeting = lastMonthMeeting.FirstOrDefault().total;

                    ViewBag.percentageMeeting = ((double)(thisMonthMeeting.FirstOrDefault().total - lastMonthMeeting.FirstOrDefault().total) / lastMonthMeeting.FirstOrDefault().total) * 100;


                    //get total report for this month's participant
                    var thisMonthParticipant = db.Database.SqlQuery<SP_Participant_Sum_Report>(
                               "SP_GetParticipantsReport @MeetingCode,@ControlBy,@StatusOptions, @Criteria, @Text, @DateType, @StartDate, @EndDate ,@Type",

                                          new SqlParameter("MeetingCode", ""),
                                          new SqlParameter("ControlBy", (string)Session["user_id"]?.ToString()),

                                           new SqlParameter("StatusOptions", "A"),
                                           new SqlParameter("Criteria", ""),
                                           new SqlParameter("Text", ""),
                                           new SqlParameter("DateType", "month"),
                                           new SqlParameter("StartDate", startDate),
                                           new SqlParameter("EndDate", endDate),
                                           new SqlParameter("Type", "sum")

                                        ).ToList();

                    //get total report for last month's participant
                    var lastMonthParticipant = db.Database.SqlQuery<SP_Participant_Sum_Report>(
                               "SP_GetParticipantsReport @MeetingCode,@ControlBy,@StatusOptions, @Criteria, @Text, @DateType, @StartDate, @EndDate ,@Type",

                                          new SqlParameter("MeetingCode", ""),
                                          new SqlParameter("ControlBy", (string)Session["user_id"]?.ToString()),

                                           new SqlParameter("StatusOptions", "A"),
                                           new SqlParameter("Criteria", ""),
                                           new SqlParameter("Text", ""),
                                           new SqlParameter("DateType", "month"),
                                           new SqlParameter("StartDate", lastMonthStartDate),
                                           new SqlParameter("EndDate", lastMonthEndDate),
                                           new SqlParameter("Type", "sum")).ToList();
                    ViewBag.totalLastMonthParticipant = lastMonthParticipant.FirstOrDefault().total;
                    ViewBag.totalParticipant = thisMonthParticipant.FirstOrDefault().total;
                    ViewBag.percentageParticipant = ((double)(thisMonthParticipant.FirstOrDefault().total - lastMonthParticipant.FirstOrDefault().total) / lastMonthParticipant.FirstOrDefault().total) * 100;
                    ViewBag.meetingAccess = meetingAccess;
                    ViewBag.participantAccess = participantAccess;
                    TempData["page"] = "dashboard";
                    var participantData = new ParticipantData();
                    participantData.MeetingReport = meetingReport;
                    return View(participantData);
                }

            }
            catch (DbEntityValidationException e)
            {

                var errormessage = "";
                foreach (var eve in e.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        errormessage += ve.PropertyName + ": " + ve.ErrorMessage;
                    }
                }

                ViewBag.ErrorMessage = errormessage;
                return View();

            }
            catch (Exception e)
            {

                ViewBag.ErrorMessage = e.InnerException == null ? e.Message : e.InnerException.Message;
                return View();
            }
        }

        public ActionResult NotFound()
        {

            return View();
        }

        public ActionResult Calendar()
        {
            try { 
            using (var db = new AttendNow_DBEntities())
            {
                    if (Session["user_id"] == null)
                    {
                        return RedirectToAction("Logout", "User");
                    }
                    
                    var startDate = "";
                    var endDate = "";
                    var month = "";
                    DateTime serverStartDateTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1); // Set to the first day of the current month
                    DateTime serverEndDateTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month))
                        .AddHours(23).AddMinutes(59).AddSeconds(59); /// Set to the end of the day

                    TimeZoneInfo serverTimeZone = null;
                    TimeZoneInfo convertedTimeZone = null;
                    if ((string)Session["timezone"] == "V")
                    {

                        convertedTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

                        // Set the Kind property to Local
                        serverStartDateTime = DateTime.SpecifyKind(serverStartDateTime, DateTimeKind.Unspecified);
                        serverEndDateTime = DateTime.SpecifyKind(serverEndDateTime, DateTimeKind.Unspecified);

                        // Convert to UTC
                        DateTime utcStartDate = TimeZoneInfo.ConvertTimeToUtc(serverStartDateTime, convertedTimeZone);
                        DateTime utcEndDate = TimeZoneInfo.ConvertTimeToUtc(serverEndDateTime, convertedTimeZone);

                        serverTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                        startDate = TimeZoneInfo.ConvertTime(utcStartDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");
                        endDate = TimeZoneInfo.ConvertTime(utcEndDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");
                        month = TimeZoneInfo.ConvertTime(utcEndDate, serverTimeZone).Month.ToString();
                    }
                    else if ((string)Session["timezone"] == "J")
                    {

                        convertedTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Jordan Standard Time");

                        // Set the Kind property to Local
                        serverStartDateTime = DateTime.SpecifyKind(serverStartDateTime, DateTimeKind.Unspecified);
                        serverEndDateTime = DateTime.SpecifyKind(serverEndDateTime, DateTimeKind.Unspecified);

                        // Convert to UTC
                        DateTime utcStartDate = TimeZoneInfo.ConvertTimeToUtc(serverStartDateTime, convertedTimeZone);
                        DateTime utcEndDate = TimeZoneInfo.ConvertTimeToUtc(serverEndDateTime, convertedTimeZone);

                        serverTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                        startDate = TimeZoneInfo.ConvertTime(utcStartDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");
                        endDate = TimeZoneInfo.ConvertTime(utcEndDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");
                        month = TimeZoneInfo.ConvertTime(utcEndDate, serverTimeZone).Month.ToString();
                    }
                    else if ((string)Session["timezone"] == "M")
                    {
                        convertedTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");

                        // Set the Kind property to Local
                        serverStartDateTime = DateTime.SpecifyKind(serverStartDateTime, DateTimeKind.Unspecified);
                        serverEndDateTime = DateTime.SpecifyKind(serverEndDateTime, DateTimeKind.Unspecified);

                        // Convert to UTC
                        DateTime utcStartDate = TimeZoneInfo.ConvertTimeToUtc(serverStartDateTime, convertedTimeZone);
                        DateTime utcEndDate = TimeZoneInfo.ConvertTimeToUtc(serverEndDateTime, convertedTimeZone);

                        serverTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                        startDate = TimeZoneInfo.ConvertTime(utcStartDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");
                        endDate = TimeZoneInfo.ConvertTime(utcEndDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");
                        month = TimeZoneInfo.ConvertTime(utcEndDate, serverTimeZone).Month.ToString();
                    }

                    //meeting calendar data 
                    var meetingsList = db.Database.SqlQuery<SP_GetMeetingsForCalander_Result>(
                     "SP_GetMeetingsForCalander @StaffNo,@Type, @Place,@StartDate,@EndDate",
                     new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                     new SqlParameter("Type", ""),
                     new SqlParameter("Place", ""),
                     new SqlParameter("StartDate", startDate),
                      new SqlParameter("EndDate", endDate)
                    ).ToList();
                    
                    // Serialize the simplifiedMeetingsList to JSON
                    ViewBag.organizedMeeting = JsonConvert.SerializeObject(meetingsList);
                    

                    //place calendar data
                    var placeList = db.Database.SqlQuery<SP_GetMeetingsForCalander_Result>(
                     "SP_GetMeetingsForCalander @StaffNo, @Type, @Place,@StartDate,@EndDate",
                     new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                     new SqlParameter("Type", "P"),
                     new SqlParameter("Place", ""),
                     new SqlParameter("StartDate", startDate),
                      new SqlParameter("EndDate", endDate)
                    ).ToList();
                    
                    // Serialize the simplifiedMeetingsList to JSON
                    ViewBag.usedPlace = JsonConvert.SerializeObject(placeList);

                    var activePlace = db.Database.SqlQuery<SP_GetPlaceWithFactory_Result>(
                                    "SP_GetPlaceWithFactory @PlaceCode ,@StatusOptions ,@Criteria,@Text,@DateType,@StartDate,@EndDate,@FactoryID,@ControlBy",
                                   new SqlParameter("PlaceCode", ""),
                                    new SqlParameter("StatusOptions", "A"),
                                    new SqlParameter("Criteria", ""),
                                    new SqlParameter("Text", ""),
                                    new SqlParameter("DateType", ""),
                                    new SqlParameter("StartDate", ""),
                                    new SqlParameter("EndDate", ""),
                                    new SqlParameter("FactoryID", ""),
                                    new SqlParameter("ControlBy", Session["user_id"])
                                    ).ToList();

                    //PLACE Record
                    var placeRecord = db.Database.SqlQuery<SP_GetPlaceRecord_Result>(
                      "SP_GetPlaceRecord @StaffNo,@Place,@StatusOptions,@StartDate,@EndDate",
                      new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                        new SqlParameter("Place", ""),
                      new SqlParameter("StatusOptions", ""),
                     new SqlParameter("StartDate", startDate),
                      new SqlParameter("EndDate", endDate)
                     ).ToList();

                    ViewBag.activePlace = activePlace;
                    var roleID = Session["role_id"]?.ToString();
                    var meetingAccess = db.sys_setting_role_permission.SingleOrDefault(rp => rp.role == roleID && rp.module == "meeting");
                    ViewBag.meetingAccess = meetingAccess;
                    ViewBag.dataDate = "Month " + month;
                    // Log to console
                    return View(placeRecord);
                }
            }
                catch (DbEntityValidationException e)
                {
                    var errormessage = "";
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        foreach (var ve in eve.ValidationErrors)
                        {
                            errormessage += ve.PropertyName + ": " + ve.ErrorMessage;
                        }
    }
    ViewBag.ErrorMessage = errormessage;
    return View();

                }
                catch (Exception e)
    {

        ViewBag.ErrorMessage = e.InnerException == null ? e.Message : e.InnerException.Message;
        return View();
    }
        }

        [HttpPost]
        public ActionResult FilterCalendar(List<string> selectedPlace, DateTime ?selectedMonth)
        {

            try
            {
               
                using (var db = new AttendNow_DBEntities())
                {
                    string place;
                    if (selectedPlace == null)
                    {
                         place = "";
                    }
                    else
                    {
                        place = string.Join(",", selectedPlace);
                    }
                   
                    var startDate = "";
                    var endDate = "";
                    DateTime serverStartDateTime;
                    DateTime serverEndDateTime ;
                    if (selectedMonth != null)
                    {
                         serverStartDateTime = new DateTime(selectedMonth.Value.Year, selectedMonth.Value.Month, 1); // Set to the first day of the current month
                         serverEndDateTime = new DateTime(selectedMonth.Value.Year, selectedMonth.Value.Month, DateTime.DaysInMonth(selectedMonth.Value.Year, selectedMonth.Value.Month))
                            .AddHours(23).AddMinutes(59).AddSeconds(59); /// Set to the end of the day
                    }
                    else
                    {
                         serverStartDateTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1); // Set to the first day of the current month
                         serverEndDateTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month))
                            .AddHours(23).AddMinutes(59).AddSeconds(59); /// Set to the end of the day
                    }
                    TimeZoneInfo serverTimeZone = null;
                        TimeZoneInfo convertedTimeZone = null;
                        if ((string)Session["timezone"] == "V")
                        {

                            convertedTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

                            // Set the Kind property to Local
                            serverStartDateTime = DateTime.SpecifyKind(serverStartDateTime, DateTimeKind.Unspecified);
                            serverEndDateTime = DateTime.SpecifyKind(serverEndDateTime, DateTimeKind.Unspecified);

                            // Convert to UTC
                            DateTime utcStartDate = TimeZoneInfo.ConvertTimeToUtc(serverStartDateTime, convertedTimeZone);
                            DateTime utcEndDate = TimeZoneInfo.ConvertTimeToUtc(serverEndDateTime, convertedTimeZone);

                            serverTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                            startDate = TimeZoneInfo.ConvertTime(utcStartDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");
                            endDate = TimeZoneInfo.ConvertTime(utcEndDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");
                           
                        }
                        else if ((string)Session["timezone"] == "J")
                        {

                            convertedTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Jordan Standard Time");

                            // Set the Kind property to Local
                            serverStartDateTime = DateTime.SpecifyKind(serverStartDateTime, DateTimeKind.Unspecified);
                            serverEndDateTime = DateTime.SpecifyKind(serverEndDateTime, DateTimeKind.Unspecified);

                            // Convert to UTC
                            DateTime utcStartDate = TimeZoneInfo.ConvertTimeToUtc(serverStartDateTime, convertedTimeZone);
                            DateTime utcEndDate = TimeZoneInfo.ConvertTimeToUtc(serverEndDateTime, convertedTimeZone);

                            serverTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                            startDate = TimeZoneInfo.ConvertTime(utcStartDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");
                            endDate = TimeZoneInfo.ConvertTime(utcEndDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");
                           
                        }
                        else if ((string)Session["timezone"] == "M")
                        {
                            convertedTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");

                            // Set the Kind property to Local
                            serverStartDateTime = DateTime.SpecifyKind(serverStartDateTime, DateTimeKind.Unspecified);
                            serverEndDateTime = DateTime.SpecifyKind(serverEndDateTime, DateTimeKind.Unspecified);

                            // Convert to UTC
                            DateTime utcStartDate = TimeZoneInfo.ConvertTimeToUtc(serverStartDateTime, convertedTimeZone);
                            DateTime utcEndDate = TimeZoneInfo.ConvertTimeToUtc(serverEndDateTime, convertedTimeZone);

                            serverTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                            startDate = TimeZoneInfo.ConvertTime(utcStartDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");
                            endDate = TimeZoneInfo.ConvertTime(utcEndDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");
                            
                        }
                    
                    var roleID = Session["role_id"]?.ToString();
                        var meetingAccess = db.sys_setting_role_permission.SingleOrDefault(rp => rp.role == roleID && rp.module == "meeting");
                        var editable = meetingAccess.editFunction;

                        //meeting calendar data 
                        var meetingsList = JsonConvert.SerializeObject(db.Database.SqlQuery<SP_GetMeetingsForCalander_Result>(
                         "SP_GetMeetingsForCalander @StaffNo,@Type,@Place,@StartDate,@EndDate",
                         new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                         new SqlParameter("Type", ""),
                         new SqlParameter("Place", place),
                         new SqlParameter("StartDate", startDate),
                         new SqlParameter("EndDate", endDate)
                        ).ToList());



                        //place calendar data
                        var placeList = JsonConvert.SerializeObject(db.Database.SqlQuery<SP_GetMeetingsForCalander_Result>(
                         "SP_GetMeetingsForCalander @StaffNo,@Type,@Place,@StartDate,@EndDate",
                         new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                          new SqlParameter("Type", "P"),
                          new SqlParameter("Place", place),
                         new SqlParameter("StartDate", startDate),
                         new SqlParameter("EndDate", endDate)
                        ).ToList());

                    // place record
                    var placeRecord = db.Database.SqlQuery<SP_GetPlaceRecord_Result>(
                      "SP_GetPlaceRecord @StaffNo,@Place,@StatusOptions,@StartDate,@EndDate",
                      new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                        new SqlParameter("Place", place),
                      new SqlParameter("StatusOptions", ""),
                      new SqlParameter("StartDate", startDate),
                         new SqlParameter("EndDate", endDate)
                     ).ToList();
                    var placeRecordData = placeRecord.Select(p => new
                    {
                        startDate = p.startDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture),
                        endDate = p.endDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture),
                        p.placeName,
                        p.title,
                        p.organizer,
                        created_by = p.CreateByName + " (" + p.createBy.ToUpper() + ")",
                        createDate = p.createDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture),


                    });
                    return Json(new { success = true, message = "Venue Filter Successfully.", meetingsList = meetingsList, placeList = placeList, editable= editable, placeRecordData = placeRecordData });
                  
                }




            }
            catch (DbEntityValidationException e)
            {
                var errormessage = "";
                foreach (var eve in e.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        errormessage += ve.PropertyName + ": " + ve.ErrorMessage;
                    }
                }
                return Json(new { success = false, error_message = errormessage });

            }
            catch (Exception e)
            {
                return Json(new { success = false, error_message = e.InnerException == null ? e.Message : e.InnerException.Message });

            }
        }


        [ChildActionOnly]
        public ActionResult Navigation()
        {

            if (Session["user_id"] == null)
            {
                return RedirectToAction("Logout", "User");
            }
            using (var db = new AttendNow_DBEntities())
            {
                var existingUser = db.Database.SqlQuery<SP_CheckUserCredentials_Result>("SP_CheckUserCredentials @StaffNo, @EncryptedPassword",
                                                    new SqlParameter("StaffNo", (string)Session["user_id"]),
                                                    new SqlParameter("EncryptedPassword", "allow")
                                                ).ToList();

                return PartialView("~/Views/Shared/_Navigation.cshtml", existingUser);
            }
        }
        /*------------------------------------------Define Table Column START------------------------------------------*/
        [HttpPost]
        public ActionResult EditUserDefine(List<DefineTable> checkboxData,string module)
        {
            var userId = (string)Session["user_id"];
            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    List<sys_setting_define_table> defineTableList = db.sys_setting_define_table
                    .Where(item => item.createBy == userId && item.module == module)
                    .ToList();

                    foreach (var checkbox in checkboxData)
                    {
                        string field = checkbox.Name;
                        string value = checkbox.Value;

                        var itemToUpdate = defineTableList.FirstOrDefault(dt => dt.field == field);

                        if (itemToUpdate != null)
                        {
                            // Update the 'status' property based on the checkbox value
                            itemToUpdate.status = value;
                            db.Entry(itemToUpdate).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }

                    @TempData["SuccessMessage"] = "Define Table Column Successfully.";
                    // Return a success response
                    return Json(new { success = true });
                }


            }
            catch (DbEntityValidationException e)
            {
                var errormessage = "";
                foreach (var eve in e.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        errormessage += ve.PropertyName + ": " + ve.ErrorMessage;
                    }
                }
                @TempData["ErrorMessage"] = errormessage;
                return Json(new { success = false });

            }
            catch (Exception e)
            {
                @TempData["ErrorMessage"] = e.InnerException == null ? e.Message : e.InnerException.Message;
                return Json(new { success = false });

            }
        }


        [HttpPost]
        public ActionResult AddUserDefine(List<DefineTable> checkboxData, string module)
        {
            var userId = (string)Session["user_id"];
            try
            {
                using (var db = new AttendNow_DBEntities())
                {

                    foreach (var checkbox in checkboxData)
                    {
                        string field = checkbox.Name;
                        string value = checkbox.Value;
                        sys_setting_define_table define_table = new sys_setting_define_table
                        {
                            module = module,
                            field = field,
                            createBy = userId,
                            createDate = DateTime.Now,
                            status = value
                        };

                        db.sys_setting_define_table.Add(define_table);



                    }
                    db.SaveChanges();
                    @TempData["SuccessMessage"] = "Define Table Column Successfully.";
                    // Return a success response
                    return Json(new { success = true });
                }

            }
            catch (DbEntityValidationException e)
            {
                var errormessage = "";
                foreach (var eve in e.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        errormessage += ve.PropertyName + ": " + ve.ErrorMessage;
                    }
                }
                @TempData["ErrorMessage"] = errormessage;
                return Json(new { success = false });

            }
            catch (Exception e)
            {
                @TempData["ErrorMessage"] = e.InnerException == null ? e.Message : e.InnerException.Message;
                return Json(new { success = false });

            }
        }
        /*------------------------------------------Define Table Column END------------------------------------------*/

        /*------------------------------------------Password START-----------------------------------------*/
        public ActionResult ForgetPassword()
        {
            return View();
        }
        public ActionResult ChangePassword(string id,string model)
        {
            string retrievedValue = RetrieveStoredValue(id);
            if (retrievedValue == null)
            {
                ViewBag.ErrorMessage = "This link has been expired or You already changed password.";
            }
            else
            {
                string SecretKey = ConfigurationManager.AppSettings["SecretKey"]; //Get SecretKey

                long SecretKey_8byte = StringTo8ByteValue(SecretKey);

                long Salt_8byte = StringTo8ByteValue(retrievedValue);
                ViewBag.encrypted_id = id;
                ViewBag.id = Decrypt(id, SecretKey_8byte.ToString(), Salt_8byte.ToString());
                ViewBag.model = model;
            }

            return View();
        }

        public JsonResult ChangeParticipantPassword(string staffNo, string newPassword, string id, string model)
        {

            try
            {
                string retrievedValue = RetrieveStoredValue(id);
                if (retrievedValue == null)
                {

                    return Json(new { success = false, error_message = "Changed password unsuccessfully. This link has been expired or You already changed password." });
                }
                using (var db = new AttendNow_DBEntities())
                {
                    // Find the existing user in the database by user_id
                   
                    if (model == "P") {
                        var existingUser = db.tmp_participant.SingleOrDefault(p => p.staff_no.ToUpper() == staffNo.ToUpper());
                        if (existingUser != null)
                        {
                            string SecretKey = ConfigurationManager.AppSettings["SecretKey"]; //Get SecretKey
                            long SecretKey_8byte = StringTo8ByteValue(SecretKey);
                            long staff_no_8byte = StringTo8ByteValue(staffNo);
                            // Update the status property based on the newStatus parameter
                            existingUser.password = Encrypt(newPassword, SecretKey_8byte.ToString(), staff_no_8byte.ToString());
                            existingUser.editBy = existingUser.staff_no;
                            existingUser.editDate = DateTime.Now;
                            db.Entry(existingUser).State = EntityState.Modified;


                        }
                        else
                        {
                            return Json(new { success = false, error_message = "Not found" });
                        }
                    }else if (model == "U")
                    {
                        var existingUser = db.tbl_user.SingleOrDefault(p => p.staff_no.ToUpper() == staffNo.ToUpper());
                        if (existingUser != null)
                        {
                            string SecretKey = ConfigurationManager.AppSettings["SecretKey"]; //Get SecretKey
                            long SecretKey_8byte = StringTo8ByteValue(SecretKey);
                            long staff_no_8byte = StringTo8ByteValue(staffNo);
                            // Update the status property based on the newStatus parameter
                            existingUser.password = Encrypt(newPassword, SecretKey_8byte.ToString(), staff_no_8byte.ToString());
                            existingUser.editBy = existingUser.staff_no;
                            existingUser.editDate = DateTime.Now;
                            db.Entry(existingUser).State = EntityState.Modified;


                        }
                        else
                        {
                            return Json(new { success = false, error_message = "Not found" });
                        }
                    }
                    db.SaveChanges();
                    ClearValue(id);

                    return Json(new
                    {
                        success = true,
                        message = "Changed Password Successfully.",

                    });
                }
            }
            catch (DbEntityValidationException e)
            {
                var errormessage = "";
                foreach (var eve in e.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        errormessage += ve.PropertyName + ": " + ve.ErrorMessage;
                    }
                }
                return Json(new { success = false, error_message = errormessage });

            }
            catch (Exception e)
            {
                return Json(new { success = false, error_message = e.InnerException == null ? e.Message : e.InnerException.Message });

            }
        }
        public JsonResult SendEmailForChangePassword(string staff_no, string email, string model)
        {

            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    var existUser = false;
                    if (model == "P")//Participant
                    {
                        existUser = db.tmp_participant.Any(p => p.staff_no == staff_no && p.email == email);
                    }
                    else if(model=="U")
                    {
                        existUser = db.tbl_user.Any(p => p.staff_no == staff_no && p.email == email);
                    }

                    if (existUser)
                    {
                        TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

                        string SecretKey = ConfigurationManager.AppSettings["SecretKey"]; //Get SecretKey
                        string EmailAddress = ConfigurationManager.AppSettings["EmailAddress"]; //Get SecretKey
                        string EmailPassword = ConfigurationManager.AppSettings["EmailPassword"]; //Get SecretKey
                        var fromAddress = new MailAddress(EmailAddress, "AttendNow"); // NEED TO CHANGE EMAIL ACCOUNT
                        var toAddress = new MailAddress(email, staff_no);
                        var subject = "Reset Password";

                        var title = "Reset Your password";
                        
                        long SecretKey_8byte = StringTo8ByteValue(SecretKey);



                        string ramdomKey = new string(Enumerable.Range(0, 6).Select(_ => (char)('0' + new Random().Next(10))).ToArray());
                        // Store the value for 1 minute

                        long Salt_8byte = StringTo8ByteValue(ramdomKey);

                        var encrypted_staffno = Encrypt(staff_no, SecretKey_8byte.ToString(), Salt_8byte.ToString());
                        StoreValue(encrypted_staffno, ramdomKey, TimeSpan.FromMinutes(5));

                        var inform = "<b>[Expired in 5 Minute]</b> Please access this link to reset your password: " + Request.Url.Scheme + @"://" + Request.Url.Authority + Url.Action("ChangePassword", new { id = encrypted_staffno,model=model });

                        var body = @"
                            <html>
                            <head>
                              <style type=""text/css"" rel=""stylesheet"" media=""all"">
                                    .header{
                                    padding:16px;
                                    background-color:#ededed;
                                    text-align:center;
                                    }
                                    hr{
                                    border:1px solid #ededed;
                                    }

                                    .footer{
                                    color:#6b6e76;
       
                                    }
                                    h2,h1,p{
                                    color:#000;
                                    }
        
                                </style>
                            </head>
                            <body>
		
                                    <table  width=""100%"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                                        <tr>
                                          <td align = ""center"" >
                                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                                              <tr>
                                                <td class=""header"">
                                                    <h2>
                      
                                                  " + title + @"
                    
                                                    </h2>
                                                </td>
                                              </tr>
                                              <!-- Email Body -->
                                              <tr>
                                                <td  width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                                  <table  align=""center"" width=""570"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                                                    <!-- Body content -->
                                                    <tr>
                                                      <td >
                                                        <div >
                                                          <h1 >Hi " + textInfo.ToTitleCase(staff_no) + @",</h1>
                               
                                                          <p>" + inform + @"</p>
                              
                                                        
                              
                            
                                                                    </div>
                                                                  </td>
                                                                </tr>
                                                              </table>
                                                            </td>
                                                          </tr>
                                                          <tr>
                    
                                                          </tr>
                                                        </table>
                                                      </td>
                                                    </tr>
                                                  </table>
                                                  <table  align=""center"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                                                                <tr>
                                                                  <td align=""center"">
                                                                    <p class=""footer"">&copy; " + DateTime.Now.Year.ToString() + @" Kuan Jiun Ying. All rights reserved.</p>
                                                                   
                                                                  </td>
                                                                </tr>
                                                              </table>
	                                          </body>
                                        </html>";




                        var smtp = new SmtpClient
                        {
                            Host = "smtp.gmail.com", // Your SMTP host
                            Port = 587,                // Your SMTP port
                            EnableSsl = true,          // Enable SSL if required by your email provider
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            UseDefaultCredentials = false,
                            Credentials = new NetworkCredential(EmailAddress, EmailPassword)// NEED TO CHANGE EMAIL ACCOUNT
                        };

                        using (var message = new MailMessage(fromAddress, toAddress)
                        {
                            Subject = subject,
                            Body = body,
                            IsBodyHtml = true // Set this to true
                        })
                        {
                            smtp.Send(message);
                        }
                        return Json(new
                        {
                            success = true,
                            message = "Kindly review your email for instructions on resetting your password.",


                        });
                    }
                    else
                    {
                        return Json(new { success = false, error_message = "Invalid Staff No or Email" });
                    }
                }
            }
            catch (DbEntityValidationException e)
            {
                var errormessage = "";
                foreach (var eve in e.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        errormessage += ve.PropertyName + ": " + ve.ErrorMessage;
                    }
                }
                return Json(new { success = false, error_message = errormessage });

            }
            catch (Exception e)
            {
                return Json(new { success = false, error_message = e.InnerException == null ? e.Message : e.InnerException.Message });

            }
        }
        public JsonResult BtnEmailNotification()
        {

            try
            {
                
                using (var db = new AttendNow_DBEntities())
                {
                    var participantList = db.Database.SqlQuery<SP_GetEmailNotificationData_Result>(
                               "SP_GetEmailNotificationData").ToList();
                    if (participantList.Count <= 0)
                    {
                        return Json(new
                        {
                            success = true,
                            message = "Process completed. There are no participant need to notice.",


                        });
                    }
                    NoticeEmail("Hope To See You Tomorrow [Reminder]", "Don't forget to join us tomorrow. See you there!","A",""); 
                    return Json(new
                    {
                        success = true,
                        message = "Notice All Participant Successfully.",


                    });
                }
            }
            catch (DbEntityValidationException e)
            {
                var errormessage = "";
                foreach (var eve in e.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        errormessage += ve.PropertyName + ": " + ve.ErrorMessage;
                    }
                }
                return Json(new { success = false, error_message = errormessage });

            }
            catch (Exception e)
            {
                return Json(new { success = false, error_message = e.InnerException == null ? e.Message : e.InnerException.Message });

            }
        }

        /*------------------------------------------Password END-----------------------------------------*/

        public void NoticeEmail(string title, string inform, string type,string meeting_code)
        {
            string EmailAddress = ConfigurationManager.AppSettings["EmailAddress"]; //Get SecretKey
            string EmailPassword = ConfigurationManager.AppSettings["EmailPassword"]; //Get SecretKey
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            var subject = "AttendNow Email Notification";

           
            List<MailAddress> toMailAddresses = new List<MailAddress>();
            var fromAddress = new MailAddress(EmailAddress, "AttendNow");

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com", // Your SMTP host
                Port = 587,               // Your SMTP port
                EnableSsl = true,         // Enable SSL if required by your email provider
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(EmailAddress, EmailPassword) // NEED TO CHANGE EMAIL ACCOUNT
            };
            using (var db = new AttendNow_DBEntities())
            {

                
                var participantList = db.Database.SqlQuery<SP_GetEmailNotificationData_Result>(
                        "SP_GetEmailNotificationData " + meeting_code).ToList();
                var previousMeetingCode = string.Empty;
                var body = string.Empty;
                foreach (var participants in participantList)
                {

                    if (string.IsNullOrEmpty(body))
                    {
                        body = @"
                            <html>
                            <head>
                              <style type=""text/css"" rel=""stylesheet"" media=""all"">
                                    .header{
                                    padding:16px;
                                    background-color:#ededed;
                                    text-align:center;
                                    }
                                    hr{
                                    border:1px solid #ededed;
                                    }

                                    .footer{
                                    color:#6b6e76;
       
                                    }
                                    h3,h2,h1,p{
                                    color:#000;
                                    }
        
                                </style>
                            </head>
                            <body>
		
                                    <table  width=""100%"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                                        <tr>
                                          <td align = ""center"" >
                                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                                              <tr>
                                                <td class=""header"">
                                                    <h2>
                      
                                                  " + title + @"
                    
                                                    </h2>
                                                </td>
                                              </tr>
                                              <!-- Email Body -->
                                              <tr>
                                                <td  width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                                  <table  align=""center"" width=""570"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                                                    <!-- Body content -->
                                                    <tr>
                                                      <td >
                                                        <div >
                                                          
                                                          <h3>" + inform + @"</h3>
                              
                                                          <!-- Action -->
                              
                                                          <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                                            
                                                           
                                                              <td colspan = ""2"" >
                                                                <table  width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                      
                                                                  <br>
                                                                 
                                                                  <h3>Activity Information</h3>
                                      
                                                                  <tr>
                                                                      <td width = ""40%"" ><span >Activity Code</span></td>
                                                                      <td  width=""60%"" ><span >: " + participants.meeting_code + @"</span></td>
                                      
                                                                  </tr>
                                                                      </br>
                                                                   <tr>
                                                                      <td width = ""40%"" ><span >Activity Title </span></td>
                                                                    <td  width=""60%"" ><span >: " + textInfo.ToTitleCase(participants.meeting_title) + @"</span></td>
                                         
                                                                  </tr>
                                                                      </br>
                                                                   <tr>
                                                                      <td width = ""40%"" ><span >Activity Presenter </span></td>
                                                                    <td  width=""60%"" ><span >: " + textInfo.ToTitleCase(participants.meeting_organizer) + @"</span></td>
                                         
                                                                  </tr>
                                                                      </br>
                                                                   <tr>
                                                                      <td width = ""40%"" ><span >Activity Start Time </span></td>
                                                                    <td  width=""60%"" ><span >: (UTC +8:00) " + participants.meeting_startDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture) + @"</span></td>
                                         
                                                                  </tr>
                                                                      </br>
                                                                   <tr>
                                                                      <td width = ""40%"" ><span >Activity End Time </span></td>
                                                                    <td  width=""60%"" ><span >: (UTC +8:00) " + participants.meeting_endDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture) + @"</span></td>
                                         
                                                                  </tr>
                                                                      </br>
                                                                   <tr>
                                                                      <td width = ""40%"" ><span >Activity Privacy Option </span></td>
                                                                   <td width=""60%""><span>: " + (participants.type == true ? "Private" : "Public") + @"</span></td>
                                         
                                                                  </tr>
                                                                      </br>
                                                                    <tr>
                                                                      <td width = ""40%"" ><span >Activity Type </span></td>
                                                                   <td width=""60%""><span>: " + (participants.meeting_type == "M" ? "Meeting" : (participants.meeting_type == "E" ? "Event" : "Training")) + @"</span></td>
                                         
                                                                  </tr>
                                                                      </br>
                                                                   <tr>
                                                                      <td width = ""40%"" ><span >Activity Remark </span></td>
                                                                    <td  width=""60%"" ><span > " + participants.meeting_remark + @"</span></td>
                                         
                                                                  </tr>";

                        if (!string.IsNullOrEmpty(participants.meeting_place) && !string.IsNullOrEmpty(participants.meeting_link))
                        {
                            body += @"
                                                                            </br>
                                                                               <tr>
                                                                                  <td width = ""40%"" ><span >Activity Location </span></td>
                                                                                <td  width=""60%"" ><span >: " + participants.meeting_place + @"</span></td>
                                         
                                                                              </tr>
                                                                                  </br>
                                                                               <tr>
                                                                                  <td width = ""40%"" ><span >Online Meeting URL </span></td>
                                                                                <td  width=""60%"" ><span > " + participants.meeting_link + @"</span></td>
                                         
                                                                              </tr> ";
                        }
                        else if (!string.IsNullOrEmpty(participants.meeting_place))
                        {
                            body += @"
                                                                             </br>
                                                                            <tr>
                                                                                  <td width = ""40%"" ><span >Activity Location </span></td>
                                                                                <td  width=""60%"" ><span >: " + participants.meeting_PlaceName + @"</span></td>
                                         
                                                                              </tr>";
                        }
                        else if (!string.IsNullOrEmpty(participants.meeting_link))
                        {
                            body += @"
                                                                                 </br>
                                                                                 <tr>
                                                                                  <td width = ""40%"" ><span >Online Meeting URL </span></td>
                                                                                <td  width=""60%"" ><span > " + participants.meeting_link + @"</span></td>
                                         
                                                                              </tr>";
                        }


                        body += @"

                                                                            </table>
                                                                          </td>
                                                                        </tr>
                                                                      </table>
                              
                            
                                                                    </div>
                                                                  </td>
                                                                </tr>
                                                              </table>
                                                            </td>
                                                          </tr>
                                                          <tr>
                    
                                                          </tr>
                                                        </table>
                                                      </td>
                                                    </tr>
                                                  </table>
                                                  <table  align=""center"" cellpadding=""0"" cellspacing=""0"" role=""presentation"">
                                                                <tr>
                                                                  <td align=""center"">
                                                                    <p class=""footer"">&copy; " + DateTime.Now.Year.ToString() + @" Kuan Jiun Ying. All rights reserved.</p>
                                                                   
                                                                  </td>
                                                                </tr>
                                                              </table>
	                                          </body>
                                        </html>";
                    }


                    if (participants.meeting_code == previousMeetingCode || string.IsNullOrWhiteSpace(previousMeetingCode))
                    {
                        toMailAddresses.Add(new MailAddress(participants.email));
                        previousMeetingCode = participants.meeting_code;
                    }
                    else
                    {

                        using (var message = new MailMessage())
                        {
                            // Set the sender's address
                            message.From = fromAddress;

                            // Add the recipients to the To property
                            foreach (MailAddress toMailAddress in toMailAddresses)
                            {
                                message.To.Add(toMailAddress);
                            }

                            // Set the subject and HTML body
                            message.Subject = subject;
                            //message.Body = body;
                            message.IsBodyHtml = true; // Set this to true

                            // Add a plain text version of the email
                            AlternateView plainTextView = AlternateView.CreateAlternateViewFromString(body, null, "text/html");
                            message.AlternateViews.Add(plainTextView);
                            message.AlternateViews.Add(plainTextView);
                            smtp.Send(message);
                        }
                        toMailAddresses.Clear();
                        body = string.Empty;
                        previousMeetingCode = string.Empty;

                    }

                }

                using (var message = new MailMessage())
                {
                    // Set the sender's address
                    message.From = fromAddress;

                    // Add the recipients to the To property
                    foreach (MailAddress toMailAddress in toMailAddresses)
                    {
                        message.To.Add(toMailAddress);
                    }

                    // Set the subject and HTML body
                    message.Subject = subject;
                    //message.Body = body;
                    message.IsBodyHtml = true; // Set this to true

                    // Add a plain text version of the email
                    AlternateView plainTextView = AlternateView.CreateAlternateViewFromString(body, null, "text/html");
                    message.AlternateViews.Add(plainTextView);
                    message.AlternateViews.Add(plainTextView);
                    smtp.Send(message);
                }

                


            }

        }

        public static byte[] GenerateQrCode(string data, int width = 150, int height = 150)
        {
            var writer = new BarcodeWriter();
            writer.Format = BarcodeFormat.QR_CODE;
            writer.Options = new QrCodeEncodingOptions
            {
                Width = width,
                Height = height,
                Margin = 0
            };

            var qrCodeBitmap = writer.Write(data);

            // Convert the Bitmap to a byte array
            using (var stream = new MemoryStream())
            {
                qrCodeBitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
        /*------------------------------------------Security Cryptography START------------------------------------------*/

        public static long StringTo8ByteValue(string inputString)
        {
            // Create a SHA-256 hash object
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Compute hash from the input string
                byte[] hashBytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(inputString));

                // Take the first 8 bytes of the hash
                byte[] truncatedBytes = new byte[8];
                Array.Copy(hashBytes, truncatedBytes, 8);

                // Convert the truncated bytes to a long integer
                long result = BitConverter.ToInt64(truncatedBytes, 0);

                return result;
            }
        }


        public static string Encrypt(string password, string secretKey, string salt)
        {
            using (Aes aesAlg = Aes.Create())
            {
                Rfc2898DeriveBytes keyDerivationFunction = new Rfc2898DeriveBytes(secretKey, Encoding.UTF8.GetBytes(salt));

                aesAlg.Key = keyDerivationFunction.GetBytes(aesAlg.KeySize / 8);
                aesAlg.IV = keyDerivationFunction.GetBytes(aesAlg.BlockSize / 8);

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(password);
                        }
                    }

                    // Convert the binary result to Base64Url encoding
                    return ToBase64UrlString(msEncrypt.ToArray());
                }
            }
        }

        public static string Decrypt(string password, string secretKey, string salt)
        {
            // Convert Base64Url to standard Base64
            password = FromBase64UrlString(password);

            using (Aes aesAlg = Aes.Create())
            {
                Rfc2898DeriveBytes keyDerivationFunction = new Rfc2898DeriveBytes(secretKey, Encoding.UTF8.GetBytes(salt));

                aesAlg.Key = keyDerivationFunction.GetBytes(aesAlg.KeySize / 8);
                aesAlg.IV = keyDerivationFunction.GetBytes(aesAlg.BlockSize / 8);

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(password)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }

        private static string ToBase64UrlString(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");
        }

        private static string FromBase64UrlString(string base64Url)
        {
            base64Url = base64Url.Replace('-', '+').Replace('_', '/');
            switch (base64Url.Length % 4)
            {
                case 2: base64Url += "=="; break;
                case 3: base64Url += "="; break;
            }
            return base64Url;
        }



        public static void StoreValue(string key, string value, TimeSpan duration)
        {
            lock (lockObject)
            {
                DateTime expirationTime = DateTime.UtcNow.Add(duration);
                storedValues[key] = Tuple.Create(value, expirationTime);

                // Schedule a background task to remove the value after the specified duration
                Task.Delay(duration).ContinueWith(_ =>
                {
                    lock (lockObject)
                    {
                        storedValues.TryRemove(key, out var removedValue);

                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());

            }
        }

        public static string RetrieveStoredValue(string key)
        {
            lock (lockObject)
            {
                if (storedValues.TryGetValue(key, out var tuple) && tuple.Item2 > DateTime.UtcNow)
                {
                    return tuple.Item1;
                }
                else
                {
                    // Key not found or expired
                    return null;
                }
            }
        }
        public static void ClearValue(string key)
        {
            lock (lockObject)
            {
                storedValues.TryRemove(key, out _);
            }
        }

        /*------------------------------------------Security Cryptography END------------------------------------------*/
    }


}