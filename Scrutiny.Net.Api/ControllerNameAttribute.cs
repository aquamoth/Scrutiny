using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scrutiny.Net.Api
{
    public class ControllerNameAttribute : Attribute
    {
        [Required]
        public string Name { get; set; }

        public ControllerNameAttribute(string name)
        {
            Name = name;
        }
    }
}
