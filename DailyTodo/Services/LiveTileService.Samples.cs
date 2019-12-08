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
        public async Task SampleUpdate()
        {
            Settings settings = await new UwpMemory().Read<Settings>("settings.json");
            var todoist = new TodoistService(settings.TodoistKey, settings.TodoistUserAgent);
            var today = await todoist.GetLabel("today");
            List<Item> allTodos = await todoist.GetItems();
            List<Item> todos = allTodos.Where(i => i.Labels.Contains(today.Id)).OrderBy(i => i.Priority).ToList();

            // Construct the tile content
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
                                    HintStyle = AdaptiveTextStyle.Title,
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
                                    HintStyle = AdaptiveTextStyle.Title,
                                }
                            }
                        }
                    },
                }
            };

            var todoText = todos.Take(3).Select(i => new AdaptiveText()
            {
                Text = $" - {i.Content} {"@" + i.Due?.Date.ToString("d\\/MM") ?? "@today"}",
                HintStyle = AdaptiveTextStyle.CaptionSubtle
            }).ToList();

            todoText.ForEach(i => (content.Visual.TileMedium.Content as TileBindingContentAdaptive).Children.Add(i));

            todoText.ForEach(i => (content.Visual.TileWide.Content as TileBindingContentAdaptive).Children.Add(i));

            todoText.ForEach(i => (content.Visual.TileLarge.Content as TileBindingContentAdaptive).Children.Add(i));



            // Then create the tile notification
            var notification = new TileNotification(content.GetXml());
            UpdateTile(notification);
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
