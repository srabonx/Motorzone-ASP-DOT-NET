using Microsoft.AspNetCore.Mvc;
using Multi.DataAccess.Repository.IUnitOfWorks;
using Multi.Utility;
using System.Security.Claims;

namespace MultiWeb.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork m_unitOfWork;
        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            m_unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity?)User.Identity;

            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                if(HttpContext.Session.GetInt32(StaticData.SessionCart) == null)
                {
                    HttpContext.Session.SetInt32(StaticData.SessionCart,
                    m_unitOfWork.ShoppingCartRepo.GetAll(u => u.ApplicationUserId == userId).Count());
                }              

                return View(HttpContext.Session.GetInt32(StaticData.SessionCart));
            }
            else
            {
                HttpContext.Session.Clear();

                return View(0);
            }
        }
    }
}
