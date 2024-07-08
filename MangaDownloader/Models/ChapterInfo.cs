using Newtonsoft.Json;

namespace MangaDownloader.Models
{
    public record ChapterInfo ([JsonProperty("response")] ChapterResponse Response);

    public record ChapterResponse ([JsonProperty("id")] int Id, [JsonProperty("pages")] Pages Pages);

    public record Pages ([JsonProperty("list")] Page[] PageList);

    public record Page ([JsonProperty("img")] string imagePath, [JsonProperty("page")] int pageNumber);
}
