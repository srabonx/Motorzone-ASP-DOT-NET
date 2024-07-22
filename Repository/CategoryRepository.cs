using Microsoft.EntityFrameworkCore;
using MultiWeb.Data;
using MultiWeb.Models;
using MultiWeb.Repository.IRepository;
using System.Linq.Expressions;

namespace MultiWeb.Repository
{
	public class CategoryRepository : Repository<Category>, ICategoryRepository
	{
		private readonly ApplicationDbContext m_db;
		private readonly DbSet<Category> m_dbSet;
        public CategoryRepository(ApplicationDbContext db) : base(db)
        {
            m_db = db;
			m_dbSet = m_db.Set<Category>();
        }

		public void Update(Category category)
		{
			m_dbSet.Update(category);
		}
	}
}
