﻿using System;
using System.Data.Entity;
using System.Web.Http;
using Autofac;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security.OAuth;
using Owin;
using Mlb5.App_Start;
using Mlb5.Migrations;
using Mlb5.Security;
using RazorEngine;

[assembly: OwinStartup(typeof(Mlb5.Startup))]

namespace Mlb5
{
    public class Startup
    {
        private IContainer _container;

        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            _container = AutofacConfig.Start();

            ConfigureOAuth(app);
            GlobalConfiguration.Configuration.IncludeErrorDetailPolicy
= IncludeErrorDetailPolicy.Always;

            JsonConfig.Register(config);

            WebApiConfig.Register(config);
            app.UseCors(CorsOptions.AllowAll);
            app.UseWebApi(config);

            AutoMapperConfig.Setup();

            Database.SetInitializer(new MigrateDatabaseToLatestVersion<Mlb5Context, Configuration>());
        }

        public void ConfigureOAuth(IAppBuilder app)
        {
            OAuthAuthorizationServerOptions OAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/oauth/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(1),


                Provider = new SimpleAuthorizationServerProvider()
            };

            // Token Generation
            app.UseOAuthAuthorizationServer(OAuthServerOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

        }
    }
}
