namespace MarketLink.Application.Service.Impl
{
    public record ServiceResult(bool IsSuccess, string Message)
    {
        public static ServiceResult Ok(string message = "Muvaffaqiyatli") => new(true, message);
        public static ServiceResult Fail(string message) => new(false, message);
    }
}
