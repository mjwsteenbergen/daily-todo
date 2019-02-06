using ApiLibs.Todoist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyTodo.Services
{
    class TodoistGenerator
    {
        public static TodoistService Todoist => new TodoistService("", "");
    }
}
