using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MStephenson.ServerForENTTECOpenDMX
{
  public class AsynchronousSocketListener
  {
    // Thread signal.
    public static ManualResetEvent fAllDone = new ManualResetEvent(false);

    public AsynchronousSocketListener() { }

    public static void StartListening()
    {
      // Create a TCP/IP socket.
      TcpListener listener = new TcpListener(IPAddress.Any, 41324);

      // Bind the socket to the local endpoint and listen for incoming connections.
      try
      {
        listener.Start();

        while (true)
        {
          // Set the event to nonsignaled state.
          fAllDone.Reset();

          // Start an asynchronous socket to listen for connections.
          Console.WriteLine("Waiting for a connection...");
          listener.BeginAcceptTcpClient(
              new AsyncCallback(AcceptCallback),
              listener);

          // Wait until a connection is made before continuing.
          fAllDone.WaitOne();
        }

      }
      catch (Exception e)
      {
        Console.WriteLine(e.ToString());
      }

      Console.WriteLine("\nPress ENTER to continue...");
      Console.Read();
    }

    public static void AcceptCallback(IAsyncResult ar)
    {
      // Signal the main thread to continue.
      fAllDone.Set();

      // Get the socket that handles the client request.
      TcpListener listener = (TcpListener)ar.AsyncState;
      TcpClient handler = listener.EndAcceptTcpClient(ar);
      NetworkStream stream = handler.GetStream();
      stream.ReadTimeout = 30000;
      Stopwatch timeoutTimer = new Stopwatch();

      byte[] buffer = new byte[513];

      while (true)
      {
        try
        {
          int receivedBytes = stream.Read(buffer, 0, 513);
          if (receivedBytes == 0)
          {
            if (!timeoutTimer.IsRunning)
            {
              timeoutTimer.Start();
            }
            if (timeoutTimer.ElapsedMilliseconds > 30000)
            {
              Console.WriteLine("Connection dropped, closing the connection");
              break;
            }
          }
          else if (timeoutTimer.IsRunning)
          {
            timeoutTimer.Reset();
          }
        }
        catch (IOException)
        {
          Console.WriteLine("Connection dropped, closing the connection");
          break;
        }

        for (int i = 0; i < 513; i++)
        {
          OpenDMX.SetDmxValue(i, buffer[i]);
        }
      }

      handler.Client.Shutdown(SocketShutdown.Both);
      handler.Close();
    }
  }
}
