namespace MarketLink.API.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
        public NotFoundException(string resource, object id)
            : base($"{resource} topilmadi (id: {id})") { }
    }
}
