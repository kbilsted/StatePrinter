using System;

namespace StatePrinter.ValueConverters
{
    /// <summary>
    /// A general converter, taking a lambda as configuration
    /// </summary>
    public class GenericValueConverter<T> : IValueConverter
    {
        readonly Func<T, string> convert;

        public GenericValueConverter(Func<T, string> convert)
        {
            this.convert = convert;
        }

        /// <summary>
        /// Is the type covered by this handler.
        /// </summary>
        public bool CanHandleType(Type type)
        {
            return type == typeof(T);
        }

        /// <summary>
        /// Convert objects of handled types into a simple one-line representation.
        /// </summary>
        public string Convert(object source)
        {
            return convert((T)source);
        }
    }
}
