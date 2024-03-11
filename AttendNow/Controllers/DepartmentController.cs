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
    public class DepartmentController : Controller
    {

        // GET: Department
        public ActionResult Index()
        {

            //
            try
            {
               
                using (var db = new AttendNow_DBEntities())
                {

                    ViewBag.Title = "Department List";
                    var roleID = Session["role_id"]?.ToString();
                    var departmentAccess = db.sys_setting_role_permission.SingleOrDefault(rp => rp.role == roleID && rp.module == "department");
                    ViewBag.departmentAccess = departmentAccess;
                    if (departmentAccess?.status != true)
                    {
                        return RedirectToAction("Logout", "User");
                    }
                    if (Session["user_id"] == null)
                    {
                        return RedirectToAction("Logout", "User");
                    }

                    var statusOptionsString = "";
                    if (departmentAccess.deleteFunction != true)
                    {
                        statusOptionsString = "A";
                    }
                     var user_id = Session["user_id"]?.ToString();

                    //Define table row
                    ViewBag.DefineTable = db.sys_setting_define_table
                     .Where(item => item.createBy == user_id && item.module == "department")
                     .ToList();


                    var departments = db.Database.SqlQuery<SP_GetDepartments_Result>(
                        "SP_GetDepartments @DepartmentCode ,@StatusOptions",
                        new SqlParameter("DepartmentCode", ""),
                        new SqlParameter("StatusOptions", statusOptionsString)).ToList();

                    TempData["page"] = "departmentlist";
                    
                    
                    return View(departments);
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

        // GET: Department/MainForm/ :FOR ADD Department 
        // GET: Department/MainForm/5 :FOR EDIT Department 
        public ActionResult MainForm(string id)
        {
            
            try
            {
                
                
                using (var db = new AttendNow_DBEntities())
                {
                    var roleID = Session["role_id"]?.ToString();
                    var departmentAccess = db.sys_setting_role_permission.SingleOrDefault(rp => rp.role == roleID && rp.module == "department");
                    ViewBag.departmentAccess = departmentAccess;
                    if (departmentAccess?.status != true) //If role permission status is false then logout
                    {
                        return RedirectToAction("Logout", "User");
                    }
                    if (id == null) //Add function
                    {
                        
                        ViewBag.Title = "Add Department";
                        TempData["page"] = "adddepartment";
                        if (departmentAccess?.status != true || (departmentAccess?.addFunction != true)) // If role permission(ADD) status is false then logout
                        {
                            return RedirectToAction("Logout", "User");
                        }
                        return View();
                    }
                    else
                    {
                        if (departmentAccess?.status != true ||
                        (departmentAccess?.editFunction != true && departmentAccess?.viewFunction != true))
                        // If role permission(EDIT||VIEW) status is false then logout
                        {
                            return RedirectToAction("Logout", "User");
                        }
                        else
                        {
                            if (departmentAccess?.editFunction == true) //Edit Function
                            {
                                ViewBag.Title = "Edit Department";
                            }
                            else //View Function
                            {
                                ViewBag.Title = "View Department";
                            }

                            TempData["page"] = "departmentlist";

                            //get specific user based on id
                            var department = db.Database.SqlQuery<SP_GetDepartments_Result>(
                                "SP_GetDepartments @DepartmentCode",
                                new SqlParameter("DepartmentCode", id)

                            ).FirstOrDefault();

                            if (department == null)
                            {
                                return RedirectToAction("NotFound", "Home");
                            }
                            else
                            {
                                return View(department);
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



        // POST: Department/Create
        [HttpPost]
        public JsonResult Create(tbl_department DepartmentData)
        {

            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    if (string.IsNullOrEmpty(DepartmentData.name) || string.IsNullOrEmpty(DepartmentData.department_code))
                    {
                        return Json(new { success = false, error_message = "Make sure all fields are filled" });
                    }
                    if (db.tbl_department.Any(l => l.department_code.ToUpper() == DepartmentData.department_code.ToUpper()))

                    {
                        return Json(new { success = false, error_message = "This Department Code is already in use.", idError = "This Department Code is already in use." });
                    }
                    //Set the department data
                    DepartmentData.department_code = DepartmentData.department_code.ToUpper();
                    DepartmentData.status = "A";
                    DepartmentData.createDate = DateTime.Now;
                    DepartmentData.createBy = (string)Session["user_id"];


                    //Insert new department into DB
                    db.tbl_department.Add(DepartmentData);
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


        // POST: Department/Edit
        [HttpPost]
        public JsonResult Edit(tbl_department DepartmentData)
        {
            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    if (string.IsNullOrEmpty(DepartmentData.name))
                    {
                        return Json(new { success = false, error_message = "Make sure all fields are filled" });
                    }

                    var existingDepartment = db.tbl_department.SingleOrDefault(l => l.department_code.ToUpper() == DepartmentData.department_code.ToUpper());
                    if (existingDepartment != null)

                    {

                        // Update the fields you want to change
                        existingDepartment.name = DepartmentData.name;
                        existingDepartment.editBy = (string)Session["user_id"];
                        existingDepartment.editDate = DateTime.Now;
                        db.Entry(existingDepartment).State = EntityState.Modified;
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
                    var existingDepartment = db.tbl_department.SingleOrDefault(u => u.department_code.ToUpper() == id.ToUpper());

                    if (existingDepartment != null)
                    {
                        // Update the status property based on the newStatus parameter
                        existingDepartment.status = newStatus;
                        existingDepartment.editBy = (string)Session["user_id"];
                        existingDepartment.editDate = DateTime.Now;
                        var existingControlUser = db.Database.SqlQuery<SP_GetUserProfiles_Result>("SP_GetUserProfiles @StaffNo",
                                                   new SqlParameter("StaffNo", (string)Session["user_id"])
                                               ).FirstOrDefault();

                        var userEditBy = existingControlUser.EditByName + " (" + existingControlUser.editBy.ToUpper() + ")";
                        var editDate = existingDepartment.editDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                        db.Entry(existingDepartment).State = EntityState.Modified;
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
                    var departmentAccess = db.sys_setting_role_permission.SingleOrDefault(rp => rp.role == roleID && rp.module == "department");
                    if (departmentAccess.deleteFunction != true)
                    {
                        statusOptionsString = "A";
                    }
                    var departments = db.Database.SqlQuery<SP_GetDepartments_Result>(
                "SP_GetDepartments @DepartmentCode, @StatusOptions, @Criteria, @Text, @DateType, @StartDate, @EndDate",
                        new SqlParameter("DepartmentCode", ""),
                        new SqlParameter("StatusOptions", statusOptionsString),
                        new SqlParameter("Criteria", criteria),
                        new SqlParameter("Text", text),
                        new SqlParameter("DateType", dateType),
                        new SqlParameter("StartDate", startDate),
                        new SqlParameter("EndDate", endDate)
                    ).ToList();

                    var user_id= Session["user_id"]?.ToString();
                    List<sys_setting_define_table> defineTableList= db.sys_setting_define_table
                     .Where(item => item.createBy == user_id && item.module == "department")
                     .ToList();
                    ViewBag.departmentAccess = departmentAccess;
                    var result = departments.Select(u => new
                    {

                        link = ((departmentAccess?.status == true && departmentAccess?.viewFunction == true) || (departmentAccess?.status == true && departmentAccess?.editFunction == true))
                    ? $@"class='clickable-row' onclick=""window.location.href = '{Url.Action("MainForm", "Department", new { id = u.department_code })}'"""
                    : "",

                        department_code = (defineTableList.Any(dt => dt.field == "department_code" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.department_code.ToUpper(): "NS",
                        name = (defineTableList.Any(dt => dt.field == "name" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.name: "NS",

                        created_by = (defineTableList.Any(dt => dt.field == "createBy" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.CreateByName + " (" + u.createBy.ToUpper() + ")": "NS",
                        createdAt = (defineTableList.Any(dt => dt.field == "createDate" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?"<span class='createDate'>" + u.createDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture) + "</span>": "NS",

                        edited_by = (defineTableList.Any(dt => dt.field == "editBy" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.editBy != null && u.editBy != "" ? u.EditByName + " (" + u.editBy.ToUpper() + ")" : "<span class='text-danger'>No data</span>": "NS",

                        edited_at = (defineTableList.Any(dt => dt.field == "editDate" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.editDate != null ? "<span class='editDate'>" + u.editDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture) + "</span>"
                                    : "<span class='text-danger'>No data</span>": "NS",

                        status = (departmentAccess?.status == true && departmentAccess?.deleteFunction == true) ? 
                                u.status == "A" ? "<span class='text-primary'>Active</span>" 
                                : "<span class='text-danger'>Inactive</span>"
                                :"NS",
                        editBtn = (departmentAccess?.status == true && departmentAccess?.editFunction == true)
                             ? $@"<a href=""{Url.Action("MainForm", "Department", new { id = u.department_code })}"" class=""btn btn-primary"">Edit</a>"
                            : (departmentAccess?.status == true && departmentAccess?.viewFunction == true)
                            ? $@"<a href=""{Url.Action("MainForm", "Department", new { id = u.department_code })}"" class=""btn btn-primary"">View</a>" : "",

                        statusBtn = (departmentAccess?.status == true && departmentAccess?.deleteFunction == true) ?
                            u.status == "A"
                            ? $@"<button style='width:6em;' class='btn btn-danger  ml-1' onclick='updateStatus(""Department"", ""{u.department_code}"", ""{u.status}"", """", ""Department Report"", ""Departments"", ""{Session["timezone"]}"")'>Inactive</button>"
                            : $@"<button style='width:6em;' class='btn btn-info  ml-1' onclick='updateStatus(""Department"", ""{u.department_code}"", ""{u.status}"", """", ""Department Report"", ""Departments"", ""{Session["timezone"]}"")'>Reactive</button>"
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