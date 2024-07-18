using Microsoft.AspNetCore.Mvc;
using MultiWeb.Data;
using MultiWeb.Models;

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

		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		public IActionResult Create(Category obj)
		{
			
			if (ModelState.IsValid)
			{
				m_db.Categories.Add(obj);
				m_db.SaveChanges();
				return RedirectToAction("Index");
			}
			return View();
		}
	}
}
