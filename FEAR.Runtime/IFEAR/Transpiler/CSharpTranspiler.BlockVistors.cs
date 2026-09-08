using Antlr4.Runtime.Misc;
using FEAR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FEAR.IFEAR.Transpiler
{
    public partial class CSharpTranspiler
    {
        protected void WriteLine(StringBuilder sb, string line)
        {
            sb.AppendLine(line);
        }

        public override object VisitInterpreter_definition([NotNull] IFEARParser.Interpreter_definitionContext context)
        {
            var stringContexts = context.stringLiteral();

            AssemblyName = stringContexts[1].GetText().Trim('\"') + ".dll";
            // Scaffold the main aspects of the assembly to generate
            WriteLine(_classWrapperOutput, @"using System;");
            WriteLine(_classWrapperOutput, @"using System.IO;");

            WriteLine(_classWrapperOutput, @"using FEAR;");
            WriteLine(_classWrapperOutput, @"using FEAR.IFEAR;");

            WriteLine(_classWrapperOutput, $"namespace FEAR.Interpreters.Compiled.{stringContexts[1].GetText().Trim('\"')}{{");
            WriteLine(_classWrapperOutput, $@"public class Interpreter : InterpreterBase{{");
            WriteLine(_classWrapperOutput, $@"public override string InterpreterCategory => ""{stringContexts[0].GetText().Trim('\"')}"";");
            WriteLine(_classWrapperOutput, $@"public override string InterpreterName => ""{stringContexts[1].GetText().Trim('\"')}"";");

            WriteLine(_classWrapperOutput, $@"private string ReturnName => ""{stringContexts[2].GetText().Trim('\"')}"";");
            WriteLine(_classWrapperOutput, $@"public override void Execute(IInterpreterContext context){{");

            WriteLine(_classWrapperOutput, $@"@EXECUTE_CODE@");
            WriteLine(_classWrapperOutput, $@"}}");
            WriteLine(_classWrapperOutput, $@"}}");
            WriteLine(_classWrapperOutput, $@"}}");

            return null;
        }

        public override object VisitNumericLiteral([NotNull] IFEARParser.NumericLiteralContext context)
        {
            return string.Join("", context.children);
        }

        public override object VisitSkipStatement([NotNull] IFEARParser.SkipStatementContext context)
        {
            var x = context.children[2] as IFEARParser.NumericLiteralContext;

            var skipValue = VisitNumericLiteral(x);
            WriteLine(_interpreterCodeOutput, $@"context.Skip({skipValue});");
            return base.VisitSkipStatement(context);
        }

        public override object VisitStatement([NotNull] IFEARParser.StatementContext context)
        {
            // Convert the block into code
            if (context.skipStatement() != null)
            {
                var skip = context.skipStatement();
                VisitSkipStatement(skip);
            }

            if (context.interpreterStatement() != null)
            {
                var interpreterLine = context.interpreterStatement();

                string assignmentName = interpreterLine.children[0].GetText();
                int readCount = int.Parse(interpreterLine.children[3].GetText());
                var type = interpreterLine.GetRuleContexts<IFEARParser.Type_specifierContext>().First();
                var type_conversion = type.type_conversion();
                string option = interpreterLine.children[7].GetText();

                string readType = type_conversion != null ? type_conversion.children[0].GetText() : type.GetText();
                string convertType = type_conversion != null ? type_conversion.children[2].GetText() : null;

                if (readType == "INT" || readType == "UINT")
                {
                    string intVariant = readType == "INT" ? "Int" : "UInt";
                    var intermediateAssignmentName = convertType != null ? $"__{assignmentName}" : assignmentName;
                    var codeLine = $@"var {intermediateAssignmentName} = context.ReadAs{intVariant}({readCount}, {(option == "L").ToString().ToLower()});";

                    WriteLine(_interpreterCodeOutput, codeLine);

                    string convertCodeLine = "";
                    if (convertType != null)
                    {
                        if (convertType == "FILETIME")
                            convertCodeLine = $@"context.SetReturnObjectValue(""{assignmentName}"", FromFileTime({intermediateAssignmentName}));";
                        else if (convertType == "UNIXTIME")
                            convertCodeLine = $@"context.SetReturnObjectValue(""{assignmentName}"", FromUnixTimeSeconds({intermediateAssignmentName})?.DateTime);";
                        else if (convertType == "STRING")
                            convertCodeLine = $@"context.SetReturnObjectValue(""{assignmentName}"", {intermediateAssignmentName}.ToString());";
                        else
                            throw new Exception($"Unknown conversion type {convertType}");
                    }
                    else
                    {
                        convertCodeLine = $@"context.SetReturnObjectValue(""{assignmentName}"", {intermediateAssignmentName});";
                    }

                    WriteLine(_interpreterCodeOutput, convertCodeLine);
                }
                else if (readType == "STRING")
                {
                    var codeLine = $@"context.SetReturnObjectValue(""{assignmentName}"", context.ReadString({readCount}, {(option == "W").ToString().ToLower()}));";

                    WriteLine(_interpreterCodeOutput, codeLine);
                }
            }

            return null;
        }
    }
}
