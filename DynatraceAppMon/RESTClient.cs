using System;
using System.Net.Sockets;
using System.Collections;

namespace DynaTrace.CodeLink
{
    public class RESTClient
    {
        #region REST Request and Response Handling
        protected System.Net.HttpWebResponse SendRESTRequest(string restFunction, bool doGet)
        {
            string uri = String.Format("http{0}://{1}:{2}{3}", Secure ? "s" : "", Servername, Port, restFunction);

            if (doGet)
                uri += "?" + _parameters;

            System.Net.HttpWebRequest webRequest = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(uri);

            if (doGet)
            {
                webRequest.Method = "GET";
            }
            else
            {
                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded";

                // write the parameters to the output stream
                try
                {
                    System.IO.StreamWriter requestStream = new System.IO.StreamWriter(webRequest.GetRequestStream());
                    requestStream.Write(_parameters);
                    requestStream.Close();
                }
                catch (System.Net.WebException e)
                {
                    return (System.Net.HttpWebResponse)e.Response;
                }
            }

            // get response
            try
            {
                return (System.Net.HttpWebResponse)webRequest.GetResponse();
            }
            catch (System.Net.WebException exp)
            {
                return (System.Net.HttpWebResponse)exp.Response;
            }
        }

        /// <summary>
        /// Returns a list of all attributeName's of XMLNodes that match the nodename
        /// </summary>
        /// <param name="response"></param>
        /// <param name="nodeName"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        protected bool ParseXMLResponse(System.Net.HttpWebResponse webResponse)
        {
            try
            {
                System.IO.StreamReader sReader = new System.IO.StreamReader(webResponse.GetResponseStream());
                string response = sReader.ReadToEnd();
                sReader.Close();

                System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
                xmlDoc.LoadXml(response);
                System.Xml.XmlNodeList nodes = xmlDoc.GetElementsByTagName("codeLinkLookup");
                if (nodes.Count == 1)
                {
                    System.Xml.XmlNode lookupNode = nodes[0];
                    System.Xml.XmlAttribute attribute = null;

                    attribute = lookupNode.Attributes["versionMatched"];
                    versionMatched = attribute != null ? attribute.Value.Equals("true") : false;

                    attribute = lookupNode.Attributes["timedOut"];
                    timedOut = attribute != null ? attribute.Value.Equals("true") : false;

                    attribute = lookupNode.Attributes["className"];
                    className = attribute != null ? attribute.Value : null;

                    attribute = lookupNode.Attributes["methodName"];
                    methodName = attribute != null ? attribute.Value : null;

                    attribute = lookupNode.Attributes["sessionId"];
                    sessionId = attribute != null ? Int64.Parse(attribute.Value) : -1;

                    ArrayList attributesValues = new ArrayList();
                    System.Xml.XmlNode attributesNode = lookupNode.SelectSingleNode("attributes");
                    if (attributesNode != null)
                    {
                        foreach (System.Xml.XmlNode attributeNode in attributesNode.ChildNodes)
                        {
                            String attributeValue = attributeNode.ChildNodes.Count > 0 ? attributeNode.ChildNodes[0].Value : attributeNode.Value;
                            attributesValues.Add(attributeValue);
                        }
                        attributes = (string[])attributesValues.ToArray(typeof(String));
                    }

                    return true;
                }
                return false;
            }
            catch (System.Net.WebException)
            {
                return false;
            }

        }
        #endregion

        #region Parameter Handling for RESTCalls
        protected string _parameters = "";
        protected void ClearParameters()
        {
            _parameters = "";
        }

        protected void AddParameter(string paramName, bool valueParam)
        {
            if (_parameters != null && _parameters.Length > 0)
                _parameters += "&";
            _parameters += String.Format("{0}={1}", paramName, valueParam ? "true" : "false");
        }

        protected void AddParameter(string paramName, long longParam)
        {
            if (_parameters != null && _parameters.Length > 0)
                _parameters += "&";
            _parameters += String.Format("{0}={1}", paramName, longParam);
        }

        protected void AddParameter(string paramName, int intParam)
        {
            if (_parameters != null && _parameters.Length > 0)
                _parameters += "&";
            _parameters += String.Format("{0}={1}", paramName, intParam);
        }

        protected void AddParameter(string paramName, string valueParam)
        {
            if (_parameters != null && _parameters.Length > 0)
                _parameters += "&";
            _parameters += String.Format("{0}={1}", paramName, System.Web.HttpUtility.UrlEncode(valueParam));
        }

        protected void AddParameter(string paramName, string[] valueParam)
        {
            foreach (string value in valueParam)
            {
                if (_parameters != null && _parameters.Length > 0)
                    _parameters += "&";
                _parameters += String.Format("{0}={1}", paramName, System.Web.HttpUtility.UrlEncode(value));
            }
        }
        #endregion

        #region Properties for RESTFul Interface
        private bool _secure = false;
        public bool Secure { get { return _secure; } set { _secure = value; } }

        private string _servername = "localhost";
        public string Servername { get { return _servername; } set { _servername = value; } }

        private int _port = 8030;
        public int Port { get { return _port; } set { _port = value; } }
        #endregion

        public RESTClient(int port, bool secure)
        {
            this.Port = port;
            this.Secure = secure;
        }

        #region REST Client Calls for CodeLink
        /// <summary>
        /// Connects the Codelink plugin to the dT Client
        /// </summary>
        /// <param name="ideId">dT internal version of VS</param>
        /// <param name="ideVersion">name of VS (e.g. "Visual Studio 2010")</param>
        /// <param name="major">major version of Codelink plugin</param>
        /// <param name="minor">minor version of Codelink plugin</param>
        /// <param name="revision">revision version of Codelink plugin</param>
        /// <param name="solution">Name of the opened solution in VS</param>
        /// <param name="solutionPath">Path of the opened solution in VS</param>
        /// <returns>true if connection to dT Client was successfull</returns>
        public bool CodeLinkConnect(int ideId, String ideVersion, int major, int minor, int revision, String solution, String solutionPath)
        {
            ClearParameters();
            AddParameter("ideid", ideId);
            AddParameter("ideversion", ideVersion);
            AddParameter("major", major);
            AddParameter("minor", minor);
            AddParameter("revision", revision);
            AddParameter("sessionid", sessionId);
            AddParameter("activeproject", solution);
            AddParameter("projectpath", solutionPath);

            // get the responses
            System.Net.HttpWebResponse webResponse = SendRESTRequest("/rest/management/codelink/connect", false);
            bool parseSuccessful = false;
            if (webResponse != null)
            {
                parseSuccessful = ParseXMLResponse(webResponse);
                webResponse.Close();
            }
            return parseSuccessful && (sessionId != -1);
        }

        public bool CodeLinkResponse(int responseCode)
        {
            ClearParameters();
            AddParameter("sessionid", sessionId);
            AddParameter("responsecode", responseCode);

            System.Net.HttpWebResponse webResponse = SendRESTRequest("/rest/management/codelink/response", false);
            webResponse.Close();

            return true;
        }
        #endregion

        private long sessionId = -1;
        private bool versionMatched = false;
        private bool timedOut = true;
        private String methodName = null;
        private String className = null;
        private String[] attributes = null;

        public long SessionId { get { return sessionId; } }
        public String LookupMethodName { get { return methodName; } }
        public String LookupClassName { get { return className; } }
        public String[] LookupAttributes { get { return attributes; } }
        public bool VersionMatched { get { return versionMatched; } }
        public bool TimedOut { get { return timedOut; } }

        public Lookup GetLookupInformation()
        {
            if (!timedOut && methodName != null && methodName.Length > 0)
                return new Lookup(className, methodName, attributes);

            return null;
        }
    }
}