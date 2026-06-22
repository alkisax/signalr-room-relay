using System.ComponentModel.DataAnnotations;

namespace backend_dotnet;

public record LogFromFrontDto
(
  [Required]
  string Data
);
