using System.Text.Json.Serialization;

namespace FundacjaKundelDomek;

internal class Charity
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("photo")]
    public string Photo { get; set; }
    [JsonPropertyName("link")]
    public string Link { get; set; }
    [JsonPropertyName("visible")]
    public bool Visible { get; set; }
}