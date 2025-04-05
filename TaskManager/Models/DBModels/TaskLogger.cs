using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SQLite;
using Newtonsoft.Json;  

namespace TaskManager.Models.DBModels
{
    public class TaskLogger : INotifyPropertyChanged
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        private string _logMessages = "[]";
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
            return JsonConvert.DeserializeObject<List<string>>(LogMessages) ?? new List<string>();
        }
    }
}