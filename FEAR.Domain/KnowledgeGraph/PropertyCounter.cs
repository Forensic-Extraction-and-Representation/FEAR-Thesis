namespace FEAR.Domain.KnowledgeGraph
{
    /// <summary>
    /// Tracks the count of properties by their identification condition (Required, Optional, None).
    /// Useful for analyzing or validating the distribution of property requirements in a knowledge graph entity or schema.
    /// </summary>
    public class PropertyCounter
    {
        /// <summary>
        /// Gets or sets the count of properties marked as <see cref="IdentifiedByCondition.Required"/>.
        /// </summary>
        public int Required { get; set; }

        /// <summary>
        /// Gets or sets the count of properties marked as <see cref="IdentifiedByCondition.Optional"/>.
        /// </summary>
        public int Optional { get; set; }

        /// <summary>
        /// Gets or sets the count of properties marked as <see cref="IdentifiedByCondition.None"/>.
        /// </summary>
        public int None { get; set; }

        /// <summary>
        /// Increments the count for the specified <see cref="IdentifiedByCondition"/>.
        /// </summary>
        /// <param name="condition">The identification condition to increment.</param>
        public void AddCount(IdentifiedByCondition condition)
        {
            switch (condition)
            {
                case IdentifiedByCondition.Required:
                    Required++;
                    break;
                case IdentifiedByCondition.Optional:
                    Optional++;
                    break;
                case IdentifiedByCondition.None:
                    None++;
                    break;
            }
        }

        /// <summary>
        /// Returns a list containing the counts for Required, Optional, and None, in that order.
        /// </summary>
        public IList<int> Counts()
        {
            return new List<int>() { Required, Optional, None };
        }

        /// <summary>
        /// Returns a list of counts for the identification conditions that match the given predicate.
        /// </summary>
        /// <param name="condition">A predicate to select which conditions to include.</param>
        /// <returns>A list of counts for the selected conditions.</returns>
        public IList<int> Counts(Predicate<IdentifiedByCondition> condition)
        {
            List<int> t = new List<int>();
            if (condition(IdentifiedByCondition.Required))
            {
                t.Add(Required);
            }

            if (condition(IdentifiedByCondition.Optional))
            {
                t.Add(Optional);
            }

            if (condition(IdentifiedByCondition.None))
            {
                t.Add(None);
            }

            return t;
        }
    }
}