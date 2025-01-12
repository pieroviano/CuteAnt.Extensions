﻿// <auto-generated />
namespace Microsoft.Extensions.Configuration.Binder
{
    using System.Globalization;
    using System.Reflection;
    using System.Resources;

    public static class Resources
    {
        private static readonly ResourceManager _resourceManager = ConfigBinderSR.ResourceManager;

        /// <summary>
        /// Cannot create instance of type '{0}' because it is either abstract or an interface.
        /// </summary>
        public static string Error_CannotActivateAbstractOrInterface
        {
            get => ConfigBinderSR.ResourceManager.GetString("Error_CannotActivateAbstractOrInterface");
        }

        /// <summary>
        /// Cannot create instance of type '{0}' because it is either abstract or an interface.
        /// </summary>
        public static string FormatError_CannotActivateAbstractOrInterface(object p0)
            => string.Format(CultureInfo.CurrentCulture, ConfigBinderSR.ResourceManager.GetString("Error_CannotActivateAbstractOrInterface"), p0);

        /// <summary>
        /// Failed to convert '{0}' to type '{1}'.
        /// </summary>
        public static string Error_FailedBinding
        {
            get => ConfigBinderSR.ResourceManager.GetString("Error_FailedBinding");
        }

        /// <summary>
        /// Failed to convert '{0}' to type '{1}'.
        /// </summary>
        public static string FormatError_FailedBinding(object p0, object p1)
            => string.Format(CultureInfo.CurrentCulture, ConfigBinderSR.ResourceManager.GetString("Error_FailedBinding"), p0, p1);

        /// <summary>
        /// Failed to create instance of type '{0}'.
        /// </summary>
        public static string Error_FailedToActivate
        {
            get => ConfigBinderSR.ResourceManager.GetString("Error_FailedToActivate");
        }

        /// <summary>
        /// Failed to create instance of type '{0}'.
        /// </summary>
        public static string FormatError_FailedToActivate(object p0)
            => string.Format(CultureInfo.CurrentCulture, ConfigBinderSR.ResourceManager.GetString("Error_FailedToActivate"), p0);

        /// <summary>
        /// Cannot create instance of type '{0}' because it is missing a public parameterless constructor.
        /// </summary>
        public static string Error_MissingParameterlessConstructor
        {
            get => ConfigBinderSR.ResourceManager.GetString("Error_MissingParameterlessConstructor");
        }

        /// <summary>
        /// Cannot create instance of type '{0}' because it is missing a public parameterless constructor.
        /// </summary>
        public static string FormatError_MissingParameterlessConstructor(object p0)
            => string.Format(CultureInfo.CurrentCulture, ConfigBinderSR.ResourceManager.GetString("Error_MissingParameterlessConstructor"), p0);

        /// <summary>
        /// Cannot create instance of type '{0}' because multidimensional arrays are not supported.
        /// </summary>
        public static string Error_UnsupportedMultidimensionalArray
        {
            get => ConfigBinderSR.ResourceManager.GetString("Error_UnsupportedMultidimensionalArray");
        }

        /// <summary>
        /// Cannot create instance of type '{0}' because multidimensional arrays are not supported.
        /// </summary>
        public static string FormatError_UnsupportedMultidimensionalArray(object p0)
            => string.Format(CultureInfo.CurrentCulture, ConfigBinderSR.ResourceManager.GetString("Error_UnsupportedMultidimensionalArray"), p0);

    }
}
