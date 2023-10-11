using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MinimalChatApplication.Domain.Dtos;
using MinimalChatApplication.Domain.Interfaces;
using MinimalChatApplication.Domain.Models;

namespace MinimalChatApplication.Data.Services
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnreadMessageRepository _unreadMessageRepository;
        private readonly UserManager<ChatApplicationUser> _userManager;

        
        public MessageService(IMessageRepository messageRepository, 
            IUserRepository userRepository,
            UserManager<ChatApplicationUser> userManager,
            IUnreadMessageRepository unreadMessageRepository)
        {
            _messageRepository = messageRepository;
            _userRepository = userRepository;
            _userManager = userManager;
            _unreadMessageRepository = unreadMessageRepository;
        }



        /// <summary>
        /// Sends a message asynchronously.
        /// </summary>
        /// <param name="messageDto">The message data.</param>
        /// <param name="senderId">The ID of the sender.</param>
        /// <returns>
        /// The unique identifier of the sent message if successful; otherwise, null.
        /// </returns>
        public async Task<int?> SendMessageAsync(MessageDto messageDto, string senderId)
        {
            if (messageDto != null)
            {
                var message = new Message
                {
                    Content = messageDto.Content,
                    SenderId = senderId,
                    ReceiverId = messageDto.ReceiverId,
                    Timestamp = DateTime.Now,
                };

                var data = await _messageRepository.AddAsync(message);
                await _messageRepository.SaveChangesAsync();
                return data.Id;
            }
            return null;
        }


        /// <summary>
        /// Edits a message with the given ID, updating its content.
        /// </summary>
        /// <param name="messageId">The ID of the message to edit.</param>
        /// <param name="userId">The ID of the user attempting to edit the message.</param>
        /// <param name="newContent">The updated content for the message.</param>
        /// <returns>
        /// A tuple containing a success flag, HTTP status code, and a message indicating the result of the operation.
        /// </returns>
        public async Task<(bool success, int StatusCode, string message)> EditMessageAsync(int messageId, string userId, string newContent)
        {
            // Check if the message with the given ID exists
            var message = await _messageRepository.GetByIdAsync(messageId);

            if (message != null)
            {
                // Check if the user is the sender of the message
                if (message.SenderId != userId)
                {
                    return (false, StatusCodes.Status401Unauthorized, "Unauthorized access");
                }
                if (newContent != null)
                {
                    message.Content = newContent;
                    _messageRepository.Update(message);
                    await _messageRepository.SaveChangesAsync();
                    return (true, StatusCodes.Status200OK, "Message edited successfully");
                }
                else
                {
                    return (false, StatusCodes.Status400BadRequest, "Message editing failed due to validation errors");
                }
            }
            return (false, StatusCodes.Status404NotFound, "Message not found");
        }


        /// <summary>
        /// Deletes a message with the given ID if it exists and if the user is the sender.
        /// </summary>
        /// <param name="messageId">The ID of the message to delete.</param>
        /// <param name="userId">The ID of the user attempting to delete the message.</param>
        /// <returns>
        /// A tuple containing a success flag, HTTP status code, and a message indicating the result of the operation.
        /// </returns>
        public async Task<(bool success, int StatusCode, string message)> DeleteMessageAsync(int messageId, string userId)
        {
            // Check if the message with the given ID exists
            var message = await _messageRepository.GetByIdAsync(messageId);

            if (message != null)
            {
                // Check if the user is the sender of the message
                if (message.SenderId != userId)
                {
                    return (false, StatusCodes.Status401Unauthorized, "Unauthorized access");
                }
                else
                {
                    _messageRepository.Remove(message);
                    await _messageRepository.SaveChangesAsync();
                    return (true, StatusCodes.Status200OK, "Message deleted successfully");
                }
            }
            return (false, StatusCodes.Status404NotFound, "Message not found");
        }


        /// <summary>
        /// Retrieves the conversation history between the logged-in user and a specific receiver user.
        /// </summary>
        /// <param name="loggedInUserId">The ID of the logged-in user.</param>
        /// <param name="receiverId">The ID of the receiver user.</param>
        /// <param name="before">Optional timestamp to filter messages before a specific time.</param>
        /// <param name="count">The number of messages to retrieve.</param>
        /// <param name="sort">The sorting mechanism for messages (asc or desc).</param>
        /// <returns>
        /// A tuple containing an IEnumerable of MessageResponseDto representing the conversation history
        /// and a boolean indicating the status of the receiver user (active or inactive).
        /// </returns>
        public async Task<(IEnumerable<MessageResponseDto>, bool status)> GetConversationHistoryAsync(string loggedInUserId, string receiverId, DateTime? before, int count, string sort)
        {
            var conversationHistory = await _messageRepository
                .GetConversationHistoryAsync(loggedInUserId, receiverId, before, count, sort);

            var userStatus = await _userRepository.GetUserStatusAsync(receiverId);

            var messageResponseDtos = conversationHistory.Select(message => new MessageResponseDto
            {
                MessageId = message.Id,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                Content = message.Content,
                Timestamp = message.Timestamp
            }).ToList();

            return (messageResponseDtos, userStatus);
        }


        ///<summary>
        /// Searches for messages containing a specific query in conversations where the user is either the sender or receiver.
        ///</summary>
        ///<param name="userId">The ID of the user initiating the search.</param>
        ///<param name="query">The string to search for in conversation messages.</param>
        ///<returns>A collection of MessageResponseDto representing the search results.</returns>
        public async Task<IEnumerable<MessageResponseDto>> SearchConversationsAsync(string userId, string query)
        {
            var searchedConversation = await _messageRepository.SearchConversationsAsync(userId, query);

            var messageResponseDtos = searchedConversation.Select(message => new MessageResponseDto
            {
                MessageId = message.Id,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                Content = message.Content,
                Timestamp = message.Timestamp
            }).ToList();

            return messageResponseDtos;
        }


        /// <summary>
        /// Asynchronously updates the chat status for a user, marking messages as read and managing unread message counts.
        /// </summary>
        /// <param name="userId">The ID of the user for whom the chat status is being updated.</param>
        /// <param name="currentUserId">The ID of the currently active user.</param>
        /// <param name="previousUserId">The ID of the previously active user (optional).</param>
        /// <returns>
        /// A tuple containing the success status, HTTP status code, and a message describing the outcome of the chat status update.
        /// </returns>
        public async Task<(bool Success, int StatusCode, string Message)> UpdateChatStatusAsync(string userId, string currentUserId, string previousUserId)
        {
            try
            {
                if (userId != null && currentUserId == null && previousUserId == null)
                {
                    var loggedInUserChat = await _unreadMessageRepository.GetLoggedInUserChat(userId);
                    if (loggedInUserChat != null)
                    {
                        loggedInUserChat.IsRead = false;
                        _unreadMessageRepository.Update(loggedInUserChat);
                    }
                    await _unreadMessageRepository.SaveChangesAsync();
                    return (true, StatusCodes.Status200OK, "Chat status updated.");
                }


                await CreateSenderChatAsync(userId, currentUserId);
                await CreateReceiverChatAsync(userId, currentUserId);
                
                if (string.IsNullOrEmpty(previousUserId) || previousUserId.ToLower() == "null" || previousUserId == "null")
                {
                    var chatExists = await _unreadMessageRepository.GetSenderMessageChat(userId, currentUserId);
                    if (chatExists != null)
                    {
                        chatExists.IsRead = true;
                        chatExists.MessageCount = 0;
                        _unreadMessageRepository.Update(chatExists);
                    }
                    else
                    {
                        return (false, StatusCodes.Status404NotFound, "Chat not exists");
                    }
                }
                else
                {
                    await CreateSenderChatAsync(userId, previousUserId);
                    await CreateReceiverChatAsync(userId, previousUserId);
                    var previousUserChat = await _unreadMessageRepository.GetSenderMessageChat(userId, previousUserId);
                    var currentUserChat = await _unreadMessageRepository.GetSenderMessageChat(userId, currentUserId);

                    if (previousUserChat != null)
                    {
                        previousUserChat.IsRead = false;
                        previousUserChat.MessageCount = 0;
                        _unreadMessageRepository.Update(previousUserChat);
                    }
                    if (currentUserChat != null)
                    {
                        currentUserChat.IsRead = true;
                        currentUserChat.MessageCount = 0;
                        _unreadMessageRepository.Update(currentUserChat);
                    }
                }
                await _unreadMessageRepository.SaveChangesAsync();
                return (true, StatusCodes.Status200OK, "Chat status updated.");
                
            }
            catch (Exception ex)
            {
                // Log or handle exceptions
                return (false, StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }


        /// <summary>
        /// Asynchronously creates a chat record for the sender-user and receiver-user if it does not already exist.
        /// </summary>
        /// <param name="senderId">The ID of the sender user.</param>
        /// <param name="receiverId">The ID of the receiver user.</param>
        /// <returns>
        /// A boolean indicating whether a new chat record was created (true) or if it already existed (false).
        /// </returns>
        private async Task<bool> CreateSenderChatAsync(string senderId, string receiverId)
        {
            var senderChat = await _unreadMessageRepository.GetSenderMessageChat(senderId, receiverId);
            if (senderChat == null)
            {
                senderChat = new UnreadMessageCount
                {
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    MessageCount = 0,
                    IsRead = false
                };

                await _unreadMessageRepository.AddAsync(senderChat);
                await _unreadMessageRepository.SaveChangesAsync();
                return true;
            }
            return false;
        }


        /// <summary>
        /// Asynchronously creates a chat record for the receiver-user and sender-user if it does not already exist.
        /// </summary>
        /// <param name="senderId">The ID of the sender user.</param>
        /// <param name="receiverId">The ID of the receiver user.</param>
        /// <returns>
        /// A boolean indicating whether a new chat record was created (true) or if it already existed (false).
        /// </returns>
        private async Task<bool> CreateReceiverChatAsync(string senderId, string receiverId)
        {
            var senderChat = await _unreadMessageRepository.GetReceiverMessageChat(senderId, receiverId);
            if (senderChat == null)
            {
                senderChat = new UnreadMessageCount
                {
                    SenderId = receiverId,
                    ReceiverId = senderId,
                    MessageCount = 0,
                    IsRead = false
                };

                await _unreadMessageRepository.AddAsync(senderChat);
                await _unreadMessageRepository.SaveChangesAsync();
                return true;
            }
            return false;
        }


        /// <summary>
        /// Asynchronously updates the message count and read status for the receiver user in the unread message repository.
        /// </summary>
        /// <param name="senderId">The ID of the sender user.</param>
        /// <param name="receiverId">The ID of the receiver user.</param>
        /// <returns>
        /// A UserResponseDto containing the updated message count, read status, and logged-in status of the receiver user.
        /// </returns>
        public async Task<UserResponseDto> UpdateMessageCount(string senderId, string receiverId)
        {
            var receiverChatExists = await _unreadMessageRepository.GetReceiverMessageChat(senderId, receiverId);
            UserResponseDto userResponseDto;

            
            if(receiverChatExists != null)
            {
                var receiverLoggedIn = await _userManager.FindByIdAsync(receiverChatExists.SenderId);
                if(receiverLoggedIn != null)
                {
                    if (receiverLoggedIn.IsActive && receiverChatExists.IsRead)
                    {
                        receiverChatExists.MessageCount = 0;
                        receiverChatExists.IsRead = true;
                    }
                    else
                    {
                        receiverChatExists.MessageCount++;
                        receiverChatExists.IsRead = false;
                    }
                }
                else
                {
                    receiverChatExists.MessageCount++;
                    receiverChatExists.IsRead = false;
                }
                _unreadMessageRepository.Update(receiverChatExists);

                userResponseDto = new UserResponseDto
                {
                    UserId = receiverChatExists.ReceiverId,
                    MessageCount = receiverChatExists.MessageCount,
                    IsRead = receiverChatExists.IsRead,
                    IsLoggedIn = receiverLoggedIn.IsActive,
                };
                await _unreadMessageRepository.SaveChangesAsync();
                return userResponseDto;
            }
            return null;
        }
    }
}
