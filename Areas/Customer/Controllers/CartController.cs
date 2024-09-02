using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multi.DataAccess.Repository.IUnitOfWorks;
using Multi.Models;
using Multi.Models.ViewModels;
using Multi.Utility;
using Stripe;
using Stripe.Checkout;
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
                HttpContext.Session.SetInt32(StaticData.SessionCart,
                m_unitOfWork.ShoppingCartRepo.GetAll(u => u.ApplicationUserId == item.ApplicationUserId).Count() - 1);

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

			HttpContext.Session.SetInt32(StaticData.SessionCart,
				m_unitOfWork.ShoppingCartRepo.GetAll(u => u.ApplicationUserId == item.ApplicationUserId).Count() - 1);

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
					var options = new SessionCreateOptions
					{
						SuccessUrl = StaticData.DomainName + $"Customer/Cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
						CancelUrl = StaticData.DomainName + "Customer/Cart/Index",
						LineItems = new List<SessionLineItemOptions>(),
						Mode = "payment",
					};

					foreach(var item in ShoppingCartVM.ShoppingCartList)
					{
						var sessionLineItem = new SessionLineItemOptions
						{
							PriceData = new SessionLineItemPriceDataOptions
							{
								UnitAmount = item.ProductBike.Price * 100 ?? 0,
								Currency = "usd",
								ProductData = new SessionLineItemPriceDataProductDataOptions
								{
									Name = item.ProductBike.Name
								},
							},
							
							Quantity = item.Count,
						};

						options.LineItems.Add(sessionLineItem);
					}

					var service = new SessionService();
					Session session = service.Create(options);

					m_unitOfWork.OrderHeaderRepo.UpdateStripePaymentId(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
					m_unitOfWork.SaveChanges();

					Response.Headers.Add("Location", session.Url);
					return new StatusCodeResult(303);

				}

				return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
			}

			return NotFound();
		}

		public IActionResult OrderConfirmation(uint id)
		{
			OrderHeader? orderHeader = m_unitOfWork.OrderHeaderRepo.Get(u => u.Id == id, includeProp: "ApplicationUser");

			if(orderHeader != null)
			{
				if(orderHeader.PaymentStatus != StaticData.PaymentStatusDelayedPayment)
				{
					// this is an order by customer
					var service = new SessionService();
					Session session = service.Get(orderHeader.SessionId);

					if(session.PaymentStatus.ToLower() == "paid")
					{
						// payment went through
						m_unitOfWork.OrderHeaderRepo.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
						m_unitOfWork.OrderHeaderRepo.UpdateStatus(id, StaticData.StatusApproved, StaticData.PaymentStatusApproved);
						m_unitOfWork.SaveChanges();
					}
				}

				// Remove the shopping cart
				List<ShoppingCart> shoppingCarts = m_unitOfWork.ShoppingCartRepo.
					GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();

				m_unitOfWork.ShoppingCartRepo.RangeRemove(shoppingCarts);
				m_unitOfWork.SaveChanges(); 

			}


			return View(id);
		}

	}
}
