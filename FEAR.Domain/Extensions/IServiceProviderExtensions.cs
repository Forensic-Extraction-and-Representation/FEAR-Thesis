using Microsoft.Extensions.DependencyInjection;

namespace FEAR.Domain.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IServiceProvider"/> and <see cref="IServiceCollection"/> to simplify service management and instantiation.
    /// </summary>
    public static class IServiceProviderExtensions 
    { 
        /// <summary>
        /// Creates an instance of the specified type using dependency injection and casts it to <typeparamref name="T"/>.
        /// Throws an exception if the type is not assignable to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The target type to cast the created instance to.</typeparam>
        /// <param name="serviceProvider">The service provider used for dependency injection.</param>
        /// <param name="type">The type to instantiate.</param>
        /// <returns>An instance of <typeparamref name="T"/> created via dependency injection.</returns>
        public static T CreateInstance<T>(this IServiceProvider serviceProvider, Type type)
        {
            if(type.IsAssignableTo(typeof(T)))
                return (T)ActivatorUtilities.CreateInstance(serviceProvider, type);
            else
            {
                throw new Exception($"Type {type.FullName} is not assignable to {typeof(T).FullName}");
            }
        }

        /// <summary>
        /// Throws an exception if a service of type <typeparamref name="T"/> is already registered in the service collection.
        /// </summary>
        /// <typeparam name="T">The service type to check for.</typeparam>
        /// <param name="services">The service collection to check.</param>
        /// <param name="message">Optional custom exception message.</param>
        public static void ThrowIfServiceExists<T>(this IServiceCollection services, string message = null) where T : class
        {
            if (String.IsNullOrEmpty(message))
                message = $"A {typeof(T).FullName} is already set.";

            if (DoesServiceExist<T>(services))
                throw new Exception(message);
        }

        /// <summary>
        /// Checks if a service of type <typeparamref name="T"/> is registered in the service collection.
        /// </summary>
        /// <typeparam name="T">The service type to check for.</typeparam>
        /// <param name="services">The service collection to check.</param>
        /// <returns>True if the service exists; otherwise, false.</returns>
        public static bool DoesServiceExist<T>(this IServiceCollection services) where T : class
        {
            return services.Any(t => t.ServiceType == typeof(T));
        }

        /// <summary>
        /// Throws an exception if a service of type <typeparamref name="T"/> is not registered in the service collection.
        /// </summary>
        /// <typeparam name="T">The service type to check for.</typeparam>
        /// <param name="services">The service collection to check.</param>
        public static void ThrowIfServiceDoesNotExist<T>(this IServiceCollection services) where T : class
        {
            if (!DoesServiceExist<T>(services))
                throw new Exception($"A {typeof(T).FullName} is not registered.");
        }
    }
}
