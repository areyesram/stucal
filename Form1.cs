using System;
using System.CodeDom.Compiler;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.CSharp;

namespace StupidCalc
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnResult_Click(object sender, EventArgs e)
        {
            try
            {
                var expression = CoerceNumbers(txtFormula.Text);
                lblResult.Text = CalculateResult(expression).ToString(CultureInfo.CurrentCulture);
            }
            catch (Exception ex)
            {
                lblResult.Text = ex.Message;
            }
        }

        private static string CoerceNumbers(string expression)
        {
            var numericValue = new Regex(@"([\d\.]+)");
            var allDigits = new Regex(@"^\d+$");
            var sb = new StringBuilder();
            foreach (var s in numericValue.Split(expression))
            {
                sb.Append(s);
                if (allDigits.IsMatch(s)) sb.Append(".0");
            }
            return sb.ToString();
        }

        private static double CalculateResult(string expression)
        {
            var parameters = new CompilerParameters {GenerateInMemory = true, GenerateExecutable = false};
            var code = Resource.ClassTemplate.Replace("{EXPR}", expression);
            var assembly = new CSharpCodeProvider().CompileAssemblyFromSource(parameters, code);
            if (assembly.Errors.HasErrors)
            {
                var sb = new StringBuilder();
                foreach (CompilerError error in assembly.Errors)
                    sb.AppendLine(error.ErrorText);
                throw new InvalidOperationException(sb.ToString());
            }
            var result = assembly
                .CompiledAssembly.GetType("StupidCalc.Calculator")
                .GetMethod("GetResult")
                .Invoke(null, null);
            return (double) result;
        }
    }
}