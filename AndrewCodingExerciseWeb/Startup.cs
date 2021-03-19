using AutoMapper;
using Andrew.Context;
using Andrew.Services.Intrefaces;
using Andrew.Services.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Swashbuckle.Swagger;

namespace AndrewCodingExerciseWeb
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            //Set database connection string from appsetings.json
            services.AddDbContext<EFDBContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            //register interfaces
            services.AddScoped<IProcessPaymentService, ProcessPaymentService>();

            //AutoMapper registered through assembly scanning
            services.AddAutoMapper(typeof(Startup).Assembly);

            //Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1.0", null);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //Swagger
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "My Demo API (V 1.0)");
            });
        }
    }
}
