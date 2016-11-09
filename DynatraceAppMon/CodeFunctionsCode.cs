using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Diagnostics;

namespace DynaTrace.CodeLink
{
    class CodeFunctionsCode
    {

        private CodeFunctionsCode() { }

        public static void Load(SelectMethodDialog form)
        {
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.ShowButton.Focus();
            form.Label1.Text = "The method " + '"' + form.MethodName + '"' + " was found " + form.CodeFunctions.Count + " times in the current solution";
            for (int i = 0; i < form.CodeFunctions.Count; i++)
            {
                CodeFunctionProject cfp = (CodeFunctionProject)form.CodeFunctions[i];
                form.ComboBox1.Items.Add(cfp.Project);
            }
            form.ComboBox1.SelectedIndex = 0;
            string link = "http://www.dynatrace.com";
            form.LinkLabel1.Links.Add(0, link.Length, link);
        }

        public static void Show(Context context, SelectMethodDialog form)
        {
            string selectedProject = form.ComboBox1.SelectedItem.ToString();

            CodeFunctionProject cfp = null;
            for (int i = 0; i < form.CodeFunctions.Count; i++)
            {
                cfp = (CodeFunctionProject)form.CodeFunctions[i];
                if (cfp.Project.Equals(selectedProject))
                {
                    break;
                }
            }
            context.markMethod(cfp.Function);
            form.Activate();
        }

        public static void Close(SelectMethodDialog codeFunctionForm)
        {
            codeFunctionForm.Close();
        }
    }
}
