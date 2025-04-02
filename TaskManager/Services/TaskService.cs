using TaskManager.Models;
using TaskManager.Models.DBModels;
using TaskManager.Models.Enums;
using TaskManager.Repositories;

namespace TaskManager.Services
{
    public class TaskService
    {
        private readonly ThreadPoolManager _threadPoolManager;
        private readonly ITaskRepository _taskRepository; 

        public TaskService(ThreadPoolManager threadPoolManager, ITaskRepository taskRepository)
        {
            _threadPoolManager = threadPoolManager;
            _taskRepository = taskRepository;
        }

        public void AddTask(BaseTask task)
        {
            _taskRepository.SaveTask(task); 
            _threadPoolManager.AddTaskToQueue(task); 
            Console.WriteLine($"Task added to DB and sent to queue: {task.Name}");
        }

        public List<BaseTask> GetTasks()
        {
            return _taskRepository.GetTasks(StatusType.Pending); 
        }

        public void UpdateTaskStatus(int taskId, StatusType status)
        {
            _taskRepository.UpdateTaskStatus(taskId, status); 
        }
        
        public int GetCompletedTasks()
        {
            return _taskRepository.GetTasks(StatusType.Completed).Count;
        }

        public int GetFailedTasks()
        {
            return _taskRepository.GetTasks(StatusType.Failed).Count;
        }
    }
}