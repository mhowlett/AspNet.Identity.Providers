## Storage providers for the ASP.NET 5 Identity membership system

### Introduction

Just launched this. More coming soon.


## Usage

### PostgreSQL

You set everything up in the ConfigureServices method of the Startup class.

Note that the relevant namespaces are:

    AspNet.Identity.StorageProviders.Common;
    AspNet.Identity.StorageProviders.PostgreSQL;

You must first inject an instance of the PostgreSQLDatabase class like so:

    services.AddPostgreSQLDatabase(
        new PostgreSQLDatabase(connectionString));

In the future, it will be possible to customize the tables and columns
used to store the identity information. For now though you can't. 

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

Show me you care and it will help me prioritize this over other things :-)

I intend to put a bit more work into this over the next week or so (including adding a MySql version).

## Roadmap

1. The class structure isn't to my liking yet - this library was hurridly created largely by grabbing bits from other projects. First priority is to make these modifications. 

2. Once this is in order, I'll create a MySQL provider, which will be very very similar to the PostgreSQL one.

3. Although pretty much feature complete, practically nothing has been tested, and it's certainly going to have a lot of bugs at this point. So priority 3 is to fix all these bugs.