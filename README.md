#﻿#Storage providers for (version 3 of) the ASP.NET Identity membership system.

### Introduction

Just launched this. More coming soon.

## Usage

### PostgreSQL

You set everything up in the ConfigureServices method of the
ASP.NET 5 Startup class.

The relevant namespaces are:

    using AspNet.Identity.StorageProviders.Common;
    using AspNet.Identity.StorageProviders.PostgreSQL;

You must first inject an instance of the PostgreSQLDatabase class like so:

    services.AddPostgreSQLDatabase(
        new PostgreSQLDatabase(connectionString));

In the future, it will be possible to customize the tables and columns
thereof used to store the identity information. For now though you 
can't. 

When you instantiate the PostgreSQLDatabase object, the required tables
will be created automatically in the database if they do not already exist:

    AspNetRoleClaims
    AspNetRoles
    AspNetUserClaims
    AspNetUserLogins
    AspNetUserRoles
    AspNetUsers

You may then set up ASP.NET Identity something like so:

    services.AddIdentity<IdentityUser, IdentityRole>(
            options => {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonLetterOrDigit = false;
                options.Password.RequireUppercase = false;
            } )
        .AddPostgreSQLStores<PostgreSQLDatabase>()
        .AddDefaultTokenProviders();

## Quality

Alpha quality. It's working for me for basic usage. 

Show me you care and it will help me prioritize this over other stuff :-)

I intend to put a bit more work into this over the next week or so (including adding a MySql version).

