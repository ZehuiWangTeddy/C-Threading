using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using TaskManager.Repositories;
using TaskManager.Services;
using TaskManager.ViewModels;

namespace TaskManager;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler<RadioButton, RadioButtonHandler>();
            })
            .Services.AddSingleton<ITaskRepository>(provider =>
                new TaskRepository(Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "TaskManager.db")));

        RegisterViewModels(builder);
#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static void RegisterViewModels(MauiAppBuilder builder)
    {
        builder.Services.AddTransient<AddTaskViewModel>();

        builder.Services.AddSingleton<TaskListViewModel>();

        builder.Services.AddSingleton<TaskUpdateService>();

        builder.Services.AddSingleton<TaskPollingService>();
    }
}