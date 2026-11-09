using RaceDayWeb.Services;
namespace RaceDayWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // the MVC app's own session
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);// if he user is idle for more than 30 minutes they are kicked out and need to restart the session
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // the HttpClient that talks to the Part 2 API
            builder.Services.AddHttpClient("RaceDayApi", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseCookies = false });

            builder.Services.AddScoped<ApiService>();
            builder.Services.AddSingleton<ImageUrlService>();

            var app = builder.Build();

           
            app.UseExceptionHandler("/Home/Error");
            app.UseStatusCodePagesWithReExecute("/Home/StatusPage", "?code={0}");

            if (app.Environment.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseExceptionHandler("/Home/Error");

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseSession();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
