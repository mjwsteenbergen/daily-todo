using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using ApiLibs.Todoist;

namespace DailyTodo.Helpers
{

    public class TodoistItem
    {
        public virtual string GetSVG => "";

        public string item;

        public TodoistItem(string item)
        {
            this.item = item;
        }

        public string GetItem()
        {
            return item;
        }
    }

    public class LabelItem : TodoistItem
    {
        public LabelItem(Label label) : base(label.Name)
        {
            Label = label;
        }

        public override string GetSVG => "M4.5 8v8c0 .82.72 1.5 1.62 1.5h9.31c.44 0 1.04-.29 1.28-.61l3.32-4.38c.2-.27.2-.75 0-1.02L16.7 7.11a1.86 1.86 0 0 0-1.28-.61H6.12c-.9 0-1.62.69-1.62 1.5z";

        public Label Label { get; }
    }

    public class ProjectItem : TodoistItem
    {
        public ProjectItem(Project project) : base(project.Name)
        {
            Project = project;
        }

        public override string GetSVG => "M6 8a1 1 0 0 0-1 1v8c0 .5.5 1 1 1h12a1 1 0 0 0 1-1V9c0-.5-.5-1-1-1H6zM4 7c0-1.1.9-2 2-2h12a2 2 0 0 1 2 2v10a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V7zm3 4h10v1H7v-1zm0 3h10v1H7v-1z";

        public Project Project { get; }
    }

    public class TodoistParser
    {

        List<Project> projects;
        List<Label> labels;

        public TodoistParser(List<Project> projects, List<Label> labels)
        {
            this.projects = projects;
            this.labels = labels;
        }

        public List<TodoistItem> Parse(string text)
        {
            List<TodoistItem> items = new List<TodoistItem>();

            for (int i = 0; i < text.Length - 1; i++)
            {
                if (text[i] != '@' && text[i] != '#' && S(text, i - 1))
                {
                    continue;
                }

                string itemName = "";
                List<string> possibles = text[i] == '@' ? labels.Select(it => it.Name).ToList() : projects.Select(it => it.Name).ToList();
                for (int j = i + 1; j < text.Length; j++)
                {
                    itemName = itemName + G(text, j);
                    if (S(text, j + 1))
                    {
                        var result = possibles.Where(it => it.ToLower() == itemName.ToLower()).ToList();
                        if (result.Count == 1)
                        {
                            var newItem = result.First();
                            if (text[i] == '@')
                            {
                                items.Add(new LabelItem(labels.First(it => it.Name == newItem)));
                            }
                            else
                            {
                                items.Add(new ProjectItem(projects.First(it =>  it.Name == newItem)));
                            }
                            i = j + 1;
                            break;
                        }
                    }
                }
            }

            return items;
        }

        private static char? G(string text, int index)
        {
            if (index >= 0 && index < text.Length - 1)
            {

                return text[index];
            }
            else
            {
                return null;
            }
        }

        /**
        * Is space
        */
        private static bool S(string text, int index)
        {
            var s = G(text, index);
            return s == ' ' || s == null;
        }

        public void Suggest(int caret, string text)
        {

        }
    }
}
