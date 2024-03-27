using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Text.Json;
using System.Xml;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using Quobject.SocketIoClientDotNet.Client;
using System.Collections.Immutable;
using Socket = Quobject.SocketIoClientDotNet.Client.Socket;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Xml.Linq;
using System.Reflection.PortableExecutable;
using System.Text.Json.Nodes;
using System.Linq;

namespace WebsocketApp
{
    public partial class BlankScreen : Window
    {
        public bool _connected = false;
        private Socket _instance;

        public class StoragePaths
        {
            public string KeysFolderPath { get; set; }
            public string KeyFileName { get; set; }
            public string DriversFolderPath { get; set; }
            public string DriverFileName { get; set; }
            public string CarsFolderPath { get; set; }
            public string CarFileName { get; set; }
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
                            writer.WriteLine("##;Name;Nationality;Individual Id;Points");

                            foreach (JsonElement jsonObject in jsonInformation.EnumerateArray())
                            {
                                int number = jsonObject.GetProperty("number").GetInt32();
                                string name = jsonObject.GetProperty("first_name").GetString() + " " + jsonObject.GetProperty("last_name").GetString();
                                int individual_id = jsonObject.GetProperty("individual_id").GetInt32();
                                int points = jsonObject.GetProperty("total_points").GetInt32();

                                string line = $"{number};{name};{individual_id};{points}";
                                writer.WriteLine(line);
                            }
                        }
                        Dispatcher.Invoke(() =>
                        {
                            AddLog("", $"JSON array saved to {Path.Combine(storagePaths.DriversFolderPath, $"{storagePaths.DriverFileName}.txt")}", "");
                        });
                        
                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            AddLog("", "", "JSON information is not an array.");
                        });
                        
                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        AddLog("", "", $"Error saving JSON to file: {ex.Message}");
                    });
                    
                }
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    // AddLog("", $"JSON array saved to {Path.Combine(storagePaths.CarsFolderPath, $"{storagePaths.CarFileName}.txt")}", "");
                });
                AddLog("", "", "Storage folder path is not specified.");
            }
        }

        private void SaveParticipant(JsonElement participant)
        {
            if (!string.IsNullOrEmpty(storagePaths.DriversFolderPath))
            {
                try
                {
                    string sourceFilePath = Path.Combine(storagePaths.KeysFolderPath, $"{storagePaths.KeyFileName}.txt");
                    string tempFilePath = Path.Combine(storagePaths.KeysFolderPath, $"{storagePaths.KeyFileName}_temp.txt");

                    using (StreamWriter writer = new StreamWriter(tempFilePath))
                    {
                        int participant_id = participant.GetProperty("id").GetInt32();
                        int individual_id = participant.GetProperty("individual_id").GetInt32();
                        string eagame = participant.GetProperty("nickname").GetString();
                        string name = $"{participant.GetProperty("first_name").GetString()} {participant.GetProperty("last_name").GetString()}";

                        if (string.IsNullOrEmpty(eagame))
                        {
                            string errorMessage = $"Error: Participant with id {participant_id} and individual id {individual_id} has Eagame as null. Name: {name}";
                            Dispatcher.Invoke(() =>
                            {
                                AddLog("", "", errorMessage);
                            });
                        }
                        else
                        {
                            bool participantUpdated = false;

                            using (StreamReader reader = new StreamReader(sourceFilePath))
                            {
                                string line;

                                while ((line = reader.ReadLine()) != null)
                                {
                                    string[] parts = line.Split(';');
                                    string existingEagame = parts[0].Trim();

                                    if (existingEagame == eagame)
                                    {
                                        writer.WriteLine($"{eagame};{name}");
                                        participantUpdated = true;
                                    }
                                    else
                                    {
                                        writer.WriteLine(line);
                                    }
                                }
                            }

                            if (!participantUpdated)
                            {
                                writer.WriteLine($"{eagame};{name}");
                            }
                        }

                        Dispatcher.Invoke(() =>
                        {
                            AddLog("", $"JSON object saved to {sourceFilePath}", "");
                        });
                    }

                    File.Delete(sourceFilePath);
                    File.Move(tempFilePath, sourceFilePath);
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        AddLog("", "", $"Error saving JSON to file: {ex.Message}");
                    });
                }
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    AddLog("", "", "Storage folder path is not specified.");
                });
            }
        }

        private void SaveFinalClassification(JsonElement finalClassification)
        {
            if (!string.IsNullOrEmpty(storagePaths.DriversFolderPath))
            {
                try
                {
                    if (finalClassification.ValueKind == JsonValueKind.Array)
                    {

                        using (StreamWriter writer = new StreamWriter(Path.Combine(storagePaths.CarsFolderPath, $"{storagePaths.CarFileName}.txt")))
                        {
                            JsonElement firstElement = finalClassification.EnumerateArray().First();
                            if (firstElement.TryGetProperty("udp_session_type", out JsonElement udpSessionType))
                            {
                                string sessionType = udpSessionType.GetString();
                                if (sessionType == "1" || sessionType == "2" || sessionType == "3" || sessionType == "4" || sessionType == "5" || sessionType == "6" || sessionType == "7" || sessionType == "8")
                                {
                                    writer.WriteLine("Vehicle Name;Best Lap Time In MS;Team Id");
                                }
                                else
                                {
                                    writer.WriteLine("Vehicle Name;Total Race Time / Best Lap Time In MS;Team Id");
                                }
                            }


                            foreach (JsonElement jsonObject in finalClassification.EnumerateArray())
                            {
                                if (jsonObject.TryGetProperty("vehicleName", out JsonElement vehicleName) &&
                                    jsonObject.TryGetProperty("m_bestLapTimeInMS", out JsonElement bestLapTime) &&
                                    jsonObject.TryGetProperty("m_totalRaceTime", out JsonElement totalRaceTime) &&
                                    jsonObject.TryGetProperty("teamId", out JsonElement teamId))
                                {
                                    string vehicle_name = vehicleName.GetString();
                                    int? best_lap_time_in_ms = bestLapTime.TryGetInt32(out int lapTime) ? lapTime : null;
                                    int? total_race_time = totalRaceTime.TryGetInt32(out int raceTime) ? raceTime : null;
                                    int? conditional = best_lap_time_in_ms != null ? best_lap_time_in_ms : total_race_time;
                                    string team_id = teamId.GetString();

                                    string line = $"{vehicle_name};{conditional};{team_id}";
                                    writer.WriteLine(line);
                                }
                                else
                                {
                                    Dispatcher.Invoke(() =>
                                    {
                                        AddLog("", "", "One or more properties are missing in the JSON object.");
                                    });
                                }
                            }
                        }
                        Dispatcher.Invoke(() =>
                        {
                            AddLog("", $"JSON array saved to {Path.Combine(storagePaths.CarsFolderPath, $"{storagePaths.CarFileName}.txt")}", "");
                        });

                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                            AddLog("", "", "JSON information is not an array.");
                        });

                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        AddLog("", "", $"Error saving JSON to file: {ex.Message}");
                    });
                }
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    AddLog("", "", "Storage folder path is not specified.");
                });
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
                        Dispatcher.Invoke(() =>
                        {
                            // AddLog("", $"JSON array saved to {Path.Combine(storagePaths.CarsFolderPath, $"{storagePaths.CarFileName}.txt")}", "");
                        });

                        
                    }
                    else
                    {
                        Dispatcher.Invoke(() =>
                        {
                           //  AddLog("", "", "JSON information is not an array.");
                        });
                        
                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                       //  AddLog("", "", $"Error saving JSON to file: {ex.Message}");
                    });
                    
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
            var httpClientHandler = new HttpClientHandlerAdapter();

            hubConnection = new HubConnectionBuilder()
                .WithUrl("http://20.121.40.254:5101/SocketHub", options =>
                {
                    options.HttpMessageHandlerFactory = (message) => httpClientHandler;
                })
                .Build();

            AddLog("Conectando", "", "");

            hubConnection.StartAsync().ContinueWith(task => {
                if (task.IsFaulted)
                {
                    Dispatcher.Invoke(() =>
                    {
                        AddLog("", "", $"Hub Connection Error: {task.Exception.GetBaseException()}");
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        AddLog("", "Hub Connection Established", "");
                    });
                }
            });

            hubConnection.On<JsonElement>("SendPosition", (jsonKeys) =>
            {
                int id = jsonKeys.GetProperty("id").GetInt32();
                int carIndex = jsonKeys.GetProperty("carIndex").GetInt32();
                string name = jsonKeys.GetProperty("name").GetString();

                Dispatcher.Invoke(() =>
                {
                    AddLog($"Received position update for car {carIndex}. ID: {id}, Name: {name}", "", "");
                });
            });

            hubConnection.On<JsonElement>("PilotsPoints", (jsonKeys) =>
            {
                SaveJsonToFile(jsonKeys);
            });

            hubConnection.On<JsonElement>("Participants", (participant) =>
            {
                SaveParticipant(participant);
            });

            hubConnection.On<JsonElement>("FinalClassification", (finalClassification) =>
            {
                SaveFinalClassification(finalClassification);
            });
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

            AddLog("Intentando reconectando", "", "");

            // Inicia la reconexión
            InitializeSignalRConnection();
        }

    }
}
