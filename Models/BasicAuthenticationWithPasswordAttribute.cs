using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Filters;
using System.Web.Http.Controllers;
using System.Net.Http;
using System.Text;
using System.Security.Principal;
using MvcApplication2.DataModel;

namespace MvcApplication2.Models
{
    public class BasicAuthenticationAttributeWithPasswordAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext.Request.Headers.Authorization == null)
            {
                actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            }
            else
            {
                string authToken = actionContext.Request.Headers.Authorization.Parameter;
                string decodedToken = Encoding.UTF8.GetString(Convert.FromBase64String(authToken));

                String username = decodedToken.Substring(0,decodedToken.IndexOf(':'));
                String password = decodedToken.Substring(decodedToken.IndexOf(':')+1);

                if (PictogramsDb.IsUserExist(username,password))
                {
                    GenericIdentity genericIdentity = new GenericIdentity(username, "");
                    HttpContext.Current.User = new GenericPrincipal(genericIdentity, new string[] { });
                    base.OnActionExecuting(actionContext);
                }

                else
                {
                    actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
                }
            }
        }
    }
}