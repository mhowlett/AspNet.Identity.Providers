// A PostgreSQL provider for (version 3 of) the ASP.NET Identity membership system
// For credits, copyright an license information refer to LICENSE.txt

using System;
using System.Collections.Generic;
using AspNet.Identity.Providers.Common;


namespace AspNet.Identity.Providers.PostgreSQL
{
    /// <summary>
    /// Class that represents the AspNetRoles table in the PostgreSQL Database.
    /// </summary>
    public class RoleTable<TRole, TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {
        private PostgreSQLDatabase _database;

        /// <summary>
        /// Constructor that takes a PostgreSQLDatabase instance.
        /// </summary>
        /// <param name="database"></param>
        public RoleTable(PostgreSQLDatabase database)
        {
            _database = database;
        }

        /// <summary>
        /// Deletes a role record from the AspNetRoles table.
        /// </summary>
        /// <param name="roleId">The role Id</param>
        /// <returns></returns>
        public int Delete(string roleId)
        {
            string commandText = "DELETE FROM AspNetRoles WHERE Id = @id";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@id", roleId);

            return _database.Execute(commandText, parameters);
        }

        /// <summary>
        /// Inserts a new Role record in the AspNetRoles table.
        /// </summary>
        /// <param name="roleName">The role's name.</param>
        /// <returns></returns>
        public int Insert(IdentityRole<TKey> role)
        {
            string commandText = "INSERT INTO AspNetRoles (Id, Name) VALUES (@id, @name)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@name", role.Name);
            parameters.Add("@id", role.Id);

            return _database.Execute(commandText, parameters);
        }

        /// <summary>
        /// Returns a role name given the roleId.
        /// </summary>
        /// <param name="roleId">The role Id.</param>
        /// <returns>Role name.</returns>
        public string GetRoleName(string roleId)
        {
            string commandText = "SELECT Name FROM AspNetRoles WHERE Id = @id";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@id", roleId);

            return _database.GetStrValue(commandText, parameters);
        }

        /// <summary>
        /// Returns the role Id given a role name.
        /// </summary>
        /// <param name="roleName">Role's name.</param>
        /// <returns>Role's Id.</returns>
        public string GetRoleId(string roleName)
        {
            string roleId = null;
            string commandText = "SELECT Id FROM AspNetRoles WHERE Name = @name";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@name", roleName } };

            var result = _database.QueryValue(commandText, parameters);
            if (result != null)
            {
                return Convert.ToString(result);
            }

            return roleId;
        }

        public int Update(IdentityRole<TKey> role)
        {
            string commandText = "UPDATE AspNetRoles SET Name = @name WHERE Id = @id";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@id", role.Id);

            return _database.Execute(commandText, parameters);
        }
    }
}
