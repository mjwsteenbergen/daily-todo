using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyTodo.Views
{

    public static class TodoistProjectColor
    {
        public static string Convert(int color)
        {
            switch (color)
            {
                case 0:
                    return "#95ef63";
                case 1:
                    return "#ff8581";
                case 2:
                    return "#ffc471";
                case 3:
                    return "#f9ec75";
                case 4:
                    return "#a8c8e4";
                case 5:
                    return "#d2b8a3";
                case 6:
                    return "#e2a8e4";
                case 7:
                    return "#cccccc";
                case 8:
                    return "#fb886e";
                case 9:
                    return "#ffcc00";
                case 10:
                    return "#74e8d3";
                case 11:
                    return "#3bd5fb";
                case 12:
                    return "#dc4fad";
                case 13:
                    return "#ac193d";
                case 14:
                    return "#d24726";
                case 15:
                    return "#82ba00";
                case 16:
                    return "#03b3b2";
                case 17:
                    return "#008299";
                case 18:
                    return "#5db2ff";
                case 19:
                    return "#0072c6";
                case 20:
                    return "#000000";
                case 21:
                    return "#777777";
                default:
                    return TodoistColor.Convert(color);
            }
        }
    }

    public static class TodoistColor
    {
        public static string Convert(int color)
        {
            switch (color)
            {
                case 30:
                    return "#b8256f";
                case 31:
                    return "#db4035";
                case 32:
                    return "#ff9933";
                case 33:
                    return "#fad000";
                case 34:
                    return "#afb83b";
                case 35:
                    return "#7ecc49";
                case 36:
                    return "#299438";
                case 37:
                    return "#6accbc";
                case 38:
                    return "#158fad";
                case 39:
                    return "#14aaf5";
                case 40:
                    return "#96c3eb";
                case 41:
                    return "#4073ff";
                case 42:
                    return "#884dff";
                case 43:
                    return "#af38eb";
                case 44:
                    return "#eb96eb";
                case 45:
                    return "#e05194";
                case 46:
                    return "#ff8d85";
                case 47:
                    return "#808080";
                case 48:
                    return "#b8b8b8";
                case 49:
                    return "#ccac93";
                default:
                    return "#000000";

            }
        }
    }

    public static class TodoistLabelColor
    {
        public static string Convert(int color)
        {
            switch (color)
            {
                case 0:
                    return "#019412";
                case 1:
                    return "#a39d01";
                case 2:
                    return "#e73d02";
                case 3:
                    return "#e702a4";
                case 4:
                    return "#9902e7";
                case 5:
                    return "#1d02e7";
                case 6:
                    return "#0082c5";
                case 7:
                    return "#555555";
                case 8:
                    return "#008299";
                case 9:
                    return "#03b3b2";
                case 10:
                    return "#ac193d";
                case 11:
                    return "#82ba00";
                case 12:
                    return "#111111";
                default:
                    return TodoistColor.Convert(color);
            }
        }
    }
}
