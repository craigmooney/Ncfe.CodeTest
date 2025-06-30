using Xunit;
using Moq;
using Ncfe.CodeTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;

namespace Ncfe.CodeTest.Tests
{
    public class FailoverModeEvaluatorTests
    {
        private readonly Mock<IFailoverRepository> _mockFailoverRepository;
        private readonly FailoverModeEvaluator _failoverModeEvaluator;

        public FailoverModeEvaluatorTests()
        {
            _mockFailoverRepository = new Mock<IFailoverRepository>();
            _failoverModeEvaluator = new FailoverModeEvaluator(_mockFailoverRepository.Object);
        }

        // Helper method to temporarily set AppSetting for these tests
        private IDisposable SetAppSetting(string key, string value)
        {
            var originalValue = ConfigurationManager.AppSettings[key];
            ConfigurationManager.AppSettings.Set(key, value);

            return new DisposableAction(() => ConfigurationManager.AppSettings.Set(key, originalValue));
        }

        [Fact]
        public void IsFailoverModeActive_WhenFailedRequestsBelowThreshold_ReturnsFalse()
        {
            // Arrange
            var entries = new List<FailoverEntry>
            {
                new FailoverEntry { DateTime = DateTime.Now.AddMinutes(-5) }, // 1 failed request within 10 min window
                new FailoverEntry { DateTime = DateTime.Now.AddMinutes(-15) } // Outside window
            };
            _mockFailoverRepository.Setup(r => r.GetFailOverEntries()).Returns(entries);

            using (SetAppSetting("IsFailoverModeEnabled", "true")) // Ensure failover is enabled in config
            {
                // Act
                var result = _failoverModeEvaluator.IsFailoverModeActive();

                // Assert
                Assert.False(result);
                _mockFailoverRepository.Verify(r => r.GetFailOverEntries(), Times.Once);
            }
        }

        [Fact]
        public void IsFailoverModeActive_WhenFailedRequestsAboveThresholdAndConfigTrue_ReturnsTrue()
        {
            // Arrange
            var entries = new List<FailoverEntry>();
            // Add 101 entries within the last 10 minutes to exceed the threshold of 100
            for (int i = 0; i < 101; i++)
            {
                entries.Add(new FailoverEntry { DateTime = DateTime.Now.AddMinutes(-1) });
            }
            _mockFailoverRepository.Setup(r => r.GetFailOverEntries()).Returns(entries);

            using (SetAppSetting("IsFailoverModeEnabled", "true")) // Config is true
            {
                // Act
                var result = _failoverModeEvaluator.IsFailoverModeActive();

                // Assert
                Assert.True(result);
                _mockFailoverRepository.Verify(r => r.GetFailOverEntries(), Times.Once);
            }
        }

        [Fact]
        public void IsFailoverModeActive_WhenFailedRequestsAboveThresholdAndConfigFalse_ReturnsFalse()
        {
            // Arrange
            var entries = new List<FailoverEntry>();
            for (int i = 0; i < 101; i++)
            {
                entries.Add(new FailoverEntry { DateTime = DateTime.Now.AddMinutes(-1) });
            }
            _mockFailoverRepository.Setup(r => r.GetFailOverEntries()).Returns(entries);

            using (SetAppSetting("IsFailoverModeEnabled", "false")) // Config is false
            {
                // Act
                var result = _failoverModeEvaluator.IsFailoverModeActive();

                // Assert
                Assert.False(result);
                _mockFailoverRepository.Verify(r => r.GetFailOverEntries(), Times.Once);
            }
        }

        [Fact]
        public void IsFailoverModeActive_WhenNoEntries_ReturnsFalse()
        {
            // Arrange
            _mockFailoverRepository.Setup(r => r.GetFailOverEntries()).Returns(new List<FailoverEntry>());

            using (SetAppSetting("IsFailoverModeEnabled", "true"))
            {
                // Act
                var result = _failoverModeEvaluator.IsFailoverModeActive();

                // Assert
                Assert.False(result);
                _mockFailoverRepository.Verify(r => r.GetFailOverEntries(), Times.Once);
            }
        }

        // Helper class to manage disposable actions for AppSettings cleanup
        private class DisposableAction : IDisposable
        {
            private readonly Action _action;
            public DisposableAction(Action action) => _action = action;
            public void Dispose() => _action?.Invoke();
        }
    }
}