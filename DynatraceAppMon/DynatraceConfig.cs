using System;
using System.IO;
using System.Xml.Serialization;

namespace DynaTrace.CodeLink
{
    public class DynatraceConfig
    {

        private const int STANDARD_CLIENT_PORT = 8030;

        private static XmlSerializer xmlSerializer = new XmlSerializer(typeof(DynatraceConfig));

        private bool pluginEnabled;
        private int clientPort, serverPort;
        private int port;
        private String serverHost;

        public DynatraceConfig()
        {
            pluginEnabled = false;
            clientPort = STANDARD_CLIENT_PORT;
        }

        public DynatraceConfig(DynatraceConfig config)
        {
            pluginEnabled = config.pluginEnabled;
            port = config.port;
            clientPort = config.clientPort;
        }

        public bool PluginEnabled
        {
            get { return pluginEnabled; }
            set { pluginEnabled = value; }
        }

        public int ClientPort
        {
            get { return clientPort; }
            set { clientPort = value; }
        }

        public String ServerHost
        {
            get { return serverHost; }
            set { serverHost = value; }
        }

        public int ServerPort
        {
            get { return serverPort; }
            set { serverPort = value; }
        }

        public void write(Context context, string visualStudioVersion)
        {
            try
            {
                string path = getConfigFilePath(visualStudioVersion);
                string dir = Path.GetDirectoryName(path);
                Directory.CreateDirectory(dir);

                FileStream stream = new FileStream(path, FileMode.Create);
                xmlSerializer.Serialize(stream, this);
                stream.Close();
            }
            catch (Exception e)
            {
                context.log(Context.LOG_ERROR + e.ToString());
            }
        }

        public static DynatraceConfig read(Context context, string visualStudioVersion)
        {
            try
            {
                string path = getConfigFilePath(visualStudioVersion);

                DynatraceConfig config;
                if (File.Exists(path))
                {
                    FileStream stream = new FileStream(path, FileMode.Open);
                    config = (DynatraceConfig)xmlSerializer.Deserialize(stream);
                    stream.Close();
                }
                else {
                    config = new DynatraceConfig();
                    config.write(context, visualStudioVersion);
                }
                return config;
            }
            catch (Exception e)
            {
                context.log(Context.LOG_ERROR + e.ToString());
                return null;
            }
        }

        public static string getAppDataPath()
        {

            //if (System.Environment.GetEnvironmentVariable("USERPROFILE") != null)
            //{
            //    return
            //    Path.Combine(
            //       Path.Combine(
            //           System.Environment.GetEnvironmentVariable("USERPROFILE"),
            //           ".dynaTrace"),
            //       "CodeLinkNet");
            //}
            return
                Path.Combine(
                   Path.Combine(
                       Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                       "dynaTrace"),
                   "CodeLinkNet");
        }

        public static string getConfigFilePath(string visualStudioVersion)
        {
            string dir = getAppDataPath();

            if (visualStudioVersion.StartsWith("7.1")) // can be 7.1 or 7.10
                return Path.Combine(dir, "vs2003.xml");
            if (visualStudioVersion.StartsWith("8."))
                return Path.Combine(dir, "vs2005.xml");
            if (visualStudioVersion.StartsWith("9."))
                return Path.Combine(dir, "vs2008.xml");
            if (visualStudioVersion.StartsWith("10."))
                return Path.Combine(dir, "vs2010.xml");
            if (visualStudioVersion.StartsWith("11."))
                return Path.Combine(dir, "vs2012.xml");

            // fallback for versions that we dont know yet
            return Path.Combine(dir, String.Format("vs{0}.xml", visualStudioVersion));
        }

    }
}
