using AutoMapper;
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
        private readonly IMapper _mapper;


        public MessageService(IMessageRepository messageRepository,
            IUserRepository userRepository,
            UserManager<ChatApplicationUser> userManager,
            IUnreadMessageRepository unreadMessageRepository,
            IMapper mapper)
        {
            _messageRepository = messageRepository;
            _userRepository = userRepository;
            _userManager = userManager;
            _unreadMessageRepository = unreadMessageRepository;
            _mapper = mapper;
        }



        /// <summary>
        /// Sends a message asynchronously.
        /// </summary>
        /// <param name="messageDto">The message data.</param>
        /// <param name="senderId">The ID of the sender.</param>
        /// <returns>
        /// The unique identifier of the sent message if successful; otherwise, null.
        /// </returns>
        public async Task<MessageResponseDto> SendMessageAsync(MessageDto messageDto, string senderId)
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
                var messageResponseDto = _mapper.Map<MessageResponseDto>(data);
                return messageResponseDto;
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
        public async Task<(bool success, int StatusCode, string message, MessageResponseDto deletedMessage)> DeleteMessageAsync(int messageId, string userId)
        {
            var message = await _messageRepository.GetByIdAsync(messageId);

            if (message != null)
            {
                if (message.SenderId != userId)
                {
                    return (false, StatusCodes.Status401Unauthorized, "Unauthorized access", null);
                }
                else
                {
                    var deletedMessageDto = _mapper.Map<MessageResponseDto>(message);

                    _messageRepository.Remove(message);
                    await _messageRepository.SaveChangesAsync();

                    return (true, StatusCodes.Status200OK, "Message deleted successfully", deletedMessageDto);
                }
            }
            return (false, StatusCodes.Status404NotFound, "Message not found", null);
        }



        /// <summary>
        /// Retrieves the conversation history between a logged-in user and a specific receiver, including user status.
        /// </summary>
        /// <param name="loggedInUserId">The ID of the logged-in user.</param>
        /// <param name="receiverId">The ID of the message receiver.</param>
        /// <param name="before">Optional. Retrieves messages created before this date.</param>
        /// <param name="count">The maximum number of messages to retrieve.</param>
        /// <param name="sort">The sorting order for the retrieved messages.</param>
        /// <returns>
        /// A tuple containing the conversation history as a collection of <see cref="MessageResponseDto"/> and a boolean
        /// indicating the user status of the receiver.
        /// </returns>
        public async Task<(IEnumerable<MessageResponseDto>, bool status)> GetConversationHistoryAsync(string loggedInUserId, string receiverId, DateTime? before, int count, string sort)
        {
            var conversationHistory = await _messageRepository
                .GetConversationHistoryAsync(loggedInUserId, receiverId, before, count, sort);

            var userStatus = await _userRepository.GetUserStatusAsync(receiverId);

            var messageResponseDtos = _mapper.Map<IEnumerable<MessageResponseDto>>(conversationHistory);

            return (messageResponseDtos, userStatus);
        }


        /// <summary>
        /// Searches for messages containing a specific query in conversations where the user is either the sender or receiver.
        /// </summary>
        /// <param name="userId">The ID of the user initiating the search.</param>
        /// <param name="query">The string to search for in conversation messages.</param>
        /// <returns>
        /// A collection of <see cref="MessageResponseDto"/> representing the search results.
        /// </returns>
        public async Task<IEnumerable<MessageResponseDto>> SearchConversationsAsync(string userId, string query)
        {
            var searchedConversation = await _messageRepository.SearchConversationsAsync(userId, query);

            var messageResponseDtos = _mapper.Map<IEnumerable<MessageResponseDto>>(searchedConversation);

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
                    var loggedInUsers = await _unreadMessageRepository.GetAllLoggedInUserChatAsync(userId);

                    foreach (var loggedInUser in loggedInUsers)
                    {
                        loggedInUser.IsRead = false;
                        _unreadMessageRepository.Update(loggedInUser);
                    }

                    await _unreadMessageRepository.SaveChangesAsync();
                    return (true, StatusCodes.Status200OK, "Chat status updated.");
                }


                await CreateSenderChatAsync(userId, currentUserId);
                await CreateReceiverChatAsync(userId, currentUserId);

                if (string.IsNullOrEmpty(previousUserId) || previousUserId.ToLower() == "null" || previousUserId == "null")
                {
                    var chatExists = await _unreadMessageRepository.GetSenderMessageChatAsync(userId, currentUserId);
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
                    var previousUserChat = await _unreadMessageRepository.GetSenderMessageChatAsync(userId, previousUserId);
                    var currentUserChat = await _unreadMessageRepository.GetSenderMessageChatAsync(userId, currentUserId);

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
            var senderChat = await _unreadMessageRepository.GetSenderMessageChatAsync(senderId, receiverId);
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
            var senderChat = await _unreadMessageRepository.GetReceiverMessageChatAsync(senderId, receiverId);
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
        /// Asynchronously increases the message count and updates the read status for the receiver user in the unread message repository.
        /// </summary>
        /// <param name="senderId">The ID of the sender user.</param>
        /// <param name="receiverId">The ID of the receiver user.</param>
        /// <returns>
        /// A tuple containing a <see cref="UserChatResponseDto"/> with the updated message count and read status,
        /// and a boolean indicating whether the receiver user is currently logged in.
        /// </returns>
        public async Task<(UserChatResponseDto userChatResponseDto, bool isLoggedIn)> IncreaseMessageCountAsync(string senderId, string receiverId)
        {
            var receiverChatExists = await _unreadMessageRepository.GetReceiverMessageChatAsync(senderId, receiverId);
            UserChatResponseDto userChatResponseDto;


            if (receiverChatExists != null)
            {
                var receiverLoggedIn = await _userManager.FindByIdAsync(receiverChatExists.SenderId);
                if (receiverLoggedIn != null)
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

                userChatResponseDto = new UserChatResponseDto
                {
                    UserId = receiverChatExists.ReceiverId,
                    MessageCount = receiverChatExists.MessageCount,
                    IsRead = receiverChatExists.IsRead,
                };
                await _unreadMessageRepository.SaveChangesAsync();
                return (userChatResponseDto, receiverLoggedIn.IsActive);
            }
            return (null, false);
        }


        /// <summary>
        /// Asynchronously decreases the message count and updates the read status for the receiver user in the unread message repository.
        /// </summary>
        /// <param name="senderId">The ID of the sender user.</param>
        /// <param name="receiverId">The ID of the receiver user.</param>
        /// <returns>
        /// A tuple containing a <see cref="UserChatResponseDto"/> with the updated message count and read status,
        /// and a boolean indicating whether the receiver user is currently logged in.
        /// </returns>
        public async Task<(UserChatResponseDto userChatResponseDto, bool isLoggedIn)> DecreaseMessageCountAsync(string senderId, string receiverId)
        {
            var receiverChatExists = await _unreadMessageRepository.GetReceiverMessageChatAsync(senderId, receiverId);
            UserChatResponseDto userChatResponseDto;


            if (receiverChatExists != null)
            {
                var receiverLoggedIn = await _userManager.FindByIdAsync(receiverChatExists.SenderId);
                if (receiverLoggedIn != null)
                {
                    if (receiverLoggedIn.IsActive && receiverChatExists.IsRead)
                    {
                        receiverChatExists.MessageCount = 0;
                        receiverChatExists.IsRead = true;
                    }
                    else
                    {
                        receiverChatExists.MessageCount--;
                        receiverChatExists.IsRead = false;
                    }
                }
                else
                {
                    receiverChatExists.MessageCount--;
                    receiverChatExists.IsRead = false;
                }
                _unreadMessageRepository.Update(receiverChatExists);

                userChatResponseDto = new UserChatResponseDto
                {
                    UserId = receiverChatExists.ReceiverId,
                    MessageCount = receiverChatExists.MessageCount,
                    IsRead = receiverChatExists.IsRead,
                };
                await _unreadMessageRepository.SaveChangesAsync();
                return (userChatResponseDto, receiverLoggedIn.IsActive);
            }
            return (null, false);
        }
    }
}
