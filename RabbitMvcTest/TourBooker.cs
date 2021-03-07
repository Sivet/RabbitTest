

namespace RabbitMvcTest
{
    public class TourBooker{

        public static void BookTour(string name, string email, int book, string tour){
            RabbitSender.Send(name + ',' + email + ',' + book + ',' + tour);
            
            //return name + value.ToString(); //just for testing blazor
        }
    }
}