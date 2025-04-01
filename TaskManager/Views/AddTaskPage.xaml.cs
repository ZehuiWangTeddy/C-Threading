using Microsoft.Maui.Controls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using TaskManager.Models;
using TaskManager.Models.DBModels;
using TaskManager.Models.Enums;
using TaskManager.Repositories;
using TaskManager.Services;
using TaskManager.Extensions;
using TaskManager.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using TaskManager.Messages;

namespace TaskManager.Views
{
    public partial class AddTaskPage : ContentPage, INotifyPropertyChanged
    {


        public AddTaskPage()
        {
            InitializeComponent();
            // _taskRepository = taskRepository;
            BindingContext = IPlatformApplication.Current?.Services.GetRequiredService<AddTaskViewModel>();

            RegisterMessager();
        }

        private void RegisterMessager()
        {
            WeakReferenceMessenger.Default.Register<ToastMessage>(this, async (r, m) =>
            {
                if (m.Master == nameof(AddTaskViewModel) && m.Slave == nameof(AddTaskPage))
                {
                    await DisplayAlert("Error", $"{m.MsgContent}", "OK");
                }
            });

            WeakReferenceMessenger.Default.Register<NavigationMessage>(this, async (r, m) =>
            {
                if (m.Master == nameof(AddTaskViewModel) && m.Slave == nameof(AddTaskPage))
                {
                    if (m.IsBack)
                    {
                        await Navigation.PopModalAsync();
                      
                        WeakReferenceMessenger.Default.Send(new CallEventMessage(nameof(AddTaskPage), nameof(TaskListViewModel), CallEventMessage.CallbackEvent.Flush,true));
                    }

                }
            });
        }

        private void OnCancelClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }
    }
}