//-----------------------------------------------------------------------
// <copyright file="ArcGisServerMembershipProvider.cs" company="Studio A&T s.r.l.">
//     Copyright (c) Studio A&T s.r.l. All rights reserved.
// </copyright>
// <author>Nicogis</author>
//-----------------------------------------------------------------------
namespace ArcGisServerCustomProvider
{
    using System;
    using System.Configuration.Provider;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Reflection;
    using System.Web.Security;

    /// <summary>
    /// class AGSMembershipProvider
    /// </summary>
    public sealed class ArcGisServerMembershipProvider : MembershipProvider
    {
        /// <summary>
        /// name provider
        /// </summary>
        private string providerName;

        /// <summary>
        /// connection string DBMS
        /// </summary>
        private string connectionString;

        /// <summary>
        /// Gets Enable Password Reset
        /// </summary>
        public override bool EnablePasswordReset
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets Enable Password Retrieval
        /// </summary>
        public override bool EnablePasswordRetrieval
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets Max Invalid Password Attempts
        /// </summary>
        public override int MaxInvalidPasswordAttempts
        {
            get { throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "Method not implemented: {0}", MethodBase.GetCurrentMethod().Name)); }
        }

        /// <summary>
        /// Gets Min Required Non Alphanumeric Characters
        /// </summary>
        public override int MinRequiredNonAlphanumericCharacters
        {
            get { throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "Method not implemented: {0}", MethodBase.GetCurrentMethod().Name)); }
        }

        /// <summary>
        /// Gets Min Required Password Length
        /// </summary>
        public override int MinRequiredPasswordLength
        {
            get { throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "Method not implemented: {0}", MethodBase.GetCurrentMethod().Name)); }
        }

        /// <summary>
        /// Gets Password Attempt Window
        /// </summary>
        public override int PasswordAttemptWindow
        {
            get { throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "Method not implemented: {0}", MethodBase.GetCurrentMethod().Name)); }
        }

        /// <summary>
        /// Gets Password Format
        /// </summary>
        public override MembershipPasswordFormat PasswordFormat
        {
            get { throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "Method not implemented: {0}", MethodBase.GetCurrentMethod().Name)); }
        }

        /// <summary>
        /// Gets Password Strength Regular Expression
        /// </summary>
        public override string PasswordStrengthRegularExpression
        {
            get { throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "Method not implemented: {0}", MethodBase.GetCurrentMethod().Name)); }
        }

        /// <summary>
        /// Gets Requires Question And Answer
        /// </summary>
        public override bool RequiresQuestionAndAnswer
        {
            get { throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "Method not implemented: {0}", MethodBase.GetCurrentMethod().Name)); }
        }

        /// <summary>
        /// Gets Requires Unique Email
        /// </summary>
        public override bool RequiresUniqueEmail
        {
            get { throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "Method not implemented: {0}", MethodBase.GetCurrentMethod().Name)); }
        }

        /// <summary>
        /// Gets or sets of application name
        /// </summary>
        public override string ApplicationName
        {
            get;
            set;
        }

        /// <summary>
        /// initialize provider
        /// </summary>
        /// <param name="name">name provider</param>
        /// <param name="config">properties in config</param>
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (string.IsNullOrEmpty(name))
            {
                name = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", this.GetType().Namespace, this.GetType().Name);
            }

            base.Initialize(name, config);
            this.providerName = name;

            string connectionStringName = config["connectionStringName"];
            if (string.IsNullOrEmpty(connectionStringName))
            {
                throw new ProviderException("Missing required property connectionStringName");
            }
            else
            {
                this.connectionString = connectionStringName;
            }

            // test for connection
            try
            {
                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();
                }
            }
            catch
            {
                throw new ProviderException("Check your DB connection!");
            }  
        }

        /// <summary>
        /// change password
        /// </summary>
        /// <param name="username">user name</param>
        /// <param name="oldPassword">old password</param>
        /// <param name="newPassword">new password</param>
        /// <returns>true is ok</returns>
        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "Method not implemented: {0}", MethodBase.GetCurrentMethod().Name));
        }

        /// <summary>
        /// Change Password Question And Answer
        /// </summary>
        /// <param name="username">value of username</param>
        /// <param name="password">value of password</param>
        /// <param name="newPasswordQuestion">new Password Question</param>
        /// <param name="newPasswordAnswer">new Password Answer</param>
        /// <returns>true is ok</returns>
        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "Method not implemented: {0}", MethodBase.GetCurrentMethod().Name));
        }

        /// <summary>
        /// create a user
        /// </summary>
        /// <param name="username">value of username</param>
        /// <param name="password">value of password</param>
        /// <param name="email">value email</param>
        /// <param name="passwordQuestion">password Question</param>
        /// <param name="passwordAnswer">password Answer</param>
        /// <param name="isApproved">is Approved</param>
        /// <param name="providerUserKey">provider User Key</param>
        /// <param name="status">value of status</param>
        /// <returns>object MembershipUser</returns>
        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            MembershipUser newUser = null;
            try
            {
                newUser = this.GetUser(username, false);
                if (newUser == null)
                {
                    using (SqlConnection connection = new SqlConnection(this.connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("INSERT INTO Users (Username, Password) VALUES (@Username, @Password)", connection))
                        {
                            cmd.Parameters.Add("@Username", SqlDbType.NVarChar).Value = username;
                            cmd.Parameters.Add("@Password", SqlDbType.NVarChar).Value = password;
                            connection.Open();

                            int recordAdded = cmd.ExecuteNonQuery();

                            if (recordAdded > 0)
                            {
                                status = MembershipCreateStatus.Success;
                                newUser = this.GetUser(username);
                            }
                            else
                            {
                                status = MembershipCreateStatus.UserRejected;
                            }
                        }
                    }
                }
                else
                {
                    status = MembershipCreateStatus.DuplicateUserName;
                }
            }
            catch (Exception e)
            {
                throw new ProviderException(e.Message);
            }

            if (status != MembershipCreateStatus.Success)
            {
                throw new ProviderException(ArcGisServerMembershipProvider.GetErrorMessage(status));
            }

            return newUser;
        }

        /// <summary>
        /// delete user
        /// </summary>
        /// <param name="username">value of username</param>
        /// <param name="deleteAllRelatedData">delete All Related Data</param>
        /// <returns>true is ok</returns>
        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            int rowsAffected = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("DELETE FROM Users WHERE Username = @Username", connection))
                    {
                        cmd.Parameters.Add("@Username", SqlDbType.NVarChar).Value = username;
                        connection.Open();

                        rowsAffected = cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
               throw new ProviderException(e.Message);  
            }

            return rowsAffected > 0;
        }

        /// <summary>
        /// Find Users By Email
        /// </summary>
        /// <param name="emailToMatch">email To Match</param>
        /// <param name="pageIndex">page Index</param>
        /// <param name="pageSize">page Size</param>
        /// <param name="totalRecords">total Records</param>
        /// <returns>object MembershipUserCollection</returns>
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "Method not implemented: {0}", MethodBase.GetCurrentMethod().Name));
        }

        /// <summary>
        /// Find Users By Name
        /// </summary>
        /// <param name="usernameToMatch">username To Match</param>
        /// <param name="pageIndex">page Index</param>
        /// <param name="pageSize">page Size</param>
        /// <param name="totalRecords">total Records</param>
        /// <returns>object MembershipUserCollection</returns>
        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            MembershipUserCollection users = new MembershipUserCollection();
            try
            {
                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT Count(*) FROM Users WHERE Username LIKE @Username", connection))
                    {
                        cmd.Parameters.Add("@Username", SqlDbType.NVarChar).Value = string.Format(CultureInfo.InvariantCulture, "%{0}%", usernameToMatch);
                        connection.Open();
                        totalRecords = (int)cmd.ExecuteScalar();

                        if (totalRecords <= 0)
                        {
                            return users;
                        }
                    }

                    using (SqlCommand cmd = new SqlCommand("SELECT Id, Username FROM Users WHERE Username LIKE @Username ORDER BY Username ASC", connection))
                    {
                        cmd.Parameters.Add("@Username", SqlDbType.NVarChar).Value = string.Format(CultureInfo.InvariantCulture, "%{0}%", usernameToMatch);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                int counter = 0;
                                int startIndex = pageSize * pageIndex;
                                int endIndex = startIndex + pageSize - 1;

                                while (reader.Read())
                                {
                                    if (counter >= startIndex)
                                    {
                                        MembershipUser user = this.GetUserByReader(reader);

                                        users.Add(user);
                                    }

                                    if (counter >= endIndex) 
                                    { 
                                        cmd.Cancel(); 
                                    }

                                    counter++;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new ProviderException(e.Message);   
            }
            
            return users;
        }

        /// <summary>
        /// get all users
        /// </summary>
        /// <param name="pageIndex">page Index</param>
        /// <param name="pageSize">page Size</param>
        /// <param name="totalRecords">total Records</param>
        /// <returns>objects MembershipUserCollection</returns>
        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            MembershipUserCollection users = new MembershipUserCollection();
            
            try
            {
                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT Count(*) FROM Users", connection))
                    {
                        connection.Open();
                        totalRecords = (int)cmd.ExecuteScalar();

                        if (totalRecords <= 0)
                        {
                            return users;
                        }
                    }

                    using (SqlCommand cmd = new SqlCommand("SELECT Id, Username FROM Users ORDER BY Username", connection))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                int counter = 0;
                                int startIndex = pageSize * pageIndex;
                                int endIndex = startIndex + pageSize - 1;

                                while (reader.Read())
                                {
                                    if (counter >= startIndex)
                                    {
                                        MembershipUser user = this.GetUserByReader(reader);
                                        users.Add(user);
                                    }

                                    if (counter >= endIndex)
                                    { 
                                        cmd.Cancel(); 
                                    }

                                    counter++;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new ProviderException(e.Message);   
            }

            totalRecords = users.Count;

            return users;
        }

        /// <summary>
        /// Get Number Of Users Online
        /// </summary>
        /// <returns>number of users online</returns>
        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "Method not implemented: {0}", MethodBase.GetCurrentMethod().Name));
        }

        /// <summary>
        /// Get password
        /// </summary>
        /// <param name="username">value of username</param>
        /// <param name="answer">value of answer</param>
        /// <returns>value of password</returns>
        public override string GetPassword(string username, string answer)
        {
            throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "Method not implemented: {0}", MethodBase.GetCurrentMethod().Name));
        }

        /// <summary>
        /// Get user
        /// </summary>
        /// <param name="username">value of username</param>
        /// <param name="userIsOnline">user Is Online</param>
        /// <returns>object MembershipUser</returns>
        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            MembershipUser user = null;

            try
            {
                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT Id, Username FROM Users WHERE Username = @Username", connection))
                    {
                        cmd.Parameters.Add("@Username", SqlDbType.NVarChar).Value = username;
                        connection.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                reader.Read();
                                user = this.GetUserByReader(reader);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
               throw new ProviderException(e.Message);  
            }

            return user;
        }

        /// <summary>
        /// Get user
        /// </summary>
        /// <param name="providerUserKey">provider User Key</param>
        /// <param name="userIsOnline">user Is Online</param>
        /// <returns>object MembershipUser</returns>
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "Method not implemented: {0}", MethodBase.GetCurrentMethod().Name));
        }

        /// <summary>
        /// Get User Name By Email
        /// </summary>
        /// <param name="email">value email</param>
        /// <returns>return user name</returns>
        public override string GetUserNameByEmail(string email)
        {
            throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "Method not implemented: {0}", MethodBase.GetCurrentMethod().Name));
        }

        /// <summary>
        /// Reset Password
        /// </summary>
        /// <param name="username">value of username</param>
        /// <param name="answer">value of answer</param>
        /// <returns>value of password</returns>
        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "Method not implemented: {0}", MethodBase.GetCurrentMethod().Name));
        }

        /// <summary>
        /// Un lock User
        /// </summary>
        /// <param name="userName">value of username</param>
        /// <returns>true is ok</returns>
        public override bool UnlockUser(string userName)
        {
            throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "Method not implemented: {0}", MethodBase.GetCurrentMethod().Name));
        }

        /// <summary>
        /// update user
        /// </summary>
        /// <param name="user">object MembershipUser</param>
        public override void UpdateUser(MembershipUser user)
        {
            //// called but not used
            //// throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "Method not implemented: {0}", MethodBase.GetCurrentMethod().Name));
        }

        /// <summary>
        /// Validate User
        /// </summary>
        /// <param name="username">value of username</param>
        /// <param name="password">value of password</param>
        /// <returns>true is ok</returns>
        public override bool ValidateUser(string username, string password)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT Count(*) FROM Users WHERE Username = @Username AND Password = @Password", connection))
                    {
                        cmd.Parameters.Add("@Username", SqlDbType.NVarChar).Value = username;
                        cmd.Parameters.Add("@Password", SqlDbType.NVarChar).Value = password;
                        connection.Open();

                        return ((int)cmd.ExecuteScalar()) > 0;
                        
                    }
                }
            }
            catch (Exception e)
            {
               throw new ProviderException(e.Message);
            }
        }

        /// <summary>
        /// error from status
        /// </summary>
        /// <param name="status">object MembershipCreateStatus</param>
        /// <returns>error from MembershipCreateStatus</returns>
        private static string GetErrorMessage(MembershipCreateStatus status)
        {
            switch (status)
            {
                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.DuplicateUserName:
                    return "Username already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A username for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }

            return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
        }

        /// <summary>
        /// MembershipUser from data reader
        /// </summary>
        /// <param name="reader">object SQL data reader</param>
        /// <returns>object MembershipUser</returns>
        private MembershipUser GetUserByReader(SqlDataReader reader)
        {
            string userName = reader.GetString(1);
            return this.GetUser(userName);
        }

        /// <summary>
        /// MembershipUser from username
        /// </summary>
        /// <param name="userName">value of username</param>
        /// <returns>object MembershipUser</returns>
        private MembershipUser GetUser(string userName)
        {
            return new MembershipUser(
                this.providerName,
                userName,
                null,
                null,
                "Secret Question",
                userName,
                true,
                false,
                DateTime.Now,               // creationDate
                DateTime.Now,               // lastLoginDate
                DateTime.Now,               // lastActivityDate
                DateTime.Now,               // lastPasswordChangedDate
                new DateTime(2000, 1, 1));    // lastLockoutDate
        }
    } 
}
