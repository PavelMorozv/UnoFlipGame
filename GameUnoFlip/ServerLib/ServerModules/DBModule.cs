using ServerLib.GameContent;

namespace ServerLib.ServerModules
{
    public class DBModule : IModule
    {
        public AppDBContext AppDBContext { get; private set; }
        public string Name;

        public void Initialize()
        {
            Name = ToString().Split(".").LastOrDefault();
            Console.WriteLine($"[{Name}] Инициализация...");
            AppDBContext = new AppDBContext();
            Console.WriteLine($"[{Name}] Инициализация завершена");
        }

        public void Shutdown()
        {
            AppDBContext.Dispose();
        }
        public void Update()
        {
            
        }
    }
}
