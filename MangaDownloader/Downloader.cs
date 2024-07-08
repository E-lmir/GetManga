using MangaDownloader.Models;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.IO.Compression;

namespace MangaDownloader
{
    public class Downloader
    {
        public Downloader (HttpClient client)
        {
            _client = client;
            client.DefaultRequestHeaders.Add("Origin", "https://desu.me");
            client.DefaultRequestHeaders.Add("Referer", "https://desu.me");
        }

        private readonly HttpClient _client;

        private const string ApiUrl = "https://desu.me/manga/api/";

        public async Task GetDataFromTitle (string linkToManga, string savePath, int firstChapter = 1, int? lastChapter = null, bool compressToZip = false)
        {
            var titleId = linkToManga.Split('.').Last().TrimEnd('/');
            var json = await this.GetJsonAsync(ApiUrl + titleId);
            var title = this.GetTitleInfo(json);

            if (title is not null)
            {
                var titleDirectory = Path.Combine(savePath, title.Response.Name);
                var chapters = new ConcurrentBag<ChapterInfo>();
                var chapterList = title?.Response?.Chapters?.ChapterList;
                if (chapterList is not null)
                {
                    await Parallel.ForEachAsync(chapterList
                        .Where(x => x.ChapterNumber >= firstChapter && x.ChapterNumber <= (lastChapter ?? int.MaxValue)), async (chapter, cts) =>
                        {
                            json = await this.GetJsonAsync(ApiUrl + titleId + "/chapter/" + chapter.Id);
                            var chapterInfo = this.GetChapterInfo(json);
                            var chapterDirectory = Path.Combine(titleDirectory, chapter.ChapterNumber.ToString());

                            Directory.CreateDirectory(chapterDirectory);
                            var pageList = chapterInfo?.Response.Pages.PageList;
                            if (pageList is not null)
                            {
                                await Parallel.ForEachAsync(pageList, async (page, cts) =>
                                {
                                    var byteArray = await GetPageStream(page.imagePath);
                                    await File
                                .WriteAllBytesAsync(Path.Combine(chapterDirectory, page.pageNumber.ToString() + page.imagePath.Split('.').Last()), byteArray);
                                });
                            }
                        });
                }

                if (compressToZip)
                    ZipFile.CreateFromDirectory(titleDirectory, titleDirectory + ".zip");
            } 
        }

        private async Task<byte[]> GetPageStream (string url)
        {
            var response = await _client.GetAsync(url);

            return await response.Content.ReadAsByteArrayAsync();
        }

        private TitleInfo? GetTitleInfo (string json) => JsonConvert.DeserializeObject<TitleInfo>(json);

        private ChapterInfo? GetChapterInfo (string json) => JsonConvert.DeserializeObject<ChapterInfo>(json);
            
        private async Task<string> GetJsonAsync(string url)
        {
            var response = await _client.PostAsync(url, null);

            return await response.Content.ReadAsStringAsync();
        }
    }
}
