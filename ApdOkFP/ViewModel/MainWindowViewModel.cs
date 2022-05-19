using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApdOkFP.ViewModel.BaseClass;
using ApdOkFP.Model;
using OpalKelly.FrontPanel;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using System.Timers;

namespace ApdOkFP.ViewModel
{
    class MainWindowViewModel : ViewModelBase, IDataErrorInfo
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("ApdPulsecounter");   //
        private static System.Timers.Timer updateTimer;

        #region Fields
        OkFpFrontPanel _myFrontPanel;
        OkFpBoard _myBoard;

        okCFrontPanel m_dev;
        okCFrontPanelDevices devices;

        bool _connected = false;
        string _sysMsg;
        uint _pulseCounts;

        RelayCommand _connectCommand;
        RelayCommand _enableCounter;
        RelayCommand _clearCounter;

        #endregion // Fields

        #region MainWindow Properties
        public string SysMsg
        {
            get { return _sysMsg; }
            set
            {
                _sysMsg = value;
                base.OnPropertyChanged("SysMsg");
            }
        }

        public bool Connected
        { 
            get { return _connected; } 
            set 
            { 
                _connected = value;
                base.OnPropertyChanged("Connected");
            }
        }

        #endregion //MainWindow Properties

        #region App control Properties
        public uint DurationQuench
        {
            get { return _myFrontPanel.tQuench; }
            set
            {
                _myFrontPanel.tQuench = value;
                base.OnPropertyChanged("DurationQuench");
                string msg = string.Format("Change Quench Duration to {0} ns", value);
                SendQuenchDuration();
                SysMsg = msg;
                log.Info(msg);
            }
        }
        public uint DurationWait
        {
            get { return _myFrontPanel.tWait; }
            set
            {
                _myFrontPanel.tWait = value;
                base.OnPropertyChanged("DurationWait");
                string msg = string.Format("Change Wait Duration to {0} ns", value);
                SendWaitDuration();
                SysMsg = msg;
                log.Info(msg);
            }
        }
        public uint DurationReset
        {
            get { return _myFrontPanel.tReset; }
            set
            {
                _myFrontPanel.tReset = value;
                base.OnPropertyChanged("DurationReset");
                string msg = string.Format("Change Reset Duration to {0} ns", value);
                SendResetDuration();
                SysMsg = msg;
                log.Info(msg);
            }
        }
        public uint PulseCounts 
        { 
            get
            {
                return _pulseCounts;
            } 
  
            set
            {
                _pulseCounts = value;
                base.OnPropertyChanged("PulseCounts");
            }
        }

        #endregion //App control Properties

        #region Board Properties
        public string DevID
        {
            get { return _myBoard.devID; }
            private set 
            {
                _myBoard.devID = value;
                base.OnPropertyChanged("DevID");
            }
        }
        public string DevSN
        {
            get { return _myBoard.devSN; }
            private set
            {
                _myBoard.devSN = value;
                base.OnPropertyChanged("DevSN");
            }
        }
        public string DevFwVersion
        {
            get { return _myBoard.devFwVersion; }
            private set
            {
                _myBoard.devFwVersion = "firmware Version: "+value;
                base.OnPropertyChanged("DevFwVersion");
            }
        }

        #endregion //Board Properties

        #region Command Properties
        public ICommand ConnectCommand
        {
            get
            {
                if (_clearCounter == null)
                {
                    _clearCounter = new RelayCommand(
                        param => this.SendSimAptPulse(),
                        param => this.Connected
                        );
                }
                return _clearCounter;
            }
        }
        public ICommand StartStopCommand
        {
            get
            {
                if (_enableCounter ==null)
                {
                    _enableCounter = new RelayCommand(
                        param => this.enableCounter(param),
                        param => this.Connected
                        );

                }
                return _enableCounter;
            }
        }
        public ICommand ClearCounterCommand
        {
            get
            {
                if (_clearCounter == null)
                {
                    _connectCommand = new RelayCommand(
                        param => this.SendClearCounter(),
                        param => this.Connected
                        );
                }
                return _connectCommand;
            }
        }

        #endregion //Command Properties

        #region Constructor
        public MainWindowViewModel()
        {
            _myBoard = new OkFpBoard();
            _myFrontPanel = new OkFpFrontPanel();
            log.Info("application Initialized.");
            SysMsg = "Application Initilized successfully";
            Connect();
        }
        #endregion //Constructor

        #region FPGA Actions
        #region connection action
        public void Connect()
        {
            Connected = false;
            devices = new okCFrontPanelDevices();
            m_dev = devices.Open();
            if (m_dev == null)
            {
                string msg = "A device could not be opened.  Is one connected?";
                log.Warn("Not available FPGA device deteded.");
                Connected = false;
                SysMsg = msg;
            }
            else
            {
                okTDeviceInfo devInfo = new okTDeviceInfo();
                m_dev.GetDeviceInfo(devInfo);
                DevFwVersion = devInfo.deviceMajorVersion + "." + devInfo.deviceMinorVersion;
                DevSN = devInfo.serialNumber;
                //DevID = devInfo.deviceID;
                DevID = "FPGA Device";
                SysMsg = "Device Opened Successful";
                // Setup the PLL from defaults.
                m_dev.LoadDefaultPLLConfiguration();
                m_dev.ConfigureFPGA(@"C:\temp\FPGA\slow\slowSim.bit");
                SysMsg = "FPGA bit file download successful";
                log.Info("FPGA hardware initialized successfully");
                Connected = true;

                SendQuenchDuration();
                SendWaitDuration();
                SendResetDuration();

                SetTimer();
            }
        }
        #endregion//connection action

        #region Start/Stop
        void enableCounter(object param)
        {
            if ((bool)param)
            {
                log.Debug("Enable Counter");
                m_dev.SetWireInValue(0x03, 0x01);
                m_dev.UpdateWireIns();
            }
            else
            {
                log.Debug("Disable Counter");
                m_dev.SetWireInValue(0x03, 0x00);
                m_dev.UpdateWireIns();
            }
;
        }

        #endregion //Start/Stop

        #region Send Simulated Pulse
        public void SendSimAptPulse()
        {
            log.Debug("Send Simulated pulse");
            m_dev.ActivateTriggerIn(0x40, 1);
        }
        #endregion //Send Simulated Pulse   

        #region Send Clear Counter
        public void SendClearCounter()
        {
            log.Debug("Send Clear Counter");
            m_dev.ActivateTriggerIn(0x40, 0);
        }
        #endregion //Update Quench Number   

        #region SendQuenchDuration
        void SendQuenchDuration()
        {
            uint tick;
            tick = Convert.ToUInt32(Math.Ceiling((Convert.ToDouble(DurationQuench) / 5)));
            m_dev.SetWireInValue(0x00, tick);
            m_dev.UpdateWireIns();
        }
        #endregion //SendQuenchDuration

        #region SendWaitDuration
        void SendWaitDuration()
        {
            uint tick;
            tick = Convert.ToUInt32(Math.Ceiling((Convert.ToDouble(DurationWait) / 5)));
            m_dev.SetWireInValue(0x01, tick);
            m_dev.UpdateWireIns();
        }
        #endregion //SendWaitDuration

        #region update counter number
        uint ReadCounter()
        {
            m_dev.UpdateWireOuts();
            return m_dev.GetWireOutValue(0x20);
        }
        #endregion //update counter number

        #region SendResetDuration
        void SendResetDuration()
        {
            uint tick;
            tick = Convert.ToUInt32(Math.Ceiling((Convert.ToDouble(DurationReset) / 5)));
            m_dev.SetWireInValue(0x02, tick);
            m_dev.UpdateWireIns();
        }
        #endregion //SendResetDuration

        #endregion  //FPGA Actions

        #region IDataErrorInfo Members

        string IDataErrorInfo.Error
        {
            get { return null; }
        }
        public string this[string propertyName]
        {
            get { return this.GetValidationError(propertyName); }
        }
        #endregion //IdataErrorInfo Members

        static readonly string[] ValidatedProperties =
        {
            "DurationQuench",
            "DurationWait",
            "DurationReset",
        };

        #region validate if input is uint and not equal to 0
        string GetValidationError(string propertyName)
        {
            if (Array.IndexOf(ValidatedProperties, propertyName) < 0)
                return null;
            switch (propertyName)
            {
                case "DurationQuench":
                    return ValidateInt(DurationQuench);
                case "DurationWait":
                    return ValidateInt(DurationWait);
                case "DurationReset":
                    return ValidateInt(DurationReset);
                default:
                    log.Warn("Unexpected property being validated on Customer: " + propertyName);
                    break;
            }
            return null;
        }
        #endregion //validate if input is uint and not equal to 0

        string ValidateInt(uint number)
        {
            if ((number % 5) != 0)
            {
                MessageBox.Show("Nubmer input must be multiples of 5ns, the input will be rounded");
                return "Nubmer input must be multiples of 5ns, the input will be rounded";
            }

            return null;
        }

        #region Set Timer
        private void SetTimer()
        {
            // Create a timer with a two second interval.
            updateTimer = new System.Timers.Timer(10);
            // Hook up the Elapsed event for the timer. 
            updateTimer.Elapsed += OnTimedEvent;
            updateTimer.AutoReset = true;
            updateTimer.Enabled = true;
        }

        void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            PulseCounts = ReadCounter();
        }

        #endregion //Set Timer
    }

}
