using Newtonsoft.Json.Linq;

namespace ConfigComponent.Dtos
{
    public class ConfigResponseDto
    {
        public JObject DefaultConfig { get; set; }
        public JObject UserConfig { get; set; }
    }
}