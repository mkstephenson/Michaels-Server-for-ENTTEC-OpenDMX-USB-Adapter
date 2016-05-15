using System;
using System.Threading;

namespace MStephenson.ServerForENTTECOpenDMX
{
  public class Program
  {
    private static byte[] fBuffer = new byte[513];
    private static Timer fTimer;

    static void Main(string[] args)
    {
      fTimer = new Timer((state) =>
      {
        OpenDMX.WriteData();
        fTimer.Change(5, Timeout.Infinite);
      }, null, 5, Timeout.Infinite);

      OpenDMX.Start();

      if (OpenDMX.fStatus == FT_STATUS.FT_DEVICE_NOT_FOUND)
      {
        Console.WriteLine("No Enttec USB Device Found");
      }
      else if (OpenDMX.fStatus == FT_STATUS.FT_OK)
      {
        Console.WriteLine("Found DMX on USB");
      }
      else
      {
        Console.WriteLine("Error Opening Device");
      }

      AsynchronousSocketListener.StartListening();
    }
  }
}
