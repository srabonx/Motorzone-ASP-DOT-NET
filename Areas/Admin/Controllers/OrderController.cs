using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using Multi.DataAccess.Repository.IUnitOfWorks;
using Multi.Models;
using Multi.Models.ViewModels;
using Multi.Utility;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace MultiWeb.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize]
	public class OrderController : Controller
	{

		private readonly IUnitOfWork m_unitOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            m_unitOfWork = unitOfWork;
        }

        public IActionResult Index()
		{
			return View();
		}

        public IActionResult Details(uint orderId)
        {
            OrderVM = new OrderVM()
            {
                OrderHeader = m_unitOfWork.OrderHeaderRepo.Get(x => x.Id == orderId, includeProp: "ApplicationUser") ?? new OrderHeader(),
                OrderDetails = m_unitOfWork.OrderDetailRepo.GetAll(x => x.OrderHeaderId == orderId, includeProp: "ProductBike")
            };

            return View(OrderVM);
        }

        [HttpPost]
        [Authorize(Roles = StaticData.Identity_Role_Admin +","+StaticData.Identity_Role_Employee)]
        public IActionResult UpdateOrderDetails()
        {

            var order = OrderVM;

            var orderHeaderFromDb = m_unitOfWork.OrderHeaderRepo.Get(x => x.Id == OrderVM.OrderHeader.Id);

            if (orderHeaderFromDb != null)
            {
                orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
                orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
                orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
                orderHeaderFromDb.City = OrderVM.OrderHeader.City;
                orderHeaderFromDb.Division = OrderVM.OrderHeader.Division;
                orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;

                if(!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
                {
                    orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
                }

                if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
                {
                    orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
                }

                m_unitOfWork.OrderHeaderRepo.Update(orderHeaderFromDb);
                m_unitOfWork.SaveChanges();

                TempData["Success"] = "Order details updated successfully";

            }

            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb?.Id ?? 0 });
        }

		[HttpPost]
		[Authorize(Roles = StaticData.Identity_Role_Admin + "," + StaticData.Identity_Role_Employee)]
		public IActionResult StartProcessing()
		{
            m_unitOfWork.OrderHeaderRepo.UpdateStatus(OrderVM.OrderHeader.Id, StaticData.StatusInProcess);
            m_unitOfWork.SaveChanges();

			TempData["Success"] = "Order status updated successfully";

            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });

		}

		[HttpPost]
		[Authorize(Roles = StaticData.Identity_Role_Admin + "," + StaticData.Identity_Role_Employee)]
		public IActionResult ShipOrder()
		{

            var orderHeaderFromDb = m_unitOfWork.OrderHeaderRepo.Get(x => x.Id == OrderVM.OrderHeader.Id);

            orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeaderFromDb.OrderStatus = StaticData.StatusShipped;
            orderHeaderFromDb.ShippingDate = DateTime.Now;

            if(orderHeaderFromDb.PaymentStatus == StaticData.PaymentStatusDelayedPayment)
            {
                orderHeaderFromDb.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }

            m_unitOfWork.OrderHeaderRepo.Update(orderHeaderFromDb);
			m_unitOfWork.SaveChanges();

			TempData["Success"] = "Order shipped successfully";

			return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });

		}

        [HttpPost]
        [Authorize(Roles = StaticData.Identity_Role_Admin + "," + StaticData.Identity_Role_Employee)]
        public IActionResult CancelOrder()
        {
            var orderHeader = m_unitOfWork.OrderHeaderRepo.Get(x => x.Id == OrderVM.OrderHeader.Id);

            if(orderHeader.PaymentStatus == StaticData.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };

                var service = new RefundService();
                var refund = service.Create(options);

                m_unitOfWork.OrderHeaderRepo.UpdateStatus(orderHeader.Id, StaticData.StatusCancelled, StaticData.StatusRefunded);
            }
            else
            {
				m_unitOfWork.OrderHeaderRepo.UpdateStatus(orderHeader.Id, StaticData.StatusCancelled, StaticData.StatusCancelled);
			}

			m_unitOfWork.SaveChanges();

			TempData["Success"] = "Order cancelled successfully";

			return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });

		}

        [ActionName("Details")]
        [HttpPost]
        public IActionResult DetailsPayNow()
        {
            OrderVM.OrderHeader = m_unitOfWork.OrderHeaderRepo.Get(x => x.Id == OrderVM.OrderHeader.Id,
                includeProp: "ApplicationUser");

            OrderVM.OrderDetails = m_unitOfWork.OrderDetailRepo.GetAll(x => x.OrderHeader.Id == OrderVM.OrderHeader.Id,
                includeProp: "ProductBike");

			var options = new SessionCreateOptions
			{
				SuccessUrl = StaticData.DomainName + $"Admin/order/PaymentConfirmation?orderHeaderId={OrderVM.OrderHeader.Id}",
				CancelUrl = StaticData.DomainName + $"Admin/order/details?orderId={OrderVM.OrderHeader.Id}",
				LineItems = new List<SessionLineItemOptions>(),
				Mode = "payment",
			};

			foreach (var item in OrderVM.OrderDetails)
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

			m_unitOfWork.OrderHeaderRepo.UpdateStripePaymentId(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
			m_unitOfWork.SaveChanges();

			Response.Headers.Add("Location", session.Url);

			return new StatusCodeResult(303);
		}


		public IActionResult PaymentConfirmation(uint orderHeaderId)
		{
			OrderHeader? orderHeader = m_unitOfWork.OrderHeaderRepo.Get(u => u.Id == orderHeaderId);

			if (orderHeader != null)
			{
				if (orderHeader.PaymentStatus == StaticData.PaymentStatusDelayedPayment)
				{
					// this is an order by a company

					var service = new SessionService();
					Session session = service.Get(orderHeader.SessionId);

					if (session.PaymentStatus.ToLower() == "paid")
					{
						// payment went through
						m_unitOfWork.OrderHeaderRepo.UpdateStripePaymentId(orderHeaderId, session.Id, session.PaymentIntentId);
						m_unitOfWork.OrderHeaderRepo.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, StaticData.PaymentStatusApproved);
						m_unitOfWork.SaveChanges();
					}
				}

			}


			return View(orderHeaderId);
		}

		#region API CALLS
		[HttpGet]
		public IActionResult GetAll(string status)
		{
            IEnumerable<OrderHeader> orderHeaders;

            if(User.IsInRole(StaticData.Identity_Role_Admin) || User.IsInRole(StaticData.Identity_Role_Employee))
            {
                orderHeaders = m_unitOfWork.OrderHeaderRepo.GetAll(includeProp: "ApplicationUser").ToList();
            }
            else
            {
                var claims = (ClaimsIdentity?)User.Identity;
                var userId = claims?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                orderHeaders = m_unitOfWork.OrderHeaderRepo.GetAll(x => x.ApplicationUserId == userId,
                                                            includeProp: "ApplicationUser").ToList();
            }

            switch (status)
            {
                case "pending":
                    orderHeaders = orderHeaders.Where(u => u.PaymentStatus == StaticData.PaymentStatusDelayedPayment).ToList();
                    break;
                case "inprocess":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == StaticData.StatusInProcess).ToList();
                    break;
                case "completed":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == StaticData.StatusShipped).ToList();
                    break;
                case "approved":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == StaticData.StatusApproved).ToList();
                    break;
                default:
                    break;
            }

            return Json(new { data = orderHeaders });
		}


		#endregion

	}

}
