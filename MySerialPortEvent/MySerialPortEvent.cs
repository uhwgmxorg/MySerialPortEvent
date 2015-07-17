/******************************************************************************/
/*                                                                            */
/*   Program: MySerialPortEvent                                               */
/*   Implementing an own event to read and write to a ComPort in .Net AND     */
/*   Mono                                                                     */
/*                                                                            */
/*   17.07.2015 0.0.0.0 uhwgmxorg Start                                       */
/*                                                                            */
/******************************************************************************/
using System;
using System.IO.Ports;
using System.Threading;

namespace MySerialPortEvent
{
    class MySerialPortEvent
    {

        public delegate void DataReceived(string data);
        public event DataReceived RaiseDataReceived;

        private SerialPort _serialPort;

        Thread _thread;

        /// <summary>
        /// Constructor
        /// </summary>
        public MySerialPortEvent()
        {
        }

        /// <summary>
        /// InitPort
        /// </summary>
        private void InitPort()
        {
            _serialPort = new SerialPort();
            if (_serialPort.IsOpen)
                _serialPort.Close();


#if MONO
            _serialPort = new SerialPort("/dev/ttyS0", 115200);
#else
            _serialPort = new SerialPort("COM4", 115200);
#endif
            _serialPort.Open();
            _serialPort.ReadTimeout = SerialPort.InfiniteTimeout;
        }

        /// <summary>
        /// WriteData
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public bool WriteData(string Data)
        {
            _serialPort.Write(Data);
            return true;
        }

        /// <summary>
        /// ReadData
        /// read data until Carriage-Return
        /// Line-Feed (13 10 0x0D 0x0A)
        /// </summary>
        /// <returns></returns>
        public void ReadData()
        {
            byte tmpByte, tmpByte_1 = 0;
            string rxString = "";

            tmpByte = (byte)_serialPort.ReadByte();

            while (true)
            {
                tmpByte_1 = tmpByte;
                rxString += ((char)tmpByte);
                tmpByte = (byte)_serialPort.ReadByte();
                if (tmpByte_1 == 13 && tmpByte == 10)
                    break;
            }

            RaiseDataReceived(rxString);
        }

        /// <summary>
        /// ComPortDataReceived
        /// THIS IS THE EVENT HANDLER FUNCTION
        /// </summary>
        /// <param name="data"></param>
        public void ComPortDataReceived(string data)
        {
            Console.WriteLine(data);
        }

        /// <summary>
        /// ClosePort
        /// </summary>
        public void ClosePort()
        {
            _serialPort.Close();
        }

        /// <summary>
        /// Main
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            MySerialPortEvent SerialPortEvent = new MySerialPortEvent();
            Console.WriteLine("Program MySerialPortEvent (enter quit to terminate the program)");

            SerialPortEvent.InitPort();

            // Set the event
            SerialPortEvent.RaiseDataReceived += SerialPortEvent.ComPortDataReceived;

            // Start the receiving thread
            SerialPortEvent._thread = new Thread(
            () =>
            {
                try
                {
                    Thread.CurrentThread.Name = "ComIOReadThread";
                    while (true)
                    {
                        SerialPortEvent.ReadData();
                        Thread.Sleep(0);
                    }
                }
                catch (Exception ex)
                {                    
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            });
            SerialPortEvent._thread.Start();

            string Input;
            while(true)
            {
                Input = Console.ReadLine();
                SerialPortEvent.WriteData(Input);
                if (Input == "quit")
                    break;
            }

            SerialPortEvent.ClosePort();

            Console.WriteLine("press any key to continue");
            SerialPortEvent._thread.Interrupt();
            SerialPortEvent._thread.Abort();
            Console.ReadKey();
        }
    }
}
