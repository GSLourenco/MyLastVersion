using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Data.SqlClient;
using MvcApplication2.DataModel;

namespace MvcApplication2.Models
{
    public class Login
    {
        [Required(ErrorMessage = "Email Id Required")]
        [DisplayName("Email ID")]
        [RegularExpression(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$",
                                    ErrorMessage = "Email Format is wrong")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Less than 15 characters")]
        public string EmailId
        {
            get;
            set;
        }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password Required")]
        [DisplayName("Password")]
        [StringLength(50 , MinimumLength=6, ErrorMessage = ":Less than 6 characters")]
        public string Password
        {
            get;
            set;
        }

        public bool IsUserExist(string emailid, string password)
        {
            bool flag = false;
            SqlConnection conn;

            using (conn = new SqlConnection())
            {
                conn.ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["cs"].ConnectionString;
                conn.Open();

                password = Utils.GenerateSaltedSHA1(password);

                SqlCommand getCommand = new SqlCommand("select count(*) from mvcUser where EmailId=@mail and Password=@password", conn);
                getCommand.Parameters.AddWithValue("@mail", emailid);
                getCommand.Parameters.AddWithValue("@password", password);
                flag = Convert.ToBoolean(getCommand.ExecuteScalar());
            }
            return flag;
        }


    }
}