using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Xml;

namespace WebsocketApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ShowOrHideForm();
        }

        private void ShowOrHideForm()
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string credentialsFilePath = Path.Combine(appDataFolder, "WebsocketApp", "credentials.xml");

            if (File.Exists(credentialsFilePath))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(credentialsFilePath);

                XmlNode nicknameNode = xmlDoc.SelectSingleNode("Credenciales/Nickname");
                XmlNode idNode = xmlDoc.SelectSingleNode("Credenciales/Id");

                if (nicknameNode != null && idNode != null)
                {
                    // Los nodos de Nickname e Id existen, mostrar los datos del usuario y la ventana de logs.
                    ShowUserDataAndLogs();
                    return;
                }
            }

            ShowInputForm();
        }

        private void ShowUserDataAndLogs()
        {
            // Lógica para mostrar los datos del usuario y la ventana de logs.
            // Crear una instancia de la nueva ventana (BlankScreen)
            BlankScreen blankScreenWindow = new BlankScreen();

            // Mostrar la ventana y esperar hasta que se cierre
            bool? result = blankScreenWindow.ShowDialog();

            // Verificar si el usuario cerró la ventana
            if (result == true)
            {
                // El usuario cerró la ventana y puedes realizar acciones adicionales si es necesario.
                Application.Current.Shutdown();
            }
            else
            {
                // El usuario cerró la ventana o hizo clic en Cancelar (result == false)
                // Puedes realizar acciones adicionales si es necesario.
                Application.Current.Shutdown();
            }
        }


        private void ShowInputForm()
        {
            InputForm inputForm = new InputForm();
            if (inputForm.ShowDialog() == true) // Abre el formulario y espera su resultado
            {
                // Mostrar los datos del usuario y la ventana de logs después de guardar
                ShowUserDataAndLogs();
            }
            else
            {
                // Si el usuario cancela el formulario, cierra la aplicación
                Application.Current.Shutdown();
            }
        }
    }
}
