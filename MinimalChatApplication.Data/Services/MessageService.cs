using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MinimalChatApplication.Domain.Dtos;
using MinimalChatApplication.Domain.Helpers;
using MinimalChatApplication.Domain.Interfaces;
using MinimalChatApplication.Domain.Models;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace MinimalChatApplication.Data.Services
{
    public class MessageService : IMessageService
    {
        private readonly IGenericRepository<Message> _messageRepository;
        private readonly IGenericRepository<UnreadMessageCount> _unreadMessageRepository;
        private readonly IGenericRepository<GifData> _gifRepository;
        private readonly IUserService _userService;
        private readonly UserManager<ChatApplicationUser> _userManager;
        private readonly IMapper _mapper;


        public MessageService(IGenericRepository<Message> messageRepository,
            IGenericRepository<GifData> gifRepository,
            IUserService userService,
            UserManager<ChatApplicationUser> userManager,
            IGenericRepository<UnreadMessageCount> unreadMessageRepository,
            IMapper mapper)
        {
            _messageRepository = messageRepository;
            _gifRepository = gifRepository;
            _userService = userService;
            _userManager = userManager;
            _unreadMessageRepository = unreadMessageRepository;
            _mapper = mapper;
        }



        /// <summary>
        /// Sends a message asynchronously, adding it to the database.
        /// </summary>
        /// <param name="messageDto">The message data to be sent.</param>
        /// <param name="senderId">The ID of the sender.</param>
        /// <returns>
        /// A message response containing the unique identifier of the sent message if successful; otherwise, null.
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
                    GifId = messageDto.GifId,
                };

                var data = await _messageRepository.AddAsync(message);
                await _messageRepository.SaveChangesAsync();

                if (data.GifId != null)
                {
                    var messageWithGif = await _messageRepository.GetFirstOrDefaultAsync(
                        m => m.Id == data.Id,
                        m => m.GifData
                    );

                    var messageResponseDto = _mapper.Map<MessageResponseDto>(messageWithGif);
                    return messageResponseDto;
                }
                else
                {
                    var messageResponseDto = _mapper.Map<MessageResponseDto>(data);
                    return messageResponseDto;
                }
            }
            return null;
        }


        /// <summary>
        /// Asynchronously edits a message with the given ID, updating its content.
        /// </summary>
        /// <param name="messageId">The ID of the message to edit.</param>
        /// <param name="userId">The ID of the user attempting to edit the message.</param>
        /// <param name="newContent">The updated content for the message.</param>
        /// <returns>
        /// A service response containing a success flag, HTTP status code, and a message indicating the result of the operation.
        /// </returns>
        public async Task<ServiceResponse<object>> EditMessageAsync(int messageId, string userId, string newContent)
        {
            var response = new ServiceResponse<object>();

            var message = await _messageRepository.GetFirstOrDefaultAsync(m => m.Id == messageId);

            if (message != null)
            {
                if (message.SenderId != userId)
                {
                    response.Succeeded = false;
                    response.StatusCode = StatusCodes.Status401Unauthorized;
                    response.Message = HttpStatusMessages.UnauthorizedAccess;
                    response.Data = null;
                }
                if (newContent != null)
                {
                    message.Content = newContent;
                    _messageRepository.Update(message);
                    await _messageRepository.SaveChangesAsync();

                    response.Succeeded = true;
                    response.StatusCode = StatusCodes.Status200OK;
                    response.Message = HttpStatusMessages.MessageEditedSuccessfully;
                    response.Data = null;
                }
                else
                {
                    response.Succeeded = false;
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Message = HttpStatusMessages.MessageEditingValidationFailure;
                    response.Data = null;
                }
            }
            else
            {
                response.Succeeded = false;
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = HttpStatusMessages.MessageNotFound;
                response.Data = null;
            }
            return response;
        }


        /// <summary>
        /// Deletes a message with the given ID if it exists and if the user is the sender.
        /// </summary>
        /// <param name="messageId">The ID of the message to delete.</param>
        /// <param name="userId">The ID of the user attempting to delete the message.</param>
        /// <returns>
        /// A service response containing the result of the operation.
        /// - Succeeded (bool): True if the message is successfully deleted; otherwise, false.
        /// - StatusCode (int): The HTTP status code indicating the outcome of the deletion.
        /// - Message (string): A message describing the result of the operation.
        /// - Data (MessageResponseDto): The deleted message data (null if the operation is not successful).
        /// </returns>
        public async Task<ServiceResponse<MessageResponseDto>> DeleteMessageAsync(int messageId, string userId)
        {
            var response = new ServiceResponse<MessageResponseDto>();

            var message = await _messageRepository.GetFirstOrDefaultAsync(m => m.Id == messageId);

            if (message != null)
            {
                if (message.SenderId != userId)
                {
                    response.Succeeded = false;
                    response.StatusCode = StatusCodes.Status401Unauthorized;
                    response.Message = HttpStatusMessages.UnauthorizedAccess;
                    response.Data = null;
                }
                else
                {
                    var deletedMessageDto = _mapper.Map<MessageResponseDto>(message);

                    _messageRepository.Remove(message);
                    await _messageRepository.SaveChangesAsync();

                    response.Succeeded = true;
                    response.StatusCode = StatusCodes.Status200OK;
                    response.Message = HttpStatusMessages.MessageDeletedSuccessfully;
                    response.Data = deletedMessageDto;
                }
            }
            else
            {
                response.Succeeded = false;
                response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = HttpStatusMessages.MessageNotFound;
                response.Data = null;
            }
            return response;
        }




        /// <summary>
        /// Retrieves the conversation history between a logged-in user and a specific receiver, including user status.
        /// </summary>
        /// <param name="loggedInUserId">The ID of the logged-in user.</param>
        /// <param name="receiverId">The ID of the message receiver.</param>
        /// <param name="before">Optional. Retrieves messages created before this date.</param>
        /// <param name="count">The maximum number of messages to retrieve.</param>
        /// <param name="sort">The sorting order for the retrieved messages ("asc" or "desc").</param>
        /// <returns>
        /// A collection of <see cref="MessageResponseDto"/> representing the conversation history, 
        /// and a boolean indicating the user status of the receiver.
        /// </returns>
        public async Task<IEnumerable<MessageResponseDto>> GetConversationHistoryAsync(string loggedInUserId, string receiverId, DateTime? before, int count, string sort)
        {
            Expression<Func<Message, bool>> filter =
                      m => (m.SenderId == loggedInUserId && m.ReceiverId == receiverId) ||
                (m.SenderId == receiverId && m.ReceiverId == loggedInUserId);

            var conversationHistory = await _messageRepository.GetByConditionAsync(filter, m => m.GifData);

            if (before.HasValue)
            {
                conversationHistory = conversationHistory.Where(m => m.Timestamp < before);
            }

            if (sort.Equals("desc", StringComparison.OrdinalIgnoreCase))
            {
                conversationHistory = conversationHistory.OrderByDescending(m => m.Timestamp);
            }
            else
            {
                conversationHistory = conversationHistory.OrderBy(m => m.Timestamp);
            }
            conversationHistory = conversationHistory.Take(count);
            conversationHistory = conversationHistory.OrderBy(m => m.Id);

            var messageResponseDtos = _mapper.Map<IEnumerable<MessageResponseDto>>(conversationHistory);

            return messageResponseDtos;
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
            Expression<Func<Message, bool>> filter =
                      m => (m.SenderId == userId || m.ReceiverId == userId) && m.Content.Contains(query);

            var searchedConversation = await _messageRepository.GetByConditionAsync(filter);

            var messageResponseDtos = _mapper.Map<IEnumerable<MessageResponseDto>>(searchedConversation);

            return messageResponseDtos;
        }


        /// <summary>
        /// Asynchronously updates the chat status for a user, marking messages as read and managing unread message counts.
        /// </summary>
        /// <param name="userId">The ID of the user for whom the chat status is being updated.</param>
        /// <param name "currentUserId">The ID of the currently active user.</param>
        /// <param name "previousUserId">The ID of the previously active user (optional).</param>
        /// <returns>
        /// A <see cref="ServiceResponse{object}"/> indicating the success status, HTTP status code, and a message describing the outcome of the chat status update.
        /// </returns>
        public async Task<ServiceResponse<object>> UpdateChatStatusAsync(string userId, string currentUserId, string previousUserId)
        {
            var response = new ServiceResponse<object>();
            try
            {
                if (userId != null && currentUserId == null && previousUserId == null)
                {
                    var loggedInUsers = await GetAllLoggedInUserChatAsync(userId);

                    foreach (var loggedInUser in loggedInUsers)
                    {
                        loggedInUser.IsRead = false;
                        _unreadMessageRepository.Update(loggedInUser);
                    }

                    await _unreadMessageRepository.SaveChangesAsync();
                    response.Succeeded = true;
                    response.StatusCode = StatusCodes.Status200OK;
                    response.Message = HttpStatusMessages.ChatStatusUpdated;
                    response.Data = null;
                    return response;
                }

                await CreateSenderChatAsync(userId, currentUserId);
                await CreateReceiverChatAsync(userId, currentUserId);

                if (string.IsNullOrEmpty(previousUserId) || previousUserId.ToLower() == "null" || previousUserId == "null")
                {
                    var chatExists = await GetSenderMessageChatAsync(userId, currentUserId);
                    if (chatExists != null)
                    {
                        chatExists.IsRead = true;
                        chatExists.MessageCount = 0;
                        _unreadMessageRepository.Update(chatExists);
                    }
                    else
                    {
                        response.Succeeded = false;
                        response.StatusCode = StatusCodes.Status404NotFound;
                        response.Message = HttpStatusMessages.ChatNotExists;
                        response.Data = null;
                        return response;
                    }
                }
                else
                {
                    await CreateSenderChatAsync(userId, previousUserId);
                    await CreateReceiverChatAsync(userId, previousUserId);
                    var previousUserChat = await GetSenderMessageChatAsync(userId, previousUserId);
                    var currentUserChat = await GetSenderMessageChatAsync(userId, currentUserId);

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
                response.Succeeded = true;
                response.StatusCode = StatusCodes.Status200OK;
                response.Message = HttpStatusMessages.ChatStatusUpdated;
                response.Data = null;
                return response;

            }
            catch (Exception ex)
            {
                response.Succeeded = false;
                response.StatusCode = StatusCodes.Status500InternalServerError;
                response.Message = $"{HttpStatusMessages.InternalServerError} {ex.Message}";
                return response;
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
            var senderChat = await GetSenderMessageChatAsync(senderId, receiverId);
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
            var senderChat = await GetReceiverMessageChatAsync(senderId, receiverId);
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
        /// A <see cref="UserChatResponseDto"/> containing the updated message count and read status for the receiver user,
        /// along with a boolean indicating whether the receiver user is currently logged in.
        /// </returns>
        public async Task<UserChatResponseDto> IncreaseMessageCountAsync(string senderId, string receiverId)
        {
            var receiverChatExists = await GetReceiverMessageChatAsync(senderId, receiverId);
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
                return userChatResponseDto;
            }
            return null;
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
        public async Task<UserChatResponseDto> DecreaseMessageCountAsync(string senderId, string receiverId)
        {
            var receiverChatExists = await GetReceiverMessageChatAsync(senderId, receiverId);
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
                return userChatResponseDto;
            }
            return null;
        }

        /// <summary>
        /// Asynchronously retrieves the chat record for the receiver-user and sender-user from the UnreadMessageCounts table.
        /// </summary>
        /// <param name="senderId">The ID of the sender user.</param>
        /// <param name="receiverId">The ID of the receiver user.</param>
        /// <returns>
        /// A Task that represents the asynchronous operation and contains the UnreadMessageCount entity
        /// representing the chat record between the receiver-user and sender-user.
        /// </returns>
        private async Task<UnreadMessageCount> GetReceiverMessageChatAsync(string senderId, string receiverId)
        {
            return await _unreadMessageRepository.GetFirstOrDefaultAsync(x => x.SenderId == receiverId && x.ReceiverId == senderId);
        }


        /// <summary>
        /// Asynchronously retrieves the chat record for the sender-user and receiver-user from the UnreadMessageCounts table.
        /// </summary>
        /// <param name="senderId">The ID of the sender user.</param>
        /// <param name="receiverId">The ID of the receiver user.</param>
        /// <returns>
        /// A Task that represents the asynchronous operation and contains the UnreadMessageCount entity
        /// representing the chat record between the sender-user and receiver-user.
        /// </returns>
        private async Task<UnreadMessageCount> GetSenderMessageChatAsync(string senderId, string receiverId)
        {
            return await _unreadMessageRepository.GetFirstOrDefaultAsync(x => x.SenderId == senderId && x.ReceiverId == receiverId);
        }


        /// <summary>
        /// Asynchronously retrieves all chat records for a logged-in user from the UnreadMessageCounts table.
        /// </summary>
        /// <param name="userId">The ID of the logged-in user.</param>
        /// <returns>
        /// A Task that represents the asynchronous operation and contains a collection of UnreadMessageCount entities
        /// representing the chat records for the logged-in user.
        /// </returns>
        private async Task<IEnumerable<UnreadMessageCount>> GetAllLoggedInUserChatAsync(string userId)
        {
            Expression<Func<UnreadMessageCount, bool>> filter =
                      m => m.SenderId == userId;

            return await _unreadMessageRepository.GetByConditionAsync(filter);
        }
    }
}
