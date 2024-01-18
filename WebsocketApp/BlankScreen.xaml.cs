using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Text.Json;
using System.Windows.Controls;
using System.Xml;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using static WebsocketApp.BlankScreen;
using System.Windows.Threading;

namespace WebsocketApp
{
    public partial class BlankScreen : Window
    {
        public class StoragePaths
        {
            public string KeysFolderPath { get; set; }
            public string KeyFileName { get; set; } // Nombres de archivos en la carpeta de claves
            public string DriversFolderPath { get; set; }
            public string DriverFileName { get; set; } // Nombres de archivos en la carpeta de conductores
            public string CarsFolderPath { get; set; }
            public string CarFileName { get; set; } // Nombres de archivos en la carpeta de autos
        }

        private StoragePaths storagePaths;

        private HubConnection hubConnection;
        private HubConnection hubConnectionOne;
        public BlankScreen()
        {
            InitializeComponent();

            InitializeSignalRConnection();

            LoadAndDisplayUserData();

            storagePaths = LoadStorageFolderPath();

            hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7231/chatHub")
                .Build();

            hubConnectionOne = new HubConnectionBuilder()
                .WithUrl("https://localhost:7231/jsonHub")
                .Build();

            StartHubConnectionAsync();
        }


        private StoragePaths LoadStorageFolderPath()
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string credentialsFilePath = Path.Combine(appDataFolder, "WebsocketApp", "credentials.xml");

            StoragePaths paths = new StoragePaths(); // Crear un objeto para almacenar las rutas

            if (File.Exists(credentialsFilePath))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(credentialsFilePath);

                // Obtener las rutas y asignarlas al objeto
                XmlNode keysPathNode = xmlDoc.SelectSingleNode("Credenciales/PathKey");
                XmlNode keysNameNode = xmlDoc.SelectSingleNode("Credenciales/NameKey");
                if (keysPathNode != null)
                {
                    paths.KeysFolderPath = keysPathNode.InnerText;
                    paths.KeyFileName = keysNameNode.InnerText;
                }

                XmlNode driversPathNode = xmlDoc.SelectSingleNode("Credenciales/PathDrivers");
                XmlNode driversNameNode = xmlDoc.SelectSingleNode("Credenciales/NameDrivers");
                if (driversPathNode != null)
                {
                    paths.DriversFolderPath = driversPathNode.InnerText;
                    paths.DriverFileName = driversNameNode.InnerText;
                }

                XmlNode carsPathNode = xmlDoc.SelectSingleNode("Credenciales/PathCars");
                XmlNode carsNameNode = xmlDoc.SelectSingleNode("Credenciales/NameCars");
                if (carsPathNode != null)
                {
                    paths.CarsFolderPath = carsPathNode.InnerText;
                    paths.CarFileName = carsNameNode.InnerText;
                }
            }

            return paths; // Retornar el objeto con las rutas
        }

        private void SaveJsonKeyToFile(string jsonKeyInformation, string jsonMessage)
        {
            if (!string.IsNullOrEmpty(storagePaths.KeysFolderPath)) // Verifica si la ruta de claves está especificada
            {
                try
                {
                    string jsonContent = jsonKeyInformation.Trim('"');

                    string fileName = $"{storagePaths.KeyFileName}.txt";
                    string filePath = Path.Combine(storagePaths.KeysFolderPath, fileName); // Usa la ruta de claves

                    File.WriteAllText(filePath, jsonContent);

                    AddLog($"{jsonMessage} and ", "JSON keys press saved", "");
                }
                catch (Exception ex)
                {
                    AddLog("", "", $"Error saving JSON to file: {ex.Message}");
                }
            }
            else
            {
                AddLog("", "", "Storage folder path is not specified.");
            }
        }

        private void SaveJsonToFile(JsonElement jsonInformation)
        {
            if (!string.IsNullOrEmpty(storagePaths.DriversFolderPath))
            {
                try
                {
                    if (jsonInformation.ValueKind == JsonValueKind.Array)
                    {
                        using (StreamWriter writer = new StreamWriter(Path.Combine(storagePaths.DriversFolderPath, $"{storagePaths.DriverFileName}.txt")))
                        {
                            writer.WriteLine("##;Name;Nationality;Team;Points");

                            foreach (JsonElement jsonObject in jsonInformation.EnumerateArray())
                            {
                                int number = jsonObject.GetProperty("number").GetInt32();
                                string name = jsonObject.GetProperty("name").GetString();
                                string nationality = jsonObject.GetProperty("nationality").GetString();
                                int points = jsonObject.GetProperty("points").GetInt32();
                                string team = "";

                                string line = $"{number};{name};{nationality};{team};{points}";
                                writer.WriteLine(line);
                            }
                        }

                        AddLog("", $"JSON array saved to {Path.Combine(storagePaths.DriversFolderPath, $"{storagePaths.DriverFileName}.txt")}", "");
                    }
                    else
                    {
                        AddLog("", "", "JSON information is not an array.");
                    }
                }
                catch (Exception ex)
                {
                    AddLog("", "", $"Error saving JSON to file: {ex.Message}");
                }
            }
            else
            {
                AddLog("", "", "Storage folder path is not specified.");
            }
        }

        private void SaveJson1ToFile(JsonElement jsonInformation)
        {
            if (!string.IsNullOrEmpty(storagePaths.CarsFolderPath))
            {
                try
                {
                    if (jsonInformation.ValueKind == JsonValueKind.Array)
                    {
                        using (StreamWriter writer = new StreamWriter(Path.Combine(storagePaths.CarsFolderPath, $"{storagePaths.CarFileName}.txt")))
                        {
                            writer.WriteLine("##CarNumber=Driver Name##");

                            foreach (JsonElement jsonObject in jsonInformation.EnumerateArray())
                            {
                                int carNumber = jsonObject.GetProperty("car_number").GetInt32();
                                string driverName = jsonObject.GetProperty("dirver_name").GetString();

                                string line = $"{carNumber}={driverName}";
                                writer.WriteLine(line);
                            }
                        }

                        AddLog("", $"JSON array saved to {Path.Combine(storagePaths.CarsFolderPath, $"{storagePaths.CarFileName}.txt")}", "");
                    }
                    else
                    {
                        AddLog("", "", "JSON information is not an array.");
                    }
                }
                catch (Exception ex)
                {
                    AddLog("", "", $"Error saving JSON to file: {ex.Message}");
                }
            }
            else
            {
                AddLog("", "", "Storage folder path is not specified.");
            }
        }

        private void LoadAndDisplayUserData()
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string credentialsFilePath = System.IO.Path.Combine(appDataFolder, "WebsocketApp", "credentials.xml");

            if (System.IO.File.Exists(credentialsFilePath))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(credentialsFilePath);

                XmlNode nicknameNode = xmlDoc.SelectSingleNode("Credenciales/Nickname");
                XmlNode idNode = xmlDoc.SelectSingleNode("Credenciales/Id");

                if (nicknameNode != null && idNode != null)
                {
                    txbWriteNickname.Text = "Nickname: " + nicknameNode.InnerText;
                    txbWriteNickname.Foreground = Brushes.Black;
                    txbWriteId.Text = "ID: " + idNode.InnerText;
                    txbWriteId.Foreground = Brushes.Black;
                }
            }
        }

        private void InitializeSignalRConnection()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7231/chatHub")
                .Build();

            hubConnectionOne = new HubConnectionBuilder()
                .WithUrl("https://localhost:7231/jsonHub")
                .Build();

            hubConnection.On<JsonElement>("ReceiveMessage", (jsonKeys) =>
            {
                if (jsonKeys.TryGetProperty("key1", out var key1Property) &&
                    jsonKeys.TryGetProperty("key2", out var key2Property))
                {
                    string key1 = key1Property.GetString();
                    string key2 = key2Property.GetString();

                    if (!string.IsNullOrEmpty(key1) && !string.IsNullOrEmpty(key2))
                    {
                        string logMessage = $"User press {key1} + {key2}  1";

                        var jsonInformation = $"{key1} + {key2}";

                        Dispatcher.Invoke(() =>
                        {
                            SaveJsonKeyToFile(logMessage, jsonInformation);
                        });
                    }
                    else if (string.IsNullOrEmpty(key1) && !string.IsNullOrEmpty(key2))
                    {
                        string logMessage = $"User press {key2}";

                        var jsonInformation = $"{key2}";

                        Dispatcher.Invoke(() =>
                        {
                            SaveJsonKeyToFile(logMessage, jsonInformation);
                        });
                    }
                }
            });

            hubConnectionOne.On<JsonElement, string>("ReceiveJson", (JsonElement jsonInfo, string jsonString) =>
            {
                Dispatcher.Invoke(() =>
                {
                    SaveJsonToFile(jsonInfo);
                    AddLog(jsonString, " - Save file drivers", "");
                });
            });

            hubConnectionOne.On<JsonElement, string>("ReceiveJson1", (JsonElement json1Info, string jsonString) =>
            {
                Dispatcher.Invoke(() =>
                {
                    SaveJson1ToFile(json1Info);
                    AddLog(jsonString, " - Save file cars", "");
                });
            });

            hubConnectionOne.StartAsync();
            hubConnection.StartAsync();
        }

        private async void StartHubConnectionAsync()
        {
            try
            {
                await hubConnection.StartAsync();
                await hubConnectionOne.StartAsync();
                AddLog("Connection started.", "", "");
            }
            catch (Exception ex)
            {
                AddLog("", "", $"Error starting connection: {ex.Message}");
            }
        }

        private void AddLog(string log, string logMessage, string logError)
        {
            Paragraph paragraph = new Paragraph();

            Span timeSpan = new Span(new Run($"{DateTime.Now.ToString("HH:mm:ss")} - "));
            timeSpan.Foreground = Brushes.Blue;

            Span logSpan = new Span(new Run(log));
            logSpan.Foreground = Brushes.White;

            Span saveSpan = new Span(new Run(logMessage));
            saveSpan.Foreground = Brushes.Green;

            Span saveError = new Span(new Run(logError));
            saveError.Foreground = Brushes.Red;

            paragraph.Inlines.Add(timeSpan);
            paragraph.Inlines.Add(logSpan);
            paragraph.Inlines.Add(saveSpan);
            paragraph.Inlines.Add(saveError);

            txtLogs.Document.Blocks.Add(paragraph);

            txtLogs.ScrollToEnd();
        }

        private void EditarInformacion_Click(object sender, RoutedEventArgs e)
        {
            EditForm editForm = new EditForm();
            editForm.EditCompleted += EditForm_EditCompleted;
            editForm.ShowDialog();
        }

        private void EditForm_EditCompleted(object sender, EventArgs e)
        {
            // Este método se ejecutará cuando se complete la edición en la ventana EditForm.
            // Actualiza las credenciales aquí.
            LoadAndDisplayUserData();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            // Cierra la conexión actual si está abierta
            if (hubConnection.State == HubConnectionState.Connected)
            {
                await hubConnection.StopAsync();
            }

            // Inicia la reconexión
            StartHubConnectionAsync();
        }

    }
}
