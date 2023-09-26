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


        ///<summary>
        /// Retrieves an entity of type T by its unique identifier asynchronously.
        ///</summary>
        ///<param name="id">The unique identifier of the entity to retrieve.</param>
        ///<returns>
        /// A Task that represents the asynchronous operation. The task result contains
        /// the entity with the specified identifier, or null if not found.
        ///</returns>
        public async Task<T> GetByIdAsync(int id)
        {
            var data = await dbSet.FindAsync(id);
            return data;
        }
    }
}
