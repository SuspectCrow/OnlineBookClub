using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBookClub.WEB.Models;
using OnlineBookClub.WEB.Models.DB.Const;
using OnlineBookClub.WEB.Models.DB.Event;
using OnlineBookClub.WEB.Models.Identity;
using OnlineBookClub.WEB.ViewModels;
using OpenLibraryNET;
using OpenLibraryNET.Data;
using OpenLibraryNET.Utility;

namespace OnlineBookClub.WEB.Controllers
{
    public class EventController : Controller
    {
        private OnlineBookClubContext _context;
        private readonly UserManager<AppUser> _userManager;

        public EventController(OnlineBookClubContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            Event_IndexVM model = new Event_IndexVM()
            {
                Events = _context.Events.Include(x => x.EventSubjects).Include(x => x.Location).ToList(),
            };

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            EventDetailsVM model = new EventDetailsVM()
            {
                Event = _context.Events.Where(x => x.Id == id).Include(x => x.EventSubjects).Include(x => x.Location).Include(x => x.EventRatings).Include(x => x.School).Include(x => x.CREATED_USER_).FirstOrDefault()!,
            };

            OpenLibraryClient olClinet = new OpenLibraryClient();

            try
            {
                OLEdition oLEdition = await olClinet.GetEditionAsync(model.Event.ISBN, OpenLibraryNET.Utility.BookIdType.ISBN);
                model.BookName = oLEdition.Data.Title;
                model.BookDescription = oLEdition.Data.Description;
                byte[] coverSmall = await olClinet.Image.GetCoverAsync(CoverIdType.ISBN, model.Event.ISBN, ImageSize.Large);

                model.BookImage = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(coverSmall));

            }
            catch (Exception)
            {
                return View("Error");
                throw;
            }

            //switch (eventModel.ISBN)
            //{
            //    case "1ASDF":
            //        ViewBag.BookName = "Nutuk";
            //        break;

            //    case "DSFDSF":
            //        ViewBag.BookName = "Ateşten Gömlek";
            //        break;

            //    default:
            //        ViewBag.BookName = eventModel.ISBN;
            //        break;
            //}

            return View(model);
        }

        public async Task<IActionResult> JoinEvent(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction(nameof(HomeController.SignUp));
            }

            var currentUser = await _userManager.FindByNameAsync(User.Identity!.Name);

            if (_context.EventRatings.FirstOrDefault(x => x.UserId == currentUser.Id) == null)
            {
                return RedirectToAction(nameof(EventController.Details), new { id = id });
            }

            _context.EventRatings.Add(new EventRating()
            {
                CREATED_DATE = DateTime.UtcNow,
                EventId = id,
                UserId = currentUser.Id,
                Rating = 1
            }) ;

            _context.SaveChanges();

            return RedirectToAction(nameof(EventController.Details), new { id = id });
        }

         public IActionResult Guides()
        {
            return View();
        }
    }
}
