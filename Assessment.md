# Technical Assessment: Senior Backend Developer (C#/.NET)
### Project: The Credit Enforcement Engine

**Role:** Senior Backend Developer  
**Tech Stack:** C# (.NET 9), ASP.NET Core, xUnit  
**Focus:** Concurrency, Performance Optimization, Middleware Architecture, System Design

---

## 1. Scenario & Context
You are a Senior Backend Engineer at Riva. We are pivoting to a "Pre-Paid Credit" model for our API to better monetize our services.

**The Problem:** We currently rely on a legacy database to track customer credits. It is reliable but slow (~100ms latency per read/write). We need a **high-performance Middleware layer** that sits in front of our API to enforce billing limits in real-time.

**The "Ask" from Leadership:** > "Build a robust enforcement engine that deducts 1 credit per request. If a user hits 0 credits, block them immediately with a `402 Payment Required`. This system must handle thousands of concurrent requests without race conditions (double-spending) and must add less than **20ms** of latency to the pipeline."

---

## 2. Technical Requirements

### A. The Middleware (The Gatekeeper)
* Create an ASP.NET Core Middleware component (`CreditEnforcementMiddleware`).
* It must intercept every HTTP request.
* If the user has credits: Deduct 1 and allow the request to proceed.
* If the user has **0** credits: Short-circuit the pipeline and return `402 Payment Required`.

### B. The Concurrency Challenge (Race Conditions)
* **Scenario:** A user has **1 Credit** remaining. They fire **10 parallel requests** simultaneously.
* **Requirement:** Only **ONE** request must succeed. The other 9 must fail.
* **Constraint:** You cannot rely solely on the database for this check due to the latency requirement. You must implement a strategy to handle this "Double-Spend" problem efficiently (e.g., locking, atomic operations, or optimistic concurrency).

### C. Performance (The 20ms Limit)
* The provided `LegacyBillingRepository` has a simulated latency of **100ms**.
* You must call this repository as minimally as possible to satisfy the speed requirement while ensuring eventual consistency.

### D. Testing (Critical - You Must Implement Tests)
* **You are required to implement comprehensive unit tests** to verify your logic.
* **Test Case 1:** You must implement a test `TryDeductCredit_ReturnsFalse_WhenUserHasNoCredits` - Verify that the service returns `false` when a user has 0 credits.
* **Test Case 2 (The Stress Test):** You must implement a test `ConcurrentDeductions_WithOneCredit_OnlyOneSucceeds` - Verify that 10 concurrent requests for a user with 1 credit results in exactly **1 Success** and **9 Failures**.
* Test class structures with basic setup are provided in `RivaAssessment.Tests/`, but **you must implement all test methods yourself**.

---

## 3. Starter Code & Setup Instructions

### Project Structure
The solution is already set up with:
- **RivaAssessment**: Main API project (.NET 9)
- **RivaAssessment.Tests**: Test project with xUnit (.NET 9)
- **LegacyBillingRepository**: Provided repository that simulates slow database (100ms latency)
  - **IMPORTANT:** The code in `ILegacyBillingRepository` and `LegacyBillingRepository` should NOT be adjusted. These classes are provided as-is.
  - Available methods: `GetCreditsAsync`, `DeductCreditsAsync`, `SetCreditsAsync`
- **CreditService**: Service class that needs implementation
- **CreditEnforcementMiddleware**: Middleware class that needs implementation
- **Test files**: Test class structures provided - **you must implement all test methods**

### Initial Setup
See [SETUP.md](SETUP.md) for detailed setup instructions. Quick start:

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests (you must implement tests first - no tests will run until you write them)
dotnet test
```

## 4. What You Need to Implement

### CreditService (`RivaAssessment/Services/CreditService.cs`)
- **Status:** Currently throws `NotImplementedException`
- **Task:** Implement `TryDeductCreditAsync(string userId)` method
- **Requirements:**
  - Implement caching to avoid 100ms database calls
  - Add concurrency control (locking/atomic operations)
  - Ensure < 20ms latency
  - Return `true` if credit deducted, `false` if insufficient credits

### CreditEnforcementMiddleware (`RivaAssessment/Middleware/CreditEnforcementMiddleware.cs`)
- **Status:** Needs implementation
- **Task:** Implement the middleware from scratch
- **Requirements:**
  - Extract user ID from `X-User-Id` header
  - Call `CreditService.TryDeductCreditAsync()`
  - Return `402 Payment Required` if no credits
  - Allow request to proceed if credit deducted
  - Register in `Program.cs`

### Tests (`RivaAssessment.Tests/`)
- **Status:** Test class structures with basic setup provided - **you must implement all test methods**
- **Task:** **Implement comprehensive unit tests** for all components
- **Requirements:**
  - **You must implement** `TryDeductCredit_ReturnsFalse_WhenUserHasNoCredits` test for CreditService
  - **You must implement** `ConcurrentDeductions_WithOneCredit_OnlyOneSucceeds` test for CreditService
  - **You must implement** tests for CreditEnforcementMiddleware (e.g., blocking users with no credits, allowing requests with credits, concurrent request handling)
  - **You must ensure** all tests pass
  - You are free to organize and structure tests as you see fit, but tests are mandatory

### CreditRefillService (`RivaAssessment/Services/CreditRefillService.cs`)
- **Status:** Currently throws `NotImplementedException`
- **Task:** Implement `RefillCreditsForAllUsersAsync()` method and a background service/scheduled job
- **Requirements:**
  - Implement a background service or scheduled job that calls `RefillCreditsForAllUsersAsync()` regularly (e.g., every hour)
  - Ensure all users have at least 5 credits every hour (refill to 5 credits if they have fewer)
  - When credits are refilled, update the cache in `CreditService` to reflect the new balance
  - Handle concurrent refill operations safely
  - Consider implementing a maximum credit limit
  - Ensure the refill operation is atomic and thread-safe

**Integration Points:**
- Must work with the existing `CreditService` caching mechanism
- Should update the in-memory cache when credits are added
- May need to invalidate or refresh cache entries

**Implementation Notes:**
- You are free to organize files and structure as you see fit
- Consider how this integrates with the existing `CreditService` and `ICreditService`
- **You must implement tests** for the refill functionality

### AuthenticationService (`RivaAssessment/Services/AuthenticationService.cs`)
- **Status:** Currently throws `NotImplementedException`
- **Task:** Implement `AuthenticateAsync(string userId)` method
- **Requirements:**
  - Track user activity timestamps
  - Implement a configurable timeout period (e.g., 30 minutes)
  - When a user's session times out, require re-authentication (throw exception or handle appropriately)
  - Consider implementing a sliding expiration (reset timeout on activity)
  - Ensure session validation doesn't impact the 20ms latency requirement
  - Method signature: `Task AuthenticateAsync(string userId)` (returns `Task`, not `Task<bool>`)

**Integration Points:**
- Middleware should check both credit balance and session validity
- Session timeout should not affect credit deduction logic
- May need to coordinate with authentication/authorization system

**Implementation Notes:**
- You are free to organize files and structure as you see fit
- Consider whether to integrate session checking into existing middleware or create separate middleware
- Ensure session validation doesn't impact the 20ms latency requirement
- **You must implement tests** for the authentication timeout functionality

---

---

## 6. Success Criteria

Your implementation is complete when:
1. **All tests are implemented and passing** (`dotnet test` shows all tests passing)
2. `CreditService.TryDeductCreditAsync()` is fully implemented
3. Middleware is registered and working
4. API blocks users with 0 credits (returns 402)
5. Concurrency test passes: 10 requests with 1 credit = 1 success, 9 failures
6. Response time is < 20ms (caching prevents 100ms database calls)
7. Credit refill system is implemented **with tests** (ensures users have at least 5 credits every hour)
8. Authentication timeout is implemented and integrated **with tests**

## 7. Testing

**Important:** Testing is a mandatory part of this assessment. You must implement unit tests for all components.

### Run Tests
```bash
dotnet test
```

**Note:** You must implement tests before running `dotnet test` - no tests will execute until you write them.

### Test the API
1. Start the API: `cd RivaAssessment && dotnet run`
2. Test with `X-User-Id` header using curl (replace `/api/your-endpoint` with an actual endpoint):
   ```bash
   # Test with a user who has credits
   curl -H "X-User-Id: user1" http://localhost:5000/api/your-endpoint
   
   # Test with a user who has no credits (should return 402)
   curl -H "X-User-Id: user3" http://localhost:5000/api/your-endpoint
   ```

### Test Users
- `user1`: 10 credits
- `user2`: 5 credits
- `user3`: 0 credits (should always return 402)
- `user4`: 100 credits

## 8. Implementation Approach

### Core Requirements (Must Implement)
You must implement the following core components:
- **CreditService**: Implement `TryDeductCreditAsync()` method in `RivaAssessment/Services/CreditService.cs`
- **CreditEnforcementMiddleware**: Implement the middleware in `RivaAssessment/Middleware/CreditEnforcementMiddleware.cs`
- **Tests**: **You must implement** unit tests in `RivaAssessment.Tests/CreditServiceTests.cs` and `RivaAssessment.Tests/CreditEnforcementMiddlewareTests.cs`
- **Program.cs**: Register services and middleware

### Interconnected Components (Must Implement)
You must also implement:
- **CreditRefillService**: Implement `RefillCreditsForAllUsersAsync()` method in `RivaAssessment/Services/CreditRefillService.cs` and a background service/scheduled job to ensure all users have at least 5 credits every hour
- **AuthenticationService**: Implement `AuthenticateAsync(string userId)` method in `RivaAssessment/Services/AuthenticationService.cs` to handle authentication timeout mechanism

### File Organization
**You are free to organize your code structure as you see fit.** This is part of the assessment - we want to see how you structure a solution. Consider:
- Separation of concerns (controllers, services, repositories, models)
- Test organization
- Dependency injection setup
- Code organization patterns that make sense for your implementation

**Important Notes:**
- **You are free to add helper methods, helper classes, and modify existing code** where you feel appropriate to implement the requirements.
- **Exception:** The code in `ILegacyBillingRepository` and `LegacyBillingRepository` should NOT be adjusted. These classes are provided as-is and must be used as specified.
- The only files you must modify are the existing starter files mentioned in the core requirements. For all new functionality, create files and organize them in a way that demonstrates good software architecture practices.
