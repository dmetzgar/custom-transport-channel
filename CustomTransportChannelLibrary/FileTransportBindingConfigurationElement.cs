using System;
using System.Globalization;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;

namespace CustomTransportChannelLibrary
{
    public class FileTransportBindingConfigurationElement : StandardBindingElement
    {
        protected override Type BindingElementType
        {
            get { return typeof(FileTransportBinding); }
        }

        protected override void OnApplyConfiguration(Binding binding)
        {
            if (binding == null)
            {
                throw new ArgumentNullException("binding");
            }

            if (binding.GetType() != typeof(FileTransportBinding))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                    "Invalid type for binding. Expected type: {0}. Type passed in: {1}.",
                    typeof(FileTransportBinding).AssemblyQualifiedName,
                    binding.GetType().AssemblyQualifiedName));
            }
        }
    }

    public class FileTransportBindingCollectionElement : 
        StandardBindingCollectionElement<FileTransportBinding, FileTransportBindingConfigurationElement> { }
}
