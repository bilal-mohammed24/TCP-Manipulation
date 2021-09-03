using System;
using System.Text;
using PacketDotNet;
using System.IO;




namespace TCPclient
{
    class Program
    {
        static ushort sport = 5693;
        static ushort dport = 8888;
        static string sip = "";
        static string dip = "";
        static string interf = "";
        

        static void Main(string[] args)
        {
            
            for (int i = 0; i < SharpPcap.LibPcap.LibPcapLiveDeviceList.Instance.Count; i++) {
                Console.WriteLine(i+": "+SharpPcap.LibPcap.LibPcapLiveDeviceList.Instance[i].Interface.FriendlyName);
            
            }

      
           Console.WriteLine("Select an Interface: ");
           interf = Console.ReadLine();
           Console.WriteLine("Selected Interface: "+Int16.Parse(interf));

           Console.WriteLine("Enter Source ip");
           sip=Console.ReadLine();

           Console.WriteLine("Enter Destination ip");
           dip = Console.ReadLine();




           var TCP = new TcpPacket(sport,dport );
           TCP.Syn = true;


           var ipSourceAddress = System.Net.IPAddress.Parse(sip);
           var ipDestinationAddress = System.Net.IPAddress.Parse(dip);
           var IP = new IPv4Packet(ipSourceAddress, ipDestinationAddress);


           var ethernetSourceHwAddress = System.Net.NetworkInformation.PhysicalAddress.Parse("01-00-02-00-00-00");
           var ethernetDestinationHwAddress = System.Net.NetworkInformation.PhysicalAddress.Parse("01-00-02-00-00-00"); 
           var ETHERNET = new EthernetPacket(ethernetSourceHwAddress, ethernetDestinationHwAddress, EthernetPacketType.IpV4);
           

           
           IP.PayloadPacket = TCP;
           ETHERNET.PayloadPacket = IP;

           TCP.UpdateTCPChecksum();
           IP.UpdateIPChecksum();
     
           
           Console.WriteLine("packet length:"+ETHERNET.Bytes.Length);

           var device=SharpPcap.LibPcap.LibPcapLiveDeviceList.Instance[Int16.Parse(interf)];
           device.Open(SharpPcap.DeviceMode.Promiscuous);




           for (; ; )
            {
                Console.WriteLine(" 1: for Sending message\n 2: for Sending file");
                int d = Int16.Parse(Console.ReadLine());
                string data = "";
                if (d == 1)
                {

                    Console.WriteLine("Enter the message");
                    data = Console.ReadLine();

                }
                else if (d == 2)
                {
                    Console.WriteLine("Enter file location");
                    data = File.ReadAllText(Console.ReadLine());
                }



                byte[] arr = Encoding.Unicode.GetBytes(data);
                if (arr.Length % 4 != 0)
                {
                    byte[] temp = new byte[arr.Length + 2];
                    for (int i = 0; i < arr.Length; i++)
                    {
                        temp[i] = arr[i];
                    }
                    arr = temp;
                }
                Console.WriteLine(arr.Length);
                uint[] val = new uint[arr.Length / 3];
                int vi = 0;
                ushort c = 0;
                for (int i = 0; i < arr.Length; i += 4)
                {

                    val[vi] = (uint)((arr[i] << 24) | arr[i + 1] << 16 | arr[i + 2] << 8 | arr[i + 3]);
                    Console.WriteLine("value : " + val[vi]);

                    TCP.SequenceNumber = val[vi];
                    TCP.WindowSize = c;


                   


                    device.SendPacket(ETHERNET);
                  
                    Console.WriteLine("sent");

                    if (c == 65530)
                        c = 0;
                    c++;

                }


            }





           device.Close();
           Console.WriteLine("finished");
           Console.ReadLine();
           



        }



    }
   
}
