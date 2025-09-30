using IAMService.Extensions;
using IAMService.Options;
using IAMService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureSerilogLogging();

const string allowAngularPolicy = "AllowAngular";

builder.Services.AddCors(options =>
{
    options.AddPolicy(allowAngularPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var authUsersOptions = builder.Configuration.GetSection(AuthUsersOptions.SectionName).Get<AuthUsersOptions>() ?? new AuthUsersOptions();
builder.Services.AddSingleton(authUsersOptions);

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddControllers();

var app = builder.Build();

app.UseCors(allowAngularPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
