using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MvcApplication2.Models;
using MvcApplication2.DataModel;
using System.Security.Principal;

namespace MvcApplication2.Controllers
{
    public class ContactController : ApiController
    {

        [BasicAuthenticationAttributeWithPassword]
        public List<ContactModel> GetContacts()
        {
            GenericIdentity idd = (GenericIdentity)System.Web.HttpContext.Current.User.Identity;
            String email = idd.Name;

            return ContactBusinessLayer.GetUsersModel(email);
        }

        [BasicAuthenticationAttributeWithPassword]
        public HttpResponseMessage PostContact([FromBody]String contact)
        {
            //Content-Type: application/x-www-form-urlencoded
            //=name
            if (!this.ModelState.IsValid || contact.Trim() == "" || contact.Length > 20)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "O contacto que inseriu não é válido");
            }

            GenericIdentity idd = (GenericIdentity)System.Web.HttpContext.Current.User.Identity;
            String name = idd.Name;
            int code = ContactBusinessLayer.PostContact(contact, name);

            if (code == 0) {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "O contacto que quer adicionar já existe, escolha outro nome");
            }
           
            return this.Request.CreateResponse(HttpStatusCode.Created, code);
        }

        [BasicAuthenticationAttributeWithPassword]
        public HttpResponseMessage DeleteContact(int id)
        {
            if (!this.ModelState.IsValid || id < 0)
                return new HttpResponseMessage(HttpStatusCode.BadRequest);

            GenericIdentity idd = (GenericIdentity)System.Web.HttpContext.Current.User.Identity;
            String username = idd.Name;

            //check if reminder is from user
            String contactName = PictogramsDb.getUserName(id);
            if (contactName == null) return new HttpResponseMessage(HttpStatusCode.BadRequest);

            ContactBusinessLayer.DeleteContact(contactName, username);
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }
    }
}
