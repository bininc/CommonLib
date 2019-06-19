using CommLiby.SocketLib;
using AddressFamily = CommLiby.SocketLib.AddressFamily;
using ProtocolType = CommLiby.SocketLib.ProtocolType;
using SocketType = CommLiby.SocketLib.SocketType;

namespace CommonLib.SocketLib
{
    public class TCPClient : ITCPClient
    {
        public override ISocketAsyncEventArgs GetISocketAsyncEventArgs()
        {
            return new WinSocketAsyncEventArgs();
        }

        protected override ISocket GetISocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            return new WinSocket(addressFamily, socketType, protocolType);
        }
    }
}
