using Andrew.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Andrew.GenericRepository
{
    public class GenericRepository<TEntity> where TEntity : class
    {
        internal EFDBContext context;
        internal DbSet<TEntity> dbSet;

        public GenericRepository(EFDBContext context)
        {
            this.context = context;
            this.dbSet = context.Set<TEntity>();
        }

        public virtual async Task<IEnumerable<TEntity>> GetAsync()
        {
            IQueryable<TEntity> query = dbSet;
            return await query.ToListAsync();
        }

        public virtual async Task<TEntity> GetByIDAsync(object id)
        {
            return await dbSet.FindAsync(id);
        }

        public virtual IList<TEntity> FindAllBy(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate)
        {
            return context.Set<TEntity>().Where(predicate).ToList();
        }

        public virtual IQueryable<TEntity> FindAllByQuery(System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate)
        {
            return context.Set<TEntity>().Where(predicate);
        }

        public virtual async Task<TEntity> InsertAsync(TEntity entity)
        {
            await dbSet.AddAsync(entity);
            context.SaveChanges();
            return entity;
        }

        public virtual async Task<List<TEntity>> BulkInsert(List<TEntity> entity)
        {
            dbSet.AddRange(entity);
            await context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<bool> Delete(object id)
        {
            try
            {
                TEntity entityToDelete = await dbSet.FindAsync(id);
                Delete(entityToDelete);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public virtual void Delete(TEntity entityToDelete)
        {
            if (context.Entry(entityToDelete).State == EntityState.Detached)
            {
                dbSet.Attach(entityToDelete);
            }
            dbSet.Remove(entityToDelete);
        }

        public virtual async Task<bool> DeleteCollectionAsync(List<TEntity> entities)
        {
            try
            {
                dbSet.RemoveRange(entities);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public virtual async Task<TEntity> UpdateAsync(TEntity entityToUpdate)
        {
            dbSet.Attach(entityToUpdate);
            context.Entry(entityToUpdate).State = EntityState.Modified;
            await context.SaveChangesAsync();
            return entityToUpdate;
        }
    }
}
