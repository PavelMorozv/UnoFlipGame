using ServerLib.ServerModules;

namespace ServerLib
{
    public class Server
    {
        private Thread serverThread;
        private bool isRunning;
        private ModuleManager Modules;

        static string dirModules = "Modules";

        public Server()
        {
            Modules = new ModuleManager();

            Modules.AddModule(new DBModule());
            Modules.AddModule(new AuthorizationModule());
            Modules.AddModule(new RoomsModule("RoomsModule"));
            Modules.AddModule(new GamesModule("GamesModule"));
            Modules.AddModule(new NetworkModule());

            if (!Directory.Exists("dirModules"))
                Directory.CreateDirectory(dirModules);

            // Загружаем модули из папки .\Modules
            Modules.LoadModules("Modules");

            Modules.InitializeModules();
        }

        /// <summary>
        /// Запускает сервер в отдельном потоке
        /// </summary>
        public void Start()
        {
            if (isRunning) return;

            serverThread = new Thread(() =>
            {

                while (isRunning)
                {
                    // Обновляем модули
                    Modules.UpdateModules();

                    // Дополнительный код для сервера

                    Thread.Sleep(5000);
                }

                // Выгружаем модули
                Modules.ShutdownModules();
            });

            isRunning = true;
            serverThread.Start();
        }

        /// <summary>
        /// Производит остановку работы сервера
        /// </summary>
        public void Stop()
        {
            if (!isRunning) return;

            isRunning = false;
            serverThread.Join();
        }
    }
}
