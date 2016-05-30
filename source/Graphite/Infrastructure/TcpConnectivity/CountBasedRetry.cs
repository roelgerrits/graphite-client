namespace Graphite.Infrastructure.TcpConnectivity
{
    /// <summary>
    /// IRetryStategy defines the methods for a retry strategy
    /// </summary>
    public interface IRetry
    {
        /// <summary>
        /// RetryAllowed
        /// </summary>
        /// <returns></returns>
        bool RetryAllowed();
        /// <summary>
        /// Reset resets retry counters
        /// </summary>
        /// <returns></returns>
        void Reset();
    }

    /// <summary>
    /// Count based retry strategy
    /// </summary>
    public class CountBasedRetry : IRetry
    {
        int _maxRetries = 10;
        int _retryCount;

        /// <summary>
        /// Count based retry
        /// </summary>
        /// <param name="maxRetryCount"></param>
        public CountBasedRetry(int maxRetryCount)
        {
            _maxRetries = maxRetryCount;
        }
        /// <summary>
        /// Reset retry count
        /// </summary>
        public void Reset()
        {
            _retryCount = 0;
        }
        /// <summary>
        /// RetryAllowed
        /// </summary>
        /// <returns></returns>
        public bool RetryAllowed()
        {
            _retryCount += 1;

            if (_retryCount > _maxRetries && _maxRetries != -1)
            {
                return false;
            }

            return true;
        }
    }
}
