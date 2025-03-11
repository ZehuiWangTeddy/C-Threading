# C-Threading

## Prerequisites
### Install .NET8 SDK
https://dotnet.microsoft.com/en-us/download/dotnet/8.0
### Install .NET MAUI
Run the following command to install .NET MAUI:
```sh
dotnet workload install maui
```

### Install Xcode (For macOS)
If you are using a MacBook, install the necessary Xcode components:
```sh
sudo xcode-select --switch /Applications/Xcode.app
sudo xcodebuild -runFirstLaunch
xcode-select --install
```

## Project Configuration

### Update `TaskManager.csproj`
Modify the target frameworks in the `.csproj` file:
```xml
<TargetFrameworks>net8.0-maccatalyst</TargetFrameworks>
```

## Running the Project

### macOS
```sh
dotnet restore
dotnet build --framework net8.0-maccatalyst
dotnet run --framework net8.0-maccatalyst
```

### Windows
```sh
dotnet restore
dotnet build --framework net8.0-windows10.0.19041.0
dotnet run --framework net8.0-windows10.0.19041.0
```

