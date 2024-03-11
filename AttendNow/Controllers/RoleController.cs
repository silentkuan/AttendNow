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
    public class RoleController : Controller
    {
        


        /*------------------------------------------Add, Edit, View Function START------------------------------------------*/
        // GET: Role
        public ActionResult Index()
        {
            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    ViewBag.Title = "Role List";
                    var roleID = Session["role_id"]?.ToString();
                    var roleAccess = db.sys_setting_role_permission.SingleOrDefault(rp => rp.role == roleID && rp.module == "role");
                    ViewBag.roleAccess = roleAccess;
                    if (roleAccess?.status != true)
                    {
                        return RedirectToAction("Logout", "User");
                    }
                    if (Session["user_id"] == null)
                    {
                        return RedirectToAction("Logout", "User");
                    }

                    var statusOptionsString = "";
                    if (roleAccess.deleteFunction != true)
                    {
                        statusOptionsString = "A";
                    }
                    var user_id = Session["user_id"]?.ToString();
                    ViewBag.DefineTable = db.sys_setting_define_table
                     .Where(item => item.createBy == user_id && item.module == "role")
                     .ToList();

                    var roles = db.Database.SqlQuery<SP_GetRoles_Result>("SP_GetRoles @RoleCode ,@StatusOptions",
                        new SqlParameter("@RoleCode", ""),
                        new SqlParameter("StatusOptions", statusOptionsString)).ToList();
                    TempData["page"] = "rolelist";


                    return View(roles);
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


        // GET: User/MainForm/ :FOR ADD Role 
        // GET: User/MainForm/5 :FOR EDIT Role 
        public ActionResult MainForm(string id)
        {
            try
            {
                using (var db = new AttendNow_DBEntities())
                {

                   
                    var roleID = Session["role_id"]?.ToString();
                    var roleAccess = db.sys_setting_role_permission.SingleOrDefault(rp => rp.role == roleID && rp.module == "role");
                    ViewBag.roleAccess = roleAccess;
                    if (roleAccess?.status != true)
                    {
                        return RedirectToAction("Logout", "User");
                    }
                    if (Session["user_id"] == null)
                    {
                        return RedirectToAction("Logout", "User");
                    }
                    if (id == null)
                    {
                        if (roleAccess?.status != true || (roleAccess?.addFunction != true)) // If role permission(ADD) status is false then logout
                        {
                            return RedirectToAction("Logout", "User");
                        }
                        ViewBag.Title = "Add Role";
                        TempData["page"] = "addrole"; //Page will be add function
                        return View();
                    }
                    else
                    {
                        if (roleAccess?.status != true ||
                        (roleAccess?.editFunction != true && roleAccess?.viewFunction != true))
                        // If role permission(EDIT||VIEW) status is false then logout
                        {
                            return RedirectToAction("Logout", "User");
                        }
                        else
                        {
                            if (roleAccess?.editFunction == true) //Edit Function
                            {
                                ViewBag.Title = "Edit Role";
                            }
                            else //View Function
                            {
                                ViewBag.Title = "View Role";
                            }

                            TempData["page"] = "rolelist";

                            //get specific user based on id
                            var role = db.Database.SqlQuery<SP_GetRoles_Result>(
                       "SP_GetRoles @RoleCode",
                       new SqlParameter("RoleCode", id)

                   ).FirstOrDefault();
                            if (role == null)
                            {
                                return RedirectToAction("NotFound", "Home");
                            }
                            else
                            {
                                ViewBag.RolePermissionUser = db.sys_setting_role_permission.FirstOrDefault(u => u.role == role.role_code && u.module == "user");
                                ViewBag.RolePermissionRole = db.sys_setting_role_permission.FirstOrDefault(u => u.role == role.role_code && u.module == "role");
                                ViewBag.RolePermissionDepartment = db.sys_setting_role_permission.FirstOrDefault(u => u.role == role.role_code && u.module == "department");
                                ViewBag.RolePermissionLocation = db.sys_setting_role_permission.FirstOrDefault(u => u.role == role.role_code && u.module == "location");
                                ViewBag.RolePermissionParticipant = db.sys_setting_role_permission.FirstOrDefault(u => u.role == role.role_code && u.module == "participant");
                                ViewBag.RolePermissionMeeting = db.sys_setting_role_permission.FirstOrDefault(u => u.role == role.role_code && u.module == "meeting");
                                ViewBag.RolePermissionFactory = db.sys_setting_role_permission.FirstOrDefault(u => u.role == role.role_code && u.module == "factory");
                                TempData["page"] = "rolelist"; //Page will be edit function
                                return View(role);
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



        // POST: Role/Create
        [HttpPost]
        public JsonResult Create(RoleData RoleData)
        {
            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    if (string.IsNullOrEmpty(RoleData.Role.name)|| string.IsNullOrEmpty(RoleData.Role.role_code))
                    {
                        return Json(new { success = false, error_message = "Make sure all fields are filled" });
                    }
                    if (db.tbl_role.Any(r => r.role_code.ToUpper() == RoleData.Role.role_code.ToUpper()))

                    {
                        return Json(new { success = false, error_message = "This Role Code is already in use.", idError = "This Role Code is already in use." });
                    }
                    //Set the role data
                    RoleData.Role.status = "A";
                    RoleData.Role.createDate = DateTime.Now;
                    RoleData.Role.createBy = (string)Session["user_id"];
                    //Insert new role into DB
                    db.tbl_role.Add(RoleData.Role);
                    db.SaveChanges();

                    //Set the create data of each modules
                    RoleData.UserModule.createDate = DateTime.Now;
                    RoleData.UserModule.createBy = (string)Session["user_id"];
                    //Insert new role permission of user module based on role_code
                    db.sys_setting_role_permission.Add(RoleData.UserModule);
                    db.SaveChanges();

                    RoleData.DepartmentModule.createDate = DateTime.Now;
                    RoleData.DepartmentModule.createBy = (string)Session["user_id"];
                    //Insert new role permission of department module based on role_code
                    db.sys_setting_role_permission.Add(RoleData.DepartmentModule);
                    db.SaveChanges();

                    RoleData.FactoryModule.createDate = DateTime.Now;
                    RoleData.FactoryModule.createBy = (string)Session["user_id"];
                    //Insert new role permission of location module based on role_code
                    db.sys_setting_role_permission.Add(RoleData.FactoryModule);
                    db.SaveChanges();

                    RoleData.LocationModule.createDate = DateTime.Now;
                    RoleData.LocationModule.createBy = (string)Session["user_id"];
                    //Insert new role permission of location module based on role_code
                    db.sys_setting_role_permission.Add(RoleData.LocationModule);
                    db.SaveChanges();

                    RoleData.RoleModule.createDate = DateTime.Now;
                    RoleData.RoleModule.createBy = (string)Session["user_id"];
                    //Insert new role permission of role module based on role_code
                    db.sys_setting_role_permission.Add(RoleData.RoleModule);
                    db.SaveChanges();

                    RoleData.ParticipantModule.createDate = DateTime.Now;
                    RoleData.ParticipantModule.createBy = (string)Session["user_id"];
                    db.sys_setting_role_permission.Add(RoleData.ParticipantModule);
                    db.SaveChanges();

                    RoleData.MeetingModule.createDate = DateTime.Now;
                    RoleData.MeetingModule.createBy = (string)Session["user_id"];
                    db.sys_setting_role_permission.Add(RoleData.MeetingModule);
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


        // POST: Role/Edit
        [HttpPost]
        public JsonResult Edit(RoleData RoleData)
        {
            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    if (string.IsNullOrEmpty(RoleData.Role.name))
                    {
                        return Json(new { success = false, error_message = "Make sure all fields are filled" });
                    }

                    var existingRole = db.tbl_role.SingleOrDefault(r => r.role_code.ToUpper() == RoleData.Role.role_code.ToUpper());
                    if (existingRole != null)

                    {

                            // Update the fields you want to change
                            existingRole.name = RoleData.Role.name;
                            existingRole.editBy = (string)Session["user_id"];
                            existingRole.editDate = DateTime.Now;
                            db.Entry(existingRole).State = EntityState.Modified;
                            db.SaveChanges();


                        var rolePermissionUser = db.sys_setting_role_permission.FirstOrDefault(u => u.role == existingRole.role_code && u.module == "user");
                            rolePermissionUser.status = RoleData.UserModule.status;
                            rolePermissionUser.addFunction = RoleData.UserModule.addFunction;
                            rolePermissionUser.editFunction = RoleData.UserModule.editFunction;
                            rolePermissionUser.viewFunction = RoleData.UserModule.viewFunction;
                            rolePermissionUser.deleteFunction = RoleData.UserModule.deleteFunction;
                            rolePermissionUser.editDate = DateTime.Now;
                            rolePermissionUser.editBy = (string)Session["user_id"];
                            db.Entry(rolePermissionUser).State = EntityState.Modified;
                            db.SaveChanges();

                        var rolePermissionDepartment = db.sys_setting_role_permission.FirstOrDefault(u => u.role == existingRole.role_code && u.module == "department");
                            rolePermissionDepartment.status = RoleData.DepartmentModule.status;
                            rolePermissionDepartment.addFunction = RoleData.DepartmentModule.addFunction;
                            rolePermissionDepartment.editFunction = RoleData.DepartmentModule.editFunction;
                            rolePermissionDepartment.viewFunction = RoleData.DepartmentModule.viewFunction;
                            rolePermissionDepartment.deleteFunction = RoleData.DepartmentModule.deleteFunction;
                            rolePermissionDepartment.editDate = DateTime.Now;
                            rolePermissionDepartment.editBy = (string)Session["user_id"];
                            db.Entry(rolePermissionDepartment).State = EntityState.Modified;
                            db.SaveChanges();

                        var rolePermissionLocation = db.sys_setting_role_permission.FirstOrDefault(u => u.role == existingRole.role_code && u.module == "location");
                            rolePermissionLocation.status = RoleData.LocationModule.status;
                            rolePermissionLocation.addFunction = RoleData.LocationModule.addFunction;
                            rolePermissionLocation.editFunction = RoleData.LocationModule.editFunction;
                            rolePermissionLocation.viewFunction = RoleData.LocationModule.viewFunction;
                            rolePermissionLocation.deleteFunction = RoleData.LocationModule.deleteFunction;
                            rolePermissionLocation.editDate = DateTime.Now;
                            rolePermissionLocation.editBy = (string)Session["user_id"];
                            db.Entry(rolePermissionLocation).State = EntityState.Modified;
                            db.SaveChanges();

                        var rolePermissionFactory = db.sys_setting_role_permission.FirstOrDefault(u => u.role == existingRole.role_code && u.module == "factory");
                        rolePermissionFactory.status = RoleData.FactoryModule.status;
                        rolePermissionFactory.addFunction = RoleData.FactoryModule.addFunction;
                        rolePermissionFactory.editFunction = RoleData.FactoryModule.editFunction;
                        rolePermissionFactory.viewFunction = RoleData.FactoryModule.viewFunction;
                        rolePermissionFactory.deleteFunction = RoleData.FactoryModule.deleteFunction;
                        rolePermissionFactory.editDate = DateTime.Now;
                        rolePermissionFactory.editBy = (string)Session["user_id"];
                        db.Entry(rolePermissionFactory).State = EntityState.Modified;
                        db.SaveChanges();

                        var rolePermissionRole = db.sys_setting_role_permission.FirstOrDefault(u => u.role == existingRole.role_code && u.module == "role");
                            rolePermissionRole.status = RoleData.RoleModule.status;
                            rolePermissionRole.addFunction = RoleData.RoleModule.addFunction;
                            rolePermissionRole.editFunction = RoleData.RoleModule.editFunction;
                            rolePermissionRole.viewFunction = RoleData.RoleModule.viewFunction;
                            rolePermissionRole.deleteFunction = RoleData.RoleModule.deleteFunction;
                            rolePermissionRole.editDate = DateTime.Now;
                            rolePermissionRole.editBy = (string)Session["user_id"];
                            db.Entry(rolePermissionRole).State = EntityState.Modified;
                            db.SaveChanges();

                        var rolePermissionParticipant = db.sys_setting_role_permission.FirstOrDefault(u => u.role == existingRole.role_code && u.module == "participant");
                        rolePermissionParticipant.status = RoleData.ParticipantModule.status;
                        rolePermissionParticipant.addFunction = RoleData.ParticipantModule.addFunction;
                        rolePermissionParticipant.editFunction = RoleData.ParticipantModule.editFunction;
                        rolePermissionParticipant.viewFunction = RoleData.ParticipantModule.viewFunction;
                        rolePermissionParticipant.deleteFunction = RoleData.ParticipantModule.deleteFunction;
                        rolePermissionParticipant.editDate = DateTime.Now;
                        rolePermissionParticipant.editBy = (string)Session["user_id"];
                        db.Entry(rolePermissionParticipant).State = EntityState.Modified;
                        db.SaveChanges();

                        var rolePermissionMeeting = db.sys_setting_role_permission.FirstOrDefault(u => u.role == existingRole.role_code && u.module == "meeting");
                        rolePermissionMeeting.status = RoleData.MeetingModule.status;
                        rolePermissionMeeting.addFunction = RoleData.MeetingModule.addFunction;
                        rolePermissionMeeting.editFunction = RoleData.MeetingModule.editFunction;
                        rolePermissionMeeting.viewFunction = RoleData.MeetingModule.viewFunction;
                        rolePermissionMeeting.deleteFunction = RoleData.MeetingModule.deleteFunction;
                        rolePermissionMeeting.editDate = DateTime.Now;
                        rolePermissionMeeting.editBy = (string)Session["user_id"];
                        db.Entry(rolePermissionMeeting).State = EntityState.Modified;
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
                    var existingRole = db.tbl_role.SingleOrDefault(u => u.role_code.ToUpper() == id.ToUpper());

                    if (existingRole != null)
                    {
                        // Update the status property based on the newStatus parameter
                        existingRole.status = newStatus;
                        existingRole.editBy = (string)Session["user_id"];
                        existingRole.editDate = DateTime.Now;
                        var existingControlUser = db.Database.SqlQuery<SP_GetUserProfiles_Result>("SP_GetUserProfiles @StaffNo",
                                                  new SqlParameter("StaffNo", (string)Session["user_id"])
                                              ).FirstOrDefault();

                        var userEditBy = existingControlUser.EditByName + " (" + existingControlUser.editBy.ToUpper() + ")";
                        var editDate = existingRole.editDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                        db.Entry(existingRole).State = EntityState.Modified;
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
                    var roleAccess = db.sys_setting_role_permission.SingleOrDefault(rp => rp.role == roleID && rp.module == "role");
                    if (roleAccess.deleteFunction != true)
                    {
                        statusOptionsString = "A";
                    }
                    var roles = db.Database.SqlQuery<SP_GetRoles_Result>(
                "SP_GetRoles @RoleCode, @StatusOptions, @Criteria, @Text, @DateType, @StartDate, @EndDate",
                        new SqlParameter("RoleCode", ""),
                        new SqlParameter("StatusOptions", statusOptionsString),
                        new SqlParameter("Criteria", criteria),
                        new SqlParameter("Text", text),
                        new SqlParameter("DateType", dateType),
                        new SqlParameter("StartDate", startDate),
                        new SqlParameter("EndDate", endDate)
                    ).ToList();
                    var user_id= Session["user_id"]?.ToString();
                    List<sys_setting_define_table> defineTableList= db.sys_setting_define_table
                     .Where(item => item.createBy == user_id && item.module == "role")
                     .ToList();
                    ViewBag.userAccess = roleAccess;
                    
                    var result = roles.Select(u => new
                    {

                        link = ((roleAccess?.status == true && roleAccess?.viewFunction == true) || (roleAccess?.status == true && roleAccess?.editFunction == true))
                    ? $@"class='clickable-row' onclick=""window.location.href = '{Url.Action("MainForm", "Role", new { id = u.role_code })}'"""
                    : "",

                        role_code = (defineTableList.Any(dt => dt.field == "role_code" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.role_code.ToUpper() : "NS",
                        name = (defineTableList.Any(dt => dt.field == "name" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.name : "NS",

                        created_by = (defineTableList.Any(dt => dt.field == "createBy" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ? u.CreateByName + " (" + u.createBy.ToUpper() + ")" : "NS",
                        createdAt = (defineTableList.Any(dt => dt.field == "createDate" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ? "<span class='createDate'>" + u.createDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture) + "</span>" : "NS",
                        edited_by = (defineTableList.Any(dt => dt.field == "editBy" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ? u.editBy != null && u.editBy != "" ? u.EditByName + " (" + u.editBy.ToUpper() + ")" : "<span class='text-danger'>No data</span>" : "NS",

                        edited_at = (defineTableList.Any(dt => dt.field == "role_code" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ? u.editDate != null ? "<span class='editDate'>" + u.editDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture) + "</span>"
                                    : "<span class='text-danger'>No data</span>" : "NS",

                        status = (roleAccess?.status == true && roleAccess?.deleteFunction == true) ? 
                                 u.status == "A" ? "<span class='text-primary'>Active</span>" 
                                 : "<span class='text-danger'>Inactive</span>"
                                 :"NS",
                        editBtn = (roleAccess?.status == true && roleAccess?.editFunction == true)
                             ? $@"<a href=""{Url.Action("MainForm", "Role", new { id = u.role_code })}"" class=""btn btn-primary"">Edit</a>"
                            : (roleAccess?.status == true && roleAccess?.viewFunction == true)
                            ? $@"<a href=""{Url.Action("MainForm", "Role", new { id = u.role_code })}"" class=""btn btn-primary"">View</a>" : "",

                        statusBtn = (roleAccess?.status == true && roleAccess?.deleteFunction == true) ?
                            u.status == "A"
                            ? $@"<button style='width:6em;' class='btn btn-danger  ml-1' onclick='updateStatus(""Role"", ""{u.role_code}"", ""{u.status}"","""", ""Role Report"", ""Roles"", ""{Session["timezone"]}"")'>Inactive</button>"
                            : $@"<button style='width:6em;' class='btn btn-info  ml-1' onclick='updateStatus(""Role"", ""{u.role_code}"", ""{u.status}"","""", ""Role Report"", ""Roles"", ""{Session["timezone"]}"")'>Reactive</button>"
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
