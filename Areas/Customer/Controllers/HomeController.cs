using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Multi.Models;
using Multi.DataAccess.Repository.IUnitOfWorks;
using Microsoft.AspNetCore.Authorization;
using Multi.Utility;
using System.Security.Claims;

namespace MultiWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork m_unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            m_unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var products = m_unitOfWork.ProductBikeRepo.GetAll(includeProp: "Category");

            return View(products);
        }

        public IActionResult Details(uint id)
        {
            var cart = new ShoppingCart()
            {
                ProductBike = m_unitOfWork.ProductBikeRepo.Get(u => u.Id == id, includeProp: "Category"),
                ProductId = id,
                Count = 1
            };


            return View(cart);
        }

        [HttpPost]
        [Authorize]
		public IActionResult Details(ShoppingCart cart)
		{
            var claimsIdentity = (ClaimsIdentity?)User.Identity;

            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            cart.ApplicationUserId = userId;
            cart.Id = 0;

            var cartFromDb = m_unitOfWork.ShoppingCartRepo.Get(u => u.ApplicationUserId == cart.ApplicationUserId &&
                                                                u.ProductId == cart.ProductId);

            if(cartFromDb != null)
            {
                // Cart already exist, UPDATE
                cartFromDb.Count += cart.Count;
                m_unitOfWork.ShoppingCartRepo.Update(cartFromDb);

				TempData["Success"] = "Cart updated successfully!";
			}
			else
            {
				// Add to DB record
				m_unitOfWork.ShoppingCartRepo.Add(cart);

				TempData["Success"] = "Product added to cart successfully!";
			}


			m_unitOfWork.SaveChanges();

            return RedirectToAction(nameof(Index));
		}

		public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
