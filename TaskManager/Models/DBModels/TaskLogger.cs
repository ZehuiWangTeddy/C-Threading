using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using SQLite;

namespace TaskManager.Models.DBModels;

public class TaskLogger : INotifyPropertyChanged
{
    private string _logMessages = "[]";
    [PrimaryKey] [AutoIncrement] public int Id { get; set; }

    public string LogMessages
    {
        get => _logMessages;
        set
        {
            if (_logMessages != value)
            {
                _logMessages = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void AddLogMessage(string message)
    {
        var logs = GetLogMessages();
        logs.Add(message);
        LogMessages = JsonConvert.SerializeObject(logs);
    }

    public List<string> GetLogMessages()
    {
        Debug.WriteLine($"LogMessages: {LogMessages}");
        return JsonConvert.DeserializeObject<List<string>>(LogMessages) ?? new List<string>();
    }
}