using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data;
using MvcApplication2.Models;
using System.Transactions;

namespace MvcApplication2.DataModel
{
    public static class PictogramsDb
    {
        static string connectionString = "Server=a439bc53-85c1-49f7-8c5a-a46b015ffb69.sqlserver.sequelizer.com;Database=dba439bc5385c149f78c5aa46b015ffb69;User ID=svjhkfovzvikurmt;Password=Vop7sKzFtMm2gRYNnxRRjtGpzF4BPM77mTbw52thxX7SbPRmbPnx8TKAP8EUP6YP;";
        static SqlConnection conn;
        static List<Reminder> reminders = new List<Reminder>();

        //Db for Codes

        //GET codes from an account
        public static List<TemporaryCode> getTemporaryCodes(String username)
        {
            List<TemporaryCode> list = new List<TemporaryCode>();

           
            using (conn = new SqlConnection())
            {
                conn.ConnectionString = connectionString;
                conn.Open();

                SqlCommand getCommand = new SqlCommand("Select * from Temporary_Codes where EmailId = @Email", conn);
                getCommand.Parameters.AddWithValue("@Email", username);

                using (SqlDataReader rdr = getCommand.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        TemporaryCode tc = new TemporaryCode();

                        tc.code = Convert.ToInt32(rdr["code"]);
                        tc.contact = rdr["contact"].ToString();
                        tc.status = rdr["status"].ToString();

                        list.Add(tc);
                    }
                }

            }

            return list;
        }

        //Validate a temporary code 
        // -> TESTAR
        public static String[] ValidateCode(int code)
        {
            SqlTransaction trans;
            String[] response=null;

            using (conn = new SqlConnection())
            {
                conn.ConnectionString = connectionString;
                conn.Open();
                trans = conn.BeginTransaction();
                try
                {
                    SqlCommand getCommand = new SqlCommand("Select * from Temporary_Codes where code = @code", conn);
                    getCommand.Parameters.AddWithValue("@code", code);
                    getCommand.Transaction = trans;

                    using (SqlDataReader rdr = getCommand.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            response = new String[] { rdr["contact"].ToString(), rdr["EmailId"].ToString() };
                        }
                        else
                        {
                            return null;
                        }
                    }


                    String status = "Code already inserted";

                    SqlCommand updateCommand = new SqlCommand("UPDATE Temporary_Codes SET status = @status, code = @code Where EmailId=@Email AND contact=@contact", conn);
                    updateCommand.Parameters.AddWithValue("@contact", response[0]);
                    updateCommand.Parameters.AddWithValue("@status", status);
                    updateCommand.Parameters.AddWithValue("@code", 0);
                    updateCommand.Parameters.AddWithValue("@Email", response[1]);
                    updateCommand.Transaction = trans;

                    updateCommand.ExecuteNonQuery();

                    trans.Commit();
                }
                catch (Exception e)
                {
                    trans.Rollback();
                    return null;
                }
            }
            
            return response; 
        }

       

        //Pensar sobre este codigo
        public static Boolean InsertTemporaryCode(String contact, int code, String status, String username)
        {
            SqlTransaction trans;
            
                using (SqlConnection conn = new SqlConnection())
                {
                    conn.ConnectionString = connectionString;
                    conn.Open();
                    trans = conn.BeginTransaction();
                    try
                    {
                        SqlCommand getCommand = new SqlCommand("Select * from Temporary_Codes where code = @code", conn);
                        getCommand.Parameters.AddWithValue("@code", code);
                        getCommand.Transaction = trans;

                        using (SqlDataReader rdr = getCommand.ExecuteReader())
                        {
                            if (rdr.Read()) return false; 
                        }

                        SqlCommand insertCommand = new SqlCommand("INSERT INTO Temporary_Codes (contact, code, status,EmailId,creationDate)  VALUES (@contact, @code, @status,@EmailId,@creationDate)", conn);
                        insertCommand.Parameters.AddWithValue("@contact", contact);
                        insertCommand.Parameters.AddWithValue("@code", code);
                        insertCommand.Parameters.AddWithValue("@status", status);
                        insertCommand.Parameters.AddWithValue("@EmailId", username);
                        insertCommand.Parameters.AddWithValue("@creationDate", DateTime.UtcNow);
                        insertCommand.Transaction = trans;

                        insertCommand.ExecuteScalar();
                        trans.Commit();
                    }
                    catch (Exception e)
                    {
                        trans.Rollback();
                        return false;
                    }
                    return true;
               
            }

        }

        //public static void 

        //Db for Contacts

        //See if token exists- identify one contact
        public static String[] ValidateToken(String token)
        {
            using (conn = new SqlConnection())
            {
                conn.ConnectionString = connectionString;
                conn.Open();

                SqlCommand getCommand = new SqlCommand("Select * from Receive_Users where token = @token ", conn);
                getCommand.Parameters.AddWithValue("@token", token);


                using (SqlDataReader rdr = getCommand.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        return new String[] { rdr["id"].ToString(), rdr["EmailId"].ToString() };
                    }
                }

            }
            return null;
        }

        internal static int getContactId(string username, string mvcuser)
        {
            int id = -1;


            using (conn = new SqlConnection())
            {
                conn.ConnectionString = connectionString;
                conn.Open();

                SqlCommand getCommand = new SqlCommand("Select * from Receive_Users where contact = @name AND EmailId = @Email ", conn);
                getCommand.Parameters.AddWithValue("@name", username);
                getCommand.Parameters.AddWithValue("@Email", mvcuser);
                using (SqlDataReader rdr = getCommand.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        id = (int)rdr["id"];
                    }

                }

            }
            
            return id;
        }

        //search for pictograms
        public static List<PictogramModel> getPictogram(String name)
        {
            List<PictogramModel> list = new List<PictogramModel>();

            using (conn = new SqlConnection())
            {
                conn.ConnectionString = connectionString;
                conn.Open();

                SqlCommand getCommand = new SqlCommand("Select * from Pictograms where name = @name", conn);
                getCommand.Parameters.Add("@name", SqlDbType.VarChar);

                string[] paramsString = new String[]{name}.Concat(name.Split(' ')).ToArray();

                for (int i = 0; i < paramsString.Length; i++)
                {
                    getCommand.Parameters["@name"].Value = paramsString[i];
                    using (SqlDataReader rdr = getCommand.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            PictogramModel pict = new PictogramModel();

                            pict.id = Convert.ToInt32(rdr["id"]);
                            pict.name = rdr["name"].ToString();
                            pict.url = rdr["url"].ToString();

                            if (!list.Contains(pict))
                                list.Add(pict);
                        }
                    }

                }
                }

            return list;
        }

        //get GCM id 
        internal static String getRegisteredId(string username, string mvcuser)
        {
            String id = String.Empty;

            using (conn = new SqlConnection())
            {
                conn.ConnectionString = connectionString;
                conn.Open();

                SqlCommand getCommand = new SqlCommand("Select * from Receive_Users where contact = @name AND EmailId = @Email ", conn);
                getCommand.Parameters.AddWithValue("@name", username);
                getCommand.Parameters.AddWithValue("@Email", mvcuser);
                using (SqlDataReader rdr = getCommand.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        id = rdr["registration_id"].ToString();
                    }

                }

            }
            return id;
        }

        // get contact list from a mvc user
        internal static List<String> getUsersFrom(String mvcuser)
        {
            List<String> usernames = new List<String>();

            using (conn = new SqlConnection())
            {
                conn.ConnectionString = connectionString;
                conn.Open();

                SqlCommand getCommand = new SqlCommand("Select * from Receive_Users where EmailId = @Email ", conn);
                getCommand.Parameters.AddWithValue("@Email", mvcuser);

                using (SqlDataReader rdr = getCommand.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        usernames.Add(rdr["contact"].ToString());
                    }

                }
            }

            return usernames;
        }

        //add Reminder to a contact
        //->Testar e ver teste.cshtml
        public static int addReminder(Reminder r, String username)
        {
             int id = -1;
             int idx = -1;
             String contact = r.contact;
             SqlTransaction trans;

             
                 using (conn = new SqlConnection())
                 {
                     conn.ConnectionString = connectionString;
                     conn.Open();
                     trans = conn.BeginTransaction();
                     try
                     {
                         SqlCommand getCommand = new SqlCommand("Select * from Receive_Users where contact = @name AND EmailId = @Email ", conn);
                         getCommand.Parameters.AddWithValue("@name", contact);
                         getCommand.Parameters.AddWithValue("@Email", username);
                         getCommand.Transaction = trans;
                         using(SqlDataReader rdr = getCommand.ExecuteReader())
                         {
                             if (rdr.Read())
                             {
                                 idx = (int)rdr["id"];
                             }

                         }

                         if (idx < 0) return idx;

                         SqlCommand insertCommand = new SqlCommand("INSERT INTO Temporary_Reminders (title,description,date, time, urls,EmailId,contact,daysofweek) OUTPUT inserted.ID VALUES (@title,@description,@date, @time, @urls,@Email,@contact,@daysofweek)", conn);
                         insertCommand.Transaction = trans;
                         insertCommand.Parameters.AddWithValue("@date", r.date);
                         insertCommand.Parameters.AddWithValue("@time", r.time);
                         insertCommand.Parameters.AddWithValue("@urls", r.urls);
                         insertCommand.Parameters.AddWithValue("@contact", idx);
                         insertCommand.Parameters.AddWithValue("@title", r.title);
                         insertCommand.Parameters.AddWithValue("@description", r.description);
                         insertCommand.Parameters.AddWithValue("@daysofweek", r.daysofweek);
                         insertCommand.Parameters.AddWithValue("@Email", username);

                         id = (int)insertCommand.ExecuteScalar();

                         trans.Commit();
                     }
                     catch (Exception e)
                     {
                         trans.Rollback();
                         throw e;
                     }

                 }
                
             return id;
        }
        
        //get reminder 
        //Testar
        public static Reminder getReminder(int idx, String mvcuser)
        {
            Boolean check = false;
            int count = 0;
            if (idx < 0) return null;
            Reminder r = new Reminder();
            SqlTransaction trans;


                using (conn = new SqlConnection())
                {

                    conn.ConnectionString = connectionString;
                    conn.Open();
                    trans = conn.BeginTransaction();

                    try
                    {

                        SqlCommand getCommand1 = new SqlCommand("Select * from Temporary_Reminders where EmailId = @Email AND id=@id", conn);
                        getCommand1.Parameters.AddWithValue("@Email", mvcuser);
                        getCommand1.Parameters.AddWithValue("@id", idx);
                        getCommand1.Transaction = trans;

                        using (SqlDataReader rdr = getCommand1.ExecuteReader())
                        {
                            if (rdr.Read())
                            {
                                check = true ;
                            }

                        }

                        if (!check) return null;

                        SqlCommand getCommand = new SqlCommand("Select * from Temporary_Reminders where ID = @ID", conn);
                        getCommand.Parameters.AddWithValue("@ID", idx);
                        getCommand.Transaction = trans;


                        using (SqlDataReader rdr = getCommand.ExecuteReader())
                        {

                            while (rdr.Read())
                            {
                                r.id = (int)rdr["id"];
                                r.date = rdr["date"].ToString();
                                r.time = rdr["time"].ToString();
                                r.urls = rdr["urls"].ToString();
                                r.title = rdr["title"].ToString();
                                r.description = rdr["description"].ToString();
                                r.contact = rdr["contact"].ToString();
                                r.daysofweek = (int)rdr["daysofweek"];
                                count++;
                            }

                        }
                        trans.Commit();
                    }
                    catch (Exception e)
                    {
                        trans.Rollback();
                        throw e;
                    }
                  
                }
            
            if (count == 0) return null;
            return r;
                


        }

        //get reminders from a user to a contac
        public static IEnumerable<Reminder> getAllReminders(int contact,String user)
        {
            List<Reminder> reminders = new List<Reminder>();
            using (conn = new SqlConnection())
            {
                conn.ConnectionString = connectionString;
                conn.Open();

                SqlCommand getCommand = new SqlCommand("Select * from Temporary_Reminders where EmailId = @Email and contact=@contact ", conn);
                getCommand.Parameters.AddWithValue("@Email", user);
                getCommand.Parameters.AddWithValue("@contact", contact);

                using (SqlDataReader rdr = getCommand.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        Reminder r = new Reminder();
                        r.contact = rdr["contact"].ToString();
                        r.id = (int)rdr["id"];
                        r.date = rdr["date"].ToString();
                        r.time = rdr["time"].ToString();
                        r.urls = rdr["urls"].ToString();
                        r.title = rdr["title"].ToString();
                        r.description = rdr["description"].ToString();
                        r.daysofweek = (int)rdr["daysofweek"];

                        reminders.Add(r);
                    }

                }

            }
            return reminders;
        }

        //Delete one reminder with id, check if reminder is from that user
        internal static void DeleteReminder(int id,String mvcuser)
        {
            SqlTransaction trans;
            Boolean check = false;
                using (conn = new SqlConnection())
                {
                    conn.ConnectionString = connectionString;
                    conn.Open();
                    trans = conn.BeginTransaction();

                    try
                    {

                        SqlCommand getCommand = new SqlCommand("Select * from Temporary_Reminders where EmailId = @Email AND id=@id", conn);
                        getCommand.Parameters.AddWithValue("@Email", mvcuser);
                        getCommand.Parameters.AddWithValue("@id", id);
                        getCommand.Transaction = trans;

                        using (SqlDataReader rdr = getCommand.ExecuteReader())
                        {
                            if (rdr.Read())
                            {
                                check = true ;
                            }

                        }

                        if (!check) return;

                        SqlCommand deleteCommand = new SqlCommand("Delete from Temporary_Reminders where ID = @ID", conn);
                        deleteCommand.Parameters.AddWithValue("@ID", id);
                        deleteCommand.Transaction = trans;

                        deleteCommand.ExecuteNonQuery();

                        trans.Commit();
                    }
                    catch (Exception e)
                    {
                        trans.Rollback();
                        throw e;
                    }

                }
              
        }

    

        //save credentials
        //-> Testar
        internal static void storeUserCredentials(string contact, string registration_id, string EncryptedToken,String username)
        {
            

                using (conn = new SqlConnection())
                {
                    conn.ConnectionString = connectionString;
                    conn.Open();

                    SqlCommand insertCommand = new SqlCommand("INSERT INTO Receive_Users (contact, registration_id, token,EmailId)  VALUES (@contact, @id, @token, @Email)", conn);
                    insertCommand.Parameters.AddWithValue("@contact", contact);
                    insertCommand.Parameters.AddWithValue("@id", registration_id);
                    insertCommand.Parameters.AddWithValue("@token", EncryptedToken);
                    insertCommand.Parameters.AddWithValue("@Email", username);

                    insertCommand.ExecuteNonQuery();

                }
            
           
        }

        internal static bool DeleteUser(String contact, String username)
        {
            SqlTransaction trans;
            int idx = -1;
           
                using (conn = new SqlConnection())
                {

                    conn.ConnectionString = connectionString;
                    conn.Open();
                    trans =conn.BeginTransaction();

                    try
                    {
                        SqlCommand getCommand = new SqlCommand("Select * from Receive_Users where contact = @name AND EmailId = @Email ", conn);
                        getCommand.Parameters.AddWithValue("@name", contact);
                        getCommand.Parameters.AddWithValue("@Email", username);
                        getCommand.Transaction = trans;

                        using (SqlDataReader rdr = getCommand.ExecuteReader())
                        {
                            if (rdr.Read())
                            {
                                idx = (int)rdr["id"];
                            }

                        }

                       

                        SqlCommand deleteCommand1 = new SqlCommand("Delete from Temporary_Codes where EmailId = @Email and contact=@contact", conn);
                        deleteCommand1.Parameters.AddWithValue("@Email", username);
                        deleteCommand1.Parameters.AddWithValue("@contact", contact);
                        deleteCommand1.Transaction = trans;

                        deleteCommand1.ExecuteNonQuery();

                        if (idx < 0)
                        {
                            trans.Commit();
                            return false;
                        }


                        SqlCommand deleteCommand2 = new SqlCommand("Delete from Temporary_Reminders where EmailId = @Email and contact=@contact", conn);
                        deleteCommand2.Parameters.AddWithValue("@Email", username);
                        deleteCommand2.Parameters.AddWithValue("@contact", idx);
                        deleteCommand2.Transaction = trans;

                        deleteCommand2.ExecuteNonQuery();

                        SqlCommand deleteCommand3 = new SqlCommand("Delete from Historical_Reminders where EmailId = @Email and contact=@contact", conn);
                        deleteCommand3.Parameters.AddWithValue("@Email", username);
                        deleteCommand3.Parameters.AddWithValue("@contact", idx);
                        deleteCommand3.Transaction = trans;

                        deleteCommand3.ExecuteNonQuery();

                        SqlCommand deleteCommand = new SqlCommand("Delete from Receive_Users where EmailId = @Email and contact=@contact", conn);
                        deleteCommand.Parameters.AddWithValue("@Email", username);
                        deleteCommand.Parameters.AddWithValue("@contact", contact);
                        deleteCommand.Transaction = trans;

                        deleteCommand.ExecuteNonQuery();
                        trans.Commit();
                        
                    }
                    catch (Exception e)
                    {
                        trans.Rollback();
                        throw e;
                    }
                }
                return true;
        }

        internal static bool checkIfUserExists(string mvcuser,string username)
        {
            using (conn = new SqlConnection())
            {
                conn.ConnectionString = connectionString;
                conn.Open();

                SqlCommand getCommand = new SqlCommand("Select * from Receive_Users where EmailId = @Email and contact = @contact ", conn);
                getCommand.Parameters.AddWithValue("@Email", mvcuser);
                getCommand.Parameters.AddWithValue("@contact", username);

                using (SqlDataReader rdr = getCommand.ExecuteReader())
                {
                    if(rdr.Read())
                    {
                        return true;
                    }

                }
            }
            return false;
        }

        public static bool IsUserExist(string emailid, string password)
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

        internal static Boolean checkIfReminderIsFromThisUser(int id, string mvcuser)
        {

            using (conn = new SqlConnection())
            {

                conn.ConnectionString = connectionString;
                conn.Open();

                SqlCommand getCommand = new SqlCommand("Select * from Temporary_Reminders where EmailId = @Email AND id=@id", conn);
                getCommand.Parameters.AddWithValue("@Email", mvcuser);
                getCommand.Parameters.AddWithValue("@id", id);

                using (SqlDataReader rdr = getCommand.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        return true;
                    }

                }
            }
            return false;
        }

        //Garantir que nenhuma property pode ser null
        internal static void EditReminder(Reminder r)
        {

            using (conn = new SqlConnection())
            {
                conn.ConnectionString = connectionString;
                conn.Open();
               
                SqlCommand updateCommand = new SqlCommand("UPDATE Temporary_Reminders SET title = @title, description = @description,date = @date, time = @time, urls=@urls Where id=@id", conn);
                updateCommand.Parameters.AddWithValue("@title", r.title);
                updateCommand.Parameters.AddWithValue("@description",r.description);
                updateCommand.Parameters.AddWithValue("@date", r.date);
                updateCommand.Parameters.AddWithValue("@time", r.time);
                updateCommand.Parameters.AddWithValue("@urls", r.urls);
                updateCommand.Parameters.AddWithValue("@id", r.id);

                updateCommand.ExecuteNonQuery();
            }
        }

        internal static void TransferReminderstoHistorical(IEnumerable<Reminder> list,int contact, string mvcuser)
        {
            SqlTransaction trans;

            using (conn = new SqlConnection())
            {
                conn.ConnectionString = connectionString;
                conn.Open();
                trans = conn.BeginTransaction();

                try
                {

                    SqlCommand deleteCommand = new SqlCommand("DELETE FROM Temporary_Reminders where EmailId = @Email and contact=@contact ", conn);
                    deleteCommand.Parameters.AddWithValue("@Email", mvcuser);
                    deleteCommand.Parameters.AddWithValue("@contact", contact);
                    deleteCommand.Transaction = trans;

                    deleteCommand.ExecuteNonQuery();

                    foreach (Reminder r in list)
                    {
                        SqlCommand insertCommand = new SqlCommand("INSERT INTO Historical_Reminders (title,description,date, time, urls,EmailId,contact,daysofweek) OUTPUT inserted.ID VALUES (@title,@description,@date, @time, @urls,@Email,@contact,@daysofweek)", conn);
                        insertCommand.Parameters.AddWithValue("@date", r.date);
                        insertCommand.Parameters.AddWithValue("@time", r.time);
                        insertCommand.Parameters.AddWithValue("@urls", r.urls);
                        insertCommand.Parameters.AddWithValue("@contact", contact);
                        insertCommand.Parameters.AddWithValue("@title", r.title);
                        insertCommand.Parameters.AddWithValue("@description", r.description);
                        insertCommand.Parameters.AddWithValue("@Email", mvcuser);
                        insertCommand.Parameters.AddWithValue("@daysofweek", r.daysofweek);
                        insertCommand.Transaction = trans;

                        insertCommand.ExecuteScalar();
                    }
                    trans.Commit();
                }
                catch (Exception e) //error occurred
                {
                    trans.Rollback();
                    throw e;
                }
            }
        }

        internal static string getUserName(int id)
        {
            using (conn = new SqlConnection())
            {
                conn.ConnectionString = connectionString;
                conn.Open();

                SqlCommand getCommand = new SqlCommand("Select * from Receive_Users where id = @id", conn);
                getCommand.Parameters.AddWithValue("@id",id );
                using (SqlDataReader rdr = getCommand.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        return rdr["contact"].ToString();
                    }

                }

            }
            return null;
        }

       

        internal static List<ContactModel> getUsersModelFrom(string mvcuser)
        {
            List<ContactModel> usernames = new List<ContactModel>();

            using (conn = new SqlConnection())
            {
                conn.ConnectionString = connectionString;
                conn.Open();

                SqlCommand getCommand = new SqlCommand("Select * from Receive_Users where EmailId = @Email ", conn);
                getCommand.Parameters.AddWithValue("@Email", mvcuser);

                using (SqlDataReader rdr = getCommand.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        ContactModel cm = new ContactModel();
                        cm.name = rdr["contact"].ToString();
                        cm.id = (int)rdr["id"];
                        usernames.Add(cm);
                    }

                }
            }

            return usernames;
        }

        internal static IEnumerable<Reminder> GetHistoricalReminders(string user, int contact)
        {
              List<Reminder> reminders = new List<Reminder>();
            using (conn = new SqlConnection())
            {
                conn.ConnectionString = connectionString;
                conn.Open();

                SqlCommand getCommand = new SqlCommand("Select * from Historical_Reminders where EmailId = @Email and contact=@contact ", conn);
                getCommand.Parameters.AddWithValue("@Email", user);
                getCommand.Parameters.AddWithValue("@contact", contact);

                using (SqlDataReader rdr = getCommand.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        Reminder r = new Reminder();
                        r.contact = rdr["contact"].ToString();
                        r.id = (int)rdr["id"];
                        r.date = rdr["date"].ToString();
                        r.time = rdr["time"].ToString();
                        r.urls = rdr["urls"].ToString();
                        r.title = rdr["title"].ToString();
                        r.description = rdr["description"].ToString();

                        reminders.Add(r);
                    }

                }

            }
            return reminders;
        }


        internal static Reminder getHistoricalReminder(int idx, string name)
        {
            Reminder r = null;
            using (conn = new SqlConnection())
            {
                conn.ConnectionString = connectionString;
                conn.Open();

                SqlCommand getCommand = new SqlCommand("Select * from Historical_Reminders where id=@id AND EmailId=@Email ", conn);
                getCommand.Parameters.AddWithValue("@id", idx);
                getCommand.Parameters.AddWithValue("@Email", name);
               

                using (SqlDataReader rdr = getCommand.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        r = new Reminder();
                        r.contact = rdr["contact"].ToString();
                        r.id = (int)rdr["id"];
                        r.date = rdr["date"].ToString();
                        r.time = rdr["time"].ToString();
                        r.urls = rdr["urls"].ToString();
                        r.title = rdr["title"].ToString();
                        r.description = rdr["description"].ToString();

                    }

                }

            }
            return r;
        }

        internal static void ScheduleValidationForCodes()
        {

            using (SqlConnection con = new SqlConnection())
            {
                con.ConnectionString = connectionString;
                con.Open();

                SqlCommand deleteCommand = new SqlCommand("Delete from Temporary_Codes where code>@code AND DATEADD(MINUTE, 5, creationDate) <  @mydate",con);
                deleteCommand.Parameters.AddWithValue("@mydate", DateTime.UtcNow);
                deleteCommand.Parameters.AddWithValue("@code", 0);

                deleteCommand.ExecuteNonQuery();
            }

        }

        internal static bool tryUpdateTraffic(string name, int size)
        {
            int traffic = -1;
            using (SqlConnection con = new SqlConnection())
            {
                con.ConnectionString = connectionString;
                con.Open();

                SqlCommand selectCommand = new SqlCommand("Select * from mvcUser where EmailId = @EmailId", con);
                selectCommand.Parameters.AddWithValue("@EmailId", name);

                using (SqlDataReader rdr = selectCommand.ExecuteReader()){
                    if (rdr.Read()){
                        traffic = (int)rdr["traffic_counter"];
                    }

                }
            }
            if (traffic + size > 10000000) return false;
            return true;
        }

        internal static bool UpdateTraffic(string name, int size)
        {
            int traffic = -1;
            SqlTransaction trans;
            using (SqlConnection con = new SqlConnection())
            {
                con.ConnectionString = connectionString;
                con.Open();
                trans = con.BeginTransaction();

                try
                {
                    SqlCommand selectCommand = new SqlCommand("Select * from mvcUser where EmailId = @EmailId", con);
                    selectCommand.Parameters.AddWithValue("@EmailId", name);
                    selectCommand.Transaction = trans;

                    using (SqlDataReader rdr = selectCommand.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            traffic = (int)rdr["traffic_counter"];
                        }

                    }

                    if ((traffic = traffic + size) > 10000000) { return false; }

                    SqlCommand updateCommand = new SqlCommand("UPDATE mvcUser SET traffic_counter = @traffic where EmailId = @EmailId", con);
                    updateCommand.Parameters.AddWithValue("@traffic", traffic);
                    updateCommand.Parameters.AddWithValue("@EmailId", name);
                    updateCommand.Transaction = trans;

                    updateCommand.ExecuteNonQuery();
                    trans.Commit();
                    return true;
                }
                catch (Exception e) //error occurred
                {
                    trans.Rollback();
                    throw e;
                }
            }
            
            

        }

        internal static void SetTraffic()
        {
            using (SqlConnection con = new SqlConnection())
            {
                con.ConnectionString = connectionString;
                con.Open();

                SqlCommand deleteCommand = new SqlCommand("Update mvcUser SET traffic_counter = @counter,creationDate = @date WHERE DATEADD(DAY, 30, creationDate) <=  @date", con);
                deleteCommand.Parameters.AddWithValue("@date", DateTime.Now);
                deleteCommand.Parameters.AddWithValue("@counter", 0);

                deleteCommand.ExecuteNonQuery();
            }
        }
    }
}