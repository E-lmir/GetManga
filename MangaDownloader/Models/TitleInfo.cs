using Newtonsoft.Json;

namespace MangaDownloader.Models
{
    public record TitleInfo ([JsonProperty("response")] TitleResponse Response);

    public record TitleResponse ([JsonProperty("chapters")] Chapters? Chapters, [JsonProperty("name")] string Name);

    public record Chapters ([JsonProperty("list")] Chapter[]? ChapterList);

    public record Chapter ([JsonProperty("id")] int Id, [JsonProperty("vol")] int Volume, [JsonProperty("ch")] double ChapterNumber);
}
