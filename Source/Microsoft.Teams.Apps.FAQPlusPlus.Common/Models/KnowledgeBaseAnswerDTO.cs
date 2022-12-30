using Newtonsoft.Json;
using System.Collections.Generic;

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{

    /// <summary>
    /// This KnowledgeBaseAnswerDTO entity is used for azure search.
    /// </summary>
    public class KnowledgeBaseAnswerDTO
    {
        /// <summary>
        /// Gets or sets the questions.
        /// </summary>
        public IList<string> Questions { get; set; }

        /// <summary>
        /// Gets or sets the Answer.
        /// </summary>
        [JsonProperty("Answer")]
        public string Answer { get; set; }

        /// <summary>
        /// Gets or sets the Confidence.
        /// </summary>
        public double? Confidence { get; set; }

        /// <summary>
        /// Gets or sets QnaId.
        /// </summary>
        [JsonProperty("Id")]
        public int? QnaId { get; set; }

        /// <summary>
        /// Gets or sets the CreatedDate.
        /// </summary>
        [JsonProperty("CpdatedDateTime")]
        public string CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the UpdatedDate.
        /// </summary>
        [JsonProperty("LastUpdatedDateTime")]
        public string UpdatedDate { get; set; }

        /// <summary>
        /// Gets or sets the Source.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the Metadata.
        /// </summary>
        public IDictionary<string, string> Metadata { get; set; }

    }
}
