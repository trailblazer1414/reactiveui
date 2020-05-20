using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace SignalrServer
{
    public class Startup
    {
 
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
            //services.AddControllers().AddJsonOptions(options =>
            //{
            //    options.JsonSerializerOptions.IgnoreNullValues = true;
            //});

            services.AddCors(options => options.AddPolicy("CorsPolicy",
            builder =>
            {
                builder.AllowAnyMethod()
                       .AllowAnyHeader()
                       .AllowCredentials();
            }));

            services.AddSingleton<IUserIdProvider, NameUserIdProvider>();
            services.AddSignalR().AddMessagePackProtocol();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            //else
            //{
            //    app.UseHsts();
            //}
            //app.UseHttpsRedirection();
            app.UseCors("CorsPolicy");
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<RelayHub>("/relayhub");
                //endpoints.MapControllers();
            });
        }

        public class NameUserIdProvider : IUserIdProvider
        {
            public string GetUserId(HubConnectionContext connection)
            {
                return connection.User?.Identity?.Name;
            }
        }
    }
}
