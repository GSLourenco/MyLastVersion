using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Text;
using MvcApplication2.DataModel;
using MvcApplication2.Models;
using System.Security.Principal;

namespace MvcApplication2.Controllers
{
    public class AccountController : ApiController
    {
        public HttpResponseMessage ValidateLogIn(LoginModel log)
        {
            if (!this.ModelState.IsValid || log == null )
                return this.Request.CreateResponse(HttpStatusCode.BadRequest,ModelState);

            try
            {
                // check if credentials are valid
                String decodedUsername = Encoding.UTF8.GetString(Convert.FromBase64String(log.EmailId));
                String decodedPassword = Encoding.UTF8.GetString(Convert.FromBase64String(log.Password));
                if (!PictogramsDb.IsUserExist(decodedUsername, decodedPassword))
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }
            }
            catch (FormatException e)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Parâmetros não foram bem formatados"); 
            }
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

       
        [BasicAuthenticationAttributeWithPassword,HttpPut]
        public HttpResponseMessage TryUpdateAccountTraffic(int size)
        {
            //check for limit sizes
            if(size<512 || size > 2000000)
                return new HttpResponseMessage(HttpStatusCode.BadRequest);

            GenericIdentity idd = (GenericIdentity)System.Web.HttpContext.Current.User.Identity;
            String username = idd.Name;

            bool check = PictogramsDb.tryUpdateTraffic(username, size);
            if (!check) return new HttpResponseMessage(HttpStatusCode.Forbidden);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }


    }
}
