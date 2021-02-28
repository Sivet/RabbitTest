

namespace RabbitMvcTest
{
    public class TourBooker{

        public static string BookTour(string name, string email, int value){
            RabbitSender.Send(name + ',' + email + ',' + value);
            
            return name + value.ToString();
        }
    }
}