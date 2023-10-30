namespace MinimalChatApplication.Domain.Helpers
{
    public static class HttpStatusMessages
    {

        // Registration
        public const string RegistrationFailedValidation = "Registration failed due to validation errors";
        public const string ConflictEmailRegistered = "The email is already registered";
        public const string EmailAlreadyExist = "The email is already registered";
        public const string RegistrationSuccess = "Registration successful";
        public const string RegistrationFailure = "Registration failed";


        // Login
        public const string LoginFailedValidation = "Login failed due to validation errors";
        public const string LoginFailedIncorrectCredentials = "Login failed due to incorrect credentials";
        public const string LoginSuccessful = "Login successful";
        public const string InvalidGoogleCredentials = "Invalid Google Credentials. Invalid Token!";

        // Refresh Token
        public const string InvalidClientRequest = "Invalid client request";
        public const string InvalidClaimPrincipal = "Invalid claim principle";
        public const string InvalidAccessTokenOrRefreshToken = "Invalid access token or refresh token";

        // Update Login Status
        public const string FailedToUpdateUserStatus = "Failed to update user status";
        public const string CurrentUserNotFound = "Current user not found";
        public const string UserStatusUpdatedSuccessfully = "User statuses updated successfully";

        // Retrive UserList
        public const string UserListRetrievedSuccessfully = "User list retrieved successfully";

        // Send Message
        public const string MessageValidationFailure = "Message sending failed due to validation errors";
        public const string MessageSentSuccessfully = "Message sent successfully";
        public const string MessageSendingFailure = "Message sending failed";


        // Edit Message
        public const string MessageEditedSuccessfully = "Message edited successfully";
        public const string MessageEditingValidationFailure = "Message editing failed due to validation errors";
        public const string MessageNotFound = "Message not found";

        //DeleteMessage
        public const string MessageDeletedSuccessfully = "Message deleted successfully";


        //Retrieve Conversation
        public const string ConversationRetrieved = "Conversation history retrieved successfully";
        public const string ConversationNotFound = "Conversation not found";

        // Update Chat status
        public const string ChatStatusUpdated = "Chat status updated.";
        public const string ChatNotExists = "Chat does not exist.";

        // Log History
        public const string LogNotFound = "No logs found";
        public const string LogRetrievedSuccessfully = "Log list received successfully";

        //Retrieve Profile Details
        public const string ProfileDetailsRetrieved = "Log list received successfully";
        public const string ProfileNotFound = "User or profile not found";

        // Update Profile 
        public const string UpdateProfileValidationFailure = "Update profile failed due to validation errors";
        public const string ProfileUpdatedSuccessfullly = "Profile updated successfully";
        public const string ProfileUpdationFailed = "User not found or profile update failed";

        // Common
        public const string InternalServerError = "An error occurred while processing your request.";
        public const string UnauthorizedAccess = "Unauthorized access";
        public const string InvalidRequestParameter = "Invalid request parameters";
        public const string NoUsersFound = "No users found";
    }
}
