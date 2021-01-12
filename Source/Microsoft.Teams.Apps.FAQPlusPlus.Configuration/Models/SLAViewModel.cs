// <copyright file="SLAViewModel.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Configuration.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents SLAViewModel object that hold welcome message text.
    /// </summary>
    public class SLAViewModel
    {
        /// <summary>
        /// Gets or sets welcome message text box to be used in View.
        /// </summary>
        [Required(ErrorMessage = "Enter a number.")]
        [MinLength(1)]
        [DataType(DataType.Text)]
        [Display(Name = "AssignTimeOut")]
        public string AssignTimeOut { get; set; }

        /// <summary>
        /// Gets or sets welcome message text box to be used in View.
        /// </summary>
        [Required(ErrorMessage = "Enter a number.")]
        [MinLength(1)]
        [DataType(DataType.Text)]
        [Display(Name = "ResolveTimeOut")]
        public string ResolveTimeOut { get; set; }

        /// <summary>
        /// Gets or sets welcome message text box to be used in View.
        /// </summary>
        [Required(ErrorMessage = "Enter a number.")]
        [MinLength(1)]
        [DataType(DataType.Text)]
        [Display(Name = "PendingTimeOut")]
        public string PendingTimeOut { get; set; }
    }
}