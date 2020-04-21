using System.Runtime.Serialization;
using Grpc.Core;

namespace ServiceModel.Grpc.Configuration
{
    /// <summary>
    /// Provides the method to create the <see cref="Marshaller{T}"/> for serializing and deserializing messages by <see cref="DataContractSerializer"/>.
    /// </summary>
    public sealed class DataContractMarshallerFactory : IMarshallerFactory
    {
        /// <summary>
        /// Default instance of <see cref="DataContractMarshallerFactory"/>.
        /// </summary>
        public static readonly IMarshallerFactory Default = new DataContractMarshallerFactory();

        /// <summary>
        /// Creates the <see cref="Marshaller{T}"/>.
        /// </summary>
        /// <typeparam name="T">The message type.</typeparam>
        /// <returns>The instance of <see cref="Marshaller{T}"/> for serializing and deserializing messages.</returns>
        public Marshaller<T> CreateMarshaller<T>() => DataContractMarshaller<T>.Default;
    }
}
