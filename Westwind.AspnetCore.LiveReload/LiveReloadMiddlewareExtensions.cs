﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Westwind.AspNetCore.LiveReload
{
    /// <summary>
    /// The Middleware Hookup extensions.
    /// </summary>
    public static class LiveReloadMiddlewareExtensions
    {
        /// <summary>
        /// Configure the MarkdownPageProcessor in Startup.ConfigureServices.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddLiveReload(this IServiceCollection services,
            Action<LiveReloadConfiguration> configAction = null)
        {
            var provider = services.BuildServiceProvider();
            var serviceConfiguration = provider.GetService<IConfiguration>();
            
            var config = new LiveReloadConfiguration();
            serviceConfiguration.Bind("LiveReload",config);
            LiveReloadConfiguration.Current = config;

            if (config.LiveReloadEnabled)
            {
                if (string.IsNullOrEmpty(config.FolderToMonitor))
                {
                    var env = provider.GetService<IHostingEnvironment>();
                    config.FolderToMonitor = env.ContentRootPath;
                }

                if (configAction != null)
                    configAction.Invoke(config);

                LiveReloadConfiguration.Current = config;
            }

            return services;
        }


        /// <summary>
        /// Hook up the Markdown Page Processing functionality in the Startup.Configure method
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseLiveReload(this IApplicationBuilder builder)
        {
            var config = LiveReloadConfiguration.Current;

            if (config.LiveReloadEnabled)
            {
                var webSocketOptions = new WebSocketOptions()
                {
                    KeepAliveInterval = TimeSpan.FromSeconds(120),
                    ReceiveBufferSize = 1024
                };
                builder.UseWebSockets(webSocketOptions);

                builder.UseMiddleware<LiveReloadMiddleware>();

                LiveReloadFileWatcher.StartFileWatcher();
            }

            return builder;
        }

    }
}