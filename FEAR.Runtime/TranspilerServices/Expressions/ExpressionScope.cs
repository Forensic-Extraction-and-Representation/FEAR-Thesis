using System.Text;

namespace FEAR.Runtime.TranspilerServices.Expressions
{
    /// <summary>
    /// Represents a hierarchical scope for building and managing expression trees during transpilation.
    ///
    /// <para>
    /// <b>ExpressionScope</b> is used in the FEAR transpiler pipeline to construct, organize, and emit code for expressions
    /// found in graph codify scripts. Each scope can represent a subexpression, a binary operation, or a grouping (such as parentheses).
    /// </para>
    /// </summary>
    public class ExpressionScope
    {
        /// <summary>
        /// Indicates which side of a binary operation this scope represents.
        /// </summary>
        public enum BopSide
        {
            /// <summary>
            /// The left side of a binary operation.
            /// </summary>
            Left,
            /// <summary>
            /// The right side of a binary operation.
            /// </summary>
            Right,
            /// <summary>
            /// Not part of a binary operation.
            /// </summary>
            None
        }

        /// <summary>
        /// Gets or sets the name of this scope (e.g., for debugging or identification).
        /// </summary>
        public string ScopeName { get; set; }

        /// <summary>
        /// Gets or sets a prefix to prepend to the generated code for this expression (e.g., an opening parenthesis).
        /// </summary>
        public string ExpressionCodePrefix { get; internal set; }

        /// <summary>
        /// Gets or sets a suffix to append to the generated code for this expression (e.g., a closing parenthesis).
        /// </summary>
        public string ExpressionCodeSuffix { get; internal set; }

        /// <summary>
        /// Gets or sets the code fragment representing this expression (e.g., a literal, variable, or operator).
        /// </summary>
        public string ExpressionCode { get; set; }

        /// <summary>
        /// Gets or sets which side of a binary operation this scope represents.
        /// </summary>
        public BopSide Side { get; set; }

        /// <summary>
        /// Gets or sets the parent scope of this expression, or null if this is the root.
        /// </summary>
        public ExpressionScope ParentScope { get; set; }

        /// <summary>
        /// Gets the list of left child expression scopes (typically for the left side of binary operations).
        /// </summary>
        public List<ExpressionScope> LeftExpressions { get; set; } = new List<ExpressionScope>();

        /// <summary>
        /// Gets the list of right child expression scopes (typically for the right side of binary operations).
        /// </summary>
        public List<ExpressionScope> RightExpressions { get; set; } = new List<ExpressionScope>();

        /// <summary>
        /// Adds a new left child scope to this expression, representing the left side of a binary operation.
        /// </summary>
        /// <param name="identifier">A name or identifier for the new scope.</param>
        /// <returns>The created <see cref="ExpressionScope"/>.</returns>
        public ExpressionScope AddLeftScope(string identifier)
        {
            var scope = new ExpressionScope { ParentScope = this, ScopeName = identifier, Side = BopSide.Left };
            LeftExpressions.Add(scope);
            return scope;
        }

        /// <summary>
        /// Adds a new right child scope to this expression, representing the right side of a binary operation.
        /// </summary>
        /// <param name="identifier">A name or identifier for the new scope.</param>
        /// <returns>The created <see cref="ExpressionScope"/>.</returns>
        public ExpressionScope AddRightScope(string identifier)
        {
            var scope = new ExpressionScope { ParentScope = this, ScopeName = identifier, Side = BopSide.Right };
            RightExpressions.Add(scope);
            return scope;
        }

        /// <summary>
        /// Recursively builds and returns the full expression tree as a string of code.
        /// </summary>
        /// <returns>The generated code for the entire expression tree rooted at this scope.</returns>
        public string GetExpressionTree()
        {
            StringBuilder sb = new StringBuilder();
            if (LeftExpressions.Count > 0)
            {
                foreach (var expression in LeftExpressions)
                {
                    if (!string.IsNullOrEmpty(ExpressionCodePrefix))
                        sb.Append(ExpressionCodePrefix);

                    sb.Append(expression.GetExpressionTree());

                    if (!string.IsNullOrEmpty(ExpressionCodeSuffix))
                        sb.Append(ExpressionCodeSuffix);
                }
            }

            sb.Append(ExpressionCode);

            if (RightExpressions.Count > 0)
            {
                foreach (var expression in RightExpressions)
                {
                    sb.Append(expression.GetExpressionTree());
                }
            }

            return sb.ToString();
        }
    }
}
