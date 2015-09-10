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
        public List<ContactModel> getContacts()
        {
            GenericIdentity idd = (GenericIdentity)System.Web.HttpContext.Current.User.Identity;
            String email = idd.Name;

            List<ContactModel> list = PictogramsDb.getUsersModelFrom(email);
            return list;

        }

        [BasicAuthenticationAttributeWithPassword]
        public HttpResponseMessage CreateContact(String contact)
        {
            if (!this.ModelState.IsValid || contact.Trim() == "" || contact.Length > 20)
            {
                HttpResponseMessage response = this.Request.CreateResponse(HttpStatusCode.BadRequest, "O contacto que inseriu não é válido");
                return response;
            }

            GenericIdentity idd = (GenericIdentity)System.Web.HttpContext.Current.User.Identity;
            String name = idd.Name;
            int idx = PictogramsDb.getContactId(contact, name);
            if (idx > 0)
            {
                HttpResponseMessage response = this.Request.CreateResponse(HttpStatusCode.BadRequest, "O contacto que quer adicionar já existe, escolha outro nome");
                return response;
            }

            Random rnd = new Random();
            int code = rnd.Next(10000, 99999);
            String status = "Código não inserido";

            while (!PictogramsDb.InsertTemporaryCode(contact, code, status, name))
            {
                code = rnd.Next(10000, 99999);
            }

            HttpResponseMessage res = this.Request.CreateResponse(HttpStatusCode.Created,code);
            return res;

        }

        [BasicAuthenticationAttributeWithPassword]
        public HttpResponseMessage DeleteContact(int id)
        {
            if (!this.ModelState.IsValid || id < 0)
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest));

            GenericIdentity idd = (GenericIdentity)System.Web.HttpContext.Current.User.Identity;
            String username = idd.Name;
            //check if reminder is from user

            String contactName = PictogramsDb.getUserName(id);
            if (contactName == null) return this.Request.CreateResponse(HttpStatusCode.BadRequest);


            bool check = PictogramsDb.DeleteUser(contactName, username);
            if (!check) return this.Request.CreateResponse(HttpStatusCode.NotFound);
            return this.Request.CreateResponse(HttpStatusCode.NoContent);
        }
    }
}
