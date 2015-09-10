using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using MvcApplication2.DataModel;
using System.Data.SqlClient;

namespace MvcApplication2.Models
{
     public class Register
    {
        [Required(ErrorMessage="FirstName Required:")]
        [DisplayName("First Name:")]
        [RegularExpression(@"^[a-zA-Z'.\s]{1,40}$",ErrorMessage="Special Characters not allowed")]
        [StringLength(15 , MinimumLength=4, ErrorMessage = "Less than 4 characters")]
        public string FirstName { get; set; }
 
        [Required(ErrorMessage="LastName Required:")]
        [RegularExpression(@"^[a-zA-Z'.\s]{1,40}$", ErrorMessage = "Special Characters  not allowed")]
        [DisplayName("Last Name:")]
        [StringLength(15 , MinimumLength=4, ErrorMessage = "Less than 4 characters")]
        public string LastName { get; set; }
 
        [Required(ErrorMessage="EmailId Required:")]
        [DisplayName("Email Id:")]
        [RegularExpression(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", ErrorMessage = "Email Format is wrong")]
        [StringLength(30, ErrorMessage = "Less than 15 characters")]
        public string EmailId { get; set; }
 
        [Required(ErrorMessage="Password Required:")]
        [DataType(DataType.Password)]
        [DisplayName("Password:")]
        public string Password { get; set; }
 
        [Required(ErrorMessage="Confirm Password Required:")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Confirm not matched.")]
        [Display(Name = "Confirm password:")]
        public string ConfirmPassword { get; set; }
 
      
 
        
 
        public bool IsUserExist(string emailid)
        {
           bool flag = false;
            SqlConnection conn;

            using (conn = new SqlConnection())
            {
                conn.ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["cs"].ConnectionString;
                conn.Open();

                SqlCommand getCommand = new SqlCommand("select count(*) from mvcUser where EmailId=@mail", conn);
                getCommand.Parameters.AddWithValue("@mail", emailid);
                flag = Convert.ToBoolean(getCommand.ExecuteScalar());
            }
            return flag;
        }
 
        public bool Insert()
        {
            bool flag = false;
            if (!IsUserExist(EmailId))
            {
                SqlConnection connection;

                using (connection = new SqlConnection())
                {
                    connection.ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["cs"].ConnectionString;
                    connection.Open();

                    Password = Utils.GenerateSaltedSHA1(Password);

                    SqlCommand command = new SqlCommand("insert into mvcUser (FirstName,LastName,EmailId,Password,traffic_counter,creationDate) values(@FirstName,@LastName,@EmailId,@Password,@traffic,@creation)", connection);
                    command.Parameters.AddWithValue("@FirstName", FirstName);
                    command.Parameters.AddWithValue("@LastName", LastName);
                    command.Parameters.AddWithValue("@EmailId", EmailId);
                    command.Parameters.AddWithValue("@Password", Password);
                    command.Parameters.AddWithValue("@traffic", 0);
                    command.Parameters.AddWithValue("@creation", DateTime.Now);
                    flag = Convert.ToBoolean(command.ExecuteNonQuery());
                }
            }
            return flag;
        }


    }
}