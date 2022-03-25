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
        /// Gets or sets assigne timeout to send .
        /// </summary>
        [Required(ErrorMessage = "Enter a number.")]
        [MinLength(1)]
        [DataType(DataType.Text)]
        [Display(Name = "AssignTimeOut")]
        public string AssignTimeOut { get; set; }

        /// <summary>
        /// Gets or sets interval to send  when ticket unassigned.
        /// </summary>
        [Required(ErrorMessage = "Enter a number.")]
        [MinLength(1)]
        [DataType(DataType.Text)]
        [Display(Name = "UnAssigneInterval")]
        public string UnAssigneInterval { get; set; }

        /// <summary>
        /// Gets or sets resolve timeout to send .
        /// </summary>
        [Required(ErrorMessage = "Enter a number.")]
        [MinLength(1)]
        [DataType(DataType.Text)]
        [Display(Name = "ResolveTimeOut")]
        public string ResolveTimeOut { get; set; }

        /// <summary>
        /// Gets or sets interval to send  when ticket unresolve.
        /// </summary>
        [Required(ErrorMessage = "Enter a number.")]
        [MinLength(1)]
        [DataType(DataType.Text)]
        [Display(Name = "UnResolveInterval")]
        public string UnResolveInterval { get; set; }

        /// <summary>
        /// Gets or sets interval to send cc  when ticket unresolve.
        /// </summary>
        [Required(ErrorMessage = "Enter a number.")]
        [MinLength(1)]
        [DataType(DataType.Text)]
        [Display(Name = "UnResolveCCInterval")]
        public string UnResolveCCInterval { get; set; }

        /// <summary>
        /// Gets or sets pending timeout to send .
        /// </summary>
        [Required(ErrorMessage = "Enter a number.")]
        [MinLength(1)]
        [DataType(DataType.Text)]
        [Display(Name = "PendingTimeOut")]
        public string PendingTimeOut { get; set; }

        /// <summary>
        /// Gets or sets interval to send  when ticket pending.
        /// </summary>
        [Required(ErrorMessage = "Enter a number.")]
        [MinLength(1)]
        [DataType(DataType.Text)]
        [Display(Name = "PendingInterval")]
        public string PendingInterval { get; set; }

        /// <summary>
        /// Gets or sets interval to send cc  when ticket pending.
        /// </summary>
        [Required(ErrorMessage = "Enter a number.")]
        [MinLength(1)]
        [DataType(DataType.Text)]
        [Display(Name = "PendingCCInterval")]
        public string PendingCCInterval { get; set; }

        /// <summary>
        /// Gets or sets welcome message text box to be used in View.
        /// </summary>
        [Required(ErrorMessage = "Enter emails separated by ';'.")]
        [MinLength(11)]
        [DataType(DataType.Text)]
        [Display(Name = "ExpertsAdmins")]
        public string ExpertsAdmins { get; set; }
    }
}