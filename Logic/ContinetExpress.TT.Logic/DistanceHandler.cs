using ContinetExpress.TT.Logic.Models;

namespace ContinetExpress.TT.Logic
{
    public interface IHandler<TRequest, TResponse>
    {
        Task<TResponse> HandleAsync(DistanceRequest distanceRequest, CancellationToken cancellationToken);
    }

    public class DistanceHandler(IDistanceCalculator distanceCalculator) : IHandler<DistanceRequest, float>
    {
        public Task<float> HandleAsync(DistanceRequest distanceRequest, CancellationToken cancellationToken)
        {
            distanceCalculator.Calculate
        }
    }
}
