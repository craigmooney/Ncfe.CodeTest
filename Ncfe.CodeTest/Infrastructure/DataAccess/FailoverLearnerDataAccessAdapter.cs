using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ncfe.CodeTest
{
    // Note: We are implementing ILearnerDataAccess on this class rather than creating its own specific interface (IFailoverLearnerDataAccessAdapter)
    // because it currently provides the exact same functionality needed for this class.
    public class FailoverLearnerDataAccessAdapter : ILearnerDataAccess 
    {
        public LearnerResponse LoadLearner(int learnerID)
        {
            // This adapter calls the static method (without the need to change its signature).
            return FailoverLearnerDataAccess.GetLearnerById(learnerID);
        }
    }
}
