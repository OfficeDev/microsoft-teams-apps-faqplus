// <copyright file="Validators.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Helpers
{
    using System.Text.RegularExpressions;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Response card helper class to call response cards.
    /// </summary>
    public static class Validators
    {
        /// <summary>
        /// Image url pattern validation expression.
        /// </summary>
        private static readonly string ValidImgUrlPattern = @"^(http|https|)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&~%\$#_]*)?([a-zA-Z0-9\-\?\,\'\/\+&~%\$#_]+)(.jpeg|.JPEG|.jpg|.JPG|.png|.PNG)$";

        /// <summary>
        /// Html pattern validation expression.
        /// </summary>
        private static readonly string HtmlPattern = @"<[^>]*>";

        /// <summary>
        ///  This method is used to check if question or answer is empty.
        /// </summary>
        /// <param name="qnaPairEntity">Qna pair entity that contains question and answer information.</param>
        /// <returns>Boolean value where true represent question or answer is empty while false represent question or answer is not empty.</returns>
        public static bool IsQnaFieldsNullOrEmpty(AdaptiveSubmitActionData qnaPairEntity)
        {
            return string.IsNullOrEmpty(qnaPairEntity?.UpdatedQuestion?.Trim()) || string.IsNullOrEmpty(qnaPairEntity?.Description?.Trim());
        }

        /// <summary>
        ///  This method is used to check if image url is valid or not.
        /// </summary>
        /// <param name="qnaPairEntity">Qna pair entity that contains question and answer information.</param>
        /// <returns>Boolean value where true represent image url is valid while false represent image url in not valid.</returns>
        public static bool IsImageUrlInvalid(AdaptiveSubmitActionData qnaPairEntity)
        {
            return !string.IsNullOrEmpty(qnaPairEntity?.ImageUrl?.Trim()) ? !Regex.IsMatch(qnaPairEntity?.ImageUrl?.Trim(), ValidImgUrlPattern) : false;
        }

        /// <summary>
        ///  This method is used to check html tags are present or not.
        /// </summary>
        /// <param name="qnaPairEntity">Qna pair entity that contains question and answer information.</param>
        /// <returns>Boolean value where true represent html tags are present while false represent html tags are not present.</returns>
        public static bool IsContainsHtml(AdaptiveSubmitActionData qnaPairEntity)
        {
            return Regex.IsMatch(qnaPairEntity?.UpdatedQuestion?.Trim(), HtmlPattern)
                || Regex.IsMatch(qnaPairEntity?.ImageUrl?.Trim(), HtmlPattern)
                || Regex.IsMatch(qnaPairEntity?.RedirectionUrl?.Trim(), HtmlPattern)
                || Regex.IsMatch(qnaPairEntity?.Description?.Trim(), HtmlPattern)
                || Regex.IsMatch(qnaPairEntity?.Subtitle?.Trim(), HtmlPattern)
                || Regex.IsMatch(qnaPairEntity?.Title?.Trim(), HtmlPattern);
        }

        /// <summary>
        ///  This method is used to check if redirect url is invalid.
        /// </summary>
        /// <param name="qnaPairEntity">Qna pair entity that contains question and answer information.</param>
        /// <returns>Boolean value where true represent redirect url is invalid while false represent redirect url is valid.</returns>
        public static bool IsRedirectionUrlInvalid(AdaptiveSubmitActionData qnaPairEntity)
        {
            return !string.IsNullOrEmpty(qnaPairEntity?.RedirectionUrl?.Trim())
                && !Regex.IsMatch(qnaPairEntity?.RedirectionUrl?.Trim(), Constants.ValidRedirectUrlPattern);
        }

        /// <summary>
        /// Html, question and answer fields validation.
        /// </summary>
        /// <param name="qnaPairEntity">Qna pair entity that contains question and answer information.</param>
        /// <returns>Return a question data object.</returns>
        public static AdaptiveSubmitActionData HtmlAndQnaEmptyValidation(AdaptiveSubmitActionData qnaPairEntity)
        {
            qnaPairEntity.IsHTMLPresent = IsContainsHtml(qnaPairEntity);
            qnaPairEntity.IsQnaNullOrEmpty = IsQnaFieldsNullOrEmpty(qnaPairEntity);
            return qnaPairEntity;
        }

        /// <summary>
        /// Image And redirection url fields validation.
        /// </summary>
        /// <param name="qnaPairEntity">Qna pair entity that contains question and answer information.</param>
        /// <returns>Return a question data object.</returns>
        public static AdaptiveSubmitActionData ValidateImageAndRedirectionUrls(AdaptiveSubmitActionData qnaPairEntity)
        {
            qnaPairEntity.IsInvalidImageUrl = IsImageUrlInvalid(qnaPairEntity);
            qnaPairEntity.IsInvalidRedirectUrl = IsRedirectionUrlInvalid(qnaPairEntity);
            return qnaPairEntity;
        }

        /// <summary>
        /// Check if Json is valid or not in case if there is only answer present in QnA.
        /// </summary>
        /// <param name="description">Description json string as input.</param>
        /// <returns>Boolean value which indicates whether json is valid or not.</returns>
        public static bool IsValidJSON(string description)
        {
            try
            {
                JObject.Parse(description);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///  This method is used to check if card is rich or not.
        /// </summary>
        /// <param name="questionObject">Question data object.</param>
        /// <returns>Boolean value where true represent it is a rich card while false represent it is a normal card.</returns>
        public static bool IsRichCard(AdaptiveSubmitActionData questionObject)
        {
            return !string.IsNullOrEmpty(questionObject?.Title?.Trim())
                    || !string.IsNullOrEmpty(questionObject?.Subtitle?.Trim())
                    || !string.IsNullOrEmpty(questionObject?.ImageUrl?.Trim())
                    || !string.IsNullOrEmpty(questionObject?.RedirectionUrl?.Trim());
        }
    }
}
