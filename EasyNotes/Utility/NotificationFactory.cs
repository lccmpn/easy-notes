using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace EasyNotes.Utility
{
    class NotificationBuilder
    {

        public static XmlDocument BuildNoTitleNotification(string content)
        {
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
            XmlNodeList toastText = toastXml.GetElementsByTagName("text");
            (toastText[0] as XmlElement).InnerText = content;
            return toastXml;
        }

        public static XmlDocument BuildTitleAndContentNotification(string title, string content)
        {
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            XmlNodeList toastText = toastXml.GetElementsByTagName("text");
            (toastText[0] as XmlElement).InnerText = title;
            (toastText[1] as XmlElement).InnerText = content;
            return toastXml;
        }
    }
}
