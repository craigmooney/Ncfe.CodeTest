using Xunit;
using Moq; 
using Ncfe.CodeTest;
using System;
using System.Configuration;

namespace Ncfe.CodeTest.UnitTests
{
    public class LearnerServiceTests
    {
        private readonly Mock<IArchivedDataService> _mockArchivedDataService;
        private readonly Mock<IFailoverModeEvaluator> _mockFailoverModeEvaluator;
        private readonly Mock<ILearnerDataAccess> _mockMainLearnerDataAccess;
        private readonly Mock<ILearnerDataAccess> _mockFailoverLearnerDataAccess;

        private readonly LearnerService _learnerService; // Class we are testing

        public LearnerServiceTests()
        {
            // Initialize Mocks
            _mockArchivedDataService = new Mock<IArchivedDataService>();
            _mockFailoverModeEvaluator = new Mock<IFailoverModeEvaluator>();
            _mockMainLearnerDataAccess = new Mock<ILearnerDataAccess>();
            _mockFailoverLearnerDataAccess = new Mock<ILearnerDataAccess>(); // This will be the adapter

            // Initialize the LearnerService, injecting the Mock objects
            _learnerService = new LearnerService(
                _mockArchivedDataService.Object,
                _mockFailoverModeEvaluator.Object,
                _mockMainLearnerDataAccess.Object,
                _mockFailoverLearnerDataAccess.Object
            );
        }

        [Fact]
        public void GetLearner_WhenLearnerIdIsZeroOrNegative_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var invalidLearnerId = 0; // Or -1, or any negative number

            // Act & Assert (using Assert.Throws to check for exceptions)
            // The lambda (() => ...) wraps the method call so Assert.Throws can catch the exception.
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                _learnerService.GetLearner(invalidLearnerId, false); // Pass any value for isLearnerArchived as it's irrelevant here
            });

            // You can also verify that no other service calls occurred, as the method should exit early
            _mockArchivedDataService.Verify(s => s.GetArchivedLearner(It.IsAny<int>()), Times.Never);
            _mockFailoverModeEvaluator.Verify(e => e.IsFailoverModeActive(), Times.Never);
            _mockMainLearnerDataAccess.Verify(d => d.LoadLearner(It.IsAny<int>()), Times.Never);
            _mockFailoverLearnerDataAccess.Verify(d => d.LoadLearner(It.IsAny<int>()), Times.Never);
        }

        [Fact] // Denotes a single test method
        public void GetLearner_WhenIsLearnerArchivedIsTrue_ReturnsArchivedLearner()
        {
            // Arrange (Set up mocks and test data)
            var learnerId = 1;
            var expectedLearner = new Learner { Id = learnerId, Name = "Archived Test Learner" };
            _mockArchivedDataService.Setup(s => s.GetArchivedLearner(learnerId)).Returns(expectedLearner);

            // Act (Call the method under test)
            var result = _learnerService.GetLearner(learnerId, true);

            // Assert (Verify the outcome)
            Assert.NotNull(result);
            Assert.Equal(expectedLearner.Id, result.Id);
            Assert.Equal(expectedLearner.Name, result.Name);

            // Verify that the archived service was called exactly once
            _mockArchivedDataService.Verify(s => s.GetArchivedLearner(learnerId), Times.Once);
            // Verify that other services were NOT called
            _mockFailoverModeEvaluator.Verify(e => e.IsFailoverModeActive(), Times.Never);
            _mockMainLearnerDataAccess.Verify(d => d.LoadLearner(It.IsAny<int>()), Times.Never);
            _mockFailoverLearnerDataAccess.Verify(d => d.LoadLearner(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void GetLearner_WhenNotArchivedAndFailoverModeIsOff_ReturnsMainLearner()
        {
            // Arrange
            var learnerId = 2;
            var expectedLearner = new Learner { Id = learnerId, Name = "Main Test Learner" };
            var learnerResponse = new LearnerResponse { Learner = expectedLearner, IsArchived = false };

            _mockFailoverModeEvaluator.Setup(e => e.IsFailoverModeActive()).Returns(false); // Failover is off
            _mockMainLearnerDataAccess.Setup(d => d.LoadLearner(learnerId)).Returns(learnerResponse);

            // Act
            var result = _learnerService.GetLearner(learnerId, false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedLearner.Id, result.Id);
            Assert.Equal(expectedLearner.Name, result.Name);

            // Verify interactions
            _mockFailoverModeEvaluator.Verify(e => e.IsFailoverModeActive(), Times.Once);
            _mockMainLearnerDataAccess.Verify(d => d.LoadLearner(learnerId), Times.Once);
            // Ensure failover and archived services were not used
            _mockFailoverLearnerDataAccess.Verify(d => d.LoadLearner(It.IsAny<int>()), Times.Never);
            _mockArchivedDataService.Verify(s => s.GetArchivedLearner(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void GetLearner_WhenNotArchivedAndFailoverModeIsOn_ReturnsFailoverLearner()
        {
            // Arrange
            var learnerId = 3;
            var expectedLearner = new Learner { Id = learnerId, Name = "Failover Test Learner" };
            var learnerResponse = new LearnerResponse { Learner = expectedLearner, IsArchived = false };

            _mockFailoverModeEvaluator.Setup(e => e.IsFailoverModeActive()).Returns(true); // Failover is on
            _mockFailoverLearnerDataAccess.Setup(d => d.LoadLearner(learnerId)).Returns(learnerResponse); // Call to adapter

            // Act
            var result = _learnerService.GetLearner(learnerId, false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedLearner.Id, result.Id);
            Assert.Equal(expectedLearner.Name, result.Name);

            // Verify interactions
            _mockFailoverModeEvaluator.Verify(e => e.IsFailoverModeActive(), Times.Once);
            _mockFailoverLearnerDataAccess.Verify(d => d.LoadLearner(learnerId), Times.Once);
            // Ensure main data access and archived services were not used
            _mockMainLearnerDataAccess.Verify(d => d.LoadLearner(It.IsAny<int>()), Times.Never);
            _mockArchivedDataService.Verify(s => s.GetArchivedLearner(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void GetLearner_WhenNotArchivedAndLearnerResponseIsArchived_ReturnsFromArchivedService()
        {
            // Arrange
            var learnerId = 4;
            var mainOrFailoverLearner = new Learner { Id = learnerId, Name = "Partial Archived Data" };
            var learnerResponse = new LearnerResponse { Learner = mainOrFailoverLearner, IsArchived = true }; // Response says it's archived

            var expectedArchivedLearner = new Learner { Id = learnerId, Name = "Full Archived Data" };

            _mockFailoverModeEvaluator.Setup(e => e.IsFailoverModeActive()).Returns(false); // Assume not in failover mode
            _mockMainLearnerDataAccess.Setup(d => d.LoadLearner(learnerId)).Returns(learnerResponse);
            _mockArchivedDataService.Setup(s => s.GetArchivedLearner(learnerId)).Returns(expectedArchivedLearner);

            // Act
            var result = _learnerService.GetLearner(learnerId, false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedArchivedLearner.Id, result.Id);
            Assert.Equal(expectedArchivedLearner.Name, result.Name); // Should return the one from archived service

            // Verify interactions
            _mockFailoverModeEvaluator.Verify(e => e.IsFailoverModeActive(), Times.Once);
            _mockMainLearnerDataAccess.Verify(d => d.LoadLearner(learnerId), Times.Once);
            _mockArchivedDataService.Verify(s => s.GetArchivedLearner(learnerId), Times.Once); // Archived service should be called
            _mockFailoverLearnerDataAccess.Verify(d => d.LoadLearner(It.IsAny<int>()), Times.Never);
        }
    }
}