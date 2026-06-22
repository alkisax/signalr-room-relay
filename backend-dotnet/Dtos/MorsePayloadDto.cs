namespace backend_dotnet;

public record MorsePayloadDto
(
  string RoomId,
  List<string> MorseLetters,
  string TranslatedText
);
