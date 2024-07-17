using Microsoft.AspNetCore.Mvc;
using MultiWeb.Data;

namespace MultiWeb.Controllers
{
	public class CategoryController : Controller
	{
		private readonly ApplicationDbContext m_db;

		public CategoryController(ApplicationDbContext db)
		{
			m_db = db;
		}
		public IActionResult Index()
		{
			var objCategoryList = m_db.Categories.ToList();
			return View(objCategoryList); 
		}
	}
}
