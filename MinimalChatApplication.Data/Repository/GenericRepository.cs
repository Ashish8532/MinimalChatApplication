using Microsoft.EntityFrameworkCore;
using MinimalChatApplication.Data.Context;
using MinimalChatApplication.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MinimalChatApplication.Data.Repository
{
    ///<summary>
    /// This is a generic repository class that provides basic CRUD operations for entities of type T.
    /// It encapsulates database access and simplifies data access operations.
    ///</summary>
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {

        private readonly ChatApplicationDbContext _context;
        internal DbSet<T> dbSet;

        ///<summary>
        /// Initializes a new instance of the GenericRepository class.
        ///</summary>
        ///<param name="context">The application's database context, which is used to interact with the database.</param>
        public GenericRepository(ChatApplicationDbContext context)
        {
            _context = context;
            dbSet = _context.Set<T>();
        }

        /// <summary>
        /// Adds a new entity of type <typeparamref name="T"/> to the repository asynchronously.
        /// </summary>
        /// <param name="entity">The entity to be added to the repository.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result is the added entity.
        /// </returns>
        public async Task<T> AddAsync(T entity)
        {
            await dbSet.AddAsync(entity);
            return entity;
        }


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
        public bool Update(T entity)
        {
            dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            return true;
        }


        ///<summary>
        /// Removes the specified entity from the database.
        /// </summary>
        /// <param name="entity">The entity to be removed.</param>
        /// <returns>True if the removal was successful, false otherwise.</returns>
        public bool Remove(T entity)
        {
            dbSet.Remove(entity);
            return true;
        }


        /// <summary>
        /// Asynchronously retrieves the first entity from the repository that matches a specified condition and includes related entities.
        /// </summary>
        /// <param name="filter">A filter expression specifying the condition for entity selection.</param>
        /// <param name="includes">Optional navigation properties to be included with the entity.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the first entity
        /// that satisfies the provided condition, or null if no matching entity is found.
        /// </returns>
        public async Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = dbSet;
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return await query.FirstOrDefaultAsync(filter);
        }



        /// <summary>
        /// Asynchronously retrieves a collection of entities from the repository that match a specified condition and includes related entities.
        /// </summary>
        /// <param name="filter">A filter expression specifying the condition for entity selection.</param>
        /// <param name="includes">Optional navigation properties to be included with the entities.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a collection of entities
        /// that satisfy the provided condition.
        /// </returns>
        public async Task<IEnumerable<T>> GetByConditionAsync(Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] includes)
        {
            var query = dbSet.Where(filter);

            // Apply includes
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.ToListAsync();
        }


        /// <summary>
        /// Retrieves all entities in the repository, optionally including related entities.
        /// </summary>
        /// <param name="include">An expression representing related entities to include in the query.</param>
        /// <returns>
        /// A collection of entities retrieved from the repository.
        /// </returns>
        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, object>> include = null)
        {
            IQueryable<T> query = dbSet;

            if (include != null)
            {
                query = query.Include(include);
            }
            return await query.ToListAsync();
        }



        /// <summary>
        /// Asynchronously saves changes to the database, persisting any pending modifications.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
