using GraphQL;
using GraphQL.Types;
using Libplanet.Seed.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Libplanet.Seed.Controllers
{
    public class SeedController : Controller
    {
        private readonly ITransportContext _context;
        private readonly Schema _schema;

        public SeedController(ITransportContext context)
        {
            _context = context;
            _schema = context.GetSchema();
        }

        [HttpGet("/")]
        public IActionResult GetRoot()
        {
            return Redirect("/playground.html");
        }

        [HttpPost("/graphql/")]
        public IActionResult GetGraphQLResult(
            [FromBody] GraphQLBody body
        )
        {
            var json = _schema.Execute(_ =>
            {
                _.UserContext = _context;
                _.Query = body.Query;
                _.ThrowOnUnhandledException = true;
                if (body.Variables != null)
                {
                    _.Inputs = body.Variables.ToString(Newtonsoft.Json.Formatting.None).ToInputs();
                }
            });
            return Ok(JObject.Parse(json));
        }
    }
}
