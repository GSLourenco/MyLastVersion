using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication2.Models
{
    public class PictogramModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string url { get; set; }

        public override bool Equals(System.Object obj)
        {
            if (obj == null) return false;

            PictogramModel p = obj as PictogramModel;
            if (p == null) return false;

            return this.id == p.id && this.name == p.name && this.url == p.url;
                
        }

    }
}