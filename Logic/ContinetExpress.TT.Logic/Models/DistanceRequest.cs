using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContinetExpress.TT.Logic.Models
{
    public struct DistanceRequest(string source, string destination)
    {
        public string Source { get; } = source;
        public string Destination { get; } = destination;
    }
}
