using ApiLibs.Todoist;
using DailyTodo.Helpers;
using DailyTodo.Services;
using DailyTodo.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Windows.ApplicationModel.Background;
using Windows.System.Threading;
using Windows.UI.Notifications;

namespace DailyTodo.BackgroundTasks
{
    public sealed class NotificationClickHandler : BackgroundTask
    {
        public static string Message { get; set; }

        private volatile bool _cancelRequested = false;
        private IBackgroundTaskInstance _taskInstance;
        private BackgroundTaskDeferral _deferral;

        public override void Register()
        {
            var taskName = GetType().Name;
            var taskRegistration = BackgroundTaskRegistration.AllTasks.FirstOrDefault(t => t.Value.Name == taskName).Value;

            if (taskRegistration == null)
            {
                var builder = new BackgroundTaskBuilder()
                {
                    Name = taskName
                };

                // TODO WTS: Define the trigger for your background task and set any (optional) conditions
                // More details at https://docs.microsoft.com/windows/uwp/launch-resume/create-and-register-an-inproc-background-task
                builder.SetTrigger(new ToastNotificationActionTrigger());

                builder.Register();
            }
        }

        public override Task RunAsyncInternal(IBackgroundTaskInstance taskInstance)
        {
            if (taskInstance == null)
            {
                return null;
            }

            _deferral = taskInstance.GetDeferral();

            return Task.Run(async () =>
            {
                var details = taskInstance.TriggerDetails as ToastNotificationActionTriggerDetail;
                Settings settings = await new UwpMemory().Read<Settings>("settings.json");
                var Todoist = new TodoistService(settings.TodoistKey, settings.TodoistUserAgent);

                if (details != null)
                {
                    string arguments = details.Argument;
                    var userInput = details.UserInput;

                    if (arguments.StartsWith("complete"))
                    {
                        TodoistService todoist = Todoist;
                        await todoist.MarkTodoAsDone(long.Parse(details.Argument.Remove(0, 9)));
                    }

                    if (arguments.StartsWith("remove"))
                    {
                        TodoistService todoist = Todoist;
                        ItemUpdate update = new ItemUpdate(long.Parse(details.Argument.Remove(0, 7)))
                        {
                            Labels = new List<long>()
                        };
                        await todoist.Update(update);

                    }

                    // Perform tasks
                }

                //// TODO WTS: Insert the code that should be executed in the background task here.
                //// This sample initializes a timer that counts to 100 in steps of 10.  It updates Message each time.

                //// Documentation:
                ////      * General: https://docs.microsoft.com/en-us/windows/uwp/launch-resume/support-your-app-with-background-tasks
                ////      * Debug: https://docs.microsoft.com/en-us/windows/uwp/launch-resume/debug-a-background-task
                ////      * Monitoring: https://docs.microsoft.com/windows/uwp/launch-resume/monitor-background-task-progress-and-completion

                //// To show the background progress and message on any page in the application,
                //// subscribe to the Progress and Completed events.
                //// You can do this via "BackgroundTaskService.GetBackgroundTasksRegistration"
                _deferral.Complete();
                _taskInstance = taskInstance;
            });
        }

        public override void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            _cancelRequested = true;

           // TODO WTS: Insert code to handle the cancelation request here.
           // Documentation: https://docs.microsoft.com/windows/uwp/launch-resume/handle-a-cancelled-background-task
        }

    }
}
