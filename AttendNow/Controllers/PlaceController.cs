using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using AttendNow.Models;
using AttendNow.Models.ViewModel;

namespace AttendNow.Controllers
{
    public class PlaceController : Controller
    {
        

        // GET: Place
        public ActionResult Index()
        {
            
            try
            {
                
                using (var db = new AttendNow_DBEntities())
                {

                    
                    ViewBag.Title = "Venue List";
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
                    var statusOptionsString = "";
                    if (meetingAccess.deleteFunction != true)
                    {
                        statusOptionsString = "A";
                    }
                    //PLACE LIST
                    var placeList = db.Database.SqlQuery<SP_GetPlace_Result>(
                        "SP_GetPlace @PlaceCode ,@StatusOptions",
                        new SqlParameter("PlaceCode", ""),
                        new SqlParameter("StatusOptions", statusOptionsString)).ToList();
                    var user_id = Session["user_id"]?.ToString();
                    ViewBag.DefineTable = db.sys_setting_define_table
                     .Where(item => item.createBy == user_id && item.module == "place")
                     .ToList();
                    TempData["page"] = "placelist";
                    ViewBag.dataDate = "Month " + DateTime.Now.Month.ToString();
                    
                    return View(placeList);
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

        // GET: Place/MainForm/ :FOR ADD Place 
        // GET: Place/MainForm/5 :FOR EDIT Place 
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
                    if (id == null)
                    {
                        ViewBag.Title = "Add Venue";
                        TempData["page"] = "addplace"; //Page will be add function
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
                                ViewBag.Title = "Edit Venue";
                            }
                            else //View Function
                            {
                                ViewBag.Title = "View Venue";
                            }

                            TempData["page"] = "placelist";

                            //pass factory to view in dropdown format
                            var activeFactory = db.Database.SqlQuery<SP_GetFactorys_Result>(
                               "SP_GetFactorys @Factory_id ,@StatusOptions",
                               new SqlParameter("Factory_id", ""),
                               new SqlParameter("StatusOptions", "A")).ToList();
                            ViewBag.FactoryList = activeFactory;

                            var userFactory =
                                   db.Database.SqlQuery<string>("SP_PlaceConnectFactory @PlaceCode", new SqlParameter("PlaceCode", id)).ToList();
                            // "BP", "KULAI", and "JB"
                            string factoryValues = string.Join(",", userFactory);//"BP,KULAI,JB"
                            ViewBag.FactoryValues = factoryValues;

                            //get specific user based on id
                            var place = db.Database.SqlQuery<SP_GetPlace_Result>(
                                "SP_GetPlace @PlaceCode",
                                new SqlParameter("PlaceCode", id)

                            ).FirstOrDefault();

                            if (place == null)
                            {
                                return RedirectToAction("NotFound", "Home");
                            }
                            else
                            {
                                return View(place);
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



        // POST: Place/Create
        [HttpPost]
        public JsonResult Create(tbl_place PlaceData)
        {
            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    if (string.IsNullOrEmpty(PlaceData.name) || string.IsNullOrEmpty(PlaceData.place_code))
                    {
                        return Json(new { success = false, error_message = "Make sure all fields are filled" });
                    }
                    if (db.tbl_PlaceFactory.Any(p => p.place.ToUpper() == PlaceData.place_code.ToUpper() ))

                    {
                        return Json(new { success = false, error_message = "This Venue ID is already in use.",idError = "This Venue ID is already in use." });
                    }
                    //Set the place data
                    PlaceData.place_code = PlaceData.place_code.ToUpper();
                    PlaceData.status = "A";
                    PlaceData.createDate = DateTime.Now;
                    PlaceData.createBy = (string)Session["user_id"];

                    


                    //Insert new place into DB
                    db.tbl_place.Add(PlaceData);
                   
                    db.SaveChanges();

                    

                    TempData["SuccessMessage"] = "Added Successfully.";

                    return Json(new { success = true, message = "Added Successfully." });

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


        // POST: Place/Edit
        [HttpPost]
        public JsonResult Edit(tbl_place PlaceData)
        {
            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    if (string.IsNullOrEmpty(PlaceData.name))
                    {
                        return Json(new { success = false, error_message = "Make sure all fields are filled" });
                    }

                    var existingPlace = db.tbl_place.SingleOrDefault(p => p.place_code.ToUpper() == PlaceData.place_code.ToUpper());
                    if (existingPlace != null)

                    {
                        // Update the fields you want to change
                        existingPlace.name = PlaceData.name;
                        existingPlace.editBy = (string)Session["user_id"];
                        existingPlace.editDate = DateTime.Now;
                        db.Entry(existingPlace).State = EntityState.Modified;
                        db.SaveChanges();

                        TempData["SuccessMessage"] = "Updated Successfully.";
                        return Json(new { success = true, message = "Updated Successfully." });


                    }
                    else
                    {
                        return Json(new { success = false, error_message = "Not Found !" });
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


        /*------------------------------------------UpdateStatus Function START------------------------------------------*/
        public JsonResult UpdateStatus(String id, string newStatus)
        {

            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    // Find the existing user in the database by user_id
                    var existingPlace = db.tbl_place.SingleOrDefault(u => u.place_code.ToUpper() == id.ToUpper());

                    if (existingPlace != null)
                    {
                        // Update the status property based on the newStatus parameter
                        existingPlace.status = newStatus;
                        existingPlace.editBy = (string)Session["user_id"];
                        existingPlace.editDate = DateTime.Now;
                        var existingControlUser = db.Database.SqlQuery<SP_GetUserProfiles_Result>("SP_GetUserProfiles @StaffNo",
                                                   new SqlParameter("StaffNo", (string)Session["user_id"])
                                               ).FirstOrDefault();

                        var userEditBy = existingControlUser.EditByName + " (" + existingControlUser.editBy.ToUpper() + ")";
                        var editDate = existingPlace.editDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                        db.Entry(existingPlace).State = EntityState.Modified;
                        db.SaveChanges();


                        return Json(new { success = true,
                            userEditBy = userEditBy,
                            userEditDate = editDate, message = "Updated Successfully." });
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

        /*------------------------------------------Manage Connect Function START------------------------------------------*/
        [HttpPost]
        public JsonResult RemoveFactory(List<string> model, string place) //delete access place function
        {
            try
            {
               
                using (var db = new AttendNow_DBEntities())
                {

                    foreach (var data in model)
                    {

                        var existingFactory = db.tbl_PlaceFactory.FirstOrDefault(pf => pf.place == place && pf.factory == data && pf.status != "V");
                        if (existingFactory != null)
                        {
                            existingFactory.status = "V";
                            existingFactory.editBy = (string)Session["user_id"];
                            existingFactory.editDate = DateTime.Now;
                           
                        }
                        
                    }

                    var existingPlace = db.tbl_place.SingleOrDefault(u => u.place_code.ToUpper() == place.ToUpper());

                    if (existingPlace != null)
                    {

                        existingPlace.editDate = DateTime.Now;
                        existingPlace.editBy = (string)Session["user_id"];
                    }
                    // Save changes to the database
                    db.SaveChanges();
                    var existingControlUser = db.Database.SqlQuery<SP_GetUserProfiles_Result>("SP_GetUserProfiles @StaffNo",
                                                   new SqlParameter("StaffNo", (string)Session["user_id"])
                                               ).FirstOrDefault();

                    var userEditBy = existingControlUser.EditByName + " (" + existingControlUser.editBy.ToUpper() + ")";
                    var editDate = existingPlace.editDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                 


                    return Json(new
                    {
                        success = true,
                        message = "Removed Factory Sucessfully",
                        
                        userEditBy = userEditBy,
                        userEditDate = editDate,
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
        public JsonResult AddFactory(List<string> model, string place, string type) //delete access place function
        {
            try
            {
                List<SP_GetPlaceWithFactory_Result> activePlace = null;

                using (var db = new AttendNow_DBEntities())
                {
                    List<tbl_PlaceFactory> connectFactory = new List<tbl_PlaceFactory>();

                    // Populate the list based on the model data
                    foreach (var data in model)
                    {
                        var existingFactory = db.tbl_PlaceFactory.FirstOrDefault(pf => pf.factory.ToUpper() == data.ToUpper() && pf.place == place);
                        if (existingFactory != null)
                        {
                            existingFactory.status = "A";
                            existingFactory.editBy = (string)Session["user_id"]; ;
                            existingFactory.editDate = DateTime.Now;
                        }
                        else
                        {
                            connectFactory.Add(new tbl_PlaceFactory
                            {
                                factory = data,
                                place = place,
                                createBy = (string)Session["user_id"],
                                createDate = DateTime.Now,
                                status = "A"

                            });
                        }

                    }
                    db.tbl_PlaceFactory.AddRange(connectFactory);
                    // Find the entity in the database using LINQ
                   

                    var existingPlace = db.tbl_place.SingleOrDefault(u => u.place_code.ToUpper() == place.ToUpper());

                    if (existingPlace != null)
                    {

                        existingPlace.editDate = DateTime.Now;
                        existingPlace.editBy = (string)Session["user_id"];
                    }
                    // Save changes to the database
                    db.SaveChanges();
                    var existingControlUser = db.Database.SqlQuery<SP_GetUserProfiles_Result>("SP_GetUserProfiles @StaffNo",
                                                   new SqlParameter("StaffNo", (string)Session["user_id"])
                                               ).FirstOrDefault();

                    var userEditBy = existingControlUser.EditByName + " (" + existingControlUser.editBy.ToUpper() + ")";
                    var editDate = existingPlace.editDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                    

                    return Json(new
                    {
                        success = true,
                        message = "Added Factory Sucessfully",
                        activePlace = activePlace,
                        userEditBy = userEditBy,
                        userEditDate = editDate,
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
        /*------------------------------------------Manage Connect Function END------------------------------------------*/

        /*------------------------------------------Filter or Search Function START------------------------------------------*/
        [HttpPost]
        public ActionResult Search(List<string> statusOptions, string criteria, string text, string dateType, string startDate, string endDate)
        {
            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    string statusOptionsString;
                    if (statusOptions == null)
                    {
                        statusOptionsString = "";
                    }
                    else
                    {
                        statusOptionsString = string.Join(",", statusOptions);
                    }
                    if (startDate != "" || endDate != "")
                    {
                        DateTime serverStartDateTime = DateTime.ParseExact(startDate, "yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);
                        DateTime serverEndDateTime = DateTime.ParseExact(endDate, "yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture).Date.AddHours(23).AddMinutes(59);

                        if ((string)Session["timezone"] == "V")
                        {

                            // Assume serverStartDateTime is in Thailand timezone
                            TimeZoneInfo thailandTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                            DateTime utcStartDate = TimeZoneInfo.ConvertTimeToUtc(serverStartDateTime, thailandTimeZone);
                            DateTime utcEndDate = TimeZoneInfo.ConvertTimeToUtc(serverEndDateTime, thailandTimeZone);

                            // Convert to Server timezone
                            TimeZoneInfo serverTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                            startDate = TimeZoneInfo.ConvertTime(utcStartDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");
                            endDate = TimeZoneInfo.ConvertTime(utcEndDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");

                        }
                        else if ((string)Session["timezone"] == "J")
                        {

                            // Assume serverStartDateTime is in Jordan timezone
                            TimeZoneInfo jordanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Jordan Standard Time");
                            DateTime utcStartDate = TimeZoneInfo.ConvertTimeToUtc(serverStartDateTime, jordanTimeZone);
                            DateTime utcEndDate = TimeZoneInfo.ConvertTimeToUtc(serverEndDateTime, jordanTimeZone);

                            // Convert to Server timezone
                            TimeZoneInfo serverTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                            startDate = TimeZoneInfo.ConvertTime(utcStartDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");
                            endDate = TimeZoneInfo.ConvertTime(utcEndDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");

                        }
                        else if ((string)Session["timezone"] == "M")
                        {
                            // Assume serverStartDateTime is in Singapore timezone
                            TimeZoneInfo singaporeTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                            DateTime utcStartDate = TimeZoneInfo.ConvertTimeToUtc(serverStartDateTime, singaporeTimeZone);
                            DateTime utcEndDate = TimeZoneInfo.ConvertTimeToUtc(serverEndDateTime, singaporeTimeZone);

                            // Convert to Server timezone
                            TimeZoneInfo serverTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                            startDate = TimeZoneInfo.ConvertTime(utcStartDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");
                            endDate = TimeZoneInfo.ConvertTime(utcEndDate, serverTimeZone).ToString("yyyy-MM-dd HH:mm:ss.fff");
                        }
                    }
                    var roleID = Session["role_id"]?.ToString();
                    var meetingAccess = db.sys_setting_role_permission.SingleOrDefault(rp => rp.role == roleID && rp.module == "meeting");
                    if (meetingAccess.deleteFunction != true)
                    {
                        statusOptionsString = "A";
                    }
                    var placeList = db.Database.SqlQuery<SP_GetPlace_Result>(
                "SP_GetPlace @PlaceCode, @StatusOptions, @Criteria, @Text, @DateType, @StartDate, @EndDate",
                        new SqlParameter("PlaceCode", ""),
                        new SqlParameter("StatusOptions", statusOptionsString),
                        new SqlParameter("Criteria", criteria),
                        new SqlParameter("Text", text),
                        new SqlParameter("DateType", dateType),
                        new SqlParameter("StartDate", startDate),
                        new SqlParameter("EndDate", endDate)
                    ).ToList();

                    
                    var user_id= Session["user_id"]?.ToString();
                    List<sys_setting_define_table> defineTableList= db.sys_setting_define_table
                     .Where(item => item.createBy == user_id && item.module == "place")
                     .ToList();
                    
                    ViewBag.meetingAccess = meetingAccess;
                    var result = placeList.Select(u => new
                    {

                        link = ((meetingAccess?.status == true && meetingAccess?.viewFunction == true) || (meetingAccess?.status == true && meetingAccess?.editFunction == true))
                    ? $@"class='clickable-row' onclick=""window.location.href = '{Url.Action("MainForm", "Place", new { id = u.place_code })}'"""
                    : "",

                        place = (defineTableList.Any(dt => dt.field == "place_code" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.place_code.ToUpper(): "NS",
                        name = (defineTableList.Any(dt => dt.field == "name" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.name: "NS",
                       
                        created_by = (defineTableList.Any(dt => dt.field == "createBy" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.CreateByName + " (" + u.createBy.ToUpper() + ")": "NS",
                        createdAt = (defineTableList.Any(dt => dt.field == "createDate" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?"<span class='createDate'>" + u.createDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture) + "</span>": "NS",

                        edited_by = (defineTableList.Any(dt => dt.field == "editBy" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.editBy != null && u.editBy != "" ? u.EditByName + " (" + u.editBy.ToUpper() + ")" : "<span class='text-danger'>No data</span>": "NS",

                        edited_at = (defineTableList.Any(dt => dt.field == "editDate" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.editDate != null ? "<span class='editDate'>" + u.editDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture) + "</span>"
                                    : "<span class='text-danger'>No data</span>": "NS",

                        status = (meetingAccess?.status == true && meetingAccess?.deleteFunction == true) ? 
                                u.status == "A" ? "<span class='text-primary'>Active</span>"
                                : "<span class='text-danger'>Inactive</span>"
                                :"NS",
                        editBtn = (meetingAccess?.status == true && meetingAccess?.editFunction == true)
                             ? $@"<a href=""{Url.Action("MainForm", "Place", new { id = u.place_code })}"" class=""btn btn-primary"">Edit</a>"
                            : (meetingAccess?.status == true && meetingAccess?.viewFunction == true)
                            ? $@"<a href=""{Url.Action("MainForm", "Place", new { id = u.place_code })}"" class=""btn btn-primary"">View</a>" : "",

                        statusBtn = (meetingAccess?.status == true && meetingAccess?.deleteFunction == true) ?
                            u.status == "A"
                            ? $@"<button style='width:6em;' class='btn btn-danger  ml-1' onclick='updateStatus(""Place"", ""{u.place_code}"", ""{u.status}"","""", ""Venue Report"", ""Venues"", ""{Session["timezone"]}"")'>Inactive</button>"
                            : $@"<button style='width:6em;' class='btn btn-info  ml-1' onclick='updateStatus(""Place"", ""{u.place_code}"", ""{u.status}"","""", ""Venue Report"", ""Venues"", ""{Session["timezone"]}"")'>Reactive</button>"
                            : ""

                    });
                   

                    return Json(new
                    {
                        //pass datatable data
                        result = result,
                       
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


    }
}
