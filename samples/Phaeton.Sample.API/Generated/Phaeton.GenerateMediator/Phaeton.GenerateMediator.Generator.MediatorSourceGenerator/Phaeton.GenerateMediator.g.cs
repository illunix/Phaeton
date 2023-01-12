
using Phaeton.Abstractions;

namespace Phaeton.Sample.API.Features
{
    public partial class GetWeatherForecast 
    {
        public partial record Query : IQuery<System.Collections.Generic.IReadOnlyList<Phaeton.Sample.API.Features.GetWeatherForecast.WeatherForecast>>;
        private class QueryHandlerCore : IQueryHandler<GetWeatherForecast.Query, System.Collections.Generic.IReadOnlyList<Phaeton.Sample.API.Features.GetWeatherForecast.WeatherForecast>>
        {
            
            public async Task<System.Collections.Generic.IReadOnlyList<Phaeton.Sample.API.Features.GetWeatherForecast.WeatherForecast>> Handle(Query request, CancellationToken cancellationToken) 
            {
                return await Handler(request);
            }
        }
    }
}

