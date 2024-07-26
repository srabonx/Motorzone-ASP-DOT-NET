using MultiWeb.Models;

namespace MultiWeb.Repository.IRepository
{
    public interface ICategoryRepository : IRepository<Category>
	{
		void Update(Category category);
	}
}
