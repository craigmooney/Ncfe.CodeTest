namespace Ncfe.CodeTest
{
    public class FailoverLearnerDataAccess
    {
        // Note: We cant directly interface this class as it is using a static method and we are not permitted to update the signature.
        public static LearnerResponse GetLearnerById(int id)
        {
            // retrieve learner from database
            return new LearnerResponse();
        }
    }
}
