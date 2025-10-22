using ContinetExpress.TT.Logic.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContinetExpress.TT.Logic.Calculate.Decorators.LocalStorage.Repositories
{
    public interface IAirportsRepository
    {
        Task<IReadOnlyCollection<Airport>> SearchAsync(string src_airport, string dest_airport, CancellationToken cancellationToken);
    }
}
