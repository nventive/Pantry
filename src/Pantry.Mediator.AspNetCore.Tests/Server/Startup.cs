using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pantry.Exceptions;

namespace Pantry.Mediator.AspNetCore.Tests.Server
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddConcurrentDictionaryRepository<StandardEntity>();

            services
                .AddMediator()
                .TryAddRepositoryHandlersForRequestsInAssemblyContaining<Startup>();
            // services.AddOpenApiDocument();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseExceptionHandler(app =>
            {
                app.Run(async context =>
                {
                    var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                    var exception = exceptionHandlerFeature?.Error!;
                    var jsonOptions = app.ApplicationServices.GetRequiredService<IOptions<JsonOptions>>().Value;

                    var responseModel = exception switch
                    {
                        NotFoundException notFoundException => new ProblemDetails
                        {
                            Title = "The target was not found.",
                            Detail = notFoundException.Message,
                            Instance = $"{notFoundException.TargetType}/{notFoundException.TargetId}",
                            Status = StatusCodes.Status404NotFound,
                        },
                        _ => new ProblemDetails
                        {
                            Title = "Internal server error.",
                            Detail = exception.Message,
                            Status = StatusCodes.Status500InternalServerError,
                        }
                    };

                    context.Response.ContentType = "application/problem+json";
                    context.Response.StatusCode = responseModel.Status ?? StatusCodes.Status500InternalServerError;
                    await JsonSerializer.SerializeAsync(context.Response.Body, responseModel, jsonOptions.JsonSerializerOptions);
            });
        });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDomainRequest<GetStandardEntityByIdQuery>(
                    "api/v1/standard-entities/{id}",
                    "GET");
            });

            app.UseOpenApi();
        }
    }
}
