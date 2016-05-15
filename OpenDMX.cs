using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace MStephenson.ServerForENTTECOpenDMX
{
  public class OpenDMX
  {
    public static byte[] fBuffer = new byte[513];
    public static uint fHandle;
    public static bool fDone = false;
    public static bool fConnected = false;
    public static int fBytesWritten = 0;
    public static FT_STATUS fStatus;

    public const byte BITS_8 = 8;
    public const byte STOP_BITS_2 = 2;
    public const byte PARITY_NONE = 0;
    public const ushort FLOW_NONE = 0;
    public const byte PURGE_RX = 1;
    public const byte PURGE_TX = 2;


    [DllImport("lib/ftd2xx64.dll")]
    public static extern FT_STATUS FT_Open(UInt32 uiPort, ref uint ftHandle);
    [DllImport("lib/ftd2xx64.dll")]
    public static extern FT_STATUS FT_Write(uint ftHandle, IntPtr lpBuffer, UInt32 dwBytesToRead, ref UInt32 lpdwBytesWritten);
    [DllImport("lib/ftd2xx64.dll")]
    public static extern FT_STATUS FT_SetDataCharacteristics(uint ftHandle, byte uWordLength, byte uStopBits, byte uParity);
    [DllImport("lib/ftd2xx64.dll")]
    public static extern FT_STATUS FT_SetFlowControl(uint ftHandle, char usFlowControl, byte uXon, byte uXoff);
    [DllImport("lib/ftd2xx64.dll")]
    public static extern FT_STATUS FT_Purge(uint ftHandle, UInt32 dwMask);
    [DllImport("lib/ftd2xx64.dll")]
    public static extern FT_STATUS FT_ClrRts(uint ftHandle);
    [DllImport("lib/ftd2xx64.dll")]
    public static extern FT_STATUS FT_SetBreakOn(uint ftHandle);
    [DllImport("lib/ftd2xx64.dll")]
    public static extern FT_STATUS FT_SetBreakOff(uint ftHandle);
    [DllImport("lib/ftd2xx64.dll")]
    public static extern FT_STATUS FT_ResetDevice(uint ftHandle);
    [DllImport("lib/ftd2xx64.dll")]
    public static extern FT_STATUS FT_SetDivisor(uint ftHandle, char usDivisor);


    public static void Start()
    {
      fHandle = 0;
      fStatus = FT_Open(0, ref fHandle);
      //setting up the WriteData method to be on it's own thread. This will also turn all channels off
      //this unrequested change of state can be managed by getting the current state of all channels
      //into the write buffer before calling this function.
      Thread thread = new Thread(new ThreadStart(WriteData));
      thread.Start();
    }

    public static void SetDmxValue(int channel, byte value)
    {
      if (fBuffer != null)
      {
        fBuffer[channel] = value;
      }
    }

    public static void WriteData()
    {
      try
      {
        InitOpenDMX();
        if (fStatus == FT_STATUS.FT_OK)
        {
          fStatus = FT_SetBreakOn(fHandle);
          fStatus = FT_SetBreakOff(fHandle);
          fBytesWritten = Write(fHandle, fBuffer, fBuffer.Length);
        }
      }
      catch (Exception exp)
      {
        Console.WriteLine(exp);
      }
    }

    public static int Write(uint handle, byte[] data, int length)
    {
      try
      {
        IntPtr ptr = Marshal.AllocHGlobal((int)length);
        Marshal.Copy(data, 0, ptr, (int)length);
        uint bytesWritten = 0;
        fStatus = FT_Write(handle, ptr, (uint)length, ref bytesWritten);
        return (int)bytesWritten;
      }
      catch (Exception exp)
      {
        Console.WriteLine(exp);
        return 0;
      }
    }

    public static void InitOpenDMX()
    {
      fStatus = FT_ResetDevice(fHandle);
      fStatus = FT_SetDivisor(fHandle, (char)12);  // set baud rate
      fStatus = FT_SetDataCharacteristics(fHandle, BITS_8, STOP_BITS_2, PARITY_NONE);
      fStatus = FT_SetFlowControl(fHandle, (char)FLOW_NONE, 0, 0);
      fStatus = FT_ClrRts(fHandle);
      fStatus = FT_Purge(fHandle, PURGE_TX);
      fStatus = FT_Purge(fHandle, PURGE_RX);
    }

  }

  /// <summary>
  /// Enumaration containing the varios return status for the DLL functions.
  /// </summary>
  public enum FT_STATUS
  {
    FT_OK = 0,
    FT_INVALID_HANDLE,
    FT_DEVICE_NOT_FOUND,
    FT_DEVICE_NOT_OPENED,
    FT_IO_ERROR,
    FT_INSUFFICIENT_RESOURCES,
    FT_INVALID_PARAMETER,
    FT_INVALID_BAUD_RATE,
    FT_DEVICE_NOT_OPENED_FOR_ERASE,
    FT_DEVICE_NOT_OPENED_FOR_WRITE,
    FT_FAILED_TO_WRITE_DEVICE,
    FT_EEPROM_READ_FAILED,
    FT_EEPROM_WRITE_FAILED,
    FT_EEPROM_ERASE_FAILED,
    FT_EEPROM_NOT_PRESENT,
    FT_EEPROM_NOT_PROGRAMMED,
    FT_INVALID_ARGS,
    FT_OTHER_ERROR
  };
}
