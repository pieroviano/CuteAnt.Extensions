#if NET35
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;

namespace System.ComponentModel.DataAnnotations
{
    public sealed class ValidationContext : IServiceProvider
    {
        private Func<Type, object> _serviceProvider;
        private object _objectInstance;
        private string _memberName;
        private string _displayName;
        private Dictionary<object, object> _items;
        private IServiceContainer _serviceContainer;

        /// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationContext" /> class using the specified object instance</summary>
        /// <param name="instance">The object instance to validate. It cannot be <see langword="null" />.</param>
        public ValidationContext(object instance)
            : this(instance, (IServiceProvider)null, (IDictionary<object, object>)null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationContext" /> class using the specified object and an optional property bag.</summary>
        /// <param name="instance">The object instance to validate.  It cannot be <see langword="null" /></param>
        /// <param name="items">An optional set of key/value pairs to make available to consumers.</param>
        public ValidationContext(object instance, IDictionary<object, object> items)
            : this(instance, (IServiceProvider)null, items)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationContext" /> class using the service provider and dictionary of service consumers.</summary>
        /// <param name="instance">The object to validate. This parameter is required.</param>
        /// <param name="serviceProvider">The object that implements the <see cref="T:System.IServiceProvider" /> interface. This parameter is optional.</param>
        /// <param name="items">A dictionary of key/value pairs to make available to the service consumers. This parameter is optional.</param>
        public ValidationContext(
            object instance,
            IServiceProvider serviceProvider,
            IDictionary<object, object> items)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (serviceProvider != null)
                this.InitializeServiceProvider((Func<Type, object>)(serviceType => serviceProvider.GetService(serviceType)));
            this._serviceContainer = !(serviceProvider is IServiceContainer parentContainer) ? (IServiceContainer)new ValidationContext.ValidationContextServiceContainer() : (IServiceContainer)new ValidationContext.ValidationContextServiceContainer(parentContainer);
            this._items = items == null ? new Dictionary<object, object>() : new Dictionary<object, object>(items);
            this._objectInstance = instance;
        }

        /// <summary>Gets the object to validate.</summary>
        /// <returns>The object to validate.</returns>
        public object ObjectInstance
        {
            get => this._objectInstance;
        }

        /// <summary>Gets the type of the object to validate.</summary>
        /// <returns>The type of the object to validate.</returns>
        public Type ObjectType
        {
            get => this.ObjectInstance.GetType();
        }

        /// <summary>Gets or sets the name of the member to validate.</summary>
        /// <returns>The name of the member to validate.</returns>
        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(this._displayName))
                {
                    this._displayName = this.GetDisplayName();
                    if (string.IsNullOrEmpty(this._displayName))
                    {
                        this._displayName = this.MemberName;
                        if (string.IsNullOrEmpty(this._displayName))
                            this._displayName = this.ObjectType.Name;
                    }
                }
                return this._displayName;
            }
            set => this._displayName = !string.IsNullOrEmpty(value) ? value : throw new ArgumentNullException(nameof(value));
        }

        /// <summary>Gets or sets the name of the member to validate.</summary>
        /// <returns>The name of the member to validate.</returns>
        public string MemberName
        {
            get => this._memberName;
            set => this._memberName = value;
        }

        /// <summary>Gets the dictionary of key/value pairs that is associated with this context.</summary>
        /// <returns>The dictionary of the key/value pairs for this context.</returns>
        public IDictionary<object, object> Items
        {
            get => (IDictionary<object, object>)this._items;
        }

        private string GetDisplayName()
        {
            string str = (string)null;
            ValidationAttributeStore instance = ValidationAttributeStore.Instance;
            DisplayAttribute displayAttribute = (DisplayAttribute)null;
            if (string.IsNullOrEmpty(this._memberName))
                displayAttribute = instance.GetTypeDisplayAttribute(this);
            else if (instance.IsPropertyContext(this))
                displayAttribute = instance.GetPropertyDisplayAttribute(this);
            if (displayAttribute != null)
                str = displayAttribute.GetName();
            return str ?? this.MemberName;
        }

        /// <summary>Initializes the <see cref="T:System.ComponentModel.DataAnnotations.ValidationContext" /> using a service provider that can return service instances by type when GetService is called.</summary>
        /// <param name="serviceProvider">The service provider.</param>
        public void InitializeServiceProvider(Func<Type, object> serviceProvider) => this._serviceProvider = serviceProvider;

        /// <summary>Returns the service that provides custom validation.</summary>
        /// <param name="serviceType">The type of the service to use for validation.</param>
        /// <returns>An instance of the service, or <see langword="null" /> if the service is not available.</returns>
        public object GetService(Type serviceType)
        {
            object service = (object)null;
            if (this._serviceContainer != null)
                service = this._serviceContainer.GetService(serviceType);
            if (service == null && this._serviceProvider != null)
                service = this._serviceProvider(serviceType);
            return service;
        }

        /// <summary>Gets the validation services container.</summary>
        /// <returns>The validation services container.</returns>
        public IServiceContainer ServiceContainer
        {
            get
            {
                if (this._serviceContainer == null)
                    this._serviceContainer = (IServiceContainer)new ValidationContext.ValidationContextServiceContainer();
                return this._serviceContainer;
            }
        }

        private class ValidationContextServiceContainer : IServiceContainer, IServiceProvider
        {
            private IServiceContainer _parentContainer;
            private Dictionary<Type, object> _services = new Dictionary<Type, object>();
            private readonly object _lock = new object();

            internal ValidationContextServiceContainer()
            {
            }

            internal ValidationContextServiceContainer(IServiceContainer parentContainer) => this._parentContainer = parentContainer;

            public void AddService(Type serviceType, ServiceCreatorCallback callback, bool promote)
            {
                if (promote && this._parentContainer != null)
                {
                    this._parentContainer.AddService(serviceType, callback, promote);
                }
                else
                {
                    lock (this._lock)
                    {
                        if (this._services.ContainsKey(serviceType))
                            throw new ArgumentException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "DataAnnotationsResources.ValidationContextServiceContainer_ItemAlreadyExists", new object[1]
                            {
                                (object) serviceType
                            }), nameof(serviceType));
                        this._services.Add(serviceType, (object)callback);
                    }
                }
            }

            public void AddService(Type serviceType, ServiceCreatorCallback callback) => this.AddService(serviceType, callback, true);

            public void AddService(Type serviceType, object serviceInstance, bool promote)
            {
                if (promote && this._parentContainer != null)
                {
                    this._parentContainer.AddService(serviceType, serviceInstance, promote);
                }
                else
                {
                    lock (this._lock)
                    {
                        if (this._services.ContainsKey(serviceType))
                            throw new ArgumentException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "DataAnnotationsResources.ValidationContextServiceContainer_ItemAlreadyExists", new object[1]
                            {
                                (object) serviceType
                            }), nameof(serviceType));
                        this._services.Add(serviceType, serviceInstance);
                    }
                }
            }

            public void AddService(Type serviceType, object serviceInstance) => this.AddService(serviceType, serviceInstance, true);

            public void RemoveService(Type serviceType, bool promote)
            {
                lock (this._lock)
                {
                    if (this._services.ContainsKey(serviceType))
                        this._services.Remove(serviceType);
                }
                if (!promote || this._parentContainer == null)
                    return;
                this._parentContainer.RemoveService(serviceType);
            }

            public void RemoveService(Type serviceType) => this.RemoveService(serviceType, true);

            public object GetService(Type serviceType)
            {
                if (serviceType == (Type)null)
                    throw new ArgumentNullException(nameof(serviceType));
                object service = (object)null;
                this._services.TryGetValue(serviceType, out service);
                if (service == null && this._parentContainer != null)
                    service = this._parentContainer.GetService(serviceType);
                if (service is ServiceCreatorCallback serviceCreatorCallback)
                    service = serviceCreatorCallback((IServiceContainer)this, serviceType);
                return service;
            }
        }
    }
}
#endif