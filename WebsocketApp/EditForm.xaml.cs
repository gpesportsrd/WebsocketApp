using System;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;
using System.Windows.Input;

namespace WebsocketApp
{
    public partial class EditForm : Window
    {
        public event EventHandler EditCompleted;
        public EditForm()
        {
            InitializeComponent();
            LoadAndDisplayCurrentUserData();
        }

        private string selectedPathKey = ""; // Variable para almacenar la ruta seleccionada
        private string selectedNameKey = "";
        private string selectedPathCars = ""; // Variable para almacenar la ruta seleccionada
        private string selectedNameCars = "";
        private string selectedPathDrivers = ""; // Variable para almacenar la ruta seleccionada
        private string selectedNameDrivers = "";

        private void LoadAndDisplayCurrentUserData()
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string credentialsFilePath = Path.Combine(appDataFolder, "WebsocketApp", "credentials.xml");

            if (File.Exists(credentialsFilePath))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(credentialsFilePath);

                XmlNode nicknameNode = xmlDoc.SelectSingleNode("Credenciales/Nickname");
                XmlNode idNode = xmlDoc.SelectSingleNode("Credenciales/Id");

                XmlNode keyPressNameNode = xmlDoc.SelectSingleNode("Credenciales/NameKey");
                XmlNode keyPressPathNode = xmlDoc.SelectSingleNode("Credenciales/PathKey");

                XmlNode drivesNameNode = xmlDoc.SelectSingleNode("Credenciales/NameDrivers");
                XmlNode drivesPathNode = xmlDoc.SelectSingleNode("Credenciales/PathDrivers");

                XmlNode carsNameNode = xmlDoc.SelectSingleNode("Credenciales/NameCars");
                XmlNode carsPathNode = xmlDoc.SelectSingleNode("Credenciales/PathCars");

                if (nicknameNode != null && idNode != null && keyPressNameNode != null && keyPressPathNode != null && drivesNameNode != null && drivesPathNode != null && carsNameNode != null && carsPathNode != null)
                {
                    string nickname = nicknameNode.InnerText;
                    string id = idNode.InnerText;

                    string keyPressName = keyPressNameNode.InnerText;
                    string keyPressPath = keyPressPathNode.InnerText;

                    string drivesName = drivesNameNode.InnerText;
                    string drivesPath = drivesPathNode.InnerText;

                    string carsName = carsNameNode.InnerText;
                    string carsPath = carsPathNode.InnerText;

                    // Mostrar los datos actuales en el TextBlock
                    tbCurrentNickName.Text = $"Nickname: {nickname}\nID:{id}";
                    tbCurrentKeyPress.Text = $"Nombre: {keyPressName}\nRuta:{keyPressPath}";
                    tbCurrentPathAndNameCars.Text = $"Nombre: {carsName}\nRuta:{carsPath}";
                    tbCurrentPathAndNameDrivers.Text = $"Nombre: {drivesName} \nRuta:{drivesPath}";
                }
            }
        }

        private void btnMessage_Click(object sender, RoutedEventArgs e)
        {
            // Verificar que se haya ingresado un nickname
            if (string.IsNullOrWhiteSpace(txtNickname.Text) || txtNickname.Text == "Agrega el Nickname")
            {
                MessageBox.Show("Por favor, ingrese un Nickname válido.", "Nickname no válido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Salir del método sin continuar
            }

            // Verificar que se hayan seleccionado las tres rutas
            if (string.IsNullOrWhiteSpace(selectedPathKey) || string.IsNullOrWhiteSpace(selectedPathCars) || string.IsNullOrWhiteSpace(selectedPathDrivers))
            {
                MessageBox.Show("Por favor, seleccione todas las rutas necesarias.", "Rutas faltantes", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Salir del método sin continuar
            }

            // Si todas las verificaciones pasaron, puedes continuar con la lógica existente

            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string credentialsFilePath = Path.Combine(appDataFolder, "WebsocketApp", "credentials.xml");

            XmlDocument xmlDoc = new XmlDocument();
            if (File.Exists(credentialsFilePath))
            {
                xmlDoc.Load(credentialsFilePath);
            }
            else
            {
                xmlDoc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?><Credenciales></Credenciales>");
            }

            XmlNode credencialesNode = xmlDoc.SelectSingleNode("Credenciales");

            // Eliminar nodos existentes de Nickname e Id
            credencialesNode.RemoveAll();

            XmlElement nicknameElement = xmlDoc.CreateElement("Nickname");
            nicknameElement.InnerText = txtNickname.Text;

            // Generar el Id a partir del Nickname y la fecha
            string currentDateTime = DateTime.Now.ToString("yyyyMMddHHmmss"); // Formato de fecha y hora
            string combinedData = txtNickname.Text + currentDateTime;
            string generatedId = GenerateId(combinedData);

            XmlElement idElement = xmlDoc.CreateElement("Id");
            idElement.InnerText = generatedId;

            XmlElement rutaKeyElement = xmlDoc.CreateElement("PathKey");
            rutaKeyElement.InnerText = selectedPathKey;
            XmlElement nameKeyElement = xmlDoc.CreateElement("NameKey");
            nameKeyElement.InnerText = selectedNameKey;

            XmlElement rutaCarsElement = xmlDoc.CreateElement("PathCars");
            rutaCarsElement.InnerText = selectedPathCars;
            XmlElement nameCarsElement = xmlDoc.CreateElement("NameCars");
            nameCarsElement.InnerText = selectedNameCars;

            XmlElement rutaDriversElement = xmlDoc.CreateElement("PathDrivers");
            rutaDriversElement.InnerText = selectedPathDrivers;
            XmlElement nameDriversElement = xmlDoc.CreateElement("NameDrivers");
            nameDriversElement.InnerText = selectedNameDrivers;

            credencialesNode.AppendChild(nicknameElement);
            credencialesNode.AppendChild(idElement);
            credencialesNode.AppendChild(rutaKeyElement);
            credencialesNode.AppendChild(nameKeyElement);
            credencialesNode.AppendChild(rutaCarsElement);
            credencialesNode.AppendChild(nameCarsElement);
            credencialesNode.AppendChild(rutaDriversElement);
            credencialesNode.AppendChild(nameDriversElement);

            xmlDoc.Save(credentialsFilePath);

            // Cambiar al contenido deseado, por ejemplo, una pantalla en blanco
            EditCompleted?.Invoke(this, EventArgs.Empty);
            this.Close();
        }

        private string GenerateId(string data)
        {
            // Generar un hash MD5 del dato combinado (Nickname + Fecha)
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(data);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }

                return sb.ToString();
            }
        }

        private void txtNickname_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtNickname.Text = txtNickname.Text.Trim();
        }

        private void txtNickname_GotFocus(object sender, RoutedEventArgs e)
        {
            // Cuando el cuadro de texto obtiene el foco (hace clic), borra el texto predeterminado si es "Agrega el Nickname".
            if (txtNickname.Text == "Editar el Nickname")
            {
                txtNickname.Text = "";
            }
        }

        private void txtNickname_LostFocus(object sender, RoutedEventArgs e)
        {
            // Cuando el cuadro de texto pierde el foco (el usuario hace clic fuera de él), restaura el texto predeterminado si el cuadro de texto está vacío.
            if (string.IsNullOrWhiteSpace(txtNickname.Text))
            {
                txtNickname.Text = "Editar el Nickname";
            }
        }

        private string PromptForRouteName(string defaultRouteName)
        {
            // Muestra un cuadro de diálogo de entrada para que el usuario ingrese el nombre de la ruta.
            string routeName = Microsoft.VisualBasic.Interaction.InputBox(
                "Ingrese un nombre para la ruta:", "Nombre de la Ruta", defaultRouteName);

            // Verifica si el usuario hizo clic en "Cancelar" o dejó el campo vacío.
            if (string.IsNullOrEmpty(routeName))
            {
                // Puedes manejar esto como desees. En este ejemplo, simplemente establecemos un nombre predeterminado.
                routeName = defaultRouteName;
            }

            return routeName;
        }


        private void SaveRouteToCredentials(string routeName, string selectedRoute)
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string credentialsFilePath = Path.Combine(appDataFolder, "WebsocketApp", "credentials.xml");

            XmlDocument xmlDoc = new XmlDocument();
            if (File.Exists(credentialsFilePath))
            {
                xmlDoc.Load(credentialsFilePath);
            }
            else
            {
                xmlDoc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?><Credenciales></Credenciales>");
            }

            XmlNode credencialesNode = xmlDoc.SelectSingleNode("Credenciales");

            // Crear un elemento XML para la ruta y asignarle un atributo "Nombre"
            XmlElement rutaElement = xmlDoc.CreateElement("Ruta");
            rutaElement.SetAttribute("Nombre", routeName);
            rutaElement.InnerText = selectedRoute;

            // Buscar y eliminar un nodo con el mismo nombre de ruta si existe
            XmlNode existingRouteNode = credencialesNode.SelectSingleNode($"Ruta[@Nombre='{routeName}']");
            if (existingRouteNode != null)
            {
                credencialesNode.RemoveChild(existingRouteNode);
            }

            // Agregar el nuevo elemento de ruta
            credencialesNode.AppendChild(rutaElement);

            xmlDoc.Save(credentialsFilePath);
        }

        public class RouteInfo
        {
            public string Path { get; set; }
            public string Name { get; set; }
        }

        private RouteInfo SelectAndSaveRoute(string defaultRouteName)
        {
            // Solicitar al usuario que ingrese un nombre para la ruta
            string routeName = PromptForRouteName(defaultRouteName);

            // Mostrar un cuadro de diálogo para seleccionar la ruta de almacenamiento
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = $"Seleccionar Ruta {routeName}";
            openFileDialog.CheckFileExists = false;
            openFileDialog.CheckPathExists = true;
            openFileDialog.FileName = routeName;
            openFileDialog.Filter = "Directorios|*.none";

            // Mostrar el cuadro de diálogo
            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                // Guardar la ruta seleccionada en una variable o hacer lo que necesites con ella
                string selectedRoute = System.IO.Path.GetDirectoryName(openFileDialog.FileName);

                // Aquí puedes guardar la ruta en el archivo de credenciales y crear el archivo .txt vacío
                SaveRouteToCredentials(routeName, selectedRoute);

                // Crear un archivo .txt vacío con el nombre de la ruta
                string emptyTextFilePath = Path.Combine(selectedRoute, $"{routeName}.txt");
                File.WriteAllText(emptyTextFilePath, "");

                // Mostrar un mensaje informativo
                MessageBox.Show($"La ruta de almacenamiento seleccionada para {routeName} es: {selectedRoute}", "Ruta de Almacenamiento", MessageBoxButton.OK, MessageBoxImage.Information);

                // Devolver la información de ruta y nombre como un objeto RouteInfo
                return new RouteInfo { Path = selectedRoute, Name = routeName };
            }

            // Si el usuario cancela la selección, devuelve un objeto RouteInfo vacío
            return new RouteInfo { Path = "", Name = "" };
        }

        private void btnSelectRouteKey_Click(object sender, RoutedEventArgs e)
        {
            RouteInfo routeInfo = SelectAndSaveRoute("Key Press");
            if (!string.IsNullOrEmpty(routeInfo.Path))
            {
                selectedPathKey = routeInfo.Path;
                selectedNameKey = routeInfo.Name;
            }
            // Haz lo que necesites con la ruta y el nombre seleccionados
        }

        private void btnSelectRouteCars_Click(object sender, RoutedEventArgs e)
        {
            RouteInfo routeInfo = SelectAndSaveRoute("Cars");
            if (!string.IsNullOrEmpty(routeInfo.Path))
            {
                selectedPathCars = routeInfo.Path;
                selectedNameCars = routeInfo.Name;
            }
            // Haz lo que necesites con la ruta seleccionada
        }

        private void btnSelectRouteDrivers_Click(object sender, RoutedEventArgs e)
        {
            RouteInfo routeInfo = SelectAndSaveRoute("Drivers");
            if (!string.IsNullOrEmpty(routeInfo.Path))
            {
                selectedPathDrivers = routeInfo.Path;
                selectedNameDrivers = routeInfo.Name;
            }
            // Haz lo que necesites con la ruta seleccionada
        }
    }
}
