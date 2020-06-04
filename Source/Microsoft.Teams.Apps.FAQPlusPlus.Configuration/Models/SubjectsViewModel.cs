// <copyright file="WelcomeMessageViewModel.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Configuration.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents WelcomeMessageViewModel object that hold welcome message text.
    /// </summary>
    public class SubjectsViewModel
    {
        /// <summary>
        /// Gets or sets welcome message text box to be used in View.
        /// </summary>
        [Required(ErrorMessage = "Enter a subjects.")]
        [MinLength(1)]
        [DataType(DataType.Text)]
        [Display(Name = "Subjects")]
        public string Subjects { get; set; }
    }
}