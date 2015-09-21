using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MvcApplication2.GCM;
using System.Net;
using System.Web.Mvc;

namespace MvcApplication2.DataModel
{
    public static class ServiceLayer
    {
        public static String SendNotification(String regId, String email)
        {
            Sender s = new Sender(Constants.Project_key);
            Message m = new Message.Builder()
                 .collapseKey("Update reminders")
                 .timeToLive(2419200)
                 .delayWhileIdle(true)
                 .dryRun(false)
                 .addData("Email", email)
                 .build();

            Result r = s.sendNoRetry(m, regId);
            String errorcode = r.getErrorCodeName();

            return errorcode;
        }

        public static HttpStatusCodeResult UploadFile(HttpPostedFileBase file,String name)
        {
            if (!Program.IsImage(file)) return new HttpStatusCodeResult(HttpStatusCode.UnsupportedMediaType);
            String url = null;

            if (file != null)
            {
                //check if user has enough traffic to upload the file
                if (PictogramsDb.tryUpdateTraffic(name, file.ContentLength))
                {
                    //upload the file
                    url = Program.putObject(file, name);
                }
                else return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            //check if exists a differente file with the same name as the one you were trying to upload
            if (url == null) return new HttpStatusCodeResult(HttpStatusCode.Conflict);
            return new HttpStatusCodeResult(HttpStatusCode.OK,url);
        }

        public static HttpStatusCodeResult ReplaceFile(HttpPostedFileBase file, String name)
        {
      
            if (!Program.IsImage(file)) return new HttpStatusCodeResult(HttpStatusCode.UnsupportedMediaType);
            String url = null;


            if (file != null)
            {
                //check if user has enough traffic to upload the file
                if (PictogramsDb.tryUpdateTraffic(name, file.ContentLength))
                {
                    //replace existing image with our file
                    url = Program.ReplaceObject(file, name);
                }
                else return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK, url);
        }

    }
}