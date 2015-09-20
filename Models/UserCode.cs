using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace MvcApplication2.Models
{
    public class UserCode
    {
        [StringLength(5,MinimumLength = 5)]
        public string code { get; set; }
        [StringLength(152, MinimumLength = 152)]
        public string registration_id { get; set; }
    }
}