using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CASimulator
{
    /// <summary>
    /// Represents a rule in a cellular automaton.
    /// </summary>
    public struct CARule
    {
        public string prefix;
        public int lhs;
        public int rhs;

        /// <summary>
        /// Defines the rule in parts.
        ///   <remarks>
        ///     An rhs of -1 says to use the current cell value.
        ///   </remarks>
        /// </summary>
        /// <param name="prefix">
        /// determines the type of rule. See CASim.
        /// </param>
        /// <param name="lhs">
        /// the value on the left-hand side, which works with the prefix.
        /// </param>
        /// <param name="rhs">
        /// the value on the right-hand side, which determines the outcome.
        /// </param>
        public CARule(string prefix, int lhs, int rhs)
        {
            this.prefix = prefix;
            this.lhs = lhs;
            this.rhs = rhs;
        }
    }
}
