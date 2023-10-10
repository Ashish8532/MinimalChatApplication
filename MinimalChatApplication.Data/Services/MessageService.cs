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

        /// <summary>
        /// Initializes a new instance of the MessageService class.
        /// </summary>
        /// <param name="messageRepository">The repository responsible for message data access.</param>
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
        /// <returns>An IEnumerable of MessageResponseDto containing conversation history.</returns>
        public async Task<IEnumerable<MessageResponseDto>> GetConversationHistoryAsync(string loggedInUserId, string receiverId, DateTime? before, int count, string sort)
        {
            var conversationHistory = await _messageRepository
                .GetConversationHistoryAsync(loggedInUserId, receiverId, before, count, sort);


            var messageResponseDtos = conversationHistory.Select(message => new MessageResponseDto
            {
                MessageId = message.Id,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                Content = message.Content,
                Timestamp = message.Timestamp
            }).ToList();

            return messageResponseDtos;
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


        public async Task<(bool Success, int StatusCode, string Message)> UpdateChatStatusAsync(string userId, string currentUserId, string previousUserId)
        {
            try
            {
                if(string.IsNullOrEmpty(previousUserId) || previousUserId.ToLower() == "null" || previousUserId == "null")
                {
                    var chatExists = await _unreadMessageRepository.GetSenderMessageChat(userId, currentUserId);
                    if (chatExists != null)
                    {
                        chatExists.IsRead = true;
                        _unreadMessageRepository.Update(chatExists);
                    }
                    else
                    {
                        return (false, StatusCodes.Status404NotFound, "Chat not exists");
                    }
                }
                else
                {
                    var chatExists = await _unreadMessageRepository.GetSenderMessageChat(userId, previousUserId);
                    if(chatExists != null)
                    {
                        chatExists.IsRead = false;
                        _unreadMessageRepository.Update(chatExists);
                    }
                    else
                    {
                        chatExists = await _unreadMessageRepository.GetSenderMessageChat(userId, currentUserId);
                        if( chatExists != null )
                        {
                            chatExists.IsRead = true;
                            _unreadMessageRepository.Update(chatExists);
                        }
                        return (false, StatusCodes.Status404NotFound, "Chat not exists");
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

        public async Task<UserResponseDto> UpsertMessageCount(string senderId, string receiverId)
        {
            var receiverChatExists = await _unreadMessageRepository.GetReceiverMessageChat(senderId, receiverId);
            var receiver = await _userManager.FindByIdAsync(receiverId);
            UserResponseDto userResponseDto;

            if (receiverChatExists == null)
            {
                // Create a new UnreadMessageCount entry if it doesn't exist
                receiverChatExists = new UnreadMessageCount
                {
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    MessageCount = 1,
                    IsRead = false
                };

                if (receiver != null)
                {
                    if (receiver.IsActive == false)
                    {
                        receiverChatExists.IsRead = false;
                    }
                    else
                    {
                        receiverChatExists.MessageCount = 0;
                        receiverChatExists.IsRead = true;
                    }
                }

                userResponseDto = new UserResponseDto
                {
                    UserId = senderId,
                    MessageCount = receiverChatExists.MessageCount,
                    IsRead = receiverChatExists.IsRead
                };

                await _unreadMessageRepository.AddAsync(receiverChatExists);
                await _unreadMessageRepository.SaveChangesAsync();
                return userResponseDto;
            }
            else
            {
                if (receiver != null)
                {
                    if (receiver.IsActive == false)
                    {
                        receiverChatExists.MessageCount++;
                        receiverChatExists.IsRead = false;
                    }
                    else
                    {
                        receiverChatExists.MessageCount = 0;
                        receiverChatExists.IsRead = true;
                    }
                }
                // Increment the count if the entry already exists
                
                _unreadMessageRepository.Update(receiverChatExists);

                userResponseDto = new UserResponseDto
                {
                    UserId = senderId,
                    MessageCount = receiverChatExists.MessageCount,
                    IsRead = receiverChatExists.IsRead
                };
                await _unreadMessageRepository.SaveChangesAsync();
                return userResponseDto;
            }
        }


        public async Task<UserResponseDto> UpdateSenderMessageCount(string senderId, string receiverId)
        {
            var unreadMessageChatExists = await _unreadMessageRepository.GetSenderMessageChat(senderId, receiverId);
            var sender = await _userManager.FindByIdAsync(senderId);

            if (unreadMessageChatExists != null)
            {
                if (sender != null)
                {
                    unreadMessageChatExists.MessageCount = 0;
                    unreadMessageChatExists.IsRead = true;
                }

                _unreadMessageRepository.Update(unreadMessageChatExists);
                await _unreadMessageRepository.SaveChangesAsync();

                // Create and return the DTO with updated values
                var userResponseDto = new UserResponseDto
                {
                    UserId = senderId,
                    MessageCount = unreadMessageChatExists.MessageCount,
                    IsRead = unreadMessageChatExists.IsRead
                };

                return userResponseDto;
            }

            return null;
        }

    }
}
