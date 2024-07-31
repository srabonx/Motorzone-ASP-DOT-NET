using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Multi.DataAccess.Repository.IUnitOfWorks;
using Multi.Models;
using Multi.Models.ViewModels;

namespace MultiWeb.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class ProductController : Controller
	{
		private readonly IUnitOfWork m_unitOfWork;
		private readonly IWebHostEnvironment m_webHostEnvironment;

		public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
		{
			m_unitOfWork = unitOfWork;
			m_webHostEnvironment = webHostEnvironment;
		}

		public IActionResult Index()
		{
			var productBike = m_unitOfWork.ProductBikeRepo.GetAll(includeProp:"Category");

			return View(productBike);
		}

		public IActionResult Upsert(uint? id)
		{
            IEnumerable<SelectListItem> categoryList = m_unitOfWork.CategoryRepo.GetAll().Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()
            });

			ViewBag.CategoryList = categoryList;

			ProductVM productVM = new()
			{
				ProductBike = new ProductBike(),
				CategoryList = categoryList
			};

			// Insert
			if(id == null || id == 0)
				return View(productVM);
			else
			{
				// Update
				productVM.ProductBike = m_unitOfWork.ProductBikeRepo.Get(id);

				return View(productVM);
			}
		}

		[HttpPost]
		public IActionResult Upsert(ProductVM productVM, IFormFile? file)
		{
			if(ModelState.IsValid)
			{
				string webRootPath = m_webHostEnvironment.WebRootPath;

				if(file != null)
				{
					string extension = Path.GetExtension(file.FileName);
					string fileName = Guid.NewGuid().ToString() + Path.Combine() + extension;
					string productPath = Path.Combine(webRootPath, @"images\product");

					if(!string.IsNullOrEmpty(productVM.ProductBike.ImageUrl))
					{
						// Delete old image
						var oldImgPath = Path.Combine(webRootPath, productVM.ProductBike.ImageUrl.TrimStart('\\'));

						if(System.IO.File.Exists(oldImgPath))
						{
							System.IO.File.Delete(oldImgPath);
						}
					}

					using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
					{
						file.CopyTo(fileStream);
					}

					productVM.ProductBike.ImageUrl = @"\images\product\" + fileName;

				}

				// Add
				if (productVM.ProductBike.Id == 0)
					m_unitOfWork.ProductBikeRepo.Add(productVM.ProductBike);
				else // Update
					m_unitOfWork.ProductBikeRepo.Update(productVM.ProductBike);

				m_unitOfWork.SaveChanges();

				TempData["success"] = "Product added successfully";
				return RedirectToAction("Index");
			}
			else
			{
				productVM.CategoryList = m_unitOfWork.CategoryRepo.GetAll().Select(c => new SelectListItem
				{
					Text = c.Name,
					Value = c.Id.ToString()
				});

				return View(productVM);
			}			
		}

		/*public IActionResult Edit(uint? id)
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
		}*/

		/*public IActionResult Delete(uint? id)
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
		}*/

		#region API CALLS
		[HttpGet]
		public IActionResult GetAll()
		{
            List<ProductBike> productBike = m_unitOfWork.ProductBikeRepo.GetAll(includeProp: "Category").ToList();
			return Json(new {data = productBike});
        }

		[HttpDelete]
		public IActionResult Delete(uint? id)
		{
			var objToBeDeleted = m_unitOfWork.ProductBikeRepo.Get(id);

			if (objToBeDeleted == null)
				return Json(new { success = false, message = "Error deleting data!" });
	
			if(!string.IsNullOrEmpty(objToBeDeleted.ImageUrl))
			{
                var oldImgPath = Path.Combine(m_webHostEnvironment.WebRootPath, objToBeDeleted.ImageUrl.TrimStart('\\'));

                if (System.IO.File.Exists(oldImgPath))
                {
                    System.IO.File.Delete(oldImgPath);
                }
            }

			m_unitOfWork.ProductBikeRepo.Remove(objToBeDeleted);
			m_unitOfWork.SaveChanges();

			return Json(new { success = true, message = "Delete successful" });
		}

        #endregion

    }
}
