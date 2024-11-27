
using Microsoft.AspNetCore.Server.Kestrel.Core;

using System.Net;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => {
    options.Listen(IPAddress.Any, 5001, listenOptions => {
        listenOptions.UseHttps();
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.Use(async (context, next) => {
    context.Response.Headers.Append("Link", "</numbers>; rel=preload");
    await next();
});

app.MapGet("/", () => "Hello, World!");

app.MapGet("/numbers", async context => {
    context.Response.Headers.Add("Content-Type", "text/event-stream");

    for (int i = 0; i < 10; i++) {
        await context.Response.WriteAsync($"data: {i}\n\n");
        await context.Response.Body.FlushAsync();
        await Task.Delay(1000); // Update every second
    }
});

app.Run();