# C-Threading

## Clean TaskManager.csproj file
<TargetFrameworks>net8.0-android;net8.0-ios;net8.0-maccatalyst</TargetFrameworks>
To
<TargetFrameworks>net8.0-maccatalyst</TargetFrameworks>

## Install maui
dotnet workload install maui

## Install Xcode plugin(For MacBook)
sudo xcode-select --switch /Applications/Xcode.app
sudo xcodebuild -runFirstLaunch
xcode-select --install

## Run project
For MacOS:
dotnet restore
dotnet build --framework net8.0-maccatalyst
dotnet run --framework net8.0-maccatalyst

For Windows:
dotnet restore
dotnet build --framework net8.0-windows10.0.19041.0
dotnet run --framework net8.0-windows10.0.19041.0
