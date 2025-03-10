using TaskManager.Pages; 

namespace TaskManager;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new TaskDetailPage();
	}
}
