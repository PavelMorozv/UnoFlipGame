using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System;
using ServerLib.ServerModules;

namespace ServerLib.ServerModules
{
    public class ModuleManager
    {
        private static List<IModule> modules = new List<IModule>();

        /// <summary>
        /// Загружает все модули в данной дирректории
        /// </summary>
        /// <param name="directory"></param>
        public void LoadModules(string directory)
        {
            // Получаем список всех сборок в директории
            string[] files = Directory.GetFiles(directory, "*.dll");
            foreach (string file in files)
            {
                Assembly assembly = Assembly.LoadFrom(file);
                foreach (Type type in assembly.GetExportedTypes())
                {
                    if (typeof(IModule).IsAssignableFrom(type))
                    {
                        IModule module = (IModule)Activator.CreateInstance(type);
                        modules.Add(module);
                    }
                }
            }
        }

        public void AddModule(IModule module)
        {
            modules.Add(module);
        }

        /// <summary>
        /// Производит обновление модулей
        /// </summary>
        public void UpdateModules()
        {
            foreach (IModule module in modules)
            {
                module.Update();
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Производит остановку и выгрузку модулей
        /// </summary>
        public void ShutdownModules()
        {
            foreach (IModule module in modules)
            {
                module.Shutdown();
            }

            modules.Clear();
        }

        public static T GetModule<T>() where T : class, IModule
        {
            return modules.Find(m => m.GetType() == typeof(T)) as T;
        }

        public void InitializeModules()
        {
            foreach (IModule module in modules)
            {
                module.Initialize();
            }
        }
    }
}
