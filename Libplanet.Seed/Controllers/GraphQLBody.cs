using Newtonsoft.Json.Linq;

namespace Libplanet.Seed.Controllers
{
    public class GraphQLBody
    {
        public string? Query { get; set; }

        public JObject? Variables { get; set; }
    }
}
