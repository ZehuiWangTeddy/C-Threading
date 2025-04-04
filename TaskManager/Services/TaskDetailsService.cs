using TaskManager.Models.DBModels;
using TaskManager.Repositories;
    
namespace TaskManager.Services
    {
        public class TaskDetailsService
        {
            private readonly ITaskRepository _taskRepository;
    
            public TaskDetailsService(ITaskRepository taskRepository)
            {
                _taskRepository = taskRepository;
            }
    
            public BaseTask? GetTaskDetails(int taskId)
            {
                return _taskRepository.GetTaskById(taskId);
            }
        }
    }