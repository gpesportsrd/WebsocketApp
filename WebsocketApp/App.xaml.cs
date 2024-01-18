using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WebsocketApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Determine la ubicación del archivo
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataFolder, "WebsocketApp");

            // Crear la carpeta si no existe
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            // Ruta completa del archivo
            string credentialsFilePath = Path.Combine(appFolder, "credentials.xml");

            // Verificar si el archivo no existe y crearlo si es necesario
            if (!File.Exists(credentialsFilePath))
            {
                // Crear el archivo con una estructura XML básica
                string xmlContent = "<?xml version=\"1.0\" encoding=\"utf-8\"?><Credenciales></Credenciales>";
                File.WriteAllText(credentialsFilePath, xmlContent);
            }

            // Continuar con el inicio normal de la aplicación
        }
    }
}
