using ApiLibs.Todoist;
using DailyTodo.Core.Helpers;
using DailyTodo.Helpers;
using DailyTodo.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace DailyTodo.Views
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        Label todayLabel;
        public static List<Project> Projects;
        public List<Project> InstanceProjects => Projects;

        public static List<Label> Labels;
        public List<Label> InstanceLabels => Labels;

        public static List<Item> Items;
        public List<Item> InstanceItems => Items.Where(i => FilterProjects.Count == 0 ? true : FilterProjects.Any(j => j.Id == i.ProjectId)).Where(i => ShouldFilter(i, Filter, QuickAddTextbox.Text)).ToList();

        public List<Project> FilterProjects = new List<Project>();
        public List<TodoistItem> Filter = new List<TodoistItem>();

        public Settings settings;
        public TodoistService Todoist;

        bool isLoaded = false;
        public ObservableCollection<TodoistItem> quickaddItems { get; set; }

        private TodoistParser parser;

        public MainPage()
        {
            InitializeComponent();
            quickaddItems = new ObservableCollection<TodoistItem>();
            Items = new List<Item>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {

            settings = await new UwpMemory().Read("settings.json", new Settings
            {

            });
            Todoist = new TodoistService(settings.TodoistKey, settings.TodoistUserAgent);

            try
            {
                todayLabel = await Todoist.GetLabel("today");
            }
            catch (Exception ex)
            {
                await ContentDialogTodoistKey.ShowAsync();
                Todoist = new TodoistService(settings.TodoistKey, settings.TodoistUserAgent);
                todayLabel = await Todoist.GetLabel("today");
            }

            Projects = Order(await Todoist.GetProjects()).ToList();
            Labels = await Todoist.GetLabels();
            Items = await Todoist.GetItems();

            await new NotificationHandler().UpdateNotifications(Todoist);
            await Singleton<LiveTileService>.Instance.SampleUpdate(Todoist);

            OnPropertyChanged(nameof(InstanceLabels));
            OnPropertyChanged(nameof(InstanceProjects));
            OnPropertyChanged(nameof(InstanceItems));

            parser = new TodoistParser(Projects, Labels);

            RedoTodos();
            
            isLoaded = true;
        }

        private IEnumerable<Project> Order(List<Project> projects, long? id = null)
        {
            return projects.Where(i => i.ParentId == id).OrderBy(i => i.ChildOrder).SelectMany(i => Order(projects, i.Id).Prepend(i));
        }

        private void RedoTodos()
        {
            Items = Items.OrderByDescending(i => Normalise(i.Due?.Date ?? i.DateAdded)).ToList();
            OnPropertyChanged(nameof(InstanceItems));
        }

        public DateTime Normalise(DateTimeOffset time)
        {
            var tim = time < DateTime.Now ? time : time - 2 * (time - DateTime.Now);
            return tim.DateTime;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is GridView panel && panel.ItemsSource is ObservableCollection<Item> items)
            {
                foreach (var forl in e.AddedItems)
                {
                    Item item = forl as Item;

                    var grid = panel.ContainerFromItem(forl) as GridViewItem;


                    if(grid != null)
                    {
                        
                        grid.BorderBrush = new SolidColorBrush(Colors.Transparent);
                        grid.BorderThickness = new Thickness(0);
                    }


                    if (isLoaded)
                    {
                        
                    }
                    
                }

                foreach (var forl in e.RemovedItems)
                {
                    Item item = forl as Item;

                    if(isLoaded)
                    {
                        
                    }
                    
                }
            }
        }

        private async void SetColor(object sender, RoutedEventArgs e)
        {
            if(sender is StackPanel panel && panel.DataContext is Item item)
            {
                panel.Background = new SolidColorBrush(GetColor(item));
            }
        }

        private Color GetColor(Item todo)
        {
            var proj = Projects.FirstOrDefault(i => i.Id == todo.ProjectId);

            string colour = "#ffffff";

            if (proj != null)
            {
                colour = TodoistProjectColor.Convert((int)proj.Color);
            }

            var col = Color.FromArgb(
            Convert.ToByte(255),
            Convert.ToByte(colour.Substring(1, 2), 16),
            Convert.ToByte(colour.Substring(3, 2), 16),
            Convert.ToByte(colour.Substring(5, 2), 16));

            return col;
        }

        private void SetPossibleTodayColor(object sender, RoutedEventArgs e)
        {
            if (sender is Grid grid && grid.DataContext is Item item)
            {
                if(item.Labels.Contains(todayLabel.Id))
                {

                    var col = GetColor(item);
                    col.A = Convert.ToByte(100);
                    grid.Background = new SolidColorBrush(col);
                }

            }
        }

        private async void Show_Todoist_Flyout(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            await ContentDialogTodoistKey.ShowAsync();
        }

        private async void Grid_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (sender is Grid grid && grid.DataContext is Item item)
            {
                Label yesterday = Labels.First(i => i.Name == "yesterday");
                if (item.Labels.Contains(todayLabel.Id))
                {
                    item.Labels.Remove(todayLabel.Id);

                    var col = GetColor(item);
                    col.A = Convert.ToByte(100);
                    grid.Background = new SolidColorBrush(Colors.Transparent);

                    await Todoist.Update(new ItemUpdate(item.Id)
                    {
                        Labels = item.Labels
                    });
                }
                else
                {
                    item.Labels.Add(todayLabel.Id);

                    if(item.Labels.Contains(yesterday.Id))
                    {
                        item.Labels.Remove(yesterday.Id);
                    }

                    var col = GetColor(item);
                    col.A = Convert.ToByte(100);
                    grid.Background = new SolidColorBrush(col);

                    await Todoist.Update(new ItemUpdate(item.Id)
                    {
                        Labels = item.Labels
                    });
                }

                await new NotificationHandler().UpdateNotifications(Todoist);

            }
        }

        private void SwipeItem_Invoked(SwipeItem sender, SwipeItemInvokedEventArgs args)
        {
            
        }

        private void QuickAddTextbox_CharacterReceived(UIElement sender, Windows.UI.Xaml.Input.CharacterReceivedRoutedEventArgs args)
        {
            if(parser != null)
            {
                Filter = parser.Parse(QuickAddTextbox.Text);
                quickaddItems.Clear();

                foreach (var item in Filter)
                {
                    quickaddItems.Add(item);
                }
                RedoTodos();
            }
        }

        public bool ShouldFilter(Item todo, List<TodoistItem> filters, string text)
        {
            text = text.ToLower();
            foreach (var filter in filters)
            {
                if(filter is ProjectItem projectFilter)
                {
                    if(projectFilter.Project.Id != todo.ProjectId) {
                        return false;
                    }
                    text = text.Replace($"#{projectFilter.Project.Name.ToLower()} ", "");
                }

                if (filter is LabelItem labelFilter)
                {
                    if (!todo.Labels.Contains(labelFilter.Label.Id))
                    {
                        return false;
                    }
                    text = text.Replace($"@{labelFilter.Label.Name.ToLower()} ", "");

                }
            }

            foreach(var word in text.Split(' '))
            {
                if(!todo.Content.ToLower().Contains(word))
                {
                    return false;
                }
            }

            return true;
        }

        private void QuickAddTextbox_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs args)
        {
            if (args.Key == Windows.System.VirtualKey.Tab)
            {
                int select = QuickAddTextbox.SelectionStart;
                QuickAddTextbox.SelectionStart = select;

                string insert = "test";

                QuickAddTextbox.Text = QuickAddTextbox.Text.Insert(select, insert);
                QuickAddTextbox.SelectionStart = select + insert.Length;

                args.Handled = true;
            }
        }

        private void ProjectSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(sender is ListView view)
            {
                FilterProjects = view.SelectedItems.Select(i => i as Project).SelectMany(i => Projects.Where(j => j.ParentId == i.Id).Prepend(i)).ToList();
            }
            OnPropertyChanged(nameof(InstanceItems));
        }

        private async void FlyoutItem_CompleteItem(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem flyout && flyout.DataContext is Item todo)
            {
                await Todoist.MarkTodoAsDone(todo);
                Items = await Todoist.GetItems();
                RedoTodos();
            }
        }

        private void FlyoutItem_Filter(object sender, RoutedEventArgs e)
        {
            if(sender is MenuFlyoutItem flyout && flyout.DataContext is Item todo)
            {
                var newItem = Projects.First(i => i.Id == todo.ProjectId);
                FilterProjects.Add(newItem);
                ProjectSelector.SelectedItems.Add(newItem);
                OnPropertyChanged(nameof(InstanceProjects));
                OnPropertyChanged(nameof(InstanceItems));
            }
        }

        private async void FlyoutItem_ChangeLabel(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem flyout && flyout.DataContext is Item todo)
            {
                Label l = Labels.First(i => i.Name == flyout.Text);
                if(todo.Labels.Contains(l.Id))
                {
                    await Todoist.Update(new ItemUpdate(todo.Id)
                    {
                        Labels = todo.Labels.Where(i => i != l.Id).ToList()
                    });
                }
                else
                {
                    todo.Labels.Add(l.Id);
                    await Todoist.Update(new ItemUpdate(todo.Id)
                    {
                        Labels = todo.Labels
                    });
                }
                Items = await Todoist.GetItems();
                RedoTodos();
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if(sender is TextBox box)
            {
                switch (box.Name)
                {
                    case "TodoistKeyBox":
                        settings.TodoistKey = box.Text;
                        break;
                    case "TodoistAgentBox":
                        settings.TodoistUserAgent = box.Text;
                        break;
                    default:
                        break;
                }

                new UwpMemory().Write("settings.json", settings);
            }
        }
    }

    public class ItemTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {

            if(value is Item item)
            {
                var labelList = item.Labels.Select(i => MainPage.Labels.First(j => i == j.Id).Name).ToList();

                List<string> list = new List<string> { MainPage.Projects.FirstOrDefault(i => i.Id == item.ProjectId)?.Name};

                switch (labelList.Count)
                {
                    case 0:
                        break;
                    case 1:
                        list.Add("[" + labelList.First() + "]");
                        break;
                    default:
                        list.Add("[" + labelList.Aggregate((i, j) => i + "," + j) + "]");
                        break;
                }

                var due = item.Due?.Date.ToString("m");

                if(due != null)
                {
                    list = list.Prepend(due).ToList();
                }

                return list.Aggregate((i,j) => i + " • " + j);
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }

    public class ContentTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Item item)
            {
                return NotificationHandler.DisplayContent(item.Content);
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }

    public class MarginConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Project proj)
            {
                return proj.ParentId == null ? new Thickness(3) : new Thickness(24, 3, 3, 3);
            }

            return new Thickness(3);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }

    public class Settings
    {
        public string TodoistKey { get; set; }
        public string TodoistUserAgent { get; internal set; }
    }
}
