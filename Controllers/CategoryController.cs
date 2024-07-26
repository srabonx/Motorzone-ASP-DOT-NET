using Microsoft.AspNetCore.Mvc;
using MultiWeb.Data;
using MultiWeb.Models;
using MultiWeb.Repository.IRepository;

namespace MultiWeb.Controllers;

public class CategoryController : Controller
{
	private readonly IUnitOfWork m_unitOfWork;

	public CategoryController(IUnitOfWork unitOfWork)
	{
		m_unitOfWork = unitOfWork;
	}
	public IActionResult Index()
	{
		var objCategoryList = m_unitOfWork.CategoryRepository.GetAll().ToList();
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
			m_unitOfWork.CategoryRepository.Add(obj);
			m_unitOfWork.Save();
			TempData["success"] = "Category created successfully";
			return RedirectToAction("Index");
		}
		return View();
	}

	public IActionResult Edit(int? id)
	{
		if (id == null || id == 0) return NotFound();

		Category? categoryFromDb = m_unitOfWork.CategoryRepository.Get(e => e.Id == id);

		if(categoryFromDb == null) return NotFound();

		return View(categoryFromDb);
	}

	[HttpPost]
	public IActionResult Edit(Category obj)
	{

		if (ModelState.IsValid)
		{
			m_unitOfWork.CategoryRepository.Update(obj);
			m_unitOfWork.Save();
			TempData["success"] = "Category updated successfully";
			return RedirectToAction("Index");
		}
		return View();
	}

	public IActionResult Delete(int? id)
	{
		if (id == null || id == 0) return NotFound();

		Category? categoryFromDb = m_unitOfWork.CategoryRepository.Get(e => e.Id == id);

		if (categoryFromDb == null) return NotFound();

		return View(categoryFromDb);
	}

	[HttpPost]
	public IActionResult Delete(Category obj)
	{
		m_unitOfWork.CategoryRepository.Remove(obj);
		m_unitOfWork.Save();
		TempData["success"] = "Category deleted successfully";
		return RedirectToAction("Index");
	}

}
