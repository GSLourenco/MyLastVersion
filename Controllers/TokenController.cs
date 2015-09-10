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
        [ActionName("PostCodeAndId")]
        //[BasicAuthenticationAttributeWithPassword]
        public HttpResponseMessage PostCodeAndRegistrationId(UserCode uc)
        {

            if (!this.ModelState.IsValid || uc == null || uc.registration_id == String.Empty || uc.code == "0")
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest));

            String code=uc.code;
            String registration_id=uc.registration_id;

            String[] arr;
            try
            {
                arr = PictogramsDb.ValidateCode(Int32.Parse(code));
            }
            catch (FormatException e)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest));
            }
            if(arr == null) throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            String contact = arr[0];
            String username = arr[1];

            String token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            String  EncryptedToken = Utils.GenerateSaltedSHA1(token);

            PictogramsDb.storeUserCredentials(contact, registration_id, EncryptedToken,username);
            HttpResponseMessage response = this.Request.CreateResponse(HttpStatusCode.Created);

            response.Content = new StringContent(token + " " + username);

           
            return response;

        }

        [BasicAuthenticationAttributeWithPassword]
        public HttpResponseMessage RefreshRegister(String token, String refresh_token)
        {
            if (!this.ModelState.IsValid || token == null || refresh_token == null)
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest));

            return null;
        }

        [BasicAuthentication]
        public HttpResponseMessage DeleteRegister()
        {
            GenericIdentity idd = (GenericIdentity)System.Web.HttpContext.Current.User.Identity;
            String[] credentials = idd.Name.Split(new char[] { ' ' });
            
            String contactName=PictogramsDb.getUserName(Int32.Parse(credentials[0]));
            if (contactName == null) return this.Request.CreateResponse(HttpStatusCode.BadRequest);


            bool check= PictogramsDb.DeleteUser(contactName, credentials[1]);
            if (!check) return this.Request.CreateResponse(HttpStatusCode.NotFound);
            return this.Request.CreateResponse(HttpStatusCode.NoContent);

        }
    }
}
