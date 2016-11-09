using System;
using System.Windows.Forms;

namespace DynaTrace.CodeLink
{
    class ConfigFormCode
    {

        private ConfigFormCode() { }

        public static void Load(Context context, FirstPackage.Config form)
        {
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.OkButton.Focus();

            form.EnabledCheckBox.Checked = context.Config.PluginEnabled;
        }

        public static void EnableChanged(FirstPackage.Config form)
        {
        }

        public static bool Ok(Context context, bool enabled, String portStr, DynatraceConfig prevConfig)
        {

            // if the plugin was disabled, close connection
            if (!enabled)
            {
                try
                {
                    context.disconnect();

                }
                catch (Exception e)
                {
                    context.log(Context.LOG_ERROR + e.ToString());
                }
                context.Config.PluginEnabled = false;
                context.Config.write(context, context.VisualStudioVersion);
                return true;
            }

            // validate input data
            int port;
            try
            {
                port = Convert.ToInt32(portStr);
            }
            catch (System.FormatException)
            {
                MessageBox.Show("Port must be an integer beetween 0-65535");
                return false;
            }

            if (!(port >= 0 && port <= 65535))
            {
                MessageBox.Show("Port must be an integer beetween 0-65535");
                return false;
            }

            context.Config.PluginEnabled = true;
            context.Config.ClientPort = port;

            if (prevConfig.ClientPort != port || !prevConfig.PluginEnabled)   //connect only if port has changed or if plugin was enabled
            {
                // connect using new port
                try
                {
                    context.connect();
                    context.Config.write(context, context.VisualStudioVersion);
                }
                catch (Exception e)
                {
                    context.log(Context.LOG_ERROR + e.ToString());
                }
            }
            return true;
        }

    }
}

