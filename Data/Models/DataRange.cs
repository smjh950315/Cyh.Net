namespace Cyh.Net.Data.Models
{
    /// <summary>
    /// Determines a range of data to fetch
    /// </summary>
    public class DataRange
    {
        /// <summary>
        /// Begin index of data in sequence.
        /// </summary>
        public int Begin { get; set; }

        /// <summary>
        /// Count of data to fetch.
        /// </summary>
        public int Count { get; set; }
    }
}
