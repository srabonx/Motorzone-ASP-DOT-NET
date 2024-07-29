using Microsoft.AspNetCore.Mvc;
using Multi.DataAccess.Repository.IUnitOfWorks;
using Multi.Models;

namespace MultiWeb.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class ProductController : Controller
	{
		private readonly IUnitOfWork m_unitOfWork;

		public ProductController(IUnitOfWork unitOfWork)
		{
			m_unitOfWork = unitOfWork;
		}

		public IActionResult Index()
		{
			var productBike = m_unitOfWork.ProductBikeRepo.GetAll();
			return View(productBike);
		}

		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		public IActionResult Create(ProductBike obj)
		{
			if(ModelState.IsValid)
			{
				m_unitOfWork.ProductBikeRepo.Add(obj);
				m_unitOfWork.SaveChanges();

				TempData["success"] = "Product created successfully";
				return RedirectToAction("Index");
			}

			return View();
		}

		public IActionResult Edit(int? id)
		{
			if (id == null || id == 0) return NotFound();

			ProductBike? obj = m_unitOfWork.ProductBikeRepo.Get(id);

			if(obj == null) return NotFound();

			return View(obj);
		}

		[HttpPost]
		public IActionResult Edit(ProductBike obj)
		{
			if (ModelState.IsValid)
			{
				m_unitOfWork.ProductBikeRepo.Update(obj);
				m_unitOfWork.SaveChanges();

				TempData["success"] = "Product updated successfully";
				return RedirectToAction("Index");
			}

			return View();
		}

		public IActionResult Delete(int? id)
		{
			if (id == null || id == 0) return NotFound();

			ProductBike? obj = m_unitOfWork.ProductBikeRepo.Get(id);

			if (obj == null) return NotFound();

			return View(obj);
		}

		[HttpPost]
		public IActionResult Delete(ProductBike obj)
		{

			m_unitOfWork.ProductBikeRepo.Remove(obj);
			m_unitOfWork.SaveChanges();

			TempData["success"] = "Product deleted successfully";
			return RedirectToAction("Index");
		}

	}
}
