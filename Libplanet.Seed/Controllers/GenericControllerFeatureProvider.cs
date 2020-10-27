using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Libplanet.Seed.Controllers
{
    public class GenericControllerFeatureProvider
        : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(
            IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            TypeInfo typeInfo = typeof(SeedController).GetTypeInfo();

            // Check to see if there is a "real" controller for this class
            if (feature.Controllers.All(t => t.Name != typeInfo.Name))
            {
                // Create a generic controller for this type
                feature.Controllers.Add(typeInfo);
            }
        }
    }
}
