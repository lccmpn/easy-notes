using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Resources;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyNotes.Utility
{
    public enum StringResources { RESOURCES, ERRORS };

    class AppResourcesLoader
    {
        public static string LoadStringResource(StringResources path, string name)
        {
            string resourcePath = "";
            switch (path)
            {
                case StringResources.RESOURCES:
                    resourcePath = "Resources";
                    break;
                case StringResources.ERRORS:
                    resourcePath = "Errors";
                    break;
                default:
                    new Exception("Can't find any resource");
                    break;
            }
            return ResourceLoader.GetForCurrentView(resourcePath).GetString(name);
        }
    }
}
