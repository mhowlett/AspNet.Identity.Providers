// A PostgreSQL provider for (version 3 of) the ASP.NET Identity membership system
// For credits, copyright an license information refer to LICENSE.txt

using System;
using Microsoft.Framework.DependencyInjection;
using AspNet.Identity.StorageProviders.PostgreSQL;


namespace Microsoft.AspNet.Identity
{
    /// <summary>
    /// Default services
    /// </summary>
    public class IdentityPostgreSQLServices
    {
        public static IServiceCollection GetDefaultServices(Type userType, Type roleType, Type databaseType, Type keyType = null)
        {
            Type userStoreType;
            Type roleStoreType;
            if (keyType != null)
            {
                userStoreType = typeof(UserStore<,,,>).MakeGenericType(userType, roleType, databaseType, keyType);
                roleStoreType = typeof(RoleStore<,,>).MakeGenericType(roleType, databaseType, keyType);
            }
            else
            {
                userStoreType = typeof(UserStore<,,>).MakeGenericType(userType, roleType, databaseType);
                roleStoreType = typeof(RoleStore<,>).MakeGenericType(roleType, databaseType);
            }

            var services = new ServiceCollection();
            services.AddScoped(
                typeof(IUserStore<>).MakeGenericType(userType),
                userStoreType);
            services.AddScoped(
                typeof(IRoleStore<>).MakeGenericType(roleType),
                roleStoreType);
            return services;
        }
    }
}
