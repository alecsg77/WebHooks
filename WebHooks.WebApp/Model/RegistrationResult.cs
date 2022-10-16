namespace WebHooks.WebApp.Model
{
    public enum RegistrationResult
    {
        Success = 0,
        NotFound,
        Conflict,
        OperationError,
        InternalError
    }
}