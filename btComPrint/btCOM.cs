using System;

using System.Collections.Generic;
using System.Text;


namespace btComPrint
{
    class btCOM : IDisposable
    {
        BluetoothSerialPort btSer;
        BluetoothEndPoint bep;
        BluetoothAddress bta;
        string sCom = "";
        public string _sCOM
        {
            get { return sCom; }
        }
        bool bRegistered = false;
        public bool _bRegistered{
            get{ return bRegistered;}
        }


        // "0006660309e8"
        // or 00.06.66.03.09.e8
        // or 00:06:66:03:09:e8
        public btCOM(string sBTaddress)
        {
            bta = BluetoothAddress.Parse(sBTaddress);
            bep = new BluetoothEndPoint(bta, BluetoothService.SerialPort);
            string cPre="COM";
            int iIdx=0;
            
            bool bSuccess = false;
            for (iIdx = 9; iIdx >= 0; iIdx--)
            {
                try{
                    btSer = BluetoothSerialPort.CreateClient(cPre, iIdx, bep);
                    bSuccess = true;
                    break;
                }
                catch (Exception ex)
                {
                    addLog("CreateClient failed for '" + cPre + iIdx.ToString() + ":', " + ex.Message);
                }
            }
            if (!bSuccess)
            {
                cPre = "BSP";
                for (iIdx = 9; iIdx >= 0; iIdx--)
                {
                    try
                    {
                        btSer = BluetoothSerialPort.CreateClient(cPre, iIdx, bep);
                        bSuccess = true;
                        break;
                    }
                    catch (Exception ex)
                    {
                        addLog("CreateClient failed for '" + cPre + iIdx.ToString() + ":', " + ex.Message);
                    }
                }
            }
            if (bSuccess)
            {
                sCom = cPre + iIdx.ToString() + ":";
                bRegistered = true;
            }
        }

        public void Close()
        {
            if (bRegistered)
            {
                btSer.Close();
                bRegistered = false;
            }
        }

        public void Dispose()
        {
            this.Close();
        }

        void addLog(string s)
        {
            System.Diagnostics.Debug.WriteLine("btCOM: " + s);
        }
    }
}
