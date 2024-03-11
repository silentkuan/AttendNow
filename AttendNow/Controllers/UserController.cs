using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Security.Cryptography;
using AttendNow.Models;
using System.IO;
using System.Globalization;
using System.Data.SqlClient;
using System.Data.Entity.Validation;
using System.Text;
using System.Configuration;
using AttendNow.Models.ViewModel;

namespace AttendNow.Controllers
{
    public class UserController : Controller
    {



        // GET: User
        public ActionResult Index()
        {
            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    ViewBag.Title = "User List";
                    var roleID = Session["role_id"]?.ToString();
                    var userAccess = db.sys_setting_role_permission.SingleOrDefault(rp => rp.role == roleID && rp.module == "user");
                    ViewBag.userAccess = userAccess;
                    if (userAccess?.status != true)
                    {
                        return RedirectToAction("Logout", "User");
                    }
                    if (Session["user_id"] == null)
                    {
                        return RedirectToAction("Logout", "User");
                    }
                    var statusOptionsString = "";
                    if (userAccess.deleteFunction != true)
                    {
                        statusOptionsString = "A";
                    }
                    var user_id = Session["user_id"]?.ToString();
                    ViewBag.DefineTable = db.sys_setting_define_table
                     .Where(item => item.createBy == user_id && item.module == "user")
                     .ToList();
                    var userProfiles = db.Database.SqlQuery<SP_GetUserProfiles_Result>(
                        "SP_GetUserProfiles @StaffNo ,@StatusOptions",
                        new SqlParameter("StaffNo", ""),
                        new SqlParameter("StatusOptions", statusOptionsString)).ToList();

                    TempData["page"] = "userlist";
                    return View(userProfiles);
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
        // GET: User/MainForm/ :FOR ADD USER 
        // GET: User/MainForm/5 :FOR EDIT USER 
        public ActionResult MainForm(string id,string userProfile)
        {
            try
            {
                
                using (var db = new AttendNow_DBEntities())
                {
                    var roleID = Session["role_id"]?.ToString();
                    var userAccess = db.sys_setting_role_permission.SingleOrDefault(rp => rp.role == roleID && rp.module == "user");
                    ViewBag.userAccess = userAccess;
                    if (userAccess?.status != true &&userProfile!="yes") //If role permission status is false then logout
                    {
                        return RedirectToAction("Logout", "User");
                    }
                    SP_GetUserProfiles_Result user ;
                    //pass location_db, department_db, role_db to view in dropdown format
                    var activeFactory = db.Database.SqlQuery<SP_GetFactorys_Result>(
                       "SP_GetFactorys @Factory_id ,@StatusOptions",
                       new SqlParameter("Factory_id", ""),
                       new SqlParameter("StatusOptions", "A")).ToList();

                    ViewBag.factory_id = new SelectList(activeFactory, "factory_id", "name");


                    var activeLocation = db.Database.SqlQuery<SP_GetLocationsWithFactory_Result>(
                        "SP_GetLocationsWithFactory @LocationCode ,@StatusOptions",
                        new SqlParameter("LocationCode", ""),
                        new SqlParameter("StatusOptions", "A")
                        ).ToList();
                   
                    ViewBag.location_code = new SelectList(
                        activeLocation.Select(l => new SelectListItem
                        {
                            Value = l.location_code,
                            Text = l.factory + "-" + l.name
                        }),
                        "Value", "Text"
                    );
                    
                    var activeDepartment = db.Database.SqlQuery<SP_GetDepartments_Result>(
                        "SP_GetDepartments @DepartmentCode ,@StatusOptions",
                        new SqlParameter("DepartmentCode", ""),
                        new SqlParameter("StatusOptions", "A")).ToList();
                    ViewBag.department_code = new SelectList(activeDepartment, "department_code", "name");

                    var activeRole = db.Database.SqlQuery<SP_GetRoles_Result>(
                        "SP_GetRoles @RoleCode,@StatusOptions",
                         new SqlParameter("RoleCode", ""),
                         new SqlParameter("StatusOptions", "A")).ToList();
                    ViewBag.role_code = new SelectList(activeRole, "role_code", "name");

                    
                    if (id == null && userProfile != "yes")
                    {
                        if (userAccess?.status != true || (userAccess?.addFunction != true)) // If role permission(ADD) status is false then logout
                        {
                            return RedirectToAction("Logout", "User");
                        }

                        ViewBag.Title = "Add User";
                        TempData["page"] = "adduser"; //Page will be add function
                        return View();
                    }
                    else if (userProfile == "yes")
                    {
                        id = (string)Session["user_id"];
                        ViewBag.Title = "Your Personal Profile";
                           user = db.Database.SqlQuery<SP_GetUserProfiles_Result>(
                     "SP_GetUserProfiles @StaffNo",
                     new SqlParameter("StaffNo", (string)Session["user_id"])

                 ).FirstOrDefault();
                    }
                    else
                    {
                        if (userAccess?.status != true ||
                        (userAccess?.editFunction != true && userAccess?.viewFunction != true))
                        // If role permission(EDIT||VIEW) status is false then logout
                        {
                            return RedirectToAction("Logout", "User");
                        }
                        else
                        {
                            if (userAccess?.editFunction == true) //Edit Function
                            {
                                ViewBag.Title = "Edit User";
                            }
                            else //View Function
                            {
                                ViewBag.Title = "View User";
                            }

                            TempData["page"] = "userlist";
                        }
                        //get specific user based on id
                          user = db.Database.SqlQuery<SP_GetUserProfiles_Result>(
                     "SP_GetUserProfiles @StaffNo",
                     new SqlParameter("StaffNo", id)

                 ).FirstOrDefault();
                    }
                            if (user == null)
                            {
                                return RedirectToAction("NotFound", "Home");
                            }
                            else
                            {
                        var activeLocationForView = db.Database.SqlQuery<SP_GetLocationsWithFactory_Result>(
                                      "SP_GetLocationsWithFactory @LocationCode ,@StatusOptions,@Criteria,@Text,@DateType,@StartDate,@EndDate,@StaffNo,@ControlBy",
                                      new SqlParameter("LocationCode", ""),
                                      new SqlParameter("StatusOptions", "A"),
                                      new SqlParameter("Criteria", ""),
                                      new SqlParameter("Text", ""),
                                      new SqlParameter("DateType", ""),
                                      new SqlParameter("StartDate", ""),
                                      new SqlParameter("EndDate", ""),
                                      new SqlParameter("StaffNo", ""),
                                      new SqlParameter("ControlBy", id)).ToList();
                        string SecretKey = ConfigurationManager.AppSettings["SecretKey"];
                                long SecretKey_8byte = HomeController.StringTo8ByteValue(SecretKey);

                                long staff_no_8byte = HomeController.StringTo8ByteValue(user.staff_no);
                                user.password = HomeController.Decrypt(user.password, SecretKey_8byte.ToString(), staff_no_8byte.ToString());



                                //get the user's view department data and save in a variable

                                var userDepartments =
                                    db.Database.SqlQuery<string>("SP_GetViewDepartments @StaffNo", new SqlParameter("StaffNo", user.staff_no)).ToList();
                                // "HR", "Finance", and "IT"

                                string departmentValues = string.Join(",", userDepartments); //"HR,Finance,IT"
                                if (userDepartments.Count >0 && departmentValues=="")
                                {
                                    departmentValues = "Get All";
                                }
                                

                        var userFactory =
                                    db.Database.SqlQuery<string>("SP_GetViewFactorys @StaffNo", new SqlParameter("StaffNo", user.staff_no)).ToList();
                                // "BP", "KULAI", and "JB"
                                string factoryValues = string.Join(",", userFactory);//"BP,KULAI,JB"
                        if (userFactory.Count > 0 && factoryValues == "")
                        {
                            factoryValues = "Get All";
                        }
                        //get the user's view location data and save in a variable

                        var userLocation =
                             db.Database.SqlQuery<string>("SP_GetViewLocations @StaffNo", new SqlParameter("StaffNo", user.staff_no)).ToList();
                        // "BP", "KULAI", and "JB"
                        string locationValues = string.Join(",", userLocation);//"BP,KULAI,JB"
                        if (userLocation.Count > 0 && locationValues == "")
                        {
                            locationValues = "Get All";
                        }
                        activeLocation = db.Database.SqlQuery<SP_GetLocationsWithFactory_Result>(
                                    "SP_GetLocationsWithFactory @LocationCode ,@StatusOptions ,@Criteria,@Text,@DateType,@StartDate,@EndDate,@StaffNo,@ControlBy,@FactoryID",
                                    new SqlParameter("LocationCode", ""),
                                    new SqlParameter("StatusOptions", "A"),
                                    new SqlParameter("Criteria", ""),
                                    new SqlParameter("Text", ""),
                                    new SqlParameter("DateType", ""),
                                    new SqlParameter("StartDate", ""),
                                    new SqlParameter("EndDate", ""),
                                    new SqlParameter("StaffNo", ""),
                                     new SqlParameter("ControlBy",""),
                                     new SqlParameter("FactoryID", user.factory)).ToList();
                        ViewBag.location_code = new SelectList(
                       activeLocation.Select(l => new SelectListItem
                       {
                           Value = l.location_code,
                           Text = l.factory + "-" + l.name
                       }),
                       "Value", "Text"
                   );
                        // Pass the all this data in viewbag which send the data to the view
                        ViewBag.DepartmentValues = departmentValues;
                                ViewBag.FactoryValues = factoryValues;
                                ViewBag.LocationValues = locationValues;
                                ViewBag.DepartmentList = activeDepartment;
                                ViewBag.FactoryList = activeFactory;
                                ViewBag.LocationList = activeLocationForView;
                                return View(user);
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

        // POST: User/Create
        [HttpPost]
        public JsonResult Create(tbl_user user)
        {
            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    if (string.IsNullOrEmpty(user.name) || string.IsNullOrEmpty(user.password)
                    || string.IsNullOrEmpty(user.staff_no) || string.IsNullOrEmpty(user.department)
                    || string.IsNullOrEmpty(user.location) || string.IsNullOrEmpty(user.role)
                    || string.IsNullOrEmpty(user.factory) || string.IsNullOrEmpty(user.timezone))
                    {
                        return Json(new { success = false, error_message = "Make sure all fields are filled" });
                    }

                    if (user.name.Contains("admin")|| user.name.Contains("administrator")||user.staff_no.Contains("admin")|| user.staff_no.Contains("administrator"))
                    {
                        return Json(new { success = false, error_message = "You are not allow to use 'admin' or 'administrator' as your name or staff no !" });
                    }
                        //if the user_id is existing in db, alert user
                        if (db.tbl_user.Any(p => p.staff_no.ToUpper() == user.staff_no.ToUpper() && p.factory.ToUpper() == user.factory.ToUpper()))
                    {
                        return Json(new { success = false, error_message = "This Staff No. is already in use.", idError = "This Staff No. is already in use." });
                    }
                    else
                    {
                        user.status = "A";
                        user.createDate = DateTime.Now;
                        user.createBy = (string)Session["user_id"];
                        string SecretKey = ConfigurationManager.AppSettings["SecretKey"];
                        long SecretKey_8byte = HomeController.StringTo8ByteValue(SecretKey);

                        long staff_no_8byte = HomeController.StringTo8ByteValue(user.staff_no);
                        user.password = HomeController.Encrypt(user.password, SecretKey_8byte.ToString(), staff_no_8byte.ToString());
                        
                        db.tbl_user.Add(user);
                        db.SaveChanges();
                        TempData["SuccessMessage"] = "Added User Successfully.";
                        //after added successfully go to edit user page
                        return Json(new { success = true, message = "Added User Successfully." });
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
        public JsonResult Edit(tbl_user user)
        {
            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    if (string.IsNullOrEmpty(user.name) || string.IsNullOrEmpty(user.password)
                    || string.IsNullOrEmpty(user.staff_no) || string.IsNullOrEmpty(user.department)
                    || string.IsNullOrEmpty(user.location) || string.IsNullOrEmpty(user.role)
                    || string.IsNullOrEmpty(user.factory) || string.IsNullOrEmpty(user.timezone))
                    {
                        return Json(new { success = false, error_message = "Make sure all fields are filled" });
                    }
                    //if the user_id is existing in db, alert user
                    var existingUser = db.tbl_user.SingleOrDefault(u => u.staff_no.ToUpper() == user.staff_no.ToUpper());

                    if (existingUser != null)
                    {

                        existingUser.editDate = DateTime.Now;
                        existingUser.editBy = (string)Session["user_id"];


                        string SecretKey = ConfigurationManager.AppSettings["SecretKey"];
                        long SecretKey_8byte = HomeController.StringTo8ByteValue(SecretKey);
                        long staff_no_8byte = HomeController.StringTo8ByteValue(user.staff_no);
                        existingUser.password = HomeController.Encrypt(user.password, SecretKey_8byte.ToString(), staff_no_8byte.ToString());


                        existingUser.name = user.name;
                        existingUser.email = user.email;
                        existingUser.role = user.role;
                        existingUser.department = user.department;
                        existingUser.factory = user.factory;
                        existingUser.location = user.location;
                        existingUser.timezone = user.timezone;
                        db.Entry(existingUser).State = EntityState.Modified;
                        db.SaveChanges();


                        if (user.staff_no == (string)Session["user_id"])
                        {
                            var userProfile = db.Database.SqlQuery<SP_GetUserProfiles_Result>(
                            "SP_GetUserProfiles @StaffNo",
                            new SqlParameter("StaffNo", (string)Session["user_id"])).FirstOrDefault();


                            Session["role_id"] = userProfile.role;
                            Session["timezone"] = userProfile.timezone;



                        }
                        TempData["SuccessMessage"] = "Updated Successfully.";
                        return Json(new { success = true, message = "Updated Successfully." });
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
        public JsonResult RemoveView(List<string> model, string staff_no, string type) //delete access location function
        {
            try
            {
                List<SP_GetLocationsWithFactory_Result> activeLocation = null;
                List<string> userLocation = null;
                using (var db = new AttendNow_DBEntities())
                {
                    if (model.FirstOrDefault() == "Get All")
                    {
                        model[0] = "A";
                    }
                    foreach (var data in model)
                    {
                       
                        
                        // Find the entity in the database using LINQ
                        switch (type)
                        {
                            case "factory":
                               var existingFactory = db.tbl_view_factory.FirstOrDefault(vf => vf.staff_no == staff_no && vf.factory_id == data &&vf.status!="V");
                                if (existingFactory != null)
                                {
                                    existingFactory.status = "V";
                                    existingFactory.editBy = (string)Session["user_id"];
                                    existingFactory.editDate = DateTime.Now;
                                    var existingViewLocation = db.tbl_view_location
                                        .Where(vl => vl.staff_no == staff_no && (vl.factory == existingFactory.factory_id || vl.location=="A" 
                                        ||existingFactory.factory_id=="A") && vl.status != "V")
                                        .ToList();

                                    foreach (var viewLocation in existingViewLocation)
                                    {
                                        viewLocation.status = "V";
                                        viewLocation.editBy = (string)Session["user_id"];
                                        viewLocation.editDate = DateTime.Now;
                                    }
                                }
                                break;
                            case "department":
                                var existingDepartment = db.tbl_view_department.FirstOrDefault(vd => vd.staff_no == staff_no && vd.department == data &&vd.status!="V");
                                if (existingDepartment != null)
                                {
                                    existingDepartment.status = "V";
                                    existingDepartment.editBy = (string)Session["user_id"];
                                    existingDepartment.editDate = DateTime.Now;
                                }
                                break;
                            case "location":
                                string[] splitData = data.Split('-');

                                // Now modelParts array will contain {"FA", "BPA"}
                                string factory = splitData[0];  // "FA"
                                string location = "";
                                if (factory != "A")
                                {
                                   location = splitData[1];
                                }
                                
                                var existingLocation = db.tbl_view_location.FirstOrDefault(vl => vl.staff_no == staff_no && (vl.location == location && vl.factory==factory ||vl.location=="A") && vl.status!="V");
                                if (existingLocation != null)
                                {
                                    existingLocation.status = "V";
                                    existingLocation.editBy = (string)Session["user_id"];
                                    existingLocation.editDate = DateTime.Now;
                                }
                                break;
                        }



                       
                    }

                    var existingUser = db.tbl_user.SingleOrDefault(u => u.staff_no.ToUpper() == staff_no.ToUpper());

                    if (existingUser != null)
                    {

                        existingUser.editDate = DateTime.Now;
                        existingUser.editBy = (string)Session["user_id"];
                    }
                        // Save changes to the database
                        db.SaveChanges();
                    var existingControlUser = db.Database.SqlQuery<SP_GetUserProfiles_Result>("SP_GetUserProfiles @StaffNo",
                                                   new SqlParameter("StaffNo", (string)Session["user_id"])
                                               ).FirstOrDefault();

                    var userEditBy = existingControlUser.EditByName + " (" + existingControlUser.editBy.ToUpper() + ")";
                    var editDate = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                    if (type == "factory") {

                        activeLocation = db.Database.SqlQuery<SP_GetLocationsWithFactory_Result>(
                                      "SP_GetLocationsWithFactory @LocationCode ,@StatusOptions,@Criteria,@Text,@DateType,@StartDate,@EndDate,@StaffNo,@ControlBy",
                                      new SqlParameter("LocationCode", ""),
                                      new SqlParameter("StatusOptions", "A"),
                                      new SqlParameter("Criteria", ""),
                                      new SqlParameter("Text", ""),
                                      new SqlParameter("DateType", ""),
                                      new SqlParameter("StartDate", ""),
                                      new SqlParameter("EndDate", ""),
                                      new SqlParameter("StaffNo", ""),
                                      new SqlParameter("ControlBy", staff_no)).ToList();

                         userLocation =
                                   db.Database.SqlQuery<string>("SP_GetViewLocations @StaffNo", new SqlParameter("StaffNo", staff_no)).ToList();
                    }
                   
                   
                    return Json(new
                    {
                        success = true,
                        message = "Removed View "+ Char.ToUpper(type[0]) + type.Substring(1)+ " Sucessfully",
                        activeLocation = activeLocation,
                        userLocation = userLocation,
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
        /*------------------------------------------Manage View Function END------------------------------------------*/


        [HttpPost]
        public JsonResult AddView(List<string> model, string staff_no, string type) //delete access location function
        {
            try
            {
                List<SP_GetLocationsWithFactory_Result> activeLocation = null;

                using (var db = new AttendNow_DBEntities())
                {
                    var addType = "";
                        // Find the entity in the database using LINQ
                        switch (type)
                        {
                            case "factory":
                            /* List<tbl_view_factory> viewFactory = new List<tbl_view_factory>();


                             // Populate the list based on the model data
                             foreach (var data in model)
                             {
                             var existingFactory = db.tbl_view_factory.FirstOrDefault(vf => vf.factory_id.ToUpper() == data.ToUpper()&&vf.staff_no==staff_no );
                             if (existingFactory!=null)
                             {
                                 existingFactory.status = "A";
                                 existingFactory.editBy = (string)Session["user_id"]; ;
                                 existingFactory.editDate= DateTime.Now;
                             }
                             else
                             {
                                 viewFactory.Add(new tbl_view_factory
                                 {
                                     factory_id = data,
                                     staff_no = staff_no,
                                     createBy = (string)Session["user_id"],
                                     createDate = DateTime.Now,
                                     status = "A"

                                 });
                             }

                             }
                             db.tbl_view_factory.AddRange(viewFactory);
                             break;*/
                            List<tbl_view_factory> viewFactory = new List<tbl_view_factory>();

                            // Populate the list based on the model data
                            if (model.FirstOrDefault() == "A" && model.Count == 1) //When select get all
                            {
                                var existingFactorys = db.tbl_view_factory //Get all existing view factory
                                .Where(f => f.staff_no == staff_no)
                                .ToList();


                                var existingGetAll = db.tbl_view_factory
                                .Where(f => f.staff_no == staff_no && f.factory_id == "A").FirstOrDefault(); //get "get all" view factory 
                                if (existingGetAll == null) //if "get all" view factory is null then add this into database
                                {
                                    viewFactory.Add(new tbl_view_factory
                                    {
                                        factory_id = "A",
                                        staff_no = staff_no,
                                        createBy = (string)Session["user_id"],
                                        createDate = DateTime.Now,
                                        status = "A"

                                    });
                                    db.tbl_view_factory.AddRange(viewFactory);
                                }
                                else
                                {
                                    // Update each record in the retrieved set
                                    foreach (var existingFactory in existingFactorys)
                                    {
                                        if (existingFactory.factory_id != "A") //Invalid all other view factory
                                        {
                                            existingFactory.status = "V";
                                            
                                        }
                                        else
                                        {
                                            existingFactory.status = "A";
                                        }
                                        existingFactory.editBy = (string)Session["user_id"]; ;
                                        existingFactory.editDate = DateTime.Now;

                                    }
                                }
                                addType = "A";
                            }
                            else
                            { //When not select "GetAll"
                                var existingFactory = db.tbl_view_factory
                                .Where(f => f.staff_no == staff_no && f.factory_id == "A" && f.status == "A")
                                .FirstOrDefault();
                                if (existingFactory == null) //when "get all" view factory is null, then allow to add view factory
                                {
                                    foreach (var data in model)
                                    {
                                        var factory = db.tbl_view_factory.FirstOrDefault(vd => vd.factory_id.ToUpper() == data.ToUpper() && vd.staff_no == staff_no);
                                        if (factory != null) //if in database has data then just upload status to active
                                        {
                                            factory.status = "A";
                                            factory.editBy = (string)Session["user_id"]; ;
                                            factory.editDate = DateTime.Now;
                                        }
                                        else //if database has no data then add this new data to database
                                        {
                                            viewFactory.Add(new tbl_view_factory
                                            {
                                                factory_id = data,
                                                staff_no = staff_no,
                                                createBy = (string)Session["user_id"],
                                                createDate = DateTime.Now,
                                                status = "A"

                                            });
                                        }

                                    }
                                    db.tbl_view_factory.AddRange(viewFactory);
                                }
                                else //If the view department already is "get all" then cannot change anythings
                                {
                                    addType = "A";
                                }

                            }

                            break;
                        case "department":
                                List<tbl_view_department> viewDepartment = new List<tbl_view_department>();

                                // Populate the list based on the model data
                                if (model.FirstOrDefault() == "A" &&model.Count==1) //When select get all
                                {
                                    var existingDepartments = db.tbl_view_department //Get all existing view department
                                    .Where(d => d.staff_no == staff_no)
                                    .ToList();

                                
                                    var existingGetAll = db.tbl_view_department
                                    .Where(d => d.staff_no == staff_no && d.department == "A").FirstOrDefault(); //get "get all" view department 
                                    if (existingGetAll == null) //if "get all" view department is null then add this into database
                                    {
                                        viewDepartment.Add(new tbl_view_department
                                        {
                                            department = "A",
                                            staff_no = staff_no,
                                            createBy = (string)Session["user_id"],
                                            createDate = DateTime.Now,
                                            status = "A"

                                        });
                                        db.tbl_view_department.AddRange(viewDepartment);
                                    }
                                    else
                                    {
                                        // Update each record in the retrieved set
                                        foreach (var existingDepartment in existingDepartments)
                                        {
                                            if (existingDepartment.department != "A") //Invalid all other view department
                                            {
                                                existingDepartment.status = "V";
                                                
                                            }
                                            else
                                            {
                                                existingDepartment.status = "A";
                                            }
                                            existingDepartment.editBy = (string)Session["user_id"]; ;
                                            existingDepartment.editDate = DateTime.Now;
                                    }
                                    }
                                    addType = "A";
                                }
                                else
                                { //When not select "GetAll"
                                    var existingDepartment = db.tbl_view_department
                                    .Where(d => d.staff_no == staff_no &&d.department=="A" &&d.status=="A")
                                    .FirstOrDefault();
                                    if (existingDepartment == null) //when "get all" view department is null, then allow to add view department
                                    {
                                        foreach (var data in model)
                                        {
                                            var department = db.tbl_view_department.FirstOrDefault(vd => vd.department.ToUpper() == data.ToUpper() && vd.staff_no == staff_no);
                                            if (department != null) //if in database has data then just upload status to active
                                            {
                                                department.status = "A";
                                                department.editBy = (string)Session["user_id"]; ;
                                                department.editDate = DateTime.Now;
                                            }
                                            else //if database has no data then add this new data to database
                                            {
                                                viewDepartment.Add(new tbl_view_department
                                                {
                                                    department = data,
                                                    staff_no = staff_no,
                                                    createBy = (string)Session["user_id"],
                                                    createDate = DateTime.Now,
                                                    status = "A"

                                                });
                                            }

                                        }
                                        db.tbl_view_department.AddRange(viewDepartment);
                                    }
                                    else //If the view department already is "get all" then cannot change anythings
                                    {
                                        addType = "A";
                                    }
                               
                                }
                            
                                break;
                        case "location":
                            List<tbl_view_location> viewLocation = new List<tbl_view_location>();

                            // Populate the list based on the model data
                            if (model.FirstOrDefault() == "A" && model.Count == 1) //When select get all
                            {
                                var existingLocations = db.tbl_view_location //Get all existing view location
                                .Where(d => d.staff_no == staff_no)
                                .ToList();


                                var existingGetAll = db.tbl_view_location
                                .Where(d => d.staff_no == staff_no && d.location == "A").FirstOrDefault(); //get "get all" view location 
                                if (existingGetAll == null) //if "get all" view location is null then add this into database
                                {
                                    viewLocation.Add(new tbl_view_location
                                    {
                                        location = "A",
                                        staff_no = staff_no,
                                        createBy = (string)Session["user_id"],
                                        createDate = DateTime.Now,
                                        status = "A"

                                    });
                                    db.tbl_view_location.AddRange(viewLocation);
                                }
                                else
                                {
                                    // Update each record in the retrieved set
                                    foreach (var existingLocation in existingLocations)
                                    {
                                        if (existingLocation.location != "A") //Invalid all other view location
                                        {
                                            existingLocation.status = "V";
                                            
                                        }
                                        else
                                        {
                                            existingLocation.status = "A";
                                        }
                                        existingLocation.editBy = (string)Session["user_id"]; ;
                                        existingLocation.editDate = DateTime.Now;
                                    }
                                }
                                addType = "A";
                            }
                            else
                            { //When not select "GetAll"
                                var existingLocation = db.tbl_view_location
                                .Where(d => d.staff_no == staff_no && d.location == "A" && d.status == "A")
                                .FirstOrDefault();
                                if (existingLocation == null) //when "get all" view location is null, then allow to add view location
                                {
                                    foreach (var data in model)
                                    {
                                        string[] splitData = data.Split('-');

                                        // Now modelParts array will contain {"FA", "BPA"}
                                        string factoryData = splitData[0];  // "FA"
                                        string locationData = splitData[1];
                                        var location = db.tbl_view_location.FirstOrDefault(vd => vd.location.ToUpper() == locationData.ToUpper() 
                                        &&vd.factory== factoryData.ToUpper() && vd.staff_no == staff_no);

                                        if (location != null) //if in database has data then just upload status to active
                                        {
                                            location.status = "A";
                                            location.editBy = (string)Session["user_id"]; ;
                                            location.editDate = DateTime.Now;
                                        }
                                        else //if database has no data then add this new data to database
                                        {
                                            viewLocation.Add(new tbl_view_location
                                            {
                                                location = locationData,
                                                factory = factoryData,
                                                staff_no = staff_no,
                                                createBy = (string)Session["user_id"],
                                                createDate = DateTime.Now,
                                                status = "A"

                                            });
                                        }

                                    }
                                    db.tbl_view_location.AddRange(viewLocation);
                                }
                                else //If the view location already is "get all" then cannot change anythings
                                {
                                    addType = "A";
                                }

                            }

                            break;
                    }

                    var existingUser = db.tbl_user.SingleOrDefault(u => u.staff_no.ToUpper() == staff_no.ToUpper());

                    if (existingUser != null)
                    {

                        existingUser.editDate = DateTime.Now;
                        existingUser.editBy = (string)Session["user_id"];
                    }
                    // Save changes to the database
                    db.SaveChanges();
                    var existingControlUser = db.Database.SqlQuery<SP_GetUserProfiles_Result>("SP_GetUserProfiles @StaffNo",
                                                   new SqlParameter("StaffNo", (string)Session["user_id"])
                                               ).FirstOrDefault();
                    
                    var userEditBy = existingControlUser.EditByName+" ("+ existingControlUser.editBy.ToUpper()+")";
                    var editDate = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                    if (type == "factory")
                    {
                        activeLocation = db.Database.SqlQuery<SP_GetLocationsWithFactory_Result>(
                                      "SP_GetLocationsWithFactory @LocationCode ,@StatusOptions,@Criteria,@Text,@DateType,@StartDate,@EndDate,@StaffNo,@ControlBy",
                                      new SqlParameter("LocationCode", ""),
                                      new SqlParameter("StatusOptions", "A"),
                                      new SqlParameter("Criteria", ""),
                                      new SqlParameter("Text", ""),
                                      new SqlParameter("DateType", ""),
                                      new SqlParameter("StartDate", ""),
                                      new SqlParameter("EndDate", ""),
                                      new SqlParameter("StaffNo", ""),
                                      new SqlParameter("ControlBy", staff_no)).ToList();
                    }


                    return Json(new
                    {
                        success = true,
                        message = "Added View " + Char.ToUpper(type[0]) + type.Substring(1) + " Sucessfully",
                        activeLocation = activeLocation,
                        userEditBy = userEditBy,
                        userEditDate = editDate,
                        addType= addType
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
        /*------------------------------------------Manage View Function END------------------------------------------*/

        /*------------------------------------------UpdateStatus Function START------------------------------------------*/
        public JsonResult UpdateStatus(String id, string newStatus)
        {

            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    // Find the existing user in the database by user_id
                    var existingUser = db.tbl_user.SingleOrDefault(u => u.staff_no.ToUpper() == id.ToUpper());

                    if (existingUser != null)
                    {
                        // Update the status property based on the newStatus parameter
                        existingUser.status = newStatus;
                        existingUser.editBy = (string)Session["user_id"];
                        existingUser.editDate = DateTime.Now;
                        var existingControlUser = db.Database.SqlQuery<SP_GetUserProfiles_Result>("SP_GetUserProfiles @StaffNo",
                                                 new SqlParameter("StaffNo", (string)Session["user_id"])
                                             ).FirstOrDefault();

                        var userEditBy = existingControlUser.EditByName + " (" + existingControlUser.editBy.ToUpper() + ")";
                        var editDate = existingUser.editDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                        db.Entry(existingUser).State = EntityState.Modified;
                        db.SaveChanges();
                        if (id == (string)Session["user_id"])
                        {
                            return Json(new { success = false, logout = "Not Existing User." });




                        }

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
                    // Format the result as a string
                    
                   
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
                    var userAccess = db.sys_setting_role_permission.SingleOrDefault(rp => rp.role == roleID && rp.module == "user");

                    if (userAccess.deleteFunction != true)
                    {
                        statusOptionsString = "A";
                    }
                    var users = db.Database.SqlQuery<SP_GetUserProfiles_Result>(
                        "SP_GetUserProfiles @StaffNo, @StatusOptions, @Criteria, @Text, @DateType, @StartDate, @EndDate",
                        new SqlParameter("StaffNo", ""),
                        new SqlParameter("StatusOptions", statusOptionsString),
                        new SqlParameter("Criteria", criteria),
                        new SqlParameter("Text", text),
                        new SqlParameter("DateType", dateType),
                        new SqlParameter("StartDate", startDate),
                        new SqlParameter("EndDate", endDate)
                    ).ToList();


                    var user_id= Session["user_id"]?.ToString();
                    List<sys_setting_define_table> defineTableList= db.sys_setting_define_table
                     .Where(item => item.createBy == user_id && item.module == "user")
                     .ToList();
                    ViewBag.userAccess = userAccess;
                    var result = users.Select(u => new
                    {
                        link = ((userAccess?.status == true && userAccess?.viewFunction == true) || (userAccess?.status == true && userAccess?.editFunction == true))
                            ? $@"class='clickable-row' onclick=""window.location.href = '{Url.Action("MainForm", "User", new { id = u.staff_no })}'"""
                            : "",
                        staff_no = (defineTableList.Any(dt => dt.field == "staff_no" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.staff_no.ToUpper(): "NS",
                        name=(defineTableList.Any(dt => dt.field == "name" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.name: "NS",
                        factory = (defineTableList.Any(dt => dt.field == "factory" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ? u.FactoryName + " (" + u.factory.ToUpper() + ")" : "NS",
                        department = (defineTableList.Any(dt => dt.field == "department" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.DepartmentName + " (" + u.department.ToUpper() + ")": "NS",
                        location = (defineTableList.Any(dt => dt.field == "location" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.LocationName + " (" + u.location.ToUpper() + ")": "NS",
                        role = (defineTableList.Any(dt => dt.field == "role" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.RoleName: "NS",
                        email = (defineTableList.Any(dt => dt.field == "email" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ? u.email : "NS",
                        createdBy = (defineTableList.Any(dt => dt.field == "createBy" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.CreateByName + " (" + u.createBy.ToUpper() + ")": "NS",
                        createdAt = (defineTableList.Any(dt => dt.field == "createDate" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?"<span class='createDate'>"+ u.createDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture)+"</span>": "NS",
                        edited_by = (defineTableList.Any(dt => dt.field == "editBy" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.EditByName != null && !string.IsNullOrWhiteSpace(u.editBy) ? u.EditByName + " (" + u.editBy.ToUpper() + ")" : "<span class='text-danger'>No data</span>": "NS",
                        edited_at = (defineTableList.Any(dt => dt.field == "editDate" && dt.status == "A") || (defineTableList != null && defineTableList.Count == 0)) ?u.editDate != null ? "<span class='editDate'>" + u.editDate?.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture) + "</span>"
                                    : "<span class='text-danger'>No data</span>": "NS",
                        status = (userAccess?.status == true && userAccess?.deleteFunction == true) ?
                                u.status == "A"
                                ? "<span class='text-primary'>Active</span>"
                                : "<span class='text-danger'>Inactive</span>"
                                : "NS",
                        editBtn = (userAccess?.status == true && userAccess?.editFunction == true)
                                    ? $@"<a href=""{Url.Action("MainForm", "User", new { id = u.staff_no })}"" class=""btn btn-primary"">Edit</a>"
                                    : (userAccess?.status == true && userAccess?.viewFunction == true)
                                    ? $@"<a href=""{Url.Action("MainForm", "User", new { id = u.staff_no })}"" class=""btn btn-primary"">View</a>" : "",
                        statusBtn = (userAccess?.status == true && userAccess?.deleteFunction == true) ?
                                    u.status == "A"
                                    ? $@"<button style='width:6em;' class='btn btn-danger  ml-1' onclick='updateStatus(""User"", ""{u.staff_no}"", ""{u.status}"","""",, ""User Report"", ""Users"", ""{Session["timezone"]}"")'>Inactive</button>"
                                    : $@"<button style='width:6em;' class='btn btn-info  ml-1' onclick='updateStatus(""User"", ""{u.staff_no}"", ""{u.status}"","""", ""User Report"", ""Users"", ""{Session["timezone"]}"")'>Reactive</button>"
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



        /*------------------------------------------Login and Log Out Function START------------------------------------------*/
        public ActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public JsonResult Login(string staff_no, string password)
        {
            try
            {
                using (var db = new AttendNow_DBEntities())
                {
                    if (string.IsNullOrEmpty(staff_no) || string.IsNullOrEmpty(password))
                    {
                        return Json(new { success = false, error_message = "Make sure all fields are filled." });
                    }
                    string SecretKey = ConfigurationManager.AppSettings["SecretKey"];
                    long SecretKey_8byte = HomeController.StringTo8ByteValue(SecretKey);

                    long staff_no_8byte = HomeController.StringTo8ByteValue(staff_no);



                    var encryptedpassword = HomeController.Encrypt(password, SecretKey_8byte.ToString(), staff_no_8byte.ToString()); // Encrypt function to encrypt password

                    var staffNo = new SqlParameter("@StaffNo", staff_no);
                    var encryptedPassword = new SqlParameter("@EncryptedPassword", encryptedpassword);

                    // Call the stored procedure and map the result to your model
                    var existingUser = db.Database.SqlQuery<SP_CheckUserCredentials_Result>("SP_CheckUserCredentials @StaffNo, @EncryptedPassword", staffNo, encryptedPassword).ToList();


                    if (existingUser.Any())
                    {
                        //save important data in session
                        Session["user_id"] = existingUser.FirstOrDefault().staff_no;

                        Session["role_id"] = existingUser.FirstOrDefault().role;
                        Session["timezone"] = existingUser.FirstOrDefault().timezone;
                        TempData["SuccessMessage"] = "Login Successfully. Welcome to AttendNow";
                        return Json(new { success = true, message = "Login Successfully. Welcome to AttendNow." });
                    }
                    else
                    {

                        return Json(new { success = false, error_message = "Wrong Username or Password !!!" });
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

        public ActionResult Logout()
        {
            // Clear user-specific session data
            Session.Remove("user_id");

            Session.Remove("role_id");

            // Abandon the session and redirect to the login page
            Session.Abandon();

            return RedirectToAction("Login", "User");
        }
        /*------------------------------------------Login and Log Out Function END------------------------------------------*/
    }
}
