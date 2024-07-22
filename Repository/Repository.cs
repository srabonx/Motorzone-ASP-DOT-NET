using Microsoft.EntityFrameworkCore;
using MultiWeb.Data;
using MultiWeb.Repository.IRepository;
using System.Linq.Expressions;

namespace MultiWeb.Repository
{
	public class Repository<T> : IRepository<T> where T : class
	{
		private readonly ApplicationDbContext m_db;
		private readonly DbSet<T> m_dbSet;

        public Repository(ApplicationDbContext db)
        {
			m_db = db;
			m_dbSet = m_db.Set<T>();
        }

        public void Add(T entity)
		{
			m_dbSet.Add(entity);	
		}

		public T Get(Expression<Func<T, bool>> filter)
		{
			IQueryable<T> query = m_dbSet;
			query = query.Where(filter);

			return query.FirstOrDefault();
		}

		public IEnumerable<T> GetAll()
		{
			IQueryable<T> query = m_dbSet;
			return query.ToList();
		}

		public void Remove(T entity)
		{
			m_dbSet.Remove(entity);
		}

		public void RemoveRange(IEnumerable<T> entities)
		{
			m_dbSet.RemoveRange(entities);
		}
	}
}
