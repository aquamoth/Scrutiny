using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.Models
{
    public class ClientConfiguration
    {
        public string[] frameworks { get; set; }
        public string[] preprocessors { get; set; }
        public string[] reporters { get; set; }
        public bool captureConsole { get; set; }
    }
}
