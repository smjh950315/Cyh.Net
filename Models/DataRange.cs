namespace Cyh.Net.Models {
    public class DataRange {
        public int Begin { get; set; }
        public int Count { get; set; }

        public DataRange() { }

        public DataRange(int begin, int count) {
            this.Begin = begin;
            this.Count = count;
        }

        public DataRange((int, int) range) : this(range.Item1, range.Item2) { }

        public static implicit operator DataRange((int, int) range) => new DataRange(range.Item1, range.Item2);
    }
}
