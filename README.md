# C-Threading

## Prerequisites
### Install .NET8 SDK
https://dotnet.microsoft.com/en-us/download/dotnet/8.0
### Install .NET MAUI
Run the following command to install .NET MAUI:
```sh
dotnet workload install maui
```
### Install Windows App SDK
If you are using a Windows, install the necessary Windows App SDK components:
https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/downloads

### Install Xcode Components (For macOS)
If you are using a MacBook, install the necessary Xcode components:
```sh
sudo xcode-select --switch /Applications/Xcode.app
sudo xcodebuild -runFirstLaunch
xcode-select --install
```
### Required Application
#### For project code (You can choose one of them)
1. Visual Studio(Only for Windows): https://visualstudio.microsoft.com/
2. Rider (project team are using this): https://www.jetbrains.com/rider/
#### For project database  (You can choose one of them)
If you want to visualize the structure and content of your database, install:
1. DB Browser(more lightweight): https://sqlitebrowser.org/
2. DataGrip(if you want to use JetBrains for everything): https://www.jetbrains.com/datagrip/download/

## Project Configuration

### Update `TaskManager.csproj`
Modify the target frameworks in the `.csproj` file:
```xml
<TargetFrameworks>net8.0-maccatalyst</TargetFrameworks>
```
If you get an error said "registered application failed", please modify the ApplicationId in the `.csproj` file:
```xml
<ApplicationId>com.companyname.taskmanager.temp</ApplicationId>
```
Then try to build project again.

## Running the Project

### macOS
You can use following commands to run project: 
```shell
dotnet restore
dotnet build
```
then click "Run" Button in the right-top corner on the Rider.
If Rider wants you to give a exact target build framework, please run:
```sh
dotnet restore
dotnet build --framework net8.0-maccatalyst
dotnet run --framework net8.0-maccatalyst
```

### Windows
You can use following commands to run project:
```shell
dotnet restore
dotnet build
```
then click "Run" Button in the right-top corner on the Rider.
If Rider wants you to give a exact target build framework, please run:
```sh
dotnet restore
dotnet build --framework net8.0-windows10.0.19041.0
dotnet run --framework net8.0-windows10.0.19041.0
```
This section is an example as using Rider, if you are using Visual Studio, then you may need extra step, so we kindly suggest you to keep same application choice with project team together.

## Tips
1. For mac0S Users: After downloading the ZIP file, move the extracted project folder out of the Downloads directory (e.g., to Documents or Desktop) before running it. macOS sandboxing restrictions may prevent the app from working correctly if it's run directly from the Downloads folder.
2. This project is only for Mac or Windows, it is not suitable for android or iOS, so when you build this app, please don't select mobile phones as target device.
3. This project required .NET8.0 SDK, .NET MAUI workload, Windows APP SDK(for Windows device user), Xcode component(for MAC device user), please make sure you have installed everything before running the application.
3. You can use Visual Studio or Rider (Recommended) to open this project, and use DB Browser or DAtaGrip to open database. From project team side, we are using Rider and DB Browser.
4. You can double-click ".sln" file (located in the project root directory) to open project, also can drag whole folder to Rider. You should be able to run this project if you already set up environment. But if you still cannot run this project successfully, please contact project team via email.
5. To avoid any loading exceptions, we recommend that you run this project locally instead of in a git environment.
