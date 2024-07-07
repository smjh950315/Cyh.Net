namespace Cyh.Net.Models {
    public interface IExecuteResult {
        ResultEnum ResultCode { get; internal set; }
        DateTime Executed { get; internal set; }
        string? Message { get; internal set; }
    }
}
