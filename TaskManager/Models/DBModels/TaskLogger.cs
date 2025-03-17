using SQLite;

namespace TaskManager.Models.DBModels
{
    public class TaskLogger
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string LogMessage { get; set; } 
        
        public int TaskId { get; set; } 
    }
}