using OoBDev.Communications.Contracts.Handler;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OoBDev.Communications.Abstractions
{
    /// <summary>
    /// ICommunicationProvider is the central portion of the Communication Center.
    /// Communication center allows for a centralized management  
    /// </summary>
    public interface ICommunicationProvider
    {
        //TODO: come up with a better pattern. 
        Task<string> SendAsync(
            ISendRequest model,
            [CallerMemberName] string? caller = null,
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string? callerPath = null
            );

        Task<string> SendAsync(
            Guid targetPersonId, 
            string messageType, 
            object extendedData,
            [CallerMemberName] string? caller = null,
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string? callerPath = null
            );

        /// <summary>
        /// Use this operation to send a message to a particular person. The object
        /// generic type being passed in may be used in combination with the attribute 
        /// [Communication] to optionally override the message type and priority.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="targetPersonId"></param>
        /// <param name="messageData"></param>
        /// <param name="caller"></param>
        /// <param name="lineNumber"></param>
        /// <param name="callerPath"></param>
        /// <returns></returns>
        Task<string> SendAsync<T>(
            Guid targetPersonId, 
            T messageData,
            [CallerMemberName] string? caller = null,
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string? callerPath = null
            );
    }
}
