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
    public class FactoryController : Controller
    {

        // GET: Department
        public ActionResult Index()
        {

            
            try
            {
               
                using (var db = new AttendNow_DBEntities())
                {

                    ViewBag.Title = "Factory List";
                    var roleID = Session["role_id"]?.ToString();
                    var factoryAccess = db.sys_setting_role_permission.SingleOrDefault(rp => rp.role == roleID && rp.module == "factory");
                    ViewBag.factoryAccess = factoryAccess;
                    if (factoryAccess?.status != true)
                    {
                        return RedirectToAction("Logout", "User");
                    }
                    if (Session["user_id"] == null)
                    {
                        return RedirectToAction("Logout", "User");
                    }

                    var statusOptionsString = "";
                    if (factoryAccess.deleteFunction != true)
                    {
                        statusOptionsString = "A";
                    }
                    var user_id = Session["user_id"]?.ToString();
                    ViewBag.DefineTable = db.sys_setting_define_table
                     .Where(item => item.createBy == user_id && item.module == "factory")
                     .ToList();
                    var factorys = db.Database.SqlQuery<SP_GetFactorys_Result>(
                        "SP_GetFactorys @Factory_id ,@StatusOptions",
                        new SqlParameter("Factory_id", ""),
                        new SqlParameter("StatusOptions", statusOptionsString)).ToList();

                    TempData["page"] = "factorylist";
                    
                    
                    return View(factorys);
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

        // GET: Factory/MainForm/ :FOR ADD Factory 
        // GET: Factory/MainForm/5 :FOR EDIT Factory 
        public ActionResult MainForm(string id)
        {
            
            try
            {
                
                
                using (var db = new AttendNow_DBEntities())
                {
                    var roleID = Session["role_id"]?.ToString();
                    var factoryAccess = db.sys_setting_role_permission.SingleOrDefault(rp => rp.role == roleID && rp.module == "factory");
                    ViewBag.factoryAccess = factoryAccess;
                    if (factoryAccess?.status != true) //If role permission status is false then logout
                    {
                        return RedirectToAction("Logout", "User");
                    }
                    if (id == null) //Add function
                    {
                        
                        ViewBag.Title = "Add Factory";
                        TempData["page"] = "addfactory";
                        if (factoryAccess?.status != true || (factoryAccess?.addFunction != true)) // If role permission(ADD) status is false then logout
                        {
                            return RedirectToAction("Logout", "User");
                        }
                        return View();
                    }
                    else
                    {
                        if (factoryAccess?.status != true ||
                        (factoryAccess?.editFunction != true && factoryAccess?.viewFunction != true))
                        // If role permission(EDIT||VIEW) status is false then logout
                        {
                            return RedirectToAction("Logout", "User");
                        }
                        else
                        {
                            if (factoryAccess?.editFunction == true) //Edit Function
                            {
                                ViewBag.Title = "Edit Factory";
                            }
                            else //View Function
                            {
                                ViewBag.Title = "View Factory";
                            }

                            TempData["page"] = "factorylist";

                            //get specific user based on id
                            var factory = db.Database.SqlQuery<SP_GetFactorys_Result>(
                                "SP_GetFactorys @Factory_id",
                                new SqlParameter("Factory_id", id)

                            ).FirstOrDefault();

                            if (factory == null)
                            {
                                return RedirectToAction("NotFound", "Home");
                            }
                            else
                            {
                                return View(factory);
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

                ViewBag.ErrorMessage  = errormessage;
                return View();

            }
            catch (Exception e)
            {

                ViewBag.ErrorMessage  = e.InnerException == null ? e.Message : e.InnerException.Message;
                return View();
            }

        }



        // POST: Factory/Create
        [HttpPost]
        public JsonResult Create(tbl_factory FactoryData)
        {

            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    if (string.IsNullOrEmpty(FactoryData.name) || string.IsNullOrEmpty(FactoryData.factory_id))
                    {
                        return Json(new { success = false, error_message = "Make sure all fields are filled" });
                    }
                    if (db.tbl_factory.Any(f => f.factory_id.ToUpper() == FactoryData.factory_id.ToUpper()))

                    {
                        return Json(new { success = false, error_message = "This Factory ID is already in use.", idError = "This Factory ID is already in use." });
                    }
                    //Set the factory data
                    FactoryData.factory_id = FactoryData.factory_id.ToUpper();
                    FactoryData.status = "A";
                    FactoryData.createDate = DateTime.Now;
                    FactoryData.createBy = (string)Session["user_id"];


                    //Insert new factory into DB
                    db.tbl_factory.Add(FactoryData);
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


        // POST: Factory/Edit
        [HttpPost]
        public JsonResult Edit(tbl_factory FactoryData)
        {
            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    if (string.IsNullOrEmpty(FactoryData.name))
                    {
                        return Json(new { success = false, error_message = "Make sure all fields are filled" });
                    }

                    var existingFactory = db.tbl_factory.SingleOrDefault(f => f.factory_id.ToUpper() == FactoryData.factory_id.ToUpper());
                    if (existingFactory != null)

                    {

                        // Update the fields you want to change
                        existingFactory.name = FactoryData.name;
                        existingFactory.editBy = (string)Session["user_id"];
                        existingFactory.editDate = DateTime.Now;
                        db.Entry(existingFactory).State = EntityState.Modified;
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

        /*------------------------------------------UpdateStatus Function START-----------------------------------f-------*/
        public JsonResult UpdateStatus(String id, string newStatus)
        {

            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    // Find the existing user in the database by user_id
                    var existingFactory= db.tbl_factory.SingleOrDefault(f => f.factory_id.ToUpper() == id.ToUpper());

                    if (existingFactory!= null)
                    {
                        // Update the status property based on the newStatus parameter
                        existingFactory.status = newStatus;
                        existingFactory.editBy = (string)Session["user_id"];
                        existingFactory.editDate = DateTime.Now;
                        var existingControlUser = db.Database.SqlQuery<SP_GetUserProfiles_Result>("SP_GetUserProfiles @StaffNo",
                                                   new SqlParameter("StaffNo", (string)Session["user_id"])
                                               ).FirstOrDefault();

                        var userEditBy = existingControlUser.EditByName + " (" + existingControlUser.editBy.ToUpper() + ")";
                        var editDate = existingFactory.editDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                        db.Entry(existingFactory).State = EntityState.Modified;
                        db.SaveChanges();


                        return Json(new { success = true, message = "Updated Successfully." ,
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
                    var factoryAccess = db.sys_setting_role_permission.SingleOrDefault(rp => rp.role == roleID && rp.module == "factory");
                    if (factoryAccess.deleteFunction != true)
                    {
                        statusOptionsString = "A";
                    }
                    var factorys = db.Database.SqlQuery<SP_GetFactorys_Result>(
                "SP_GetFactorys @FactoryCode, @StatusOptions, @Criteria, @Text, @DateType, @StartDate, @EndDate",
                        new SqlParameter("FactoryCode", ""),
                        new SqlParameter("StatusOptions", statusOptionsString),
                        new SqlParameter("Criteria", criteria),
                        new SqlParameter("Text", text),
                        new SqlParameter("DateType", dateType),
                        new SqlParameter("StartDate", startDate),
                        new SqlParameter("EndDate", endDate)
                    ).ToList();

                    var user_id= Session["user_id"]?.ToString();
                    List<sys_setting_define_table> defineTableList= db.sys_setting_define_table
                     .Where(item => item.createBy == user_id &&item.module=="factory")
                     .ToList();
                    ViewBag.factoryAccess = factoryAccess;
                    var result = factorys.Select(u => new
                    {

                        link = ((factoryAccess?.status == true && factoryAccess?.viewFunction == true) || (factoryAccess?.status == true && factoryAccess?.editFunction == true))
                    ? $@"class='clickable-row' onclick=""window.location.href = '{Url.Action("MainForm", "Factory", new { id = u.factory_id })}'"""
                    : "",

                        factory_id = (defineTableList.Any(dt => dt.field == "factory_id" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.factory_id.ToUpper(): "NS",
                        name = (defineTableList.Any(dt => dt.field == "name" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.name: "NS",

                        created_by = (defineTableList.Any(dt => dt.field == "createBy" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.CreateByName + " (" + u.createBy.ToUpper() + ")": "NS",
                        createdAt = (defineTableList.Any(dt => dt.field == "createDate" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?"<span class='createDate'>" + u.createDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture) + "</span>": "NS",
                        edited_by = (defineTableList.Any(dt => dt.field == "editBy" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.editBy != null && u.editBy != "" ? u.EditByName + " (" + u.editBy.ToUpper() + ")" : "<span class='text-danger'>No data</span>": "NS",

                        edited_at = (defineTableList.Any(dt => dt.field == "editDate" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.editDate != null ? "<span class='editDate'>" + u.editDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture) + "</span>"
                                    : "<span class='text-danger'>No data</span>": "NS",

                        status = (factoryAccess?.status == true && factoryAccess?.deleteFunction == true) ? 
                                u.status == "A" ? "<span class='text-primary'>Active</span>" 
                                : "<span class='text-danger'>Inactive</span>"
                                :"NS",
                        editBtn = (factoryAccess?.status == true && factoryAccess?.editFunction == true)
                             ? $@"<a href=""{Url.Action("MainForm", "Factory", new { id = u.factory_id })}"" class=""btn btn-primary"">Edit</a>"
                            : (factoryAccess?.status == true && factoryAccess?.viewFunction == true)
                            ? $@"<a href=""{Url.Action("MainForm", "Factory", new { id = u.factory_id })}"" class=""btn btn-primary"">View</a>" : "",

                        statusBtn = (factoryAccess?.status == true && factoryAccess?.deleteFunction == true) ?
                            u.status == "A"
                            ? $@"<button style='width:6em;' class='btn btn-danger  ml-1' onclick='updateStatus(""Factory"", ""{u.factory_id}"", ""{u.status}"","""", ""Factory Report"", ""Factories"", ""{Session["timezone"]}"")'>Inactive</button>"
                            : $@"<button style='width:6em;' class='btn btn-info  ml-1' onclick='updateStatus(""Factory"", ""{u.factory_id}"", ""{u.status}"","""", ""Factory Report"", ""Factories"", ""{Session["timezone"]}"")'>Reactive</button>"
                            : ""

                    });


                    return Json(new { success = true, result = result }, JsonRequestBehavior.AllowGet);
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