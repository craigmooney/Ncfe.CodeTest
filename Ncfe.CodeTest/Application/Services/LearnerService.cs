using Ncfe.CodeTest;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ncfe.CodeTest
{
    public class LearnerService
    {
        private readonly IArchivedDataService _archivedDataService;
        private readonly IFailoverModeEvaluator _failoverModeEvaluator;
        private readonly ILearnerDataAccess _mainLearnerDataAccess;
        private readonly ILearnerDataAccess _failoverLearnerDataAccess;

        // Constructor for Dependency Injection
        public LearnerService(
            IArchivedDataService archivedDataService,
            IFailoverModeEvaluator failoverModeEvaluator,
            ILearnerDataAccess mainLearnerDataAccess,
            ILearnerDataAccess failoverLearnerDataAccess)
        {
            _archivedDataService = archivedDataService ?? throw new ArgumentNullException(nameof(archivedDataService));
            _failoverModeEvaluator = failoverModeEvaluator ?? throw new ArgumentNullException(nameof(failoverModeEvaluator));
            _mainLearnerDataAccess = mainLearnerDataAccess ?? throw new ArgumentNullException(nameof(mainLearnerDataAccess));
            _failoverLearnerDataAccess = failoverLearnerDataAccess ?? throw new ArgumentNullException(nameof(failoverLearnerDataAccess));
        }

        public Learner GetLearner(int learnerId, bool isLearnerArchived)
        {
            // Validate learnerId
            if (learnerId <= 0) // Assuming IDs must be positive
            {
                throw new ArgumentOutOfRangeException(nameof(learnerId), "Learner ID must be a positive integer.");
            }

            if (isLearnerArchived)
            {
                return _archivedDataService.GetArchivedLearner(learnerId);
            }

            LearnerResponse learnerResponse = null;

            if (_failoverModeEvaluator.IsFailoverModeActive())
            {
                learnerResponse = _failoverLearnerDataAccess.LoadLearner(learnerId);
            }
            else
            {
                learnerResponse = _mainLearnerDataAccess.LoadLearner(learnerId);
            }

            // Note: In this small test project learnerResponse should never be null at this point but in a real project you might want to 
            // test for it being null here and if so throw a specific business exception. For example, LearnerNotFoundException.

            // Note: We would not expect the learner to be archived here but will leave the original code in as an additonal check.
            if (learnerResponse.IsArchived)
            {
                return _archivedDataService.GetArchivedLearner(learnerId);
            }
            else
            {
                return learnerResponse.Learner;
            }
        }
    }
}