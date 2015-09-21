using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcApplication2.DataModel;
using MvcApplication2.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Web.Http.Filters;
using System.Web.Http.Controllers;
using System.Net;
using MvcApplication2.GCM;
using System.Web.Security;
using System.Security.Principal;
using System.Data.SqlClient;
using System.IO;


namespace MvcApplication2.Controllers
{
    public class HomeController : Controller
    {

        public ViewResult Error()
        {
            Response.StatusCode = 500;
            return View();
        }

        public ViewResult LogOn()
        {
            return View();
        }
        public ViewResult Register()
        {
            if (Request.Browser.IsMobileDevice)
            {
                return View("RegisterMobile");
            }
            return View();
        }

        public ActionResult LogOut()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("LogOn");
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult LogOn(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.IsUserExist(model.EmailId, model.Password))
                {
                    FormsAuthentication.RedirectFromLoginPage(model.EmailId, false);
                }
                else
                {
                    ModelState.AddModelError("", "EmailId or Password Incorrect.");
                }
            }
            return View(model);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Register(Register model)
        {

            if (ModelState.IsValid)
            {

                if (model.Insert())
                {
                    return RedirectToAction("LogOn", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Email Id Already Exist");
                }

            }
            return View(model);
        }


        public ViewResult Home()
        {
            return View();
        }

        public ViewResult About()
        {
            if (Request.Browser.IsMobileDevice)
            {
                return View("AboutMobile");
            }
            return View();
        }


        public ActionResult GetPictogram(String pictogram)
        {
            if (!this.ModelState.IsValid)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Bad Request");

            List<PictogramModel> list = PictogramsDb.getPictogram(pictogram);
            var json = JsonConvert.SerializeObject(list);
            return Json(list, JsonRequestBehavior.AllowGet);

        }

    }
}
