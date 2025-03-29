public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder MapStaticAssets(this IApplicationBuilder app)
    {
        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = ctx =>
            {
                // Cache static files for 30 days in production
                if (!ctx.Context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
                {
                    ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=2592000");
                }
            }
        });
        return app;
    }

    public static IEndpointRouteBuilder WithStaticAssets(this IEndpointRouteBuilder endpoints)
    {
        // Additional static file configuration if needed
        return endpoints;
    }
}