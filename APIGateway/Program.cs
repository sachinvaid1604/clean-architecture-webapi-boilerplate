using Microsoft.AspNetCore;
using Newtonsoft.Json;
using Ocelot.Configuration.File;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text.RegularExpressions;

public class Program
{
    public static void Main(string[] args)
    {
        CreateWebHostBuilder(args)
            .Build()
            .Run();
    }


    #region "Public"

    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .UseContentRoot(Directory.GetCurrentDirectory())
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
               
                config.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                .AddOcelot(GetOcelotFiles(hostingContext.HostingEnvironment.ContentRootPath + "\\Microservices", hostingContext.HostingEnvironment))
                .AddEnvironmentVariables();
            })
            .ConfigureServices(s =>
            {
                s.AddOcelot();

            })
            .Configure(app =>
            {
                var configuration = new OcelotPipelineConfiguration
                {
                    PreErrorResponderMiddleware = async (context, next) =>
                    {
                        await next.Invoke();
                    },
                    PreAuthenticationMiddleware = async (context, next) =>
                    {
                        await next.Invoke();
                    },
                    PreAuthorizationMiddleware = async (context, next) =>
                    {
                        await next.Invoke();
                    },
                    PreQueryStringBuilderMiddleware = async (context, next) =>
                    {
                        await next.Invoke();
                    }

                };
                app.UseOcelot(configuration).Wait();
            });

    #endregion

    #region "Private"

    private static readonly Regex SubConfigRegexVar = new Regex("^ocelot\\.(.*?)\\.json$", RegexOptions.IgnoreCase | RegexOptions.Singleline, TimeSpan.FromMilliseconds(1000.0));

    private static Regex SubConfigRegex()
    {
        return SubConfigRegexVar;
    }
    private static FileConfiguration GetOcelotFiles(string folder, IWebHostEnvironment env)
    {
        string excludeConfigName = ((env != null && env.EnvironmentName != null) ? ("ocelot." + env.EnvironmentName + ".json") : string.Empty);
        Regex reg = SubConfigRegex();
        FileInfo[] array = (from fi in new DirectoryInfo(folder).EnumerateFiles("*", SearchOption.AllDirectories)
                            where reg.IsMatch(fi.Name) && fi.Name != excludeConfigName
                            select fi).ToArray();
        FileConfiguration fileConfiguration = new FileConfiguration();
        FileInfo[] array2 = array;
        foreach (FileInfo fileInfo in array2)
        {
            if (array.Length <= 1 || !fileInfo.Name.Equals("ocelot.json", StringComparison.OrdinalIgnoreCase))
            {
                FileConfiguration fileConfiguration2 = JsonConvert.DeserializeObject<FileConfiguration>(File.ReadAllText(fileInfo.FullName));
                if (fileInfo.Name.Equals("ocelot.global.json", StringComparison.OrdinalIgnoreCase))
                {
                    fileConfiguration.GlobalConfiguration = fileConfiguration2.GlobalConfiguration;
                }

                fileConfiguration.Aggregates.AddRange(fileConfiguration2.Aggregates);
                fileConfiguration.Routes.AddRange(fileConfiguration2.Routes);
            }
        }

        return fileConfiguration;
    }
    #endregion
}
