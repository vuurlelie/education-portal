namespace EducationPortal.Presentation.Infrastructure.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseAppPipeline(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }
        else
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.MapStaticAssets();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.Use(async (context, next) =>
        {
            try
            {
                await next();
            }
            catch (UnauthorizedAccessException)
            {
                context.Response.Redirect($"/Identity/Account/Login");
            }
        });

        app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
           .WithStaticAssets();

        app.UseStatusCodePages(context =>
        {
            if (context.HttpContext.Response.StatusCode == StatusCodes.Status404NotFound)
            {
                context.HttpContext.Response.Redirect("/");
            }
            return Task.CompletedTask;
        });

        return app;
    }
}