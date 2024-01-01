using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using UeiDaq;

namespace SerialPort
{
    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    public class Form1 : System.Windows.Forms.Form
    {
        private System.Windows.Forms.TextBox Resource;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button Start;
        private System.Windows.Forms.Button Stop;
        private System.Windows.Forms.Button Quit;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox Speed;
        private System.Windows.Forms.ComboBox DataBits;
        private System.Windows.Forms.ComboBox Parity;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox MessageToSend;
        private System.Windows.Forms.Button Send;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.ComboBox StopBits;

        private Session SrlSession;
        private SerialReader[] SrlReader;
        private System.Windows.Forms.ListView MessagesReceived;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox terminatorString;
        private AsyncCallback[] readerAsyncCallback;
        private IAsyncResult[] readerIAsyncResult;
        private delegate void UpdateUIDelegate(byte[] bytes);

        public Form1()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Resource = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.Send = new System.Windows.Forms.Button();
            this.MessageToSend = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.MessagesReceived = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Start = new System.Windows.Forms.Button();
            this.Stop = new System.Windows.Forms.Button();
            this.Quit = new System.Windows.Forms.Button();
            this.Speed = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.DataBits = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.Parity = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.StopBits = new System.Windows.Forms.ComboBox();
            this.terminatorString = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // Resource
            // 
            this.Resource.Location = new System.Drawing.Point(14, 41);
            this.Resource.Name = "Resource";
            this.Resource.Size = new System.Drawing.Size(360, 29);
            this.Resource.TabIndex = 10;
            this.Resource.Text = "pdna://192.168.100.40/Dev3/Com0,1";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(14, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(216, 27);
            this.label2.TabIndex = 11;
            this.label2.Text = "Resource name";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.Send);
            this.groupBox1.Controls.Add(this.MessageToSend);
            this.groupBox1.Location = new System.Drawing.Point(14, 95);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1008, 243);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Send Message";
            // 
            // Send
            // 
            this.Send.Enabled = false;
            this.Send.Location = new System.Drawing.Point(14, 27);
            this.Send.Name = "Send";
            this.Send.Size = new System.Drawing.Size(130, 203);
            this.Send.TabIndex = 1;
            this.Send.Text = "Send";
            this.Send.Click += new System.EventHandler(this.Send_Click);
            // 
            // MessageToSend
            // 
            this.MessageToSend.Location = new System.Drawing.Point(144, 27);
            this.MessageToSend.Multiline = true;
            this.MessageToSend.Name = "MessageToSend";
            this.MessageToSend.Size = new System.Drawing.Size(850, 203);
            this.MessageToSend.TabIndex = 0;
            this.MessageToSend.Text = "Type a message to send....";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.MessagesReceived);
            this.groupBox2.Location = new System.Drawing.Point(14, 352);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(1008, 244);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Received Messages";
            // 
            // MessagesReceived
            // 
            this.MessagesReceived.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.MessagesReceived.GridLines = true;
            this.MessagesReceived.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.MessagesReceived.HideSelection = false;
            this.MessagesReceived.Location = new System.Drawing.Point(14, 27);
            this.MessagesReceived.Name = "MessagesReceived";
            this.MessagesReceived.Size = new System.Drawing.Size(980, 203);
            this.MessagesReceived.TabIndex = 0;
            this.MessagesReceived.UseCompatibleStateImageBehavior = false;
            this.MessagesReceived.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Message";
            this.columnHeader1.Width = 540;
            // 
            // Start
            // 
            this.Start.Location = new System.Drawing.Point(14, 623);
            this.Start.Name = "Start";
            this.Start.Size = new System.Drawing.Size(260, 54);
            this.Start.TabIndex = 14;
            this.Start.Text = "Start";
            this.Start.Click += new System.EventHandler(this.Start_Click);
            // 
            // Stop
            // 
            this.Stop.Enabled = false;
            this.Stop.Location = new System.Drawing.Point(389, 623);
            this.Stop.Name = "Stop";
            this.Stop.Size = new System.Drawing.Size(259, 54);
            this.Stop.TabIndex = 15;
            this.Stop.Text = "Stop";
            this.Stop.Click += new System.EventHandler(this.Stop_Click);
            // 
            // Quit
            // 
            this.Quit.Location = new System.Drawing.Point(763, 623);
            this.Quit.Name = "Quit";
            this.Quit.Size = new System.Drawing.Size(259, 54);
            this.Quit.TabIndex = 16;
            this.Quit.Text = "Quit";
            this.Quit.Click += new System.EventHandler(this.Quit_Click);
            // 
            // Speed
            // 
            this.Speed.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Speed.Items.AddRange(new object[] {
            "110",
            "300",
            "600",
            "1200",
            "2400",
            "4800",
            "9600",
            "14400",
            "19200",
            "28800",
            "38400",
            "57600",
            "115200",
            "128000",
            "250000",
            "256000",
            "1000000",
            "Custom"});
            this.Speed.Location = new System.Drawing.Point(403, 41);
            this.Speed.Name = "Speed";
            this.Speed.Size = new System.Drawing.Size(173, 32);
            this.Speed.TabIndex = 17;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(403, 14);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(173, 27);
            this.label3.TabIndex = 18;
            this.label3.Text = "Speed";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(590, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 27);
            this.label1.TabIndex = 20;
            this.label1.Text = "Data Bits";
            // 
            // DataBits
            // 
            this.DataBits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DataBits.Items.AddRange(new object[] {
            "5",
            "6",
            "7",
            "8"});
            this.DataBits.Location = new System.Drawing.Point(590, 41);
            this.DataBits.Name = "DataBits";
            this.DataBits.Size = new System.Drawing.Size(101, 32);
            this.DataBits.TabIndex = 19;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(706, 14);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 27);
            this.label4.TabIndex = 22;
            this.label4.Text = "Parity";
            // 
            // Parity
            // 
            this.Parity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Parity.Items.AddRange(new object[] {
            "None",
            "Odd",
            "Even"});
            this.Parity.Location = new System.Drawing.Point(706, 41);
            this.Parity.Name = "Parity";
            this.Parity.Size = new System.Drawing.Size(100, 32);
            this.Parity.TabIndex = 21;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(821, 14);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(101, 27);
            this.label5.TabIndex = 24;
            this.label5.Text = "Stop Bits";
            // 
            // StopBits
            // 
            this.StopBits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.StopBits.Items.AddRange(new object[] {
            "1",
            "1.5",
            "2"});
            this.StopBits.Location = new System.Drawing.Point(821, 41);
            this.StopBits.Name = "StopBits";
            this.StopBits.Size = new System.Drawing.Size(101, 32);
            this.StopBits.TabIndex = 23;
            // 
            // terminatorString
            // 
            this.terminatorString.Location = new System.Drawing.Point(936, 41);
            this.terminatorString.Name = "terminatorString";
            this.terminatorString.Size = new System.Drawing.Size(86, 29);
            this.terminatorString.TabIndex = 25;
            this.terminatorString.Text = "\\r\\n";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(936, 14);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(72, 27);
            this.label6.TabIndex = 26;
            this.label6.Text = "Term.";
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(9, 22);
            this.ClientSize = new System.Drawing.Size(1234, 822);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.terminatorString);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.StopBits);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.Parity);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.DataBits);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Speed);
            this.Controls.Add(this.Quit);
            this.Controls.Add(this.Stop);
            this.Controls.Add(this.Start);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.Resource);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.groupBox2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.OnLoad);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.Run(new Form1());
        }

        private void InitUI(bool isRunning)
        {
            if (isRunning)
            {
                Start.Enabled = false;
                Quit.Enabled = false;
                Stop.Enabled = true;
                Send.Enabled = true;
            }
            else
            {
                Start.Enabled = true;
                Stop.Enabled = false;
                Quit.Enabled = true;
                Send.Enabled = false;
            }
        }

        private void OnLoad(object sender, System.EventArgs e)
        {
            Speed.SelectedIndex = 11;
            DataBits.SelectedIndex = 3;
            Parity.SelectedIndex = 0;
            StopBits.SelectedIndex = 0;

            InitUI(false);
        }

        private void Start_Click(object sender, System.EventArgs e)
        {
            try
            {
                // Textbox returns a verbatim literal string and won't correctly interpret
                // escaped characters, replace verbatim CR and LF with the regular equivalent
                string termStr = terminatorString.Text;
                termStr = termStr.Replace(@"\r", "\r");
                termStr = termStr.Replace(@"\n", "\n");

                SrlSession = new Session();
                SrlSession.CreateSerialPort(Resource.Text,
                                            SerialPortMode.RS485FullDuplex,
                                            (SerialPortSpeed)Speed.SelectedIndex,
                                            (SerialPortDataBits)DataBits.SelectedIndex,
                                            (SerialPortParity)Parity.SelectedIndex,
                                            (SerialPortStopBits)StopBits.SelectedIndex,
                                            termStr);

                // Configure timing to return serial message when either of the following conditions occured
                // - The termination string was detected
                // - 100 bytes have been received
                // - 10ms elapsed (rate set to 100Hz);
                SrlSession.ConfigureTimingForMessagingIO(1000, 100.0);
                SrlSession.GetTiming().SetTimeout(500);

                // Configure custom baud rate by calling SetCustomSpeed
                if (Speed.Text.Equals("Custom") || Speed.SelectedIndex >= 17)
                {
                    MessageBox.Show("Speed set programatically to 230400 baud.");
                    for (int c = 0; c < SrlSession.GetNumberOfChannels(); c++)
                    {
                        UeiDaq.SerialPort myPort = (UeiDaq.SerialPort)SrlSession.GetChannel(c);
                        myPort.SetCustomSpeed(230400);
                    }
                }


                SrlReader = new SerialReader[SrlSession.GetNumberOfChannels()];
                readerAsyncCallback = new AsyncCallback[SrlSession.GetNumberOfChannels()];
                readerIAsyncResult = new IAsyncResult[SrlSession.GetNumberOfChannels()];

                for (int c = 0; c < SrlSession.GetNumberOfChannels(); c++)
                {
                    SrlReader[c] = new SerialReader(SrlSession.GetDataStream(), SrlSession.GetChannel(c).GetIndex());

                    readerAsyncCallback[c] = new AsyncCallback(ReaderCallback);

                }

                // Start the session
                SrlSession.Start();
                for (int c = 0; c < SrlSession.GetNumberOfChannels(); c++)
                {
                    readerIAsyncResult[c] = SrlReader[c].BeginRead(200, readerAsyncCallback[c], c);
                }

                // Switch UI components to start state
                InitUI(true);
            }
            catch (UeiDaqException ex)
            {
                SrlSession.Dispose();
                SrlSession = null;
                MessageBox.Show(this, ex.Message, "Error");
                InitUI(false);
            }
        }

        private void Stop_Click(object sender, System.EventArgs e)
        {
            try
            {
                SrlSession.Stop();
            }
            catch (UeiDaqException ex)
            {
                MessageBox.Show(this, ex.Message, "Error");
            }

            // wait for current async call to complete
            // before destroying the session
            for (int c = 0; c < SrlSession.GetNumberOfChannels(); c++)
            {
                readerIAsyncResult[c].AsyncWaitHandle.WaitOne();
            }
            SrlSession.Dispose();
            SrlSession = null;
            InitUI(false);
        }

        private void Quit_Click(object sender, System.EventArgs e)
        {
            Application.Exit();
        }

        private void Send_Click(object sender, System.EventArgs e)
        {
            try
            {
                string message = MessageToSend.Text;
                message = message.Replace(@"\r", "\r");
                message = message.Replace(@"\n", "\n");
                //SrlWriter.Write(System.Text.Encoding.ASCII.GetBytes(message));
            }
            catch (UeiDaqException ex)
            {
                SrlSession.Dispose();
                SrlSession = null;
                MessageBox.Show(this, ex.Message, "Error");
                InitUI(false);
            }
        }

        private void ReaderCallback(IAsyncResult ar)
        {
            int index = (int)ar.AsyncState;
            try
            {

                byte[] recvBytes = SrlReader[index].EndRead(ar);

                // We can't directly access the UI from an asynchronous method
                // need to invoke a delegate that will take care of updating
                // the UI from the proper thread
                if (recvBytes != null)
                {
                    UpdateReceiveUI(recvBytes);
                }

                if (SrlSession != null && SrlSession.IsRunning())
                {
                    readerIAsyncResult[index] = SrlReader[index].BeginRead(200, readerAsyncCallback[index], index);
                }
            }
            catch (UeiDaqException ex)
            {
                // only handle exception if the session is running
                if (SrlSession.IsRunning())
                {
                    if (Error.Timeout == ex.Error)
                    {
                        // Ignore timeout error, they will occur if the send button is not
                        // clicked on fast enough!
                        // Just reinitiate a new asynchronous read.
                        readerIAsyncResult[index] = SrlReader[index].BeginRead(200, readerAsyncCallback[index], index);
                        Console.WriteLine("Timeout");
                    }
                    else
                    {
                        SrlSession.Dispose();
                        SrlSession = null;
                        MessageBox.Show(this, ex.Message, "Error");
                        InitUI(false);
                    }
                }
            }
        }

        private void UpdateReceiveUI(byte[] recvBytes)
        {
            if (this.InvokeRequired)
            {
                UpdateUIDelegate uidlg = new UpdateUIDelegate(UpdateReceiveUI);
                Invoke(uidlg, new object[] { recvBytes });
            }
            else
            {
                if (recvBytes.Length > 0)
                {
                    ListViewItem item = new ListViewItem(System.Text.Encoding.ASCII.GetString(recvBytes), 0);

                    MessagesReceived.Items.Add(item);

                    // scroll content of listview to new item
                    item.EnsureVisible();
                }
            }
        }
    }
}
