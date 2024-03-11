using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AttendNow.Models;
using System.Globalization;
using System.Data.SqlClient;
using System.Data.Entity.Validation;
using System.Text.RegularExpressions;
using System.Net.Mail;
using AttendNow.Models.ViewModel;
using System.Configuration;
using Newtonsoft.Json;


namespace AttendNow.Controllers
{
    public class ParticipantController : Controller
    {

        //GET: Participant
        public ActionResult Index(string id)
        {
            try
            {

                using (var db = new AttendNow_DBEntities())
                {

                    ViewBag.Title = "Participant List";
                    var roleID = Session["role_id"]?.ToString();
                    var participantAccess = db.sys_setting_role_permission.SingleOrDefault(rp => rp.role == roleID && rp.module == "participant");
                    ViewBag.participantAccess = participantAccess;
                    var dateType = "joinDate";
                    var startDate = "";
                    var endDate = "";
                    var month = "";
                    var meetingAccess = db.sys_setting_role_permission.SingleOrDefault(rp => rp.role == roleID && rp.module == "meeting");
                    ViewBag.meetingAccess = meetingAccess;
                    if (participantAccess?.status != true)
                    {
                        return RedirectToAction("Logout", "User");
                    }
                    if (Session["user_id"] == null)
                    {
                        return RedirectToAction("Logout", "User");
                    }
                    var statusOptionsString = "A,V,P";
                    if (participantAccess.deleteFunction != true && participantAccess.editFunction != true)
                    {
                        statusOptionsString = "A";
                    }
                    if (participantAccess.deleteFunction != true)
                    {
                        statusOptionsString = "A,P";
                    }
                    if (participantAccess.editFunction != true)
                    {
                        statusOptionsString = "A,V";
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


                    var participantReport = db.Database.SqlQuery<SP_Participant_Report>(
                               "SP_GetParticipantsReport @MeetingCode,@ControlBy,@StatusOptions, @Criteria, @Text, @DateType, @StartDate, @EndDate ,@Type",

                                          new SqlParameter("MeetingCode", id?.ToString() ?? ""),
                                          new SqlParameter("ControlBy", (string)Session["user_id"]?.ToString()),

                                           new SqlParameter("StatusOptions", "A"),
                                           new SqlParameter("Criteria", ""),
                                           new SqlParameter("Text", ""),
                                           new SqlParameter("DateType", dateType),
                                           new SqlParameter("StartDate", startDate),
                                           new SqlParameter("EndDate", endDate),
                                           new SqlParameter("Type", "participant")

                                        ).ToList();


                    var participantReportBasedDate = db.Database.SqlQuery<SP_Participant_Report>(
                               "SP_GetParticipantsReport @MeetingCode,@ControlBy,@StatusOptions, @Criteria, @Text, @DateType, @StartDate, @EndDate ,@Type,@BasedDate",

                                          new SqlParameter("MeetingCode", id?.ToString() ?? ""),
                                          new SqlParameter("ControlBy", (string)Session["user_id"]?.ToString()),
                                           new SqlParameter("StatusOptions", "A"),
                                           new SqlParameter("Criteria", ""),
                                           new SqlParameter("Text", ""),
                                           new SqlParameter("DateType", dateType),
                                           new SqlParameter("StartDate", startDate),
                                           new SqlParameter("EndDate", endDate),
                                           new SqlParameter("Type", "participant"),
                                           new SqlParameter("BasedDate", "Y")

                                        ).ToList();


                    var participantStatusReport = db.Database.SqlQuery<SP_Participant_Report>(
                              "SP_GetParticipantsReport @MeetingCode,@ControlBy,@StatusOptions, @Criteria, @Text, @DateType, @StartDate, @EndDate ,@Type",

                                         new SqlParameter("MeetingCode", id?.ToString() ?? ""),
                                         new SqlParameter("ControlBy", (string)Session["user_id"]?.ToString()),

                                          new SqlParameter("StatusOptions", "A,P,V"),
                                          new SqlParameter("Criteria", ""),
                                          new SqlParameter("Text", ""),
                                          new SqlParameter("DateType", dateType),
                                          new SqlParameter("StartDate", startDate),
                                          new SqlParameter("EndDate", endDate),
                                          new SqlParameter("Type", "participantStatus")

                                       ).ToList();


                    var participantStatusReportBasedDate = db.Database.SqlQuery<SP_Participant_Report>(
                               "SP_GetParticipantsReport @MeetingCode,@ControlBy,@StatusOptions, @Criteria, @Text, @DateType, @StartDate, @EndDate ,@Type,@BasedDate",

                                          new SqlParameter("MeetingCode", id?.ToString() ?? ""),
                                          new SqlParameter("ControlBy", (string)Session["user_id"]?.ToString()),
                                           new SqlParameter("StatusOptions", "A,P,V"),
                                           new SqlParameter("Criteria", ""),
                                           new SqlParameter("Text", ""),
                                           new SqlParameter("DateType", dateType),
                                           new SqlParameter("StartDate", startDate),
                                           new SqlParameter("EndDate", endDate),
                                           new SqlParameter("Type", "participantStatus"),
                                           new SqlParameter("BasedDate", "Y")

                                        ).ToList();
                    var locationReport = db.Database.SqlQuery<SP_Participant_Report>(
                               "SP_GetParticipantsReport @MeetingCode,@ControlBy,@StatusOptions, @Criteria, @Text, @DateType, @StartDate, @EndDate ,@Type",

                                          new SqlParameter("MeetingCode", id?.ToString() ?? ""),
                                          new SqlParameter("ControlBy", (string)Session["user_id"]?.ToString()),

                                           new SqlParameter("StatusOptions", "A"),
                                           new SqlParameter("Criteria", ""),
                                           new SqlParameter("Text", ""),
                                           new SqlParameter("DateType", dateType),
                                           new SqlParameter("StartDate", startDate),
                                           new SqlParameter("EndDate", endDate),
                                           new SqlParameter("Type", "location")

                                        ).ToList();

                    var locationReportBasedDate = db.Database.SqlQuery<SP_Participant_Report>(
                               "SP_GetParticipantsReport @MeetingCode,@ControlBy,@StatusOptions, @Criteria, @Text, @DateType, @StartDate, @EndDate,@Type ,@BasedDate",

                                          new SqlParameter("MeetingCode", id?.ToString() ?? ""),
                                          new SqlParameter("ControlBy", (string)Session["user_id"]?.ToString()),

                                           new SqlParameter("StatusOptions", "A"),
                                           new SqlParameter("Criteria", ""),
                                           new SqlParameter("Text", ""),
                                           new SqlParameter("DateType", dateType),
                                           new SqlParameter("StartDate", startDate),
                                           new SqlParameter("EndDate", endDate),
                                           new SqlParameter("Type", "location"),
                                           new SqlParameter("BasedDate", "Y")

                                        ).ToList();
                    var participantList = db.Database.SqlQuery<SP_GetParticipants_Result>(
                        "SP_GetParticipants @CertificateNo,@MeetingCode,@ControlBy,@StaffNo, @StatusOptions, @Criteria, @Text, @DateType, @StartDate, @EndDate",
                       new SqlParameter("CertificateNo", ""),
                        new SqlParameter("MeetingCode", id?.ToString() ?? ""),
                        new SqlParameter("ControlBy", (string)Session["user_id"]?.ToString()),
                         new SqlParameter("StaffNo", ""),
                         new SqlParameter("StatusOptions", statusOptionsString),
                         new SqlParameter("Criteria", ""),
                         new SqlParameter("Text", ""),
                         new SqlParameter("DateType", dateType),
                         new SqlParameter("StartDate", startDate),
                         new SqlParameter("EndDate", endDate)

                        ).ToList();

                   
                    if (id != null)
                    {
                        ViewBag.MeetingTitle = db.tbl_meeting.FirstOrDefault(m => m.meeting_code == id).title;
                        ViewBag.MeetingCode = id;
                    }
                    //GRAF PARTICIPANT PART

                    var participantTop3 = participantReport
                    .GroupBy(item => item.total)
                    .Select(group => new { Participant = string.Join(", ", group.Select(item => item.staff_no + " (" + item.name + ")")), Total = group.Key })
                   
                    .ToList();

                    ViewBag.participantNameTop3 = JsonConvert.SerializeObject(participantTop3.Select(item => item.Participant).ToArray());
                    ViewBag.participantTotalTop3 = JsonConvert.SerializeObject(participantTop3.Select(item => item.Total).ToArray());
                   

                    ViewBag.participantTop3BasedDate = JsonConvert.SerializeObject(participantReportBasedDate
                    .GroupBy(item => item.staff_no)
                    
                    .Select(group => group
                       
                        .Select(item => new List<object> { item.staff_no + " (" + item.name + ")", item.joinedDate, item.total })
                    )
                    .ToList());
                    

                    //GRAF LOCATION PART

                    var locationTop3 = locationReport
                    .GroupBy(item => item.total)
                    .Select(group => new { Location = string.Join(", ", group.Select(item => item.location)), Total = group.Key })
                   
                    .ToList();

                    ViewBag.locationNameTop3 = JsonConvert.SerializeObject(locationTop3.Select(item => item.Location).ToArray());
                    ViewBag.locationTotalTop3 = JsonConvert.SerializeObject(locationTop3.Select(item => item.Total).ToArray());
                    ViewBag.locationTop3BasedDate = JsonConvert.SerializeObject(locationReportBasedDate
                     .GroupBy(item => item.location)
                     
                     .Select(group => group
                       
                         .Select(item => new List<object> { item.location, item.joinedDate, item.total })
                     )
                     .ToList());

                    //GRAF Participant Status PART

                    var participantStatusTop3 = participantStatusReport
                    .GroupBy(item => item.total)
                    .Select(group => new { Status = string.Join(", ", group.Select(item => item.status)), Total = group.Key })

                    .ToList();

                    ViewBag.participantStatusNameTop3 = JsonConvert.SerializeObject(participantStatusTop3.Select(item => item.Status).ToArray());
                    ViewBag.participantStatusTotalTop3 = JsonConvert.SerializeObject(participantStatusTop3.Select(item => item.Total).ToArray());
                    ViewBag.participantStatusTop3BasedDate = JsonConvert.SerializeObject(participantStatusReportBasedDate
                     .GroupBy(item => item.status)

                     .Select(group => group

                         .Select(item => new List<object> { item.status, item.joinedDate, item.total })
                     )
                     .ToList());
                    var user_id = Session["user_id"]?.ToString();
                    ViewBag.DefineTable = db.sys_setting_define_table
                     .Where(item => item.createBy == user_id && item.module == "participant")
                     .ToList();
                    var participantData = new ParticipantData();
                    participantData.ParticipantReport = participantReport;
                    participantData.LocationReport = locationReport;
                    participantData.ParticipantList = participantList;
                    participantData.ParticipantStatusReport = participantStatusReport;
                   
                    ViewBag.dataDate = "Month " + month;
                    TempData["page"] = "participantlist";
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



        /*------------------------------------------Add, Edit, View Function START------------------------------------------*/
        // GET: Participant/MainForm/ :FOR ADD Participant 
        // GET: Participant/MainForm/5 :FOR EDIT Participant 
        public ActionResult MainForm(string id)
        {
            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    var roleID = Session["role_id"]?.ToString();
                    var participantAccess = db.sys_setting_role_permission.SingleOrDefault(rp => rp.role == roleID && rp.module == "participant");
                    ViewBag.participantAccess = participantAccess;
                    if (participantAccess?.status != true) //If role permission status is false then logout
                    {
                        return RedirectToAction("Logout", "User");
                    }

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
                    var activeMeetings = db.Database.SqlQuery<SP_GetMeetings_Result>(
                       "SP_GetMeetings @StaffNo, @MeetingCode,@StatusOption,@ConditionOptions",
                       new SqlParameter("StaffNo", (string)Session["user_id"]?.ToString() ?? ""),
                       new SqlParameter("MeetingCode", ""),
                       new SqlParameter("StatusOption", "A"),
                       new SqlParameter("ConditionOptions", "P")).ToList();
                  
                   
                    TimeZoneInfo serverTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                    if ((string)Session["timezone"]?.ToString() == "M")
                    {
                        TimeZoneInfo malaysiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                        ViewBag.meeting_code = new SelectList(
                        activeMeetings.Select(m => new SelectListItem
                        {
                            Value = m.meeting_code,
                            Text = m.title + " (Time: " +
                                   (m.startDate.HasValue ? TimeZoneInfo.ConvertTime(TimeZoneInfo.ConvertTimeToUtc(m.startDate.Value, serverTimeZone), malaysiaTimeZone).ToString("dd/MM/yyyy h:mm:ss tt") : "N/A") +
                                   " to " +
                                    (m.endDate.HasValue ? TimeZoneInfo.ConvertTime(TimeZoneInfo.ConvertTimeToUtc(m.endDate.Value, serverTimeZone), malaysiaTimeZone).ToString("dd/MM/yyyy h:mm:ss tt") : "N/A") +
                                   "| Presenter: " + m.organizer + ", " + m.DepartmentName + ", " + m.LocationName + ")"
                        }),
                        "Value", "Text"
                    );
                    }else if((string)Session["timezone"]?.ToString() == "V")
                    {
                        TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                        ViewBag.meeting_code = new SelectList(
                        activeMeetings.Select(m => new SelectListItem
                        {
                            Value = m.meeting_code,
                            Text = m.title + " (Time: " +
                                   (m.startDate.HasValue ? TimeZoneInfo.ConvertTime(TimeZoneInfo.ConvertTimeToUtc(m.startDate.Value, serverTimeZone), vietnamTimeZone).ToString("dd/MM/yyyy h:mm:ss tt") : "N/A") +
                                   " to " +
                                    (m.endDate.HasValue ? TimeZoneInfo.ConvertTime(TimeZoneInfo.ConvertTimeToUtc(m.endDate.Value, serverTimeZone), vietnamTimeZone).ToString("dd/MM/yyyy h:mm:ss tt") : "N/A") +
                                   "| Presenter: " + m.organizer + ", " + m.DepartmentName + ", " + m.LocationName + ")"
                        }),
                        "Value", "Text"
                    );
                    }else if ((string)Session["timezone"]?.ToString() == "J")
                    {
                        TimeZoneInfo jordanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Jordan Standard Time");
                        ViewBag.meeting_code = new SelectList(
                        activeMeetings.Select(m => new SelectListItem
                        {
                            Value = m.meeting_code,
                            Text = m.title + " (Time: " +
                                   (m.startDate.HasValue ? TimeZoneInfo.ConvertTime(TimeZoneInfo.ConvertTimeToUtc(m.startDate.Value, serverTimeZone), jordanTimeZone).ToString("dd/MM/yyyy h:mm:ss tt") : "N/A") +
                                   " to " +
                                    (m.endDate.HasValue ? TimeZoneInfo.ConvertTime(TimeZoneInfo.ConvertTimeToUtc(m.endDate.Value, serverTimeZone), jordanTimeZone).ToString("dd/MM/yyyy h:mm:ss tt") : "N/A") +
                                   "| Presenter: " + m.organizer + ", " + m.DepartmentName + ", " + m.LocationName + ")"
                        }),
                        "Value", "Text"
                    );
                    }






                        if (id == null)
                    {
                        ViewBag.Title = "Add Participant";
                        TempData["page"] = "addparticipant"; //Page will be add function
                        if (participantAccess?.status != true || (participantAccess?.addFunction != true)) // If role permission(ADD) status is false then logout
                        {
                            return RedirectToAction("Logout", "User");
                        }
                        return View();
                    }
                    else
                    {
                        if (participantAccess?.status != true ||
                        (participantAccess?.editFunction != true && participantAccess?.viewFunction != true))
                        // If role permission(EDIT||VIEW) status is false then logout
                        {
                            return RedirectToAction("Logout", "User");
                        }
                        else
                        {
                            if (participantAccess?.editFunction == true) //Edit Function
                            {
                                ViewBag.Title = "Edit Participant";
                            }
                            else //View Function
                            {
                                ViewBag.Title = "View Participant";
                            }

                            TempData["page"] = "participantlist";
                            ViewBag.CertificateNo = id;
                            //get specific user based on id
                            var participant = db.Database.SqlQuery<SP_GetParticipants_Result>(
                               "SP_GetParticipants @CertificateNo, @MeetingCode, @ControlBy,@StaffNo," +
                               "@StatusOptions,@Criteria,@Text,@DateType,@StartDate,@EndDate,@MeetingStartDate," +
                               "@MeetingEndDate,@MeetingCondition",
                               new SqlParameter("CertificateNo", id),
                               new SqlParameter("MeetingCode", ""),
                                new SqlParameter("ControlBy", (string)Session["user_id"]?.ToString()),
                                new SqlParameter("@StaffNo", ""),
                                new SqlParameter("@StatusOptions", ""),
                                new SqlParameter("@Criteria", ""),
                                new SqlParameter("@Text", ""),
                                new SqlParameter("@DateType", ""),
                                new SqlParameter("@StartDate", ""),
                                new SqlParameter("@EndDate", ""),
                                new SqlParameter("@MeetingStartDate", ""),
                                new SqlParameter("@MeetingEndDate", ""),
                                new SqlParameter("@MeetingCondition", "E")

                       ).FirstOrDefault();

                            if (participant == null)
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
                                   new SqlParameter("StaffNo", ""),
                                    new SqlParameter("ControlBy", ""),
                                    new SqlParameter("FactoryID", participant.factory)).ToList();
                                ViewBag.location_code = new SelectList(
                        activeLocations.Select(l => new SelectListItem
                        {
                            Value = l.location_code,
                            Text = l.factory + "-" + l.name
                        }),
                        "Value", "Text"
                    );
                                return View(participant);
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
        public JsonResult Create(tbl_participant participant)
        {
            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    if (string.IsNullOrEmpty(participant.staff_no) || string.IsNullOrEmpty(participant.name)
                     || string.IsNullOrEmpty(participant.department)
                     || string.IsNullOrEmpty(participant.email)
                    || string.IsNullOrEmpty(participant.location)
                    || string.IsNullOrEmpty(participant.factory_id)
                    || string.IsNullOrEmpty(participant.meeting))
                    {
                        return Json(new { success = false, error_message = "Make sure all fields are filled" });
                    }

                    if (participant.name.Contains("admin") || participant.name.Contains("administrator") || participant.staff_no.Contains("admin") || participant.staff_no.Contains("administrator"))
                    {
                        return Json(new { success = false, error_message = "You are not allow to use 'admin' or 'administrator' as your name or staff no !" });
                    }
                    //if the staff_no is existing in db, alert user
                    if (db.tbl_participant.Any(p => p.staff_no.ToUpper() == participant.staff_no.ToUpper() && p.factory_id.ToUpper() == participant.factory_id.ToUpper() && p.meeting.ToUpper() == participant.meeting.ToUpper()))
                    {
                        return Json(new { success = false, error_message = "This Staff No. and Factory is already in use.", idError = "This Staff No. and Factory is already in use." });
                    }
                    else
                    {
                        int count_participant = db.tbl_participant.Count(p =>
                            p.meeting.ToUpper() == participant.meeting.ToUpper()&&p.status=="A"
                        );

                        var limit=db.tbl_meeting.SingleOrDefault(m => m.meeting_code.ToUpper() == participant.meeting.ToUpper()).limit;
                        var running_Num = db.sys_setting_running_num.SingleOrDefault(n => n.module == "participant");
                        
                        if (running_Num != null)
                        {
                            if (limit <= count_participant && db.tbl_meeting.SingleOrDefault(m => m.meeting_code.ToUpper() == participant.meeting.ToUpper()).limit_status==true)
                            {
                                participant.status = "P";
                                TempData["SuccessMessage"] = "Record Saved.";
                            }
                            else
                            {
                                participant.status = "A";
                                TempData["SuccessMessage"] = "Registered Successfully.";
                            }
                            participant.certificate_no = running_Num.prefix + running_Num.running_num + participant.staff_no.ToUpper() + DateTime.Now.Millisecond.ToString();
                            
                            participant.createDate = DateTime.Now;
                            participant.staff_no = participant.staff_no.ToUpper();
                            db.tbl_participant.Add(participant);
                            db.SaveChanges();
                            if (int.TryParse(running_Num.running_num, out int running_num))
                            {
                                running_Num.running_num = $"{running_num + 1:D5}";
                                db.Entry(running_Num).State = EntityState.Modified;
                                db.SaveChanges();
                            }

                            // Send an email to inform the user
                            
                                string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$";
                                if (Regex.IsMatch(participant.email, pattern))
                                {
                                if (participant.email != null && limit > count_participant)
                                {
                                    SendEmailToParticipant(participant.certificate_no,"A");
                                }
                                else
                                {
                                    SendEmailToParticipant(participant.certificate_no, "P");
                                }
                                    
                                }
                            

                            
                            //after added successfully go to edit user page
                            return Json(new { success = true, message = "Joined Successfully.", id = participant.certificate_no });




                        }
                        return Json(new { success = false, message = "Joined Unsuccessfully." });
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



        // POST: User/Edit
        [HttpPost]
        public JsonResult Edit(tbl_participant participant)
        {
            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    if (string.IsNullOrEmpty(participant.staff_no) || string.IsNullOrEmpty(participant.name)
                    || string.IsNullOrEmpty(participant.department) || string.IsNullOrEmpty(participant.location)
                    || string.IsNullOrEmpty(participant.factory_id)
                    || string.IsNullOrEmpty(participant.meeting))
                    {
                        return Json(new { success = false, error_message = "Make sure all fields are filled" });
                    }
                    //if the user_id is existing in db, alert user
                    var existingParticipant = db.tbl_participant.SingleOrDefault(p => p.certificate_no.ToUpper() == participant.certificate_no.ToUpper());

                    if (existingParticipant != null)
                    {

                        existingParticipant.editDate = DateTime.Now;
                        existingParticipant.editBy = (string)Session["user_id"];

                        existingParticipant.name = participant.name;
                        existingParticipant.email = participant.email;
                        existingParticipant.mobile = participant.mobile;
                        existingParticipant.department = participant.department;
                        existingParticipant.location = participant.location;
                        existingParticipant.factory_id = participant.factory_id;
                        existingParticipant.meeting = participant.meeting;
                        existingParticipant.remark = participant.remark;

                        db.Entry(existingParticipant).State = EntityState.Modified;
                        db.SaveChanges();



                        TempData["SuccessMessage"] = "Updated Successfully.";
                        return Json(new { success = true, message = "Updated Successfully.", id = participant.certificate_no });
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
                                    new SqlParameter("StaffNo", ""),
                                     new SqlParameter("ControlBy", ""),
                                     new SqlParameter("FactoryID", factory_id)).ToList();

                    return Json(new
                    {
                        success = true,
                        message = "Filtering Location Successfully",
                        activeLocation = activeLocation,

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
        [HttpPost]
        public JsonResult RegisterInformation(tmp_participant participant)
        {
            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    if (string.IsNullOrEmpty(participant.name) || string.IsNullOrEmpty(participant.password)
                    || string.IsNullOrEmpty(participant.staff_no) || string.IsNullOrEmpty(participant.department)
                    || string.IsNullOrEmpty(participant.location) || string.IsNullOrEmpty(participant.factory_id)
                    || string.IsNullOrEmpty(participant.email)
                   )
                    {
                        return Json(new { success = false, error_message = "Make sure all fields are filled" });
                    }
                    //if the user_id is existing in db, alert user
                    if (db.tmp_participant.Any(p => p.staff_no.ToUpper() == participant.staff_no.ToUpper() && p.factory_id.ToUpper() == participant.factory_id.ToUpper()))
                    {
                        return Json(new { success = false, error_message = "This Staff No. is already in use.", idError = "This Staff No. is already in use." });
                    }
                    else
                    {
                        participant.status = "A";
                        participant.createDate = DateTime.Now;
                        participant.staff_no = participant.staff_no.ToUpper();
                        string SecretKey = ConfigurationManager.AppSettings["SecretKey"];
                        long SecretKey_8byte = HomeController.StringTo8ByteValue(SecretKey);

                        long staff_no_8byte = HomeController.StringTo8ByteValue(participant.staff_no.ToUpper());
                        participant.password = HomeController.Encrypt(participant.password, SecretKey_8byte.ToString(), staff_no_8byte.ToString());

                        db.tmp_participant.Add(participant);
                        db.SaveChanges();
                        TempData["SuccessMessage"] = "Registered Successfully.";
                        //after added successfully go to edit user page
                        return Json(new { success = true, message = "Registered Successfully.", particpant = participant, });
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

        [HttpPost]
        public JsonResult LoginNFillInformation(tmp_participant participant) //delete access location function
        {
            try
            {

                using (var db = new AttendNow_DBEntities())
                {
                    if (string.IsNullOrEmpty(participant.staff_no) || string.IsNullOrEmpty(participant.password))
                    {
                        return Json(new { success = false, error_message = "Make sure all fields are filled" });
                    }
                    string SecretKey = ConfigurationManager.AppSettings["SecretKey"];
                    long SecretKey_8byte = HomeController.StringTo8ByteValue(SecretKey);

                    long staff_no_8byte = HomeController.StringTo8ByteValue(participant.staff_no.ToUpper());



                    var encryptedpassword = HomeController.Encrypt(participant.password, SecretKey_8byte.ToString(), staff_no_8byte.ToString()); // Encrypt function to encrypt password

                    var particpant = db.tmp_participant.SingleOrDefault(tp => tp.staff_no.ToUpper() == participant.staff_no.ToUpper() && tp.password == encryptedpassword);
                    if (particpant == null)
                    {
                        return Json(new { success = false, error_message = "Information Not Found!" });
                    }
                    return Json(new
                    {
                        success = true,
                        message = "Fill in Information Successfully",
                        particpant = particpant,

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


        

       
        public ActionResult ParticipantDetail(string id)
        {
            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    string[] splitData = id.Split('-');


                    string getID = splitData[1];
                    var roleID = Session["role_id"]?.ToString();
                    var participantAccess = db.sys_setting_role_permission.SingleOrDefault(rp => rp.role == roleID && rp.module == "participant");
                    ViewBag.participantAccess = participantAccess;
                    if (participantAccess?.status != true) //If role permission status is false then logout
                    {
                        return RedirectToAction("Logout", "User");
                    }

                    if (getID == null)
                    {
                        return RedirectToAction("NotFound", "Home");
                    }

                    //get specific user based on getID
                    var participant = db.Database.SqlQuery<SP_GetParticipants_Result>(
                      "SP_GetParticipants @CertificateNo,@MeetingCode,@ControlBy,@StaffNo, @StatusOptions, @Criteria, @Text, @DateType, @StartDate, @EndDate," +
                       "@MeetingStartDate,@MeetingEndDate,@MeetingCondition",
                       new SqlParameter("CertificateNo", ""),
                       new SqlParameter("MeetingCode", ""),
                        new SqlParameter("ControlBy", (string)Session["user_id"]?.ToString()),
                        new SqlParameter("StaffNo", getID),
                          new SqlParameter("StatusOptions", ""),
                       new SqlParameter("Criteria", ""),
                       new SqlParameter("Text", ""),
                       new SqlParameter("DateType", ""),
                       new SqlParameter("StartDate", ""),
                       new SqlParameter("EndDate", ""),
                       new SqlParameter("MeetingStartDate", ""),
                       new SqlParameter("MeetingEndDate", ""),
                       new SqlParameter("MeetingCondition", "E")
                       ).ToList();

                    if (participant == null)
                    {
                        return RedirectToAction("NotFound", "Home");

                    }

                    TempData["page"] = "participantlist"; //Page will be edit function
                    ViewBag.Title = "Participant Details";


                    return View(participant);
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
                return View();

            }
            catch (Exception e)
            {

                @TempData["ErrorMessage"] = e.InnerException == null ? e.Message : e.InnerException.Message;
                return View();
            }

        }

        /*------------------------------------------UpdateStatus Function START------------------------------------------*/
        public JsonResult UpdateStatus(String id, string newStatus)
        {

            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    // Find the existing user in the database by user_id
                    var existingParticipant = db.tbl_participant.SingleOrDefault(p => p.certificate_no.ToUpper() == id.ToUpper());

                    if (existingParticipant != null)
                    {
                        if(newStatus=="A"&& existingParticipant.status == "P" || newStatus == "A" && existingParticipant.status == "V")
                        {
                            SendEmailToParticipant(existingParticipant.certificate_no,"RA"); //Change To Active
                        }
                        else if (newStatus == "P" && existingParticipant.status == "A"|| newStatus == "V" && existingParticipant.status == "A")
                        {
                            SendEmailToParticipant(existingParticipant.certificate_no, "TP"); //Change To Pending OR Change To Inactive
                        }
                        // Update the status property based on the newStatus parameter
                        existingParticipant.status = newStatus;
                        existingParticipant.editBy = (string)Session["user_id"];
                        existingParticipant.editDate = DateTime.Now;
                        db.Entry(existingParticipant).State = EntityState.Modified;
                        var existingControlUser = db.Database.SqlQuery<SP_GetUserProfiles_Result>("SP_GetUserProfiles @StaffNo",
                                                  new SqlParameter("StaffNo", (string)Session["user_id"])
                                              ).FirstOrDefault();

                        var userEditBy = existingControlUser.EditByName + " (" + existingControlUser.editBy.ToUpper() + ")";
                        var editDate = existingParticipant.editDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
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


        /*------------------------------------------Filter or Search Function START------------------------------------------*/
        [HttpPost]
        public ActionResult Search(List<string> statusOptions, string criteria, string text, string dateType, string startDate, string endDate, string meeting_code,string meeting_condition)
        {

            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    DateTime? serverStartDateTime = null;
                    DateTime? serverEndDateTime = null;
                    if (dateType == "null")
                    {
                        dateType = "joinDate";
                    }
                    TimeZoneInfo serverTimeZone = null;
                    TimeZoneInfo convertedTimeZone = null;
                    var roleID = Session["role_id"]?.ToString();
                    var participantAccess = db.sys_setting_role_permission.SingleOrDefault(rp => rp.role == roleID && rp.module == "participant");
                    var meetingAccess = db.sys_setting_role_permission.SingleOrDefault(rp => rp.role == roleID && rp.module == "meeting");
                    ViewBag.participantAccess = participantAccess;
                    string statusOptionsString;
                    if (statusOptions == null)
                    {
                         statusOptionsString = "A,V,P";
                        if (participantAccess.deleteFunction != true && participantAccess.editFunction != true)
                        {
                            statusOptionsString = "A";
                        }
                        if (participantAccess.deleteFunction != true)
                        {
                            statusOptionsString = "A,P";
                        }
                        if (participantAccess.editFunction != true)
                        {
                            statusOptionsString = "A,V";
                        }

                    }
                    else
                    {
                        statusOptionsString = string.Join(",", statusOptions);
                    }
                    if (startDate == "" && endDate == "" )
                    {
                        serverStartDateTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1); // Set to the first day of the current month
                        serverEndDateTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month))
                            .AddHours(23).AddMinutes(59).AddSeconds(59); /// Set to the end of the day

                    }
                    else if (startDate != "" || endDate != "")
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
                    ViewBag.meetingAccess = meetingAccess;
                   
                    
                    var participantReport = db.Database.SqlQuery<SP_Participant_Report>(
                               "SP_GetParticipantsReport @MeetingCode,@ControlBy,@StatusOptions, @Criteria, @Text, @DateType, @StartDate, @EndDate ,@Type",

                                          new SqlParameter("MeetingCode", meeting_code),
                                          new SqlParameter("ControlBy", (string)Session["user_id"]?.ToString()),

                                           new SqlParameter("StatusOptions", "A"),
                                           new SqlParameter("Criteria", criteria),
                                           new SqlParameter("Text", text),
                                           new SqlParameter("DateType", dateType),
                                           new SqlParameter("StartDate", startDate),
                                           new SqlParameter("EndDate", endDate),
                                           new SqlParameter("Type", "participant")

                                        ).ToList();
                    var participantReportBasedDate = db.Database.SqlQuery<SP_Participant_Report>(
                               "SP_GetParticipantsReport @MeetingCode,@ControlBy,@StatusOptions, @Criteria, @Text, @DateType, @StartDate, @EndDate ,@Type,@BasedDate",

                                          new SqlParameter("MeetingCode", meeting_code),
                                          new SqlParameter("ControlBy", (string)Session["user_id"]?.ToString()),
                                           new SqlParameter("StatusOptions", "A"),
                                           new SqlParameter("Criteria", criteria),
                                           new SqlParameter("Text", text),
                                           new SqlParameter("DateType", dateType),
                                           new SqlParameter("StartDate", startDate),
                                           new SqlParameter("EndDate", endDate),
                                           new SqlParameter("Type", "participant"),
                                           new SqlParameter("BasedDate", "Y")

                                        ).ToList();
                    

                    var locationReport = db.Database.SqlQuery<SP_Participant_Report>(
                              "SP_GetParticipantsReport @MeetingCode,@ControlBy,@StatusOptions, @Criteria, @Text, @DateType, @StartDate, @EndDate ,@Type",

                                         new SqlParameter("MeetingCode", meeting_code),
                                         new SqlParameter("ControlBy", (string)Session["user_id"]?.ToString()),

                                          new SqlParameter("StatusOptions", "A"),
                                          new SqlParameter("Criteria", criteria),
                                          new SqlParameter("Text", text),
                                          new SqlParameter("DateType", dateType),
                                          new SqlParameter("StartDate", startDate),
                                          new SqlParameter("EndDate", endDate),
                                          new SqlParameter("Type", "location")

                                       ).ToList();
                    var locationReportBasedDate = db.Database.SqlQuery<SP_Participant_Report>(
                              "SP_GetParticipantsReport @MeetingCode,@ControlBy,@StatusOptions, @Criteria, @Text, @DateType, @StartDate, @EndDate,@Type ,@BasedDate",

                                         new SqlParameter("MeetingCode", meeting_code),
                                         new SqlParameter("ControlBy", (string)Session["user_id"]?.ToString()),
                                          new SqlParameter("StatusOptions", "A"),
                                          new SqlParameter("Criteria", criteria),
                                          new SqlParameter("Text", text),
                                          new SqlParameter("DateType", dateType),
                                          new SqlParameter("StartDate", startDate),
                                          new SqlParameter("EndDate", endDate),
                                          new SqlParameter("Type", "location"),
                                          new SqlParameter("BasedDate", "Y")

                                       ).ToList();



                    var participantList = db.Database.SqlQuery<SP_GetParticipants_Result>(
                       "SP_GetParticipants @CertificateNo,@MeetingCode,@ControlBy,@StaffNo, @StatusOptions, @Criteria, @Text, @DateType, @StartDate, @EndDate," +
                       "@MeetingStartDate,@MeetingEndDate,@MeetingCondition",
                      new SqlParameter("CertificateNo", ""),
                      new SqlParameter("MeetingCode", meeting_code),
                      new SqlParameter("ControlBy", (string)Session["user_id"]?.ToString()),
                       new SqlParameter("StaffNo", ""),
                       new SqlParameter("StatusOptions", statusOptionsString),
                       new SqlParameter("Criteria", criteria),
                       new SqlParameter("Text", text),
                       new SqlParameter("DateType", dateType),
                       new SqlParameter("StartDate", startDate),
                       new SqlParameter("EndDate", endDate),
                       new SqlParameter("MeetingStartDate", ""),
                       new SqlParameter("MeetingEndDate", ""),
                       new SqlParameter("MeetingCondition", meeting_condition)
                       ).ToList();



                    //GRAF PARTICIPANT PART

                    var participantTop3 = participantReport
                    .GroupBy(item => item.total)
                    .Select(group => new { Participant = string.Join(", ", group.Select(item => item.staff_no + " (" + item.name + ")")), Total = group.Key })
                    
                    .ToList();

                    var participantNameTop3 = JsonConvert.SerializeObject(participantTop3.Select(item => item.Participant).ToArray());
                    var participantTotalTop3 = JsonConvert.SerializeObject(participantTop3.Select(item => item.Total).ToArray());
                    var participantTop3BasedDate = JsonConvert.SerializeObject(participantReportBasedDate
                    .GroupBy(item => item.staff_no)
                    
                    .Select(group => group
                        
                        .Select(item => new List<object> { item.staff_no + " (" + item.name + ")", item.joinedDate, item.total })
                    )
                    .ToList());



                    //GRAF LOCATION PART

                    var locationTop3 = locationReport
                    .GroupBy(item => item.total)
                    .Select(group => new { Location = string.Join(", ", group.Select(item => item.location)), Total = group.Key })
                  
                    .ToList();

                    var locationNameTop3 = JsonConvert.SerializeObject(locationTop3.Select(item => item.Location).ToArray());
                    var locationTotalTop3 = JsonConvert.SerializeObject(locationTop3.Select(item => item.Total).ToArray());
                    var locationTop3BasedDate = JsonConvert.SerializeObject(locationReportBasedDate
                     .GroupBy(item => item.location)
                    
                     .Select(group => group
                         
                         .Select(item => new List<object> { item.location, item.joinedDate, item.total })
                     )
                     .ToList());

                   var user_id= Session["user_id"]?.ToString();
                    List<sys_setting_define_table> defineTableList= db.sys_setting_define_table
                     .Where(item => item.createBy == user_id && item.module == "participant")
                     .ToList();
                    

                    var result = participantList.Select(p => new
                    {
                        link = ((participantAccess?.status == true && participantAccess?.viewFunction == true) || (participantAccess?.status == true && participantAccess?.editFunction == true))
                            ? $@"class='clickable-row' onclick=""window.location.href = '{Url.Action("MainForm", "Participant", new { id = p.certificate_no })}'"""
                            : "",
                        certificateNo=(defineTableList.Any(dt => dt.field == "certificate" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?p.certificate_no.ToUpper(): "NS",
                        staff_no = (defineTableList.Any(dt => dt.field == "staff_no" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?p.staff_no.ToUpper(): "NS",
                        name=(defineTableList.Any(dt => dt.field == "name" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?p.name: "NS",
                        factory = (defineTableList.Any(dt => dt.field == "factory" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?p.FactoryName + " (" + p.factory.ToUpper() + ")": "NS",
                        location = (defineTableList.Any(dt => dt.field == "location" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?p.LocationName + " (" + p.location.ToUpper() + ")": "NS",
                        department = (defineTableList.Any(dt => dt.field == "department" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?p.DepartmentName + " (" + p.department.ToUpper() + ")": "NS",
                        mobile = (defineTableList.Any(dt => dt.field == "mobile" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?p.mobile != null && !string.IsNullOrWhiteSpace(p.mobile) ? p.mobile : "<span class='text-danger'>No data</span>": "NS",
                        email = (defineTableList.Any(dt => dt.field == "email" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?p.email != null && !string.IsNullOrWhiteSpace(p.email) ? p.email : "<span class='text-danger'>No data</span>": "NS",
                        meeting = (defineTableList.Any(dt => dt.field == "joinMeeting" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?p.meeting_title + " (" + p.meeting_code.ToUpper() + ")": "NS",
                        
                        joinDate = (defineTableList.Any(dt => dt.field == "joinDate" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?"<span class='createDate'>" + p.createDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture) + "</span>": "NS",

                        edited_by = (defineTableList.Any(dt => dt.field == "editBy" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?p.EditByName != null && !string.IsNullOrWhiteSpace(p.editBy) ? p.EditByName + " (" + p.editBy.ToUpper() + ")" : "<span class='text-danger'>No data</span>": "NS",
                        edited_at = (defineTableList.Any(dt => dt.field == "editDate" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?p.editDate != null ? "<span class='editDate'>" + p.editDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture) + "</span>"
                                    : "<span class='text-danger'>No data</span>": "NS",
                        status = (participantAccess?.status == true && participantAccess?.deleteFunction == true) ?
                                    p.status == "A" ? "<span class='text-primary'>Active</span>"
                                    : p.status == "V" ? "<span class='text-danger'>Inactive</span>"
                                    : "<span class='text-warning'>Pending</span>"
                                    : "NS",
                        editBtn = (participantAccess?.status == true && participantAccess?.editFunction == true)
                                    ? $@"<a href=""{Url.Action("MainForm", "Participant", new { id = p.certificate_no })}"" class=""btn btn-primary"">Edit</a>"
                                    : (participantAccess?.status == true && participantAccess?.viewFunction == true)
                                    ? $@"<a href=""{Url.Action("MainForm", "Participant", new { id = p.certificate_no })}"" class=""btn btn-primary"">View</a>" : "",
                        statusBtn = (participantAccess?.status == true && participantAccess?.deleteFunction == true)|| (participantAccess?.status == true && participantAccess?.editFunction == true) ?
                                    p.status == "A"
                                    ? p.meeting_limit_status == true
                                    ? $@"<button style='width:6em;' class='btn btn-success  ml-1' onclick='updateMainStatus(""Participant"", ""{p.certificate_no}"", ""{p.status}"", ""{meeting_code}"", ""Participant Report"",""Y"", ""Participants"", ""{Session["timezone"]}"")'>Action</button>"
                                    : participantAccess.deleteFunction != true
                                    ? $@"<button style='width:6em; cursor:no-drop;' class='btn btn-dark  ml-1' onclick='updateMainStatus(""Participant"", ""{p.certificate_no}"", ""{p.status}"", ""{meeting_code}"", ""Participant Report"","""", ""Participants"", ""{Session["timezone"]}"")' disabled>Action</button>"
                                    : $@"<button style='width:6em;' class='btn btn-success  ml-1' onclick='updateMainStatus(""Participant"", ""{p.certificate_no}"", ""{p.status}"", ""{meeting_code}"", ""Participant Report"","""", ""Participants"", ""{Session["timezone"]}"")'>Action</button>"
                                    : p.status == "P"
                                    ? $@"<button style='width:6em;' class='btn btn-warning  ml-1' onclick='updateMainStatus(""Participant"", ""{p.certificate_no}"", ""{p.status}"", ""{meeting_code}"", ""Participant Report"",""Y"", ""Participants"", ""{Session["timezone"]}"")'>Action</button>"
                                    : $@"<button style='width:6em;' class='btn btn-danger  ml-1' onclick='updateMainStatus(""Participant"", ""{p.certificate_no}"", ""{p.status}"", ""{meeting_code}"", ""Participant Report"","""", ""Participants"", ""{Session["timezone"]}"")'>Action</button>"
                                    : ""
                    });
                    
                    var participantReportData = participantReport.Select(p => new
                    {
                        link = ((participantAccess?.status == true && participantAccess?.viewFunction == true) || (participantAccess?.status == true && participantAccess?.editFunction == true))
                            ? $@"class='clickable-row' onclick=""window.location.href = '{Url.Action("ParticipantDetail", "Participant", new { id = p.staff_no })}'"""
                            : "",
                        staff_no = p.staff_no.ToUpper(),
                        factory = p.factory,
                        p.name,
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

                  


                    


                    return Json(new
                    {
                        result = result,
                        
                        participantReportData = participantReportData,
                       
                        locationReportData = locationReportData,
                       
                        locationNameTop3 = locationNameTop3,
                        locationTotalTop3 = locationTotalTop3,
                        locationTop3BasedDate = locationTop3BasedDate,
                        participantNameTop3 = participantNameTop3,
                        participantTotalTop3 = participantTotalTop3,
                        participantTop3BasedDate = participantTop3BasedDate,
                        grafType = "doughnut",
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


        /*------------------------------------------Meeting History START ------------------------------------------*/
        public ActionResult MeetingHistory(string id)
        {
            try
            {
                using (var db = new AttendNow_DBEntities())
                {

                    if (id == null)
                    {
                        return RedirectToAction("NotFound", "Home");

                    }

                    var participants = db.Database.SqlQuery<SP_GetParticipants_Result>(
                                "SP_GetParticipants @CertificateNo,@MeetingCode,@ControlBy,@StaffNo,@StatusOptions, @Criteria, @Text, @DateType, @StartDate, @EndDate," +
                       "@MeetingStartDate,@MeetingEndDate,@MeetingCondition",
                                new SqlParameter("CertificateNo", id),
                                new SqlParameter("MeetingCode", ""),
                                new SqlParameter("ControlBy", ""),
                                new SqlParameter("StaffNo", ""),
                                new SqlParameter("StatusOptions", "A,P"),
                                   new SqlParameter("Criteria", ""),
                                   new SqlParameter("Text", ""),
                                   new SqlParameter("DateType", ""),
                                   new SqlParameter("StartDate", ""),
                                   new SqlParameter("EndDate", ""),
                                   new SqlParameter("MeetingStartDate", ""),
                                   new SqlParameter("MeetingEndDate", ""),
                                   new SqlParameter("MeetingCondition", "E")
                                ).FirstOrDefault();
                    if (participants == null)
                    {
                        return RedirectToAction("NotFound", "Home");

                    }

                    string eventName = participants.meeting_title;

                    string location = participants.meeting_place;
                    string meeting_link = participants.meeting_link == null ? "" : $"\n\n<b>Meeting Online URL: </b> {participants.meeting_link} ";
                    string remark = participants.remark == null ? "" : $"\n\n<b>Remark: </b> {participants.meeting_remark}";
                    string description = $"<b>Meeting History URL:</b>" +
                        $"\n\n<a href='{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action("MeetingHistory", new { id = participants.certificate_no })}'>" +
                        $"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action("MeetingHistory", new { id = participants.certificate_no })}</a>" +
                        $"\n\n<b>Presenter: </b>{participants.meeting_organizer} ({participants.meeting_DepartmentName}, {participants.meeting_LocationName})" +
                       meeting_link
                         +
                         remark
                         +
                        $"\n\n<b>Meeting Details / Properties: </b>\n{participants.meeting_detail}";



                                        string googleCalendarUrl = $"https://www.google.com/calendar/render?action=TEMPLATE&text={Uri.EscapeDataString(eventName)}" +
                        $"&dates={participants.meeting_startDate:yyyyMMddTHHmmss}/{participants.meeting_endDate:yyyyMMddTHHmmss}&location={Uri.EscapeDataString(location)}" +
                        $"&details={Uri.EscapeDataString(description)}&ctz=Asia/Kuala_Lumpur";

                    ViewBag.googleCalendarUrl = googleCalendarUrl;
                    ViewBag.QR_MeetingHistory= HomeController.GenerateQrCode($"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Action("MeetingHistory", new { id = participants.certificate_no })}");
                    if (participants.meeting_link != null)
                    {
                        ViewBag.QR_OnlineMeeting = HomeController.GenerateQrCode($"{participants.meeting_link}");

                    }



                    return View(participants);
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
        /*------------------------------------------Meeting History END ------------------------------------------*/







 

        /*------------------------------------------Send Email START------------------------------------------*/
        private void SendEmailToParticipant(string certificateNo,string pending)
        {

            using (var db = new AttendNow_DBEntities())
            {
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                var participants = db.Database.SqlQuery<SP_GetParticipants_Result>(
                                 "SP_GetParticipants @CertificateNo ",
                                 new SqlParameter("CertificateNo", certificateNo)
                                ).FirstOrDefault();
                string smtpUsername = ConfigurationManager.AppSettings["EmailAddress"];
                string smtpPassword = ConfigurationManager.AppSettings["EmailPassword"];
                var fromAddress = new MailAddress(smtpUsername, "AttendNow"); // NEED TO CHANGE EMAIL ACCOUNT
                var toAddress = new MailAddress(participants.email, participants.name);
                var subject = "AttendNow Email Notification";

                var title = "";
                var inform = "";
                if (pending == "A")
                {
                    title = "Thank You for Joining " + textInfo.ToTitleCase(participants.meeting_title);
                    inform = "Thanks for joining " + textInfo.ToTitleCase(participants.meeting_title) + "." +
                        " This email serves as a record of your meeting participation.";
                }else if (pending == "RA")
                {
                    title = "[Approved] Thank You for Joining " + textInfo.ToTitleCase(participants.meeting_title);
                    inform = "Thanks for joining " + textInfo.ToTitleCase(participants.meeting_title) + "." +
                        " You are being approved to join us. This email serves as a record of your meeting participation.";

                }else if (pending == "P")
                {
                    title = "Registration for Joining " + textInfo.ToTitleCase(participants.meeting_title)+" unsuccessful";
                    inform = "Registration unsuccessful for joining " + textInfo.ToTitleCase(participants.meeting_title) + 
                        " due to full capacity, but your record has been saved in our system. If there are any changes, we will update with you.";

                }else if (pending == "TP")
                {
                    title = "[Rejected] Sorry to Inform You";
                    inform = "Your Registration for Joining " + textInfo.ToTitleCase(participants.meeting_title) + 
                        "has been rejected due to some specific cause, but your record has been saved in our system. If there are any " +
                        "changes, we will update with you.";
 
                }
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
                                                          <h1 >Hi " + textInfo.ToTitleCase(participants.name) + @",</h1>
                               
                                                          <p>" + inform + @"</p>
                              
                                                          <!-- Action -->
                              
                                                          <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                                            <tr>
                                                              <td colspan = ""2"" >
                                                                <table  width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                      
                                                                  <br>
                                                                  
                                                                  <h3>Your Information</h3>
                                      
                                                                  <tr>
                                                                      <td width = ""40%"" ><span >Staff No.</span></td>
                                                                      <td  width=""60%"" ><span >: " + participants.staff_no.ToUpper() + @"</span></td>
                                      
                                                                  </tr>
                                                                    </br>
                                                                   <tr>
                                                                      <td width = ""40%"" ><span >Name</span></td>
                                                                      <td  width=""60%"" ><span >: " + textInfo.ToTitleCase(participants.name) + @"</span></td>
                                      
                                                                  </tr>

                                                                      </br>
                                                                   <tr>
                                                                      <td width = ""40%"" ><span >Department</span></td>
                                                                      <td  width=""60%"" ><span >: " + textInfo.ToTitleCase(participants.DepartmentName) + @" (" + participants.department.ToUpper() + @")</span></td>
                                      
                                                                  </tr>

                                                                      </br>
                                                                   <tr>
                                                                      <td width = ""40%"" ><span >Location</span></td>
                                                                     <td  width=""60%"" ><span >: " + textInfo.ToTitleCase(participants.LocationName) + @" (" + participants.location.ToUpper() + @")</span></td>
                                      
                                                                  </tr>
                                                                      </br>
                                                                   <tr>
                                                                      <td width = ""40%"" ><span >Email</span></td>
                                                                       <td  width=""60%"" ><span >: " + participants.email + @"</span></td>
                                      
                                                                  </tr>
                                                                      </br>
                                                                   <tr>
                                                                      <td width = ""40%"" ><span >Mobile</span></td>
                                                                      <td  width=""60%"" ><span >: " + participants.mobile + @"</span></td>
                                      
                                                                  </tr>
                                                                      </br>
                                                                   <tr>
                                                                      <td width = ""40%"" ><span >Certificate No.</span></td>
                                                                      <td  width=""60%"" ><span >: " + participants.certificate_no + @"</span></td>
                                      
                                                                  </tr>
                                                                      </br>
                                                                   <tr>
                                                                      <td width = ""40%"" ><span >Meeting Participation Record URL </span></td>
                                                                        <td  width=""60%"" ><span >: " + Request.Url.Scheme + @"://" + Request.Url.Authority + Url.Action("MeetingHistory", new { id = participants.certificate_no }) + @"</span></td>
                                         
                                                                  </tr>
                                                            <tr>
                                                           
                                                              <td colspan = ""2"" >
                                                                <table  width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                      
                                                                  <br>
                                                                 
                                                                  <h3>Meeting Information</h3>
                                      
                                                                  <tr>
                                                                      <td width = ""40%"" ><span >Meeting Code</span></td>
                                                                      <td  width=""60%"" ><span >: " + participants.meeting_code + @"</span></td>
                                      
                                                                  </tr>
                                                                      </br>
                                                                   <tr>
                                                                      <td width = ""40%"" ><span >Meeting Title </span></td>
                                                                    <td  width=""60%"" ><span >: " + textInfo.ToTitleCase(participants.meeting_title) + @"</span></td>
                                         
                                                                  </tr>
                                                                      </br>
                                                                   <tr>
                                                                      <td width = ""40%"" ><span >Meeting Presenter </span></td>
                                                                    <td  width=""60%"" ><span >: " + textInfo.ToTitleCase(participants.meeting_organizer) + @"</span></td>
                                         
                                                                  </tr>
                                                                      </br>
                                                                   <tr>
                                                                      <td width = ""40%"" ><span >Meeting Start Time </span></td>
                                                                    <td  width=""60%"" ><span >: (UTC +8:00) " + participants.meeting_startDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture) + @"</span></td>
                                         
                                                                  </tr>
                                                                      </br>
                                                                   <tr>
                                                                      <td width = ""40%"" ><span >Meeting End Time </span></td>
                                                                    <td  width=""60%"" ><span >: (UTC +8:00) " + participants.meeting_endDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture) + @"</span></td>
                                         
                                                                  </tr>
                                                                      </br>
                                                                   <tr>
                                                                      <td width = ""40%"" ><span >Meeting Type </span></td>
                                                                   <td width=""60%""><span>: " + (participants.meeting_type == true ? "Private" : "Public") + @"</span></td>
                                         
                                                                  </tr>
                                                                      </br>
                                                                   <tr>
                                                                      <td width = ""40%"" ><span >Meeting Remark </span></td>
                                                                    <td  width=""60%"" ><span > " + participants.meeting_remark + @"</span></td>
                                         
                                                                  </tr>";

                if (!string.IsNullOrEmpty(participants.meeting_place) && !string.IsNullOrEmpty(participants.meeting_link))
                {
                    body += @"
                                                                            </br>
                                                                               <tr>
                                                                                  <td width = ""40%"" ><span >Meeting Location </span></td>
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
                                                                                  <td width = ""40%"" ><span >Meeting Location </span></td>
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




                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com", // Your SMTP host
                    Port = 587,                // Your SMTP port
                    EnableSsl = true,          // Enable SSL if required by your email provider
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword)// NEED TO CHANGE EMAIL ACCOUNT
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

            }

        }
        /*------------------------------------------Send Email END------------------------------------------*/


        /*------------------------------------------Security Cryptography START------------------------------------------*/

       /* private long StringTo8ByteValue(string inputString)
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
        }*/

/*
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
        }*/

        /*------------------------------------------Security Cryptography END------------------------------------------*/

    }
}
