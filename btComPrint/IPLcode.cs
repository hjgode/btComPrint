using System;
using System.Collections.Generic;
using System.Text;

namespace btComPrint
{
    public class IPLcode
    {
        static string[] szIPLsample ={
	        "<STX><ESC>C<ETX>\n",
	        "<STX><ESC>P<ETX>\n",
	        "<STX>E4;F4;<ETX>\n",
	        "<STX>H0;o102,51;f0;c25;h20;w20;d0,30<ETX>\n",
	        "<STX>L1;o102,102;f0;l575;w5;<ETX>\n",
	        "<STX>B2;o203,153;c0,0;h100;w2;i1;d0,10<ETX>\n",
	        "<STX>I2;h1;w1;c20;<ETX>\n",
	        "<STX>R<ETX>\n",
	        "<STX><ESC>E4<ETX>\n",
	        "<STX><CAN><ETX>\n",
	        "<STX>THIS IS THE SAMPLE LABEL<CR><ETX>\n",
	        "<STX>SAMPLE<ETX>\n",
	        "<STX><ETB><ETX> \n",
	        null
        };
        public static string getIPLsample()
        {
            StringBuilder sb = new StringBuilder();
            int i = 0;
            while (szIPLsample[i] != null)
            {
                sb.Append(szIPLsample[i]);
                i++;
            }
            return sb.ToString();
        }
        public static string getQueryStatus()
        {
            string sStatus = "<ENQ>\n";
            return sStatus;
        }
    }
}
