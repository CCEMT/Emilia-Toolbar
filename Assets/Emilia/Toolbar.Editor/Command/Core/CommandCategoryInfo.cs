using System.Collections.Generic;

namespace Emilia.Toolbar.Editor
{
    public class CommandCategoryInfo
    {
        public string categoryName;
        public string parent;
        public List<string> children = new List<string>();
        public List<string> commands = new List<string>();
    }
}