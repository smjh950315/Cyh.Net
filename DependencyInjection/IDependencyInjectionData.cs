namespace Cyh.Net.DependencyInjection {
    /// <summary>
    /// Minimum requirements of DependencyInjection
    /// </summary>
    public interface IDependencyInjectionData {
        /// <summary>
        /// (service) =&gt; { service.AddScoped(Type, Func&lt;IServiceProvider, object&gt;); }
        /// </summary>
        Action<Type, Func<IServiceProvider, object>> AddScoped { get; }

        /// <summary>
        /// service.Where(s => s.ServiceType == Type);
        /// </summary>
        Func<Type, object?> GetService { get; }
    }
}
