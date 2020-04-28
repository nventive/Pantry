using System.Linq;
using System.Text.Json;
using FluentValidation;
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

            services.AddControllers();
            services
                .AddMediator()
                .TryAddRepositoryHandlersForRequestsInAssemblyContaining<Startup>()
                .AddFluentValidationRequestMiddleware()
                .AddValidatorsFromAssemblyContaining<Startup>()
                .AddDomainRequestRestApiExecution();

            services.AddOpenApiDocument();
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
                        ValidationException validationException => new ValidationProblemDetails(
                            validationException.Errors
                                .GroupBy(x => x.PropertyName)
                                .ToDictionary(
                                x => x.Key,
                                x => x.Select(y => y.ErrorMessage).ToArray()))
                        {
                            Status = StatusCodes.Status400BadRequest,
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
                    await JsonSerializer.SerializeAsync(context.Response.Body, responseModel, responseModel.GetType(), jsonOptions.JsonSerializerOptions);
                });
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet<FindStandardEntityQuery>("api/v1/standard-entities", groupName: "Standard Entities");
                endpoints.MapPost<CreateStandardEntityCommand>("api/v1/standard-entities", groupName: "Standard Entities", createdAtRedirectPattern: "api/v1/standard-entities/{Id}");
                endpoints.MapGet<GetStandardEntityByIdQuery>("api/v1/standard-entities/{id}", groupName: "Standard Entities");
                endpoints.MapPut<UpdateStandardEntityCommand>("api/v1/standard-entities/{id}", groupName: "Standard Entities");
                endpoints.MapDelete<DeleteStandardEntityCommand>("api/v1/standard-entities/{id}", groupName: "Standard Entities");
            });

            app.UseOpenApi();
        }
    }
}
