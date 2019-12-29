using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiLibs.MicrosoftGraph;
using ApiLibs.Todoist;
using DailyTodo.Helpers;
using DailyTodo.Views;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.ApplicationModel.Contacts;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;

namespace DailyTodo.Services
{
    internal partial class LiveTileService
    {
        // More about Live Tiles Notifications at https://docs.microsoft.com/windows/uwp/controls-and-patterns/tiles-and-notifications-sending-a-local-tile-notification
        public async Task SampleUpdate(TodoistService todoist)
        {
            var today = await todoist.GetLabel("today");
            List<Item> allTodos = await todoist.GetItems();
            List<Item> todos = allTodos.Where(i => i.Labels.Contains(today.Id)).OrderBy(i => i.Priority).ToList();

            var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            updater.Clear();
            updater.EnableNotificationQueue(true);



            for (int i = 0; i < todos.Count; i += 4)
            {

                var content = new TileContent()
                {
                    Visual = new TileVisual()
                    {

                        TileMedium = new TileBinding()
                        {
                            Content = new TileBindingContentAdaptive()
                            {
                                Children = {
                                    new AdaptiveText()
                                    {
                                        Text = $"DailyTodo ({todos.Count})",
                                    }
                                }
                            }
                        },

                        TileWide = new TileBinding()
                        {
                            Content = new TileBindingContentAdaptive()
                            {
                                Children = {
                                    new AdaptiveText()
                                    {
                                        Text = $"DailyTodo ({todos.Count})",
                                        HintStyle = AdaptiveTextStyle.Base,
                                    }
                                }
                            }
                        },

                        TileLarge = new TileBinding()
                        {
                            Content = new TileBindingContentAdaptive()
                            {
                                Children = {
                                    new AdaptiveText()
                                    {
                                        Text = $"DailyTodo ({todos.Count})",
                                        HintStyle = AdaptiveTextStyle.Base,
                                    }
                                }
                            }
                        }
                    }
                };

                var todoText = todos.Skip(i).Take(4).Select(j => new AdaptiveText()
                {
                    Text = $" - {NotificationHandler.DisplayContent(j.Content)}",
                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                }).ToList();

                todoText.ForEach(j => (content.Visual.TileMedium.Content as TileBindingContentAdaptive).Children.Add(j));

                todoText.ForEach(j => (content.Visual.TileWide.Content as TileBindingContentAdaptive).Children.Add(j));

                todoText.ForEach(j => (content.Visual.TileLarge.Content as TileBindingContentAdaptive).Children.Add(j));

                // Then create the tile notification
                var notification = new TileNotification(content.GetXml());
                notification.Tag = "item" + i;

                try
                {
                    updater.Update(notification);
                }
                catch (Exception)
                {
                    // TODO WTS: Updating LiveTile can fail in rare conditions, please handle exceptions as appropriate to your scenario.
                }
            }

            // Construct the tile content





        }

        public async Task SamplePinSecondaryAsync(string pageName)
        {
            // TODO WTS: Call this method to Pin a Secondary Tile from a page.
            // You also must implement the navigation to this specific page in the OnLaunched event handler on App.xaml.cs
            var tile = new SecondaryTile(DateTime.Now.Ticks.ToString());
            tile.Arguments = pageName;
            tile.DisplayName = pageName;
            tile.VisualElements.Square44x44Logo = new Uri("ms-appx:///Assets/Square44x44Logo.scale-200.png");
            tile.VisualElements.Square150x150Logo = new Uri("ms-appx:///Assets/Square150x150Logo.scale-200.png");
            tile.VisualElements.Wide310x150Logo = new Uri("ms-appx:///Assets/Wide310x150Logo.scale-200.png");
            tile.VisualElements.ShowNameOnSquare150x150Logo = true;
            tile.VisualElements.ShowNameOnWide310x150Logo = true;
            await PinSecondaryTileAsync(tile);
        }
    }
}
