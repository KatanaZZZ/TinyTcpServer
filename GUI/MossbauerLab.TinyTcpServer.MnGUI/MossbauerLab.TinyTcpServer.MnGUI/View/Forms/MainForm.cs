﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using log4net;
using log4net.Config;
using log4net.Core;
using log4net.Repository.Hierarchy;
using MossbauerLab.TinyTcpServer.Core.Client;
using MossbauerLab.TinyTcpServer.Core.Server;
using MossbauerLab.TinyTcpServer.MnGUI.Factories;
using MossbauerLab.TinyTcpServer.MnGUI.LogUtils;
using MossbauerLab.TinyTcpServer.MnGUI.View.Helpers;

namespace MossbauerLab.TinyTcpServer.MnGUI.View.Forms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            Load += (sender, args) => FillControls();
            _startButton.Click += (sender, args) => Start();
            _stopButton.Click += (sender, args) => Stop();
            _restartButton.Click += (sender, args) => Restart();
            _serverScriptButton.Click += OnChooseScriptFileButtonClick;
            _logLevelComboBox.SelectedIndexChanged += (sender, args) => ApplyLogLevel();
        }

        private void FillControls()
        {
            // fill ip address
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    _ipAddressComboBox.Items.Add(ip.ToString());
            }

            // load last IP address and TCP Port settings
            if (File.Exists(ConfigFile))
            {
                String[] filelines = File.ReadAllLines(ConfigFile);
                ApplySettings(GetOptions(filelines));
            }
            else
            {
                if (_ipAddressComboBox.Items.Count > 0)
                    _ipAddressComboBox.SelectedIndex = 0;
                _portTextBox.Text = DefaultTcpPort.ToString();
            }

            // add server type
            //foreach (KeyValuePair<ServerType, String> server in _servers)
                //_serverTypeComboBox.Items.Add(server.Value);
            //_serverTypeComboBox.SelectedIndex = 0;

            // init logger + fill log level
            XmlConfigurator.Configure();
            _logger = LogManager.GetLogger(typeof(MainForm));
            _richTextBoxAppender = new RichTextBoxAppender(_logsTextBox);
            ((Logger)_logger.Logger).AddAppender(_richTextBoxAppender);
            foreach (KeyValuePair<Level, String> level in _logLevels)
                _logLevelComboBox.Items.Add(level.Value);
            _logLevelComboBox.SelectedIndex = 5;

            // fill server config
            IList<String> configStrings = ServerConfigInfoHelper.GetConfigStrings(_serverConfig);
            foreach (String configString in configStrings)
                _serverParametersView.Items.Add(configString);
        }

        private IDictionary<String, String> GetOptions(String[] lines)
        {
            IDictionary<String, String> options = new Dictionary<String, String>();
            // todo: parse options ....
            return options;
        }

        private void ApplySettings(IDictionary<String, String> options)
        {

        }

        private void Start()
        {
            //ServerType serverType = ServerType.Scripting;
/*            foreach (KeyValuePair<ServerType, String> server in _servers)
            {
                if (String.Equals(_serverTypeComboBox.Items[_serverTypeComboBox.SelectedIndex].ToString(), server.Value))
                    serverType = server.Key;
            }
            if (_server == null)
            { 
                _server = ServerFactory.Create(serverType, _ipAddressComboBox.Items[_ipAddressComboBox.SelectedIndex].ToString(), 
                                               UInt16.Parse(_portTextBox.Text), _logger, _serverConfig);
            }
            _server.Start();
            if (_timers[0] == null)
            {
                System.Threading.Timer periodicalUpdater = new System.Threading.Timer(StateUpdater, null, 500, 500);
                _timers[0] = periodicalUpdater;
            }
            else _timers[0].Change(500, 500);*/
        }

        public void Stop()
        {
            _server.Stop(false);
            if (_timers[0] != null)
            {
                _timers[0].Change(-1, -1);
            }
            UpdateControlsState();
        }

        public void Restart()
        {
            Stop();
            Start();
        }

        public void OnChooseScriptFileButtonClick(Object sender, EventArgs args)
        {
            OpenFileDialog openScriptFile = new OpenFileDialog();
            openScriptFile.RestoreDirectory = true;
            openScriptFile.Title = @"Choose C# script file";
            openScriptFile.Filter = @" CSharp files (*.cs)|*.cs";
            if (openScriptFile.ShowDialog() == DialogResult.OK)
            {
                _scriptFile = openScriptFile.SafeFileName;
                _serverScriptBox.Text = Path.GetFileName(_scriptFile);
            }
        }

        void UpdateControlsState()
        {
            if (_server == null)
                return;
            _startButton.Enabled = !_server.IsReady;
            _restartButton.Enabled = _server.IsReady;
            _stopButton.Enabled = _server.IsReady;
            _applyButton.Enabled = !_server.IsReady;
            _logLevelComboBox.Enabled = !_server.IsReady;
            _ipAddressComboBox.Enabled = !_server.IsReady;
            _portTextBox.Enabled = !_server.IsReady;
        }

        void PopulateClients()
        {
            _clientsListBox.Items.Clear();
            IList<TcpClientContext> clients = _server.Clients;
            if (clients != null)
            {
                _clientsListBox.Items.Add(String.Format(TotalClientsTemplate, clients.Count));
                foreach (TcpClientContext client in clients)
                {
                    String ipAddress = ((IPEndPoint)client.Client.Client.RemoteEndPoint).Address.ToString();
                    _clientsListBox.Items.Add(String.Format(ClientInfoTemplate, client.Id, ipAddress));
                }
            }
        }

        void StateUpdater(Object state)
        {
            BeginInvoke((Action)(UpdateControlsState));
            BeginInvoke((Action) PopulateClients);
        }

        void ApplyLogLevel()
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.Threshold =_logLevels.First(item => item.Value.Equals(_logLevelComboBox.Items[_logLevelComboBox.SelectedIndex].ToString())).Key;
        }

        private const String ConfigFile = @".\settings.txt";
        private const UInt16 DefaultTcpPort = 9999;
        private const String TotalClientsTemplate = "Total connected clients: {0}";
        private const String ClientInfoTemplate = "Client {0}, ip: {1}";

/*        private readonly IDictionary<ServerType, String> _servers = new Dictionary<ServerType, String>()
        {
            {ServerType.Echo, "Echo server"},
            {ServerType.Time, "Time server"},
        };*/

        private readonly IDictionary<Level, String> _logLevels = new Dictionary<Level, String>()
        {
            {Level.Alert, "Alert"}, 
            {Level.Critical, "Critical"}, 
            {Level.Debug, "Debug"},
            {Level.Emergency, "Emergency"}, 
            {Level.Error, "Error"}, 
            {Level.Info, "Info"}, 
            {Level.Warn, "Warn"}
        };

        private String _scriptFile;
        private ILog _logger;
        private RichTextBoxAppender _richTextBoxAppender;
        private readonly TcpServerConfig _serverConfig = new TcpServerConfig();
        private ITcpServer _server;
        private readonly System.Threading.Timer[] _timers = new System.Threading.Timer[1];
    }
}
