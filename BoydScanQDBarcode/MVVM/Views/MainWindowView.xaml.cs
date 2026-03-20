using BoydScanQDBarcode.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using log4net;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Reflection.Metadata;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BoydScanQDBarcode.MVVM.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindowView : Window
    {
        #region Fields
        private Cls_DBMsSQL ParameterDB = new Cls_DBMsSQL();

        private int iLifeTimeLimit;
        private string StationID;

        //Used for logging application events, errors, and important information. log4net is a popular logging library that allows for flexible logging configurations and supports various log levels (Info, Warning, Error, etc.).
        private ILog log = LogManager.GetLogger("ScanQRCableLogger");

        //Used for periodic UI updates to ensure the input textbox remains focused for efficient scanning workflow
        private System.Timers.Timer focusTimer;

        //Used for periodic log cleanup if you want to implement log retention policies in the future
        private System.Timers.Timer _logTimer;

        //Used for pending log messages if you want to implement asynchronous logging in the future
        //Safe for multi-threaded access if you decide to log from background threads or tasks
        private ConcurrentQueue<string> _pendingLogsQueue;
        #endregion

        #region Properties
        public ObservableCollection<string> LogMessages { get; set; }
        #endregion



        #region Constructor
        public MainWindowView()
        {
            InitializeComponent();

            this.DataContext = this;

            log.Info("Application started. Initializing MainWindowView...");

            StateCommon.Secsion = StateCommon.SecsionTest.QD_Asembly_Dummy;

            string stationID = ClsIO.ReadValue("LOGGING_DATA", "Tester_ID", "", "C:\\Aavid_Test\\Setup-ini\\LCS_Logging_Setup.ini").Trim().Trim('"').Trim('\'');
            if (stationID == "" || stationID == null || stationID == string.Empty)
            {
                stationID = ClsIO.ReadValue("DEVICES", "Tester_ID", "", "C:\\Aavid_Test\\Setup-ini\\Leak_Test_Setup.ini").Trim().Trim('\'').Trim('"');
                if (stationID == "" || stationID == null || stationID == string.Empty)
                {
                    WDMessageView frmMessageError = new WDMessageView("Station ID is not valid. Please call Technical check again");
                    {
                        frmMessageError.ShowDialog();
                        Environment.Exit(0);
                        this.Close();
                    }
                }
            }

            LogMessages = new ObservableCollection<string>();
            _pendingLogsQueue = new ConcurrentQueue<string>();
            LogMessages.CollectionChanged += LogMessages_CollectionChanged;

            _logTimer = new System.Timers.Timer(5 * 60 * 1000);
            _logTimer.Elapsed += LogTimer_Elapsed;
            _logTimer.AutoReset = true; // Set to true to keep the timer running periodically
            _logTimer.Start();


            //Initialize database connections
            AddLogInfor($"Initializing Parameter database connections...");
            try
            {
                ParameterDB.Initialize("10.102.4.20", "EquipmentManagerment", "readuser", "Boyd@2025");
                ParameterDB.Open();
                AddLogSuccess($"Parameter database connection initialized successfully.");
            }
            catch (Exception ex)
            {
                AddLogError($"Failed to connect to Parameter database: {ex.Message}");
                WDMessageView frmMessageError = new WDMessageView("Failed to connect to Parameter database. Please call Technical check again");
                {
                    frmMessageError.ShowDialog();
                    Environment.Exit(0);
                    this.Close();
                }
            }

            // Get fixture lifetime limit from database
            AddLogInfor("Getting fixture lifetime limit from database...");
            GetFixtureLifeTime();
            AddLogSuccess($"Fixture lifetime limit for station {StateCommon.Secsion.ToString()}: {iLifeTimeLimit} cycles");

            StationID = stationID;
            txtStationID.Text = stationID;
            txtQDLifetime.Text = iLifeTimeLimit.ToString();

            txtQDDataInput.Focus();

            focusTimer = new System.Timers.Timer(3000); // Check every 3 seconds
            focusTimer.Elapsed += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (!txtQDDataInput.IsFocused)
                    {
                        txtQDDataInput.Focus();
                    }
                });
            };
            focusTimer.Start();

            SetWaitResult();
        }


        #endregion



        #region Event Handlers
        /// <summary>
        /// Main event handler for when the text in the QD data input textbox changes. This is where the core logic of processing the scanned fixture occurs, including validating the input, checking against the database, updating logs, and providing feedback to the user through the UI. It ensures that only valid fixtures are processed and that any issues are clearly communicated to the operator.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnQDDataInputChanged(object sender, TextChangedEventArgs e)
        {
            // Handle text change event for QD data input
            // You can add your logic here to process the input data

            string QDInput = txtQDDataInput.Text;
            if (QDInput.Length == 17)
            {
                await SetCheckingResult();

                AddLogInfor($"-------------------------------------------------------------------------------------------------------------------------");
                AddLogInfor($"-------------------------------------------------------------------------------------------------------------------------");

                AddLogInfor($"Scanned QD fixture with Serial Number: {QDInput}");

                bool bCheckCable = CheckCableInDB(QDInput.Trim());
                AddLogInfor($"Check cable in database completed for fixture {QDInput}. Valid: {bCheckCable}");

                if (bCheckCable)
                {
                    try
                    {
                        InsertCableInDB(QDInput.Trim());
                        AddLogSuccess($"Fixture {QDInput} has been successfully inserted into the database.");
                        SetPassResult();
                    }
                    catch (Exception ex)
                    {
                        AddLogError($"Failed to insert fixture {QDInput} into database: {ex.Message}");
                        WDMessageView frmMessageError = new WDMessageView($"Failed to record fixture in database. Please call Technical check. Error: {ex.Message}");
                        {
                            frmMessageError.ShowDialog();
                        }
                        SetFailResult();
                    }

                    txtQDDataInput.Clear();
                    
                    if(txtQDDataInput.IsFocused == false)
                    {
                        txtQDDataInput.Focus();
                    }

                }
                else
                {
                    SetFailResult();
                    AddLogError($"Fixture {QDInput} is invalid and cannot be used. Please check the fixture and try again.");
                    txtQDDataInput.Clear();

                    if (txtQDDataInput.IsFocused == false)
                    {
                        txtQDDataInput.Focus();
                    }
                }
            }
        }


        /// <summary>
        /// Event handler for when the application window is closing. This is where you can perform any necessary cleanup, such as closing database connections, stopping timers, and logging the shutdown process. It ensures that resources are properly released and that the application exits gracefully.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Clean up resources, close database connections, etc.
            AddLogInfor("Application is closing. Cleaning up resources...");
            if (ParameterDB != null)
            {
                ParameterDB.Close();
                AddLogInfor("Parameter database connection closed.");
            }

            AddLogInfor("Cleanup completed. Application will now exit.");

            //Stop the focus timer to prevent it from firing after the application has started closing.
            focusTimer?.Stop();

            //Stop the log timer to prevent it from firing after the application has started closing.
            //This is important to avoid any potential issues with trying to access UI elements or resources that may have already been disposed.
            _logTimer?.Stop();

            // Flush any pending logs before shutting down log4net to ensure all log messages are written out.
            FlushPendingLogs();

            // Shutdown log4net to release any resources it may be holding and to ensure a clean exit.
            LogManager.Shutdown();
        }


        /// <summary>
        /// Event handler for when the LogMessages collection changes. This can be used to implement additional logic when new log messages are added, such as auto-scrolling a log view or triggering notifications for certain log levels. Currently, it is not implemented and will throw an exception if triggered, but you can enhance it based on your application's needs.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void LogMessages_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Only care about new items being added to the collection
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                foreach (string newItem in e.NewItems)
                {
                    //Add new log message to the pending logs queue for potential asynchronous processing in the future
                    _pendingLogsQueue.Enqueue(newItem);
                }
            }
        }

        /// <summary>
        /// Event handler for when the log timer elapses. This can be used to implement periodic log maintenance tasks, such as clearing old log messages, archiving logs, or performing health checks on the logging system. Currently, it is not implemented and will throw an exception if triggered, but you can enhance it based on your application's needs.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void LogTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            FlushPendingLogs();
        }

        #endregion



        #region Check formats and database operations
        /// <summary>
        /// Check if the scanned serial number matches the expected format for QD fixtures. This is a basic check and can be enhanced with regex or more complex logic if needed.
        /// </summary>
        /// <param name="sn"></param>
        /// <returns></returns>
        private bool CheckQDFormat(string sn)
        {
            if (sn.StartsWith("QD_00_"))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Check if the scanned cable (fixture) is valid for use based on its serial number. This includes checking if it exists in the database, 
        /// calculating remaining lifetime, and determining if it needs to be scrapped. 
        /// If the cable is valid, it will return true; otherwise, it will show a warning message and return false.
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        private bool CheckCableInDB(string serialNumber)
        {
            if (!CheckQDFormat(serialNumber))
            {
                //log.Warn($"[{serialNumber}] does not match the expected format for station {StateCommon.Secsion.ToString()}");
                AddLogError($"Scanned fixture {serialNumber} does not match the expected format for station {StateCommon.Secsion.ToString()}");

                Dispatcher.Invoke(() =>
                {
                    var dialog = new WDMessageView($"Fixture không phải thuộc station này!!!\r\nCall technical kiểm tra.",10000);
                    dialog.ShowDialog();   // Fixed: removed 'this'
                });
                return false;
            }
            string query = $@"SELECT [SerialNumber]
                                FROM [EquipmentManagerment].[dbo].[EquipmentRemaining]
                                where SerialNumber = '{serialNumber}' ";

            DataTable dtResult = ParameterDB.ExecuteQuery(query);


            //Calculate remaining lifetime and update UI
            int timeRemaining = iLifeTimeLimit - dtResult.Rows.Count;
            txtQDRemainingTimes.Text = timeRemaining.ToString();
            borderRemainingTimes.Background = (timeRemaining <= ClsDefine.DefineSystem.iLifeTimeWarning) ? Brushes.Red : Brushes.White;

            string message = $"Fixture {serialNumber} has been used {dtResult.Rows.Count} times based on database records. Remaining times {timeRemaining}.";
            if (timeRemaining <= ClsDefine.DefineSystem.iLifeTimeWarning)
            {
                AddLogWarning(message + $" Fixture is approaching its lifetime limit.");
            }
            else
            {
                AddLogInfor(message);
            }

            //
            query = $@"SELECT TOP 1 [Date_Time]
                 FROM [EquipmentManagerment].[dbo].[EquipmentRemaining]
                 WHERE SerialNumber = '{serialNumber}' 
                 ORDER BY Date_Time DESC";
            dtResult = ParameterDB.ExecuteQuery(query);

            if (dtResult.Rows.Count > 0)
            {
                DateTime latestDate = Convert.ToDateTime(dtResult.Rows[0]["Date_Time"]);
                
                double hoursSinceLastUse = (DateTime.Now - latestDate).TotalHours;

                AddLogInfor($"Latest usage of fixture {serialNumber} was on {latestDate}. Time since last use: {hoursSinceLastUse:F2} hours.");

                if(hoursSinceLastUse < ClsDefine.DefineSystem.iLastScanTimeOut)
                {
                    AddLogWarning($"Fixture {serialNumber} was last used {hoursSinceLastUse:F2} hours ago, which is less than the recommended rest time of {ClsDefine.DefineSystem.iLastScanTimeOut} hours. " +
                        $"Please ensure the fixture has had sufficient rest before reuse to avoid potential issues.");

                    Dispatcher.Invoke(() =>
                    {
                        var dialog = new WDMessageView($"Fixture {serialNumber} đã được scan {hoursSinceLastUse:F2} tiếng trước, ít hơn thời gian tối thiểu để được scan lại là {ClsDefine.DefineSystem.iLastScanTimeOut} tiếng. ");
                        dialog.ShowDialog();   // Fixed: removed 'this'
                    });

                    return false;
                }

            }



            if (dtResult.Rows.Count > iLifeTimeLimit)
            {


                try
                {
                    InsertCableScrapInDB(serialNumber);
                    AddLogWarning($"Fixture {serialNumber} has exceeded its lifetime limit and has been marked as scrapped in the database.");
                }
                catch
                {
                    AddLogError($"Failed to mark fixture {serialNumber} as scrapped in database. Please check the database connection and try again.");

                    Dispatcher.Invoke(() =>
                    {
                        var dialog = new WDMessageView($"Không đẩy được dữ liệu fixture {serialNumber} lên bảng Scrapped! Hãy gọi Technical! ", 10000);
                        dialog.ShowDialog();   // Fixed: removed 'this'
                    });


                }


                Dispatcher.Invoke(() =>
                {
                    var dialog = new WDMessageView($"Fixture cần được thay thế!!!\r\n tuổi thọ còn lại {iLifeTimeLimit - dtResult.Rows.Count}", 10000);
                    dialog.ShowDialog();   // Fixed: removed 'this'
                });
                return false;
            }
            else
                return true;
        }

        /// <summary>
        /// Insert a new record into the database for the scanned cable (fixture) with its serial number, current date/time, process monitor, and station ID.
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        private bool InsertCableInDB(string serialNumber)
        {
            try
            {
                string insertQuery = $@"INSERT INTO [EquipmentManagerment].[dbo].[EquipmentRemaining] 
                                    ( [SerialNumber]
                                      ,[Date_Time]
                                      ,[Process_Monitor]
                                      ,[StationID]) 
                                    VALUES ('{serialNumber}', GETDATE(), '{StateCommon.Secsion.ToString()}','{StationID}')";

                ParameterDB.ExecuteQuery(insertQuery);
            }
            catch (Exception ex)
            { 
                throw new Exception($"{ex.Message}");
            }

            return true;
        }

        /// <summary>
        /// Insert a new record into the database to mark the cable (fixture) as scrapped when it exceeds its lifetime limit. This allows tracking of scrapped fixtures for future reference and analysis.
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        private bool InsertCableScrapInDB(string serialNumber)
        {
            try
            {
                string insertQuery = $@"INSERT INTO [EquipmentManagerment].[dbo].[EquipmentFixtureScrapped] 
                                    ( [SerialNumber]
                                      ,[Date_Time])
                                    VALUES ('{serialNumber}', GETDATE())";
                ParameterDB.ExecuteQuery(insertQuery);
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }

            return true;
        }
        #endregion



        #region Helper Methods
        /// <summary>
        /// Get fixture lifetime limit from database based on the current station/section. This allows different stations to have different lifetime limits if needed.
        /// </summary>
        private void GetFixtureLifeTime()
        {
            iLifeTimeLimit = 6000;      // default

            string query = $@"SELECT FixtureType, LifeTimeLimit
                            FROM [EquipmentManagerment].[dbo].[EquipmentFixtureLifeTimeSetting]
                            WHERE Process_Monitor = '{StateCommon.Secsion.ToString()}'";

            DataTable dt = ParameterDB.ExecuteQuery(query);

            if (dt != null)
            {
                if(dt.Rows.Count == 0)
                {
                    AddLogWarning($"No lifetime limit found for station {StateCommon.Secsion.ToString()} in the database. Using default value: {iLifeTimeLimit} cycles.");
                }

                foreach (DataRow row in dt.Rows)
                {
                    string fixtureType = row["FixtureType"].ToString();
                    int lifeTime = Convert.ToInt32(row["LifeTimeLimit"]);

                    switch (fixtureType)
                    {
                        case "QD":
                            iLifeTimeLimit = lifeTime;
                            break;
                    }
                }
            }
        }

        private void FlushPendingLogs()
        {
            // Check if there are any pending logs to write
            if (_pendingLogsQueue.IsEmpty) return;

            List<string> logsToWrite = new List<string>();



            //Get all pending logs from the queue to write them in a batch.
            //This can help improve performance if you decide to write logs to a file or database asynchronously in the future.
            while (_pendingLogsQueue.TryDequeue(out string item))
            {
                logsToWrite.Add(item);
            }

            if (logsToWrite.Any())
            {
                // For demonstration, we will just log the batch of messages as a single log entry.
                // In a real application, you might want to write these to a file or database.
                string logContent = string.Join(Environment.NewLine + " - ", logsToWrite);
                log.Info($"[Periodly update] New Lines:\n - {logContent}");
            }
        }
        private void AddLogMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                LogMessages.Add($"[{DateTime.Now.ToString("HH:mm:ss")}] {message}");
            });
        }

        private void AddLogInfor(string message)
        {
            AddLogMessage($"[INFO] {message}");
        }

        private void AddLogWarning(string message)
        {
            AddLogMessage($"[WARNING] {message}");
        }

        private void AddLogError(string message)
        {
            AddLogMessage($"[ERROR] {message}");
        }
        private void AddLogSuccess(string message)
        {
            AddLogMessage($"[SUCCESS] {message}");
        }

        private void ClearLogMessages()
        {
            Dispatcher.Invoke(() =>
            {
                LogMessages.Clear();
            });
        }

        private void RaisePropertyChanged(string propertyName)
        {
            Dispatcher.Invoke(() =>
            {
                RaisePropertyChanged raisePropertyChanged = new RaisePropertyChanged();
                raisePropertyChanged.OnPropertyChanged(propertyName);
            });
        }

        private async Task SetPassResult()
        {
            Dispatcher.Invoke(() =>
            {
                txtResult.Text = "PASS";
                borderResult.Background = Brushes.LimeGreen;
            });

            await Task.Delay(100);
        }

        private async Task SetFailResult()
        {
            Dispatcher.Invoke(() =>
            {
                txtResult.Text = "FAIL";
                borderResult.Background = Brushes.Red;
            });

            await Task.Delay(100);
        }

        private async Task SetCheckingResult()
        {
            Dispatcher.Invoke(() =>
            {
                txtResult.Text = "CHECKING...";
                borderResult.Background = Brushes.Yellow;
            });
            await Task.Delay(200);
        }

        private async Task SetWaitResult()
        {
            Dispatcher.Invoke(() =>
            {
                txtResult.Text = "WAITING...";
                borderResult.Background = Brushes.Gray;
            });
            await Task.Delay(100);
        }

        #endregion
    }
}