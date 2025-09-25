using EducationPortal.Presentation.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAppInfrastructure(builder.Configuration);

builder.Services.AddIdentityWithUi();

builder.Services.AddApplicationServices();

builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

if(app.Environment.IsDevelopment())
{
    var seedSection = app.Configuration.GetSection("DemoSeed");

    if (seedSection.GetValue("Enable", false))
    {
        var hardReset = seedSection.GetValue("HardReset", false);

        if (hardReset)
            await app.ResetDemoDomainAsync();

        await app.SeedLookupsAsync();
        await app.SeedDemoDomainAsync();
    }
}

app.UseAppPipeline();
app.Run();