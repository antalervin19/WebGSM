using HyperDatabase;
using HyperContainer;
using HyperUtility;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// ---------------- DATABASE INIT ----------------
DatabaseManagement.Initialize();

// ---------------- SELF ID ----------------
string selfId = DatabaseManagement.GetSetting("selfid");

if (string.IsNullOrWhiteSpace(selfId))
{
    selfId = Utility.GetRandomID(64);
    DatabaseManagement.SetSetting("selfid", selfId);

    Console.WriteLine($"Generated Hyper Self ID: {selfId}");
}
else
{
    Console.WriteLine($"Loaded Hyper Self ID: {selfId}");
}

// ---------------- PAIR TOKEN ----------------
string pairToken = DatabaseManagement.GetSetting("pairtoken");

if (string.IsNullOrWhiteSpace(pairToken))
{
    pairToken = Utility.GetRandomID(32);
    DatabaseManagement.SetSetting("pairtoken", pairToken);

    Console.WriteLine($"Generated Pair Token: {pairToken}");
}
else
{
    Console.WriteLine($"Loaded Pair Token: {pairToken}");
}

Console.WriteLine("Hyper Running!");

// ============================================================
// AUTH
// ============================================================
bool IsAuthorized(HttpRequest request)
{
    string? panelId = request.Headers["X-Panel-ID"];
    string? apiKey = request.Headers["X-API-Key"];

    string savedPanel = DatabaseManagement.GetSetting("panelid");
    string savedKey = DatabaseManagement.GetSetting("apikey");

    // Unpaired mode = allow requests
    if (string.IsNullOrWhiteSpace(savedPanel))
        return true;

    return panelId == savedPanel && apiKey == savedKey;
}

// ============================================================
// BASIC
// ============================================================
app.MapGet("/", () => "Hyper Running");

app.MapGet("/api/ping", () => "Bong");

// ============================================================
// HYPER ROUTES
// ============================================================

// Info
app.MapGet("/api/hyper/info", () =>
{
    return Results.Ok(new
    {
        SelfId = DatabaseManagement.GetSetting("selfid"),
        PanelId = DatabaseManagement.GetSetting("panelid"),
        Paired = DatabaseManagement.IsPaired()
    });
});

// Pair panel
app.MapPost("/api/hyper/pairpanel/{panelId}/{apiKey}/{token}",
    (string panelId, string apiKey, string token) =>
{
    string existingPanel = DatabaseManagement.GetSetting("panelid");

    if (!string.IsNullOrWhiteSpace(existingPanel))
        return Results.BadRequest("Hyper already paired.");

    string savedToken = DatabaseManagement.GetSetting("pairtoken");

    if (token != savedToken)
        return Results.Unauthorized();

    DatabaseManagement.SetSetting("panelid", panelId);
    DatabaseManagement.SetSetting("apikey", apiKey);

    return Results.Ok("Panel paired successfully.");
});

// Unpair
app.MapPost("/api/hyper/unpair", (HttpRequest request) =>
{
    if (!IsAuthorized(request))
        return Results.Unauthorized();

    DatabaseManagement.RemoveSetting("panelid");
    DatabaseManagement.RemoveSetting("apikey");

    return Results.Ok("Hyper unpaired.");
});

// ============================================================
// CONTAINER ROUTES
// ============================================================

// Create
app.MapPost("/api/container/create/{id}",
    (HttpRequest request, string id) =>
{
    if (!IsAuthorized(request))
        return Results.Unauthorized();

    var limits = new ContainerLimits();

    ContainerManagement.CreateContainer(id, limits);

    return Results.Ok($"Created container {id}");
});

// Start
app.MapPost("/api/container/start/{id}",
    (HttpRequest request, string id) =>
{
    if (!IsAuthorized(request))
        return Results.Unauthorized();

    ContainerManagement.StartContainer(id);
    return Results.Ok($"Started {id}");
});

// Stop
app.MapPost("/api/container/stop/{id}",
    (HttpRequest request, string id) =>
{
    if (!IsAuthorized(request))
        return Results.Unauthorized();

    ContainerManagement.StopContainer(id);
    return Results.Ok($"Stopped {id}");
});

// Restart
app.MapPost("/api/container/restart/{id}",
    (HttpRequest request, string id) =>
{
    if (!IsAuthorized(request))
        return Results.Unauthorized();

    ContainerManagement.RestartContainer(id);
    return Results.Ok($"Restarted {id}");
});

// Kill
app.MapPost("/api/container/kill/{id}",
    (HttpRequest request, string id) =>
{
    if (!IsAuthorized(request))
        return Results.Unauthorized();

    ContainerManagement.KillContainer(id);
    return Results.Ok($"Killed {id}");
});

// Delete
app.MapPost("/api/container/delete/{id}",
    (HttpRequest request, string id) =>
{
    if (!IsAuthorized(request))
        return Results.Unauthorized();

    ContainerManagement.DeleteContainer(id);
    return Results.Ok($"Deleted {id}");
});

// Stats
app.MapGet("/api/container/stats/{id}",
    (HttpRequest request, string id) =>
{
    if (!IsAuthorized(request))
        return Results.Unauthorized();

    return Results.Ok(ContainerManagement.GetContainerStats(id));
});

// Monitor
app.MapGet("/api/container/monitor/{id}",
    (HttpRequest request, string id) =>
{
    if (!IsAuthorized(request))
        return Results.Unauthorized();

    return Results.Ok(ContainerManagement.MonitorContainer(id));
});

// List
app.MapGet("/api/container/list",
    (HttpRequest request) =>
{
    if (!IsAuthorized(request))
        return Results.Unauthorized();

    return Results.Ok(ContainerManagement.ListAllContainersJson());
});

// ============================================================
// START SERVER
// ============================================================
app.Run("http://0.0.0.0:5000");