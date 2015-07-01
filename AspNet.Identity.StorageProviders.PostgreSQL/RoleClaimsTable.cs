// A PostgreSQL provider for (version 3 of) the ASP.NET Identity membership system
// For credits, copyright an license information refer to LICENSE.txt

using System;
using System.Collections.Generic;
using System.Security.Claims;


namespace AspNet.Identity.PostgreSQL
{
    public class RoleClaimsTable<TKey>
        where TKey : IEquatable<TKey>
    {
        private PostgreSQLDatabase _database;

        public RoleClaimsTable(PostgreSQLDatabase database)
        {
            _database = database;
        }

        public ClaimsIdentity FindByRoleId(string roleId)
        {
            ClaimsIdentity claims = new ClaimsIdentity();
            string commandText = "SELECT * FROM \"AspNetRoleClaims\" WHERE \"RoleId\" = @roleId";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@RoleId", roleId } };

            var rows = _database.Query(commandText, parameters);
            foreach (var row in rows)
            {
                Claim claim = new Claim(row["ClaimType"], row["ClaimValue"]);
                claims.AddClaim(claim);
            }

            return claims;
        }

        public int Insert(Claim roleClaim, string roleId)
        {
            string commandText = "INSERT INTO \"AspNetRoleClaims\" (\"ClaimValue\", \"ClaimType\", \"RoleId\") VALUES (@value, @type, @roleId)";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("value", roleClaim.Value);
            parameters.Add("type", roleClaim.Type);
            parameters.Add("roleId", roleId);

            return _database.Execute(commandText, parameters);
        }

        public int Delete(string roleId, Claim claim)
        {
            string commandText = "DELETE FROM \"AspNetRoleClaims\" WHERE \"RoleId\" = @roleId AND @ClaimValue = @value AND ClaimType = @type";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("roleId", roleId);
            parameters.Add("value", claim.Value);
            parameters.Add("type", claim.Type);

            return _database.Execute(commandText, parameters);
        }

        /// <summary>
        ///     Deletes all claims from a role given a roleId.
        /// </summary>
        public int Delete(string roleId)
        {
            string commandText = "DELETE FROM \"AspNetRoleClaims\" WHERE \"RoleId\" = @roleId";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("roleId", roleId);

            return _database.Execute(commandText, parameters);
        }
    }
}
