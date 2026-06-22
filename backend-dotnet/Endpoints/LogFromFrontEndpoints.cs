using System;

namespace backend_dotnet;

public static class LogFromFrontEndpoints
{
  public static RouteGroupBuilder MapLogFromFrontEndpoints(
    this WebApplication app
  )
  {
    RouteGroupBuilder group = app.MapGroup("/api/log-from-front");
    group.MapPost("/", (LogFromFrontController controller, LogFromFrontDto dto) =>
    {
      return controller.Log(dto);
    });

    return group;
  }
}
