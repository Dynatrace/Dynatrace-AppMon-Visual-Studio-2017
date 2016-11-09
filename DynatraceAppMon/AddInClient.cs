using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Reflection;

namespace DynaTrace.CodeLink
{

    class Protocol
    {

        public enum IDE : sbyte
        {
            UNKNOWN = -1,
            Eclipse = 0,
            VS2003 = 1,
            VS2005 = 2,
            VS2008 = 3,
            VS2010 = 4
        }

        public enum ResponseCode : sbyte
        {
            FOUND = 50,
            NOT_FOUND = 51,
            ERROR = 52,
            UNKNOWN = 53
        }

    }

    class AddinClient
    {
        private Context context;
        private bool running;
        private bool errorLogged = false;

        public AddinClient(Context context)
        {
            this.context = context;
        }

        internal void connect()
        {
            this.running = true;
            Thread thread = new Thread(new ThreadStart(this.run));
            thread.IsBackground = true;
            thread.Start();
        }

        internal void disconnect()
        {
            running = false;
        }

        public void run()
        {
            // get the new REST Client Interface which is introduced with dT 3.5
            RESTClient restClient = new RESTClient(context.Config.ClientPort, false);

            // get information about current ide and project that we need for the connect call
            int ideId = Context.getVisualStudioVersionAsInt(context.VisualStudioVersion);
            string ideVersion = Context.getVisualStudioVersion(context.VisualStudioVersion) + " " +
                                         context.VisualStudioVersion;
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            string solutionName = context.SolutionName;
            string solutionPath = context.getSolutionPath();

            // always running until our thread is killed
            while (running)
            {

                // connect and poll information
                if (!restClient.CodeLinkConnect(ideId, ideVersion, version.Major, version.Minor, version.Revision, solutionName, solutionPath))
                {
                    if (!errorLogged)
                    {
                        context.log(String.Format(Context.LOG_ERROR + "when calling CodeLinkConnect. Timeout={0}, versionMatched={1}, sessionId={2}", restClient.TimedOut, restClient.VersionMatched, restClient.SessionId));
                        errorLogged = true;
                    }
                }
                else
                {
                    errorLogged = false;
                    // if we got a lookup request do the lookup
                    Lookup lookup = restClient.GetLookupInformation();

                    if (lookup != null)
                    {
                        toCorrectFQClassName(ref lookup.typeName);
                        context.log(Context.LOG_INFO + "CodeLink connected with Lookup Information");

                        bool found = false;

                        /*
                         * For explicit implemened interface methods we get from restClient method named for example: [namespace].ISomeInterface.Method. For source lookup we need to have method name: ISomeInterface.Method
                         * So, when we have "." in method name from rest client we know that it is this case. We search for other "." as we can add parts of the namespace to the explicit interface implementation.                          
                         * 
                         * Algorithm looks in that way:
                         * - check for any occurrences of the ‘.’ character in the method name, as it’s not allowed for class & method names
                         * if "." not found: just search for this method;   if "." found then:
                         * - take the right substring from the 2nd-last occurrence of ‘.’ from the right and search for this method
                         *  - if not found: take the right substring from the 3rd-last occurrence of ‘.’ from the right and search for this method
                         *  - if not found, continue with 4th-last, and so on ...
                         */
                        if (lookup.methodName.Contains("."))
                        {
                            string[] splitMethod = lookup.methodName.Split('.');
                            string substringMethod = splitMethod[splitMethod.Length - 1];
                            for (int i = splitMethod.Length - 1; i > 0; i--)
                            {
                                substringMethod = splitMethod[i - 1] + "." + substringMethod;
                                lookup.methodName = substringMethod;
                                found = context.goToMethod(lookup);
                                if (found)
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            found = context.goToMethod(lookup);
                        }

                        if (found)
                        {
                            // return success?
                            context.log(Context.LOG_INFO + "Lookup succeeded");
                            restClient.CodeLinkResponse((int)Protocol.ResponseCode.FOUND);
                        }
                        else
                        {
                            // return failure
                            context.log(Context.LOG_INFO + "Lookup failed");
                            restClient.CodeLinkResponse((int)Protocol.ResponseCode.NOT_FOUND);
                        }
                    }

                    //                    else
                    //                   {
                    //                        context.log("CodeLink connected but no current lookup information");
                    //                    }
                }
            }
        }

        private void toCorrectFQClassName(ref string typeNameFromDTClient)
        {
            if (typeNameFromDTClient != null)
            {
                // from java code: SourceLookupAction::adaptForVS(String)
                typeNameFromDTClient = typeNameFromDTClient.Replace("/", ".");
                typeNameFromDTClient = typeNameFromDTClient.Replace("+", ".");
            }
        }
    }
}
