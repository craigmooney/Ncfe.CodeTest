# NCFE Code Test: Learner Service Refactoring

## Project Overview

This project is a solution to the NCFE Code Test, focusing on refactoring the `GetLearner` method within the `LearnerService` class. The primary goal was to apply SOLID principles, enhance maintainability, and improve testability.

The `GetLearner` method's original logic involved retrieving learner data based on an `isLearnerArchived` parameter, managing a failover mechanism for a third-party data store, and evaluating failover mode based on failed request thresholds.

## Technologies Used

* **C#**
* **.NET Framework**
* **Visual Studio 2022 Community Edition**
* **Moq** (for mocking dependencies in unit tests)
* **xUnit** (for unit testing framework)

## How to Run the Project

1.  **Clone the repository:**
    ```bash
    git clone [https://github.com/craigmooney/Ncfe.CodeTest.git](https://github.com/craigmooney/Ncfe.CodeTest.git)
    ```
2.  **Open in Visual Studio:**
    * Navigate to the cloned directory.
    * Open the `Ncfe.CodeTest.sln` solution file in Visual Studio 2022.
3.  **Restore NuGet Packages:** Visual Studio should automatically restore the necessary NuGet packages (Moq, xUnit) upon opening the solution. If not, right-click on the solution in Solution Explorer and select "Restore NuGet Packages".
4.  **Build the Solution:** Build the solution (`Ctrl+Shift+B` or `Build > Build Solution`) to compile the code.
5.  **Run Unit Tests:**
    * Go to `Test` > `Test Explorer` in Visual Studio.
    * Click "Run All Tests" (or select specific tests) to execute the unit tests located in the `Ncfe.CodeTest.UnitTests` project.

## Key Design Decisions & Approach

During the refactoring process, the following key decisions and principles were applied:

* **Single Responsibility Principle (SRP):** The `GetLearner` method was broken down into smaller, more focused units. For example, `FailoverModeEvaluator` was introduced to encapsulate the logic for determining failover mode, separating it from the core learner retrieval.
* **Dependency Inversion Principle (DIP):** Dependencies such as `ArchivedDataService`, `FailoverModeEvaluator`, `LearnerDataAccess`, and `FailoverLearnerDataAccess` were abstracted behind interfaces (e.g., `IArchivedDataService`, `IFailoverModeEvaluator`, `ILearnerDataAccess`). These interfaces are then injected into `LearnerService`'s constructor, promoting loose coupling and testability.
* **Testability:** The use of interfaces and dependency injection allowed for comprehensive unit testing using Moq and xUnit, enabling isolated testing of `LearnerService`'s logic without relying on concrete data access implementations.
* **Readability:** Code was refactored to improve clarity, including the use of guard clauses and extracting complex logic into dedicated methods or properties (e.g., `IsFailoverEnabledInConfig` within `FailoverModeEvaluator`).
* **Defensive Programming:** Input validation for `learnerId` was added to ensure the method operates with valid parameters and fails fast on incorrect input. Null checks were also considered for responses from data access layers.

## Further Improvements & Considerations

While the refactoring significantly improves the solution, the following areas could be further enhanced in a production environment:

* **Full Dependency Injection:** Currently, some dependencies like `FailoverRepository`, `LearnerDataAccess`, and `ArchivedDataService` are still directly instantiated within `LearnerService` or `FailoverModeEvaluator`. For full adherence to DIP and maximum flexibility, these should also be injected via constructors (as interfaces) wherever they are consumed.
* **Configuration Abstraction:** The `ConfigurationManager.AppSettings` is still directly accessed. In a real application, this static dependency should be abstracted behind an `IConfiguration` interface and injected, making configuration retrieval mockable and testable without modifying global state.
* **Magic Numbers/Strings:** Hardcoded values (e.g., failover threshold numbers, time durations) could be externalized into configurable settings or well-named constants.
* **Error Handling:** The dummy data access layers currently return default objects. In a production system, these would interact with real databases/APIs and require robust error handling (e.g., retries, circuit breakers, specific exception types for "not found" scenarios).
* **Static Methods:** The `FailoverLearnerDataAccess.GetLearnerById` method is static. For better mockability and adherence to object-oriented principles, it would typically be an instance method exposed via an interface.

---
