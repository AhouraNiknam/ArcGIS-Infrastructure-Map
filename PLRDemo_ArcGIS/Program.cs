using PLRDemo_ArcGIS;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

builder.Services.AddSingleton<IInfrastructureService, ArcGisInfrastructureService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod())); // temporary only

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();

app.MapGet("/infrastructure", async (
    double lat, double lon, double radiusMeters,
    IInfrastructureService svc, CancellationToken ct) =>
{
    if (radiusMeters is <= 0)
        return Results.BadRequest("radiusMeters must be higher than 0");
    var result = await svc.QueryAsync(lat, lon, radiusMeters, ct);
    return Results.Ok(result);
});

app.Run();