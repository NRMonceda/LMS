using NLTD.EmployeePortal.LMS.Ux.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
            string employeePath = HostingEnvironment.MapPath("~/menuconfigs/employee.xml");
            string leadPath = HostingEnvironment.MapPath("~/menuconfigs/lead.xml");
            string adminPath = HostingEnvironment.MapPath("~/menuconfigs/admin.xml");
            string hrPath = HostingEnvironment.MapPath("~/menuconfigs/hr.xml");
            string commonPath = HostingEnvironment.MapPath("~/menuconfigs/help.xml");
            switch (RoleText.ToUpper())
            {
                case "ADMIN":
                    path = adminPath;
                    break;

                case "HR":
                    path = hrPath;
                    break;

                case "EMPLOYEE":
                    if (IsHandlingTeam)
                    {
                        path = leadPath;
                    }
                    else
                    {
                        path = employeePath;
                    }
                    break;

                default://check and remove this
                    break;
            }
            menu.menuitem = new List<MenuItem>();
            Menu menuPerPath = null;
            XmlSerializer serializer = new XmlSerializer(typeof(Menu));

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                StreamReader reader = new StreamReader(stream);
                menuPerPath = (Menu)serializer.Deserialize(reader);
                menu.menuitem.AddRange(menuPerPath.menuitem);
            }

            using (var stream = new FileStream(commonPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                StreamReader reader = new StreamReader(stream);
                menuPerPath = (Menu)serializer.Deserialize(reader);
                menu.menuitem.AddRange(menuPerPath.menuitem);
            }
            return FormatMenuForAdminLTE(menu);
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