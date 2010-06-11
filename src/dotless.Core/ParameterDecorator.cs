namespace dotless.Core
{
    using System.Collections.Generic;
    using System.Text;
    using Parameters;

    public class ParameterDecorator : ILessEngine
    {
        private readonly ILessEngine subEngine;
        private readonly IParameterSource parameterSource;

        public ParameterDecorator(ILessEngine subEngine, IParameterSource parameterSource)
        {
            this.subEngine = subEngine;
            this.parameterSource = parameterSource;
        }

        public string TransformToCss(string source, string fileName)
        {
            var sb = new StringBuilder();
            var parameters = parameterSource.GetParameters();
            foreach (var parameter in parameters)
            {
                sb.AppendFormat("@{0}: {1};\n", parameter.Key, parameter.Value);
            }
            sb.Append(source);
            return subEngine.TransformToCss(sb.ToString(), fileName);
        }

        public IEnumerable<string> GetImports()
        {
            return subEngine.GetImports();
        }
    }
}