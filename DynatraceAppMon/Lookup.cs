using System;
using System.IO;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;


using Microsoft.Win32;

using EnvDTE;
using EnvDTE80;

namespace DynaTrace.CodeLink
{


    public class Lookup
    {

        public string typeName;
        public string methodName;
        public string[] parameters;

        public Lookup(string typeName, string methodName, string[] parameters)
        {
            this.typeName = typeName;
            this.methodName = methodName;
            this.parameters = parameters;
            fixGenericParameters();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(typeName);
            sb.Append("::");
            sb.Append(methodName);
            sb.Append("(");
            string sep = "";
            foreach (string parameter in parameters)
            {
                sb.Append(sep);
                sep = ";";
                sb.Append(parameter);
            }
            sb.Append(")");
            return sb.ToString();
        }

        private static string[] parseParams(string s)
        {

            if (s == "")
            {
                return new string[0];
            }

            int len = s.Length;
            ArrayList list = new ArrayList();
            int pos = 0;
            int d = 0;
            int a = 0;
            while (pos < len)
            {
                switch (s[pos])
                {
                    case ',':
                        if (d == 0 && a == 0)
                        {
                            list.Add(pos);
                        }
                        break;
                    case '<':
                        d++;
                        break;
                    case '>':
                        d--;
                        break;
                    case '[':
                        a++;
                        break;
                    case ']':
                        a--;
                        break;
                    default:
                        break;
                }
                pos++;
            }

            if (list.Count == 0)
            {
                return new string[] { s };
            }

            string[] res = new string[list.Count + 1];

            int start = 0;

            for (int i = 0; i < res.Length - 1; i++)
            {
                int cur = (int)list[i];
                res[i] = s.Substring(start, cur - start);
                start = cur + 1;
            }
            res[res.Length - 1] = s.Substring(start);
            return res;
        }

        private void fixGenericParameters()
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                // generic types like IList`1
                parameters[i] = Regex.Replace(parameters[i], @"`\d+", "");
            }
        }
    }
}
