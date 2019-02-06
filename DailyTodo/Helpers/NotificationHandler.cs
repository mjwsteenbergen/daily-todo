using ApiLibs.Todoist;
using DailyTodo.Services;
using Microsoft.QueryStringDotNET;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace DailyTodo.Helpers
{
    class NotificationHandler
    {
        public async Task UpdateNotifications()
        {
            ToastNotificationManager.History.Clear();
            var newToasts = await CreateNotifications();
            foreach (var toast in newToasts)
            {
                ToastNotification notification = new ToastNotification(toast.GetXml());
                notification.SuppressPopup = true;
                ToastNotificationManager.CreateToastNotifier().Show(notification);
            }
        }

        public async Task<IEnumerable<ToastContent>> CreateNotifications()
        {
            TodoistService todoist = TodoistGenerator.Todoist;
            var today = await todoist.GetLabel("today");
            List<Item> todos = (await todoist.GetItems()).Where(i => i.DueDateUtc < DateTime.UtcNow | i.Labels.Contains(today.Id)).ToList();
            var proj = await todoist.GetProjects();
            return todos.Select(i => new ToastContent()
            {
                Visual = GetVisual(i, proj.First(j => j.Id == i.ProjectId)),
                Actions = GetActions(i.Id),

                // Arguments when the user taps body of toast
                Launch = new QueryString()
                {
                    { "action", "viewConversation" },
                    { "conversationId", i.Id.ToString() }

                }.ToString(),
                ActivationType = ToastActivationType.Background,
            });
        }

        private IToastActions GetActions(long todoId)
        {
            return new ToastActionsCustom()
            {
                Buttons =
                {
                    new ToastButton("Complete", "complete=" + todoId)
                    {
                        ActivationType = ToastActivationType.Background,
                    }
                }
            };
        }

        public ToastVisual GetVisual(Item todo, Project p)
        {
            return new ToastVisual()
            {
                BindingGeneric = new ToastBindingGeneric()
                {
                    Children =
                    {
                        new AdaptiveText()
                        {
                            Text = todo.Content,
                            HintMaxLines = 1
                        },

                        new AdaptiveText()
                        {
                            Text = p.Name
                        },

                        new AdaptiveText()
                        {
                            Text = todo.DateString ?? "@today"
                        }
                    }
                }
            }; 
        }


    }
}
