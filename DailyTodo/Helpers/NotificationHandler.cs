using ApiLibs.Todoist;
using DailyTodo.Services;
using Microsoft.QueryStringDotNET;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace DailyTodo.Helpers
{
    class NotificationHandler
    {
        public async Task UpdateNotifications(TodoistService todoist)
        {
            var oldToasts = ToastNotificationManager.History.GetHistory();
            var oldToastsIds = oldToasts.Select(i => i.Tag).ToList();

            var newToasts = await CreateNotifications(todoist);
            var newToastIds = newToasts.Select(i => i.Tag).ToList();


            foreach (var toast in newToasts)
            {
                if (!oldToastsIds.Contains(toast.Tag))
                {
                    ToastNotificationManager.CreateToastNotifier().Show(toast);
                }
            }




            foreach (var toast in oldToasts)
            {
                if (!newToastIds.Contains(toast.Tag))
                {
                    ToastNotificationManager.History.Remove(toast.Tag);
                }
            }
        }

        public static string DisplayContent(string input)
        {
            var match = Regex.Match(input, "https?:\\/\\/(www\\.)?[-a-zA-Z0-9@:%._\\+~#=]{2,256}\\.[a-z]{2,4}\\b([-a-zA-Z0-9@:%_\\+.~#?&//=]*)\\s\\(([^\\)]+)\\)");
            if (match.Success)
            {
                return match.Groups[3].Value;
            }
            return input;
        }

        public async Task<IEnumerable<ToastNotification>> CreateNotifications(TodoistService todoist)
        {
            var today = await todoist.GetLabel("today");
            List<Item> allTodos = await todoist.GetItems();
            List<Item> todos = allTodos.Where(i => i.Labels.Contains(today.Id)).OrderBy(i => i.Priority).ToList();
            //if(todos.Count == 0)
            //{
            //    todos = allTodos.Where(i => i.DueDateUtc < DateTime.UtcNow.Date.AddDays(1)).ToList();
            //}
            var proj = await todoist.GetProjects();
            return todos.Select(i => (todo: i, toast: new ToastContent()
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
            })).Select(i => new ToastNotification(i.toast.GetXml())
            {
                SuppressPopup = true,
                Tag = i.todo.Id.ToString()
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
                    },
                    new ToastButton("Remove", "remove=" + todoId)
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
                            Text = DisplayContent(todo.Content),
                            HintMaxLines = 1
                        },

                        new AdaptiveText()
                        {
                            Text = p.Name
                        },

                        new AdaptiveText()
                        {
                            Text = todo.Due?.String ?? "@today"
                        }
                    }
                }
            }; 
        }


    }
}
