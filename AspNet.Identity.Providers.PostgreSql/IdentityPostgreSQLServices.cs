// A PostgreSQL provider for (version 3 of) the ASP.NET Identity membership system
// For credits, copyright an license information refer to LICENSE.txt

using System;
using System.Collections.Generic;
using Microsoft.Framework.DependencyInjection;
using Microsoft.AspNet.Identity;

namespace AspNet.Identity.Providers.PostgreSQL
{
    /// <summary>
    ///     Default services
    /// </summary>
    public class IdentityPostgreSQLServices
    {
        public static IEnumerable<ServiceDescriptor> GetDefaultServices(Type userType, Type roleType, Type databaseType, Type keyType = null)
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

            // why is ServiceCollection no longer available?
            var services = new List<ServiceDescriptor>();
            services.Add(
                new ServiceDescriptor(
                typeof(IUserStore<>).MakeGenericType(userType),
                userStoreType, ServiceLifetime.Scoped));
            services.Add(
                new ServiceDescriptor(
                typeof(IRoleStore<>).MakeGenericType(roleType),
                roleStoreType, ServiceLifetime.Scoped));
            return services;
        }
    }
}
