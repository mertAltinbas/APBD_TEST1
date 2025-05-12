namespace Test1.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException() { }
    
    public NotFoundException(string message){}
    
    public NotFoundException(string? message, Exception? innerException) : base(message, innerException) { }
}