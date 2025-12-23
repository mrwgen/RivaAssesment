# Setup Guide

## Prerequisites

Install .NET SDK 9.0 or later:

**macOS (Homebrew):**
```bash
brew install dotnet
```

**Or download from:** https://dotnet.microsoft.com/download

Verify installation:
```bash
dotnet --version
```

## Initial Setup

1. **Navigate to the project directory:**
   ```bash
   cd <your-project-directory>
   ```
   (Replace `<your-project-directory>` with the path where you cloned or extracted the project)

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Build the solution:**
   ```bash
   dotnet build
   ```

## Running the Backend

Start the API server:

```bash
cd RivaAssessment
dotnet run
```

You should see:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
      Now listening on: https://localhost:5001
```

The API is now running! 🚀

## Testing the API

The API requires the `X-User-Id` header to identify which user is making the request. Since Swagger UI doesn't support adding custom headers by default, we recommend using `curl` or browser developer tools for testing.

### Testing with curl (Recommended)

The easiest way to test the API is using `curl` from the command line. You'll need to create your own API endpoints to test. Once you have endpoints set up, test them like this:

```bash
# Test with a user who has credits (should return 200 OK)
curl -H "X-User-Id: user1" http://localhost:5000/api/your-endpoint

# Test with a user who has no credits (should return 402 Payment Required)
curl -H "X-User-Id: user3" http://localhost:5000/api/your-endpoint

# Test with another user who has credits
curl -H "X-User-Id: user2" http://localhost:5000/api/your-endpoint
```

### Testing with Browser Developer Tools

You can also test using your browser's developer tools:

1. Open your browser's Developer Tools (F12 or right-click → Inspect)
2. Go to the **Network** tab
3. Navigate to http://localhost:5000/swagger to view the API documentation
4. To make a request with headers, use the **Console** tab and run:
   ```javascript
   fetch('http://localhost:5000/api/your-endpoint', {
     headers: { 'X-User-Id': 'user1' }
   }).then(r => r.json()).then(console.log)
   ```

### Viewing API Documentation with Swagger

You can view the API documentation (but not test with custom headers) at:

- **http://localhost:5000/swagger** ✅

The Swagger UI shows the available endpoints and their schemas, but you'll need to use `curl` or browser developer tools to add the `X-User-Id` header.

### Test Users

The API includes these test users:
- `user1`: 10 credits
- `user2`: 5 credits  
- `user3`: 0 credits (will always return 402 Payment Required)
- `user4`: 100 credits

## Troubleshooting

### Port Already in Use
If port 5000 is already in use:
```bash
lsof -ti:5000 | xargs kill
```

### Swagger Not Loading
- Make sure the API is running (`dotnet run`)
- Try http://localhost:5000/swagger (not https)
- Check the console output for any errors

### Build Errors
- Make sure you've run `dotnet restore` first
- Verify .NET SDK is installed: `dotnet --version`

## Stopping the Server

Press `Ctrl+C` in the terminal where the API is running.

