using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SDKProcesio.Config;

namespace SDKProcesio
{
    public class Startup
    {
        private IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public void ConfigureService(IServiceCollection service)
        {
            var modelConfiguration = Configuration
                .GetSection("ProcesioConfig")
                .Get<ProcesioConfig>();
            service.AddSingleton(modelConfiguration);
        }

        public void Configure() { }
        
    }
}

