namespace Optimization
{
    public interface ILogManager
    {
        void Output(string line);
        void Output(string line, params object[] format);
    }
}