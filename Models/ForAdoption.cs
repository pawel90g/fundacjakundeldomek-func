using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FundacjaKundelDomek;
internal class ForAdoption
{
    [JsonPropertyName("photos")]
    public List<string> Photos { get; set; } = [];
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; }
    [JsonPropertyName("expired")]
    public bool Expired { get; set; }
}