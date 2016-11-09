using EnvDTE;

namespace DynaTrace.CodeLink
{
    class CodeFunctionProject
    {
        private string project;
        private CodeFunction function;

        public CodeFunctionProject(string project, CodeFunction function)
        {
            this.project = project;
            this.function = function;
        }

        public string Project
        {
            get { return project; }
            set { project = value; }
        }

        public CodeFunction Function
        {
            get { return function; }
            set { function = value; }
        }
    }
}
