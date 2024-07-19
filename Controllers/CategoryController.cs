using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiWeb.Data;
using MultiWeb.Models;

namespace MultiWeb.Controllers;

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
			TempData["success"] = "Category created successfully";
			return RedirectToAction("Index");
		}
		return View();
	}

	public IActionResult Edit(int? id)
	{
		if (id == null || id == 0) return NotFound();

		Category? categoryFromDb = m_db.Categories.Find(id);

		if(categoryFromDb == null) return NotFound();

		return View(categoryFromDb);
	}

	[HttpPost]
	public IActionResult Edit(Category obj)
	{

		if (ModelState.IsValid)
		{
			m_db.Categories.Update(obj);
			m_db.SaveChanges();
			TempData["success"] = "Category updated successfully";
			return RedirectToAction("Index");
		}
		return View();
	}

	public IActionResult Delete(int? id)
	{
		if (id == null || id == 0) return NotFound();

		Category? categoryFromDb = m_db.Categories.Find(id);

		if (categoryFromDb == null) return NotFound();

		return View(categoryFromDb);
	}

	[HttpPost]
	public IActionResult Delete(Category obj)
	{
		m_db.Categories.Remove(obj);
		m_db.SaveChanges();
		TempData["success"] = "Category deleted successfully";
		return RedirectToAction("Index");
	}

}
