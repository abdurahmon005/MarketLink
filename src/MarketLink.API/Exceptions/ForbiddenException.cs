namespace MarketLink.API.Exceptions
{
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message = "Bu amalni bajarishga ruxsat yo'q")
            : base(message) { }
    }
}
