using System.Text.Json;

namespace Cyh.Net.Data.Extension
{
    public static class JsonSerializerExtends
    {
        /// <summary>
        /// 將輸入可能是Json的物件反序列化為目標模型
        /// </summary>
        /// <typeparam name="TModel">目標模型</typeparam>
        /// <param name="maybe_json">可能是Json的物件</param>
        /// <param name="options">反序列化的選項</param>
        /// <returns>反序列化取得的目標模型，如果失敗會回傳預設值或是null</returns>
        unsafe public static TModel? JsonDeserialize<TModel>(this IJsonSerializer? controllerBase, object? maybe_json, JsonSerializerOptions? options = null) {
            if (maybe_json == null) { return default; }
            if (maybe_json is TModel model) { return model; }
            string? rawString = maybe_json is string jsonStr ? jsonStr : maybe_json.ToString();
            if (rawString.IsNullOrEmpty()) { return default; }
            try {
                return JsonSerializer.Deserialize<TModel>(rawString, options) ?? default;
            } catch {
                return default;
            }
        }

        /// <summary>
        /// 將輸入模型序列化為Json字串
        /// </summary>
        /// <typeparam name="TModel">來源模型</typeparam>
        /// <param name="model">輸入模型</param>
        /// <param name="options">序列化的選項</param>
        /// <returns>序列化取得的Jons字串，如果失敗會回傳空字串</returns>
        unsafe public static string JsonSerialize<TModel>(this IJsonSerializer? controllerBase, TModel? model, JsonSerializerOptions? options = null) {
            if (model is null) { return String.Empty; }
            try {
                return JsonSerializer.Serialize(model, options) ?? String.Empty;
            } catch {
                return String.Empty;
            }
        }
    }
}
