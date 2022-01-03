namespace ShareOut.Lock;

public interface ILock
{
    ValueTask Release();
}