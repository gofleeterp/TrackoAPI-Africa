using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoApi.Core
{
    interface IMessageQueue
    {
        void Setup();
        void Teardown();
        void Send(byte[] message);
        void Receive();
    }
}
