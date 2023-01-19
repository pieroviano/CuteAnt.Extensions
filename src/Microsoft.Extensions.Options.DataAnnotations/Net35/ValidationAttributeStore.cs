#if NET35
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace System.ComponentModel.DataAnnotations
{
    internal class ValidationAttributeStore
    {
        private static ValidationAttributeStore _singleton = new ValidationAttributeStore();
        private Dictionary<Type, ValidationAttributeStore.TypeStoreItem> _typeStoreItems = new Dictionary<Type, ValidationAttributeStore.TypeStoreItem>();

        internal static ValidationAttributeStore Instance => ValidationAttributeStore._singleton;

        internal IEnumerable<ValidationAttribute> GetTypeValidationAttributes(
            ValidationContext validationContext)
        {
            ValidationAttributeStore.EnsureValidationContext(validationContext);
            return this.GetTypeStoreItem(validationContext.ObjectType).ValidationAttributes;
        }

        internal DisplayAttribute GetTypeDisplayAttribute(
            ValidationContext validationContext)
        {
            ValidationAttributeStore.EnsureValidationContext(validationContext);
            return this.GetTypeStoreItem(validationContext.ObjectType).DisplayAttribute;
        }

        internal IEnumerable<ValidationAttribute> GetPropertyValidationAttributes(
            ValidationContext validationContext)
        {
            ValidationAttributeStore.EnsureValidationContext(validationContext);
            return this.GetTypeStoreItem(validationContext.ObjectType).GetPropertyStoreItem(validationContext.MemberName).ValidationAttributes;
        }

        internal DisplayAttribute GetPropertyDisplayAttribute(
            ValidationContext validationContext)
        {
            ValidationAttributeStore.EnsureValidationContext(validationContext);
            return this.GetTypeStoreItem(validationContext.ObjectType).GetPropertyStoreItem(validationContext.MemberName).DisplayAttribute;
        }

        internal Type GetPropertyType(ValidationContext validationContext)
        {
            ValidationAttributeStore.EnsureValidationContext(validationContext);
            return this.GetTypeStoreItem(validationContext.ObjectType).GetPropertyStoreItem(validationContext.MemberName).PropertyType;
        }

        internal bool IsPropertyContext(ValidationContext validationContext)
        {
            ValidationAttributeStore.EnsureValidationContext(validationContext);
            ValidationAttributeStore.TypeStoreItem typeStoreItem = this.GetTypeStoreItem(validationContext.ObjectType);
            ValidationAttributeStore.PropertyStoreItem propertyStoreItem = (ValidationAttributeStore.PropertyStoreItem)null;
            return typeStoreItem.TryGetPropertyStoreItem(validationContext.MemberName, out propertyStoreItem);
        }

        private ValidationAttributeStore.TypeStoreItem GetTypeStoreItem(
            Type type)
        {
            if (type == (Type)null)
                throw new ArgumentNullException(nameof(type));
            lock (this._typeStoreItems)
            {
                ValidationAttributeStore.TypeStoreItem typeStoreItem = (ValidationAttributeStore.TypeStoreItem)null;
                if (!this._typeStoreItems.TryGetValue(type, out typeStoreItem))
                {
                    IEnumerable<Attribute> attributes = TypeDescriptor.GetAttributes(type).Cast<Attribute>();
                    typeStoreItem = new ValidationAttributeStore.TypeStoreItem(type, attributes);
                    this._typeStoreItems[type] = typeStoreItem;
                }
                return typeStoreItem;
            }
        }

        private static void EnsureValidationContext(ValidationContext validationContext)
        {
            if (validationContext == null)
                throw new ArgumentNullException(nameof(validationContext));
        }

        private abstract class StoreItem
        {
            private static IEnumerable<ValidationAttribute> _emptyValidationAttributeEnumerable = (IEnumerable<ValidationAttribute>)new ValidationAttribute[0];
            private IEnumerable<ValidationAttribute> _validationAttributes;

            internal StoreItem(IEnumerable<Attribute> attributes)
            {
                this._validationAttributes = attributes.OfType<ValidationAttribute>();
                this.DisplayAttribute = attributes.OfType<DisplayAttribute>().SingleOrDefault<DisplayAttribute>();
            }

            internal IEnumerable<ValidationAttribute> ValidationAttributes => this._validationAttributes;

            internal DisplayAttribute DisplayAttribute { get; set; }
        }

        private class TypeStoreItem : ValidationAttributeStore.StoreItem
        {
            private object _syncRoot = new object();
            private Type _type;
            private Dictionary<string, ValidationAttributeStore.PropertyStoreItem> _propertyStoreItems;

            internal TypeStoreItem(Type type, IEnumerable<Attribute> attributes)
                : base(attributes)
            {
                this._type = type;
            }

            internal ValidationAttributeStore.PropertyStoreItem GetPropertyStoreItem(
                string propertyName)
            {
                ValidationAttributeStore.PropertyStoreItem propertyStoreItem = (ValidationAttributeStore.PropertyStoreItem)null;
                if (!this.TryGetPropertyStoreItem(propertyName, out propertyStoreItem))
                    throw new ArgumentException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "DataAnnotationsResources.AttributeStore_Unknown_Property", new object[2]
                    {
                        (object) this._type.Name,
                        (object) propertyName
                    }), nameof(propertyName));
                return propertyStoreItem;
            }

            internal bool TryGetPropertyStoreItem(
                string propertyName,
                out ValidationAttributeStore.PropertyStoreItem item)
            {
                if (string.IsNullOrEmpty(propertyName))
                    throw new ArgumentNullException(nameof(propertyName));
                if (this._propertyStoreItems == null)
                {
                    lock (this._syncRoot)
                    {
                        if (this._propertyStoreItems == null)
                            this._propertyStoreItems = this.CreatePropertyStoreItems();
                    }
                }
                return this._propertyStoreItems.TryGetValue(propertyName, out item);
            }

            private Dictionary<string, ValidationAttributeStore.PropertyStoreItem> CreatePropertyStoreItems()
            {
                Dictionary<string, ValidationAttributeStore.PropertyStoreItem> propertyStoreItems = new Dictionary<string, ValidationAttributeStore.PropertyStoreItem>();
                foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(this._type))
                {
                    ValidationAttributeStore.PropertyStoreItem propertyStoreItem = new ValidationAttributeStore.PropertyStoreItem(property.PropertyType, ValidationAttributeStore.TypeStoreItem.GetExplicitAttributes(property).Cast<Attribute>());
                    propertyStoreItems[property.Name] = propertyStoreItem;
                }
                return propertyStoreItems;
            }

            public static AttributeCollection GetExplicitAttributes(
                PropertyDescriptor propertyDescriptor)
            {
                List<Attribute> attributeList = new List<Attribute>(propertyDescriptor.Attributes.Cast<Attribute>());
                IEnumerable<Attribute> attributes = TypeDescriptor.GetAttributes(propertyDescriptor.PropertyType).Cast<Attribute>();
                bool flag = false;
                foreach (Attribute attribute in attributes)
                {
                    for (int index = attributeList.Count - 1; index >= 0; --index)
                    {
                        if (attribute == attributeList[index])
                        {
                            attributeList.RemoveAt(index);
                            flag = true;
                        }
                    }
                }
                return !flag ? propertyDescriptor.Attributes : new AttributeCollection(attributeList.ToArray());
            }
        }

        private class PropertyStoreItem : ValidationAttributeStore.StoreItem
        {
            private Type _propertyType;

            internal PropertyStoreItem(Type propertyType, IEnumerable<Attribute> attributes)
                : base(attributes)
            {
                this._propertyType = propertyType;
            }

            internal Type PropertyType => this._propertyType;
        }
    }
}
#endif