namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    using System;
    using System.Collections.Generic;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// To record the conversation last active time and subject user selected.
    /// </summary>
    public class ConversationEntity : TableEntity
    {
        /// <summary>
        /// Gets or sets the unique conversation id.
        /// </summary>
        public string ConversationId { get; set; }

        /// <summary>
        ///  Gets or sets project metadata related to this QnA pari.
        /// </summary>
        public string Project { get; set; }

        /// <summary>
        ///  Gets or sets the question user asked.
        /// </summary>
        public string Question { get; set; }

        /// <summary>
        ///  Gets or sets the options user selected in multi-turn.
        /// </summary>
        public string Turns { get; set; }

        /// <summary>
        ///  Gets or sets the final answer.
        /// </summary>
        public string FinalAnswer { get; set; }

    }
}
