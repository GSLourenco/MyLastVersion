using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Text;
using MvcApplication2.DataModel;
using MvcApplication2.Models;

namespace MvcApplication2.Controllers
{
    public class AccountController : ApiController
    {
        public HttpResponseMessage ValidateLogIn(Login log)
        {
            if(log.EmailId == null || log.Password == null)
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest));
            try
            {
                String decodedUsername = Encoding.UTF8.GetString(Convert.FromBase64String(log.EmailId));
                String decodedPassword = Encoding.UTF8.GetString(Convert.FromBase64String(log.Password));
                if (!PictogramsDb.IsUserExist(decodedUsername, decodedPassword))
                {
                    return this.Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
            }
            catch (FormatException e)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Parâmetros não foram bem formatados"); 
            }
            return this.Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
