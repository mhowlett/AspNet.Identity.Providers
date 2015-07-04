// A PostgreSQL provider for (version 3 of) the ASP.NET Identity membership system
// For credits, copyright an license information refer to LICENSE.txt

using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;


namespace AspNet.Identity.Providers.PostgreSQL
{
    /// <summary>
    ///     Class that encapsulates PostgreSQL database connections and CRUD operations.
    /// </summary>
    public class PostgreSQLDatabase : IDisposable
    {
        private NpgsqlConnection _connection;

        public NpgsqlConnection Connection { get { return _connection; } }

        public PostgreSQLDatabase(string connectionString)
        {
            _connection = new NpgsqlConnection(connectionString);

            if (!TablesExist())
            {
                ExecuteCreateTablesScript();
            }
        }

        /// <summary>
        ///     Executes a non-query PostgreSQL statement.
        /// </summary>
        /// <param name="commandText">
        ///     The PostgreSQL query to execute.
        /// </param>
        /// <param name="parameters">
        ///     Optional parameters to pass to the query.
        /// </param>
        /// <returns>
        ///     The count of records affected by the PostgreSQL statement.
        /// </returns>
        public int Execute(string commandText, Dictionary<string, object> parameters)
        {
            int result = 0;

            if (String.IsNullOrEmpty(commandText))
            {
                throw new ArgumentException("Command text cannot be null or empty.");
            }

            try
            {
                OpenConnection();
                var command = CreateCommand(commandText, parameters);
                result = command.ExecuteNonQuery();
            }
            finally
            {
                _connection.Close();
            }

            return result;
        }

        /// <summary>
        ///     Executes a PostgreSQL query that returns a single scalar value as the result.
        /// </summary>
        /// <param name="commandText">
        ///     The PostgreSQL query to execute.
        /// </param>
        /// <param name="parameters">
        ///     Optional parameters to pass to the query.
        /// </param>
        /// <returns></returns>
        public object QueryValue(string commandText, Dictionary<string, object> parameters)
        {
            object result = null;

            if (String.IsNullOrEmpty(commandText))
            {
                throw new ArgumentException("Command text cannot be null or empty.");
            }

            try
            {
                OpenConnection();
                var command = CreateCommand(commandText, parameters);
                result = command.ExecuteScalar();
            }
            finally
            {
                CloseConnection();
            }

            return result;
        }

        /// <summary>
        ///     Executes a SQL query that returns a list of rows as the result.
        /// </summary>
        /// <param name="commandText">
        ///     The PostgreSQL query to execute.
        /// </param>
        /// <param name="parameters">
        ///     Parameters to pass to the PostgreSQL query.
        /// </param>
        /// <returns>
        ///     A list of a Dictionary of Key, values pairs representing the ColumnName and corresponding value.
        /// </returns>
        public List<Dictionary<string, string>> Query(string commandText, Dictionary<string, object> parameters)
        {
            List<Dictionary<string, string>> rows = null;
            if (String.IsNullOrEmpty(commandText))
            {
                throw new ArgumentException("Command text cannot be null or empty.");
            }

            try
            {
                OpenConnection();
                var command = CreateCommand(commandText, parameters);
                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    rows = new List<Dictionary<string, string>>();
                    while (reader.Read())
                    {
                        var row = new Dictionary<string, string>();
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            var columnName = reader.GetName(i);
                            var columnValue = reader.IsDBNull(i) ? null : reader.GetValue(i).ToString();
                            row.Add(columnName, columnValue);
                        }
                        rows.Add(row);
                    }
                }
            }
            finally
            {
                CloseConnection();
            }

            return rows;
        }

        /// <summary>
        ///     Creates a NpgsqlCommand object with the given parameters.
        /// </summary>
        /// <param name="commandText">
        ///     The PostgreSQL query to execute.
        /// </param>
        /// <param name="parameters">
        ///     Parameters to pass to the PostgreSQL query.
        /// </param>
        /// <returns></returns>
        private NpgsqlCommand CreateCommand(string commandText, Dictionary<string, object> parameters)
        {
            NpgsqlCommand command = _connection.CreateCommand();
            command.CommandText = commandText;
            AddParameters(command, parameters);

            return command;
        }

        /// <summary>
        ///     Adds the parameters to a PostgreSQL command.
        /// </summary>
        /// <param name="commandText">
        ///     The PostgreSQL query to execute.
        /// </param>
        /// <param name="parameters">
        ///     Parameters to pass to the PostgreSQL query.
        /// </param>
        private static void AddParameters(NpgsqlCommand command, Dictionary<string, object> parameters)
        {
            if (parameters == null)
            {
                return;
            }

            foreach (var param in parameters)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = param.Key;
                parameter.Value = param.Value ?? DBNull.Value;
                command.Parameters.Add(parameter);
            }
        }

        /// <summary>
        ///     Helper method to return query a string value. 
        /// </summary>
        /// <param name="commandText">
        ///     The PostgreSQL query to execute.
        /// </param>
        /// <param name="parameters">
        ///     Parameters to pass to the PostgreSQL query.
        /// </param>
        /// <returns>
        ///     The string value resulting from the query.
        /// </returns>
        public string GetStrValue(string commandText, Dictionary<string, object> parameters)
        {
            return QueryValue(commandText, parameters) as string;
        }

        /// <summary>
        ///     Opens a connection if not open.
        /// </summary>
        private void OpenConnection()
        {
            var retries = 10;
            if (_connection.State == System.Data.ConnectionState.Open)
            {
                return;
            }
            else
            {
                while (retries >= 0 && _connection.State != System.Data.ConnectionState.Open)
                {
                    _connection.Open();
                    retries--;
                    Thread.Sleep(50);
                }

            }
        }

        /// <summary>
        ///     Closes the connection if it is open.
        /// </summary>
        public void CloseConnection()
        {
            if (_connection.State == System.Data.ConnectionState.Open)
            {
                _connection.Close();
            }
        }
        
        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
        }

        public bool TablesExist()
        {
            return (bool)QueryValue("select exists(select * from information_schema.tables where table_name = 'aspnetusers')", null);
        }

        public void ExecuteCreateTablesScript()
        {
            // commands in _script are separated by a double new line.

            // i'd really have preferred to load _script from an embedded resource.
            //  .. but currently unsure how to do that in the new dnx world.

            var lines = _script.Split('\n');

            var currentCommand = "";
            foreach (var line in lines)
            {
                if (line.Trim() == "")
                {
                    if (currentCommand.Trim() != "")
                    {
                        Execute(currentCommand.Trim(), null);
                        currentCommand = "";
                    }
                }
                currentCommand += line.Trim() + "\n";
            }
        }

        private string _script = @"
CREATE TABLE AspNetRoles ( 
  Id varchar(128) NOT NULL,
  Name varchar(256) NOT NULL,
  PRIMARY KEY(Id)
);

CREATE TABLE AspNetUsers (
  Id character varying(128) NOT NULL,
  UserName character varying(256) NOT NULL,
  PasswordHash character varying(256),
  SecurityStamp character varying(256),
  Email character varying(256) DEFAULT NULL::character varying,
  EmailConfirmed boolean NOT NULL DEFAULT false,
  PRIMARY KEY (Id)
);

CREATE TABLE AspNetUserClaims ( 
  Id serial NOT NULL,
  ClaimType varchar(256) NULL,
  ClaimValue varchar(256) NULL,
  UserId varchar(128) NOT NULL,
  PRIMARY KEY(Id)
);

CREATE TABLE AspNetUserLogins ( 
  UserId varchar(128) NOT NULL,
  LoginProvider varchar(128) NOT NULL,
  ProviderKey varchar(128) NOT NULL,
  DisplayName varchar(256) NOT NULL,
  PRIMARY KEY(UserId, LoginProvider, ProviderKey)
);

CREATE TABLE AspNetUserRoles ( 
  UserId varchar(128) NOT NULL,
  RoleId varchar(128) NOT NULL,
  PRIMARY KEY(UserId, RoleId)
);

CREATE TABLE AspNetRoleClaims ( 
  Id serial NOT NULL,
  ClaimType varchar(256) NULL,
  ClaimValue varchar(256) NULL,
  RoleId varchar(128) NOT NULL,
  PRIMARY KEY(Id)
);

CREATE INDEX IX_AspNetUserClaims_UserId	ON AspNetUserClaims	(UserId);

CREATE INDEX IX_AspNetUserLogins_UserId	ON AspNetUserLogins	(UserId);

CREATE INDEX IX_AspNetUserRoles_RoleId	ON AspNetUserRoles	(RoleId);

CREATE INDEX IX_AspNetRoleClaims_RoleId	ON AspNetRoleClaims	(RoleId);

CREATE INDEX IX_AspNetUserRoles_UserId	ON AspNetUserRoles	(UserId);

ALTER TABLE AspNetRoleClaims
  ADD CONSTRAINT FK_AspNetRoleClaims_AspNetRoles_Role_Id FOREIGN KEY(RoleId) REFERENCES AspNetRoles (Id)
  ON DELETE CASCADE;

ALTER TABLE AspNetUserClaims
  ADD CONSTRAINT FK_AspNetUserClaims_AspNetUsers_User_Id FOREIGN KEY(UserId) REFERENCES AspNetUsers (Id)
  ON DELETE CASCADE;

ALTER TABLE AspNetUserLogins
  ADD CONSTRAINT FK_AspNetUserLogins_AspNetUsers_UserId FOREIGN KEY(UserId) REFERENCES AspNetUsers (Id)
  ON DELETE CASCADE;

ALTER TABLE AspNetUserRoles
  ADD CONSTRAINT FK_AspNetUserRoles_AspNetRoles_RoleId FOREIGN KEY(RoleId) REFERENCES AspNetRoles (Id)
  ON DELETE CASCADE;

ALTER TABLE AspNetUserRoles
  ADD CONSTRAINT FK_AspNetUserRoles_AspNetUsers_UserId FOREIGN KEY(UserId) REFERENCES AspNetUsers (Id)
  ON DELETE CASCADE;
";

    }
}
