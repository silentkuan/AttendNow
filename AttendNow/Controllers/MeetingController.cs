using AttendNow.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web.Mvc;
using System.Text;
using System.Configuration;
using Newtonsoft.Json;
using AttendNow.Models.ViewModel;
using System.Web;

namespace AttendNow.Controllers
{
    public class MeetingController : Controller
    {
        // GET: Meeting
        public ActionResult Index()
        {
            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    ViewBag.Title = "Activity List";
                    var roleID = Session["role_id"]?.ToString();
                    var meetingAccess = db.sys_setting_role_permission.SingleOrDefault(rp => rp.role == roleID && rp.module == "meeting");
                    ViewBag.meetingAccess = meetingAccess;
                    if (meetingAccess?.status != true)
                    {
                        return RedirectToAction("Logout", "User");
                    }
                    if (Session["user_id"] == null)
                    {
                        return RedirectToAction("Logout", "User");
                    }
                    var statusOptionsString = "A,V";
                    var dateType = "createDate";
                    var startDate = "";
                    var endDate = "";
                    var month = "";
                    if (meetingAccess.deleteFunction != true)
                    {
                        statusOptionsString = "A";
                    }
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
                        month = TimeZoneInfo.ConvertTime(utcStartDate, serverTimeZone).Month.ToString();
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
                        month = TimeZoneInfo.ConvertTime(utcStartDate, serverTimeZone).Month.ToString();

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
                        month = TimeZoneInfo.ConvertTime(utcStartDate, serverTimeZone).Month.ToString();
                    }

                    //Meeting List
                    var meetingsList = db.Database.SqlQuery<SP_GetMeetings_Result>(
                      "SP_GetMeetings @StaffNo,@MeetingCode,@StatusOptions,@ConditionOptions,@TypeOptions,@MeetingType,@Criteria,@Text,@DateType," +
                     "@StartDate, @EndDate",
                      new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString() ),
                      new SqlParameter("MeetingCode", ""),
                      new SqlParameter("StatusOptions", statusOptionsString),
                      new SqlParameter("ConditionOptions", ""),
                      new SqlParameter("TypeOptions", ""),
                      new SqlParameter("MeetingType", ""),
                      new SqlParameter("Criteria", ""),
                      new SqlParameter("Text", ""),
                      new SqlParameter("DateType", dateType),
                     new SqlParameter("StartDate", startDate),
                     new SqlParameter("EndDate", endDate)
                     ).ToList();
                    var meetingReport = db.Database.SqlQuery<SP_Participant_Report>(
                               "SP_GetParticipantsReport @MeetingCode,@ControlBy,@StatusOptions, @Criteria, @Text, @DateType, @StartDate, @EndDate ,@Type",

                                          new SqlParameter("MeetingCode", ""),
                                          new SqlParameter("ControlBy", (string)Session["user_id"]?.ToString()),

                                           new SqlParameter("StatusOptions", "A"),
                                           new SqlParameter("Criteria", ""),
                                           new SqlParameter("Text", ""),
                                           new SqlParameter("DateType", dateType),
                                           new SqlParameter("StartDate", startDate),
                                           new SqlParameter("EndDate", endDate),
                                           new SqlParameter("Type", "meeting")

                                        ).ToList();

                    var meetingReportBasedDate = db.Database.SqlQuery<SP_Participant_Report>(
                               "SP_GetParticipantsReport @MeetingCode,@ControlBy,@StatusOptions, @Criteria, @Text, @DateType, @StartDate, @EndDate ,@Type,@BasedDate",

                                          new SqlParameter("MeetingCode", ""),
                                          new SqlParameter("ControlBy", (string)Session["user_id"]?.ToString()),

                                           new SqlParameter("StatusOptions", "A"),
                                           new SqlParameter("Criteria", ""),
                                           new SqlParameter("Text", ""),
                                           new SqlParameter("DateType", dateType),
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

                    ViewBag.meetingCodeTop3 = JsonConvert.SerializeObject(meetingTop3.Select(item => item.Meeting).ToArray());
                    ViewBag.meetingTotalTop3 = JsonConvert.SerializeObject(meetingTop3.Select(item => item.Total).ToArray());

                    ViewBag.meetingTop3BasedDate = JsonConvert.SerializeObject(meetingReportBasedDate
                     .GroupBy(item => item.meeting_code)
                     
                     .Select(group => group
                         
                         .Select(item => new List<object> { item.meeting_code + " (" + item.meeting + ")", item.joinedDate, item.total })
                     )
                     .ToList());

                    //Meeting Report Based on Location
                    var locationReport = db.Database.SqlQuery<SP_Meeting_Report>(
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
                      new SqlParameter("DateType", dateType),
                      new SqlParameter("StartDate", startDate),
                      new SqlParameter("EndDate", endDate),
                      new SqlParameter("MeetingStartDate", ""),
                      new SqlParameter("MeetingEndDate", ""),
                      new SqlParameter("Type", "location")).ToList();


                    var locationReportBasedDate = db.Database.SqlQuery<SP_Meeting_Report>(
                      "SP_GetMeetingsReport @StaffNo,@MeetingCode,@StatusOptions,@ConditionOptions,@TypeOptions,@MeetingType,@Criteria,@Text,@DateType," +
                      "@StartDate, @EndDate, @MeetingStartDate, @MeetingEndDate, @Type,@BasedDate",
                      new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                      new SqlParameter("MeetingCode", ""),
                      new SqlParameter("StatusOptions", "A"),
                      new SqlParameter("ConditionOptions", ""),
                      new SqlParameter("TypeOptions", ""),
                      new SqlParameter("MeetingType", ""),
                      new SqlParameter("Criteria", ""),
                      new SqlParameter("Text", ""),
                      new SqlParameter("DateType", dateType),
                      new SqlParameter("StartDate", startDate),
                      new SqlParameter("EndDate", endDate),
                      new SqlParameter("MeetingStartDate", ""),
                      new SqlParameter("MeetingEndDate", ""),
                      new SqlParameter("Type", "location"),
                      new SqlParameter("BasedDate", "Y")).ToList();

                    //GRAF Meeting Location PART

                    var locationTop3 = locationReport
                    .GroupBy(item => item.total)
                    .Select(group => new { Location = string.Join(", ", group.Select(item => item.location)), Total = group.Key })
                    .ToList();

                    ViewBag.locationNameTop3 = JsonConvert.SerializeObject(locationTop3.Select(item => item.Location).ToArray());
                    ViewBag.locationTotalTop3 = JsonConvert.SerializeObject(locationTop3.Select(item => item.Total).ToArray());

                    ViewBag.locationTop3BasedDate = JsonConvert.SerializeObject(locationReportBasedDate
                     .GroupBy(item => item.location)
                     .Select(group => group                         
                         .Select(item => new List<object> { item.location, item.createDate, item.total })
                     )
                     .ToList());



                    //Meeting Report Based on Place
                    var placeReport = db.Database.SqlQuery<SP_Meeting_Report>(
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
                      new SqlParameter("DateType", dateType),
                      new SqlParameter("StartDate", startDate),
                      new SqlParameter("EndDate", endDate),
                      new SqlParameter("MeetingStartDate", ""),
                      new SqlParameter("MeetingEndDate", ""),
                      new SqlParameter("Type", "place")).ToList();


                    var placeReportBasedDate = db.Database.SqlQuery<SP_Meeting_Report>(
                      "SP_GetMeetingsReport @StaffNo,@MeetingCode,@StatusOptions,@ConditionOptions,@TypeOptions,@MeetingType,@Criteria,@Text,@DateType," +
                      "@StartDate, @EndDate, @MeetingStartDate, @MeetingEndDate, @Type,@BasedDate",
                      new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                      new SqlParameter("MeetingCode", ""),
                      new SqlParameter("StatusOptions", "A"),
                      new SqlParameter("ConditionOptions", ""),
                      new SqlParameter("TypeOptions", ""),
                      new SqlParameter("MeetingType", ""),
                      new SqlParameter("Criteria", ""),
                      new SqlParameter("Text", ""),
                      new SqlParameter("DateType", dateType),
                      new SqlParameter("StartDate", startDate),
                      new SqlParameter("EndDate", endDate),
                      new SqlParameter("MeetingStartDate", ""),
                      new SqlParameter("MeetingEndDate", ""),
                      new SqlParameter("Type", "place"),
                      new SqlParameter("BasedDate", "Y")).ToList();

                    //GRAF Meeting Location PART

                    var placeTop3 = placeReport
                    .GroupBy(item => item.total)
                    .Select(group => new { Place = string.Join(", ", group.Select(item => item.place)), Total = group.Key })
                    .ToList();

                    ViewBag.placeNameTop3 = JsonConvert.SerializeObject(placeTop3.Select(item => item.Place).ToArray());
                    ViewBag.placeTotalTop3 = JsonConvert.SerializeObject(placeTop3.Select(item => item.Total).ToArray());

                    ViewBag.placeTop3BasedDate = JsonConvert.SerializeObject(placeReportBasedDate
                     .GroupBy(item => item.place)
                     .Select(group => group
                         .Select(item => new List<object> { item.place, item.createDate, item.total })
                     )
                     .ToList());

                    ////Meeting Report Based on Type
                    var typeReport = db.Database.SqlQuery<SP_Meeting_Report>(
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
                      new SqlParameter("DateType", dateType),
                      new SqlParameter("StartDate", startDate),
                      new SqlParameter("EndDate", endDate),
                      new SqlParameter("MeetingStartDate", ""),
                      new SqlParameter("MeetingEndDate", ""),
                      new SqlParameter("Type", "type")).ToList();

                    var typeReportBasedDate = db.Database.SqlQuery<SP_Meeting_Report>(
                      "SP_GetMeetingsReport @StaffNo,@MeetingCode,@StatusOptions,@ConditionOptions,@TypeOptions,@MeetingType,@Criteria,@Text,@DateType," +
                      "@StartDate, @EndDate, @MeetingStartDate, @MeetingEndDate, @Type,@BasedDate",
                      new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                      new SqlParameter("MeetingCode", ""),
                      new SqlParameter("StatusOptions", "A"),
                      new SqlParameter("ConditionOptions", ""),
                      new SqlParameter("TypeOptions", ""),
                      new SqlParameter("MeetingType", ""),
                      new SqlParameter("Criteria", ""),
                      new SqlParameter("Text", ""),
                      new SqlParameter("DateType", dateType),
                      new SqlParameter("StartDate", startDate),
                      new SqlParameter("EndDate", endDate),
                      new SqlParameter("MeetingStartDate", ""),
                      new SqlParameter("MeetingEndDate", ""),
                      new SqlParameter("Type", "type"),
                      new SqlParameter("BasedDate", "Y")).ToList();

                    //GRAF Meeting Type PART

                    var type = typeReport
                    .GroupBy(item => item.total)
                    .Select(group => new { Type = string.Join(", ", group.Select(item => item.type)), Total = group.Key })
                   
                    .ToList();

                    ViewBag.typeName = JsonConvert.SerializeObject(type.Select(item => item.Type).ToArray());
                    ViewBag.typeTotal = JsonConvert.SerializeObject(type.Select(item => item.Total).ToArray());
                    ViewBag.typeBasedDate = JsonConvert.SerializeObject(typeReportBasedDate
                     .GroupBy(item => item.type)
                    
                     .Select(group => group
                         
                         .Select(item => new List<object> { item.type, item.createDate, item.total })
                     )
                     .ToList());


                    //Meeting Report Based on Mode
                    var modeReport = db.Database.SqlQuery<SP_Meeting_Report>(
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
                      new SqlParameter("DateType", dateType),
                      new SqlParameter("StartDate", startDate),
                      new SqlParameter("EndDate", endDate),
                      new SqlParameter("MeetingStartDate", ""),
                      new SqlParameter("MeetingEndDate", ""),
                      new SqlParameter("Type", "mode")).ToList();

                    var modeReportBasedDate = db.Database.SqlQuery<SP_Meeting_Report>(
                      "SP_GetMeetingsReport @StaffNo,@MeetingCode,@StatusOptions,@ConditionOptions,@TypeOptions,@MeetingType,@Criteria,@Text,@DateType," +
                      "@StartDate, @EndDate, @MeetingStartDate, @MeetingEndDate, @Type,@BasedDate",
                      new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                      new SqlParameter("MeetingCode", ""),
                      new SqlParameter("StatusOptions", "A"),
                      new SqlParameter("ConditionOptions", ""),
                      new SqlParameter("TypeOptions", ""),
                      new SqlParameter("MeetingType", ""),
                      new SqlParameter("Criteria", ""),
                      new SqlParameter("Text", ""),
                      new SqlParameter("DateType", dateType),
                      new SqlParameter("StartDate", startDate),
                      new SqlParameter("EndDate", endDate),
                      new SqlParameter("MeetingStartDate", ""),
                      new SqlParameter("MeetingEndDate", ""),
                      new SqlParameter("Type", "mode"),
                      new SqlParameter("BasedDate", "Y")).ToList();

                    //GRAF Meeting Mode PART

                    var mode = modeReport
                    .GroupBy(item => item.total)
                    .Select(group => new { Mode = string.Join(", ", group.Select(item => item.mode)), Total = group.Key })
                    
                    .ToList();

                    ViewBag.modeName = JsonConvert.SerializeObject(mode.Select(item => item.Mode).ToArray());
                    ViewBag.modeTotal = JsonConvert.SerializeObject(mode.Select(item => item.Total).ToArray());
                    ViewBag.modeBasedDate = JsonConvert.SerializeObject(modeReportBasedDate
                     .GroupBy(item => item.mode)
                      .Select(group => group

                         .Select(item => new List<object> { item.mode, item.createDate, item.total })
                     )
                     
                     .ToList());


                    //Meeting Report Based on Meeting Type
                    var meetingTypeReport = db.Database.SqlQuery<SP_Meeting_Report>(
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
                      new SqlParameter("DateType", dateType),
                      new SqlParameter("StartDate", startDate),
                      new SqlParameter("EndDate", endDate),
                      new SqlParameter("MeetingStartDate", ""),
                      new SqlParameter("MeetingEndDate", ""),
                      new SqlParameter("Type", "meeting_type")).ToList();

                    var meetingTypeReportBasedDate = db.Database.SqlQuery<SP_Meeting_Report>(
                      "SP_GetMeetingsReport @StaffNo,@MeetingCode,@StatusOptions,@ConditionOptions,@TypeOptions,@MeetingType,@Criteria,@Text,@DateType," +
                      "@StartDate, @EndDate, @MeetingStartDate, @MeetingEndDate, @Type,@BasedDate",
                      new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                      new SqlParameter("MeetingCode", ""),
                      new SqlParameter("StatusOptions", "A"),
                      new SqlParameter("ConditionOptions", ""),
                      new SqlParameter("TypeOptions", ""),
                      new SqlParameter("MeetingType", ""),
                      new SqlParameter("Criteria", ""),
                      new SqlParameter("Text", ""),
                      new SqlParameter("DateType", dateType),
                      new SqlParameter("StartDate", startDate),
                      new SqlParameter("EndDate", endDate),
                      new SqlParameter("MeetingStartDate", ""),
                      new SqlParameter("MeetingEndDate", ""),
                      new SqlParameter("Type", "meeting_type"),
                      new SqlParameter("BasedDate", "Y")).ToList();

                    //GRAF Meeting Meeting Type PART

                    var meetingType = meetingTypeReport
                    .GroupBy(item => item.total)
                    .Select(group => new { MeetingType = string.Join(", ", group.Select(item => item.meeting_type)), Total = group.Key })

                    .ToList();

                    ViewBag.meetingTypeName = JsonConvert.SerializeObject(meetingType.Select(item => item.MeetingType).ToArray());
                    ViewBag.meetingTypeTotal = JsonConvert.SerializeObject(meetingType.Select(item => item.Total).ToArray());
                    ViewBag.meetingTypeBasedDate = JsonConvert.SerializeObject(meetingTypeReportBasedDate
                     .GroupBy(item => item.meeting_type)
                      .Select(group => group

                         .Select(item => new List<object> { item.meeting_type, item.createDate, item.total })
                     )

                     .ToList());

                    //Meeting Report Based on Certificate
                    var certificateReport = db.Database.SqlQuery<SP_Meeting_Report>(
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
                      new SqlParameter("DateType", dateType),
                      new SqlParameter("StartDate", startDate),
                      new SqlParameter("EndDate", endDate),
                      new SqlParameter("MeetingStartDate", ""),
                      new SqlParameter("MeetingEndDate", ""),
                      new SqlParameter("Type", "certificate")).ToList();


                    var certificateReportBasedDate = db.Database.SqlQuery<SP_Meeting_Report>(
                      "SP_GetMeetingsReport @StaffNo,@MeetingCode,@StatusOptions,@ConditionOptions,@TypeOptions,@MeetingType,@Criteria,@Text,@DateType," +
                      "@StartDate, @EndDate, @MeetingStartDate, @MeetingEndDate, @Type,@BasedDate",
                      new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                      new SqlParameter("MeetingCode", ""),
                      new SqlParameter("StatusOptions", "A"),
                      new SqlParameter("ConditionOptions", ""),
                      new SqlParameter("TypeOptions", ""),
                      new SqlParameter("MeetingType", ""),
                      new SqlParameter("Criteria", ""),
                      new SqlParameter("Text", ""),
                      new SqlParameter("DateType", dateType),
                      new SqlParameter("StartDate", startDate),
                      new SqlParameter("EndDate", endDate),
                      new SqlParameter("MeetingStartDate", ""),
                      new SqlParameter("MeetingEndDate", ""),
                      new SqlParameter("Type", "certificate"),
                      new SqlParameter("BasedDate", "Y")).ToList();


                    //GRAF Meeting Certificate PART

                    var certificate = certificateReport
                    .GroupBy(item => item.total)
                    .Select(group => new { Certificate = string.Join(", ", group.Select(item => item.certificate)), Total = group.Key })
                    
                    .ToList();

                    ViewBag.certificateName = JsonConvert.SerializeObject(certificate.Select(item => item.Certificate).ToArray());
                    ViewBag.certificateTotal = JsonConvert.SerializeObject(certificate.Select(item => item.Total).ToArray());
                    ViewBag.certificateBasedDate = JsonConvert.SerializeObject(certificateReportBasedDate
                     .GroupBy(item => item.certificate)
                     
                     .Select(group => group
                        
                         .Select(item => new List<object> { item.certificate, item.createDate, item.total })
                     )
                     .ToList());


                    //Meeting Report Based on Condition
                    var conditionReport = db.Database.SqlQuery<SP_Meeting_Report>(
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
                     new SqlParameter("DateType", dateType),
                     new SqlParameter("StartDate", startDate),
                     new SqlParameter("EndDate", endDate),
                     new SqlParameter("MeetingStartDate", ""),
                     new SqlParameter("MeetingEndDate", ""),
                     new SqlParameter("Type", "condition")).ToList();

                    var conditionReportBasedDate = db.Database.SqlQuery<SP_Meeting_Report>(
                      "SP_GetMeetingsReport @StaffNo,@MeetingCode,@StatusOptions,@ConditionOptions,@TypeOptions,@MeetingType,@Criteria,@Text,@DateType," +
                      "@StartDate, @EndDate, @MeetingStartDate, @MeetingEndDate, @Type,@BasedDate",
                      new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                      new SqlParameter("MeetingCode", ""),
                      new SqlParameter("StatusOptions", "A"),
                      new SqlParameter("ConditionOptions", ""),
                      new SqlParameter("TypeOptions", ""),
                      new SqlParameter("MeetingType", ""),
                      new SqlParameter("Criteria", ""),
                      new SqlParameter("Text", ""),
                      new SqlParameter("DateType", dateType),
                      new SqlParameter("StartDate", startDate),
                      new SqlParameter("EndDate", endDate),
                      new SqlParameter("MeetingStartDate", ""),
                      new SqlParameter("MeetingEndDate", ""),
                      new SqlParameter("Type", "condition"),
                      new SqlParameter("BasedDate", "Y")).ToList();

                    //GRAF Meeting Condition PART

                    var condition = conditionReport
                    .GroupBy(item => item.total)
                    .Select(group => new { Condition = string.Join(", ", group.Select(item => item.condition)), Total = group.Key })
                   
                    .ToList();

                    ViewBag.conditionName = JsonConvert.SerializeObject(condition.Select(item => item.Condition).ToArray());
                    ViewBag.conditionTotal = JsonConvert.SerializeObject(condition.Select(item => item.Total).ToArray());
                    ViewBag.conditionBasedDate = JsonConvert.SerializeObject(conditionReportBasedDate
                     .GroupBy(item => item.condition)
                     
                     .Select(group => group
                         
                         .Select(item => new List<object> { item.condition, item.createDate, item.total })
                     )
                     .ToList());
                    var user_id = Session["user_id"]?.ToString();
                    ViewBag.DefineTable = db.sys_setting_define_table
                     .Where(item => item.createBy == user_id && item.module == "meeting")
                     .ToList();
                    var meetingData = new MeetingData();
                    meetingData.LocationReport = locationReport;
                    meetingData.TypeReport = typeReport;
                    meetingData.ModeReport = modeReport;
                    meetingData.MeetingTypeReport = meetingTypeReport;
                    meetingData.PlaceReport = placeReport;
                    meetingData.MeetingReport = meetingReport;
                    meetingData.CertificateReport = certificateReport;
                    meetingData.ConditionReport = conditionReport;
                    meetingData.MeetingList = meetingsList;
                    TempData["page"] = "meetinglist";

                    
                    ViewBag.dataDate = "Month " + month;

                    return View(meetingData);


                    
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


        /*------------------------------------------Add, Edit, View Function START------------------------------------------*/
        // GET: User/MainForm/ :FOR ADD Meeting 
        // GET: User/MainForm/5 :FOR EDIT Meeting 
        public ActionResult MainForm(string id)
        {
            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    var roleID = Session["role_id"]?.ToString();
                    var meetingAccess = db.sys_setting_role_permission.SingleOrDefault(rp => rp.role == roleID && rp.module == "meeting");
                    ViewBag.meetingAccess = meetingAccess;
                    if (meetingAccess?.status != true) //If role permission status is false then logout
                    {
                        return RedirectToAction("Logout", "User");
                    }

                   

               


                    var activeDepartment = db.Database.SqlQuery<SP_GetDepartments_Result>(
                        "SP_GetDepartments @DepartmentCode ,@StatusOptions,@Criteria,@Text,@DateType,@StartDate,@EndDate,@StaffNo",
                        new SqlParameter("DepartmentCode", ""),
                        new SqlParameter("StatusOptions", "A"),
                        new SqlParameter("Criteria", ""),
                        new SqlParameter("Text", ""),
                        new SqlParameter("DateType", ""),
                        new SqlParameter("StartDate", ""),
                        new SqlParameter("EndDate", ""),
                        new SqlParameter("StaffNo", Session["user_id"])).ToList();
                    ViewBag.department_code = new SelectList(activeDepartment, "department_code", "name");
                    var activeFactory = db.Database.SqlQuery<SP_GetFactorys_Result>(
                    "SP_GetFactorys @Factory_id ,@StatusOptions,@Criteria,@Text,@DateType,@StartDate,@EndDate,@StaffNo",
                        new SqlParameter("Factory_id", ""),
                        new SqlParameter("StatusOptions", "A") ,
                        new SqlParameter("Criteria", ""),
                        new SqlParameter("Text", ""),
                        new SqlParameter("DateType", ""),
                        new SqlParameter("StartDate", ""),
                        new SqlParameter("EndDate", ""),
                        new SqlParameter("StaffNo", Session["user_id"])).ToList();
                    ViewBag.factory_id = new SelectList(activeFactory, "factory_id", "name");
                    var activeLocations = db.Database.SqlQuery<SP_GetLocationsWithFactory_Result>(
                                    "SP_GetLocationsWithFactory @LocationCode ,@StatusOptions ,@Criteria,@Text,@DateType,@StartDate,@EndDate,@StaffNo",
                                    new SqlParameter("LocationCode", ""),
                                    new SqlParameter("StatusOptions", "A"),
                                    new SqlParameter("Criteria", ""),
                                    new SqlParameter("Text", ""),
                                    new SqlParameter("DateType", ""),
                                    new SqlParameter("StartDate", ""),
                                    new SqlParameter("EndDate", ""),
                                    new SqlParameter("StaffNo", Session["user_id"])).ToList();

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

                    ViewBag.place_code = new SelectList(
                        activePlace.Select(p => new SelectListItem
                        {
                            Value = p.place_code,
                            Text = p.factory + "-" + p.name
                        }),
                        "Value", "Text"
                    );
                    ViewBag.location_code = new SelectList(
                        activeLocations.Select(l => new SelectListItem
                        {
                            Value = l.location_code,
                            Text = l.factory + "-" + l.name
                        }),
                        "Value", "Text"
                    );
                    
                    
                    if (id == null)
                    {
                        ViewBag.Title = "Add Activity";
                        TempData["page"] = "addmeeting"; //Page will be add function
                        if (meetingAccess?.status != true || (meetingAccess?.addFunction != true)) // If role permission(ADD) status is false then logout
                        {
                            return RedirectToAction("Logout", "User");
                        }
                        return View();
                    }
                    else
                    {
                        if (meetingAccess?.status != true ||
                       (meetingAccess?.editFunction != true && meetingAccess?.viewFunction != true))
                        // If role permission(EDIT||VIEW) status is false then logout
                        {
                            return RedirectToAction("Logout", "User");
                        }
                        else
                        {
                            if (meetingAccess?.editFunction == true) //Edit Function
                            {
                                ViewBag.Title = "Edit Activity";
                            }
                            else //View Function
                            {
                                ViewBag.Title = "View Activity";
                            }

                            TempData["page"] = "meetinglist";

                            //get specific user based on id
                            var meeting = db.Database.SqlQuery<SP_GetMeetings_Result>(
                               "SP_GetMeetings @StaffNo ,@MeetingCode",
                               new SqlParameter("StaffNo", Session["user_id"]),
                               new SqlParameter("MeetingCode", id)).FirstOrDefault();

                            if (meeting == null)
                            {
                                return RedirectToAction("NotFound", "Home");
                            }
                            else
                            {
                                activeLocations = db.Database.SqlQuery<SP_GetLocationsWithFactory_Result>(
                                   "SP_GetLocationsWithFactory @LocationCode ,@StatusOptions ,@Criteria,@Text,@DateType,@StartDate,@EndDate,@StaffNo,@ControlBy,@FactoryID",
                                   new SqlParameter("LocationCode", ""),
                                   new SqlParameter("StatusOptions", "A"),
                                   new SqlParameter("Criteria", ""),
                                   new SqlParameter("Text", ""),
                                   new SqlParameter("DateType", ""),
                                   new SqlParameter("StartDate", ""),
                                   new SqlParameter("EndDate", ""),
                                   new SqlParameter("StaffNo", Session["user_id"]),
                                    new SqlParameter("ControlBy", ""),
                                    new SqlParameter("FactoryID", meeting.factory)).ToList();
                                ViewBag.location_code = new SelectList(
                        activeLocations.Select(l => new SelectListItem
                        {
                            Value = l.location_code,
                            Text = l.factory + "-" + l.name
                        }),
                        "Value", "Text"
                    );


                                 activePlace = db.Database.SqlQuery<SP_GetPlaceWithFactory_Result>(
                                    "SP_GetPlaceWithFactory @PlaceCode ,@StatusOptions ,@Criteria,@Text,@DateType,@StartDate,@EndDate,@FactoryID,@ControlBy",
                                    new SqlParameter("PlaceCode", ""),
                                    new SqlParameter("StatusOptions", "A"),
                                    new SqlParameter("Criteria", ""),
                                    new SqlParameter("Text", ""),
                                    new SqlParameter("DateType", ""),
                                    new SqlParameter("StartDate", ""),
                                    new SqlParameter("EndDate", ""),
                                    new SqlParameter("FactoryID", meeting.factory),
                                    new SqlParameter("ControlBy", Session["user_id"])
                                    ).ToList();

                                ViewBag.place_code = new SelectList(
                                    activePlace.Select(p => new SelectListItem
                                    {
                                        Value = p.place_code,
                                        Text = p.factory + "-" + p.name
                                    }),
                                    "Value", "Text"
                                );
                                if (meeting.type == true)
                                {
                                    string SecretKey = ConfigurationManager.AppSettings["SecretKey"];
                                    long SecretKey_8byte = StringTo8ByteValue(SecretKey);
                                    
                                    long meeting_code_8byte = StringTo8ByteValue(meeting.meeting_code);


                                    meeting.password = Decrypt(meeting.password, SecretKey_8byte.ToString(), meeting_code_8byte.ToString());
                                    
                                   
                                }
                                ViewBag.QRCode = HomeController.GenerateQrCode($"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action("InviteMeeting", new { id = meeting.meeting_code })}");
                                var participantAccess = db.sys_setting_role_permission.SingleOrDefault(rp => rp.role == roleID && rp.module == "participant");
                                ViewBag.meetingGallery = db.Database.SqlQuery<SP_GetMeetingGallery_Result>(
                        "SP_GetMeetingGallery @MeetingCode ",
                        new SqlParameter("MeetingCode", id)).ToList();

                                ViewBag.participantAccess = participantAccess;
                                return View(meeting);
                            }
                        }


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
                ViewBag.ErrorMessage = errormessage;
                return View();

            }
            catch (Exception e)
            {

                ViewBag.ErrorMessage = e.InnerException == null ? e.Message : e.InnerException.Message;
                return View();
            }

        }

        // POST: User/Create
        [HttpPost]
        public JsonResult Create(MeetingDetailsData meetingData)
        {
            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    //validation
                    if (string.IsNullOrEmpty(meetingData.title)|| string.IsNullOrEmpty(meetingData.meeting_type) 
                   || string.IsNullOrEmpty(meetingData.detail)
                   || string.IsNullOrEmpty(meetingData.startDate.ToString()) || string.IsNullOrEmpty(meetingData.endDate.ToString())
                   || string.IsNullOrEmpty(meetingData.department) || string.IsNullOrEmpty(meetingData.location)
                   || string.IsNullOrEmpty(meetingData.factory_id) || string.IsNullOrEmpty(meetingData.certificate.ToString())
                   || string.IsNullOrEmpty(meetingData.type.ToString())
                   || string.IsNullOrEmpty(meetingData.organizer)
                   || string.IsNullOrEmpty(meetingData.limit_status.ToString())
                    || (string.IsNullOrEmpty(meetingData.place) && string.IsNullOrEmpty(meetingData.link))
                    )
                    {
                        return Json(new { success = false, error_message = "Make sure all fields are filled" });
                    }
                    //Meeting List
                    var existingPlace = db.Database.SqlQuery<SP_GetMeetings_Result>(
                      "SP_GetMeetings @StaffNo,@MeetingCode,@StatusOptions,@ConditionOptions,@TypeOptions,@MeetingType,@Criteria,@Text,@DateType," +
                      "@StartDate, @EndDate, @MeetingStartDate, @MeetingEndDate",
                      new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                      new SqlParameter("MeetingCode", ""),
                      new SqlParameter("StatusOptions", "A"),
                      new SqlParameter("ConditionOptions", ""),
                      new SqlParameter("TypeOptions", ""),
                      new SqlParameter("MeetingType", ""),
                      new SqlParameter("Criteria", "place"),
                      new SqlParameter("Text", meetingData.place),
                      new SqlParameter("DateType", ""),
                      new SqlParameter("StartDate", ""),
                      new SqlParameter("EndDate", ""),
                      new SqlParameter("MeetingStartDate", meetingData.startDate),
                      new SqlParameter("MeetingEndDate", meetingData.endDate)).ToList();
                    if (existingPlace.Count >= 1)
                    {
                        return Json(new { success = false, error_message = "The venue is currently unavailable. Please explore other date or venue options." });
                    }
                    var running_Num = db.sys_setting_running_num.SingleOrDefault(u => u.module == "meeting");

                        if (running_Num != null)
                        {
                            meetingData.meeting_code = running_Num.prefix + running_Num.running_num;
                            meetingData.status = "A";
                            meetingData.createDate = DateTime.Now;
                            meetingData.createBy = (string)Session["user_id"];
                            if (meetingData.type == true)
                            {
                            // password encryption
                            string SecretKey = ConfigurationManager.AppSettings["SecretKey"];
                            long SecretKey_8byte = StringTo8ByteValue(SecretKey);
                            long meeting_code_8byte = StringTo8ByteValue(meetingData.meeting_code);

                            
                            meetingData.password = Encrypt(meetingData.password, SecretKey_8byte.ToString(), meeting_code_8byte.ToString());
                            }


                            db.tbl_meeting.Add(new tbl_meeting
                            {
                                meeting_code=meetingData.meeting_code,
                                title = meetingData.title,
                                meeting_type = meetingData.meeting_type,
                                limit_status=meetingData.limit_status,
                                limit=meetingData.limit,
                                detail = meetingData.detail,
                                startDate=meetingData.startDate,
                                endDate=meetingData.endDate,
                                organizer=meetingData.organizer,
                                remark=meetingData.remark,
                                place=meetingData.place,
                                link=meetingData.link,
                                department=meetingData.department,
                                location=meetingData.location,
                                createBy=meetingData.createBy,
                                createDate=meetingData.createDate,
                                type=meetingData.type,
                                password=meetingData.password,
                                status=meetingData.status,
                                certificate=meetingData.certificate,
                                factory_id=meetingData.factory_id,
                                
                            });

                        db.SaveChanges();

                        if (int.TryParse(running_Num.running_num, out int running_num))
                            {
                                running_Num.running_num = $"{running_num + 1:D5}";
                                db.Entry(running_Num).State = EntityState.Modified;
                                db.SaveChanges();
                            }

                        }

                        TempData["SuccessMessage"] = "Added User Successfully.";
                        //after added successfully go to edit user page
                        return Json(new { success = true, message = "Added Successfully.",id=meetingData.meeting_code });
                    
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

        // POST: User/Edit
        [HttpPost]
        public JsonResult Edit(MeetingDetailsData meetingData, string sendEmail)
        {
            try
            {
                using (var db = new AttendNow_DBEntities())
                {

                    var message = "";


                        var existingMeeting = db.tbl_meeting.SingleOrDefault(u => u.meeting_code.ToUpper() == meetingData.meeting_code.ToUpper());
                    //Meeting List
                    if (!string.IsNullOrEmpty(meetingData.place))
                    {
                        var existingPlace = db.Database.SqlQuery<SP_GetMeetings_Result>(
                      "SP_GetMeetings @StaffNo,@MeetingCode,@StatusOptions,@ConditionOptions,@TypeOptions,@MeetingType,@Criteria,@Text,@DateType," +
                      "@StartDate, @EndDate, @MeetingStartDate, @MeetingEndDate",
                      new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                      new SqlParameter("MeetingCode", ""),
                      new SqlParameter("StatusOptions", "A"),
                      new SqlParameter("ConditionOptions", ""),
                      new SqlParameter("TypeOptions", ""),
                      new SqlParameter("MeetingType", ""),
                      new SqlParameter("Criteria", "place"),
                      new SqlParameter("Text", meetingData.place),
                      new SqlParameter("DateType", ""),
                      new SqlParameter("StartDate", ""),
                      new SqlParameter("EndDate", ""),
                      new SqlParameter("MeetingStartDate", meetingData.startDate),
                      new SqlParameter("MeetingEndDate", meetingData.endDate)).ToList();
                        if (existingPlace.Count >= 1 && existingPlace.FirstOrDefault().meeting_code != meetingData.meeting_code)
                        {
                            return Json(new { success = false, error_message = "The venue is currently unavailable. Please explore other date or venue options." });
                        }
                    }
                    


                    if (existingMeeting != null )
                    {

                        
                            if (string.IsNullOrEmpty(meetingData.title) || string.IsNullOrEmpty(meetingData.detail)
                            || string.IsNullOrEmpty(meetingData.startDate.ToString()) || string.IsNullOrEmpty(meetingData.endDate.ToString())
                            || string.IsNullOrEmpty(meetingData.department) || string.IsNullOrEmpty(meetingData.location)
                            || string.IsNullOrEmpty(meetingData.factory_id)
                            || string.IsNullOrEmpty(meetingData.type.ToString()) || string.IsNullOrEmpty(meetingData.certificate.ToString())
                            || string.IsNullOrEmpty(meetingData.organizer)
                            || string.IsNullOrEmpty(meetingData.meeting_type)
                            || string.IsNullOrEmpty(meetingData.limit_status.ToString())
                            || (string.IsNullOrEmpty(meetingData.link) && string.IsNullOrEmpty(meetingData.place)))

                            {
                                return Json(new { success = false, error_message = "Make sure all fields are filled" });
                            }
                            //if the meeting_code is existing in db, alert user

                            if (DateTime.Now > meetingData.startDate && DateTime.Now > meetingData.endDate)
                            {
                                return Json(new { success = false, error_message = "It's not possible to set a date that has already passed !" });
                            }
                            existingMeeting.editDate = DateTime.Now;
                            existingMeeting.editBy = (string)Session["user_id"];
                            existingMeeting.password = meetingData.password;
                            if (meetingData.type == true)
                            {
                                // password encryption
                                string SecretKey = ConfigurationManager.AppSettings["SecretKey"];
                                long SecretKey_8byte = StringTo8ByteValue(SecretKey);
                                long meeting_code_8byte = StringTo8ByteValue(meetingData.meeting_code);


                                existingMeeting.password = Encrypt(meetingData.password, SecretKey_8byte.ToString(), meeting_code_8byte.ToString());

                            }
                            if (meetingData.limit_status == true)
                            {
                                existingMeeting.limit = meetingData.limit;
                            }
                            //update new data
                            existingMeeting.limit_status = meetingData.limit_status;
                            existingMeeting.title = meetingData.title;
                            existingMeeting.detail = meetingData.detail;
                            existingMeeting.organizer = meetingData.organizer;
                            existingMeeting.place = meetingData.place;
                            existingMeeting.link = meetingData.link;
                            existingMeeting.department = meetingData.department;
                            existingMeeting.location = meetingData.location;
                            existingMeeting.type = meetingData.type;
                            existingMeeting.startDate = meetingData.startDate;
                            existingMeeting.endDate = meetingData.endDate;
                            existingMeeting.remark = meetingData.remark;
                            existingMeeting.meeting_type = meetingData.meeting_type;
                            existingMeeting.factory_id = meetingData.factory_id;
                            existingMeeting.certificate = meetingData.certificate;
                            db.Entry(existingMeeting).State = EntityState.Modified;
                            message = "Updated Successfully.";
                        
                        db.SaveChanges();
                        if (sendEmail == "Y")
                        {
                            var participantList = db.Database.SqlQuery<SP_GetEmailNotificationData_Result>(
                               "SP_GetEmailNotificationData " + meetingData.meeting_code).ToList();
                            if (participantList.Count > 0)
                            {
                                HomeController homeController = new HomeController();
                                homeController.NoticeEmail("There are adjustments to the " + (meetingData.meeting_type == "M" ? "Meeting" : (meetingData.meeting_type == "E" ? "Event" : "Training")),
                                    "You may check to see if you can still join", "C", meetingData.meeting_code);
                            }
                        }
                        
                        TempData["SuccessMessage"] = message;
                        return Json(new { success = true, message = message, id = meetingData.meeting_code });
                    }
                    else
                    {
                        return Json(new { success = false, error_message = "Not found !" });
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
        /*------------------------------------------Add, Edit, View Function END------------------------------------------*/



        // POST: User/Edit
        [HttpPost]
        public JsonResult UploadGallery(MeetingDetailsData meetingData, HttpPostedFileBase[] Attachment,string type)
        {
            try
            {
                using (var db = new AttendNow_DBEntities())
                {

                    var message = "";
                    if (Attachment != null && Attachment.Length > 0)
                    {
                        // Process each uploaded image
                        foreach (var file in Attachment)
                        {
                            if (file != null && file.ContentLength > 0)
                            {
                                // Generate a unique filename for the image
                                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                                var filePath = Path.Combine(Server.MapPath("~/Images/Gallery/"), fileName);

                                // Save the image to the server
                                file.SaveAs(filePath);
                                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

                                // Extract the file extension
                                string fileExtension = Path.GetExtension(fileName).TrimStart('.');
                                // Save the image path in the database
                                var image = new tbl_meeting_gallery
                                {
                                    path = "/Images/Gallery/",
                                    filename = fileNameWithoutExtension,
                                    format = fileExtension,
                                    meeting_code = meetingData.meeting_code,
                                    status = "A",
                                    createBy = (string)Session["user_id"],
                                    createDate = DateTime.Now
                                };

                                db.tbl_meeting_gallery.Add(image);
                            }
                        }

                        message = "Uploaded Gallery Successfully.";
                    }


                    db.SaveChanges();
                    if (type != "Index")
                    {
                        TempData["SuccessMessage"] = message;
                    }
                   
                    return Json(new { success = true, successMessage = message, id = meetingData.meeting_code });

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
        /*------------------------------------------Add, Edit, View Function END------------------------------------------*/


        [HttpPost]
        public JsonResult GetLocation(string factory_id) //delete access location function
        {
            try
            {
                
                using (var db = new AttendNow_DBEntities())
                {
                    // Call the stored procedure using Entity Framework
                    var activeLocation = db.Database.SqlQuery<SP_GetLocationsWithFactory_Result>(
                                    "SP_GetLocationsWithFactory @LocationCode ,@StatusOptions ,@Criteria,@Text,@DateType,@StartDate,@EndDate,@StaffNo,@ControlBy,@FactoryID",
                                    new SqlParameter("LocationCode", ""),
                                    new SqlParameter("StatusOptions", "A"),
                                    new SqlParameter("Criteria", ""),
                                    new SqlParameter("Text", ""),
                                    new SqlParameter("DateType", ""),
                                    new SqlParameter("StartDate", ""),
                                    new SqlParameter("EndDate", ""),
                                    new SqlParameter("StaffNo", Session["user_id"]),
                                     new SqlParameter("ControlBy", ""),
                                     new SqlParameter("FactoryID", factory_id)).ToList();
                    var activePlace = db.Database.SqlQuery<SP_GetPlaceWithFactory_Result>(
                                    "SP_GetPlaceWithFactory @PlaceCode ,@StatusOptions ,@Criteria,@Text,@DateType,@StartDate,@EndDate,@FactoryID,@ControlBy",
                                    new SqlParameter("PlaceCode", ""),
                                    new SqlParameter("StatusOptions", "A"),
                                    new SqlParameter("Criteria", ""),
                                    new SqlParameter("Text", ""),
                                    new SqlParameter("DateType", ""),
                                    new SqlParameter("StartDate", ""),
                                    new SqlParameter("EndDate", ""),
                                    new SqlParameter("FactoryID", factory_id),
                                    new SqlParameter("ControlBy", Session["user_id"])
                                    ).ToList();

                  
                    return Json(new
                    {
                        success = true,
                        message = "Filtering Location Successfully",
                        activeLocation = activeLocation,
                        activePlace=activePlace

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

        /*------------------------------------------DeleteImage Function START------------------------------------------*/
        public ActionResult DeleteImage(string fileName,string format, string meeting_code)
        {
            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    var filePath = Path.Combine(Server.MapPath("~/Images/Gallery/"), fileName + "." + format);

                    // Check if the file exists
                    if (System.IO.File.Exists(filePath))
                    {
                        // Delete the file
                        System.IO.File.Delete(filePath);

                        var existingMeetingGallery = db.tbl_meeting_gallery.SingleOrDefault(u => u.meeting_code.ToUpper() == meeting_code.ToUpper() && u.filename == fileName && u.format == format);

                        if (existingMeetingGallery != null)
                        {
                            existingMeetingGallery.status= "V";
                            existingMeetingGallery.editDate = DateTime.Now;
                            existingMeetingGallery.editBy = (string)Session["user_id"];

                            db.Entry(existingMeetingGallery).State = EntityState.Modified;
                            db.SaveChanges();
                            return Json(new { success = true, message = "Image Deleted Successfully" });

                        }
                        else
                        {
                            return Json(new { success = false, message = "Image not found." });
                        }
                        
                    }
                    return Json(new { success = false, message = "Image not found." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error_message = "Error deleting image: " + ex.Message });
            }
        }
        /*------------------------------------------DeleteImage Function END------------------------------------------*/
        /*------------------------------------------UpdateStatus Function START------------------------------------------*/
        public JsonResult UpdateStatus(String id, string newStatus)
        {

            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    // Find the existing user in the database by user_id
                   
                    var existingmeeting = db.tbl_meeting.SingleOrDefault(u => u.meeting_code.ToUpper() == id.ToUpper());
                    if (existingmeeting != null)
                    {
                        // Update the status property based on the newStatus parameter
                        existingmeeting.status = newStatus;
                        existingmeeting.editBy = (string)Session["user_id"];
                        existingmeeting.editDate = DateTime.Now;
                        var existingControlUser = db.Database.SqlQuery<SP_GetUserProfiles_Result>("SP_GetUserProfiles @StaffNo",
                                                  new SqlParameter("StaffNo", (string)Session["user_id"])
                                              ).FirstOrDefault();

                        var userEditBy = existingControlUser.EditByName + " (" + existingControlUser.editBy.ToUpper() + ")";
                        var editDate = existingmeeting.editDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                        db.Entry(existingmeeting).State = EntityState.Modified;
                        db.SaveChanges();


                        return Json(new { success = true, message = "Updated Successfully.",
                            userEditBy = userEditBy,
                            userEditDate = editDate
                        });
                    }
                    return Json(new { success = false, error_message = "Not found" });
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
        /*------------------------------------------UpdateStatus Function END------------------------------------------*/


        [HttpPost]
        public ActionResult UpdateDate(string newStartDate, string newEndDate,string meeting_code, DateTime? selectedMonth,string sendEmail)
        {
            
            try {
                
                using (var db = new AttendNow_DBEntities())
                {
                    
                    var meeting = db.tbl_meeting.SingleOrDefault(u => u.meeting_code.ToUpper() == meeting_code.ToUpper());
                   
                    DateTime newStartDateTime = DateTime.ParseExact(newStartDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    DateTime newEndDateTime = DateTime.ParseExact(newEndDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                   

                    if (meeting != null)
                        

                    {
                        DateTime updatedStartDateTime;
                        DateTime updatedEndDateTime;
                      
                             updatedStartDateTime = new DateTime(
                            newStartDateTime.Year,
                            newStartDateTime.Month,
                            newStartDateTime.Day,
                            newStartDateTime.Hour,
                            newStartDateTime.Minute,
                            newStartDateTime.Second
                            );

                             updatedEndDateTime = new DateTime(
                            newEndDateTime.Year,
                            newEndDateTime.Month,
                            newEndDateTime.Day,
                            newEndDateTime.Hour,
                            newEndDateTime.Minute,
                            newEndDateTime.Second
                            );
                     
                        var startDate = "";
                        var endDate = "";
                        DateTime serverStartDateTime;
                        DateTime serverEndDateTime;
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
                        //Meeting List
                        var existingPlace = db.Database.SqlQuery<SP_GetMeetings_Result>(
                          "SP_GetMeetings @StaffNo,@MeetingCode,@StatusOptions,@ConditionOptions,@TypeOptions,@MeetingType,@Criteria,@Text,@DateType," +
                          "@StartDate, @EndDate, @MeetingStartDate, @MeetingEndDate",
                          new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                          new SqlParameter("MeetingCode", ""),
                          new SqlParameter("StatusOptions", "A"),
                          new SqlParameter("ConditionOptions", ""),
                          new SqlParameter("TypeOptions", ""),
                          new SqlParameter("MeetingType", ""),
                          new SqlParameter("Criteria", "place"),
                          new SqlParameter("Text", meeting.place),
                          new SqlParameter("DateType", ""),
                          new SqlParameter("StartDate", ""),
                          new SqlParameter("EndDate", ""),
                          new SqlParameter("MeetingStartDate", updatedStartDateTime),
                          new SqlParameter("MeetingEndDate", updatedEndDateTime)).ToList();
                        if (existingPlace.Count >= 1 && existingPlace.FirstOrDefault().meeting_code != meeting_code)
                        {
                            return Json(new { success = false, error_message = "The venue is currently unavailable. Please explore other date or venue options." });
                        }
                        if (DateTime.Now<updatedStartDateTime && DateTime.Now<updatedEndDateTime  )
                        {
                            meeting.startDate = updatedStartDateTime;
                            meeting.endDate = updatedEndDateTime;

                            db.Entry(meeting).State = EntityState.Modified;
                            db.SaveChanges();
                            var meetingsList = JsonConvert.SerializeObject(db.Database.SqlQuery<SP_GetMeetingsForCalander_Result>(
                                 "SP_GetMeetingsForCalander @StaffNo,@Type, @Place,@StartDate,@EndDate",
                                 new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                                 new SqlParameter("Type", ""),
                                 new SqlParameter("Place", ""),
                                 new SqlParameter("StartDate", startDate),
                                   new SqlParameter("EndDate", endDate)
                                ).ToList());

                            var placeList = JsonConvert.SerializeObject(db.Database.SqlQuery<SP_GetMeetingsForCalander_Result>(
                                 "SP_GetMeetingsForCalander @StaffNo, @Type, @Place,@StartDate,@EndDate",
                                 new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                                 new SqlParameter("Type", "P"),
                                 new SqlParameter("Place", ""),
                                 new SqlParameter("StartDate", startDate),
                                 new SqlParameter("EndDate", endDate)
                                ).ToList());
                            if (sendEmail == "Y")
                            {
                                var participantList = db.Database.SqlQuery<SP_GetEmailNotificationData_Result>(
                              "SP_GetEmailNotificationData " + meeting.meeting_code).ToList();
                                if (participantList.Count > 0)
                                {
                                    HomeController homeController = new HomeController();
                                    homeController.NoticeEmail("There are adjustments to the " + (meeting.meeting_type == "M" ? "Meeting" : (meeting.meeting_type == "E" ? "Event" : "Training")),
                                        "You may check to see if you can still join", "C", meeting.meeting_code);
                                }
                            }
                           
                           
                            return Json(new { success = true, message = "Date updated successfully", meetingsList= meetingsList, placeList= placeList });
                        }
                        else
                        {
                            
                            return Json(new { success = false, error_message = "It's not possible to set a date that has already passed !" });
                        }
                       
                     
                        
                    }
                    return Json(new { success = false, message = "Meeting Not Found!" });
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



        /*------------------------------------------Filter or Search Function START------------------------------------------*/
        [HttpPost]
        public ActionResult Search(List<string> statusOptions, List<string> conditionOptions, List<string> typeOptions, string criteria, 
            string text, string dateType, string startDate, string endDate,string meetingStartDate, string meetingEndDate,string meeting_type)
        {
            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    string statusOptionsString;
                    string typeOptionsString;
                    string conditionOptionsString;
                    DateTime ?serverStartDateTime = null;
                    DateTime ?serverEndDateTime = null; 
                    if (dateType == "null")
                    {
                        dateType = "createDate";
                    }
                    TimeZoneInfo serverTimeZone = null;
                    TimeZoneInfo convertedTimeZone = null;
                    var roleID = Session["role_id"]?.ToString();
                    var meetingAccess = db.sys_setting_role_permission.SingleOrDefault(rp => rp.role == roleID && rp.module == "meeting");
                    ViewBag.meetingAccess = meetingAccess;
                    if (statusOptions == null)
                    {
                        statusOptionsString = "A,V";
                        if (meetingAccess.deleteFunction != true)
                        {
                            statusOptionsString = "A";
                        }
                    }
                    else
                    {
                        statusOptionsString = string.Join(",", statusOptions);
                    }

                    if (typeOptions == null)
                    {
                        typeOptionsString = "";
                    }
                    else
                    {
                        typeOptionsString = string.Join(",", typeOptions);
                    }

                    if (conditionOptions == null)
                    {
                        conditionOptionsString = "";
                    }
                    else
                    {
                        conditionOptionsString = string.Join(",", conditionOptions);
                    }
                    if (startDate == "" && endDate == "" && meetingStartDate=="" &&meetingEndDate=="")
                    {
                        serverStartDateTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1); // Set to the first day of the current month
                        serverEndDateTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month))
                            .AddHours(23).AddMinutes(59).AddSeconds(59); /// Set to the end of the day

                    }else
                    if (startDate != "" || endDate != "")
                    {
                        serverStartDateTime = DateTime.ParseExact(startDate, "yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);
                        serverEndDateTime = DateTime.ParseExact(endDate, "yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture).Date.AddHours(23).AddMinutes(59);

                    }
                    else
                    {
                        serverStartDateTime = null;
                        serverEndDateTime = null;
                        dateType = "";
                    }
                    if (serverStartDateTime.HasValue && serverEndDateTime.HasValue)
                    {
                        if ((string)Session["timezone"] == "V")
                        {

                            // Assume serverStartDateTime is in Thailand timezone
                            convertedTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                            DateTime utcStartDate = TimeZoneInfo.ConvertTimeToUtc(serverStartDateTime.Value, convertedTimeZone);
                            DateTime utcEndDate = TimeZoneInfo.ConvertTimeToUtc(serverEndDateTime.Value, convertedTimeZone);

                            // Convert to Server timezone
                            serverTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                            startDate = TimeZoneInfo.ConvertTime(utcStartDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");
                            endDate = TimeZoneInfo.ConvertTime(utcEndDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");

                        }
                        else if ((string)Session["timezone"] == "J")
                        {

                            // Assume serverStartDateTime is in Jordan timezone
                            convertedTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Jordan Standard Time");
                            DateTime utcStartDate = TimeZoneInfo.ConvertTimeToUtc(serverStartDateTime.Value, convertedTimeZone);
                            DateTime utcEndDate = TimeZoneInfo.ConvertTimeToUtc(serverEndDateTime.Value, convertedTimeZone);

                            // Convert to Server timezone
                            serverTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                            startDate = TimeZoneInfo.ConvertTime(utcStartDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");
                            endDate = TimeZoneInfo.ConvertTime(utcEndDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");

                        }
                        else if ((string)Session["timezone"] == "M")
                        {
                            // Assume serverStartDateTime is in Singapore timezone
                            convertedTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                            DateTime utcStartDate = TimeZoneInfo.ConvertTimeToUtc(serverStartDateTime.Value, convertedTimeZone);
                            DateTime utcEndDate = TimeZoneInfo.ConvertTimeToUtc(serverEndDateTime.Value, convertedTimeZone);

                            // Convert to Server timezone
                            serverTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                            startDate = TimeZoneInfo.ConvertTime(utcStartDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");
                            endDate = TimeZoneInfo.ConvertTime(utcEndDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");
                        }

                    }

                    if (meetingStartDate != "" || meetingEndDate != "")
                    {
                        DateTime serverMeetingStartDateTime = DateTime.ParseExact(meetingStartDate, "yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);
                        DateTime serverMeetingEndDateTime = DateTime.ParseExact(meetingEndDate, "yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture).Date.AddHours(23).AddMinutes(59);

                        if ((string)Session["timezone"] == "V")
                        {


                            convertedTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                            DateTime utcMeetingStartDate = TimeZoneInfo.ConvertTimeToUtc(serverMeetingStartDateTime, convertedTimeZone);
                            DateTime utcMeetingEndDate = TimeZoneInfo.ConvertTimeToUtc(serverMeetingEndDateTime, convertedTimeZone);

                            // Convert to Server timezone
                            serverTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                            meetingStartDate = TimeZoneInfo.ConvertTime(utcMeetingStartDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");
                            meetingEndDate = TimeZoneInfo.ConvertTime(utcMeetingEndDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");

                        }
                        else if ((string)Session["timezone"] == "J")
                        {

                            // Assume serverStartDateTime is in Jordan timezone
                            convertedTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Jordan Standard Time");
                            DateTime utcMeetingStartDate = TimeZoneInfo.ConvertTimeToUtc(serverMeetingStartDateTime, convertedTimeZone);
                            DateTime utcMeetingEndDate = TimeZoneInfo.ConvertTimeToUtc(serverMeetingEndDateTime, convertedTimeZone);

                            // Convert to Server timezone
                            serverTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                            meetingStartDate = TimeZoneInfo.ConvertTime(utcMeetingStartDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");
                            meetingEndDate = TimeZoneInfo.ConvertTime(utcMeetingEndDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");

                        }
                        else if ((string)Session["timezone"] == "M")
                        {
                            // Assume serverStartDateTime is in Singapore timezone
                            convertedTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                            DateTime utcMeetingStartDate = TimeZoneInfo.ConvertTimeToUtc(serverMeetingStartDateTime, convertedTimeZone);
                            DateTime utcMeetingEndDate = TimeZoneInfo.ConvertTimeToUtc(serverMeetingEndDateTime, convertedTimeZone);

                            // Convert to Server timezone
                            serverTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                            meetingStartDate = TimeZoneInfo.ConvertTime(utcMeetingStartDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");
                            meetingEndDate = TimeZoneInfo.ConvertTime(utcMeetingEndDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");
                        }
                    }

                    



                    //Meeting List
                    var meetingsList = db.Database.SqlQuery<SP_GetMeetings_Result>(
                      "SP_GetMeetings @StaffNo,@MeetingCode,@StatusOptions,@ConditionOptions,@TypeOptions,@MeetingType,@Criteria,@Text,@DateType," +
                      "@StartDate, @EndDate, @MeetingStartDate, @MeetingEndDate",
                      new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                      new SqlParameter("MeetingCode", ""),
                      new SqlParameter("StatusOptions", statusOptionsString),
                      new SqlParameter("ConditionOptions", conditionOptionsString),
                      new SqlParameter("TypeOptions", typeOptionsString),
                      new SqlParameter("MeetingType", meeting_type),
                      new SqlParameter("Criteria", criteria),
                      new SqlParameter("Text", text),
                      new SqlParameter("DateType", dateType),
                      new SqlParameter("StartDate", startDate),
                      new SqlParameter("EndDate", endDate),
                      new SqlParameter("MeetingStartDate", meetingStartDate),
                      new SqlParameter("MeetingEndDate", meetingEndDate)).ToList();

                    var meetingReport = db.Database.SqlQuery<SP_Participant_Report>(
                               "SP_GetParticipantsReport @MeetingCode,@ControlBy,@StatusOptions, @Criteria, @Text, @DateType, @StartDate, @EndDate ,@Type" +
                               ",@BasedDate,@MeetingStartDate,@MeetingEndDate,@ConditionOptions,@TypeOptions,@MeetingType",

                                          new SqlParameter("MeetingCode", ""),
                                          new SqlParameter("ControlBy", (string)Session["user_id"]?.ToString()),

                                           new SqlParameter("StatusOptions", "A"),
                                           new SqlParameter("Criteria", criteria),
                                           new SqlParameter("Text", text),
                                           new SqlParameter("DateType", dateType),
                                           new SqlParameter("StartDate", startDate),
                                           new SqlParameter("EndDate", endDate),
                                           new SqlParameter("Type", "meeting"),
                                            new SqlParameter("BasedDate", ""),
                                           new SqlParameter("MeetingStartDate", meetingStartDate),
                                           new SqlParameter("MeetingEndDate", meetingEndDate),
                                           new SqlParameter("ConditionOptions", conditionOptionsString),
                                           new SqlParameter("TypeOptions", typeOptionsString),
                                           new SqlParameter("MeetingType", meeting_type)

                                        ).ToList();


                    var meetingReportBasedDate = db.Database.SqlQuery<SP_Participant_Report>(
                              "SP_GetParticipantsReport @MeetingCode,@ControlBy,@StatusOptions, @Criteria, @Text, @DateType, @StartDate, @EndDate " +
                              ",@Type,@BasedDate,@MeetingStartDate,@MeetingEndDate,@ConditionOptions,@TypeOptions,@MeetingType",

                                         new SqlParameter("MeetingCode", ""),
                                         new SqlParameter("ControlBy", (string)Session["user_id"]?.ToString()),

                                          new SqlParameter("StatusOptions", "A"),
                                          new SqlParameter("Criteria", criteria),
                                          new SqlParameter("Text", text),
                                          new SqlParameter("DateType", dateType),
                                          new SqlParameter("StartDate", startDate),
                                          new SqlParameter("EndDate", endDate),
                                          new SqlParameter("Type", "meeting"),
                                           new SqlParameter("BasedDate", "Y"),
                                             new SqlParameter("MeetingStartDate", meetingStartDate),
                                           new SqlParameter("MeetingEndDate", meetingEndDate),
                                             new SqlParameter("ConditionOptions", conditionOptionsString),
                                           new SqlParameter("TypeOptions", typeOptionsString),
                                           new SqlParameter("MeetingType", meeting_type)

                                       ).ToList();


                    //GRAF MEETING PART

                    var meetingTop3 = meetingReport
                    .GroupBy(item => item.total)
                    .Select(group => new { Meeting = string.Join(", ", group.Select(item => item.meeting_code + " (" + item.meeting + ")")), Total = group.Key })
                    
                    .ToList();

                    var meetingCodeTop3 = JsonConvert.SerializeObject(meetingTop3.Select(item => item.Meeting).ToArray());
                    var meetingTotalTop3 = JsonConvert.SerializeObject(meetingTop3.Select(item => item.Total).ToArray());
                    var meetingTop3BasedDate = JsonConvert.SerializeObject(meetingReportBasedDate
                      .GroupBy(item => item.meeting_code)

                      .Select(group => group

                          .Select(item => new List<object> { item.meeting_code + " (" + item.meeting + ")",item.joinedDate, item.total })
                      )
                      .ToList());



                    //Meeting Report Based on Location
                    var locationReport = db.Database.SqlQuery<SP_Meeting_Report>(
                      "SP_GetMeetingsReport @StaffNo,@MeetingCode,@StatusOptions,@ConditionOptions,@TypeOptions,@MeetingType,@Criteria,@Text,@DateType," +
                      "@StartDate, @EndDate, @MeetingStartDate, @MeetingEndDate, @Type",
                      new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                      new SqlParameter("MeetingCode", ""),
                      new SqlParameter("StatusOptions", "A"),
                      new SqlParameter("ConditionOptions", conditionOptionsString),
                      new SqlParameter("TypeOptions", typeOptionsString),
                      new SqlParameter("MeetingType", meeting_type),
                      new SqlParameter("Criteria",criteria),
                      new SqlParameter("Text", text),
                      new SqlParameter("DateType", dateType),
                      new SqlParameter("StartDate", startDate),
                      new SqlParameter("EndDate", endDate),
                      new SqlParameter("MeetingStartDate", meetingStartDate),
                      new SqlParameter("MeetingEndDate",meetingEndDate),
                      new SqlParameter("Type", "location")).ToList();


                    var locationReportBasedDate = db.Database.SqlQuery<SP_Meeting_Report>(
                      "SP_GetMeetingsReport @StaffNo,@MeetingCode,@StatusOptions,@ConditionOptions,@TypeOptions,@MeetingType,@Criteria,@Text,@DateType," +
                      "@StartDate, @EndDate, @MeetingStartDate, @MeetingEndDate, @Type,@BasedDate",
                      new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                      new SqlParameter("MeetingCode", ""),
                        new SqlParameter("StatusOptions", "A"),
                      new SqlParameter("ConditionOptions", conditionOptionsString),
                      new SqlParameter("TypeOptions", typeOptionsString),
                      new SqlParameter("MeetingType", meeting_type),
                      new SqlParameter("Criteria", criteria),
                      new SqlParameter("Text", text),
                      new SqlParameter("DateType", dateType),
                      new SqlParameter("StartDate", startDate),
                      new SqlParameter("EndDate", endDate),
                      new SqlParameter("MeetingStartDate", meetingStartDate),
                      new SqlParameter("MeetingEndDate", meetingEndDate),
                      new SqlParameter("Type", "location"),
                      new SqlParameter("BasedDate", "Y")).ToList();

                    //GRAF Meeting Location PART

                    var locationTop3 = locationReport
                    .GroupBy(item => item.total)
                    .Select(group => new { Location = string.Join(", ", group.Select(item => item.location)), Total = group.Key })
                  
                    .ToList();

                    var meetingLocationNameTop3 = JsonConvert.SerializeObject(locationTop3.Select(item => item.Location).ToArray());
                    var meetingLocationTotalTop3 = JsonConvert.SerializeObject(locationTop3.Select(item => item.Total).ToArray());
                    var meetingLocationTop3BasedDate = JsonConvert.SerializeObject(locationReportBasedDate
                     .GroupBy(item => item.location)
                    
                     .Select(group => group
                         
                         .Select(item => new List<object> { item.location, item.createDate, item.total })
                     )
                     .ToList());


                    //Meeting Report Based on Place
                    var placeReport = db.Database.SqlQuery<SP_Meeting_Report>(
                      "SP_GetMeetingsReport @StaffNo,@MeetingCode,@StatusOptions,@ConditionOptions,@TypeOptions,@MeetingType,@Criteria,@Text,@DateType," +
                      "@StartDate, @EndDate, @MeetingStartDate, @MeetingEndDate, @Type",
                      new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                      new SqlParameter("MeetingCode", ""),
                      new SqlParameter("StatusOptions", "A"),
                      new SqlParameter("ConditionOptions", conditionOptionsString),
                      new SqlParameter("TypeOptions", typeOptionsString),
                      new SqlParameter("MeetingType", meeting_type),
                      new SqlParameter("Criteria", criteria),
                      new SqlParameter("Text", text),
                      new SqlParameter("DateType", dateType),
                      new SqlParameter("StartDate", startDate),
                      new SqlParameter("EndDate", endDate),
                      new SqlParameter("MeetingStartDate", meetingStartDate),
                      new SqlParameter("MeetingEndDate", meetingEndDate),
                      new SqlParameter("Type", "place")).ToList();


                    var placeReportBasedDate = db.Database.SqlQuery<SP_Meeting_Report>(
                      "SP_GetMeetingsReport @StaffNo,@MeetingCode,@StatusOptions,@ConditionOptions,@TypeOptions,@MeetingType,@Criteria,@Text,@DateType," +
                      "@StartDate, @EndDate, @MeetingStartDate, @MeetingEndDate, @Type,@BasedDate",
                      new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                      new SqlParameter("MeetingCode", ""),
                      new SqlParameter("StatusOptions", "A"),
                      new SqlParameter("ConditionOptions", conditionOptionsString),
                      new SqlParameter("TypeOptions", typeOptionsString),
                      new SqlParameter("MeetingType", meeting_type),
                      new SqlParameter("Criteria", criteria),
                      new SqlParameter("Text", text),
                      new SqlParameter("DateType", dateType),
                      new SqlParameter("StartDate", startDate),
                      new SqlParameter("EndDate", endDate),
                      new SqlParameter("MeetingStartDate", meetingStartDate),
                      new SqlParameter("MeetingEndDate", meetingEndDate),
                      new SqlParameter("Type", "place"),
                      new SqlParameter("BasedDate", "Y")).ToList();

                    //GRAF Meeting Place PART

                    var placeTop3 = placeReport
                    .GroupBy(item => item.total)
                    .Select(group => new { Place = string.Join(", ", group.Select(item => item.place)), Total = group.Key })

                    .ToList();

                    var meetingPlaceNameTop3 = JsonConvert.SerializeObject(placeTop3.Select(item => item.Place).ToArray());
                    var meetingPlaceTotalTop3 = JsonConvert.SerializeObject(placeTop3.Select(item => item.Total).ToArray());
                    var meetingPlaceTop3BasedDate = JsonConvert.SerializeObject(placeReportBasedDate
                     .GroupBy(item => item.place)

                     .Select(group => group

                         .Select(item => new List<object> { item.place, item.createDate, item.total })
                     )
                     .ToList());

                    ////Meeting Report Based on Type
                    var typeReport = db.Database.SqlQuery<SP_Meeting_Report>(
                      "SP_GetMeetingsReport @StaffNo,@MeetingCode,@StatusOptions,@ConditionOptions,@TypeOptions,@MeetingType,@Criteria,@Text,@DateType," +
                      "@StartDate, @EndDate, @MeetingStartDate, @MeetingEndDate, @Type",
                      new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                      new SqlParameter("MeetingCode", ""),
                        new SqlParameter("StatusOptions", "A"),
                      new SqlParameter("ConditionOptions", conditionOptionsString),
                      new SqlParameter("TypeOptions", typeOptionsString),
                      new SqlParameter("MeetingType", meeting_type),
                      new SqlParameter("Criteria", criteria),
                      new SqlParameter("Text", text),
                      new SqlParameter("DateType", dateType),
                      new SqlParameter("StartDate", startDate),
                      new SqlParameter("EndDate", endDate),
                      new SqlParameter("MeetingStartDate", meetingStartDate),
                      new SqlParameter("MeetingEndDate", meetingEndDate),
                      new SqlParameter("Type", "type")).ToList();

                    var typeReportBasedDate = db.Database.SqlQuery<SP_Meeting_Report>(
                      "SP_GetMeetingsReport @StaffNo,@MeetingCode,@StatusOptions,@ConditionOptions,@TypeOptions,@MeetingType,@Criteria,@Text,@DateType," +
                      "@StartDate, @EndDate, @MeetingStartDate, @MeetingEndDate, @Type,@BasedDate",
                      new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                      new SqlParameter("MeetingCode", ""),
                       new SqlParameter("StatusOptions", "A"),
                      new SqlParameter("ConditionOptions", conditionOptionsString),
                      new SqlParameter("TypeOptions", typeOptionsString),
                      new SqlParameter("MeetingType", meeting_type),
                      new SqlParameter("Criteria", criteria),
                      new SqlParameter("Text", text),
                      new SqlParameter("DateType", dateType),
                      new SqlParameter("StartDate", startDate),
                      new SqlParameter("EndDate", endDate),
                      new SqlParameter("MeetingStartDate", meetingStartDate),
                      new SqlParameter("MeetingEndDate", meetingEndDate),
                      new SqlParameter("Type", "type"),
                      new SqlParameter("BasedDate", "Y")).ToList();

                    //GRAF Meeting Type PART

                    var type = typeReport
                    .GroupBy(item => item.total)
                    .Select(group => new { Type = string.Join(", ", group.Select(item => item.type)), Total = group.Key })
                    
                    .ToList();

                    var typeName = JsonConvert.SerializeObject(type.Select(item => item.Type).ToArray());
                    var typeTotal = JsonConvert.SerializeObject(type.Select(item => item.Total).ToArray());
                    var typeBasedDate = JsonConvert.SerializeObject(typeReportBasedDate
                     .GroupBy(item => item.type)
                     
                     .Select(group => group
                        
                         .Select(item => new List<object> { item.type, item.createDate, item.total })
                     )
                     .ToList());


                    //Meeting Report Based on Mode
                    var modeReport = db.Database.SqlQuery<SP_Meeting_Report>(
                      "SP_GetMeetingsReport @StaffNo,@MeetingCode,@StatusOptions,@ConditionOptions,@TypeOptions,@MeetingType,@Criteria,@Text,@DateType," +
                      "@StartDate, @EndDate, @MeetingStartDate, @MeetingEndDate, @Type",
                      new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                      new SqlParameter("MeetingCode", ""),
                       new SqlParameter("StatusOptions", "A"),
                      new SqlParameter("ConditionOptions", conditionOptionsString),
                      new SqlParameter("TypeOptions", typeOptionsString),
                      new SqlParameter("MeetingType", meeting_type),
                      new SqlParameter("Criteria", criteria),
                      new SqlParameter("Text", text),
                      new SqlParameter("DateType", dateType),
                      new SqlParameter("StartDate", startDate),
                      new SqlParameter("EndDate", endDate),
                      new SqlParameter("MeetingStartDate", meetingStartDate),
                      new SqlParameter("MeetingEndDate", meetingEndDate),
                      new SqlParameter("Type", "mode")).ToList();

                    var modeReportBasedDate = db.Database.SqlQuery<SP_Meeting_Report>(
                      "SP_GetMeetingsReport @StaffNo,@MeetingCode,@StatusOptions,@ConditionOptions,@TypeOptions,@MeetingType,@Criteria,@Text,@DateType," +
                      "@StartDate, @EndDate, @MeetingStartDate, @MeetingEndDate, @Type,@BasedDate",
                      new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                      new SqlParameter("MeetingCode", ""),
                       new SqlParameter("StatusOptions", "A"),
                      new SqlParameter("ConditionOptions", conditionOptionsString),
                      new SqlParameter("TypeOptions", typeOptionsString),
                      new SqlParameter("MeetingType", meeting_type),
                      new SqlParameter("Criteria", criteria),
                      new SqlParameter("Text", text),
                      new SqlParameter("DateType", dateType),
                      new SqlParameter("StartDate", startDate),
                      new SqlParameter("EndDate", endDate),
                      new SqlParameter("MeetingStartDate", meetingStartDate),
                      new SqlParameter("MeetingEndDate", meetingEndDate),
                      new SqlParameter("Type", "mode"),
                      new SqlParameter("BasedDate", "Y")).ToList();

                    //GRAF Meeting Mode PART

                    var mode = modeReport
                    .GroupBy(item => item.total)
                    .Select(group => new { Mode = string.Join(", ", group.Select(item => item.mode)), Total = group.Key })
                    
                    .ToList();

                    var modeName = JsonConvert.SerializeObject(mode.Select(item => item.Mode).ToArray());
                    var modeTotal = JsonConvert.SerializeObject(mode.Select(item => item.Total).ToArray());
                    var modeBasedDate = JsonConvert.SerializeObject(modeReportBasedDate
                     .GroupBy(item => item.mode)
                     
                     .Select(group => group
                         
                         .Select(item => new List<object> { item.mode, item.createDate, item.total })
                     )
                     .ToList());



                    //Meeting Report Based on Meeting Type
                    var meetingTypeReport = db.Database.SqlQuery<SP_Meeting_Report>(
                      "SP_GetMeetingsReport @StaffNo,@MeetingCode,@StatusOptions,@ConditionOptions,@TypeOptions,@MeetingType,@Criteria,@Text,@DateType," +
                      "@StartDate, @EndDate, @MeetingStartDate, @MeetingEndDate, @Type",
                      new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                      new SqlParameter("MeetingCode", ""),
                       new SqlParameter("StatusOptions", "A"),
                      new SqlParameter("ConditionOptions", conditionOptionsString),
                      new SqlParameter("TypeOptions", typeOptionsString),
                      new SqlParameter("MeetingType", meeting_type),
                      new SqlParameter("Criteria", criteria),
                      new SqlParameter("Text", text),
                      new SqlParameter("DateType", dateType),
                      new SqlParameter("StartDate", startDate),
                      new SqlParameter("EndDate", endDate),
                      new SqlParameter("MeetingStartDate", meetingStartDate),
                      new SqlParameter("MeetingEndDate", meetingEndDate),
                      new SqlParameter("Type", "meeting_type")).ToList();

                    var meetingTypeReportBasedDate = db.Database.SqlQuery<SP_Meeting_Report>(
                      "SP_GetMeetingsReport @StaffNo,@MeetingCode,@StatusOptions,@ConditionOptions,@TypeOptions,@MeetingType,@Criteria,@Text,@DateType," +
                      "@StartDate, @EndDate, @MeetingStartDate, @MeetingEndDate, @Type,@BasedDate",
                      new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                      new SqlParameter("MeetingCode", ""),
                       new SqlParameter("StatusOptions", "A"),
                      new SqlParameter("ConditionOptions", conditionOptionsString),
                      new SqlParameter("TypeOptions", typeOptionsString),
                      new SqlParameter("MeetingType", meeting_type),
                      new SqlParameter("Criteria", criteria),
                      new SqlParameter("Text", text),
                      new SqlParameter("DateType", dateType),
                      new SqlParameter("StartDate", startDate),
                      new SqlParameter("EndDate", endDate),
                      new SqlParameter("MeetingStartDate", meetingStartDate),
                      new SqlParameter("MeetingEndDate", meetingEndDate),
                      new SqlParameter("Type", "meeting_type"),
                      new SqlParameter("BasedDate", "Y")).ToList();

                    //GRAF Meeting Meeting Type PART

                    var meetingType = meetingTypeReport
                    .GroupBy(item => item.total)
                    .Select(group => new { MeetingType = string.Join(", ", group.Select(item => item.meeting_type)), Total = group.Key })

                    .ToList();

                    var meetingTypeName = JsonConvert.SerializeObject(meetingType.Select(item => item.MeetingType).ToArray());
                    var meetingTypeTotal = JsonConvert.SerializeObject(meetingType.Select(item => item.Total).ToArray());
                    var meetingTypeBasedDate = JsonConvert.SerializeObject(meetingTypeReportBasedDate
                     .GroupBy(item => item.meeting_type)

                     .Select(group => group

                         .Select(item => new List<object> { item.meeting_type, item.createDate, item.total })
                     )
                     .ToList());

                    //Meeting Report Based on Certificate
                    var certificateReport = db.Database.SqlQuery<SP_Meeting_Report>(
                      "SP_GetMeetingsReport @StaffNo,@MeetingCode,@StatusOptions,@ConditionOptions,@TypeOptions,@MeetingType,@Criteria,@Text,@DateType," +
                      "@StartDate, @EndDate, @MeetingStartDate, @MeetingEndDate, @Type",
                      new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                      new SqlParameter("MeetingCode", ""),
                       new SqlParameter("StatusOptions", "A"),
                      new SqlParameter("ConditionOptions", conditionOptionsString),
                      new SqlParameter("TypeOptions", typeOptionsString),
                      new SqlParameter("MeetingType", meeting_type),
                      new SqlParameter("Criteria", criteria),
                      new SqlParameter("Text", text),
                      new SqlParameter("DateType", dateType),
                      new SqlParameter("StartDate", startDate),
                      new SqlParameter("EndDate", endDate),
                      new SqlParameter("MeetingStartDate", meetingStartDate),
                      new SqlParameter("MeetingEndDate", meetingEndDate),
                      new SqlParameter("Type", "certificate")).ToList();


                    var certificateReportBasedDate = db.Database.SqlQuery<SP_Meeting_Report>(
                      "SP_GetMeetingsReport @StaffNo,@MeetingCode,@StatusOptions,@ConditionOptions,@TypeOptions,@MeetingType,@Criteria,@Text,@DateType," +
                      "@StartDate, @EndDate, @MeetingStartDate, @MeetingEndDate, @Type,@BasedDate",
                      new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                      new SqlParameter("MeetingCode", ""),
                        new SqlParameter("StatusOptions", "A"),
                      new SqlParameter("ConditionOptions", conditionOptionsString),
                      new SqlParameter("TypeOptions", typeOptionsString),
                      new SqlParameter("MeetingType", meeting_type),
                      new SqlParameter("Criteria", criteria),
                      new SqlParameter("Text", text),
                      new SqlParameter("DateType", dateType),
                      new SqlParameter("StartDate", startDate),
                      new SqlParameter("EndDate", endDate),
                      new SqlParameter("MeetingStartDate", meetingStartDate),
                      new SqlParameter("MeetingEndDate", meetingEndDate),
                      new SqlParameter("Type", "certificate"),
                      new SqlParameter("BasedDate", "Y")).ToList();


                    //GRAF Meeting Certificate PART

                    var certificate = certificateReport
                    .GroupBy(item => item.total)
                    .Select(group => new { Certificate = string.Join(", ", group.Select(item => item.certificate)), Total = group.Key })
        
                    .ToList();

                    var certificateName = JsonConvert.SerializeObject(certificate.Select(item => item.Certificate).ToArray());
                    var certificateTotal = JsonConvert.SerializeObject(certificate.Select(item => item.Total).ToArray());
                    var certificateBasedDate = JsonConvert.SerializeObject(certificateReportBasedDate
                     .GroupBy(item => item.certificate)
                     
                     .Select(group => group
                         
                         .Select(item => new List<object> { item.certificate, item.createDate, item.total })
                     )
                     .ToList());


                    //Meeting Report Based on Condition
                    var conditionReport = db.Database.SqlQuery<SP_Meeting_Report>(
                     "SP_GetMeetingsReport @StaffNo,@MeetingCode,@StatusOptions,@ConditionOptions,@TypeOptions,@MeetingType,@Criteria,@Text,@DateType," +
                     "@StartDate, @EndDate, @MeetingStartDate, @MeetingEndDate, @Type",
                     new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                     new SqlParameter("MeetingCode", ""),
                       new SqlParameter("StatusOptions", "A"),
                      new SqlParameter("ConditionOptions", conditionOptionsString),
                      new SqlParameter("TypeOptions", typeOptionsString),
                      new SqlParameter("MeetingType", meeting_type),
                      new SqlParameter("Criteria", criteria),
                      new SqlParameter("Text", text),
                      new SqlParameter("DateType", dateType),
                      new SqlParameter("StartDate", startDate),
                      new SqlParameter("EndDate", endDate),
                      new SqlParameter("MeetingStartDate", meetingStartDate),
                      new SqlParameter("MeetingEndDate", meetingEndDate),
                     new SqlParameter("Type", "condition")).ToList();

                    var conditionReportBasedDate = db.Database.SqlQuery<SP_Meeting_Report>(
                      "SP_GetMeetingsReport @StaffNo,@MeetingCode,@StatusOptions,@ConditionOptions,@TypeOptions,@MeetingType,@Criteria,@Text,@DateType," +
                      "@StartDate, @EndDate, @MeetingStartDate, @MeetingEndDate, @Type,@BasedDate",
                      new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString()),
                      new SqlParameter("MeetingCode", ""),
                        new SqlParameter("StatusOptions", "A"),
                      new SqlParameter("ConditionOptions", conditionOptionsString),
                      new SqlParameter("TypeOptions", typeOptionsString),
                      new SqlParameter("MeetingType", meeting_type),
                      new SqlParameter("Criteria", criteria),
                      new SqlParameter("Text", text),
                      new SqlParameter("DateType", dateType),
                      new SqlParameter("StartDate", startDate),
                      new SqlParameter("EndDate", endDate),
                      new SqlParameter("MeetingStartDate", meetingStartDate),
                      new SqlParameter("MeetingEndDate", meetingEndDate),
                      new SqlParameter("Type", "condition"),
                      new SqlParameter("BasedDate", "Y")).ToList();

                    //GRAF Meeting Condition PART

                    var condition = conditionReport
                    .GroupBy(item => item.total)
                    .Select(group => new { Condition = string.Join(", ", group.Select(item => item.condition)), Total = group.Key })
           
                    .ToList();

                    var conditionName = JsonConvert.SerializeObject(condition.Select(item => item.Condition).ToArray());
                    var conditionTotal = JsonConvert.SerializeObject(condition.Select(item => item.Total).ToArray());
                    var conditionBasedDate = JsonConvert.SerializeObject(conditionReportBasedDate
                     .GroupBy(item => item.condition)
                    
                     .Select(group => group
                       
                         .Select(item => new List<object> { item.condition, item.createDate, item.total })
                     )
                     .ToList());

                
                     var user_id= Session["user_id"]?.ToString();
                    List<sys_setting_define_table> defineTableList= db.sys_setting_define_table
                     .Where(item => item.createBy == user_id && item.module == "meeting")
                     .ToList();


                    var result = meetingsList.Select(u => new
                    {
                        link = ((meetingAccess?.status == true && meetingAccess?.viewFunction == true) || (meetingAccess?.status == true && meetingAccess?.editFunction == true))
                            ? $@"class='clickable-row' onclick=""window.location.href = '{Url.Action("MainForm", "Meeting", new { id = u.meeting_code })}'"""
                            : "",
                        meeting_code=(defineTableList.Any(dt => dt.field == "meeting_code" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.meeting_code.ToUpper(): "NS",
                        title=(defineTableList.Any(dt => dt.field == "title" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.title: "NS",
                        meeting_type = (defineTableList.Any(dt => dt.field == "meeting_type" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.meeting_type == "M" ? "<span >Meeting / Discussion</span>" : u.meeting_type == "E" ? "<span >Event</span>": u.meeting_type == "T" ? "<span >Training</span>" : "": "NS",
                        factory = (defineTableList.Any(dt => dt.field == "factory" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.factoryName + " (" + u.factory.ToUpper() + ")": "NS",
                        location = (defineTableList.Any(dt => dt.field == "location" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.LocationName + " (" + u.location.ToUpper() + ")": "NS",
                        department = (defineTableList.Any(dt => dt.field == "department" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.DepartmentName + " (" + u.department.ToUpper() + ")": "NS",
                        
                        startDate = (defineTableList.Any(dt => dt.field == "meetingStartDate" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?"<span class='createDate'>" + u.startDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture) + "</span>": "NS",
                        endDate = (defineTableList.Any(dt => dt.field == "meetingEndDate" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?"<span class='createDate'>" + u.endDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture) + "</span>": "NS",
                        organizer = (defineTableList.Any(dt => dt.field == "organizer" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.organizer + " (" + u.department + ","+ u.location +")": "NS",
                        place = (defineTableList.Any(dt => dt.field == "place" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.place != null &&u.link!=null? "<span >"+u.place+ "<br /><b>OR</b> <br />Online</span>" : u.place != null ? "<span >" + u.place+"</span>": u.link != null ? "<span >Online</span>":"": "NS",
                        type = (defineTableList.Any(dt => dt.field == "type" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.type ==true ? "<span >Private</span>" : "<span >Public</span>": "NS",
                        certificate  = (defineTableList.Any(dt => dt.field == "certificate" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.certificate == true ? "<span >Certificate Provided</span>" : "<span >No Certificate</span>": "NS",
                       
                        createdBy = (defineTableList.Any(dt => dt.field == "createBy" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.CreateByName + " (" + u.createBy.ToUpper() + ")": "NS",
                        createdAt = (defineTableList.Any(dt => dt.field == "createDate" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?"<span class='createDate'>" + u.createDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture) + "</span>": "NS",

                        edited_by = (defineTableList.Any(dt => dt.field == "editBy" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.EditByName != null && !string.IsNullOrWhiteSpace(u.editBy) ? u.EditByName + " (" + u.editBy.ToUpper() + ")" : "<span class='text-danger'>No data</span>": "NS",
                        edited_at = (defineTableList.Any(dt => dt.field == "editDate" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.editDate != null ? "<span class='editDate'>" + u.editDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture) + "</span>"
                                    : "<span class='text-danger'>No data</span>": "NS",
                        condition = (defineTableList.Any(dt => dt.field == "condition" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.condition == "P" ? "<span class='text-primary'>Pending</span>" : u.condition == "S" ? "<span class='text-primary'>Started</span>" : "<span class='text-danger'>Expired</span>": "NS",
                        status = (meetingAccess?.status == true && meetingAccess?.deleteFunction == true) ? 
                                u.status == "A" ? "<span class='text-primary'>Active</span>" 
                                : "<span class='text-danger'>Inactive</span>"
                                :"NS",
                       
                        uploadGallery = (meetingAccess?.status == true && meetingAccess?.editFunction == true)
                                        ? u.condition == "E"
                                            ? $@"<input name=""attachments"" id=""attachments_{u.meeting_code}"" type=""file"" multiple="""" style=""width: 160px; "">
                                                <section class=""progress-demo mt-2"">
                                                    <button class=""ladda-button btn btn-primary btn-block"" data-style=""zoom-in""
                                                            onclick=""uploadImg('{u.meeting_code}')"" >
                                                        Upload
                                                    </button>
                                                </section>"
                                            : ""
                                        : "NS",
                        editBtn = (meetingAccess?.status == true && meetingAccess?.editFunction == true)
                                    ? $@"<a href=""{Url.Action("MainForm", "Meeting", new { id = u.meeting_code })}"" class=""btn btn-primary"">Edit</a>"
                                    : (meetingAccess?.status == true && meetingAccess?.viewFunction == true)
                                    ? $@"<a href=""{Url.Action("MainForm", "Meeting", new { id = u.meeting_code })}"" class=""btn btn-primary"">View</a>" : "",
                        statusBtn = (meetingAccess?.status == true && meetingAccess?.deleteFunction == true) ?
                                    u.status == "A"
                                    ? $@"<button style='width:6em;' class='btn btn-danger  ml-1' onclick='updateStatus(""Meeting"", ""{u.meeting_code}"", ""{u.status}"","""", ""Meeting Report"", ""Meetings"", ""{Session["timezone"]}"")'>Inactive</button>"
                                    : $@"<button style='width:6em;' class='btn btn-info  ml-1' onclick='updateStatus(""Meeting"", ""{u.meeting_code}"", ""{u.status}"","""", ""Meeting Report"", ""Meetings"", ""{Session["timezone"]}"")'>Reactive</button>"
                                    : ""
                    });

                    var meetingReportData = meetingReport.Select(p => new
                    {
                        link = ((meetingAccess?.status == true && meetingAccess?.viewFunction == true) || (meetingAccess?.status == true && meetingAccess?.editFunction == true))
                           ? $@"class='clickable-row' onclick=""window.location.href = '{Url.Action("MainForm", "Meeting", new { id = p.meeting_code })}'"""
                           : "",
                        p.meeting_code,
                        p.meeting,
                        p.factory,
                        p.location,
                        p.department,
                        p.total


                    });


                    var locationReportData = locationReport.Select(p => new
                    {

                        p.location,
                        p.factory,
                        p.department,
                        p.total


                    });
                    var placeReportData = placeReport.Select(p => new
                    {

                        p.place,
                        p.total



                    });
                    var typeReportData = typeReport.Select(p => new
                    {

                        p.type,
                        p.total
                    });

                    var modeReportData =modeReport.Select(p => new
                    {

                        p.mode,
                        p.total
                    });

                    var meetingTypeReportData = meetingTypeReport.Select(p => new
                    {

                        p.meeting_type,
                        p.total
                    });
                    var certificateReportData = certificateReport.Select(p => new
                    {

                        p.certificate,
                        p.total
                    });
                    var conditionReportData = conditionReport.Select(p => new
                    {

                        p.condition,
                        p.total
                    });

                    
                    return Json(new { 
                        //pass datatable data
                        result = result,
                        locationReportData = locationReportData,
                        typeReportData = typeReportData,
                        modeReportData = modeReportData,
                        meetingTypeReportData = meetingTypeReportData,
                        certificateReportData = certificateReportData,
                        conditionReportData = conditionReportData,
                        meetingReportData = meetingReportData,
                        placeReportData = placeReportData,

                        //pass meeting graf data                       
                        meetingCodeTop3 = meetingCodeTop3,
                        meetingTotalTop3 = meetingTotalTop3,
                        meetingTop3BasedDate = meetingTop3BasedDate,

                        //pass location graf data
                        meetingLocationNameTop3 = meetingLocationNameTop3,
                        meetingLocationTotalTop3 = meetingLocationTotalTop3,
                        meetingLocationTop3BasedDate = meetingLocationTop3BasedDate,

                        //pass place graf data
                        meetingPlaceNameTop3 = meetingPlaceNameTop3,
                        meetingPlaceTotalTop3 = meetingPlaceTotalTop3,
                        meetingPlaceTop3BasedDate = meetingPlaceTop3BasedDate,

                        //pass type graf data
                        typeName = typeName,
                        typeTotal = typeTotal,
                        typeBasedDate = typeBasedDate,

                        //pass mode graf data
                        modeName = modeName,
                        modeTotal = modeTotal,
                        modeBasedDate = modeBasedDate,

                        //pass meetingType graf data
                        meetingTypeName = meetingTypeName,
                        meetingTypeTotal = meetingTypeTotal,
                        meetingTypeBasedDate = meetingTypeBasedDate,

                        //pass certificate graf data
                        certificateName = certificateName,
                        certificateTotal = certificateTotal,
                        certificateBasedDate = certificateBasedDate,

                        //pass condition graf data
                        conditionName = conditionName,
                        conditionTotal = conditionTotal,
                        conditionBasedDate = conditionBasedDate,

                        //grafType
                        grafType= "doughnut",
                        success = true
                    }, JsonRequestBehavior.AllowGet);
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
        /*------------------------------------------Filter or Search Function END------------------------------------------*/

        /*---------------------------------------------------Invite Meeting START------------------------------------------*/
        public ActionResult InviteMeeting(string id)
        {
            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    if (id == null)
                    {
                        return RedirectToAction("NotFound", "Home");

                    }
                    var meeting = db.Database.SqlQuery<SP_GetMeetings_Result>(
                                "SP_GetMeetings @StaffNo ,@MeetingCode",
                                new SqlParameter("StaffNo", "GetDetail"),
                                new SqlParameter("MeetingCode", id)).FirstOrDefault();
                    if (meeting == null)
                    {
                        return RedirectToAction("NotFound", "Home");

                    }
                    if (TempData[id]?.ToString() != "true")
                    {
                        Session.Remove(id);
                    }
                    ViewBag.QRCode = HomeController.GenerateQrCode($"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action("InviteMeeting", new { id = meeting.meeting_code })}");
                    return View(meeting);
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

        /*---------------------------------------------------Invite Meeting END------------------------------------------*/

        /*---------------------------------------------------Join Meeting START------------------------------------------*/
        public ActionResult JoinMeeting(string id)
        {
            try
            {
                using (var db = new AttendNow_DBEntities())
                {

                    //pass location_db, department_db, role_db to view in dropdown format
                    var activeLocations = db.Database.SqlQuery<SP_GetLocationsWithFactory_Result>(
                        "SP_GetLocationsWithFactory @LocationCode ,@StatusOptions",
                        new SqlParameter("LocationCode", ""),
                        new SqlParameter("StatusOptions", "A")).ToList();

                    ViewBag.location_code = new SelectList(
                        activeLocations.Select(l => new SelectListItem
                        {
                            Value = l.location_code,
                            Text = l.factory + "-" + l.name
                        }),
                        "Value", "Text"
                    );

                    var activeDepartments = db.Database.SqlQuery<SP_GetDepartments_Result>(
                        "SP_GetDepartments @DepartmentCode ,@StatusOptions",
                        new SqlParameter("DepartmentCode", ""),
                        new SqlParameter("StatusOptions", "A")).ToList();
                    ViewBag.department_code = new SelectList(activeDepartments, "department_code", "name");
                    var activeFactory = db.Database.SqlQuery<SP_GetFactorys_Result>(
                      "SP_GetFactorys @Factory_id ,@StatusOptions",
                      new SqlParameter("Factory_id", ""),
                      new SqlParameter("StatusOptions", "A")).ToList();

                    ViewBag.factory_id = new SelectList(activeFactory, "factory_id", "name");
                    


                    if (id == null)
                    {
                        return RedirectToAction("NotFound", "Home");

                    }
                    var meeting = db.Database.SqlQuery<SP_GetMeetings_Result>(
                                "SP_GetMeetings @StaffNo ,@MeetingCode",
                                new SqlParameter("StaffNo", "GetDetail"),
                                new SqlParameter("MeetingCode", id)).FirstOrDefault();
                    if (meeting == null)
                    {
                        return RedirectToAction("NotFound", "Home");

                    }
                    TempData[id] = Session[id]?.ToString();
                    Session.Remove(id);
                    return View(meeting);

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

                ViewBag.ErrorMessage  = e.InnerException == null ? e.Message : e.InnerException.Message;
                return View();
            }

        }
        /*---------------------------------------------------Join Meeting END------------------------------------------*/

        [HttpPost]
        public JsonResult AccessPrivateMeeting(string password, string meeting_code)
        {
            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    if (string.IsNullOrEmpty(password) )
                    {
                        return Json(new { success = false, error_message = "Make sure all fields are filled" });
                    }
                   
                    string SecretKey = ConfigurationManager.AppSettings["SecretKey"];
                    long SecretKey_8byte = StringTo8ByteValue(SecretKey);
                    long meeting_code_8byte = StringTo8ByteValue(meeting_code);


                    
                    var encryptedpassword = Encrypt(password, SecretKey_8byte.ToString(), meeting_code_8byte.ToString());





                    if (db.tbl_meeting.Any(m => m.meeting_code == meeting_code && m.password == encryptedpassword) )
                    {
                        Session[meeting_code] = "true";
                        TempData[meeting_code] = "true";
                        TempData["SuccessMessage"] = "Entry successful. We hope you can join our meeting.";

                        return Json(new { success = true, message = "Entry successful. We hope you can join our meeting." });
                    }
                    else
                    {

                        return Json(new { success = false, error_message = "Wrong Password !!!" });
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


        /*------------------------------------------Security Cryptography START------------------------------------------*/

        private long StringTo8ByteValue(string inputString)
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

                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        public static string Decrypt(string password, string secretKey, string salt)
        {
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


        
        /*------------------------------------------Security Cryptography END------------------------------------------*/


    }
}