using App3.CoreSpace;
using App3.CoreSpace.Interfaces;
using App3.Modals;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace App3
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            
            services.AddSingleton<IServices, TrackServices>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

                
                c.MapType<Criterion>(() => new OpenApiSchema
                {
                    Type = "string",
                    Enum = Enum.GetNames(typeof(Criterion))
                        .Select(name => new OpenApiString(name))
                        .Cast<IOpenApiAny>()
                        .ToList()
                });

                
                c.SchemaFilter<EnumSchemaFilter>();
            });

            
            services.AddSingleton<IRepository>(sp =>
                new TrackRepository(sp.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection")));
        }

            public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // Включите Swagger UI
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API V1");
                });
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}