namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    using System;

    /// <summary>
    /// To record the conversation last active time and subject user selected
    /// </summary>
    public class ConversationInfo
    {
        /// <summary>
        /// Gets or sets last active time, used to clear conversation state when expired
        /// </summary>
        public DateTime LastActiveTime { get; set; }

        /// <summary>
        ///  Gets or sets subject user selected, used to narrow down search in QnA maker
        /// </summary>
        public string SubjectSelected { get; set; }
    }
}
