#if NET35
using System.Globalization;

namespace System.ComponentModel.DataAnnotations
{
    /// <summary>Provides a general-purpose attribute that lets you specify localizable strings for types and members of entity partial classes.</summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    
    public sealed class DisplayAttribute : Attribute
    {
        private Type _resourceType;
        private LocalizableString _shortName = new LocalizableString(nameof(ShortName));
        private LocalizableString _name = new LocalizableString(nameof(Name));
        private LocalizableString _description = new LocalizableString(nameof(Description));
        private LocalizableString _prompt = new LocalizableString(nameof(Prompt));
        private LocalizableString _groupName = new LocalizableString(nameof(GroupName));
        private bool? _autoGenerateField;
        private bool? _autoGenerateFilter;
        private int? _order;

        /// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.DisplayAttribute" /> class.</summary>
        
        public DisplayAttribute()
        {
        }

        /// <summary>Gets or sets a value that is used for the grid column label.</summary>
        /// <returns>A value that is for the grid column label.</returns>
        
        public string ShortName
        {
            
            get => this._shortName.Value;
            
            set
            {
                if (!(this._shortName.Value != value))
                    return;
                this._shortName.Value = value;
            }
        }

        /// <summary>Gets or sets a value that is used for display in the UI.</summary>
        /// <returns>A value that is used for display in the UI.</returns>
        
        public string Name
        {
            
            get => this._name.Value;
            
            set
            {
                if (!(this._name.Value != value))
                    return;
                this._name.Value = value;
            }
        }

        /// <summary>Gets or sets a value that is used to display a description in the UI.</summary>
        /// <returns>The value that is used to display a description in the UI.</returns>
        
        public string Description
        {
            
            get => this._description.Value;
            
            set
            {
                if (!(this._description.Value != value))
                    return;
                this._description.Value = value;
            }
        }

        /// <summary>Gets or sets a value that will be used to set the watermark for prompts in the UI.</summary>
        /// <returns>A value that will be used to display a watermark in the UI.</returns>
        
        public string Prompt
        {
            
            get => this._prompt.Value;
            
            set
            {
                if (!(this._prompt.Value != value))
                    return;
                this._prompt.Value = value;
            }
        }

        /// <summary>Gets or sets a value that is used to group fields in the UI.</summary>
        /// <returns>A value that is used to group fields in the UI.</returns>
        
        public string GroupName
        {
            
            get => this._groupName.Value;
            
            set
            {
                if (!(this._groupName.Value != value))
                    return;
                this._groupName.Value = value;
            }
        }

        /// <summary>Gets or sets the type that contains the resources for the <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.ShortName" />, <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.Name" />, <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.Prompt" />, and <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.Description" /> properties.</summary>
        /// <returns>The type of the resource that contains the <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.ShortName" />, <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.Name" />, <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.Prompt" />, and <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.Description" /> properties.</returns>
        
        public Type ResourceType
        {
            
            get => this._resourceType;
            
            set
            {
                if (!(this._resourceType != value))
                    return;
                this._resourceType = value;
                this._shortName.ResourceType = value;
                this._name.ResourceType = value;
                this._description.ResourceType = value;
                this._prompt.ResourceType = value;
                this._groupName.ResourceType = value;
            }
        }

        /// <summary>Gets or sets a value that indicates whether UI should be generated automatically in order to display this field.</summary>
        /// <returns>
        /// <see langword="true" /> if UI should be generated automatically to display this field; otherwise, <see langword="false" />.</returns>
        /// <exception cref="T:System.InvalidOperationException">An attempt was made to get the property value before it was set.</exception>
        
        public bool AutoGenerateField
        {
            
            get => this._autoGenerateField.HasValue ? this._autoGenerateField.Value : throw new InvalidOperationException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "DataAnnotationsResources.DisplayAttribute_PropertyNotSet", new object[2]
            {
                (object) nameof (AutoGenerateField),
                (object) "GetAutoGenerateField"
            }));
            
            set => this._autoGenerateField = new bool?(value);
        }

        /// <summary>Gets or sets a value that indicates whether filtering UI is automatically displayed for this field.</summary>
        /// <returns>
        /// <see langword="true" /> if UI should be generated automatically to display filtering for this field; otherwise, <see langword="false" />.</returns>
        /// <exception cref="T:System.InvalidOperationException">An attempt was made to get the property value before it was set.</exception>
        
        public bool AutoGenerateFilter
        {
            
            get => this._autoGenerateFilter.HasValue ? this._autoGenerateFilter.Value : throw new InvalidOperationException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "DataAnnotationsResources.DisplayAttribute_PropertyNotSet", new object[2]
            {
                (object) nameof (AutoGenerateFilter),
                (object) "GetAutoGenerateFilter"
            }));
            
            set => this._autoGenerateFilter = new bool?(value);
        }

        /// <summary>Gets or sets the order weight of the column.</summary>
        /// <returns>The order weight of the column.</returns>
        
        public int Order
        {
            
            get => this._order.HasValue ? this._order.Value : throw new InvalidOperationException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, "DataAnnotationsResources.DisplayAttribute_PropertyNotSet", new object[2]
            {
                (object) nameof (Order),
                (object) "GetOrder"
            }));
            set => this._order = new int?(value);
        }

        /// <summary>Returns the value of the <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.ShortName" /> property.</summary>
        /// <returns>The localized string for the <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.ShortName" /> property if the <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.ResourceType" /> property has been specified and if the <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.ShortName" /> property represents a resource key; otherwise, the non-localized value of the <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.ShortName" /> value property.</returns>
        public string GetShortName() => this._shortName.GetLocalizableValue() ?? this.GetName();

        /// <summary>Returns a value that is used for field display in the UI.</summary>
        /// <returns>The localized string for the <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.Name" /> property, if the <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.ResourceType" /> property has been specified and the <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.Name" /> property represents a resource key; otherwise, the non-localized value of the <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.Name" /> property.</returns>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.ResourceType" /> property and the <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.Name" /> property are initialized, but a public static property that has a name that matches the <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.Name" /> value could not be found for the <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.ResourceType" /> property.</exception>
        public string GetName() => this._name.GetLocalizableValue();

        /// <summary>Returns the value of the <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.Description" /> property.</summary>
        /// <returns>The localized description, if the <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.ResourceType" /> has been specified and the <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.Description" /> property represents a resource key; otherwise, the non-localized value of the <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.Description" /> property.</returns>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.ResourceType" /> property and the <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.Description" /> property are initialized, but a public static property that has a name that matches the <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.Description" /> value could not be found for the <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.ResourceType" /> property.</exception>
        public string GetDescription() => this._description.GetLocalizableValue();

        /// <summary>Returns the value of the <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.Prompt" /> property.</summary>
        /// <returns>The localized string for the <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.Prompt" /> property if the <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.ResourceType" /> property has been specified and if the <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.Prompt" /> property represents a resource key; otherwise, the non-localized value of the <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.Prompt" /> property.</returns>
        public string GetPrompt() => this._prompt.GetLocalizableValue();

        /// <summary>Returns the value of the <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.GroupName" /> property.</summary>
        /// <returns>A value that will be used for grouping fields in the UI, if <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.GroupName" /> has been initialized; otherwise, <see langword="null" />. If the <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.ResourceType" /> property has been specified and the <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.GroupName" /> property represents a resource key, a localized string is returned; otherwise, a non-localized string is returned.</returns>
        public string GetGroupName() => this._groupName.GetLocalizableValue();

        /// <summary>Returns the value of the <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.AutoGenerateField" /> property.</summary>
        /// <returns>The value of <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.AutoGenerateField" /> if the property has been initialized; otherwise, <see langword="null" />.</returns>
        public bool? GetAutoGenerateField() => this._autoGenerateField;

        /// <summary>Returns a value that indicates whether UI should be generated automatically in order to display filtering for this field.</summary>
        /// <returns>The value of <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.AutoGenerateFilter" /> if the property has been initialized; otherwise, <see langword="null" />.</returns>
        public bool? GetAutoGenerateFilter() => this._autoGenerateFilter;

        /// <summary>Returns the value of the <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.Order" /> property.</summary>
        /// <returns>The value of the <see cref="P:System.ComponentModel.DataAnnotations.DisplayAttribute.Order" /> property, if it has been set; otherwise, <see langword="null" />.</returns>
        public int? GetOrder() => this._order;
    }
}
#endif