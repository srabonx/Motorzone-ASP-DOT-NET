using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Multi.DataAccess.Repository.IUnitOfWorks;
using Multi.Models;
using Multi.Models.ViewModels;
using Multi.Utility;
using System.Diagnostics;

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


        #region API CALLS
        [HttpGet]
		public IActionResult GetAll(string status)
		{
			IEnumerable<OrderHeader> orderHeaders = m_unitOfWork.OrderHeaderRepo.GetAll(includeProp : "ApplicationUser").ToList();

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
