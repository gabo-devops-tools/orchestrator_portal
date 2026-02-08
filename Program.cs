using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Logging;
using orchestrator_portal.Db;
using orchestrator_portal.Dto;
using System.Net;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

//ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
//Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

// https://login.microsoftonline.com/18b08011-0a66-4da0-97f0-cc3e9571c9e9/v2.0/.well-known/openid-configuration

//var client = new HttpClient();
//var res = await client.GetAsync(
//    "https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration"
//);

//Console.WriteLine(res.StatusCode);
//Console.WriteLine(await res.Content.ReadAsStringAsync());


// Add authentication
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
      .AddMicrosoftIdentityWebApp(builder.Configuration)
      .EnableTokenAcquisitionToCallDownstreamApi(new string[]
        {
            "499b84ac-1321-427f-aa17-267ca6975798/.default"
        })
      .AddInMemoryTokenCaches();

// Add MVC + Identity UI
builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

builder.Services.AddScoped<IAzureDevopsFactory, AzureDevopsFactory>();
builder.Services.AddScoped<AzureArmService>();

var dbPath = Path.Combine(AppContext.BaseDirectory, "app.db");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));
//IdentityModelEventSource.ShowPII = true;
//IdentityModelEventSource.LogCompleteSecurityArtifact = true;

builder.Services.AddAuthorization();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    db.Subscriptions.RemoveRange(db.Subscriptions);
    db.Projects.RemoveRange(db.Projects);
    db.Organization.RemoveRange(db.Organization);

    // ⚠️ Only seed Resources if empty
    if (!db.Resources.Any())
    {
        var resources = new List<Resources>
        {
            new Resources
            {
                Name = "Key Vault for infra",
                Rbac = "Key Vault Secrets Officer",
                Description = "Use this resource to assign rbac",
                applyforcode = false,
                applyforinfra = true,
                type = "KeyVault"
            },
            new Resources
            {
                Name = "Storage Account for infra",
                Rbac = "Storage Blob Data Contributor",
                Description = "Use this resource to assign rbac",
                applyforcode = false,
                applyforinfra = true,
                type = "StorageAccount"
            }
        };


        db.Resources.AddRange(resources);
    }

    // ⚠️ Only seed Resources if empty
    if (!db.ServiceConnection.Any())
    {
        var ServiceConnection = new List<ServiceConnection>
        {
            new ServiceConnection
            {
                Name = "azure-devops-$project-$subscriptions[0]",
                Description = "Use this service connection for infra",
                RepoType = RepoType.Infra,
                Scope= "$subscriptions[0]"
            },
            new ServiceConnection
            {
                Name = "azure-devops-kubernetes-$project-$subscriptions[0]",
                Description = "Use this service connection for code",
                RepoType = RepoType.Code,
                Scope= "$subscriptions[0]"

            }
        };


        db.ServiceConnection.AddRange(ServiceConnection);
    }

    if (!db.VariableGroup.Any())
    {
        var VariableGroup = new List<VariableGroup>
        {
            new VariableGroup
            {
                Name = "infra-$project-$subscription[0]",
                Description = "variable group for infra",
            },
            new VariableGroup
            {
                Name = "infra-$project-$subscription[1]",
                Description = "variable group for infra",
            }
        };


        db.VariableGroup.AddRange(VariableGroup);
    }

    if (!db.VariableGroupAssociation.Any())
    {
        var VariableGroupAssociation = new List<VariableGroupAssociation>
        {
            new VariableGroupAssociation
            {
                VariableGroupId = 1,
                 Key= "data",
                 Value = "value"
                 
                
            },
            new VariableGroupAssociation
            {
                 VariableGroupId = 1,
                 Key= "other-data",
                 Value = "value"
            }
        };


        db.VariableGroupAssociation.AddRange(VariableGroupAssociation);
    }

    db.SaveChanges();

    //if (!db.Projects.Any())
    //{
    //    db.Projects.Add(new Projects
    //    {
    //        Id = 1,
    //        Projectname = "demo",
    //        ProjectId = "2"
    //    });

    //    db.SaveChanges();
    //}
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Auth middleware
app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
   // pattern: "{controller=Home}/{action=CreateProjects}");
    pattern: "{controller=Account}/{action=Login}");


app.Run();
