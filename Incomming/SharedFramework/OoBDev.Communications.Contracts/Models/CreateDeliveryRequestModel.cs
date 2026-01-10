using OoBDev.Communications.Contracts.DeliveryLog;
using System;

namespace OoBDev.Communications.Contracts.Models
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class CreateDeliveryRequestModel : ICreateDeliveryRequest
    {
        /// <summary>
        /// This should be the same value for each delivery channel related to a single send request
        /// </summary>
        public Guid RequestGroupId { get; set; }
        /// <summary>
        /// This is an optional foreign key Core.Notifications
        /// </summary>
        public Guid? NotificationId { get; set; }

        /// <summary>
        /// This is the time the request was received by communication center
        /// </summary>
        public DateTimeOffset Requested { get; set; }

        /// <summary>
        /// targeted receiver's person ID
        /// </summary>
        public Guid PersonId { get; set; }

        /// <summary>
        /// this will be mapped to the related foreign key in Core.NotificationChannelTypes 
        /// </summary>
        public DeliveryChannelType DeliveryChannel { get; set; }

        /// <summary>
        /// Message Type requested for Communication Center
        /// </summary>
        public string MessageType { get; set; }
        /// <summary>
        /// To line address/phone number for targeted person
        /// </summary>
        public string SentTo { get; set; }
        /// <summary>
        /// Summary line test generated for request
        /// </summary>
        public string? Subject { get; set; }
        /// <summary>
        /// Long text message generated for request
        /// </summary>
        public string? Text { get; set; }
        /// <summary>
        /// Hyper text version of message if generated
        /// </summary>
        public string? Html { get; set; }
        /// <summary>
        /// Request Data post Enhancement
        /// </summary>
        public string? EnhancedData { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
