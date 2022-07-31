using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ConsoleApp2019
{
    class Mediator
    {

        Dictionary<string, IEnqueue> subsDic = new Dictionary<string, IEnqueue>();

        internal void AddSubscriber(IEnqueue subscriber, string subscriberName)
        {
            subsDic.Add(subscriberName, subscriber);
        }

        internal void Enqueue(EventArgs mm, string dest)
        {
            IEnqueue d = subsDic[dest];
            d.Enqueue(mm, dest);
        }
    }

    interface IEnqueue
    {
        void Enqueue(EventArgs mm, string dest);
    }
    class Client : IEnqueue
    {
        //void SendMessage
        private Mediator med;

        public Client(Mediator med)
        {
            this.med = med;
        }

        public void Run()
        {
            int n = 0;
            while (true)
            {
                MediatorArgs mm = new MediatorArgs();
                mm.Index = n++;
                med.Enqueue(mm, "CO2");

                System.Threading.Thread.Sleep(1000);
            }
        }

        void IEnqueue.Enqueue(EventArgs mm, string dest)
        {
            MediatorArgs ma = (MediatorArgs)mm;
            Console.WriteLine($"To {dest} , {ma.Index}");
        }
    }

    class MediatorArgs: EventArgs
    {
        int index = 0;
        public int Index { get => index; set => index = value; }
    }

}
