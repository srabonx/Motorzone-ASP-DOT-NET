using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Multi.DataAccess.Repository.IUnitOfWorks;
using Multi.Models.ViewModels;
using Multi.Models;
using Multi.Utility;

namespace MultiWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticData.Identity_Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork m_unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            m_unitOfWork = unitOfWork;
   
        }

        public IActionResult Index()
        {
            var companies = m_unitOfWork.CompanyRepo.GetAll();

            return View(companies);
        }

        public IActionResult Upsert(uint? id)
        {
            // Insert
            if (id == null || id == 0)
                return View(new Company());
            else
            {
                // Update
                var company = m_unitOfWork.CompanyRepo.Get(id) ?? new Company();

                return View(company);
            }
        }

        [HttpPost]
        public IActionResult Upsert(Company companyObj)
        {
            if (ModelState.IsValid)
            {
                // Add
                if (companyObj.Id == 0)
                {
                    m_unitOfWork.CompanyRepo.Add(companyObj);

                    m_unitOfWork.SaveChanges();

                    TempData["success"] = "Company added successfully";
                }
                else // Update
                {
                    m_unitOfWork.CompanyRepo.Update(companyObj);

                    m_unitOfWork.SaveChanges();

                    TempData["success"] = "Company updated successfully";
                }

                
                return RedirectToAction("Index");
            }
            else
            {
                return View(companyObj);
            }
        }

      
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> companies = m_unitOfWork.CompanyRepo.GetAll().ToList();
            return Json(new { data = companies });
        }

        [HttpDelete]
        public IActionResult Delete(uint? id)
        {
            var objToBeDeleted = m_unitOfWork.CompanyRepo.Get(id);

            if (objToBeDeleted == null)
                return Json(new { success = false, message = "Error deleting data!" });


            m_unitOfWork.CompanyRepo.Remove(objToBeDeleted);
            m_unitOfWork.SaveChanges();

            return Json(new { success = true, message = "Delete successful" });
        }

        #endregion


    }
}
