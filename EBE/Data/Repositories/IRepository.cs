using System;
using System.Collections.Generic;

namespace EBE.Data
{
    /// <summary>
    /// Repository interface to database.
    /// </summary>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Add the specified item to the database.
        /// </summary>
        void Add(T item);

        /// <summary>
        /// Update the specified item in the database.
        /// </summary>
        void Update(T item);

        /// <summary>
        /// Remove the specified item from the database.
        /// </summary>
        void Remove(T item);

        /// <summary>
        /// Finds item by identifier in database and returns it.
        /// </summary>
        /// <returns>The found item.</returns>
        /// <param name="id">Unique identifier.</param>
        T FindById(Guid id);

        /// <summary>
        /// CHecks whether item exists in database.
        /// </summary>
        /// <param name="id">Unique identifier.</param>
        bool Exists(Guid id);

        /// <summary>
        /// Finds items that match the given property with the given value.
        /// </summary>
        /// <param name="propertyName">Property name to look at.</param>
        /// <param name="value">Value for the property.</param>
        /// <returns>List of items that match.</returns>
        List<T> Find(string propertyName, object value);

        /// <summary>
        /// Finds items that matches the given properties with the given values.
        /// </summary>
        /// <param name="propertyName">Property name to look at.</param>
        /// <param name="value">Value for the property.</param>
        /// <param name="propertyName2">Second property name to look at.</param>
        /// <param name="value2">Value for the second property.</param>
        /// <returns>List of items that match.</returns>
        List<T> Find(string propertyName, object value, string propertyName2, object value2);
    }
}
