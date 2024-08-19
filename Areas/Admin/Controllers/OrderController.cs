using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multi.DataAccess.Repository.IUnitOfWorks;
using Multi.Models;
using Multi.Utility;
using System.Diagnostics;

namespace MultiWeb.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = StaticData.Identity_Role_Admin)]
	public class OrderController : Controller
	{

		private readonly IUnitOfWork m_unitOfWork;

        public OrderController(IUnitOfWork unitOfWork)
        {
            m_unitOfWork = unitOfWork;
        }

        public IActionResult Index()
		{

			return View();
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
