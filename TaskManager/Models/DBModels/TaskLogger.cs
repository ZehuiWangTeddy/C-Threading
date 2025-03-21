using System.Collections.Generic;
using SQLite;
using Newtonsoft.Json;  

namespace TaskManager.Models.DBModels
{
    public class TaskLogger
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string LogMessages { get; set; } = "[]";

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