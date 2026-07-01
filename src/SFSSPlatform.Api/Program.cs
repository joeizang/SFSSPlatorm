using System.Text.Json;
using System.Text.Json.Serialization;
using SFSSPlatform.Api.Features.Catalog;
using SFSSPlatform.Api.Features.LearningResources;
using SFSSPlatform.Api.Features.Sources;
using SFSSPlatform.Api.Features.StudyItems;
using SFSSPlatform.Api.Features.StudySession;
using SFSSPlatform.Api.Infrastructure;
using SFSSPlatform.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<CSharpExerciseAnalyzer>();
builder.Services.AddStudyPlatformPersistence(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddPolicy("WebUi", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("WebUi");

app.MapCatalogEndpoints();
app.MapTopicNoteEndpoints();
app.MapSourceEndpoints();
app.MapStudyItemEndpoints();
app.MapCodingExerciseEndpoints();
app.MapStudySessionEndpoints();
app.MapLearningResourceEndpoints();
app.MapTrustedYouTubeChannelEndpoints();
app.MapVideoCandidateEndpoints();
app.MapTopicResourceLinkEndpoints();

await app.InitializeDatabaseAsync();
app.Run();

public partial class Program;
