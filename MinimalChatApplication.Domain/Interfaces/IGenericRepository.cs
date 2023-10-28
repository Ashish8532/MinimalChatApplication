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


        /// <summary>
        /// Asynchronously retrieves the first entity from the repository that matches a specified condition.
        /// </summary>
        /// <param name="filter">A filter expression specifying the condition for entity selection.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the first entity
        /// that satisfies the provided condition, or null if no matching entity is found.
        /// </returns>
        Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter);


        /// <summary>
        /// Asynchronously retrieves a collection of entities from the repository that match a specified condition.
        /// </summary>
        /// <param name="filter">A filter expression specifying the condition for entity selection.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a collection of entities
        /// that satisfy the provided condition.
        /// </returns>
        Task<IEnumerable<T>> GetByConditionAsync(Expression<Func<T, bool>> filter);


        /// <summary>
        /// Retrieves all entities in the repository, optionally including related entities.
        /// </summary>
        /// <param name="include">An expression representing related entities to include in the query.</param>
        /// <returns>
        /// A collection of entities retrieved from the repository.
        /// </returns>
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, object>> include = null);


        /// <summary>
        /// Asynchronously saves changes to the database, persisting any pending modifications.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SaveChangesAsync();
    }
}
