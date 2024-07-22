using MultiWeb.Data;
using MultiWeb.Repository.IRepository;

namespace MultiWeb.Repository
{
	public class UnitOfWork : IUnitOfWork
	{
		public ICategoryRepository CategoryRepository { get; private set; }

		private ApplicationDbContext m_db;

		public UnitOfWork(ApplicationDbContext db)
		{
			m_db = db;
			CategoryRepository = new CategoryRepository(db);
		}

		public void Save()
		{
			m_db.SaveChanges();
		}
	}
}
