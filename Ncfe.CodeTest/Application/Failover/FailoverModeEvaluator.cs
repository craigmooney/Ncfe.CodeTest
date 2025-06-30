using System;
using System.Configuration;
using System.Linq;

namespace Ncfe.CodeTest
{
    public class FailoverModeEvaluator : IFailoverModeEvaluator
    {
        private readonly IFailoverRepository _failoverRepository;
        private readonly int _failedRequestThreshold = 100;     
        private readonly int _failoverWindowMinutes = 10;       

        public FailoverModeEvaluator(IFailoverRepository failoverRepository)
        {
            _failoverRepository = failoverRepository ?? throw new ArgumentNullException(nameof(failoverRepository));
        }

        private bool IsFailoverEnabledInConfig
        {
            get
            {
                if (bool.TryParse(ConfigurationManager.AppSettings["IsFailoverModeEnabled"], out bool configValue))
                {
                    return configValue; // Return parsed value if successful
                }
                return false; // Default to false if parsing fails or setting is missing
            }
        }

        public bool IsFailoverModeActive()
        {
            var failoverEntries = _failoverRepository.GetFailOverEntries();
            var failedRequests = failoverEntries.Count(fe => fe.DateTime > DateTime.Now.AddMinutes(-_failoverWindowMinutes));

            return failedRequests > _failedRequestThreshold && IsFailoverEnabledInConfig;
        }
    }
}