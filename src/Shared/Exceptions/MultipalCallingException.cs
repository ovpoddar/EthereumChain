namespace Shared.Exceptions;
internal class MultipleCallingException : Exception
{
    public MultipleCallingException() : base("This method is not suppose to called twice.")
    {
    }
}
