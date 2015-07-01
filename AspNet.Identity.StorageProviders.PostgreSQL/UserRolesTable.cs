// A PostgreSQL provider for (version 3 of) the ASP.NET Identity membership system
// For credits, copyright an license information refer to LICENSE.txt

using System;
using System.Collections.Generic;


namespace AspNet.Identity.PostgreSQL
{
    /// <summary>
    /// Class that represents the AspNetUserRoles table in the PostgreSQL Database.
    /// </summary>
    public class UserRolesTable<TUser, TRole, TKey>
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {
        private PostgreSQLDatabase _database;

        /// <summary>
        /// Constructor that takes a PostgreSQLDatabase instance.
        /// </summary>
        /// <param name="database"></param>
        public UserRolesTable(PostgreSQLDatabase database)
        {
            _database = database;
        }

        /// <summary>
        /// Returns a list of user's roles.
        /// </summary>
        /// <param name="userId">The user's id.</param>
        /// <returns></returns>
        public List<string> RoleNamesForUserId(string userId)
        {
            throw new NotImplementedException();

            var roles = new List<string>();
            // TODO: This probably does not work, and may need testing.
            string commandText = "SELECT \"AspNetRoles\".\"Name\" FROM \"AspNetUsers\", \"AspNetRoles\", \"AspNetUserRoles\" ";
            commandText += "WHERE \"AspNetUsers\".\"Id\" = \"AspNetUserRoles\".\"UserId\" AND \"AspNetUserRoles\".\"RoleId\" = \"AspNetRoles\".\"Id\";";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@userId", userId);

            var rows = _database.Query(commandText, parameters);
            foreach (var row in rows)
            {
                roles.Add(row["Name"]);
            }

            return roles;
        }

        public List<TKey> UsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes all roles from a user in the AspNetUserRoles table.
        /// </summary>
        /// <param name="userId">The user's id.</param>
        /// <returns></returns>
        public int Delete(string userId)
        {
            string commandText = "DELETE FROM \"AspNetRoles\" WHERE \"UserId\" = @userId";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("UserId", userId);

            return _database.Execute(commandText, parameters);
        }

        public int Delete(string userId, string roleId)
        {
            string commandText = "DELETE FROM \"AspNetRoles\" WHERE \"UserId\" = @userId AND \"RoleId\" = @roleId";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("UserId", userId);
            parameters.Add("RoleId", roleId);

            return _database.Execute(commandText, parameters);
        }

        /// <summary>
        /// Inserts a new role record for a user in the UserRoles table.
        /// </summary>
        /// <param name="user">The User.</param>
        /// <param name="roleId">The Role's id.</param>
        /// <returns></returns>
        public int Insert(string userId, string roleId) 
        {
            string commandText = "INSERT INTO \"AspNetUserRoles\" (\"UserId\", \"RoleId\") VALUES (@userId, @roleId)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("userId", userId);
            parameters.Add("roleId", roleId);

            return _database.Execute(commandText, parameters);
        }
    }
}
