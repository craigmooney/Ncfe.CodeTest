namespace Ncfe.CodeTest
{
    public interface ILearnerDataAccess
    {
        LearnerResponse LoadLearner(int learnerId);
    }
}