using System;

namespace Kinode.Seeker
{
    static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(" K  I  N  O  D  E ");
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
