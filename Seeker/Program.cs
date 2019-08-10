using System;

namespace Seeker
{
    static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("╔═╗╔═╗╔═╗╦╔═╔═╗╦═╗");
            Console.WriteLine("╚═╗║╣ ║╣ ╠╩╗║╣ ╠╦╝");
            Console.WriteLine("╚═╝╚═╝╚═╝╩ ╩╚═╝╩╚═");

            Console.WriteLine("Started: " + DateTime.Now.ToString(Constants.TIMESTAMP_FORMAT));
            Manager.Run();
            Console.WriteLine("Finished: " + DateTime.Now.ToString(Constants.TIMESTAMP_FORMAT));

            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
        }
    }
}
