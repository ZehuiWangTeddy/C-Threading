using TaskManager.Views;

namespace TaskManager;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new TaskListPage();
	}
}
