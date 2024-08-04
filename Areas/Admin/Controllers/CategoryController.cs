using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multi.DataAccess.Data;
using Multi.DataAccess.Repository;
using Multi.DataAccess.Repository.IUnitOfWorks;
using Multi.Models;
using Multi.Utility;

namespace MultiWeb.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = StaticData.Identity_Role_Admin)]
public class CategoryController : Controller
{
    private readonly IUnitOfWork m_unitOfWork;

    public CategoryController(IUnitOfWork unitOfWork)
    {
		m_unitOfWork = unitOfWork;
    }
    public IActionResult Index()
    {
        var objCategoryList = m_unitOfWork.CategoryRepo.GetAll();
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
			m_unitOfWork.CategoryRepo.Add(obj);
			m_unitOfWork.SaveChanges();

            TempData["success"] = "Category created successfully";
            return RedirectToAction("Index");
        }
        return View();
    }

    public IActionResult Edit(int? id)
    {
        if (id == null || id == 0) return NotFound();

        Category? categoryFromDb = m_unitOfWork.CategoryRepo.Get((uint)id);

        if (categoryFromDb == null) return NotFound();

        return View(categoryFromDb);
    }

    [HttpPost]
    public IActionResult Edit(Category obj)
    {

        if (ModelState.IsValid)
        {
			m_unitOfWork.CategoryRepo.Update(obj);
			m_unitOfWork.SaveChanges();
            TempData["success"] = "Category updated successfully";
            return RedirectToAction("Index");
        }
        return View();
    }

    public IActionResult Delete(int? id)
    {
        if (id == null || id == 0) return NotFound();

        Category? categoryFromDb = m_unitOfWork.CategoryRepo.Get((uint)id);

        if (categoryFromDb == null) return NotFound();

        return View(categoryFromDb);
    }

    [HttpPost]
    public IActionResult Delete(Category obj)
    {
		m_unitOfWork.CategoryRepo.Remove(obj);
		m_unitOfWork.SaveChanges();
        TempData["success"] = "Category deleted successfully";
        return RedirectToAction("Index");
    }

}
