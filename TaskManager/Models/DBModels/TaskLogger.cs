using SQLite;
using Newtonsoft.Json;  
using SQLiteNetExtensions.Attributes;

namespace TaskManager.Models.DBModels
{
    public class TaskLogger
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string LogMessages { get; set; } = "[]";

        [ForeignKey(typeof(BaseTask))]
        public int TaskId { get; set; }

        [OneToOne(CascadeOperations = CascadeOperation.All)]
        public BaseTask Task { get; set; } 

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