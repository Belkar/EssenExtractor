using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EssenExtractor
{
    public class MenuDay
    {
        public DateTime DateTime { get; set; }
        public List<MenuEntry> MenuEntries { get; set; }
        public string Location { get; set; }

        public string CreateDescription()
        {
            StringBuilder sb = new StringBuilder();
            foreach (MenuEntry entry in MenuEntries)
            {
                sb.AppendLine(entry.Title);
                sb.AppendLine(entry.MenuDesc); //.Replace("\n", "\r\n")
            }
            sb.Append("Stand: ").AppendLine(DateTime.Now.ToString());
            string result = sb.ToString();
            return result.Replace("\r\n", "\n").Replace("\n", "\\n");
        }
    }

    public class MenuEntry
    {
        public string Title { get; set; }
        public string MenuDesc { get; set; }
    }
}
