using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApplication.Domain.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>
        /// Adds a new entity of type <typeparamref name="T"/> to the repository asynchronously.
        /// </summary>
        /// <param name="entity">The entity to be added to the repository.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result is the added entity.
        /// </returns>
        Task<T> AddAsync(T entity);

        
        ///<summary>
        /// Removes the specified entity from the database.
        /// </summary>
        /// <param name="entity">The entity to be removed.</param>
        /// <returns>True if the removal was successful, false otherwise.</returns>
        bool Remove(T entity);

        /// <summary>
        /// Updates an existing entity in the database by attaching it to the current data context
        /// and marking it as modified. This method is typically used for making changes to an
        /// entity's properties before saving those changes to the database.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <returns>
        ///   <c>true</c> if the entity was successfully attached and marked as modified;
        ///   otherwise, <c>false</c> if an error occurred during the update process.
        /// </returns>
        bool Update(T entity);

        ///<summary>
        /// Retrieves an entity of type T by its unique identifier asynchronously.
        ///</summary>
        ///<param name="id">The unique identifier of the entity to retrieve.</param>
        ///<returns>
        /// A Task that represents the asynchronous operation. The task result contains
        /// the entity with the specified identifier, or null if not found.
        ///</returns>
        Task<T> GetByIdAsync(int id);
    }
}
