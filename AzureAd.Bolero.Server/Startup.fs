namespace AzureAd.Bolero.Server

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.AzureAD.UI;
open Microsoft.AspNetCore.Authentication.JwtBearer
open Bolero.Remoting.Server
open Bolero.Server.RazorHost
open Bolero.Templating.Server

type Startup (configuration : IConfiguration) =

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    member this.ConfigureServices(services: IServiceCollection) =
        services.AddMvc().AddRazorRuntimeCompilation() |> ignore
        services.AddServerSideBlazor() |> ignore
                
        services
            .AddAuthorization()
            .AddAuthentication(AzureADDefaults.BearerAuthenticationScheme)
            .AddAzureADBearer(fun options ->
                options.Instance <- "https://login.microsoftonline.com/"
                options.ClientId <- configuration.["ClientId"]
                options.Domain <- "azurewebsites.net"
                options.TenantId <- configuration.["TenantId"])
            .Services
            .Configure<JwtBearerOptions>(
                AzureADDefaults.JwtBearerAuthenticationScheme,
                (fun (options : JwtBearerOptions) ->
                    options.Audience <- configuration.["Audience"]
                    options.TokenValidationParameters.NameClaimType <- "name"))
            .AddBoleroHost()
#if DEBUG
            .AddHotReload(templateDir = __SOURCE_DIRECTORY__ + "/../AzureAd.Bolero.Web")
#endif
        |> ignore

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        app
            .UseAuthentication()
            .UseAuthorization()
            .UseRemoting()
            .UseStaticFiles()
            .UseRouting()
            .UseBlazorFrameworkFiles()
            .UseEndpoints(fun endpoints ->
#if DEBUG
                endpoints.UseHotReload()
#endif
                endpoints.MapBlazorHub() |> ignore
                endpoints.MapFallbackToPage("/_Host") |> ignore)
        |> ignore

module Program =

    [<EntryPoint>]
    let main args =
        WebHost
            .CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(fun context config ->
                config.AddJsonFile "appsettings.json" |> ignore)
            .UseStaticWebAssets()
            .UseStartup<Startup>()
            .Build()
            .Run()
        0
