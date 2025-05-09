using SmartElectricityAPI.Middleware;

namespace SmartElectricityAPI;

public static class ApplicationMiddlewareExtensions
{
    public static void UseApplicationMiddleware(this WebApplication app)
    {
     
        // Configure the HTTP request pipeline
       
        app.UseCors("ReactFrontend");

            app.UseSwagger();
            app.UseSwaggerUI();
        //}
        //else
        //{
        //    app.UseExceptionHandler("/Home/Error");
        //    app.UseHsts();
        //}

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        //app.UseCookiePolicy(
        //    new CookiePolicyOptions
        //    {
        //        Secure = CookieSecurePolicy.Always,
        //        HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always
        //    });
        app.UseCookiePolicy(new CookiePolicyOptions
        {
            Secure = app.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always,
            HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always
        });

        app.UseCustomAuthMiddleware();

        app.MapControllers();

        app.Use((context, next) =>
        {
            context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            return next.Invoke();
        });
    }
}
