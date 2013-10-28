using System;

using System.Collections.Generic;
using System.Text;

using System.Runtime.InteropServices;

namespace btComPrint
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct PORTEMUPortParams
    {
        internal int channel;
        [MarshalAs(UnmanagedType.Bool)]
        internal bool flocal;
        internal long device;//10
        internal int imtu;
        internal int iminmtu;
        internal int imaxmtu;
        internal int isendquota;
        internal int irecvquota;
        internal Guid uuidService;//16
        internal RFCOMM_PORT_FLAGS uiportflags;
    }

    [Flags()]
    internal enum RFCOMM_PORT_FLAGS : int
    {
        REMOTE_DCB = 0x00000001,
        KEEP_DCD = 0x00000002,
        AUTHENTICATE = 0x00000004,
        ENCRYPT = 0x00000008,
    }

    /// <summary>
    /// Represents a virtual COM port.
    /// </summary>
    /// <remarks>Deprecated. Use <see cref="M:InTheHand.Net.Sockets.BluetoothDeviceInfo.SetServiceState(System.Guid,System.Boolean)"/> 
    /// to enable a virtual COM port.
    /// <para>Supported on Windows CE Only.
    /// </para>
    /// </remarks>
    public class BluetoothSerialPort : IDisposable
    {
        private const int IOCTL_BLUETOOTH_GET_RFCOMM_CHANNEL = 0x1b0060;
        private const int IOCTL_BLUETOOTH_GET_PEER_DEVICE = 0x1b0064;

        private string portPrefix;
        private int portIndex;
        private PORTEMUPortParams pep;
        private IntPtr handle;

        internal BluetoothSerialPort(string portPrefix, int portIndex)
        {
            pep = new PORTEMUPortParams();
            pep.uiportflags = RFCOMM_PORT_FLAGS.REMOTE_DCB;

            this.portPrefix = portPrefix;
            this.portIndex = portIndex;
        }

        private void Register()
        {
            GC.KeepAlive(this);

            handle = RegisterDevice(portPrefix, portIndex, "btd.dll", ref pep);
            System.Diagnostics.Debug.Write("RegisterDevice: ");
            System.Diagnostics.Debug.WriteLine("Handle: 0x"+handle.ToInt32().ToString("x"));
            if (handle == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                System.Diagnostics.Debug.Write("GetLastWin32Error: ");
                System.Diagnostics.Debug.WriteLine(error);
                throw new Win32Exception(error, "Error creating virtual com port");
            }
        }
        public class Win32Exception : Exception
        {
            public Win32Exception(int iError, string sMsg)
            {
                throw new Exception("Win32Exception: " + iError.ToString()+ ", " + sMsg);
            }
        }
        public class PortStatusChangedEventArgs : EventArgs
        {
            public PortStatusChangedEventArgs(bool connected, string portName, BluetoothAddress address)
            {
                Connected = connected;
                PortName = portName;
                Address = address;
            }

            public bool Connected { get; private set; }
            public string PortName { get; private set; }
            public BluetoothAddress Address { get; private set; }
        }

        /// <summary>
        /// Create a virtual server port to listen for incoming connections.
        /// </summary>
        /// <param name="portName">Port name e.g. "COM4"</param>
        /// <param name="service">Bluetooth service to listen on.</param>
        /// <returns></returns>
        public static BluetoothSerialPort CreateServer(string portName, Guid service)
        {
            string portPrefix;
            int portIndex;
            SplitPortName(portName, out portPrefix, out portIndex);

            BluetoothSerialPort bsp = new BluetoothSerialPort(portPrefix, portIndex);
            bsp.pep.flocal = true;
            bsp.pep.uuidService = service;
            bsp.Register();
            return bsp;
        }

        /// <summary>
        /// Create a virtual server port to listen for incoming connections. Auto allocates a port from the COM0-9 range.
        /// </summary>
        /// <param name="service">Service GUID to listen on.</param>
        /// <returns></returns>
        public static BluetoothSerialPort CreateServer(Guid service)
        {
            BluetoothSerialPort bsp = new BluetoothSerialPort("COM", 9);
            bsp.pep.flocal = true;
            bsp.pep.uuidService = service;
            for (int iPort = 8; iPort > -1; iPort--)
            {
                try
                {
                    bsp.Register();
                    break;
                }
                catch
                {
                    bsp.portIndex = iPort;
                }
            }
            if (bsp.portIndex == 0)
            {
                throw new SystemException("Unable to create a Serial Port");
            }
            return bsp;
        }
        /// <summary>
        /// Create a client port for connection to a remote device.
        /// </summary>
        /// <param name="portName">Port name e.g. "COM4"</param>
        /// <param name="endPoint">Remote device to connect to</param>
        /// <returns>A BluetoothSerialPort.</returns>
        public static BluetoothSerialPort CreateClient(string portPrefix, int portIndex, BluetoothEndPoint endPoint)
        {
            BluetoothSerialPort bsp = new BluetoothSerialPort(portPrefix, portIndex);
            bsp.pep.flocal = false;
            bsp.pep.device = endPoint.Address.ToInt64();
            bsp.pep.uuidService = endPoint.Service;

            bsp.Register();

            return bsp;
        }

        /// <summary>
        /// Create a client port for connection to a remote device.
        /// </summary>
        /// <param name="portName">Port name e.g. "COM4"</param>
        /// <param name="endPoint">Remote device to connect to</param>
        /// <returns>A BluetoothSerialPort.</returns>
        public static BluetoothSerialPort CreateClient(string portName, BluetoothEndPoint endPoint)
        {
            string portPrefix;
            int portIndex;
            SplitPortName(portName, out portPrefix, out portIndex);
            BluetoothSerialPort bsp = new BluetoothSerialPort(portPrefix, portIndex);
            bsp.pep.flocal = false;
            bsp.pep.device = endPoint.Address.ToInt64();
            bsp.pep.uuidService = endPoint.Service;

            bsp.Register();

            return bsp;
        }
        /// <summary>
        /// Create a client port for connection to a remote device.  Auto allocates a port from the COM0-9 range.
        /// </summary>
        /// <param name="endPoint">Remote device to connect to.</param>
        /// <returns></returns>
        public static BluetoothSerialPort CreateClient(BluetoothEndPoint endPoint)
        {
            BluetoothSerialPort bsp = new BluetoothSerialPort("COM", 9);
            bsp.pep.flocal = false;
            bsp.pep.device = endPoint.Address.ToInt64();
            bsp.pep.uuidService = endPoint.Service;

            for (int iPort = 8; iPort > -1; iPort--)
            {
                try
                {
                    bsp.Register();
                    break;
                }
                catch
                {
                    bsp.portIndex = iPort;
                }
            }
            if (bsp.portIndex == 0)
            {
                throw new SystemException("Unable to create a Serial Port");
            }
            return bsp;
        }

        /// <summary>
        /// Creates a BluetoothSerialPort instance from an existing open virtual serial port handle.
        /// </summary>
        /// <param name="handle">Handle value created previously by BluetoothSerialPort.</param>
        /// <returns>BluetoothSerialPort wrapper around handle.</returns>
        public static BluetoothSerialPort FromHandle(IntPtr handle)
        {
            BluetoothSerialPort bsp = new BluetoothSerialPort("COM", 0);
            bsp.handle = handle;
            return bsp;
        }

        /// <summary>
        /// The full representation of the port name e.g. &quot;COM5&quot;
        /// </summary>
        public string PortName
        {
            get
            {
                return portPrefix + portIndex.ToString();
            }
        }

        private static void SplitPortName(string portName, out string prefix, out int index)
        {
            if (portName.Length < 4)
            {
                throw new ArgumentException("Invalid Port Name");
            }
            prefix = portName.Substring(0, 3);
            index = Int32.Parse(portName.Substring(3, 1));
        }

        /// <summary>
        /// The address of the remote device to which this port will connect (Client Ports only).
        /// </summary>
        public BluetoothAddress Address
        {
            get
            {
                return new BluetoothAddress(pep.device);
            }
        }

        /// <summary>
        /// The Bluetooth service to connect to.
        /// </summary>
        public Guid Service
        {
            get
            {
                return pep.uuidService;
            }
        }

        #region Local
        /// <summary>
        /// Specifies whether the port is a local service or for outgoing connections.
        /// </summary>
        /// <value>TRUE for a server port that accepts connections, or to FALSE for a client port that is used to creating outgoing connections.</value>
        public bool Local
        {
            get
            {
                return pep.flocal;
            }
        }
        #endregion

        #region Handle
        /// <summary>
        /// Native handle to virtual port.
        /// </summary>
        public IntPtr Handle
        {
            get
            {
                return this.handle;
            }
        }
        #endregion

        #region Close
        /// <summary>
        /// Closes the virtual serial port releasing all resources.
        /// </summary>
        public void Close()
        {
            GC.KeepAlive(this);

            if (handle != IntPtr.Zero)
            {
                bool success = DeregisterDevice(handle);

                if (success)
                {
                    handle = IntPtr.Zero;
                }
                else
                {
                    throw new SystemException("Error deregistering virtual COM port " + Marshal.GetLastWin32Error().ToString("X"));
                }
            }
        }
        #endregion

        #region P/Invokes

        [DllImport("coredll.dll", SetLastError = true)]
        private static extern IntPtr CreateFile(string lpFileName, int dwDesiredAccess, int dwShareMode, IntPtr lpSecurityAttributes,
            int dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("coredll.dll", SetLastError = true)]
        private static extern IntPtr RegisterDevice(
            string lpszType,
            int dwIndex,
            string lpszLib,
            ref PORTEMUPortParams dwInfo);
        [DllImport("coredll.dll", SetLastError = true)]
        private static extern IntPtr RegisterDevice(
            string lpszType,
            int dwIndex,
            string lpszLib,
            byte[] dwInfo);

        [DllImport("coredll.dll", SetLastError = true)]
        private static extern bool DeregisterDevice(
            IntPtr handle);

        [DllImport("coredll.dll", SetLastError = true)]
        private static extern bool DeviceIoControl(IntPtr hDevice, int dwIoControlCode, byte[] lpInBuffer, int nInBufferSize, byte[] lpOutBuffer, int nOutBufferSize, out int lpBytesReturned, int lpOverlapped);

        #endregion

        #region IDisposable Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            try
            {
                Close();
            }
            catch { }

            if (disposing)
            {
                portPrefix = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 
        /// </summary>
        ~BluetoothSerialPort()
        {
            Dispose(false);
        }

        #endregion
    }
}
