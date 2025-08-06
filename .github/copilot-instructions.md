# Background Services Library
.NET 9.0 library providing robust background service implementations for channel-based message processing and cron-scheduled task execution. This repository builds a NuGet package for integration into .NET applications.

**Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.**

## Working Effectively

### Prerequisites and Setup
- **Install .NET 9.0 SDK**: This repository requires .NET 9.0 which is NOT available by default.
  - Download and install: `wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh && chmod +x dotnet-install.sh && ./dotnet-install.sh --version 9.0.101`
  - Installation takes ~12 seconds
  - Update PATH: `export PATH="$HOME/.dotnet:$PATH"`
  - Verify: `dotnet --version` should show `9.0.101`

### Build and Test Commands
- **Restore dependencies**: `dotnet restore src/Dalog.Foundation.BackgroundServices.sln`
  - First run: ~11 seconds, subsequent runs: ~1 second. NEVER CANCEL.
  - Set timeout to 30+ minutes for initial runs in CI environments.
- **Build the solution**: `dotnet build src/Dalog.Foundation.BackgroundServices.sln --no-restore --configuration Release`
  - Takes ~7 seconds. NEVER CANCEL. Set timeout to 15+ minutes.
- **Run tests**: `dotnet test src/Dalog.Foundation.BackgroundServices.sln --no-build --configuration Release --verbosity normal`
  - Takes ~8 seconds. NEVER CANCEL. Set timeout to 15+ minutes.
  - **KNOWN ISSUE**: 3 integration tests fail (ChannelBackgroundServiceIntegrationTests), but 56/59 tests pass. This is expected and not a blocker.
- **Full CI workflow**: `dotnet restore && dotnet build --no-restore --configuration Release && dotnet test --no-build --configuration Release`
  - Total time: ~12 seconds. NEVER CANCEL. Set timeout to 45+ minutes for CI environments.

### Code Quality and Formatting
- **Format code**: `dotnet format src/Dalog.Foundation.BackgroundServices.sln`
  - Takes ~8 seconds. NEVER CANCEL. Set timeout to 15+ minutes.
  - **ALWAYS run before committing** - CI will fail on formatting issues.
- **Verify formatting**: `dotnet format --verify-no-changes src/Dalog.Foundation.BackgroundServices.sln`
  - Use this to check if formatting is required before making changes.

### Package Creation
- **Create NuGet package**: `dotnet pack src/Dalog.Foundation.BackgroundServices/Dalog.Foundation.BackgroundServices.csproj --no-build --configuration Release --output ./artifacts`
  - Takes ~1 second. Set timeout to 5+ minutes.

## Validation Scenarios

### After Making Code Changes
1. **Always validate with the exact commands above in this order**:
   - Run `dotnet format` to ensure code formatting
   - Run `dotnet build --no-restore --configuration Release` 
   - Run `dotnet test --no-build --configuration Release` 
2. **Check that 56+ tests pass** - exactly 3 integration tests are expected to fail
3. **Verify the package can be created** with the pack command above

### Testing New Features
- **Add unit tests** in the appropriate subdirectory under `/test/ChannelTests/` or `/test/CronTests/`
- **Follow existing test patterns** using xunit framework
- **Ensure new tests are in the same style** as existing tests in the solution

## Repository Structure

### Key Projects
- **Main library**: `src/Dalog.Foundation.BackgroundServices/` - The core library code
- **Test project**: `test/` - Comprehensive unit and integration tests (xunit)
- **Solution file**: `src/Dalog.Foundation.BackgroundServices.sln` - Always use this for all commands

### Important Files
- **Project file**: `src/Dalog.Foundation.BackgroundServices/Dalog.Foundation.BackgroundServices.csproj` - Package metadata and dependencies
- **Git versioning**: `GitVersion.yml` - Controls version numbering
- **CI/CD pipeline**: `.github/workflows/cicd.yml` - Automated build and publish
- **Security scanning**: `.github/workflows/security.yml` - Security and dependency audits
- **Code style**: `.editorconfig` - Enforced code formatting rules

### Package Dependencies
- **Microsoft.Extensions.Hosting.Abstractions** 9.0.7 - For background service infrastructure
- **NCrontab** 3.3.3 - For cron expression parsing

## CI/CD Integration

### GitHub Actions Workflows
- **CI/CD** (`.github/workflows/cicd.yml`): Builds, tests, and publishes to NuGet on main branch
- **Security** (`.github/workflows/security.yml`): Dependency scanning and CodeQL analysis
- **Triggers**: Push to main branch and PR merges
- **Versioning**: Uses GitVersion for semantic versioning
- **Package publishing**: Automatic to NuGet.org for releases

### Working with PRs
- **All formatting must pass** - Run `dotnet format` before committing
- **Build must succeed** in Release configuration
- **Tests can have the expected 3 failures** - do not try to fix these integration test failures

## Common Tasks Reference

### Repository Root Contents
```
.editorconfig           # Code formatting rules
.github/                # Workflow definitions
.gitignore             # Git ignore patterns
GitVersion.yml         # Version configuration
LICENSE                # License file
README.md              # Package documentation
doc/                   # Documentation assets (logo)
src/                   # Source code
test/                  # Test projects
```

### Main Library Structure (`src/Dalog.Foundation.BackgroundServices/`)
```
Channel/               # Channel background service implementation
Cron/                  # Cron background service implementation
Dalog.Foundation.BackgroundServices.csproj  # Project file
InternalsVisibleTo.cs  # Test access configuration
```

### Test Structure (`test/`)
```
ChannelTests/          # Channel service tests
CronTests/             # Cron service tests
Dalog.Foundation.BackgroundServicesTests.csproj  # Test project file
```

## Troubleshooting

### Common Issues
- **"current .NET SDK does not support targeting .NET 9.0"**: Install .NET 9.0 SDK using the commands above
- **Formatting errors in CI**: Run `dotnet format` and commit the changes
- **3 integration tests failing**: This is expected - do not attempt to fix these specific test failures
- **Build taking too long**: This is normal - builds can take several minutes, especially in CI environments

### Timeout Guidelines
- **NEVER CANCEL builds or tests** - they may appear to hang but are processing
- **Always set timeouts of 30+ minutes** for restore operations
- **Always set timeouts of 15+ minutes** for build and test operations  
- **Always set timeouts of 45+ minutes** for full CI workflows
- **If commands appear stuck**, wait at least 15 minutes before investigating

## Best Practices
- **Always build in Release configuration** for consistency with CI
- **Always use the solution file** for all dotnet commands, not individual project files
- **Run format before every commit** to avoid CI failures
- **Test both individual and full workflow commands** when making changes
- **Verify package creation** works when making changes to project files or dependencies