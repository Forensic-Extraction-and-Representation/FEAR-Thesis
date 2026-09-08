using Antlr4.Runtime.Misc;
using FEAR.Domain.GraphCodifier;
using FEAR.Domain.KnowledgeGraph;
using FEAR.Domain.KnowledgeGraph.GraphCodify;
using FEAR.Runtime.Helpers;
using FEAR.Runtime.TranspilerServices;
using FEAR.Runtime.TranspilerServices.Expressions;
using FEAR.Runtime.TranspilerServices.Variables;
using System.Text;
using CSTH = FEAR.Runtime.Helpers.CSharpTranspilerHelpers;

namespace FEAR.GFEAR.Transpiler
{
    /// <summary>
    /// Implements the core logic for visiting and transpiling GFEAR codifier blocks into C# code.
    /// This transpiler is responsible for generating C# classes that implement graph codify scripts.
    /// 
    /// The generated scripts process data that is queued for knowledge extraction, which can be either:
    ///   - An artifact directly (raw evidence or input data)
    ///   - The result of a collector (CFEAR) script
    /// 
    /// The transpiler ensures that all relevant data, regardless of its source, is processed
    /// by the appropriate graph codifiers for further transformation or enrichment.
    /// 
    /// In the FEAR pipeline, this means the code generated here will process items from the queue
    /// that are either direct artifacts or the output of a collector script, and transform them
    /// into knowledge graph entities, properties, and relationships.
    /// </summary>
    public partial class CSharpTranspiler
    {
        // Tracks the current indentation level for generated code blocks.
        int executableCodeIndentation = 0;

        /// <summary>
        /// Writes a line to the StringBuilder with the specified indentation.
        /// </summary>
        protected void WriteLine(StringBuilder sb, int tabs, string line)
        {
            sb.AppendLine(CSTH.Tab(tabs) + line);
        }

        /// <summary>
        /// Visits the codifier definition block and generates the C# class structure for a graph codifier.
        /// </summary>
        /// <remarks>
        /// The generated codifier class will process queued data (artifacts or collector results) and
        /// implement the logic for transforming them into graph entities and relationships.
        /// </remarks>
        public override object VisitCodifierDefinitionBlock([NotNull] GFEARParser.CodifierDefinitionBlockContext context)
        {
            // Write using statements and scaffold the class
            WriteLine(_classWrapperOutput, 0, @"using System;");
            WriteLine(_classWrapperOutput, 0, @"using System.IO;");
            WriteLine(_classWrapperOutput, 0, @"using System.Collections.Generic;");

            WriteLine(_classWrapperOutput, 0, @"using FEAR.Domain.GraphCodifier;");
            WriteLine(_classWrapperOutput, 0, @"using FEAR.GFEAR;");
            WriteLine(_classWrapperOutput, 0, @"using FEAR.Domain.KnowledgeGraph;");
            WriteLine(_classWrapperOutput, 0, @"using FEAR.Domain.KnowledgeGraph.GraphCodify;");
            WriteLine(_classWrapperOutput, 0, @"using FEAR.Domain.KnowledgeGraph.EntitySearch;");

            var cDefStmt = context.codifierDefinitionStatement();
            var codifierName = cDefStmt.codifierName.Text;
            var codifierCategory = cDefStmt.codifierCategory.Text;
            var codifierNamespace = codifierCategory.Replace("\"", "").Replace("/", ".");
            var className = codifierName.Replace("\"", "").Replace("+", "");
            _assemblyManager.AssemblyName = codifierNamespace + ".dll";

            // Begin namespace and class definition
            WriteLine(_classWrapperOutput, 0, $"namespace FEAR.GraphCodifiers.Compiled.{codifierNamespace}");
            WriteLine(_classWrapperOutput, 0, $"{{");
            WriteLine(_classWrapperOutput, 1, $@"[{nameof(FEARGraphCodifierAttribute)}({codifierCategory}, {codifierName})]");
            WriteLine(_classWrapperOutput, 1, $@"public class {className} : {nameof(GraphCodifierBase)}");
            WriteLine(_classWrapperOutput, 1, $"{{");
            WriteLine(_classWrapperOutput, 2, $@"public override string {nameof(GraphCodifierBase.GraphCodifierCategory)} => {codifierCategory};");
            WriteLine(_classWrapperOutput, 2, $@"public override string {nameof(GraphCodifierBase.CodifierName)} => {codifierName};");

            // Accepts statement: defines which properties this codifier can process
            var aStmt = context.acceptsStatement();
            if (aStmt != null)
            {
                var accepts = aStmt.IDENTIFIER().ToList();
                var acceptsProperties = String.Join(", ", accepts.Select((a) => $@"""{a.GetText()}"""));
                WriteLine(_classWrapperOutput, 2, $@"public override string[] {nameof(GraphCodifierBase.AcceptsProperties)} => new string[] {{ {acceptsProperties} }};");
            }
            else
            {
                WriteLine(_classWrapperOutput, 2, $@"public override string[] {nameof(GraphCodifierBase.AcceptsProperties)} => new string[] {{ }};");
            }

            // Ontology imports and namespace prefixes
            var oStmt = context.ontologyStatement();
            var ontologies = oStmt.ToList();
            ontologies.ForEach(x =>
            {
                Context.TypeManager.AddPrefix(x.ontPrefix.Text.Replace("\"", ""), x.ontUrl.Text.Replace("\"", ""));
            });

            string ontologyImports = String.Join("," + Environment.NewLine, ontologies.Select((a) => $@"{CSTH.Tab(3)}{{{a.ontPrefix.Text}, new Uri({a.ontUrl.Text})}}"));
            WriteLine(_classWrapperOutput, 2, $@"public override Dictionary<string, Uri> {nameof(GraphCodifierBase.OntologyNamespaceImports)} => new Dictionary<string, Uri>()");
            WriteLine(_classWrapperOutput, 2, $"{{");
            WriteLine(_classWrapperOutput, 0, $@"{ontologyImports}");
            WriteLine(_classWrapperOutput, 2, $@"}};");
            WriteLine(_classWrapperOutput, 0, "");

            // Include and prefix statements
            var iStmt = context.includeStatement();
            var includes = String.Join(Environment.NewLine, iStmt.IDENTIFIER().Select((a) => $@"{CSTH.Tab(3)}{nameof(GraphCodifierBase.AddDefaultNamespace)}(""{a.GetText()}"");"));
            var pStmt = context.prefixStatement();
            var prefixes = String.Join(Environment.NewLine, pStmt.Select((a) => $@"{CSTH.Tab(3)}{nameof(GraphCodifierBase.AddNamespacePrefix)}({a.prefixLabel.Text}, new Uri({a.prefixUri.Text}));"));

            Array.ForEach(iStmt.IDENTIFIER(), x =>
            {
                Context.TypeManager.AddPrefix(x.GetText(), GraphCodifierBase.ResolveDefaultNamespace(x.GetText()));
            });

            Array.ForEach(pStmt, x =>
            {
                Context.TypeManager.AddPrefix(x.prefixLabel.Text.Replace("\"", ""), x.prefixUri.Text.Replace("\"", ""));
            });

            // Constructor for the generated codifier class
            WriteLine(_classWrapperOutput, 2, $@"public {className}()");
            WriteLine(_classWrapperOutput, 2, $"{{");
            WriteLine(_classWrapperOutput, 0, $@"{includes}");
            WriteLine(_classWrapperOutput, 0, $@"{prefixes}");
            WriteLine(_classWrapperOutput, 2, $"}}");
            WriteLine(_classWrapperOutput, 0, "");

            // Main Execute method for the codifier
            WriteLine(_classWrapperOutput, 2, $@"public override void {nameof(GraphCodifierBase.Execute)}({nameof(IGraphCodifierContext)} context)");
            WriteLine(_classWrapperOutput, 2, $"{{");

            // Placeholder for the main execution code
            WriteLine(_classWrapperOutput, 0, $@"@EXECUTE_CODE@");
            WriteLine(_classWrapperOutput, 2, $@"}}");
            WriteLine(_classWrapperOutput, 1, $@"}}");
            WriteLine(_classWrapperOutput, 0, $@"}}");

            executableCodeIndentation = 3;
            return null;
        }

        /// <summary>
        /// Visits a result entity block, generating code to create and process an entity from queued data.
        /// The data processed here is either a direct artifact or the result of a collector (CFEAR) script.
        /// </summary>
        public override object VisitResultEntityBlock([NotNull] GFEARParser.ResultEntityBlockContext context)
        {
            var resultName = context.resultName.Text;
            var resultType = context.typeUri.GetText();
            var eIriClause = context.iriFormatClause();
            EntityTypeMeta etm = Context.TypeManager.AddEntityType(resultName, resultType);

            string iriSegmentFormatter = "";
            if (eIriClause != null)
            {
                iriSegmentFormatter = ParseIriSegmentFormatClause(eIriClause);
            }

            // Generate code to create an ephemeral entity for the result
            WriteLine(_graphCodifierCodeOutput, executableCodeIndentation, $@"{nameof(Entity)} {etm.EphemeralName} = context.{nameof(GraphCodifierContext.GraphService)}.{nameof(IGraphCodifyService.CreateEphemeralEntity)}(""{etm.Name}"",""{etm.Type}"" {iriSegmentFormatter});");

            var resultVariable = _variableManager.AddRootVariable(etm.Name, etm.EphemeralName, VariableContext.VariableSource.Result);

            _variableManager.AddScope(context.Start.Line.ToString(), etm.Name);

            _variableManager.Current.CurrentEntityVariableContext = resultVariable;
            context.block().blockStatement().ToList().ForEach((a) =>
            {
                VisitBlockStatement(a);
            });

            var idClause = context.identifiedByClauseBlock();
            if(idClause != null)
                VisitIdentifiedByClauseBlock(context.identifiedByClauseBlock());

            _variableManager.ExitScope();

            // Finalize the entity by finding or updating it in the graph
            WriteLine(_graphCodifierCodeOutput, executableCodeIndentation, @$"context.{nameof(GraphCodifierContext.FindOrUpdateEntity)}({resultVariable.TranslatedName});");
            return null;
        }

        /// <summary>
        /// Parses an IRI segment format clause for use in entity or relationship creation.
        /// </summary>
        public string ParseIriSegmentFormatClause([NotNull] GFEARParser.IriFormatClauseContext context)
        {
            if (context.children.Count > 0)
            {
                var iri = context.children[1].GetText();
                string iriFormat = $@"{nameof(IriSegmentFormatter)}.{nameof(IriSegmentFormatter.Parse)}({iri}, context.{nameof(GraphCodifierContext.Data)})";
                if (!String.IsNullOrEmpty(iriFormat))
                    return $@", {iriFormat}";
            }

            return "";
        }

        // The following methods visit and transpile various GFEAR script statements into C# code.
        // These statements define how queued data (artifacts or collector results) are processed
        // and transformed into graph entities, properties, and relationships.
        public override object VisitCreateCollectionStatement([NotNull] GFEARParser.CreateCollectionStatementContext context)
        {
            var collectionType = context.collectionType.GetText();
            var expectedType = context.expectedType;
            CollectionPropertyTypeMeta ptm = new CollectionPropertyTypeMeta(context.entityName.Text, collectionType, expectedType.GetText());
            Context.TypeManager.AddProperty(CurrentEntityScopeType.Name, ptm.Name, ptm);

            // Add new variable for the collection to the scope
            var collectionVariable = _variableManager.AddVariable(ptm.Name, ptm.EphemeralName, VariableContext.VariableSource.Variable);
            collectionVariable.Type = collectionType;

            // Supported types: rdf:Bag, rdf:Seq, rdf:Alt, rdf:List
            if (collectionType != "rdf:Bag" && collectionType != "rdf:Seq" && collectionType != "rdf:Alt" && collectionType != "rdf:List")
            {
                _transpileErrors.Add(new FEARParseError(context.Start.Line, context.Start.Column, $"Collection type {collectionType} is not supported. Use 'rdf:List' or one of 'rdf:Bag', 'rdf:Seq', or 'rdf:Alt'.", context.GetText(), FEARErrorType.TypeMismatch));
            }
            else
                if (expectedType != null)
                WriteLine(_graphCodifierCodeOutput, executableCodeIndentation, $"var {collectionVariable.TranslatedName} = context.{nameof(GraphCodifierContext.GraphService)}.{nameof(IGraphCodifyService.CreateEphemeralCollection)}(\"{collectionType}\", \"{expectedType.GetText()}\");");
            else
                WriteLine(_graphCodifierCodeOutput, executableCodeIndentation, $"var {collectionVariable.TranslatedName} = context.{nameof(GraphCodifierContext.GraphService)}.{nameof(IGraphCodifyService.CreateEphemeralCollection)}(\"{collectionType}\");");
            return base.VisitCreateCollectionStatement(context);
        }

        public override object VisitAppendEntityStatement([NotNull] GFEARParser.AppendEntityStatementContext context)
        {
            var collectionVariable = _variableManager.GetObjectContext(context.collectionName.Text);
            var entityVariable = _variableManager.GetObjectContext(context.entityName.Text);
            if (entityVariable == null)
                _transpileErrors.Add(new FEARParseError(context.Start.Line, context.Start.Column, $"Entity {context.entityName.Text} does not exist", context.GetText(), FEARErrorType.Semantic));

            if (collectionVariable == null)
                _transpileErrors.Add(new FEARParseError(context.Start.Line, context.Start.Column, $"Collection {context.collectionName.Text} does not exist", context.GetText(), FEARErrorType.Semantic));

            if (collectionVariable != null && entityVariable != null)
                WriteLine(_graphCodifierCodeOutput, executableCodeIndentation, $"{collectionVariable.TranslatedName}.{nameof(CollectionProperty.AppendEntity)}({entityVariable.TranslatedName});");
            return base.VisitAppendEntityStatement(context);
        }

        public override object VisitAppendPropertyRelationshipStatement([NotNull] GFEARParser.AppendPropertyRelationshipStatementContext context)
        {
            var collectionVariable = _variableManager.GetObjectContext(context.collectionName.Text);
            var eIriClause = context.iriFormatClause();

            string iriSegmentFormatter = "";
            if (eIriClause != null)
            {
                iriSegmentFormatter = ParseIriSegmentFormatClause(eIriClause);
            }

            if (collectionVariable == null)
                _transpileErrors.Add(new FEARParseError(context.Start.Line, context.Start.Column, $"Collection {context.collectionName.Text} does not exist", context.GetText(), FEARErrorType.Semantic));

            if (collectionVariable != null)
                WriteLine(_graphCodifierCodeOutput, executableCodeIndentation, $"{collectionVariable.TranslatedName}.{nameof(CollectionProperty.AppendRelationship)}({iriSegmentFormatter});");
            return base.VisitAppendPropertyRelationshipStatement(context);
        }

        public override object VisitAppendPropertyValueStatement([NotNull] GFEARParser.AppendPropertyValueStatementContext context)
        {
            var collectionVariable = _variableManager.GetObjectContext(context.collectionName.Text);
            var valueName = context.valueName;
            string valueStr = "";
            var expr = valueName as GFEARParser.ExpressionContext;
            _expressionContext = new TranspilerExpressionContext(context.Start.Line.ToString());

            VisitExpression(expr);
            valueStr = _expressionContext.Root.GetExpressionTree();

            _expressionContext = null;

            if (collectionVariable == null)
                _transpileErrors.Add(new FEARParseError(context.Start.Line, context.Start.Column, $"Collection {context.collectionName.Text} does not exist", context.GetText(), FEARErrorType.Semantic));

            if (collectionVariable != null)
                WriteLine(_graphCodifierCodeOutput, executableCodeIndentation, $"{collectionVariable.TranslatedName}.{nameof(CollectionProperty.AppendLiteral)}({valueStr});");
            return base.VisitAppendPropertyValueStatement(context);
        }

        public override object VisitStatement([NotNull] GFEARParser.StatementContext context)
        {
            if (context.Foreach() != null)
            {
                // Handles foreach blocks for iterating over collections in the codifier script
                var foreachControl = context.foreachControl();
                var variable = foreachControl.identifier().GetText();
                var collection = foreachControl.collectionExpression.GetText();
                var collectionVariable = ResolveVariable(collection);
                ControlBlockVariables.Add(variable);
                WriteLine(_graphCodifierCodeOutput, executableCodeIndentation, "");
                WriteLine(_graphCodifierCodeOutput, executableCodeIndentation, $@"var {variable}TempList = {collectionVariable};");
                WriteLine(_graphCodifierCodeOutput, executableCodeIndentation, $@"if({variable}TempList != null && {variable}TempList is System.Collections.IEnumerable)");
                WriteLine(_graphCodifierCodeOutput, executableCodeIndentation, $"{{");
                executableCodeIndentation++;
                WriteLine(_graphCodifierCodeOutput, executableCodeIndentation, $@"foreach(var {variable}X in {variable}TempList)");
                WriteLine(_graphCodifierCodeOutput, executableCodeIndentation, $"{{");
                executableCodeIndentation++;
                WriteLine(_graphCodifierCodeOutput, executableCodeIndentation, $"dynamic {variable} = new {nameof(NullingExpandoObject)}({variable}X);");
                var statements = context.statement();
                foreach (var x in statements)
                {
                    VisitStatement(x);
                }
                executableCodeIndentation--;
                WriteLine(_graphCodifierCodeOutput, executableCodeIndentation, $"}}");

                executableCodeIndentation--;
                WriteLine(_graphCodifierCodeOutput, executableCodeIndentation, $"}}");
                WriteLine(_graphCodifierCodeOutput, executableCodeIndentation, "");
                ControlBlockVariables.Remove(variable);
                return null;
            }

            return base.VisitStatement(context);
        }

        public override object VisitCreateEntityStatement([NotNull] GFEARParser.CreateEntityStatementContext context)
        {
            Antlr4.Runtime.IToken eName = context.entityName;
            GFEARParser.EntityUriContext eType = context.entityType;
            GFEARParser.IdentifiedByClauseBlockContext eIdentifiedBy = context.identifiedByClauseBlock();
            GFEARParser.IriFormatClauseContext eIriClause = context.iriFormatClause();
            EntityTypeMeta etm = Context.TypeManager.AddEntityType(eName.Text, eType.GetText());

            var ephemeralVariable = _variableManager.AddVariable(etm.Name, etm.EphemeralName, VariableContext.VariableSource.Variable);

            string iriSegmentFormatter = "";
            if (eIriClause != null)
            {
                iriSegmentFormatter = ParseIriSegmentFormatClause(eIriClause);
            }

            WriteLine(_graphCodifierCodeOutput, executableCodeIndentation, $@"{nameof(Entity)} {ephemeralVariable.TranslatedName} = context.{nameof(GraphCodifierContext.GraphService)}.{nameof(IGraphCodifyService.CreateEphemeralEntity)}(""{etm.Name}"", ""{etm.Type}"" {iriSegmentFormatter});");

            // BLOCKS
            _variableManager.AddScope(context.Start.Line.ToString(), etm.Name);
            _variableManager.Current.CurrentEntityVariableContext = ephemeralVariable;
            _entityPropertyManager.AddScope(context.Start.Line.ToString(), etm.Name);
            context.block().blockStatement().ToList().ForEach((a) =>
            {
                VisitBlockStatement(a);
            });

            if (context.identifiedByClauseBlock() != null)
            {
                VisitIdentifiedByClauseBlock(context.identifiedByClauseBlock());
            }

            _entityPropertyManager.ExitScope();
            _variableManager.ExitScope();

            return null;
        }

        public override object VisitIdentifiedByClauseBlock([NotNull] GFEARParser.IdentifiedByClauseBlockContext context)
        {
            return base.VisitIdentifiedByClauseBlock(context);
        }

        public override object VisitIdentifiedByPropertyClause([NotNull] GFEARParser.IdentifiedByPropertyClauseContext context)
        {
            var condition = context.condition.Text;
            var propertyUri = context.propertyUri.GetText();
            if (_entityPropertyManager.ObjectContextExists(propertyUri))
                WriteLine(_graphCodifierCodeOutput, executableCodeIndentation, $@"{_variableManager.Current.CurrentEntityVariableContext.TranslatedName}.{nameof(Entity.IdentifiedByProperty)}({nameof(IdentifiedByCondition)}.{condition.FirstCharToUpper()}, ""{propertyUri}"");");
            else
            {
                _transpileErrors.Add(new FEARParseError(context.Start.Line, context.Start.Column, $"Property {propertyUri} does not exist on entity {_variableManager.Current.CurrentEntityVariableContext.PropertyName}", context.GetText(), FEARErrorType.MissingProperty));
            }
            return null;
        }

        public override object VisitIdentifiedByEntityClause([NotNull] GFEARParser.IdentifiedByEntityClauseContext context)
        {
            var condition = context.condition.Text;
            var propertyUri = context.propertyUri.GetText();
            var entityName = context.entityName.Text;
            if (_entityPropertyManager.ObjectContextExists(propertyUri))
                WriteLine(_graphCodifierCodeOutput, executableCodeIndentation, $@"{_variableManager.Current.CurrentEntityVariableContext.TranslatedName}.{nameof(Entity.IdentifiedByEntity)}({nameof(IdentifiedByCondition)}.{condition.FirstCharToUpper()}, ""{propertyUri}"", {entityName}__ephemeral);");
            else
            {
                _transpileErrors.Add(new FEARParseError(context.Start.Line, context.Start.Column,
                    $"Property {propertyUri} does not exist on entity {_variableManager.Current.CurrentEntityVariableContext.PropertyName}",
                    context.GetText(), FEARErrorType.MissingProperty));
            }
            return null;
        }

        public override object VisitCreatePropertyEntityStatement([NotNull] GFEARParser.CreatePropertyEntityStatementContext context)
        {
            var propertyNameUri = context.propertyNameUri.GetText();
            var entity = context.entityName.Text;
            _entityPropertyManager.AddObjectContext(propertyNameUri, "");

            EntityTypeMeta targetEntityType = Context.TypeManager.GetEntityType(entity);
            PropertyTypeMeta ptm = new PropertyTypeMeta(propertyNameUri, targetEntityType.Type);
            ptm.RelationType = RelationType.Object;
            Context.TypeManager.AddProperty(CurrentEntityScopeType.Name, propertyNameUri, ptm);

            WriteLine(_graphCodifierCodeOutput, executableCodeIndentation, $@"context.{nameof(GraphCodifierContext.GraphService)}.{nameof(IGraphCodifyService.AddProperty)}({_variableManager.Current.CurrentEntityVariableContext.TranslatedName}, ""{propertyNameUri}"", {targetEntityType.EphemeralName});");

            return null;
        }

        public override object VisitCreatePropertyCollectionStatement([NotNull] GFEARParser.CreatePropertyCollectionStatementContext context)
        {
            var relUri = context.propertyNameUri.GetText();
            VariableContext collectionVariable = _variableManager.GetObjectContext(context.collectionName.Text);
            _entityPropertyManager.AddObjectContext(relUri, "");

            PropertyTypeMeta ptm = new PropertyTypeMeta(relUri, collectionVariable.Type);
            ptm.RelationType = RelationType.Collection;
            Context.TypeManager.AddProperty(CurrentEntityScopeType.Name, relUri, ptm);

            WriteLine(_graphCodifierCodeOutput, executableCodeIndentation, $@"context.{nameof(GraphCodifierContext.GraphService)}.{nameof(IGraphCodifyService.AddCollectionAsProperty)}({_variableManager.Current.CurrentEntityVariableContext.TranslatedName}, ""{relUri}"", {collectionVariable.TranslatedName});");

            return null;
        }

        public override object VisitLiteral([NotNull] GFEARParser.LiteralContext context)
        {
            var exprScope = _expressionContext.Current;
            var child = context.GetChild(0);
            exprScope.ExpressionCode = context.GetText();

            return null;
        }

        public override object VisitIdentifier([NotNull] GFEARParser.IdentifierContext context)
        {
            var exprScope = _expressionContext.Current;
            if (_expressionContext.Current.Side == ExpressionScope.BopSide.Left)
                exprScope.ExpressionCode = ResolveVariable(context.GetText());
            else
                exprScope.ExpressionCode = context.GetText();

            return null;
        }

        public override object VisitPrimary([NotNull] GFEARParser.PrimaryContext context)
        {
            var exprScope = _expressionContext.Current;
            var child = context.GetChild(0);
            if (child is GFEARParser.LiteralContext)
            {
                exprScope.ExpressionCode = context.GetText();
            }
            else if (child is GFEARParser.IdentifierContext)
            {
                if (_expressionContext.Current.Side == ExpressionScope.BopSide.Left)
                    exprScope.ExpressionCode = ResolveVariable(context.GetText());
                else
                    exprScope.ExpressionCode = context.GetText();
            }
            else
            {
                exprScope.ExpressionCodePrefix = "(";
                exprScope.ExpressionCodeSuffix = ")";
                // ExpressonContext
                Visit(child);
            }

            return null;
        }

        public override object VisitExpression([NotNull] GFEARParser.ExpressionContext context)
        {
            var parent = _expressionContext.Current;

            if (context.bop != null)
            {
                _expressionContext.Current.ExpressionCode = "?" + context.bop.Text;

                var exprScope = parent.AddLeftScope(context.Start.Line.ToString());
                _expressionContext.Current = exprScope;

                Visit(context.GetChild(0));

                exprScope = parent.AddRightScope(context.Start.Line.ToString());
                _expressionContext.Current = exprScope;
                Visit(context.GetChild(2));
            }
            else
            {
                var exprScope = parent.AddLeftScope(context.Start.Line.ToString());
                _expressionContext.Current = exprScope;

                Visit(context.GetChild(0));
            }

            _expressionContext.Current = _expressionContext.Current.ParentScope;

            return null;
        }

        public override object VisitCreatePropertyValueStatement([NotNull] GFEARParser.CreatePropertyValueStatementContext context)
        {
            // Add properties to the current entity property manager
            var relUri = context.propertyNameUri.GetText();
            var typeUri = context.typeUri?.GetText() ?? null;
            var valueName = context.valueName;
            string valueStr = "";
            var expr = valueName as GFEARParser.ExpressionContext;
            _expressionContext = new TranspilerExpressionContext(context.Start.Line.ToString());

            PropertyTypeMeta ptm = new PropertyTypeMeta(relUri, typeUri);
            Context.TypeManager.AddProperty(CurrentEntityScopeType.Name, relUri, ptm);

            VisitExpression(expr);
            valueStr = _expressionContext.Root.GetExpressionTree();

            _expressionContext = null;

            _entityPropertyManager.AddObjectContext(relUri, "");
            WriteLine(_graphCodifierCodeOutput, executableCodeIndentation, "");
            WriteLine(_graphCodifierCodeOutput, executableCodeIndentation, "//" + context.Start.Text);
            WriteLine(_graphCodifierCodeOutput, executableCodeIndentation, $@"context.{nameof(GraphCodifierContext.GraphService)}.{nameof(IGraphCodifyService.AddProperty)}({_variableManager.Current.CurrentEntityVariableContext.TranslatedName}, ""{relUri}"", ""{typeUri}"",{valueStr});");

            return null;
        }

        public override object VisitCreatePropertyRelationshipStatement([NotNull] GFEARParser.CreatePropertyRelationshipStatementContext context)
        {
            var propertyUri = context.propertyNameUri.GetText();
            var eIriClause = context.iriFormatClause();

            string iriSegmentFormatter = "";
            if (eIriClause != null)
            {
                iriSegmentFormatter = ParseIriSegmentFormatClause(eIriClause);
            }
            _entityPropertyManager.AddObjectContext(propertyUri, "");

            // Need to resolve the Uri and determine if it's an rdf:a
            bool isRdfA = false;
            if (isRdfA)
            {
                Context.TypeManager.AddEntityType(CurrentEntityScopeType.Name, iriSegmentFormatter);
            }

            WriteLine(_graphCodifierCodeOutput, executableCodeIndentation, $@"context.{nameof(GraphCodifierContext.GraphService)}.{nameof(IGraphCodifyService.AddRelationship)}({_variableManager.Current.CurrentEntityVariableContext.TranslatedName}, ""{propertyUri}"", new Uri(""{iriSegmentFormatter}""));");

            return null;
        }

        public override object VisitCreatePropertyRelationshipUriStatement([NotNull] GFEARParser.CreatePropertyRelationshipUriStatementContext context)
        {
            var propertyNameUri = context.propertyNameUri.GetText();
            var relUri = context.relUri.GetText();

            // Need to resolve the Uri and determine if it's an rdf:a
            bool isRdfA = false;
            if (isRdfA)
            {
                Context.TypeManager.AddEntityType(CurrentEntityScopeType.Name, relUri);
            }

            WriteLine(_graphCodifierCodeOutput, executableCodeIndentation, $@"context.{nameof(GraphCodifierContext.GraphService)}.{nameof(IGraphCodifyService.AddRelationship)}({_variableManager.Current.CurrentEntityVariableContext.TranslatedName}, ""{propertyNameUri}"", ""{relUri}"");");

            return null;
        }

    }
}
