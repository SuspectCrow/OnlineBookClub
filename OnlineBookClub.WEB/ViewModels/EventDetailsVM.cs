using OnlineBookClub.WEB.Models.DB.Event;

namespace OnlineBookClub.WEB.ViewModels
{
    public class EventDetailsVM
    {
        public Event Event {  get; set; }
        public  string BookName { get; set; }
        public string BookDescription { get; set; }
        public string BookImage { get; set; }


    }
}
