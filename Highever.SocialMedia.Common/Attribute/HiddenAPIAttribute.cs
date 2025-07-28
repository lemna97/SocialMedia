using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Highever.SocialMedia.Common
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class HiddenAPIAttribute: System.Attribute
    {
    }
}
