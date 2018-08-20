using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NLTD.EmployeePortal.LMS.Ux.Models
{
    public interface IMenuItem
    {
        String title { get; set; }
        String url { get; set; }
        String icon { get; set; }
    }

    public class MenuItem : IMenuItem
    {
        public String title { get; set; }
        public String url { get; set; }
        public String icon { get; set; }
        public List<MenuItem> level1item { get; set; }
    }

    public class Menu
    {
        public List<MenuItem> menuitem { get; set; }
    }
}