using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multi.DataAccess.Repository.IUnitOfWorks;
using Multi.Models;
using Multi.Models.ViewModels;
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

			var shoppingCartList = m_unitOfWork.ShoppingCartRepo.GetAll(u => u.ApplicationUserId == userId, includeProp: "ProductBike");

			uint? totalPrice = 0;

			foreach(var item in shoppingCartList)
			{
				totalPrice += item.ProductBike.Price * item.Count;
			}

			ShoppingCartVM shoppingCartVM = new ShoppingCartVM()
			{
				ShoppingCartList = shoppingCartList,
				TotalPrice = totalPrice ?? 0
			};


			return View(shoppingCartVM);
		}

		public IActionResult Plus(uint cartId)
		{
			var item = m_unitOfWork.ShoppingCartRepo.Get(cartId);

			if (item == null)
			{
				TempData["Failure"] = "Failed to update cart information!";
				return RedirectToAction(nameof(Index));
			}

			item.Count++;

			m_unitOfWork.ShoppingCartRepo.Update(item);
			m_unitOfWork.SaveChanges();

			TempData["success"] = "Cart information updated successfully!";

			return RedirectToAction(nameof(Index));
		}

		public IActionResult Minus(uint cartId)
		{
			var item = m_unitOfWork.ShoppingCartRepo.Get(cartId);

			if (item == null)
			{
				TempData["Failure"] = "Failed to update cart information!";
				return RedirectToAction(nameof(Index));
			}


			item.Count--;

			if (item.Count <= 0)
			{
				m_unitOfWork.ShoppingCartRepo.Remove(item);
			}
			else
			{
				m_unitOfWork.ShoppingCartRepo.Update(item);

			}

			
			m_unitOfWork.SaveChanges();

			TempData["success"] = "Cart information updated successfully!";

			return RedirectToAction(nameof(Index));
		}

		public IActionResult Remove(uint cartId)
		{
			var item = m_unitOfWork.ShoppingCartRepo.Get(cartId);

			if (item == null)
			{
				TempData["Failure"] = "Failed to update cart information!";
				return RedirectToAction(nameof(Index));
			}

			m_unitOfWork.ShoppingCartRepo.Remove(item);
			m_unitOfWork.SaveChanges();

			TempData["success"] = "Cart information updated successfully!";

			return RedirectToAction(nameof(Index));
		}

		public IActionResult Summary(ShoppingCartVM shoppingCartVm)
		{

			return View();
		}

	}
}
