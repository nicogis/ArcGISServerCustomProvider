//-----------------------------------------------------------------------
// <copyright file="ArcGisServerRoleProvider.cs" company="Studio A&T s.r.l.">
//     Copyright (c) Studio A&T s.r.l. All rights reserved.
// </copyright>
// <author>Nicogis</author>
//-----------------------------------------------------------------------
namespace ArcGisServerCustomProvider
{
    using System;
    using System.Collections.Generic;
    using System.Configuration.Provider;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Web.Security;
    /// <summary>
    /// class AGS Role Provider
    /// </summary>
    public class ArcGisServerRoleProvider : RoleProvider
    {

        /// <summary>
        /// connectionString DBMS
        /// </summary>
        private string connectionString;

        private string applicationName;

        public override string ApplicationName
        {
            get
            {
                return this.applicationName;
            }
            set
            {
                this.applicationName = value;
            }
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
        /// Add Users To Roles
        /// </summary>
        /// <param name="usernames">list of users</param>
        /// <param name="roleNames">list of roles</param>
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            try
            {
                int[] idUsers = this.Users(usernames);
                int[] idRoles = this.Roles(roleNames);

                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();

                    SqlCommand cmd = connection.CreateCommand();
                    SqlTransaction transaction = connection.BeginTransaction(MethodBase.GetCurrentMethod().Name);


                    cmd.Connection = connection;
                    cmd.Transaction = transaction;
                    try
                    {
                        foreach (int u in idUsers)
                        {
                            foreach (int r in idRoles)
                            {

                                cmd.CommandText = "SELECT Count(*) FROM UsersRoles WHERE IdRolename=@idRole AND IdUsername=@idUser";
                                cmd.Parameters.Clear();
                                cmd.Parameters.Add("@idRole", SqlDbType.Int).Value = r;
                                cmd.Parameters.Add("@idUser", SqlDbType.Int).Value = u;

                                int count = (int)cmd.ExecuteScalar();
                                if (count > 0)
                                {
                                    continue;
                                }


                                cmd.CommandText = "INSERT INTO UsersRoles" +
                                        " (IdRolename,IdUsername)" +
                                        " VALUES (@IdRole, @IdUser)";

                                cmd.Parameters.Clear();
                                cmd.Parameters.Add("@IdRole", SqlDbType.Int).Value = r;
                                cmd.Parameters.Add("@IdUser", SqlDbType.Int).Value = u;
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        if (transaction != null)
                        {
                            try
                            {
                                transaction.Rollback();
                            }
                            catch (Exception e2)
                            {
                                throw new ProviderException(e2.Message);
                            }
                        }

                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                throw new ProviderException(e.Message);
            }
        }

        /// <summary>
        /// create a role
        /// </summary>
        /// <param name="roleName">role name</param>
        public override void CreateRole(string roleName)
        {
            try
            {
                if (this.RoleExists(roleName))
                {
                    throw new ProviderException("Role exists!");
                }

                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO Roles (Rolename) VALUES (@Role)", connection))
                    {
                        cmd.Parameters.Add("@Role", SqlDbType.NVarChar).Value = roleName;
                        
                        connection.Open();

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                throw new ProviderException(e.Message);
            }
        }

        /// <summary>
        /// delete a role
        /// </summary>
        /// <param name="roleName">role name</param>
        /// <param name="throwOnPopulatedRole">throw On Populated Role</param>
        /// <returns>true is ok</returns>
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            int rowsAffected = 0;

            try
            {
                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT Users.Id FROM Roles INNER JOIN UsersRoles ON Roles.Id = UsersRoles.IdRolename INNER JOIN Users ON UsersRoles.IdUsername = Users.Id WHERE Roles.Rolename = @Role", connection))
                    {
                        cmd.Parameters.Add("@Role", SqlDbType.NVarChar).Value = roleName;
                        object id = cmd.ExecuteScalar();

                        if (id != null)
                        {
                            throw new ProviderException("Cannot delete a populated role.");
                        }
                    }

                    using (SqlCommand cmd = new SqlCommand("DELETE FROM Roles WHERE Rolename= @Role", connection))
                    {
                        cmd.Parameters.Add("@Role", SqlDbType.NVarChar).Value = roleName;

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
        /// Find Users In Role
        /// </summary>
        /// <param name="roleName">role name</param>
        /// <param name="usernameToMatch">username To Match</param>
        /// <returns>return users in role</returns>
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "Method not implemented: {0}", MethodBase.GetCurrentMethod().Name));
        }

        /// <summary>
        /// get all roles
        /// </summary>
        /// <returns>list of roles</returns>
        public override string[] GetAllRoles()
        {
            List<string> roles = new List<string>();
            try
            {
                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT Rolename FROM Roles ORDER BY Rolename", connection))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    roles.Add(reader.GetString(0));
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

            return roles.ToArray();
        }

        /// <summary>
        /// Get of roles for user
        /// </summary>
        /// <param name="username">name of user</param>
        /// <returns>list of roles</returns>
        public override string[] GetRolesForUser(string username)
        {
            List<string> roles = new List<string>();
            try
            {
                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT Roles.Rolename FROM Roles INNER JOIN UsersRoles ON Roles.Id = UsersRoles.IdRolename INNER JOIN Users ON UsersRoles.IdUsername = Users.Id WHERE Users.Username = @UserName ORDER BY Roles.Rolename", connection))
                    {
                        cmd.Parameters.Add("@UserName", SqlDbType.NVarChar).Value = username;
                        connection.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    roles.Add(reader.GetString(0));
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

            return roles.ToArray();
        }

        /// <summary>
        /// Get Users In Role
        /// </summary>
        /// <param name="roleName">name of role</param>
        /// <returns>list of users</returns>
        public override string[] GetUsersInRole(string roleName)
        {
            List<string> users = new List<string>();
            try
            {
                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT Users.Username FROM Roles INNER JOIN UsersRoles ON Roles.Id = UsersRoles.IdRolename INNER JOIN Users ON UsersRoles.IdUsername = Users.Id WHERE (Roles.Rolename = @RoleName) ORDER BY Users.Username", connection))
                    {
                        cmd.Parameters.Add("@RoleName", SqlDbType.NVarChar).Value = roleName;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    users.Add(reader.GetString(0));
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

            return users.ToArray();
        }

        /// <summary>
        /// user in role
        /// </summary>
        /// <param name="username">name of user</param>
        /// <param name="roleName">name of role</param>
        /// <returns>true if user in in role</returns>
        public override bool IsUserInRole(string username, string roleName)
        {
            bool exists = false;
            try
            {
                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT Users.Id FROM UsersRoles INNER JOIN Users ON UsersRoles.IdUsername = Users.Id INNER JOIN Roles ON UsersRoles.IdRolename = Roles.Id WHERE (Users.Username = @Username) AND (Roles.Rolename = @RoleName)", connection))
                    {
                        cmd.Parameters.Add("@RoleName", SqlDbType.NVarChar).Value = roleName;
                        cmd.Parameters.Add("@Username", SqlDbType.NVarChar).Value = username;

                        int count = (int)cmd.ExecuteScalar();
                        exists = count > 0;
                    }
                }
            }
            catch (Exception e)
            {
                throw new ProviderException(e.Message);
            }

            return exists;
        }

        /// <summary>
        /// Remove Users From Roles
        /// </summary>
        /// <param name="usernames">list of users</param>
        /// <param name="roleNames">list of roles</param>
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            try
            {
                int[] idUsers = this.Users(usernames);
                int[] idRoles = this.Roles(roleNames);

                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();

                    SqlCommand cmd = connection.CreateCommand();
                    SqlTransaction transaction = connection.BeginTransaction(MethodBase.GetCurrentMethod().Name);


                    cmd.Connection = connection;
                    cmd.Transaction = transaction;
                    try
                    {
                        foreach (int u in idUsers)
                        {
                            foreach (int r in idRoles)
                            {
                                cmd.CommandText = "DELETE FROM UsersRoles" +
                                        " WHERE IdRolename = @IdRole AND IdUsername = @IdUser";

                                cmd.Parameters.Clear();
                                cmd.Parameters.Add("@IdRole", System.Data.SqlDbType.Int).Value = r;
                                cmd.Parameters.Add("@IdUser", System.Data.SqlDbType.Int).Value = u;
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        if (transaction != null)
                        {
                            try
                            {
                                transaction.Rollback();
                            }
                            catch (Exception e2)
                            {
                                throw new ProviderException(e2.Message);
                            }
                        }

                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                throw new ProviderException(e.Message);
            }
        }

        /// <summary>
        /// Role exists
        /// </summary>
        /// <param name="roleName">name of role</param>
        /// <returns>true if role exists</returns>
        public override bool RoleExists(string roleName)
        {
            bool exists = false;
            try
            {
                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT Count(*) FROM Roles WHERE Rolename = @RoleName", connection))
                    {
                        cmd.Parameters.Add("@RoleName", System.Data.SqlDbType.NVarChar).Value = roleName;

                        int count = (int)cmd.ExecuteScalar();
                        exists = count > 0;
                    }
                }
            }
            catch (Exception e)
            {
                throw new ProviderException(e.Message);
            }

            return exists;
        }

        /// <summary>
        /// return list id users from list of users
        /// </summary>
        /// <param name="userNames">list of users</param>
        /// <returns>list of id users</returns>
        private int[] Users(string[] userNames)
        {
            List<int> idUsername = new List<int>();
            try
            {
                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();
                    
                    foreach (string u in userNames.Distinct())
                    {
                        using (SqlCommand cmd = new SqlCommand("SELECT Id FROM Users WHERE Username = @Username", connection))
                        {
                            cmd.Parameters.Add("@Username", SqlDbType.NVarChar).Value = u;

                            idUsername.Add((int)cmd.ExecuteScalar());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new ProviderException(e.Message);
            }

            return idUsername.ToArray();
        }

        /// <summary>
        /// return list id roles from list of roles
        /// </summary>
        /// <param name="roleNames">list of roles</param>
        /// <returns>list of id roles</returns>
        private int[] Roles(string[] roleNames)
        {
            List<int> idRoles = new List<int>();
            try
            {
                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();
                    foreach (string r in roleNames.Distinct())
                    {
                        using (SqlCommand cmd = new SqlCommand("SELECT Id FROM Roles WHERE Rolename = @RoleName", connection))
                        {
                            cmd.Parameters.Add("@RoleName", SqlDbType.NVarChar).Value = r;

                            idRoles.Add((int)cmd.ExecuteScalar());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new ProviderException(e.Message);
            }

            return idRoles.ToArray();
        }

    }
}
