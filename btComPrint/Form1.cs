using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.IO.Ports;

namespace btComPrint
{
    public partial class Form1 : Form
    {
        btCOM _btcom;
        SerialPort serPort;
        bool _bConnected = false;
        bool bConnected
        {
            get { return _bConnected; }
            set { _bConnected = value;
                if(_bConnected)
                    btnConnect.Text = "disconnect";
                else
                    btnConnect.Text = "connect";
            }
        }
        public Form1()
        {
            InitializeComponent();
            txtBTaddress.Text = "0006660309e8";
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (!bConnected)
            {
                addLog("Trying to register...");
                _btcom = new btCOM(txtBTaddress.Text);
                if (_btcom._bRegistered)
                {
                    addLog("register OK. Using '" + _btcom._sCOM + "'");
                    bConnected = true;
                    printSample(_btcom._sCOM);
                }
                else
                {
                    addLog("could not register :-(");
                }
            }
            else
            {
                addLog("Closing COM port...");
                serPort.DataReceived -= serPort_DataReceived;
                serPort.Close();
                addLog("Closing btCOM...");
                _btcom.Close();
                bConnected = false;
            }
        }
        void printSample(string sCom)
        {
            addLog("opening '"+sCom+ "'...");
            try
            {
                serPort = new SerialPort(sCom);
                serPort.DataReceived += new SerialDataReceivedEventHandler(serPort_DataReceived);
                serPort.Open();

                addLog("Query printer...");
                serPort.WriteLine(IPLcode.getQueryStatus());

                addLog("printing sample...");
                serPort.WriteLine(IPLcode.getIPLsample());
            }
            catch (Exception ex)
            {
                addLog("open com failed with " + ex.Message);
            }
        }

        void serPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (e.EventType == SerialData.Chars)
            {
                string sData = serPort.ReadExisting();
                addLog("data received: " + sData);
            }
        }
        delegate void SetTextCallback(string text);
        public void addLog(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.txtLog.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(addLog);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                if (txtLog.Text.Length > 2000)
                    txtLog.Text = "";
                txtLog.Text += text + "\r\n";
                txtLog.SelectionLength = 0;
                txtLog.SelectionStart = txtLog.Text.Length - 1;
                txtLog.ScrollToCaret();
            }
        }

        private void Form1_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                addLog("Closing COM port...");
                serPort.DataReceived -= serPort_DataReceived;
                serPort.Close();
            }
            catch (Exception)
            {
            }
            try
            {
                addLog("Closing btCOM...");
                _btcom.Close();
                bConnected = false;
            }
            catch (Exception)
            {
            }
        }
    }
}