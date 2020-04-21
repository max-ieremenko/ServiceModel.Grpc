using Grpc.Core;

namespace ServiceModel.Grpc.Configuration
{
    /// <summary>
    /// Represents a type which provides support to create <see cref="Marshaller{T}"/>.
    /// </summary>
    public interface IMarshallerFactory
    {
        /// <summary>
        /// Creates an instance of <see cref="Marshaller{T}"/> for serializing and deserializing messages.
        /// </summary>
        /// <typeparam name="T">The message type.</typeparam>
        /// <returns>The instance of <see cref="Marshaller{T}"/> for serializing and deserializing messages.</returns>
        Marshaller<T> CreateMarshaller<T>();
    }
}
