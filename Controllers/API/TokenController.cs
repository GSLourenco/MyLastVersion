using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;
using MvcApplication2.DataModel;
using MvcApplication2.Models;
using System.Security.Principal;

namespace MvcApplication2.Controllers
{
    public class TokenController : ApiController
    {

        public HttpResponseMessage PostRegister(UserCode uc)
        {

            if (!this.ModelState.IsValid || uc == null || uc.registration_id == String.Empty || uc.code == "0")
                return new HttpResponseMessage(HttpStatusCode.BadRequest);

            String code = uc.code;
            String registration_id = uc.registration_id;

            //check if code send is a number and it´s valid
            String[] arr;
            try
            {
                arr = PictogramsDb.ValidateCode(Int32.Parse(code));
            }
            catch (FormatException e)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            //code was not valid
            if (arr == null) return new HttpResponseMessage(HttpStatusCode.NotFound);
            String contact = arr[0];
            String username = arr[1];

            //generate token that is going to be used to synchronize reminders
            String token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            String EncryptedToken = Utils.GenerateSaltedSHA1(token);

            //add user to contacts list
            PictogramsDb.storeUserCredentials(contact, registration_id, EncryptedToken, username);
            HttpResponseMessage response = this.Request.CreateResponse(HttpStatusCode.Created, token + " " + username);
            return response;

        }

        [BasicAuthentication]
        public HttpResponseMessage DeleteRegister()
        {
            GenericIdentity idd = (GenericIdentity)System.Web.HttpContext.Current.User.Identity;
            String[] credentials = idd.Name.Split(new char[] { ' ' });

            String contactName = PictogramsDb.getUserName(Int32.Parse(credentials[0]));
            if (contactName == null) return new HttpResponseMessage(HttpStatusCode.BadRequest);

            //dele user from contact list
            PictogramsDb.DeleteUser(contactName, credentials[1]);
            return new HttpResponseMessage(HttpStatusCode.NoContent);

        }
    }
}
