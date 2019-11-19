// <copyright file="TourCarousel.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System.Collections.Generic;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class process Tour Carousel feature : Common Method for user tour and team tour.
    /// </summary>
    public class TourCarousel
    {
        /// <summary>
        /// Create the set of cards that comprise the team tour carousel.
        /// </summary>
        /// <param name="appBaseUri">The base URI where the app is hosted</param>
        /// <returns>The cards that comprise the team tour</returns>
        public static IEnumerable<Attachment> GetTeamTourCards(string appBaseUri)
        {
            return new List<Attachment>()
            {
                GetCard(Resource.TeamNotificationHeaderText, Resource.TeamNotificationContent, appBaseUri + "/content/Notifications.png"),
                GetCard(Resource.TeamChatHeaderText, Resource.TeamChatContent, appBaseUri + "/content/Enduserchat.png"),
                GetCard(Resource.TeamTicketSystemHeaderText, Resource.TeamTicketSystemContent, appBaseUri + "/content/Ticketsystem.png"),
            };
        }

        /// <summary>
        /// Create the set of cards that comprise the user tour carousel.
        /// </summary>
        /// <param name="appBaseUri">The base URI where the app is hosted</param>
        /// <returns>The cards that comprise the user tour</returns>
        public static IEnumerable<Attachment> GetUserTourCards(string appBaseUri)
        {
            return new List<Attachment>()
            {
                GetCard(Resource.FunctionCardText1, Resource.FunctionCardText2, appBaseUri + "/content/Askaquestion.png"),
                GetCard(Resource.AskAnExpertText1, Resource.AskAnExpertText2, appBaseUri + "/content/Expertinquiry.png"),
                GetCard(Resource.ShareFeedbackTitleText, Resource.FeedbackText1, appBaseUri + "/content/Sharefeedback.png"),
            };
        }

        private static Attachment GetCard(string title, string text, string imageUri)
        {
            HeroCard tourCarouselCard = new HeroCard()
            {
                Title = title,
                Text = text,
                Images = new List<CardImage>()
                {
                    new CardImage(imageUri),
                }
            };

            return tourCarouselCard.ToAttachment();
        }
    }
}