using Amazon.S3;
using Amazon.Extensions.NETCore.Setup;
using BlazorAuthApp.Components;
using BlazorAuthApp.Components.Account;
using BlazorAuthApp.Data;
using BlazorAuthApp.Interfaces;
using BlazorAuthApp.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// Try to load .env if it exists (development), but don't fail in Docker
try 
{
    if (File.Exists(".env")) 
    {
        DotNetEnv.Env.Load();
    }
} 
catch (Exception ex) 
{
    Console.WriteLine($"Note: .env file not loaded: {ex.Message}");
    // Continue with environment variables already set in Docker
}

var builder = WebApplication.CreateBuilder(args);

// Check if we're running EF migrations or design-time operations
var isDesignTime = args.Contains("--") || 
                   Environment.GetEnvironmentVariable("EF_DESIGN_TIME") == "true" ||
                   AppDomain.CurrentDomain.GetAssemblies()
                       .Any(assembly => assembly.FullName?.StartsWith("Microsoft.EntityFrameworkCore.Design") == true);

Console.WriteLine($"Design-time mode: {isDesignTime}");

// Configure Kestrel for container environments
if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(8081); // HTTP only in containers
    });
}

// Force HTTP only for Docker deployment
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080); // HTTP only
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// Configure AWS only when NOT running EF migrations
if (!isDesignTime)
{
    try 
    {
        // Get AWS credentials exclusively from environment variables
        var awsAccessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID") 
            ?? throw new InvalidOperationException("AWS_ACCESS_KEY_ID not found in environment variables");
            
        var awsSecretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY")
            ?? throw new InvalidOperationException("AWS_SECRET_ACCESS_KEY not found in environment variables");
            
        var awsRegion = Environment.GetEnvironmentVariable("AWS_DEFAULT_REGION")
            ?? throw new InvalidOperationException("AWS_DEFAULT_REGION not found in environment variables");
            
        var s3BucketName = Environment.GetEnvironmentVariable("S3_BUCKET_NAME")
            ?? throw new InvalidOperationException("S3_BUCKET_NAME not found in environment variables");

        // Disable AWS SDK's automatic credential resolution
        Environment.SetEnvironmentVariable("AWS_SDK_LOAD_USER_SETTINGS", "false");

        // Configure AWS with explicit credentials from environment
        var awsOptions = new AWSOptions
        {
            Credentials = new Amazon.Runtime.BasicAWSCredentials(awsAccessKey, awsSecretKey),
            Region = Amazon.RegionEndpoint.GetBySystemName(awsRegion)
        };

        builder.Services.AddAWSService<IAmazonS3>(awsOptions);

        // Store bucket name in configuration for services to use
        builder.Configuration["AWS:S3:BucketName"] = s3BucketName;

        Console.WriteLine("AWS services configured successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"AWS configuration skipped: {ex.Message}");
        // For design-time or when AWS is not available, add a null S3 service
        builder.Services.AddScoped<IAmazonS3>(provider => null!);
    }
}
else
{
    Console.WriteLine("Skipping AWS configuration during design-time operations");
    // Add a placeholder S3 service for design-time
    builder.Services.AddScoped<IAmazonS3>(provider => null!);
}

builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBlogService, BlogService>();
builder.Services.AddScoped<IBlogLikeService, BlogLikeService>();
builder.Services.AddScoped<IBlogCommentService, BlogCommentService>();

// Only add ImageUploadService if not in design-time mode
if (!isDesignTime)
{
    builder.Services.AddScoped<IImageUploadService, ImageUploadService>();
}

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

builder.Services.AddAntDesign();

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

// Add health check endpoint
app.MapHealthChecks("/health");

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();