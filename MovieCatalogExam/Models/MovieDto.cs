using System.Text.Json.Serialization;


namespace MovieCatalogExam.Models
{
    internal class MovieDTO
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("posterUrl")]
        public string PosterUrl { get; set; } = string.Empty;

        [JsonPropertyName("trailerUrl")]
        public string TrailerUrl { get; set; } = string.Empty;

        [JsonPropertyName("isWatched")]
        public bool IsWatched { get; set; } = false;
    }
}
