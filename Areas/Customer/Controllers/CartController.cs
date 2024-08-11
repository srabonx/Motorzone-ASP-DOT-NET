using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multi.DataAccess.Repository.IUnitOfWorks;
using Multi.Models;
using System.Security.Claims;

namespace MultiWeb.Areas.Customer.Controllers
{

	[Authorize]
	[Area("Customer")]
	public class CartController : Controller
	{
		private readonly IUnitOfWork m_unitOfWork;
		public CartController( IUnitOfWork unitOfWork)
		{
			m_unitOfWork = unitOfWork;
		}

		public IActionResult Index()
		{
			var claimsIdentity = (ClaimsIdentity?)User.Identity;
			var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;


			List<ShoppingCart> items = m_unitOfWork.ShoppingCartRepo.GetAll(u => u.ApplicationUserId == userId, includeProp: "ProductBike").ToList();

			return View(items);
		}
	}
}
