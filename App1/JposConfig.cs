using Android.Content;
using Com.Shtrih.Util;
using Jpos.Loader;
using Jpos.Util;
using System.Collections.Generic;
using System.IO;

namespace App1
{
    public class JposConfig
    {

        public static void configure(string deviceName, string portName, Context context)
        {

            var props = new Dictionary<string, string>();
            props["portName"] = portName;

            configure(deviceName, context, props);
        }

        public static void configure(string deviceName, string portName, Context context, string portType, string protocol)
        {
            var props = new Dictionary<string, string>();
            props["portName"] = portName;
            props["portType"] = portType;
            props["protocolType"] = protocol;

            configure(deviceName, context, props);
        }

        public static void configure(string deviceName, Context context, Dictionary<string, string> props)
        {
            //LogbackConfig.configure(SysUtils.FilesPath + "tinyJavaPosTester.log");

            copyAsset("jpos.xml", SysUtils.FilesPath + "jpos.xml", context);
            string fileURL = "file://" + SysUtils.FilesPath + "jpos.xml";
            Java.Lang.JavaSystem.SetProperty(JposPropertiesConst.JposPopulatorFileUrlPropName, fileURL);

            Java.Lang.JavaSystem.SetProperty(
                        JposPropertiesConst.JposRegPopulatorClassPropName,
                        "jpos.config.simple.xml.SimpleXmlRegPopulator");

            var registry = JposServiceLoader.Manager.EntryRegistry;
            if (registry.HasJposEntry(deviceName))
            {
                var jposEntry = registry.GetJposEntry(deviceName);
                if (jposEntry != null)
                {
                    foreach (var item in props)
                    {
                        string key = item.Key;
                        string value = item.Value;
                        if (jposEntry.HasPropertyWithName(key))
                            jposEntry.ModifyPropertyValue(key, value);
                        else
                            jposEntry.AddProperty(key, value);
                    }
                }
            }

            // TODO: throw?
        }

        public static void copyAsset(string assetFileName, string destFile, Context context)
        {
            using (var rdr = context.Assets.Open(assetFileName))
            using (var writer = System.IO.File.Open(destFile, FileMode.OpenOrCreate))
            {

                rdr.CopyTo(writer);
                writer.Flush(true);
            }
        }
    }
}
