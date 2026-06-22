using System;

namespace backend_dotnet;

public class LogFromFrontController
{
  public IResult Log(LogFromFrontDto dto)
  {
    System.Console.WriteLine($"Frontend logger: {dto.Data}");
    return Results.Ok(new
    {
      status = true
    });
  }
}
