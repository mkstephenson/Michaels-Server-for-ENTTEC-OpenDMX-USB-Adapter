# Michael's Server for ENTTEC OpenDMX USB Adapter
This is a simple TCP based server to allow the control of an ENTTEC OpenDMX USB adapter.

This has been tested with Windows 10 (x64, 10586.318) with the drivers which are installed automatically by Windows 10 when the device is first plugged in.

You will need to grab the FTDI DLL (ftd2xx64.dll) from http://www.ftdichip.com/Drivers/D2XX.htm and drop it into the lib folder. If you are using a 32 bit system, you will need to change the relevant DllImport references in the OpenDMX class.

The OpenDMX class is a lightly modified version of the example found on the ENTTEC site (https://www.enttec.com/?main_menu=Products&pn=70303).
