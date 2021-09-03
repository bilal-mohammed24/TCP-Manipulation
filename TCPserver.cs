using System;
using System.Text;
using PacketDotNet;

namespace TCPserver
{
    class Program
    {

        static string interf="";
        static uint[] data = new uint[1024];
        static ushort[] winsa = new ushort[1024];
        static int c = 0;




        static void Main(string[] args)
        {

            for (int i = 0; i < SharpPcap.LibPcap.LibPcapLiveDeviceList.Instance.Count; i++)
            {
                Console.WriteLine(i + ": " + SharpPcap.LibPcap.LibPcapLiveDeviceList.Instance[i].Interface.FriendlyName);

            }
   
            Console.WriteLine("Select an Interface: ");
            interf = Console.ReadLine();


            var device = SharpPcap.LibPcap.LibPcapLiveDeviceList.Instance[Int16.Parse(interf)];

            device.Open(SharpPcap.DeviceMode.Promiscuous);


            device.OnPacketArrival += device_OnPacketArrival;


            device.OnCaptureStopped += device_OnCaptureStopped;

            device.StartCapture();

            Console.WriteLine("Start capturing... Enter to stop");
            Console.ReadLine();
            device.Close();

        }  



      

        private static void device_OnPacketArrival(object sender,SharpPcap.CaptureEventArgs e)
        {
         
            SharpPcap.RawCapture rc=e.Packet;
            var packet = PacketDotNet.Packet.ParsePacket(rc.LinkLayerType, rc.Data);
            TcpPacket tcp = TcpPacket.GetEncapsulated(packet);
 
            try
            {
                
               if (tcp.DestinationPort.ToString().Equals("8888")) {

                     byte[] converter = BitConverter.GetBytes(tcp.SequenceNumber);

                     ushort id = tcp.WindowSize;

                         for (int i = 0; i < converter.Length / 2; i++)
                         {
                             byte temp = converter[i];
                             
                             converter[i] = converter[converter.Length - i - 1];
                             converter[converter.Length - i - 1] = temp;

                         }

                         Console.Write(Encoding.Unicode.GetString(converter));

                          
                         c++;
                }
            }catch(Exception){}
  
        }







        static void device_OnCaptureStopped(Object s,SharpPcap.CaptureStoppedEventStatus e) {
            Console.WriteLine("Stopped");
        
        }


        }
    }

