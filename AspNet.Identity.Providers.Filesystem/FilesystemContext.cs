// A plain text filesystem provider for (version 3 of) the ASP.NET Identity membership system
// For credits, copyright an license information refer to LICENSE.txt

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;
using AspNet.Identity.Providers.Common;
using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;


namespace AspNet.Identity.Providers.Filesystem
{
    public class FilesystemContext : FilesystemContext<IdentityUser, IdentityRole>
    {
        public FilesystemContext(string basePath) : base(basePath) { }
    }

    public class FilesystemContext<TUser, TRole>
        where TUser : IdentityUser
        where TRole : IdentityRole
    {
        protected enum DirectoryStructureStatus
        {
            NotExist,
            Valid,
            Invalid
        }

        private string _basePath;

        public FilesystemContext(string basePath)
        {
            _basePath = basePath;
            switch (ValidateDirectoryStructure())
            {
                case DirectoryStructureStatus.Invalid:
                    throw new Exception("there is a problem with the Filesystem Identity Store directory structure");
                case DirectoryStructureStatus.Valid:
                    break;
                case DirectoryStructureStatus.NotExist:
                    CreateDirectories();
                    break;
                default:
                    throw new Exception("unknown directory structure state.");
            }
        }

        protected virtual DirectoryStructureStatus ValidateDirectoryStructure()
        {
            // TODO: more thorough check for validity.
            if (Directory.Exists(Path.Combine(_basePath, "users")))
            {
                return DirectoryStructureStatus.Valid;
            }
            return DirectoryStructureStatus.NotExist;
        }

        protected void CreateDirectories()
        {
            var userDirectory = Directory.CreateDirectory(Path.Combine(_basePath, "users"));
            for (int l1 = 0; l1 < 36; ++l1)
            {
                char c1;
                if (l1 < 10)
                {
                    c1 = (char)((int)'0' + l1);
                }
                else
                {
                    c1 = (char)((int)'a' + (l1 - 10));
                }
                for (int l2 = 0; l2 < 36; ++l2)
                {
                    char c2;
                    if (l2 < 10)
                    {
                        c2 = (char)((int)'0' + l2);
                    }
                    else
                    {
                        c2 = (char)((int)'a' + (l2 - 10));
                    }
                    Directory.CreateDirectory(Path.Combine(userDirectory.FullName, "" + c1 + c2));
                }
            }
            Directory.CreateDirectory(Path.Combine(_basePath, "users-by-name"));
            Directory.CreateDirectory(Path.Combine(_basePath, "roles"));
            Directory.CreateDirectory(Path.Combine(_basePath, "roles-by-name"));
        }

        private void JsonReadAssertHelper(bool condition)
        {
            if (!condition)
            {
                throw new Exception("unexpected format");
            }
        }

        protected async virtual Task<List<string>> LoadUserFileRoleIds(string userId)
        {
            return new List<string>();
        }

        protected async virtual Task<List<Claim>> LoadUserFileClaims(string userId)
        {
            // these can just be stored inline I think.
            return new List<Claim>();
        }

        protected async virtual Task<List<UserLoginInfo>> LoadUserFileLogins(string userId)
        {
            return new List<UserLoginInfo>();
        }

        protected async virtual Task<TUser> LoadUserFile(string userId)
        {
            return await Task.Run(() =>
            {
                var path = Path.Combine(new[] { _basePath, "users", userId.Substring(2), userId });
                if (!File.Exists(path))
                {
                    return null;
                }

                var result = new IdentityUser();
                result.Id = userId;
                using (var tr = new StreamReader(path))
                using (var jr = new JsonTextReader(tr))
                {
                    JsonReadAssertHelper(jr.Read() && jr.TokenType == JsonToken.StartObject);
                    while (jr.Read())
                    {
                        if (jr.TokenType == JsonToken.EndObject)
                        {
                            break;
                        }
                        var propertyName = jr.Value.ToString();
                        JsonReadAssertHelper(propertyName != null);
                        switch (propertyName)
                        {
                            case "UserName":
                                jr.Read();
                                JsonReadAssertHelper(jr.Value != null);
                                result.UserName = jr.Value.ToString();
                                break;
                            case "NormalizedUserName":
                                jr.Read();
                                JsonReadAssertHelper(jr.Value != null);
                                result.NormalizedUserName = jr.Value.ToString();
                                break;
                            case "Email":
                                jr.Read();
                                JsonReadAssertHelper(jr.Value != null);
                                result.Email = jr.Value.ToString();
                                break;
                            case "NormalizedEmail":
                                jr.Read();
                                JsonReadAssertHelper(jr.Value != null);
                                result.NormalizedEmail = jr.Value.ToString();
                                break;
                            case "EmailConfirmed":
                                jr.Read();
                                JsonReadAssertHelper(jr.Value != null);
                                result.EmailConfirmed = (bool)jr.Value;
                                break;
                            case "PasswordHash":
                                jr.Read();
                                JsonReadAssertHelper(jr.Value != null);
                                result.PasswordHash = jr.Value.ToString();
                                break;
                            case "SecurityStamp":
                                jr.Read();
                                JsonReadAssertHelper(jr.Value != null);
                                result.SecurityStamp = jr.Value.ToString();
                                break;
                            case "ConcurrencyStamp":
                                jr.Read();
                                JsonReadAssertHelper(jr.Value != null);
                                result.ConcurrencyStamp = jr.Value.ToString();
                                break;
                            case "PhoneNumber":
                                jr.Read();
                                JsonReadAssertHelper(jr.Value != null);
                                result.PhoneNumber = jr.Value.ToString();
                                break;
                            case "PhoneNumberConfirmed":
                                jr.Read();
                                JsonReadAssertHelper(jr.Value != null);
                                result.PhoneNumberConfirmed = (bool)jr.Value;
                                break;
                            case "TwoFactorEnabled":
                                jr.Read();
                                JsonReadAssertHelper(jr.Value != null);
                                result.TwoFactorEnabled = (bool)jr.Value;
                                break;
                            case "LockoutEnd":
                                throw new Exception("LockoutEnd serialization not implemented");
                                break;
                            case "LockoutEnabled":
                                jr.Read();
                                JsonReadAssertHelper(jr.Value != null);
                                result.LockoutEnabled = (bool)jr.Value;
                                break;
                            case "AccessFailedCount":
                                jr.Read();
                                JsonReadAssertHelper(jr.Value != null);
                                result.AccessFailedCount = (int)jr.Value;
                                break;
                            case "RoleIds":
                                // read past array - .Roles is not writable - skip over this data in this method.
                                jr.Read();
                                JsonReadAssertHelper(jr.TokenType == JsonToken.StartArray);
                                while (jr.TokenType != JsonToken.EndArray)
                                {
                                    jr.Read();
                                }
                                break;
                            case "Claims":
                                // read past array - .Claims is not writable - skip over this data in this method.
                                jr.Read();
                                JsonReadAssertHelper(jr.TokenType == JsonToken.StartArray);
                                while (jr.TokenType != JsonToken.EndArray)
                                {
                                    // the data for claims can be stored inline.
                                    jr.Read();
                                }
                                break;
                            case "Logins":
                                // read past array - .Logins is not writable - skip over this data in this method.
                                jr.Read();
                                JsonReadAssertHelper(jr.TokenType == JsonToken.StartArray);
                                while (jr.TokenType != JsonToken.EndArray)
                                {
                                    // the data for logins can be stored inline.
                                    jr.Read();
                                }
                                break;
                            default:
                                throw new Exception("unexpected format");
                        }
                    }
                }

                return (TUser)result;
            });
        }

        protected async Task<Dictionary<string,string>> LoadLookup(string path)
        {
            var result = new Dictionary<string, string>();
            if (!File.Exists(path))
            {
                return result;
            }
            var lines = File.ReadAllLines(path);
            for (int i=0; i<lines.Length; i+=2)
            {
                result.Add(lines[i], lines[i + 1]);
            }
            return result;
        }

        protected async Task SaveLookup(string path, Dictionary<string, string> lookup)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            using (var sw = new StreamWriter(path))
            {
                foreach (var l in lookup)
                {
                    sw.WriteLine(l.Key);
                    sw.WriteLine(l.Value);
                }
            }
        }

        protected async virtual Task AddNameToIdLookup(string path, string name, string val)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("name not defined");
            }
            var lookup = await LoadLookup(path);
            if (lookup.ContainsKey(name))
            {
                lookup[name] = val;
            }
            else
            {
                lookup.Add(name, val);
            }
            await SaveLookup(path, lookup);
        }

        protected async virtual Task SaveUserFile(TUser user)
        {
            await Task.Run(() =>
            {
                var path = Path.Combine(new[] { _basePath, "users", user.Id.Substring(0,2), user.Id });
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                using (var tw = new StreamWriter(path))
                using (var jw = new JsonTextWriter(tw))
                {
                    jw.WriteStartObject();
                    if (user.UserName != null)
                    {
                        jw.WritePropertyName("UserName");
                        jw.WriteValue(user.UserName);
                    }
                    if (user.NormalizedUserName != null)
                    {
                        jw.WritePropertyName("NormalizedUserName");
                        jw.WriteValue(user.NormalizedUserName);
                    }
                    if (user.Email != null)
                    {
                        jw.WritePropertyName("Email");
                        jw.WriteValue(user.Email);
                    }
                    // i'm unclear whether I'm supposed to do the normalization or the framework.
                    if (user.NormalizedEmail != null)
                    {
                        jw.WritePropertyName("NormalizedEmail");
                        jw.WriteValue(user.NormalizedEmail);
                    }
                    jw.WritePropertyName("EmailConfirmed");
                    jw.WriteValue(user.EmailConfirmed);
                    if (user.PasswordHash != null)
                    {
                        jw.WritePropertyName("PasswordHash");
                        jw.WriteValue(user.PasswordHash);
                    }
                    if (user.SecurityStamp != null)
                    {
                        jw.WritePropertyName("SecurityStamp");
                        jw.WriteValue(user.SecurityStamp);
                    }
                    if (user.ConcurrencyStamp != null)
                    {
                        jw.WritePropertyName("ConcurrencyStamp");
                        jw.WriteValue(user.ConcurrencyStamp);
                    }
                    if (user.PhoneNumber != null)
                    {
                        jw.WritePropertyName("PhoneNumber");
                        jw.WriteValue(user.PhoneNumber);
                    }
                    jw.WritePropertyName("PhoneNumberConfirmed");
                    jw.WriteValue(user.PhoneNumberConfirmed);
                    jw.WritePropertyName("TwoFactorEnabled");
                    jw.WriteValue(user.TwoFactorEnabled);
                    if (user.LockoutEnd != null)
                    {
                        jw.WritePropertyName("LockoutEnd");
                        jw.WriteValue(user.LockoutEnd.Value);
                        throw new NotImplementedException("LockoutEnd serialization not implemented");
                    }
                    if (user.LockoutEnabled)
                    {
                        jw.WritePropertyName("LockoutEnabled");
                        jw.WriteValue(user.LockoutEnabled);
                    }
                    jw.WritePropertyName("AccessFailedCount");
                    jw.WriteValue(user.AccessFailedCount);
                    jw.WritePropertyName("RoleIds");
                    jw.WriteStartArray();
                    foreach (var role in user.Roles)
                    {
                        throw new NotImplementedException("role writing not implemented");
                    }
                    jw.WriteEndArray();
                    jw.WritePropertyName("Claims");
                    jw.WriteStartArray();
                    foreach (var claim in user.Claims)
                    {
                        // write claim data inline.
                        throw new NotImplementedException("claim writing not implemented");
                    }
                    jw.WriteEndArray();
                    jw.WritePropertyName("Logins");
                    jw.WriteStartArray();
                    foreach (var login in user.Logins)
                    {
                        // write login data inline.
                        throw new NotImplementedException("login writing not implemented");
                    }
                    jw.WriteEndArray();
                    jw.WriteEndObject();
                }
            });
        }

        protected async virtual Task<TRole> LoadRoleFile(string roleId)
        {
            return await Task.Run(() =>
           {
               var path = Path.Combine(new[] { _basePath, "roles", roleId });
               if (!File.Exists(path))
               {
                   return null;
               }

               var result = new IdentityRole();
               result.Id = roleId;
               using (var tr = new StreamReader(path))
               using (var jr = new JsonTextReader(tr))
               {
                   JsonReadAssertHelper(jr.Read() && jr.TokenType == JsonToken.StartObject);
                   while (jr.Read())
                   {
                       if (jr.TokenType == JsonToken.EndObject)
                       {
                           break;
                       }
                       var propertyName = jr.Value.ToString();
                       JsonReadAssertHelper(propertyName != null);
                       switch (propertyName)
                       {
                           case "Name":
                               jr.Read();
                               JsonReadAssertHelper(jr.Value != null);
                               result.Name = jr.Value.ToString();
                               break;
                           case "NormalizedName":
                               jr.Read();
                               JsonReadAssertHelper(jr.Value != null);
                               result.NormalizedName = jr.Value.ToString();
                               break;
                           case "ConcurrencyStamp":
                               jr.Read();
                               JsonReadAssertHelper(jr.Value != null);
                               result.ConcurrencyStamp = jr.Value.ToString();
                               break;
                           case "Claims":
                               // read past array - .Claims is not writable - skip over this data in this method.
                               jr.Read();
                               JsonReadAssertHelper(jr.TokenType == JsonToken.StartArray);
                               while (jr.TokenType != JsonToken.EndArray)
                               {
                                   jr.Read();
                               }
                               break;
                           default:
                               throw new Exception("unexpected format");
                       }
                   }
               }

               return (TRole)result;
           });
        }

        protected async virtual Task SaveRoleExceptUsers(TRole role)
        {
            await Task.Run(() =>
            {
                var path = Path.Combine(new[] { _basePath, "roles", role.Id });
                File.Delete(path);
                using (var tw = new StreamWriter(path))
                using (var jw = new JsonTextWriter(tw))
                {
                    jw.WriteStartObject();
                    if (role.Name != null)
                    {
                        jw.WritePropertyName("Name");
                        jw.WriteValue(role.Name);
                    }
                    if (role.NormalizedName != null)
                    {
                        jw.WritePropertyName("NormalizedName");
                        jw.WriteValue(role.NormalizedName);
                    }
                    if (role.ConcurrencyStamp != null)
                    {
                        jw.WritePropertyName("ConcurrencyStamp");
                        jw.WriteValue(role.ConcurrencyStamp);
                    }
                    jw.WritePropertyName("Claims");
                    jw.WriteStartArray();
                    foreach (var claim in role.Claims)
                    {
                        throw new NotImplementedException();
                    }
                    jw.WriteEnd();
                    jw.WriteEndObject();
                }
            });
        }


        public virtual async Task UpdateRole(TRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("UpdateRole");
        }

        public virtual async Task RemoveRole(TRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("RemoveRole");
        }

        public virtual async Task AddRole(TRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("AddRole");
        }

        public virtual async Task<TRole> FindRoleById(string roldId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("FindRoleById");
            return await Task.FromResult((TRole)(new IdentityRole<string>()));
        }

        public virtual async Task<TRole> FindRoleByName(string normalizedName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("FindRoleByName");
            return await Task.FromResult((TRole)(new IdentityRole<string>()));
        }

        public virtual async Task<IList<Claim>> GetRoleClaims(string id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("GetRoleClaims");
            return new List<Claim>();
        }

        public virtual async Task AddRoleClaim(IdentityRoleClaim<string> roleClaim, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("AddRoleClaim");
        }

        public virtual async Task RemoveRoleClaim(string roleId, string roleType, string roleValue, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("RemoveRoleClaim");
        }




        public virtual async Task AddUser(TUser user, CancellationToken cancellationToken)
        {
            await SaveUserFile(user);
            string prefix = user.NormalizedUserName.Substring(0, 1);
            if (user.NormalizedUserName.Length > 1)
            {
                prefix = user.NormalizedUserName.Substring(0, 2);
            }
            await AddNameToIdLookup(Path.Combine(new[] { _basePath, "users-by-name", prefix }), user.NormalizedUserName, user.Id);
        }

        public virtual async Task UpdateUser(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("UpdateUser");
        }

        public virtual async Task RemoveUser(TUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("RemoveUser");
        }

        public virtual async Task<TUser> FindUserById(string userId, CancellationToken cancellationToken)
        {
            return await LoadUserFile(userId);
        }

        public virtual async Task<TUser> FindUserByName(string normalizedName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(normalizedName))
            {
                return null;
            }
            string prefix = normalizedName.Substring(0, 1);
            if (normalizedName.Length > 1)
            {
                prefix = normalizedName.Substring(0, 2);
            }
            var path = Path.Combine(new[] { _basePath, "users-by-name", prefix });
            var lookup = await LoadLookup(path);
            if (!lookup.ContainsKey(normalizedName))
            {
                return null;
            }
            var userId = lookup[normalizedName];
            return await LoadUserFile(userId);
        }

        public virtual async Task AddRoleToUser(IdentityUserRole<string> userRole, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("AddRoleToUser");
        }

        public virtual async Task RemoveRoleFromUser(string userId, string roleId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("RemoveRoleFromUser");
        }

        public virtual async Task<List<string>> GetUserRoles(string userId, CancellationToken cancellationToken)
        {
            var roleIds = await LoadUserFileRoleIds(userId);
            var result = new List<string>();
            foreach (var rid in roleIds)
            {
                var role = (await LoadRoleFile(rid)).NormalizedName;
                result.Add(role);
            }
            return result;
        }

        public virtual async Task<bool> UserIsInRole(string userId, string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("UserIsInRole");
            return await Task.FromResult(false);
        }

        public virtual async Task<TUser> FindUserByEmail(string normalizedEmail, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("FindUserByEmail");
            return await Task.FromResult((TUser)(new IdentityUser<string>()));
        }
        
        public virtual async Task<List<TUser>> GetUsersForClaim(Claim claim, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("GetUsersForClaim");
            return await Task.FromResult(new List<TUser>());
        }

        public virtual async Task<List<TUser>> GetUsersInRole(string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("GetUsersInRole");
            return await Task.FromResult(new List<TUser>());
        }

        public virtual async Task<List<Claim>> GetUserClaims(TUser user, CancellationToken cancellationToken)
        {
            return await LoadUserFileClaims(user.Id);
        }

        public virtual async Task AddUserClaims(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("AddUserClaims");
            await Task.FromResult(false);
        }

        public virtual async Task ReplaceUserClaims(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("ReplaceUserClaims");
            await Task.FromResult(false);
            /*
            var matchedClaims = await UserClaims.Where(uc => uc.UserId.Equals(user.Id) && uc.ClaimValue == claim.Value && uc.ClaimType == claim.Type).ToListAsync(cancellationToken);
            foreach (var matchedClaim in matchedClaims)
            {
                matchedClaim.ClaimValue = newClaim.Value;
                matchedClaim.ClaimType = newClaim.Type;
            }
            */
        }

        public virtual async Task RemoveUserClaims(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            /*
            foreach (var claim in claims)
            {
                var matchedClaims = await UserClaims.Where(uc => uc.UserId.Equals(user.Id) && uc.ClaimValue == claim.Value && uc.ClaimType == claim.Type).ToListAsync(cancellationToken);
                foreach (var c in matchedClaims)
                {
                    UserClaims.Remove(c);
                }
            }
            */
            throw new NotImplementedException("RemoveUserClaims");
        }

        public virtual async Task AddLogin(TUser user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("AddLogin");
        }

        public virtual async Task RemoveLogin(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("RemoveLogin");
            /*
            var userId = user.Id;
            var entry = await UserLogins.SingleOrDefaultAsync(l => l.UserId.Equals(userId) && l.LoginProvider == loginProvider && l.ProviderKey == providerKey, cancellationToken);
            if (entry != null)
            {
                UserLogins.Remove(entry);
            }
            */
        }

        public virtual async Task<List<UserLoginInfo>> GetLoginsForUser(TUser user, CancellationToken cancellationToken)
        {
            return await LoadUserFileLogins(user.Id);
        }

        public virtual async Task<TUser> FindUserByLogin(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("FindUserByLogin");
            /*
                        var userLogin = await
                UserLogins.FirstOrDefaultAsync(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey, cancellationToken);
            if (userLogin != null)
            {
                return await Users.FirstOrDefaultAsync(u => u.Id.Equals(userLogin.UserId), cancellationToken);
            }
            return null;
            */
        }
    }
}
