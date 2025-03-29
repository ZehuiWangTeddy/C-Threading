using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using TaskManager.Repositories;

namespace TaskManager;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
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
					"TaskManager.db")));;;

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
