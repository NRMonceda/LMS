using NLTD.EmployeePortal.LMS.Ux.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Xml.Serialization;

namespace NLTD.EmployeePortal.LMS.Ux.AppHelpers
{
    public static class MenuHelper
    {
        public static string GetMenu(string RoleText, bool IsHandlingTeam)
        {
            Menu menu = new Menu();

            String path = "";
            string path1 = HostingEnvironment.MapPath("~/menuconfigs/employee.xml");
            string path2 = HostingEnvironment.MapPath("~/menuconfigs/lead.xml");
            string path3 = HostingEnvironment.MapPath("~/menuconfigs/admin.xml");
            string path4 = HostingEnvironment.MapPath("~/menuconfigs/hr.xml");
            string path5 = HostingEnvironment.MapPath("~/menuconfigs/help.xml");
            switch (RoleText.ToUpper())
            {
                case "ADMIN":
                    path = path3;
                    break;

                case "HR":
                    path = path4;
                    break;

                case "EMPLOYEE":
                    if (IsHandlingTeam)
                    {
                        path = path2;
                    }
                    else
                    {
                        path = path1;
                    }
                    break;

                default://check and remove this
                    break;
            }
            if (true)     //!String.IsNullOrEmpty(path))
            {
                menu.menuitem = new List<MenuItem>();
                Menu menuPerPath = null;
                XmlSerializer serializer = new XmlSerializer(typeof(Menu));

                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    StreamReader reader = new StreamReader(stream);
                    menuPerPath = (Menu)serializer.Deserialize(reader);
                    menu.menuitem.AddRange(menuPerPath.menuitem);
                }

                using (var stream = new FileStream(path5, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    StreamReader reader = new StreamReader(stream);
                    menuPerPath = (Menu)serializer.Deserialize(reader);
                    menu.menuitem.AddRange(menuPerPath.menuitem);
                }
                //menu.menuitem=RemoveDublicates(menu.menuitem);
                return FormatMenuForAdminLTE(menu);
            }
            else
            {
                return "";
            }
        }

        private static string FormatMenuForAdminLTE(Menu menu)
        {
            StringBuilder MenuBuilder = new StringBuilder();
            MenuBuilder.Append("<ul class=\"sidebar-menu\">");
            foreach (var item in menu.menuitem)
            {
                if (item.level1item != null && item.level1item.Count > 0)
                {
                    MenuBuilder.Append("<li class=\"treeview\">");
                    MenuBuilder.Append(FormatMenuItemForAdminLTE(item, true));
                    MenuBuilder.Append("<ul class=\"treeview-menu\">");
                    MenuBuilder.Append(FormatMenuItemForAdminLTE(item.level1item));
                    MenuBuilder.Append("</ul>");
                    MenuBuilder.Append("</li>");
                }
                else
                {
                    MenuBuilder.Append("<li>");
                    MenuBuilder.Append(FormatMenuItemForAdminLTE(item));
                    MenuBuilder.Append("</li>");
                }
            }
            MenuBuilder.Append("</ul>");
            return MenuBuilder.ToString();
        }

        private static string FormatMenuItemForAdminLTE(List<MenuItem> menuList)
        {
            StringBuilder MenuBuilder = new StringBuilder();
            foreach (var item in menuList)
            {
                MenuBuilder.Append("<li>");
                MenuBuilder.Append(FormatMenuItemForAdminLTE(item));
                MenuBuilder.Append("</li>");
            }
            return MenuBuilder.ToString();
        }

        private static string FormatMenuItemForAdminLTE(MenuItem item, Boolean IsParent = false)
        {
            StringBuilder MenuBuilder = new StringBuilder();
            MenuBuilder.Append("<a href=\"");
            MenuBuilder.Append(item.url);
            MenuBuilder.Append("\">");
            if (!String.IsNullOrEmpty(item.icon))
            {
                MenuBuilder.Append("<i class=\"");
                MenuBuilder.Append(item.icon);
                MenuBuilder.Append("\"></i>");
            }
            MenuBuilder.Append("<span>");
            MenuBuilder.Append(item.title);
            MenuBuilder.Append("</span>");
            if (IsParent)
            {
                MenuBuilder.Append("<span class=\"pull-right-container\"><i class=\"fa fa-angle-left pull-right\"></i></span>");
            }
            MenuBuilder.Append("</a>");
            return MenuBuilder.ToString();
        }
    }
}