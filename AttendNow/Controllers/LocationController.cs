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

namespace AttendNow.Controllers
{
    public class LocationController : Controller
    {
        

        // GET: Location
        public ActionResult Index()
        {
            
            try
            {
                
                using (var db = new AttendNow_DBEntities())
                {

                    
                    ViewBag.Title = "Location List";
                    var roleID = Session["role_id"]?.ToString();
                    var locationAccess = db.sys_setting_role_permission.SingleOrDefault(rp => rp.role == roleID && rp.module == "location");
                    ViewBag.locationAccess = locationAccess;
                    if (locationAccess?.status != true)
                    {
                        return RedirectToAction("Logout", "User");
                    }
                    if (Session["user_id"] == null)
                    {
                        return RedirectToAction("Logout", "User");
                    }
                    var statusOptionsString = "";
                    if (locationAccess.deleteFunction != true)
                    {
                        statusOptionsString = "A";
                    }
                    var user_id = Session["user_id"]?.ToString();
                    ViewBag.DefineTable = db.sys_setting_define_table
                     .Where(item => item.createBy == user_id && item.module == "location")
                     .ToList();
                    var locations = db.Database.SqlQuery<SP_GetLocations_Result>(
                        "SP_GetLocations @LocationCode ,@StatusOptions",
                        new SqlParameter("LocationCode", ""),
                        new SqlParameter("StatusOptions", statusOptionsString)).ToList();

                    TempData["page"] = "locationlist";
                    return View(locations);
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
       
        // GET: Location/MainForm/ :FOR ADD Location 
        // GET: Location/MainForm/5 :FOR EDIT Location 
        public ActionResult MainForm(string id)
        {
            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    var roleID = Session["role_id"]?.ToString();
                    var locationAccess = db.sys_setting_role_permission.SingleOrDefault(rp => rp.role == roleID && rp.module == "location");
                    ViewBag.locationAccess = locationAccess;
                   
                    if (locationAccess?.status != true) //If role permission status is false then logout
                    {
                        return RedirectToAction("Logout", "User");
                    }
                    if (id == null)
                    {
                        ViewBag.Title = "Add Location";
                        TempData["page"] = "addlocation"; //Page will be add function
                        if (locationAccess?.status != true || (locationAccess?.addFunction != true)) // If role permission(ADD) status is false then logout
                        {
                            return RedirectToAction("Logout", "User");
                        }
                        return View();
                    }
                    else
                    {
                        if (locationAccess?.status != true ||
                        (locationAccess?.editFunction != true && locationAccess?.viewFunction != true))
                        // If role permission(EDIT||VIEW) status is false then logout
                        {
                            return RedirectToAction("Logout", "User");
                        }
                        else
                        {
                            if (locationAccess?.editFunction == true) //Edit Function
                            {
                                ViewBag.Title = "Edit Location";
                            }
                            else //View Function
                            {
                                ViewBag.Title = "View Location";
                            }

                            TempData["page"] = "locationlist";

                            //pass factory to view in dropdown format
                            var activeFactory = db.Database.SqlQuery<SP_GetFactorys_Result>(
                               "SP_GetFactorys @Factory_id ,@StatusOptions",
                               new SqlParameter("Factory_id", ""),
                               new SqlParameter("StatusOptions", "A")).ToList();
                            ViewBag.FactoryList = activeFactory;

                            var userFactory =
                                   db.Database.SqlQuery<string>("SP_LocationConnectFactory @LocationCode", new SqlParameter("LocationCode", id)).ToList();
                            // "BP", "KULAI", and "JB"
                            string factoryValues = string.Join(",", userFactory);//"BP,KULAI,JB"
                            ViewBag.FactoryValues = factoryValues;

                            //get specific user based on id
                            var location = db.Database.SqlQuery<SP_GetLocations_Result>(
                                "SP_GetLocations @LocationCode",
                                new SqlParameter("LocationCode", id)

                            ).FirstOrDefault();

                            if (location == null)
                            {
                                return RedirectToAction("NotFound", "Home");
                            }
                            else
                            {
                                return View(location);
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



        // POST: Location/Create
        [HttpPost]
        public JsonResult Create(tbl_location LocationData)
        {
            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    if (string.IsNullOrEmpty(LocationData.name) || string.IsNullOrEmpty(LocationData.location_code))
                    {
                        return Json(new { success = false, error_message = "Make sure all fields are filled" });
                    }
                    if (db.tbl_LocationFactory.Any(l => l.location.ToUpper() == LocationData.location_code.ToUpper() ))

                    {
                        return Json(new { success = false, error_message = "This Location Code is already in use.",idError = "This Location Code is already in use." });
                    }
                    //Set the location data
                    LocationData.location_code = LocationData.location_code.ToUpper();
                    LocationData.status = "A";
                    LocationData.createDate = DateTime.Now;
                    LocationData.createBy = (string)Session["user_id"];

                    


                    //Insert new location into DB
                    db.tbl_location.Add(LocationData);
                   
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


        // POST: Location/Edit
        [HttpPost]
        public JsonResult Edit(tbl_location LocationData)
        {
            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    if (string.IsNullOrEmpty(LocationData.name))
                    {
                        return Json(new { success = false, error_message = "Make sure all fields are filled" });
                    }

                    var existingLocation = db.tbl_location.SingleOrDefault(l => l.location_code.ToUpper() == LocationData.location_code.ToUpper());
                    if (existingLocation != null)

                    {
                        // Update the fields you want to change
                        existingLocation.name = LocationData.name;
                        existingLocation.editBy = (string)Session["user_id"];
                        existingLocation.editDate = DateTime.Now;
                        db.Entry(existingLocation).State = EntityState.Modified;
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
                    var existingLocation = db.tbl_location.SingleOrDefault(u => u.location_code.ToUpper() == id.ToUpper());

                    if (existingLocation != null)
                    {
                        // Update the status property based on the newStatus parameter
                        existingLocation.status = newStatus;
                        existingLocation.editBy = (string)Session["user_id"];
                        existingLocation.editDate = DateTime.Now;
                        var existingControlUser = db.Database.SqlQuery<SP_GetUserProfiles_Result>("SP_GetUserProfiles @StaffNo",
                                                   new SqlParameter("StaffNo", (string)Session["user_id"])
                                               ).FirstOrDefault();

                        var userEditBy = existingControlUser.EditByName + " (" + existingControlUser.editBy.ToUpper() + ")";
                        var editDate = existingLocation.editDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                        db.Entry(existingLocation).State = EntityState.Modified;
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
        public JsonResult RemoveFactory(List<string> model, string location) //delete access location function
        {
            try
            {
               
                using (var db = new AttendNow_DBEntities())
                {

                    foreach (var data in model)
                    {

                        var existingFactory = db.tbl_LocationFactory.FirstOrDefault(lf => lf.location == location && lf.factory == data && lf.status != "V");
                        if (existingFactory != null)
                        {
                            existingFactory.status = "V";
                            existingFactory.editBy = (string)Session["user_id"];
                            existingFactory.editDate = DateTime.Now;
                           
                        }
                        
                    }

                    var existingLocation = db.tbl_location.SingleOrDefault(u => u.location_code.ToUpper() == location.ToUpper());

                    if (existingLocation != null)
                    {

                        existingLocation.editDate = DateTime.Now;
                        existingLocation.editBy = (string)Session["user_id"];
                    }
                    // Save changes to the database
                    db.SaveChanges();
                    var existingControlUser = db.Database.SqlQuery<SP_GetUserProfiles_Result>("SP_GetUserProfiles @StaffNo",
                                                   new SqlParameter("StaffNo", (string)Session["user_id"])
                                               ).FirstOrDefault();

                    var userEditBy = existingControlUser.EditByName + " (" + existingControlUser.editBy.ToUpper() + ")";
                    var editDate = existingLocation.editDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                 


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
        public JsonResult AddFactory(List<string> model, string location, string type) //delete access location function
        {
            try
            {
                List<SP_GetLocationsWithFactory_Result> activeLocation = null;

                using (var db = new AttendNow_DBEntities())
                {
                    List<tbl_LocationFactory> connectFactory = new List<tbl_LocationFactory>();

                    // Populate the list based on the model data
                    foreach (var data in model)
                    {
                        var existingFactory = db.tbl_LocationFactory.FirstOrDefault(lf => lf.factory.ToUpper() == data.ToUpper() && lf.location == location);
                        if (existingFactory != null)
                        {
                            existingFactory.status = "A";
                            existingFactory.editBy = (string)Session["user_id"]; ;
                            existingFactory.editDate = DateTime.Now;
                        }
                        else
                        {
                            connectFactory.Add(new tbl_LocationFactory
                            {
                                factory = data,
                                location = location,
                                createBy = (string)Session["user_id"],
                                createDate = DateTime.Now,
                                status = "A"

                            });
                        }

                    }
                    db.tbl_LocationFactory.AddRange(connectFactory);
                    // Find the entity in the database using LINQ
                   

                    var existingLocation = db.tbl_location.SingleOrDefault(u => u.location_code.ToUpper() == location.ToUpper());

                    if (existingLocation != null)
                    {

                        existingLocation.editDate = DateTime.Now;
                        existingLocation.editBy = (string)Session["user_id"];
                    }
                    // Save changes to the database
                    db.SaveChanges();
                    var existingControlUser = db.Database.SqlQuery<SP_GetUserProfiles_Result>("SP_GetUserProfiles @StaffNo",
                                                   new SqlParameter("StaffNo", (string)Session["user_id"])
                                               ).FirstOrDefault();

                    var userEditBy = existingControlUser.EditByName + " (" + existingControlUser.editBy.ToUpper() + ")";
                    var editDate = existingLocation.editDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                    

                    return Json(new
                    {
                        success = true,
                        message = "Added Factory Sucessfully",
                        activeLocation = activeLocation,
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
                    var locationAccess = db.sys_setting_role_permission.SingleOrDefault(rp => rp.role == roleID && rp.module == "location");
                    if (locationAccess.deleteFunction != true)
                    {
                        statusOptionsString = "A";
                    }
                    var locations = db.Database.SqlQuery<SP_GetLocations_Result>(
                "SP_GetLocations @LocationCode, @StatusOptions, @Criteria, @Text, @DateType, @StartDate, @EndDate",
                        new SqlParameter("LocationCode", ""),
                        new SqlParameter("StatusOptions", statusOptionsString),
                        new SqlParameter("Criteria", criteria),
                        new SqlParameter("Text", text),
                        new SqlParameter("DateType", dateType),
                        new SqlParameter("StartDate", startDate),
                        new SqlParameter("EndDate", endDate)
                    ).ToList();

                   
                    ViewBag.locationAccess = locationAccess;
                    var user_id= Session["user_id"]?.ToString();
                    List<sys_setting_define_table> defineTableList= db.sys_setting_define_table
                     .Where(item => item.createBy == user_id && item.module == "location")
                     .ToList();
                    var result = locations.Select(u => new
                    {

                        link = ((locationAccess?.status == true && locationAccess?.viewFunction == true) || (locationAccess?.status == true && locationAccess?.editFunction == true))
                    ? $@"class='clickable-row' onclick=""window.location.href = '{Url.Action("MainForm", "Location", new { id = u.location_code })}'"""
                    : "",

                        location_code = (defineTableList.Any(dt => dt.field == "location_code" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.location_code.ToUpper(): "NS",
                        name = (defineTableList.Any(dt => dt.field == "name" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.name: "NS",
                       
                        created_by = (defineTableList.Any(dt => dt.field == "createBy" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.CreateByName + " (" + u.createBy.ToUpper() + ")": "NS",
                        createdAt = (defineTableList.Any(dt => dt.field == "createDate" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?"<span class='createDate'>" + u.createDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture) + "</span>": "NS",
                        edited_by = (defineTableList.Any(dt => dt.field == "editBy" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.editBy != null && u.editBy != "" ? u.EditByName + " (" + u.editBy.ToUpper() + ")" : "<span class='text-danger'>No data</span>": "NS",

                        edited_at = (defineTableList.Any(dt => dt.field == "editDate" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.editDate != null ? "<span class='editDate'>" + u.editDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture) + "</span>"
                                    : "<span class='text-danger'>No data</span>": "NS",

                        status = (locationAccess?.status == true && locationAccess?.deleteFunction == true) ? 
                                u.status == "A" ? "<span class='text-primary'>Active</span>"
                                : "<span class='text-danger'>Inactive</span>"
                                :"NS",
                        editBtn = (locationAccess?.status == true && locationAccess?.editFunction == true)
                             ? $@"<a href=""{Url.Action("MainForm", "Location", new { id = u.location_code })}"" class=""btn btn-primary"">Edit</a>"
                            : (locationAccess?.status == true && locationAccess?.viewFunction == true)
                            ? $@"<a href=""{Url.Action("MainForm", "Location", new { id = u.location_code })}"" class=""btn btn-primary"">View</a>" : "",

                        statusBtn = (locationAccess?.status == true && locationAccess?.deleteFunction == true) ?
                            u.status == "A"
                            ? $@"<button style='width:6em;' class='btn btn-danger  ml-1' onclick='updateStatus(""Location"", ""{u.location_code}"", ""{u.status}"","""", ""Location Report"", ""Locations"", ""{Session["timezone"]}"")'>Inactive</button>"
                            : $@"<button style='width:6em;' class='btn btn-info  ml-1' onclick='updateStatus(""Location"", ""{u.location_code}"", ""{u.status}"","""", ""Location Report"", ""Locations"", ""{Session["timezone"]}"")'>Reactive</button>"
                            : ""

                    });


                    return Json(new { success = true,result = result }, JsonRequestBehavior.AllowGet);
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
