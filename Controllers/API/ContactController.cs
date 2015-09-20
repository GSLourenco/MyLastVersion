﻿using System;
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
            if (!this.ModelState.IsValid || contact.Trim() == "" || contact.Length > 20){
                return  this.Request.CreateResponse(HttpStatusCode.BadRequest, "O contacto que inseriu não é válido");
            }

            GenericIdentity idd = (GenericIdentity)System.Web.HttpContext.Current.User.Identity;
            String name = idd.Name;
            //check if contact already exists
            int idx = PictogramsDb.getContactId(contact, name);
            if (idx > 0){
               return this.Request.CreateResponse(HttpStatusCode.BadRequest, "O contacto que quer adicionar já existe, escolha outro nome");
            }

            //generate random code to that contact
            Random rnd = new Random();
            int code = rnd.Next(10000, 99999);
            String status = "Código não inserido";

            //try to insert code, if already exists, a new one is generated
            while (!PictogramsDb.InsertTemporaryCode(contact, code, status, name))
            {
                code = rnd.Next(10000, 99999);
            }

            return this.Request.CreateResponse(HttpStatusCode.Created,code);
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

            //delete contact
            PictogramsDb.DeleteUser(contactName, username);
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        }
    }
}