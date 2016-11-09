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

        public static Lookup parse(Context context, String str)
        {
            // split lookup string and namespace, class, name,... to variables
            string[] parts = str.Split(';');

            string typeName = parts[0];
            string methodName = parts[1];
            string[] parameters = parseParams(parts[2]);

            for (int i = 0; i < parameters.Length; i++)
            {
                parameters[i] = parameters[i].Replace("System.SByte", "sbyte");
                parameters[i] = parameters[i].Replace("System.Int16", "short");
                parameters[i] = parameters[i].Replace("System.Int32", "int");
                parameters[i] = parameters[i].Replace("System.Int64", "long");
                parameters[i] = parameters[i].Replace("System.Byte", "byte");
                parameters[i] = parameters[i].Replace("System.UInt16", "ushort");
                parameters[i] = parameters[i].Replace("System.UInt32", "uint");
                parameters[i] = parameters[i].Replace("System.UInt64", "ulong");
                parameters[i] = parameters[i].Replace("System.Boolean", "bool");
                parameters[i] = parameters[i].Replace("System.Char", "char");
                parameters[i] = parameters[i].Replace("System.Decimal", "decimal");
                parameters[i] = parameters[i].Replace("System.Single", "float");
                parameters[i] = parameters[i].Replace("System.Double", "double");
                parameters[i] = parameters[i].Replace("System.Object", "object");
                parameters[i] = parameters[i].Replace("System.String", "string");
                parameters[i] = parameters[i].Replace("&", "");

                // generic types like IList`1
                parameters[i] = Regex.Replace(parameters[i], @"`\d+", "");
            }


            Lookup res = new Lookup(typeName, methodName, parameters);
            context.log(Context.LOG_INFO + "looking up " + res.ToString());
            return res;

        }
    }
}
