using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Http.Controllers;
using System.Text;
using System.Security.Principal;
using MvcApplication2.Models;
using MvcApplication2.DataModel;
using System.Web.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace MvcApplication2.Controllers
{
    public class PictogramsController : ApiController
    {
        public IEnumerable<PictogramModel> GetAllPictogramas(String pictogram)
        {
            List<PictogramModel> list = PictogramsDb.getPictogram(pictogram);
            return list;
        }


    }
}
