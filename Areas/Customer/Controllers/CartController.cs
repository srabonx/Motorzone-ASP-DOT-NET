﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multi.DataAccess.Repository.IUnitOfWorks;
using Multi.Models;
using Multi.Models.ViewModels;
using Multi.Utility;
using System.Security.Claims;

namespace MultiWeb.Areas.Customer.Controllers
{

	[Authorize]
	[Area("Customer")]
	public class CartController : Controller
	{
		private readonly IUnitOfWork m_unitOfWork;

		[BindProperty]
		public ShoppingCartVM? ShoppingCartVM { get; set; }

		public CartController( IUnitOfWork unitOfWork)
		{
			m_unitOfWork = unitOfWork;
		}

		public IActionResult Index()
		{
			var claimsIdentity = (ClaimsIdentity?)User.Identity;
			var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			var shoppingCartList = m_unitOfWork.ShoppingCartRepo.GetAll(u => u.ApplicationUserId == userId, includeProp: "ProductBike");

			double? totalPrice = 0;

			foreach(var item in shoppingCartList)
			{
				totalPrice += item.ProductBike.Price * item.Count;
			}

			ShoppingCartVM = new ShoppingCartVM()
			{
				ShoppingCartList = shoppingCartList,
				OrderHeader = new() { OrderTotal = totalPrice ?? 0.0 }
			};


			return View(ShoppingCartVM);
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

		public IActionResult Summary()
		{

			var claimsIdentity = (ClaimsIdentity?)User.Identity;
			var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			var shoppingCartList = m_unitOfWork.ShoppingCartRepo.GetAll(u => u.ApplicationUserId == userId, includeProp: "ProductBike");

			double? totalPrice = 0;

			foreach (var item in shoppingCartList)
			{
				totalPrice += item.ProductBike.Price * item.Count;
			}

			ShoppingCartVM = new ShoppingCartVM()
			{
				ShoppingCartList = shoppingCartList,
				OrderHeader = new() { OrderTotal = totalPrice ?? 0.0 }
			};

			ShoppingCartVM.OrderHeader.ApplicationUser = m_unitOfWork.ApplicationUserRepo.Get(u => u.Id == userId) ?? new ApplicationUser();

			ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
			ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
			ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
			ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
			ShoppingCartVM.OrderHeader.Division = ShoppingCartVM.OrderHeader.ApplicationUser.Division;
			ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;



			return View(ShoppingCartVM);
		}

		[HttpPost]
		[ActionName("Summary")]
		public IActionResult SummaryPOST(ShoppingCartVM shoppingCartVm)
		{

			var claimsIdentity = (ClaimsIdentity?)User.Identity;
			var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			var shoppingCartList = m_unitOfWork.ShoppingCartRepo.GetAll(u => u.ApplicationUserId == userId, includeProp: "ProductBike");

			double? totalPrice = 0;

			foreach (var item in shoppingCartList)
			{
				totalPrice += item.ProductBike.Price * item.Count;
			}

			if(ShoppingCartVM != null)
			{
				ShoppingCartVM.ShoppingCartList = shoppingCartList;
				ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
				ShoppingCartVM.OrderHeader.ApplicationUserId = userId ?? "";
				
				ApplicationUser ApplicationUser = m_unitOfWork.ApplicationUserRepo.Get(u => u.Id == userId) ?? new ApplicationUser();
				ShoppingCartVM.OrderHeader.OrderTotal = totalPrice ?? 0.0;



				if (ApplicationUser.CompanyId.GetValueOrDefault() == 0)
				{
					// Regular user
					ShoppingCartVM.OrderHeader.PaymentStatus = StaticData.PaymentStatusPending;
					ShoppingCartVM.OrderHeader.OrderStatus = StaticData.StatusPending; 
				}
				else
				{
					// Company user
					ShoppingCartVM.OrderHeader.PaymentStatus = StaticData.PaymentStatusDelayedPayment;
					ShoppingCartVM.OrderHeader.OrderStatus = StaticData.StatusApproved;
				}

				m_unitOfWork.OrderHeaderRepo.Add(ShoppingCartVM.OrderHeader);
				m_unitOfWork.SaveChanges();

				// Create order detail

				foreach(var cart in ShoppingCartVM.ShoppingCartList)
				{
					OrderDetail orderDetail = new OrderDetail()
					{
						ProductId = cart.ProductId,
						OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
						Price = cart.ProductBike.Price ?? 0 * cart.Count,
						Count = cart.Count,
					};

					m_unitOfWork.OrderDetailRepo.Add(orderDetail);
					m_unitOfWork.SaveChanges();
				}


				if (ApplicationUser.CompanyId.GetValueOrDefault() == 0)
				{
					// Regular user, we need to capture payment
				}

				return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
			}

			return NotFound();
		}

		public IActionResult OrderConfirmation(uint id)
		{
			return View(id);
		}

	}
}
