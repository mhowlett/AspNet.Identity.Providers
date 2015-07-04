// A PostgreSQL provider for (version 3 of) the ASP.NET Identity membership system
// For credits, copyright an license information refer to LICENSE.txt

using System;
using System.Collections.Generic;
using AspNet.Identity.Providers.Common;


namespace AspNet.Identity.Providers.PostgreSQL
{
    public delegate TKey ConverIdFromStringDelegate<TKey>(string id);

    /// <summary>
    /// Class that represents the AspNetUsers table in the PostgreSQL database.
    /// </summary>
    public class UserTable<TUser, TKey>
        where TUser : IdentityUser<TKey>
        where TKey : IEquatable<TKey>
    {
        private PostgreSQLDatabase _database;

        private string _tableName = "AspNetUsers";
        public string TableName { get; }

        public UserTable(PostgreSQLDatabase db)
        {
            _database = db;
        }

        public UserTable(PostgreSQLDatabase db, 
            string tableName,
            Dictionary<string, string> columnNameMapping)
        {
            _database = db;
            _tableName = tableName;
            if (columnNameMapping != null)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the user's name, provided with an ID.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public string GetUserName(string userId)
        {
            string commandText = "SELECT UserName FROM " + _tableName + " WHERE Id = @id";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@id", userId } };

            return _database.GetStrValue(commandText, parameters);
        }

        /// <summary>
        /// Gets the user's ID, provided with a user name.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public string GetUserId(string userName)
        {
            //Due to PostgreSQL's case sensitivity, we have another column for the user name in lowercase.
            if (userName != null)
                userName = userName.ToLower();

            string commandText = "SELECT Id FROM " + _tableName + " WHERE LOWER(UserName) = @name";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@name", userName } };

            return _database.GetStrValue(commandText, parameters);
        }

        /// <summary>
        /// Returns an TUser given the user's id.
        /// </summary>
        /// <param name="userId">The user's id.</param>
        /// <returns></returns>
        public TUser GetUserById(string userId, ConverIdFromStringDelegate<TKey> stringToId)
        {
            TUser user = null;
            string commandText = "SELECT * FROM " + _tableName + " WHERE Id = @id";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@id", userId } };

            var rows = _database.Query(commandText, parameters);
            if (rows != null && rows.Count == 1)
            {
                var row = rows[0];
                user = (TUser)Activator.CreateInstance(typeof(TUser));
                user.Id = stringToId(row["id"]);
                user.UserName = row["username"];
                user.PasswordHash = string.IsNullOrEmpty(row["passwordhash"]) ? null : row["passwordhash"];
                user.SecurityStamp = string.IsNullOrEmpty(row["securitystamp"]) ? null : row["securitystamp"];
                user.Email = string.IsNullOrEmpty(row["email"]) ? null : row["email"];
                user.EmailConfirmed = row["emailconfirmed"] == "True";
            }

            return user;
        }

        /// <summary>
        /// Returns a list of TUser instances given a user name.
        /// </summary>
        /// <param name="userName">User's name.</param>
        /// <returns></returns>
        public List<TUser> GetUserByName(string userName, ConverIdFromStringDelegate<TKey> stringToId)
        {
            //Due to PostgreSQL's case sensitivity, we have another column for the user name in lowercase.
            if (userName != null)
                userName = userName.ToLower();

            List<TUser> users = new List<TUser>();
            string commandText = "SELECT * FROM " + _tableName + " WHERE LOWER(UserName) = @name";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@name", userName } };

            var rows = _database.Query(commandText, parameters);
            foreach (var row in rows)
            {
                TUser user = (TUser)Activator.CreateInstance(typeof(TUser));
                user.Id = stringToId(row["id"]);
                user.UserName = row["username"];
                user.PasswordHash = string.IsNullOrEmpty(row["passwordhash"]) ? null : row["passwordhash"];
                user.SecurityStamp = string.IsNullOrEmpty(row["securitystamp"]) ? null : row["securitystamp"];
                user.Email = string.IsNullOrEmpty(row["email"]) ? null : row["email"];
                user.EmailConfirmed = row["emailconfirmed"] == "True";
                users.Add(user);
            }

            return users;
        }

        /// <summary>
        /// Returns a list of TUser instances given a user email.
        /// </summary>
        /// <param name="email">User's email address.</param>
        /// <returns></returns>
        public List<TUser> GetUserByEmail(string email, ConverIdFromStringDelegate<TKey> stringToId)
        {
            //Due to PostgreSQL's case sensitivity, we have another column for the user name in lowercase.
            if (email != null)
                email = email.ToLower();

            List<TUser> users = new List<TUser>();
            string commandText = "SELECT * FROM " + _tableName + " WHERE LOWER(Email) = @email";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@email", email } };

            var rows = _database.Query(commandText, parameters);
            foreach (var row in rows)
            {
                TUser user = (TUser)Activator.CreateInstance(typeof(TUser));
                user.Id = stringToId(row["id"]);
                user.UserName = row["username"];
                user.PasswordHash = string.IsNullOrEmpty(row["passwordhash"]) ? null : row["passwordhash"];
                user.SecurityStamp = string.IsNullOrEmpty(row["securitystamp"]) ? null : row["securitystamp"];
                user.Email = string.IsNullOrEmpty(row["email"]) ? null : row["email"];
                user.EmailConfirmed = row["emailconfirmed"] == "True";
                users.Add(user);
            }

            return users;
        }

        /// <summary>
        /// Return the user's password hash.
        /// </summary>
        /// <param name="userId">The user's id.</param>
        /// <returns></returns>
        public string GetPasswordHash(string userId)
        {
            string commandText = "SELECT PasswordHash FROM " + _tableName + " WHERE Id = @id";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@id", userId);

            var passHash = _database.GetStrValue(commandText, parameters);
            if (string.IsNullOrEmpty(passHash))
            {
                return null;
            }

            return passHash;
        }

        /// <summary>
        /// Sets the user's password hash.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="passwordHash"></param>
        /// <returns></returns>
        public int SetPasswordHash(string userId, string passwordHash)
        {
            string commandText = "UPDATE " + _tableName + " SET PasswordHash = @pwdHash WHERE Id = @id";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@pwdHash", passwordHash);
            parameters.Add("@id", userId);

            return _database.Execute(commandText, parameters);
        }

        /// <summary>
        /// Returns the user's security stamp.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public string GetSecurityStamp(string userId)
        {
            string commandText = "SELECT SecurityStamp FROM " + _tableName + " WHERE Id = @id";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@id", userId } };
            var result = _database.GetStrValue(commandText, parameters);

            return result;
        }

        /// <summary>
        /// Inserts a new user in the AspNetUsers table.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public int Insert(TUser user)
        {
            var lowerCaseEmail = user.Email == null ? null : user.Email.ToLower();

            string commandText = @"
            INSERT INTO AspNetUsers(Id, UserName, PasswordHash, SecurityStamp, Email, 
                                        EmailConfirmed)
            VALUES (@id, @name, @pwdHash, @SecStamp, @email, @emailconfirmed);";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@name", user.UserName);
            parameters.Add("@id", user.Id);
            parameters.Add("@pwdHash", user.PasswordHash);
            parameters.Add("@SecStamp", user.SecurityStamp);
            parameters.Add("@email", user.Email);
            parameters.Add("@emailconfirmed", user.EmailConfirmed);

            return _database.Execute(commandText, parameters);
        }

        /// <summary>
        /// Deletes a user from the AspNetUsers table.
        /// </summary>
        /// <param name="userId">The user's id.</param>
        /// <returns></returns>
        public int Delete(string userId)
        {
            string commandText = "DELETE FROM " + _tableName + " WHERE Id = @userId";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@userId", userId);

            return _database.Execute(commandText, parameters);
        }

        /// <summary>
        /// Updates a user in the AspNetUsers table.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public int Update(TUser user)
        {
            var lowerCaseEmail = user.Email == null ? null : user.Email.ToLower();

            string commandText = @"
                UPDATE AspNetUsers
                   SET UserName = @userName, PasswordHash = @pswHash, SecurityStamp = @secStamp, Email= @email, 
                       EmailConfirmed = @emailconfirmed,
                 WHERE Id = @userId;";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@userName", user.UserName);
            parameters.Add("@pswHash", user.PasswordHash);
            parameters.Add("@secStamp", user.SecurityStamp);
            parameters.Add("@userId", user.Id);
            parameters.Add("@email", user.Email);
            parameters.Add("@emailconfirmed", user.EmailConfirmed);

            return _database.Execute(commandText, parameters);
        }
    }
}
