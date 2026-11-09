using System.Reflection;
using Microsoft.EntityFrameworkCore;
using RaceDayApi.Data;
using System.Globalization;

namespace RaceDayApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // always read and write numbers like 10.5 with a dot or whatever language the computer uses
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            // Add services to the container.

            builder.Services.AddControllers();
           

            builder.Services.AddDbContext<RaceDayDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.Cookie.Name = ".RaceDayWeb.Session";
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Swagger (requirement 4)
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                // read the /// comments from the XML file the compiler creates
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile), true);
            });

            var app = builder.Build();

            app.UseSwagger();     // creates the swagger.json description
            app.UseSwaggerUI();   // creates the web page at /swagger

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
