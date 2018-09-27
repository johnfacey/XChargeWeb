using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Web;
using System.Threading;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        String OTK_Form = "";
        String OTK_Response = "";
        String OTK = "";
        String Data_Response = "";
        bool firstRun = true;
        String url = "";
        String xwebid = "";
        String authid = "";
        String tid = "";
        String transactionType = "";
        String amount = "";
        String responseFile = "";
        String dataURL = "https://test.t3secure.net/X-chargeweb.dll?";
        private void Form1_Load(object sender, EventArgs e)
        {

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length < 7)
            {
                
                MessageBox.Show("Not enough parameters");
                Application.Exit();
            }
            
            url = args[0];
            xwebid = args[1];
            authid = args[2];
            tid = args[3];
            transactionType = args[4];
            amount = args[5];
            responseFile = args[6];

            ProcessXCharge();
        }

        private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            Application.Exit();
        }
        
        public void ProcessXCharge() {

            //Gather Args url, xwebid, authid, tid, transactionType, Amount, responseFile

            //OTK_Response = Get_URI("https://test.t3secure.net/X-chargeweb.dll?XWebID=800000000419&AuthKey=CBLOkLFZpgvpy4pR3plXKVkSKVkD5Kyq&SpecVersion=XWebSecure3.0&TerminalID=80020262&Industry=ECOMMERCE&TransactionType=CreditSaleTransaction");
           
            OTK_Response = Get_URI(dataURL + 
                "XWebID=" + xwebid +
                "&AuthKey=" + authid + 
                "&SpecVersion=XWebSecure3.0" + 
                "&Industry=ECOMMERCE" +
                "&TerminalID=" + tid +
                 "&Amount=" + amount +
                "&TransactionType=" + transactionType);
            
            OTK = OTK_Response.Substring(OTK_Response.IndexOf("OTK=") + 4);
            if (OTK == "") { 
                MessageBox.Show("Invalid Response Key");
                Application.Exit();
            }
            OTK_Form = "https://integrator.t3secure.net/hpf/hpf.aspx?otk=" + OTK;
            webBrowser1.Navigate(OTK_Form);
            webBrowser1.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser1_DocumentCompleted);
        
        }

        public static string Get_URI(string uri)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
            HttpWebResponse response = (HttpWebResponse)req.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);

            string responseString = reader.ReadToEnd();

            reader.Close();
            responseStream.Close();
            response.Close();

            return responseString;
        }


        public void WriteResponse(String [] Response) {
            StreamWriter SW;
            SW = File.CreateText(Application.StartupPath + "\\" + responseFile);
            //MessageBox.Show(Application.StartupPath + "\\" + responseFile);
            for (int i = 0; i < Response.Length; i++) {
                string [] values = Response[i].Split('=');
                switch (values[0]) {
                    case "ResponseDescription":
                        SW.WriteLine("RESULT=SUCCESS");
                        break;
                    case "CardType":
                        SW.WriteLine("ACCOUNTTYPE=" + values[1]);
                        break;
                    case "ExpDate":
                        SW.WriteLine("EXPIRTATION=" + values[1]);
                        break;
                    case "TransactionID":
                        SW.WriteLine("XCTRANSACTIONID=" + values[1]);
                        break;
                    case "TransactionType":
                        if (values[1] == "CreditSaleTransaction")
                            SW.WriteLine("XCTRANSACTIONID=Purchase");

                        if (values[1] == "CreditVoidTransaction")
                            SW.WriteLine("XCTRANSACTIONID=VOID");

                        if (values[1] == "CreditReturnTransaction")
                            SW.WriteLine("XCTRANSACTIONID=Refund");
                        break;
                    default:
                        SW.WriteLine(values[0] + "=" + values[1]);
                        break;
                }
                
            }
            SW.Flush();
            SW.Close();
            Application.Exit();
        }

        public void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (firstRun == true) {
                firstRun = false;
                return;
            }
            Thread.Sleep(5000);

            Data_Response = Get_URI(dataURL + 
             "XWebID=" + xwebid + 
             "&AuthKey=" + authid + 
             "&SpecVersion=XWebSecure3.0" +
             "&TerminalID=" + tid + 
             "&Industry=ECOMMERCE" +
             "&OTK=" + OTK + 
             "&ResponseMode=POLL");
            String[] ResponseArray = Data_Response.Split('&');
            WriteResponse(ResponseArray);
         }
    }
}
